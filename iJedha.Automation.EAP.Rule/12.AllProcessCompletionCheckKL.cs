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
/// <summary>
/// 触发功能：
/// </summary>
namespace iJedha.Automation.EAP.Rule
{
    public partial class AllProcessCompletionCheckKL
    {
        /// <summary>
        /// 检查清线是否完成
        /// </summary>
        /// <param name="_DowryObj"></param>
        public void Invoke(object _DowryObj)
        {
            try
            {
                if (!Environment.EAPEnvironment.commonLibrary.commonModel.InAllProcessCompletionCheckStart)
                {
                    return;
                }
                //获取线程参数
                Dictionary<MessageModel.LotInfoKL, CommModel> Dic_Message = (Dictionary<MessageModel.LotInfoKL, CommModel>)_DowryObj;
                #region [定义物件，并赋值]
                MessageModel.LotInfoKL lotInfo = new MessageModel.LotInfoKL();
                PortModel port = new PortModel();
                //LotModel lm = new LotModel();
                EquipmentModel em = new EquipmentModel();
                Lot subLot = new Lot();
                foreach (var item in Dic_Message)
                {
                    lotInfo = item.Key;
                    port =item.Value.pm ;
                    //lm = item.Value.lm;
                    em = item.Value.em;
                    subLot = item.Value.lot;
                }
                #endregion
                #region [清线完成逻辑]
                if (EAPEnvironment.commonLibrary.isAllProcessOK)
                {
                    #region 杀掉当前线程
                    Environment.EAPEnvironment.commonLibrary.commonModel.InAllProcessCompletionCheckInitial();
                    EAPEnvironment.EAPAp.DeleteThreadWork(ConstLibrary.CONST_DLL_RULE_AllProcessCompletionCheckKL);
                    #endregion
                    
                    #region 清线完成，下载生产任务
                    foreach (var _v in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
                    {
                        //获取设备参数
                        List<MessageModel.Param> parameterModel = _v.NewEQParameter= Environment.EAPEnvironment.commonLibrary.commonModel.GetParameterModel_KL(_v, subLot); ;
                        //下载生产任务
                        new HostService.HostService().JobDataDownload(_v, subLot, "", parameterModel);
                    }
                    
                    if (!EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckStart)
                    {
                        //开启开料线生产任务下载处理线程
                        if (EAPEnvironment.EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_JobDataDownloadKL, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_JobDataDownloadKL, 4000, true, subLot))
                        {
                            EAPEnvironment.commonLibrary.isProcessOK = true;
                            BaseComm.LogMsg(Log.LogLevel.Info, string.Format("LotID<{0}>确认设备数据中......", subLot.LotID));
                            EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckStart = true;
                            EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckTime = DateTime.Now;
                        }
                    }
                    #endregion
                    EAPEnvironment.commonLibrary.isJobDataProcessDataTigger = false;//检查完生产条件后，完工信号Flag复位
                }
                #endregion
                else
                {
                    if (!EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckStart)
                    {
                        //清线未完成，再次开启开料线生产条件检查线程
                        if (EAPEnvironment.EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_ProductionConditionCheckKL, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_ProductionConditionCheckKL, 4000, true, Dic_Message))
                        {
                            //isNormalCheckData=false，说明不是首次进入开料线生产条件检查线程
                            EAPEnvironment.commonLibrary.isNormalCheckData = false;
                            BaseComm.LogMsg(Log.LogLevel.Info, string.Format("线体<{0}>正在检查生产条件......", EAPEnvironment.commonLibrary.LineName));
                            EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckStart = true;
                            EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckTime = DateTime.Now;
                        }
                    }
                   
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
