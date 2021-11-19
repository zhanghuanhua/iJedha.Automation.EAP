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
    public partial class EquipmentJobDataRequest
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
                //获取线程带进来的参数
                Dictionary<MessageModel.LotInfo, CommModel> Dic_Message = (Dictionary<MessageModel.LotInfo, CommModel>)_DowryObj;
                #region [定义物件信息，并取得数据]
                MessageModel.LotInfo lotInfo = new MessageModel.LotInfo();
                PortModel port = new PortModel();
                LotModel lm = new LotModel();
                EquipmentModel em = new EquipmentModel();
                foreach (var item in Dic_Message)
                {
                    lotInfo = item.Key;
                    port =item.Value.pm ;
                    lm = item.Value.lm;
                    em = item.Value.em;
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
                            //如果配方没有变更，跳过
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
                    else
                    {
                        //清线完成，清线完成Flag=true（isProcessOK = true）
                        EAPEnvironment.commonLibrary.isProcessOK = true;
                        //初始化配方变更Flag
                        foreach (var item in EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
                        {
                            item.isRecipeChange = false;
                        }
                    }
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
                    var LoadEquipment = EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.Type == eEquipmentType.L).ToList();
                    #region 杀掉当前线程
                    Environment.EAPEnvironment.commonLibrary.commonModel.InEquipmentJobDataRequestCheckInitial();
                    EAPEnvironment.EAPAp.DeleteThreadWork(ConstLibrary.CONST_DLL_RULE_EquipmentJobDataRequest);
                    #endregion
                    //初始化条件检查线程开始Flag
                    Environment.EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckInitial();
                    //杀掉条件检查线程
                    EAPEnvironment.EAPAp.DeleteThreadWork(ConstLibrary.CONST_DLL_RULE_ProductionConditionCheck);

                    #region 清线完成，下载生产任务   【目前给所有设备下载生产任务】
                    foreach (var _v in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
                    {
                        #region[如果是isMaterialLoadComplete设定请求物料Lot信息，不下载生产任务给设备]
                        if (_v.isMaterialLoadComplete)
                        {
                            continue;
                        }
                        #endregion

                        #region [如果设备是扫描底盘下载生产任务]
                        if (_v.isCarrierDownloadData)
                        {
                            continue;
                        }
                        #endregion

                        #region[如果设备是投板机，投板机数量大于1，但是和当前请求的设备ID不一致，不下载生产任务。避免投板机生产任务重复下发]
                        if (LoadEquipment.Count > 1)
                        {
                            if (em.EQID != _v.EQID)
                            {
                                if (_v.Type == eEquipmentType.L)
                                {
                                    _v.isMultiLoad = true;//不同设备ID，是投板机设备时，多投板机Flag为True
                                    continue;
                                }
                                else
                                    _v.isMultiLoad = false;//不同设备ID，不是投板机设备时，多投板机Flag为False
                            }
                            else
                               _v.isMultiLoad = false;//相同设备时，多投板机Flag为False
                        }
                        #endregion

                        #region[如果非连线，设备没做设定，不下载生产任务]
                        if (lm.IsConnectionStatus==eConnectionStatus.Disconnection.GetEnumDescription())
                        {
                            if (!_v.isDisconnectionDataDownload)
                            {
                                continue;
                            }
                        }
                        #endregion
                        List<MessageModel.Param> parameterModel = _v.NewEQParameter;//= Environment.EAPEnvironment.commonLibrary.commonModel.GetParameterModel(_v, lm); ;

                        new HostService.HostService().JobDataDownload(_v, lm, lm.LocalPortStation, parameterModel);
                    }
                    //定义一个线程参数，并赋值
                    Dictionary<EquipmentModel, LotModel> el = new Dictionary<EquipmentModel, LotModel>();
                    el.Add(em, lm);

                    if (!EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckStart)
                    {
                        //开启任务下载线程
                        if (EAPEnvironment.EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_JobDataDownload, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_JobDataDownload, 4000, true, el))
                        {
                            EAPEnvironment.commonLibrary.isProcessOK = true;
                            BaseComm.LogMsg(Log.LogLevel.Info, string.Format("Lot<{0}>确认设备数据中......", lotInfo.LotID));
                            EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckStart = true;
                            EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckTime = DateTime.Now;
                        }
                    }
                    #endregion
                }
                #region [如果清线未完成，重新开启条件检查线程]
                else
                {
                    if (!EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckStart)
                    {
                        //开启条件检查线程
                        if (EAPEnvironment.EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_ProductionConditionCheck, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_ProductionConditionCheck,4000, true, Dic_Message))
                        {
                            EAPEnvironment.commonLibrary.isNormalCheckData = false;
                            BaseComm.LogMsg(Log.LogLevel.Info, string.Format("线体<{0}>正在检查生产条件......", lotInfo.MainEqpID));
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
