//******************************************************************
//   系统名称 : iJedha.Automation.EAP.Rule
//   文件概要 : PPSelectCheck
//   作    者 : 张明哲
//   <更新履历>
//   1.0.0.0    2020/2/6 10:40:34
//******************************************************************
using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Environment;
using iJedha.Automation.EAP.Library;
using iJedha.Automation.EAP.Model;
using System;
using System.Collections.Generic;
using System.Linq;
/// <summary>
/// 触发功能：
/// </summary>
namespace iJedha.Automation.EAP.Rule
{
    public partial class LoadCompleteReportCheck
    {
        /// <summary>
        /// 检查设备上报LoadComplete数量
        /// </summary>
        /// <param name="_DowryObj"></param>
        public void Invoke(object _DowryObj)
        {
            try
            {
                string MESDownloadInfo = string.Empty;
                string EQPReportInfo = string.Empty;
                if (!Environment.EAPEnvironment.commonLibrary.commonModel.InLoadCompleteReportCheckStart)
                {
                    return;
                }
                //获取线程参数
                Dictionary<MessageModel.LotInfo, CommModel> Dic_Message = (Dictionary<MessageModel.LotInfo, CommModel>)_DowryObj;
                #region [定义物件，并赋值]
                MessageModel.LotInfo lotInfo = new MessageModel.LotInfo();
                LotModel lot = new LotModel();
                EquipmentModel em = new EquipmentModel();
                foreach (var item in Dic_Message)
                {
                    lotInfo = item.Key;
                    lot = item.Value.lm;
                    em = item.Value.em;
                }
                #endregion
                if (1 == lotInfo.InnerLotID.Count)
                {
                    #region PP裁切机检查上报次数逻辑
                    if (EAPEnvironment.commonLibrary.lineModel.isCheckScanCodeReport)
                    {
                       
                        //获取MESDownloadInfo和EQPReportInfo值方法
                        EAPEnvironment.commonLibrary.GetPPMaterialLotInfo(em, out MESDownloadInfo, out EQPReportInfo);

                        if (!string.IsNullOrEmpty(MESDownloadInfo) && !string.IsNullOrEmpty(EQPReportInfo) && MESDownloadInfo == EQPReportInfo)
                        {
                            #region 杀掉当前线程
                            Environment.EAPEnvironment.commonLibrary.commonModel.InLoadCompleteReportCheckInitial();
                            EAPEnvironment.EAPAp.DeleteThreadWork(ConstLibrary.CONST_DLL_RULE_LoadCompleteReportCheck);

                            EAPEnvironment.commonLibrary.commonModel.LoadCompleteCount.Clear();


                            EAPEnvironment.commonLibrary.isSameWithInnerLotCount = true;

                            #endregion
                            #region 正在检查生产条件
                            if (!EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckStart)
                            {
                                if (EAPEnvironment.EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_ProductionConditionCheck, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_ProductionConditionCheck, 4000, true, Dic_Message))
                                {
                                    BaseComm.LogMsg(Log.LogLevel.Info, string.Format("线体<{0}>正在检查生产条件......", lotInfo.MainEqpID));
                                    EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckStart = true;
                                    EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckTime = DateTime.Now;
                                }
                            }
                            #endregion
                        }
                    }
                    #endregion

                    #region  冲孔连棕化逻辑
                    else
                    {
                        #region 杀掉当前线程
                        Environment.EAPEnvironment.commonLibrary.commonModel.InLoadCompleteReportCheckInitial();
                        EAPEnvironment.EAPAp.DeleteThreadWork(ConstLibrary.CONST_DLL_RULE_LoadCompleteReportCheck);
                        if (!em.isMultiPort)
                        {
                            EAPEnvironment.commonLibrary.commonModel.LoadCompleteCount.Clear();
                        }

                        EAPEnvironment.commonLibrary.isSameWithInnerLotCount = true;

                        #endregion
                        #region 正在检查生产条件
                        if (!EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckStart)
                        {
                            if (EAPEnvironment.EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_ProductionConditionCheck, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_ProductionConditionCheck, 4000, true, Dic_Message))
                            {
                                BaseComm.LogMsg(Log.LogLevel.Info, string.Format("线体<{0}>正在检查生产条件......", lotInfo.MainEqpID));
                                EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckStart = true;
                                EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckTime = DateTime.Now;
                            }
                        }

                        #endregion
                    }
                    #endregion
                }
                else
                {
                    BaseComm.LogMsg(Log.LogLevel.Warn, string.Format("线体<{0}>侧向投板机上报Load Complete信号次数<{1}>和内层Lot数量<{2}>不同。", lotInfo.MainEqpID,
                      EAPEnvironment.commonLibrary.commonModel.LoadCompleteCount.Count, lot.InnerLotList.Count), false);

                    EAPEnvironment.commonLibrary.isSameWithInnerLotCount = false;
                }

            }
            catch (Exception ex)
            {
                Environment.BaseComm.LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return;
            }
            finally
            {

            }
        }

    }
}
