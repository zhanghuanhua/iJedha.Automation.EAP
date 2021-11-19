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
    public partial class JobDataDownload
    {
        /// <summary>
        /// 任务下载检查
        /// </summary>
        /// <param name="_DowryObj"></param>
        public void Invoke(object _DowryObj)
        {
            string err = "";
            try
            {
                //当前时间到达JobDataDownloadCheckTime设定时间后，才做任务下载
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
                    //LotModel lot = (LotModel)_DowryObj;
                    //获取线程传回的数据信息
                    Dictionary<EquipmentModel, LotModel> Dic_Info = (Dictionary<EquipmentModel, LotModel>)_DowryObj;
                    LotModel lot = new LotModel();
                    EquipmentModel em = new EquipmentModel();
                    foreach (var item in Dic_Info)
                    {
                        lot = item.Value;
                        em = item.Key;
                    }
                    bool isCheckNg = false;
                    bool ReturnFlag = false;
                    string portID = string.Empty;

                    EquipmentModel skipEm = new EquipmentModel();
                    if (!string.IsNullOrEmpty(lot.SkipSubEqpID))
                    {
                        skipEm = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByEqID(lot.SkipSubEqpID);
                    }
                    if (!Environment.EAPEnvironment.commonLibrary.isGUITriggerDataDownload)
                    {
                        //任务下载逻辑处理
                        isJobDataDownloadFlow(lot, skipEm, em, out isCheckNg,out ReturnFlag);
                        //判断返回值，如果是true，说明未达到重复次数，或者程序执行失败，return
                        if (ReturnFlag)
                        {
                            return;
                        }
                      
                    }
                    else
                    {
                        #region [GUI触发Download事件]
                        isJobDataDownloadFlow(lot, skipEm, em, out isCheckNg,out ReturnFlag);
                        //判断返回值，如果是true，说明未达到重复次数，或者程序执行失败，return
                        if (ReturnFlag)
                        {
                            return;
                        }   
                       
                        #endregion
                    }
                    //如果是多Load设备，用当前Lot重新获取设备信息
                    if (EAPEnvironment.commonLibrary.lineModel.isMultiLoadEquipment)
                    {
                        em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByLotID(lot.LotID);
                    }
                    else
                    {
                        //如果设备信息时null，用当前Lot信息获取设备信息
                        if (em.EQID == null)
                        {
                            em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByLotID(lot.LotID);
                        }
                    }
                    #region [如果检查结果NG，杀掉当前线程，记录错误信息，通知设备异常原因]
                    if (isCheckNg == true)
                    {
                        string ErrorMsg = string.Format("E4001:Lot[{0}]上机配方切换异常，拒绝上机.", lot.LotID);
                        BaseComm.LogMsg(Log.LogLevel.Error, ErrorMsg);
                        new HostService.HostService().CIMMessageCommand(em.EQID, "10", ErrorMsg, DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                        //杀掉 JobDataDownload 线程
                        EAPEnvironment.EAPAp.DeleteThreadWork(ConstLibrary.CONST_DLL_RULE_JobDataDownload);
                        Environment.EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckInitial();
                        //清空任务下载Retry次数
                        foreach (var item in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
                        {
                            item.RetryCount = 0;
                        }
                        Environment.EAPEnvironment.commonLibrary.isGUITriggerDataDownload = false;//检查失败时，初始化GUI Download Flag，确保不影响正常触发
                        lot.LotProcessStatus = eLotProcessStatus.Error;
                        //更新Lot状态
                        EAP.Environment.EAPEnvironment.commonLibrary.commonModel.AddProcessLotModel((LotModel)lot.Clone());
                    }
                    #endregion
                    else
                    {
                        if (lot.FirstInspFlag)
                        {
                            #region [更新Lot信息，以备首件结果OK后，下载剩余生产任务数量给设备]
                            //获取首件Lot信息
                            LotModel updateLot = EAPEnvironment.commonLibrary.commonModel.GetLotModelByInspectLotID(lot.LotID);
                            //如果是null，说明首件Lot还未赋值，刚开始做首件
                            if (updateLot == null)
                            {
                                lot.LotQty = lot.PanelTotalQty - int.Parse(lot.FirstInspQty==""?"0": lot.FirstInspQty);
                                //添加首件Lot信息
                                EAPEnvironment.commonLibrary.commonModel.AddInspectProcessLotModel((LotModel)lot.Clone());
                                //初始化首件TrackIn Flag
                                EAPEnvironment.commonLibrary.isFirstInspectTrackIn = false;
                                //初始化首件OK Flag
                                EAPEnvironment.commonLibrary.isInspecResultOK = false;
                                BaseComm.LogMsg(Log.LogLevel.Info, string.Format("首件任务下载成功.首件批次ID<{0}>,首件批次数量<{1}>", lot.LotID, lot.FirstInspQty));
                            }
                            else
                            {
                                if (EAPEnvironment.commonLibrary.isInspecResultOK)
                                {
                                    BaseComm.LogMsg(Log.LogLevel.Info, string.Format("首件任务下载成功.首件批次ID<{0}>,首件批次数量<{1}>", lot.LotID, lot.LotQty));
                                    #region [下载开始生产命令]
                                    new HostService.HostService().RemoteControlCommand(em.EQID, lot.LocalPortStation, eRemoteCommand.Start.GetEnumDescription());
                                    #endregion

                                    #region [如果是单机，并且有首件板，做完首件Lot后清除Lot信息]
                                    if (!EAPEnvironment.commonLibrary.lineModel.isNeedPost)
                                    {
                                        //不需过账时，要把帐料清除--单机设备使用
                                        PortModel pm = em.GetPortModelByPortID(lot.LocalPortStation);
                                        if (pm != null)
                                        {
                                            pm.List_Lot.Remove(lot.LotID);
                                        }
                                        Environment.EAPEnvironment.commonLibrary.commonModel.RemoveProcessLotModel(lot);
                                    }
                                    #endregion
                                }
                                else
                                {
                                    //更新生产批次数量
                                    updateLot.LotQty = updateLot.LotQty - int.Parse(lot.FirstInspQty);
                                    Environment.EAPEnvironment.commonLibrary.commonModel.AddInspectProcessLotModel((LotModel)updateLot.Clone());

                                    if (Environment.EAPEnvironment.commonLibrary.isFirstInspectTrackIn)
                                    {
                                        #region [下载开始生产命令]
                                        new HostService.HostService().RemoteControlCommand(em.EQID, lot.LocalPortStation, eRemoteCommand.Start.GetEnumDescription());
                                        BaseComm.LogMsg(Log.LogLevel.Info, string.Format("首件任务下载成功.首件批次ID<{0}>,首件批次数量<{1}>", lot.LotID, lot.FirstInspQty));
                                        #endregion
                                    }
                                }
                            }
                            //自动线，做首件，只做一次TrackIn
                            if (!Environment.EAPEnvironment.commonLibrary.isFirstInspectTrackIn)
                            {
                                Environment.EAPEnvironment.commonLibrary.isFirstInspectTrackIn = true;
                                //如果需要过账(isNeedPost=true)，才进行过账处理
                                if (EAPEnvironment.commonLibrary.lineModel.isNeedPost)
                                {
                                    new WebAPIReport().EAP_LotTrackIn(new MessageModel.LotTrackIn()
                                    {
                                        MainEqpID = Environment.EAPEnvironment.commonLibrary.MDLN,
                                        SubEqpID = em.EQID,
                                        PortID = lot.LocalPortStation,
                                        LotID = lot.LotID
                                    }, lot, 1,out err);
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            #region [融铆合上机前检查]
                            if (EAPEnvironment.commonLibrary.lineModel.isCheckBeforeTrackIn)
                            {
                                CommModel cm = new CommModel();
                                cm.lm = lot;
                                cm.em = em;
                                switch (lot.Direction)
                                {
                                    case "0":
                                        //检查逆向物料是否上齐
                                        if (!EAPEnvironment.commonLibrary.commonModel.InTrackInCheckNegativeStart)
                                        {
                                            if (EAPEnvironment.EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_TrackInCheck0, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_TrackInCheck0, 4000, true, cm))
                                            {
                                                BaseComm.LogMsg(Log.LogLevel.Info, string.Format("设备<{0}>正在检查物料上机情况......", em.EQName));
                                                EAPEnvironment.commonLibrary.commonModel.InTrackInCheckNegativeStart = true;
                                                EAPEnvironment.commonLibrary.commonModel.InTrackInCheckNegativeTime = DateTime.Now;
                                            }
                                        }
                                        break;
                                    case "1":
                                        //检查正向物料是否上齐
                                        if (!EAPEnvironment.commonLibrary.commonModel.InTrackInCheckPositiveStart)
                                        {
                                            if (EAPEnvironment.EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_TrackInCheck1, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_TrackInCheck1, 4000, true, cm))
                                            {
                                                BaseComm.LogMsg(Log.LogLevel.Info, string.Format("设备<{0}>正在检查物料上机情况......", em.EQName));
                                                EAPEnvironment.commonLibrary.commonModel.InTrackInCheckPositiveStart = true;
                                                EAPEnvironment.commonLibrary.commonModel.InTrackInCheckPositiveTime = DateTime.Now;
                                            }
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                            #endregion
                            else
                            {
                                //如果设定不需要过账（MES手动过账），不需要进行在制品上机请求
                                if (EAPEnvironment.commonLibrary.lineModel.isNeedPost)
                                {
                                    MessageModel.LotTrackIn _data = new MessageModel.LotTrackIn();
                                    _data.MainEqpID = Environment.EAPEnvironment.commonLibrary.MDLN;
                                    _data.SubEqpID = em.EQID;
                                    _data.PortID = lot.LocalPortStation;
                                    _data.LotID = lot.LotID;
                                    new WebAPIReport().EAP_LotTrackIn(_data, lot, 1,out err);
                                }
                                else
                                {
                                    EAPEnvironment.commonLibrary.ShowLotInfoMessage = eLotinfo.PartUp.ToString();
                                    //不需过账时，要把帐料清除--单机设备使用
                                    #region [如果是单机，做完Lot后清除Lot信息]
                                    PortModel pm = em.GetPortModelByPortID(lot.LocalPortStation);
                                    if (pm != null)
                                    {
                                        pm.List_Lot.Remove(lot.LotID);
                                    }
                                    Environment.EAPEnvironment.commonLibrary.commonModel.RemoveProcessLotModel(lot);
                                    #endregion
                                }
                            }
                        }
                        Environment.EAPEnvironment.commonLibrary.isGUITriggerDataDownload = false;//检查成功时，初始化GUI Download Flag，确保不影响正常触发
                        //清空任务下载Retry次数
                        foreach (var item in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
                        {
                            item.RetryCount = 0;
                        }
                        EAPEnvironment.commonLibrary.isAllProcessOK = false;//清线Flag初始化
                        //杀掉当前线程
                        EAPEnvironment.EAPAp.DeleteThreadWork(ConstLibrary.CONST_DLL_RULE_JobDataDownload);
                        //初始化线程启动Flag
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

        public void isJobDataDownloadFlow(LotModel lot, EquipmentModel skipEm, EquipmentModel em, out bool isCheckNg,out bool ReturnFlag)
        {
            try
            {
                ReturnFlag = false;
                isCheckNg = false;
                #region[前处理塞孔喷涂，非连线工序逻辑]
                if (lot.IsConnectionStatus == eConnectionStatus.Disconnection.GetEnumDescription())
                {
                    #region [确认设备Ready]
                    #region[检查是否有回复NG的设备，如果有，直接跳出重复询问]
                    foreach (var v in EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.isDisconnectionDataDownload))
                    {
                        if (v.JobDataDownloadChangeResult == eCheckResult.ng)
                        {
                            lot.ErrorMsg = string.Format("Lot<{0}>上机配方切换异常：设备<{1}>配方切换失败，拒绝上机.", lot.LotID, v.EQName);
                            ErrorCodeModelBase errm = new ErrorCodeModelBase();
                            Environment.BaseComm.ErrorHandleRule("E2112", lot.ErrorMsg, ref errm);
                            isCheckNg = true;
                        }
                    }
                    #endregion
                    if (isCheckNg == false)
                    {
                        //获取非连线工艺未回复的设备物件
                        var waitEm = EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => (r.JobDataDownloadChangeResult == eCheckResult.other || r.JobDataDownloadChangeResult == eCheckResult.nothing) && r.isDisconnectionDataDownload).ToList();
                        List<EquipmentModel> _dataDownloadEq = new List<EquipmentModel>();
                        //非连线下载设备获取，先获取备用
                        _dataDownloadEq = EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.isDisconnectionDataDownload).ToList();
                        EAPEnvironment.commonLibrary.isTransferStatus = false;
                        if (waitEm.Count != 0)
                        {
                            foreach (var v in waitEm)
                            {
                                #region[如果非连线工艺时，不设定下载设备，不下载生产任务给设备]
                                if (v.isDisconnectionDataDownload)
                                {
                                    EAPEnvironment.commonLibrary.isTransferStatus = true;//开启传送功能
                                }
                                else
                                {
                                    continue;
                                }
                                #endregion
                                #region [切换配方超时]
                                bool isSameCount = false;
                                v.RetryCount++;
                                //重复次数达到设定次数，逻辑处理
                                if (v.RetryCount == EAPEnvironment.commonLibrary.customizedLibrary.JobDataDownloadRetryCount)
                                {
                                    lot.ErrorMsg = string.Format("Lot<{0}>上机配方切换异常：设备<{1}>重复<{2}次>切换失败，拒绝上机.", lot.LotID, v.EQName, v.RetryCount);
                                    ErrorCodeModelBase errm = new ErrorCodeModelBase();
                                    Environment.BaseComm.ErrorHandleRule("E1102", lot.ErrorMsg, ref errm);
                                    isSameCount = true;
                                }
                                //如果未达到重复次数，需再重复下载生产任务给设备
                                if (!isSameCount)
                                {
                                    List<MessageModel.Param> parameterModel = Environment.EAPEnvironment.commonLibrary.commonModel.GetParameterModel(v, lot);//v.NewEQParameter;

                                    new HostService.HostService().JobDataDownload(v, lot, lot.LocalPortStation, parameterModel);
                                }
                                else
                                    isCheckNg = true;
                                #endregion
                            }
                            #region 非连线需要下载的设备数量和正常回复的数量相比较
                            if (_dataDownloadEq.Count > 0)
                            {
                                //非连线需要下载的设备数量
                                int DisconnectionDataDownloadCount = 0;
                                //正常回复的数量
                                int EquipmentOKCount = 0;
                                //获取非连线工艺需要下载生产任务的设备数量
                                DisconnectionDataDownloadCount = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.isDisconnectionDataDownload).ToList().Count;
                                //获取非连线工艺任务回复OK的设备数量
                                EquipmentOKCount = EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.JobDataDownloadChangeResult == eCheckResult.ok && r.isDisconnectionDataDownload).ToList().Count;
                                //非连线需要下载的设备数量和正常回复的数量是否一致
                                if (DisconnectionDataDownloadCount != EquipmentOKCount)
                                {
                                    #region 如果所有状态是wait的设备都达到Retry次数，杀掉线程
                                    if (isCheckNg == false)
                                    {
                                        //获取非连线工艺任务回复不是OK的，并且重复次数小于设定次数的设备数量
                                        var errEm = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.isDisconnectionDataDownload &&
                                                                                                                                                                                                                    (r.JobDataDownloadChangeResult == eCheckResult.nothing || r.JobDataDownloadChangeResult == eCheckResult.other)
                                                                                                                                                                                                                   && r.RetryCount < EAPEnvironment.commonLibrary.customizedLibrary.JobDataDownloadRetryCount).ToList();
                                        if (errEm.Count == 0)//0说明都已经到达Retry次数
                                        {
                                            isCheckNg = true;
                                        }
                                        else
                                        {
                                            return;
                                        }
                                    }
                                    #endregion
                                }
                            }
                            #endregion
                        }
                    }
                    #endregion
                }
                #endregion
                else
                {
                    #region[一台设备是一个Main设备，机械钻机使用]
                    if (EAPEnvironment.commonLibrary.lineModel.isMainEqpID)
                    {
                        #region[检查是否有回复NG的设备，如果有，直接跳出重复询问]
                        foreach (var v in EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
                        {
                            if (v.JobDataDownloadChangeResult == eCheckResult.ng)
                            {
                                lot.ErrorMsg = string.Format("Lot<{0}>上机配方切换异常：设备<{1}>配方切换失败，拒绝上机.", lot.LotID, v.EQName);
                                ErrorCodeModelBase errm = new ErrorCodeModelBase();
                                Environment.BaseComm.ErrorHandleRule("E2112", lot.ErrorMsg, ref errm);
                                isCheckNg = true;
                            }
                        }
                        #endregion

                        if (isCheckNg == false)
                        {
                            //获取设备ID和主线ID相同的设备物件
                            var waitEm = EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => (r.JobDataDownloadChangeResult == eCheckResult.other || r.JobDataDownloadChangeResult == eCheckResult.nothing) && r.EQID == lot.MainEqpID).ToList();
                            if (waitEm.Count != 0)
                            {
                                foreach (var v in waitEm)
                                {
                                    #region [切换配方超时]
                                    bool isSameCount = false;
                                    v.RetryCount++;
                                    //下载重复次数达到设定次数逻辑
                                    if (v.RetryCount == EAPEnvironment.commonLibrary.customizedLibrary.JobDataDownloadRetryCount)
                                    {
                                        lot.ErrorMsg = string.Format("Lot<{0}>上机配方切换异常：设备<{1}>重复<{2}次>切换失败，拒绝上机.", lot.LotID, v.EQName, v.RetryCount);
                                        ErrorCodeModelBase errm = new ErrorCodeModelBase();
                                        Environment.BaseComm.ErrorHandleRule("E1102", lot.ErrorMsg, ref errm);
                                        isSameCount = true;
                                    }
                                    //判断Retry次数是否与设定次数相同
                                    if (!isSameCount)
                                    {
                                        List<MessageModel.Param> parameterModel = Environment.EAPEnvironment.commonLibrary.commonModel.GetParameterModel(v, lot);//v.NewEQParameter;

                                        new HostService.HostService().JobDataDownload(v, lot, lot.LocalPortStation, parameterModel);
                                    }
                                    else
                                        isCheckNg = true;
                                    #endregion
                                }

                                #region 如果所有状态是wait的设备都达到Retry次数，杀掉线程
                                if (isCheckNg == false)
                                {
                                    //获取设备ID和主线ID一样的回复不是OK的，并且重复次数小于设定次数的设备数量
                                    var errEm = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.EQID == lot.MainEqpID &&
                                                                                                                                                                                                                (r.JobDataDownloadChangeResult == eCheckResult.nothing || r.JobDataDownloadChangeResult == eCheckResult.other)
                                                                                                                                                                                                               && r.RetryCount < EAPEnvironment.commonLibrary.customizedLibrary.JobDataDownloadRetryCount).ToList();
                                    if (errEm.Count == 0)//0说明都已经到达Retry次数
                                    {
                                        isCheckNg = true;
                                    }
                                    else
                                    {
                                        //返回一个Flag，中断当前方法
                                        ReturnFlag = true;
                                        return;
                                    }
                                }
                                #endregion
                            }
                        }
                    }
                    #endregion
                    else
                    {
                        #region[多投板机设备检查回复逻辑，目前融铆合使用]
                        if (EAPEnvironment.commonLibrary.lineModel.isMultiLoadEquipment)
                        {
                            #region [确认设备Ready]
                            //用当前生产LotID获取设备物件
                            EquipmentModel DownLoadEn = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByLotID(lot.LotID);
                            if (DownLoadEn != null)
                            {
                                if (DownLoadEn.JobDataDownloadChangeResult == eCheckResult.other || DownLoadEn.JobDataDownloadChangeResult == eCheckResult.nothing)
                                {
                                    bool isSameCount = false;
                                    #region [切换配方超时]
                                    //下载重复次数达到设定次数逻辑
                                    if (DownLoadEn.RetryCount == EAPEnvironment.commonLibrary.customizedLibrary.JobDataDownloadRetryCount)
                                    {
                                        lot.ErrorMsg = string.Format("Lot<{0}>上机配方切换异常：设备<{1}>重复<{2}次>切换失败，拒绝上机.", lot.LotID, DownLoadEn.EQName, DownLoadEn.RetryCount);
                                        ErrorCodeModelBase errm = new ErrorCodeModelBase();
                                        Environment.BaseComm.ErrorHandleRule("E1102", lot.ErrorMsg, ref errm);
                                        isSameCount = true;
                                    }
                                    //判断Retry次数是否与设定次数相同
                                    if (!isSameCount)
                                    {
                                        List<MessageModel.Param> parameterModel = Environment.EAPEnvironment.commonLibrary.commonModel.GetParameterModel(DownLoadEn, lot);//DownLoadEn.NewEQParameter;

                                        new HostService.HostService().JobDataDownload(DownLoadEn, lot, lot.LocalPortStation, parameterModel);
                                        DownLoadEn.RetryCount++;
                                        //返回一个Flag，中断当前方法
                                        ReturnFlag = true;
                                        return;
                                    }
                                    else
                                        isCheckNg = true;
                                    #endregion
                                }
                                else
                                {
                                    #region [切换配方失败]
                                    if (DownLoadEn.JobDataDownloadChangeResult == eCheckResult.ng)
                                    {
                                        lot.ErrorMsg = string.Format("Lot<{0}>上机配方切换异常：设备<{1}>配方切换失败，拒绝上机.", lot.LotID, DownLoadEn.EQName);
                                        ErrorCodeModelBase errm = new ErrorCodeModelBase();
                                        Environment.BaseComm.ErrorHandleRule("E2112", lot.ErrorMsg, ref errm);
                                        isCheckNg = true;
                                    }
                                    #endregion
                                }
                            }
                            #endregion
                        }
                        #endregion
                        else
                        {
                            EAPEnvironment.commonLibrary.isTransferStatus = false;
                            #region [MES主动下载Lot逻辑]
                            if (!string.IsNullOrEmpty(EAPEnvironment.commonLibrary.lineModel.StartEquipmentID) && EAPEnvironment.commonLibrary.isMESDownloadData)
                            {
                                //用设定开始设备，获取下载开始设备物件
                                EquipmentModel startEq = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByEqID(EAPEnvironment.commonLibrary.lineModel.StartEquipmentID);
                                //MES主动下载任务处理逻辑
                                isMESDownloadConfirmEquipmentReady(lot, startEq, out isCheckNg,out ReturnFlag);
                            }
                            #endregion
                            else
                            {
                                #region [Lot请求下载逻辑]
                                isConfirmEquipmentReady(lot, skipEm, em, out isCheckNg,out ReturnFlag);
                                #endregion
                                
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Environment.BaseComm.LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                isCheckNg = true;
                ReturnFlag = true;
                return;
            }
        }
        public void isConfirmEquipmentReady(LotModel lot, EquipmentModel skipEm, EquipmentModel em, out bool isCheckNg,out bool ReturnFlag)
        {
            try
            {
                ReturnFlag = false;
                isCheckNg = false;
                #region [确认设备Ready]
                #region[检查是否有回复NG的设备，如果有，直接跳出重复询问]
                foreach (var v in EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
                {
                    if (v.JobDataDownloadChangeResult == eCheckResult.ng)
                    {
                        lot.ErrorMsg = string.Format("Lot<{0}>上机配方切换异常：设备<{1}>配方切换失败，拒绝上机.", lot.LotID, v.EQName);
                        ErrorCodeModelBase errm = new ErrorCodeModelBase();
                        Environment.BaseComm.ErrorHandleRule("E2112", lot.ErrorMsg, ref errm);
                        isCheckNg = true;
                    }
                }
                #endregion
                if (isCheckNg == false)
                {
                    //获取未回复的设备物件
                    var waitEm = EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => (r.JobDataDownloadChangeResult == eCheckResult.other || r.JobDataDownloadChangeResult == eCheckResult.nothing) && !r.isCarrierDownloadData).ToList();
                    //获取投板机设备物件
                    var LoadEquipment = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.Type == eEquipmentType.L).ToList();
                    List<EquipmentModel> skipEquipment = new List<EquipmentModel>();
                    if (skipEm != null)
                    {
                        if (skipEm.EQID != null)
                        {//需要跳过的设备，先获取备用
                            skipEquipment = EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.EQNo >= skipEm.EQNo).ToList();
                        }
                    }

                    if (waitEm.Count != 0)
                    {
                        foreach (var v in waitEm)
                        {
                            #region[如果是isMaterialLoadComplete设定请求物料Lot信息，不下载生产任务给设备]
                            if (v.isMaterialLoadComplete)
                            {
                                continue;
                            }
                            #endregion
                            #region[叠板压合下货逻辑。如果设备是投板机，投板机数量大于1，但是和当前请求的设备ID不一致，不下载生产任务。避免投板机生产任务重复下发]
                            if (LoadEquipment.Count > 1)
                            {
                                if (em.EQID != v.EQID)
                                {
                                    if (v.Type == eEquipmentType.L)
                                    {
                                        v.isMultiLoad = true;//不同设备ID，是投板机设备时，多投板机Flag为True
                                        continue;
                                    }
                                    else
                                        v.isMultiLoad = false;//不同设备ID，不是投板机设备时，多投板机Flag为False
                                }
                                else
                                    v.isMultiLoad = false;//相同设备时，多投板机Flag为False
                            }
                            #endregion

                            #region[如果SkipSubEqpID有值，不下载生产任务给后面设备]
                            if (skipEm.EQID != null)
                            {
                                EAPEnvironment.commonLibrary.isTransferStatus = true;//开启传送功能
                                if (v.EQNo >= skipEm.EQNo)
                                {
                                    v.isSkipEquipment = true;//如果设备No>跳过的设备No，跳过Flag为True
                                    continue;
                                }
                                else v.isSkipEquipment = false;
                            }
                            else v.isSkipEquipment = false;
                            #endregion

                            #region [切换配方超时]
                            bool isSameCount = false;
                            v.RetryCount++;
                            //下载重复次数达到设定次数逻辑
                            if (v.RetryCount == EAPEnvironment.commonLibrary.customizedLibrary.JobDataDownloadRetryCount)
                            {
                                lot.ErrorMsg = string.Format("Lot<{0}>上机配方切换异常：设备<{1}>重复<{2}次>切换失败，拒绝上机.", lot.LotID, v.EQName, v.RetryCount);
                                ErrorCodeModelBase errm = new ErrorCodeModelBase();
                                Environment.BaseComm.ErrorHandleRule("E1102", lot.ErrorMsg, ref errm);
                                isSameCount = true;
                            }
                            //判断Retry次数是否与设定次数相同
                            if (!isSameCount)
                            {
                                List<MessageModel.Param> parameterModel = Environment.EAPEnvironment.commonLibrary.commonModel.GetParameterModel(v, lot);//v.NewEQParameter

                                new HostService.HostService().JobDataDownload(v, lot, lot.LocalPortStation, parameterModel);
                            }
                            else
                                isCheckNg = true;
                            #endregion
                        }

                        #region 叠板压合多投板机逻辑，避免重复下载生产任务给投板机
                        if (LoadEquipment.Count > 1)
                        {
                            //是投板机设备，但是与当前设备ID不同时，多投板机Flag为True的数量
                            int LoadExcuteCount = 0;
                            //未回复任务结果的投板机数量
                            int LoadEquipmentCount = 0;
                            LoadExcuteCount = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.isMultiLoad).ToList().Count;
                            LoadEquipmentCount = EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => (r.JobDataDownloadChangeResult == eCheckResult.other || r.JobDataDownloadChangeResult == eCheckResult.nothing) && !r.isCarrierDownloadData).ToList().Count;
                            //判断与当前投板机不同的投板机设备数和等待回复的设备数是否一致【如果LoadExcuteCount = LoadEquipmentCount，说明只有非当前设备的投板机没回复，回流线已回复】
                            if (LoadExcuteCount != LoadEquipmentCount)
                            {
                                #region 如果所有状态是wait的设备都达到Retry次数，杀掉线程
                                if (isCheckNg == false)
                                {
                                    var errEm = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => !r.isMultiLoad && !r.isCarrierDownloadData &&
                                                                                                                                                                                                                (r.JobDataDownloadChangeResult == eCheckResult.nothing || r.JobDataDownloadChangeResult == eCheckResult.other)
                                                                                                                                                                                                               && r.RetryCount < EAPEnvironment.commonLibrary.customizedLibrary.JobDataDownloadRetryCount).ToList();
                                    if (errEm.Count == 0)//0说明都已经到达Retry次数
                                    {
                                        isCheckNg = true;
                                    }
                                    else
                                    {
                                        //返回一个Flag，中断当前方法
                                        ReturnFlag = true;
                                        return;
                                    }
                                }
                                #endregion
                            }
                        }
                        #endregion
                        else
                        {
                            #region 需要跳过的设备数和执行后数量相比较
                            if (skipEquipment.Count > 0)
                            {
                                int SkipExcuteCount = 0;//跳过的设备数量
                                int SkipEquipmentCount = 0;//正常没有回复的数量
                                SkipExcuteCount = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.isSkipEquipment).ToList().Count;
                                SkipEquipmentCount = EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => (r.JobDataDownloadChangeResult == eCheckResult.other || r.JobDataDownloadChangeResult == eCheckResult.nothing) && !r.isCarrierDownloadData).ToList().Count;
                                //判断跳过的设备数和等待回复的设备数是否一致
                                if (SkipExcuteCount != SkipEquipmentCount)
                                {
                                    #region 如果所有状态是wait的设备都达到Retry次数，杀掉线程
                                    if (isCheckNg == false)
                                    {
                                        var errEm = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => !r.isSkipEquipment && !r.isCarrierDownloadData &&
                                                                                                                                                                                                                    (r.JobDataDownloadChangeResult == eCheckResult.nothing || r.JobDataDownloadChangeResult == eCheckResult.other)
                                                                                                                                                                                                                   && r.RetryCount < EAPEnvironment.commonLibrary.customizedLibrary.JobDataDownloadRetryCount).ToList();
                                        if (errEm.Count == 0)//0说明都已经到达Retry次数
                                        {
                                            isCheckNg = true;
                                        }
                                        else
                                        {
                                            //返回一个Flag，中断当前方法
                                            ReturnFlag = true;
                                            return;
                                        }
                                    }
                                    #endregion
                                }
                            }
                            #endregion
                            else
                            {
                                #region 如果所有状态是wait的设备都达到Retry次数，杀掉线程
                                if (isCheckNg == false)
                                {
                                    var errEm = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => !r.isCarrierDownloadData &&
                                                                                                                                                                                                                (r.JobDataDownloadChangeResult == eCheckResult.nothing || r.JobDataDownloadChangeResult == eCheckResult.other)
                                                                                                                                                                                                               && r.RetryCount < EAPEnvironment.commonLibrary.customizedLibrary.JobDataDownloadRetryCount).ToList();
                                    if (errEm.Count == 0)//0说明都已经到达Retry次数
                                    {
                                        isCheckNg = true;
                                    }
                                    else
                                    {
                                        //返回一个Flag，中断当前方法
                                        ReturnFlag = true;
                                        return;
                                    }
                                }
                                #endregion
                            }
                        }
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                Environment.BaseComm.LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                isCheckNg = true;
                ReturnFlag = true;
                return;
            }
        }
        public void isMESDownloadConfirmEquipmentReady(LotModel lot, EquipmentModel startEq, out bool isCheckNg,out bool ReturnFlag)
        {
            try
            {
                ReturnFlag = false;
                isCheckNg = false;
                #region [确认设备Ready]
                #region[检查是否有回复NG的设备，如果有，直接跳出重复询问]
                //foreach获取比当前开始设备No大的所有设备物件，进行检查任务下载是否回复NG
                foreach (var v in EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.EQNo >= startEq.EQNo))
                {
                    if (v.JobDataDownloadChangeResult == eCheckResult.ng)
                    {
                        lot.ErrorMsg = string.Format("Lot<{0}>上机配方切换异常：设备<{1}>配方切换失败，拒绝上机.", lot.LotID, v.EQName);
                        ErrorCodeModelBase errm = new ErrorCodeModelBase();
                        Environment.BaseComm.ErrorHandleRule("E2112", lot.ErrorMsg, ref errm);
                        isCheckNg = true;
                    }
                }
                #endregion
                if (isCheckNg == false)
                {
                    //获取获取比当前开始设备No大的所有未回复的设备物件
                    var waitEm = EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => (r.JobDataDownloadChangeResult == eCheckResult.other || r.JobDataDownloadChangeResult == eCheckResult.nothing) && r.EQNo >= startEq.EQNo).ToList();
                    
                    if (waitEm.Count != 0)
                    {
                        foreach (var v in waitEm)
                        {
                            #region [切换配方超时]
                            bool isSameCount = false;
                            v.RetryCount++;
                            //下载重复次数达到设定次数逻辑
                            if (v.RetryCount == EAPEnvironment.commonLibrary.customizedLibrary.JobDataDownloadRetryCount)
                            {
                                lot.ErrorMsg = string.Format("Lot<{0}>上机配方切换异常：设备<{1}>重复<{2}次>切换失败，拒绝上机.", lot.LotID, v.EQName, v.RetryCount);
                                ErrorCodeModelBase errm = new ErrorCodeModelBase();
                                Environment.BaseComm.ErrorHandleRule("E1102", lot.ErrorMsg, ref errm);
                                isSameCount = true;
                            }
                            //判断Retry次数是否与设定次数相同
                            if (!isSameCount)
                            {
                                List<MessageModel.Param> parameterModel =Environment.EAPEnvironment.commonLibrary.commonModel.GetParameterModel(v, lot);// v.NewEQParameter;

                                new HostService.HostService().JobDataDownload(v, lot, lot.LocalPortStation, parameterModel);
                            }
                            else
                                isCheckNg = true;
                            #endregion
                        }

                        #region 如果所有状态是wait的设备都达到Retry次数，杀掉线程
                        if (isCheckNg == false)
                        {
                            //获取比下载开始设备No大的，未回复OK的，重复次数小于设定次数的所有设备物件
                            var errEm = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.EQNo >= startEq.EQNo &&
                                                                                                                                                                                                        (r.JobDataDownloadChangeResult == eCheckResult.nothing || r.JobDataDownloadChangeResult == eCheckResult.other)
                                                                                                                                                                                                       && r.RetryCount < EAPEnvironment.commonLibrary.customizedLibrary.JobDataDownloadRetryCount).ToList();
                            if (errEm.Count == 0)//0说明都已经到达Retry次数
                            {
                                isCheckNg = true;
                            }
                            else
                            {
                                //返回一个Flag，中断当前方法
                                ReturnFlag = true;
                                return;
                            }
                        }
                        #endregion
                    }
                    //初始化flag
                    EAPEnvironment.commonLibrary.isMESDownloadData = false;
                }
                #endregion
            }
            catch (Exception ex)
            {
                Environment.BaseComm.LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                isCheckNg = true;
                ReturnFlag = true;
                return;
            }
        }
        public Dictionary<string, string> GetTrackInWipData(LotModel lot)
        {
            Dictionary<string, string> List_wip = new Dictionary<string, string>();
            try
            {
                foreach (var v in lot.LotParameterList)
                {
                    if (v.ServiceName != "TrackInLot")
                    {
                        continue;
                    }
                    EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(v.SubEqpID);
                    string tracevaule = string.Empty;
                    var vv = (from t in em.List_KeyTraceDataSpec where t.ItemID == v.ItemID select t).FirstOrDefault();
                    if (vv != null)
                    {
                        tracevaule = vv.DefaultValue;
                        List_wip.Add(v.WIPDataName, tracevaule);
                    }
                }
                return List_wip;
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return List_wip;
            }
        }
    }
}
