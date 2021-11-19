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
using iJedha.Automation.EAP.ModelBase;
using iJedha.Automation.EAP.WebAPI;
using System;
using System.Collections.Generic;
using System.Linq;
/// <summary>
/// 触发功能：
/// </summary>
namespace iJedha.Automation.EAP.Rule
{
    public partial class JobDataDownloadKL
    {
        /// <summary>
        /// 任务下载检查
        /// </summary>
        /// <param name="_DowryObj"></param>
        public void Invoke(object _DowryObj)
        {
            try
            {
                if (EAPEnvironment.EAPAp.TimeComparisonSeconds(EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckTime, EAPEnvironment.commonLibrary.customizedLibrary.JobDataDownloadCheckTime))
                {
                    EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckTime = DateTime.Now;
                    if (!Environment.EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckStart)
                    {
                        return;
                    }
                    if (!Environment.EAPEnvironment.commonLibrary.isProcessOK)
                    {
                        return;
                    }
                    //获取线程带进来的参数
                    Lot lot = (Lot)_DowryObj;
                    bool isCheckNg = false;
                    string portID = string.Empty;

                    if (!Environment.EAPEnvironment.commonLibrary.isGUITriggerDataDownload)
                    {
                        foreach (var v in EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
                        {
                            #region [切换配方失败]
                            if (v.JobDataDownloadChangeResult == eCheckResult.ng)
                            {
                                lot.ErrorMsg = string.Format("Lot<{0}>上机配方切换异常：设备<{1}>配方切换失败，拒绝上机.", lot.LotID, v.EQName);
                                ErrorCodeModelBase errm = new ErrorCodeModelBase();
                                Environment.BaseComm.ErrorHandleRule("E2112", lot.ErrorMsg, ref errm);
                                isCheckNg = true;
                            }
                            #endregion
                        }
                        #region [确认设备Ready]
                        if (isCheckNg == false)
                        {
                            //获取下载生产任务未回复的设备物件
                            var waitEm = EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.JobDataDownloadChangeResult == eCheckResult.other || r.JobDataDownloadChangeResult == eCheckResult.nothing).ToList();
                            if (waitEm.Count != 0)
                            {
                                foreach (var v in waitEm)
                                {
                                    #region [切换配方超时]
                                    //达到Retry次数后记录错误信息
                                    if (v.RetryCount == EAPEnvironment.commonLibrary.customizedLibrary.JobDataDownloadRetryCount)
                                    {
                                        lot.ErrorMsg = string.Format("Lot<{0}>上机配方切换异常：设备<{1}>重复<{2}次>切换失败，拒绝上机.", lot.LotID, v.EQName, v.RetryCount);
                                        ErrorCodeModelBase errm = new ErrorCodeModelBase();
                                        Environment.BaseComm.ErrorHandleRule("E1102", lot.ErrorMsg, ref errm);
                                        v.RetryCount++;
                                        continue;
                                    }

                                    List<MessageModel.Param> parameterModel =Environment.EAPEnvironment.commonLibrary.commonModel.GetParameterModel_KL(v, lot);// v.NewEQParameter;

                                    new HostService.HostService().JobDataDownload(v, lot, "", parameterModel);
                                    v.RetryCount++;
                                }
                                #endregion

                                #region 如果所有状态是wait的设备都达到Retry次数，杀掉线程
                                //获取未回复OK并且重复下载次数到达设定次数的设备物件
                                var errEm = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => (r.JobDataDownloadChangeResult == eCheckResult.nothing || r.JobDataDownloadChangeResult == eCheckResult.other)
                                                                                                                                                                                                               && r.RetryCount <= EAPEnvironment.commonLibrary.customizedLibrary.JobDataDownloadRetryCount).ToList();
                                if (errEm.Count == 0)//0说明都已经到达Retry次数
                                {
                                    isCheckNg = true;
                                }
                                else
                                {
                                    return;
                                }
                                #endregion
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        #region [GUI触发Download事件]
                        //获取所有设备，进行遍历，检查配方切换是否失败
                        foreach (var v in EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
                        {
                            #region [切换配方失败]
                            if (v.JobDataDownloadChangeResult == eCheckResult.ng)
                            {
                                lot.ErrorMsg = string.Format("Lot<{0}>上机配方切换异常：设备<{1}>配方切换失败，拒绝上机.", lot.LotID, v.EQName);
                                ErrorCodeModelBase errm = new ErrorCodeModelBase();
                                Environment.BaseComm.ErrorHandleRule("E2112", lot.ErrorMsg, ref errm);
                                isCheckNg = true;
                            }
                            #endregion
                        }
                        #region [确认设备Ready]
                        if (isCheckNg == false)
                        {
                            //获取下载生产任务未回复的设备物件
                            var waitEm = EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.JobDataDownloadChangeResult == eCheckResult.other || r.JobDataDownloadChangeResult == eCheckResult.nothing).ToList();
                            if (waitEm.Count != 0)
                            {
                                //遍历
                                foreach (var v in waitEm)
                                {
                                    #region [切换配方超时]
                                    //达到Retry次数后记录错误信息
                                    if (v.RetryCount == EAPEnvironment.commonLibrary.customizedLibrary.JobDataDownloadRetryCount)
                                    {
                                        lot.ErrorMsg = string.Format("Lot<{0}>上机配方切换异常：设备<{1}>重复<{2}次>切换失败，拒绝上机.", lot.LotID, v.EQName, v.RetryCount);
                                        ErrorCodeModelBase errm = new ErrorCodeModelBase();
                                        Environment.BaseComm.ErrorHandleRule("E1102", lot.ErrorMsg, ref errm);
                                        v.RetryCount++;
                                        continue;
                                    }

                                    List<MessageModel.Param> parameterModel = Environment.EAPEnvironment.commonLibrary.commonModel.GetParameterModel_KL(v, lot);//v.NewEQParameter;

                                    new HostService.HostService().JobDataDownload(v, lot, "", parameterModel);
                                    v.RetryCount++;
                                }
                                #endregion

                                #region 如果所有状态是wait的设备都达到Retry次数，杀掉线程
                                //获取未回复OK并且重复下载次数到达设定次数的设备物件
                                var errEm = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => (r.JobDataDownloadChangeResult == eCheckResult.nothing || r.JobDataDownloadChangeResult == eCheckResult.other)
                                                                                                                                                                                                               && r.RetryCount <= EAPEnvironment.commonLibrary.customizedLibrary.JobDataDownloadRetryCount).ToList();
                                if (errEm.Count == 0)//0说明都已经到达Retry次数
                                {
                                    isCheckNg = true;
                                }
                                else
                                {
                                    return;
                                }
                                #endregion
                            }
                        }
                        #endregion
                        #endregion

                    }
                    //用设备是否投板机（L）来获取设备物件
                    EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByType(eEquipmentType.L);
                    if (em == null)
                    {
                        em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByLotID(lot.LotID);
                    }
                    // port = em.GetPortModelByPortID(lot.LocalPortStation);
                    if (isCheckNg == true)
                    {

                        string ErrorMsg = string.Format("E4001:Lot[{0}]上机配方切换异常：拒绝上机.", lot.LotID);
                        BaseComm.LogMsg(Log.LogLevel.Error, ErrorMsg);
                        //下发信息提示
                        new HostService.HostService().CIMMessageCommand(em.EQID, "10", ErrorMsg, DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                        //杀掉当前线程
                        EAPEnvironment.EAPAp.DeleteThreadWork(ConstLibrary.CONST_DLL_RULE_JobDataDownloadKL);
                        //初始化线程开始Flag
                        Environment.EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckInitial();
                        //初始化Retry数量
                        foreach (var item in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
                        {
                            item.RetryCount = 0;
                        }
                        //检查失败时，初始化GUI Download Flag，确保不影响正常触发
                        Environment.EAPEnvironment.commonLibrary.isGUITriggerDataDownload = false;
                        lot.LotProcessStatus = eLotProcessStatus.Error;
                        //更新Lot信息
                        EAP.Environment.EAPEnvironment.commonLibrary.commonModel.AddProcessLotModelKL((Lot)lot.Clone());
                    }
                    else
                    {
                        string err = "";
                        //在制品上机
                        new WebAPIReport().EAP_LotTrackIn(new MessageModel.LotTrackIn()
                        {
                            MainEqpID = Environment.EAPEnvironment.commonLibrary.MDLN,
                            SubEqpID = em.EQID,
                            PortID = "",
                            LotID = lot.LotID
                        }, lot, 1,out err);


                        //检查成功时，初始化GUI Download Flag，确保不影响正常触发
                        EAPEnvironment.commonLibrary.isGUITriggerDataDownload = false;

                        //杀掉当前线程
                        EAPEnvironment.EAPAp.DeleteThreadWork(ConstLibrary.CONST_DLL_RULE_JobDataDownloadKL);
                        //初始化开始线程Flag
                        Environment.EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckInitial();
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
