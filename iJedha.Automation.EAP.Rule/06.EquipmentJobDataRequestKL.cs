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
    public partial class EquipmentJobDataRequestKL
    {
        /// <summary>
        /// 检查清机是否完成
        /// </summary>
        /// <param name="_DowryObj"></param>
        public void Invoke(object _DowryObj)
        {
            try
            {
                if (!Environment.EAPEnvironment.commonLibrary.commonModel.InEquipmentJobDataRequestCheckStart)
                {
                    return;
                }
                //获取线程参数
                Dictionary<MessageModel.LotInfoKL, CommModel> Dic_Message = (Dictionary<MessageModel.LotInfoKL, CommModel>)_DowryObj;
                #region[ 定义物件，并赋值]
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
                List<EquipmentModel> lem = new List<EquipmentModel>();
                #region [确认设备清线是否完成，如果完成下载生产任务给设备]
                if (em.ProcessCompletionEQ.Count > 0)//数量大于0：需要分段清机台；反之不清线或者不需要分段清线
                {
                    foreach (var v in em.ProcessCompletionEQ)
                    {
                        //获取设定了ProcessCompletionEQ设备，并且设备未清线（isProcessCompletion == false）的设备物件
                        var emo = EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.isProcessCompletion == false && r.EQID == v).FirstOrDefault();
                        if (emo != null)
                        {
                            if (!emo.isRecipeChange)
                            {
                                continue;
                            }
                            new HostService.HostService().EquipmentJobDataRequest(emo.EQID);
                            lem.Add(emo);
                        }
                        else
                        {
                            
                        }
                    }
                    if (lem.Count != 0)//Count>0说明存在未清线完成的设备
                    {
                        EAPEnvironment.commonLibrary.isProcessOK = false;
                    }
                    //清线完成，清线完成Flag=true（isProcessOK = true）
                    else EAPEnvironment.commonLibrary.isProcessOK = true;

                }
                else
                {
                    //获取设备未清线（isProcessCompletion == false）的设备物件
                    var emo = EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.isProcessCompletion == false).ToList();
                    if (emo != null)
                    {
                        EAPEnvironment.commonLibrary.isProcessOK = false;
                    }
                    else EAPEnvironment.commonLibrary.isProcessOK = true;
                }
                #endregion

                if (EAPEnvironment.commonLibrary.isProcessOK)
                {
                    #region 杀掉当前线程
                    //初始化线程开始Flag
                    Environment.EAPEnvironment.commonLibrary.commonModel.InEquipmentJobDataRequestCheckInitial();
                    //杀掉当前线程
                    EAPEnvironment.EAPAp.DeleteThreadWork(ConstLibrary.CONST_DLL_RULE_EquipmentJobDataRequestKL);
                    #endregion
                    //初始化条件检查线程开始Flag
                    Environment.EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckInitial();
                    //杀掉条件检查线程
                    EAPEnvironment.EAPAp.DeleteThreadWork(ConstLibrary.CONST_DLL_RULE_ProductionConditionCheckKL);

                    #region 清线完成，下载生产任务   【目前给所有设备下载生产任务】
                    foreach (var _v in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
                    {

                        List<MessageModel.Param> parameterModel = _v.NewEQParameter= Environment.EAPEnvironment.commonLibrary.commonModel.GetParameterModel_KL(_v, subLot); ;

                        new HostService.HostService().JobDataDownload(_v, subLot, "", parameterModel);
                    }


                    if (!EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckStart)
                    {
                        //开启任务下载线程
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
                #region [如果清线未完成，重新开启条件检查线程]
                else
                {
                    if (!EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckStart)
                    {
                        if (EAPEnvironment.EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_ProductionConditionCheckKL, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_ProductionConditionCheckKL, 4000, true, Dic_Message))
                        {
                            EAPEnvironment.commonLibrary.isNormalCheckData = false;
                            BaseComm.LogMsg(Log.LogLevel.Info, string.Format("线体<{0}>正在检查生产条件......", EAPEnvironment.commonLibrary.LineName));
                            EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckStart = true;
                            EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckTime = DateTime.Now;
                        }
                    }
                    
                }
                #endregion
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
