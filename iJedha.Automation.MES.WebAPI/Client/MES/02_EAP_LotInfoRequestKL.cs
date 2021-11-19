using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Environment;
using iJedha.Automation.EAP.Library;
using iJedha.Automation.EAP.LibraryBase;
using iJedha.Automation.EAP.Model;
using iJedha.Automation.EAP.ModelBase;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;

namespace iJedha.Automation.EAP.WebAPI
{
    public partial class WebAPIReport : BaseComm
    {
        /// <summary>
        /// EAP收到投板机读取的载具后,询问MES在制品信息。开料线
        /// </summary>
        /// <param name="_indata"></param>
        /// <param name="em"></param>
        public void EAP_LotInfoRequest_KL(MessageModel.LotInfoRequest _indata, EquipmentModel em, int Retrytime)
        {
            ErrorCodeModelBase errm = new ErrorCodeModelBase();
            try
            {
                if (EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckStart)
                {
                    EAPEnvironment.EAPAp.DeleteThreadWork(ConstLibrary.CONST_DLL_RULE_ProductionConditionCheckKL);
                }
                if (EAPEnvironment.commonLibrary.HostConnectMode == EAP.LibraryBase.eHostConnectMode.DISCONNECT)
                {
                    return;
                }

                #region  [Web API调用]
                string _outdata;
                if (ConvertJSON(_indata, out _outdata))
                {
                    LogMsg(Log.LogLevel.Info, string.Format("WebAPI Message<{0}> Send OK, Content<{1}>", System.Reflection.MethodBase.GetCurrentMethod().Name, _outdata));
                    History.EAP_EQP_EVENTHISTORY(em, eEventName.EAP_LOTINFOREQUEST, eEventFlow.EAP2MES, $"WebAPI Message<{System.Reflection.MethodBase.GetCurrentMethod().Name}> Send OK", em.CurrentLotID, _indata.PortID);
                }
                if (!EAPEnvironment.commonLibrary.baseLib.apiClientParaLibrary.Client_Enable)
                {
                    LogMsg(Log.LogLevel.Warn, string.Format("WebAPI服务器设定关闭，停止消息发送"));
                    return;
                }
                var Client = new iJedha.Automation.EAP.Core.WebAPIClient();
                string _returndata = Client.SendMessage(EAPEnvironment.commonLibrary.baseLib.apiClientParaLibrary.RemoteUrlString, System.Reflection.MethodBase.GetCurrentMethod().Name, _outdata);

                object returnobject;
                if (new Serialize().DeSerializeJSON(_returndata, new MessageModelBase.ApiResult().GetType(), out returnobject))
                {
                    LogMsg(Log.LogLevel.Trace, string.Format("WebAPI Message<{0}> Recv OK, Content<{1}>", System.Reflection.MethodBase.GetCurrentMethod().Name, _returndata));
                }
                MessageModelBase.ApiResult returnInfo = (MessageModelBase.ApiResult)returnobject;
                #endregion
                if (returnInfo.strCode != "0000")
                {
                    string errMsg = string.Format("E3002:MES回复, 接口名称[{0}], 错误代码[{1}], 错误描述[{2}][{3}]", System.Reflection.MethodBase.GetCurrentMethod().Name, returnInfo.strCode, _indata.SubEqpID, returnInfo.strMsg);
                    EAP.Environment.BaseComm.ErrorHandleRule("E3002", errMsg, ref errm);

                    new HostService.HostService().CIMMessageCommand(em.EQID, "10", errMsg, DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                    History.EAP_EQP_EVENTHISTORY(em, eEventName.EAP_LOTINFOREQUEST, eEventFlow.MES2EAP, errMsg, em.CurrentLotID, _indata.PortID);
                    return;
                }

                string err;
                PortModel port;
                LotModel lot;
                object lotinfoobject;
                string LotID;

                try
                {
                    new EAP.Core.Serialize().DeSerializeJSON(returnInfo.Content.ToString(), new MessageModel.LotInfoKL().GetType(), out lotinfoobject);
                }
                catch (Exception e)
                {
                    StackTrace st = new StackTrace(new StackFrame(true));
                    StackFrame sf = st.GetFrame(0);
                    string errMsg = string.Format("EAP发送<{0}>异常：程序出错，代码行数<{1}>，ErrMsg<{2}>",
                    System.Reflection.MethodBase.GetCurrentMethod().Name, sf.GetFileLineNumber(),
                    string.Format("设备<{0}>Lotinfo下载失败：MES传输数据错误.", em.EQID));
                    EAP.Environment.BaseComm.ErrorHandleRule("E0001", errMsg, ref errm);
                    History.EAP_EQP_EVENTHISTORY(em, eEventName.EAP_LOTINFOREQUEST, eEventFlow.EAP2MES, errMsg, em.CurrentLotID, _indata.PortID);
                    return;
                }

                MessageModel.LotInfoKL lotinfo = (MessageModel.LotInfoKL)lotinfoobject;

                if (em.ControlMode == eControlMode.REMOTE)
                {
                    #region [Check LotInfo]
                    if (!CheckLotInfoKL(em, _indata, lotinfo, out port, out err, out LotID))
                    {
                        //0:OK 1:NG
                        BaseComm.ErrorHandleRule("E2103", string.Format("JobID<{0}>大板资料下载检查异常：{1}...，拒绝生产.",
                            lotinfo.JobID, err), ref errm);

                        new HostService.HostService().CIMMessageCommand(em.EQID, "10", "E2103:" + err, DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                        History.EAP_EQP_EVENTHISTORY(em, eEventName.EAP_LOTINFOREQUEST, eEventFlow.MES2EAP, err, em.CurrentLotID, _indata.PortID);
                        return;
                    }
                    #endregion

                    #region [handle Info]
                    if (!HandleLotInfoKL(em, port, lotinfo, out lot, out err))
                    {
                        BaseComm.ErrorHandleRule("E2103", string.Format("JobID<{0}>大板资料下载检查异常：{1}...，拒绝生产.",
                            lotinfo.JobID, err), ref errm);

                        new HostService.HostService().CIMMessageCommand(em.EQID, "10", "E2103:" + err, DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                        History.EAP_EQP_EVENTHISTORY(em, eEventName.EAP_LOTINFOREQUEST, eEventFlow.MES2EAP, err, em.CurrentLotID, _indata.PortID);
                        return;
                    }
                    #endregion
                    foreach (var item in port.List_Lot)
                    {
                        //add Process Lot
                        //EAP.Environment.EAPEnvironment.commonLibrary.commonModel.AddProcessLotModelKL((Lot)item.Value.LotList[0].Clone());
                        //var aa = item.Value;
                        item.Value.LocalPortStation = ePortID.L02.ToString();
                        foreach (var vv in item.Value.LotList)
                        {
                            //暂存口改成上料口
                            vv.PortID = ePortID.L02.ToString();
                            //add Process Lot
                            EAP.Environment.EAPEnvironment.commonLibrary.commonModel.AddProcessLotModelKL((Lot)vv.Clone());
                        }
                    }
                    //add Process Lot
                    Environment.EAPEnvironment.commonLibrary.commonModel.AddProcessLotModel((LotModel)lot.Clone());

                    #region 初始化完成清线Flag
                    foreach (var v in em.ProcessCompletionEQ)
                    {
                        var emo = EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.EQID == v).FirstOrDefault();
                        if (emo != null)
                        {
                            emo.isProcessCompletion = false;
                        }
                    }
                    EAPEnvironment.commonLibrary.isProcessOK = false;
                    #endregion

                    #region 初始化设备回复状态
                    foreach (var item in EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
                    {
                        item.JobDataDownloadChangeResult = eCheckResult.nothing;
                    }
                    #endregion

                    #region  初始化所有线程Start Flag
                    EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckInitial();
                    EAPEnvironment.EAPAp.DeleteThreadWork(ConstLibrary.CONST_DLL_RULE_ProductionConditionCheckKL);
                    EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckInitial();
                    EAPEnvironment.EAPAp.DeleteThreadWork(ConstLibrary.CONST_DLL_RULE_JobDataDownloadKL);
                    EAPEnvironment.commonLibrary.commonModel.InEquipmentJobDataRequestCheckInitial();
                    EAPEnvironment.EAPAp.DeleteThreadWork(ConstLibrary.CONST_DLL_RULE_EquipmentJobDataRequestKL);
                    EAPEnvironment.commonLibrary.commonModel.InAllProcessCompletionCheckInitial();
                    EAPEnvironment.EAPAp.DeleteThreadWork(ConstLibrary.CONST_DLL_RULE_AllProcessCompletionCheckKL);
                    #endregion

                    #region 获取开料机上料口是否有正在执行的Lot
                    var portL02 = em.GetPortModelByPortID(ePortID.L02.ToString());
                    if (portL02 == null)
                    {
                        err = string.Format("异常项目：[{0}]，异常值：[{1}]", "PortID", lotinfo.PortID);
                        return;
                    }
                    #endregion


                    #region  手动下载开关
                    if (Environment.EAPEnvironment.commonLibrary.lineModel.isManualJobDataDoanload)
                    {
                        return;
                    }
                    #endregion


                    //如果上料口有料，停止下货逻辑
                    if (portL02.List_Lot.Count > 0)
                    {
                        return;
                    }
                    else
                    {
                        #region 正在检查生产条件
                        Dictionary<MessageModel.LotInfoKL, CommModel> Dic_Message = new Dictionary<MessageModel.LotInfoKL, CommModel>();
                        CommModel cm = new CommModel();
                        //cm.lm = port.List_Lot.ElementAt(0).Value;
                        cm.pm = port;
                        cm.lot = Environment.EAPEnvironment.commonLibrary.commonModel.List_ProcessLotKL.ElementAt(0).Value;
                        cm.em = em;

                        Dic_Message.Add(lotinfo, cm);
                        if (!EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckStart)
                        {
                            if (EAPEnvironment.EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_ProductionConditionCheckKL, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_ProductionConditionCheckKL, 4000, true, Dic_Message))
                            {
                                LogMsg(Log.LogLevel.Info, string.Format("线体<{0}>正在检查生产条件......", lotinfo.MainEqpID));
                                EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckStart = true;
                                EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckTime = DateTime.Now;
                            }
                        }

                        #endregion
                        //显示Lot信息
                        EAPEnvironment.commonLibrary.MainLotID = cm.lot.LotID;
                        EAPEnvironment.commonLibrary.ShowLotInfoMessage = cm.lot.LotStatus = eLotinfo.WaitingUp.ToString();

                        //取得当前Lot ID
                        EAPEnvironment.commonLibrary.commonModel.currentProcessLotID = cm.lot.LotID;
                    }
                    //bool isCheckNg = false;
                }
                else
                {
                    LogMsg(Log.LogLevel.Error, $"Equipment ID<{em.EQID}>,连线状态<{em.ConnectMode}>,控制模式<{em.ControlMode}>.");
                }
                History.EAP_EQP_EVENTHISTORY(em, eEventName.EAP_LOTINFOREQUEST, eEventFlow.MES2EAP , "", em.CurrentLotID, _indata.PortID);
            }
            catch (Exception e)
            {

                #region [超时及异常处理]
                string errMsg;
                if (e.Message.ToString() == ConstLibrary.CONST_WEBAPI_TIMEOUT_KEYWORD)
                {
                    errMsg = string.Format("第{0}次发送<{1}>超时", Retrytime, System.Reflection.MethodBase.GetCurrentMethod().Name);
                    BaseComm.LogMsg(EAP.Core.Log.LogLevel.Warn, errMsg);
                    if (Retrytime < EAPEnvironment.commonLibrary.baseLib.apiClientParaLibrary.RetryCount)
                    {
                        Retrytime++;
                        EAP_LotInfoRequest(_indata, em, Retrytime,out errMsg);
                    }
                    else
                    {
                        errMsg = string.Format("EAP发送<{0}>失败", System.Reflection.MethodBase.GetCurrentMethod().Name);
                        BaseComm.ErrorHandleRule("E0005", errMsg, ref errm);
                    }
                }
                else
                {
                    StackTrace st = new StackTrace(new StackFrame(true));
                    StackFrame sf = st.GetFrame(0);
                    errMsg = string.Format("EAP发送<{0}>异常：程序出错，代码行数<{1}>，ErrMsg<{2}>",
                    System.Reflection.MethodBase.GetCurrentMethod().Name, sf.GetFileLineNumber(), e.Message);
                    BaseComm.ErrorHandleRule("E0001", errMsg, ref errm);
                    errMsg = errMsg = string.Format("MES API服务端连接断开.");
                    BaseComm.ErrorHandleRule("E0004", errMsg, ref errm);
                    EAPEnvironment.commonLibrary.HostConnectMode = EAP.LibraryBase.eHostConnectMode.DISCONNECT;
                }
                #endregion

                History.EAP_EQP_EVENTHISTORY(em, eEventName.EAP_LOTINFOREQUEST, eEventFlow.MES2EAP , errMsg, em.CurrentLotID, _indata.PortID);
            }
        }

        /// <summary>
        /// 检查Lot信息
        /// </summary>
        /// <param name="em"></param>
        /// <param name="_data"></param>
        /// <param name="lotInformation"></param>
        /// <param name="port"></param>
        /// <param name="err"></param>
        /// <returns></returns>
        public bool CheckLotInfoKL(EquipmentModel em, MessageModel.LotInfoRequest _data, MessageModel.LotInfoKL lotinfo, out PortModel port, out string err, out string LotID)
        {
            LotID = string.Empty;
            err = string.Empty;
            try
            {
                #region [比对Port]
                port = em.GetPortModelByPortID(_data.PortID);
                if (port == null)
                {
                    err = string.Format("异常项目：[{0}]，异常值：[{1}]", "PortID", lotinfo.PortID);
                    return false;
                }
                #endregion
                foreach (var item in lotinfo.LotIDList)
                {
                    LotID = item.LotID;
                    #region [比对LotID]
                    if (item.LotID == string.Empty)
                    {
                        err = string.Format("异常项目：[{0}]，异常值：[{1}]", "LotID", item.LotID);
                        return false;
                    }
                    #endregion

                    #region [比对Panel Count]
                    if (int.Parse(item.PanelTotalQty) == 0)
                    {
                        err = string.Format("异常项目：[{0}]，异常值：[{1}]", "PanelTotalQty", item.PanelTotalQty);
                        return false;
                    }
                    #endregion
                    string msg = string.Format("MES下载生产任务Lot[{0}]接收检查完成.", LotID);
                    LogMsg(Log.LogLevel.Info, msg);
                }




                return true;
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                err = string.Format("Lot[{0}]资料检查失败：程式出错.", LotID);
                LogMsg(Log.LogLevel.Warn, err);
                port = null;
                return false;
            }
        }

        /// <summary>
        /// 建置Lot资料
        /// </summary>
        /// <param name="em"></param>
        /// <param name="port"></param>
        /// <param name="lotInformation"></param>
        /// <param name="lot"></param>
        /// <param name="err"></param>
        /// <returns></returns>
        public bool HandleLotInfoKL(EquipmentModel em, PortModel port, MessageModel.LotInfoKL lotinfo, out LotModel lot, out string err)
        {

            lot = new LotModel();
            err = string.Empty;
            //port.DeepInitial_UDRQ(lotinfo.LotID);
            try
            {
                int seq = 1;
                lot.JobID = lotinfo.JobID;
                lot.SheetTotalQty = lotinfo.SheetTotalQty;
                lot.IsMutiLot = lotinfo.IsMutiLot;
                lot.LocalPortStation = lotinfo.PortID;
                foreach (var item in lotinfo.LotIDList)
                {
                    Lot subLot = new Lot();
                    subLot.LotSeq = seq;
                    subLot.PN = item.PN;
                    subLot.LotID = item.LotID;
                    subLot.LotStatus = item.LotStatus;
                    subLot.LoadQty = item.LoadQty;
                    subLot.ProductRev = item.ProductRev;
                    subLot.PanelTotalQty = int.Parse(item.PanelTotalQty);
                    subLot.PNLength = item.PNLength;
                    subLot.PNWidth = item.PNWidth;
                    subLot.IsRotate = item.IsRotate;
                    subLot.PortID = lotinfo.PortID;
                    subLot.WorkOrder = item.WorkOrder;
                    //取得配方
                    if (item.ParamList == null)
                    {
                        item.ParamList = new List<MessageModel.Param>();
                    }

                    foreach (var v in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
                    {
                        v.NewEQParameter.Clear();
                    }


                    var aa = GetEqUseParameter(item.ParamList);
                    //
                    foreach (var vP in aa)
                    {

                        EquipmentModel subem = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(vP.SubEqpID);
                        if (subem == null)
                        {
                            continue;
                        }

                        SubEqp eqp = new SubEqp();
                        ParameterModel pm = new ParameterModel();
                        eqp.SubEqpID = vP.SubEqpID;
                        pm.ItemName = vP.ParamName;//GetDataCode(vP.ParamName, subem.EQID); //vP.ParamName;
                        pm.ItemValue = vP.ParamValue;
                        pm.DataType = vP.ParamType;
                        eqp.EQParameter.Add(pm);
                        subLot.SubEqpList.Add(eqp);
                    }


                    //取得WIP DATA
                    if (item.WIPDataList == null)
                    {
                        item.WIPDataList = new List<MessageModel.WipData>();
                    }
                    List<string> lst1 = item.WIPDataList.Select(t => t.SubEqpID).Distinct().ToList();
                    foreach (var v in lst1)
                    {
                        EquipmentModel subem = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(v);
                        if (subem == null)
                        {
                            continue;
                        }
                        subem.List_KeyTraceDataSpec = new List<WIPDataModel>();
                    }

                    #region 
                    WIPDataModel wipData;
                    foreach (var v in item.WIPDataList)
                    {
                        EquipmentModel subem = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(v.SubEqpID);
                        if (subem == null)
                        {
                            continue;
                        }
                        wipData = new WIPDataModel();
                        wipData.SubEqpID = v.SubEqpID;
                        wipData.WIPDataName = v.WIPDataName;
                        wipData.DefaultValue = v.DefaultValue;
                        wipData.ItemMaxValue = v.ItemMaxValue;
                        wipData.ItemMinValue = v.ItemMinValue;
                        wipData.DataType = v.DataType;
                        wipData.ServiceName = v.ServiceName;
                        wipData.TraceFactor = v.TraceFactor;
                        subem.List_KeyTraceDataSpec.Add(wipData);
                        subLot.LotParameterList.Add(wipData);
                    }
                    #endregion
                    //取得Panel List
                    if (item.PanelList == null)
                    {
                        item.PanelList = new List<MessageModel.Panel>();
                    }
                    int LotPanelSeq = 0;
                    foreach (var v in item.PanelList)
                    {
                        PanelModel panel = new PanelModel();
                        if (v.StripList == null)
                        {
                            v.StripList = new List<MessageModel.Strip>();
                        }
                        if (v.BatchPnlList == null)
                        {
                            v.BatchPnlList = new List<MessageModel.BatchPnl>();
                        }
                        panel.CarrierID = lot.CarrierID;
                        panel.LotID = lot.LotID;
                        panel.PN = lot.PN;
                        panel.PanelID = v.PanelID;
                        panel.PanelType = ePanelType.OK;
                        panel.SequenceNo = LotPanelSeq + 1;
                        panel.CreateTime = string.Format("{0:yyyyMMddHHmmss}", DateTime.Now);
                        panel.StartTime = "";
                        panel.EndTime = "";
                        //panel.OutCode = v.OutCode;
                        panel.StripIDList = v.StripList;
                        panel.BatchIDList = v.BatchPnlList;
                        panel.HolePnlID = v.HolePnlID;
                        subLot.PanelList.Add(panel);
                        //port.List_Panel.Add(panel.LotID + "_" + panel.SequenceNo, panel);
                        LotPanelSeq++;
                    }
                    subLot.LotProcessStatus = eLotProcessStatus.Create;
                    subLot.ProcessTime = DateTime.Now;
                    lot.LotList.Add(subLot);
                    seq++;
                }
                //要处理的Panel
                lot.LotProcessStatus = eLotProcessStatus.Create;

                if (port.List_Lot.ContainsKey(lot.JobID))
                {
                    port.List_Lot.Remove(lot.JobID);
                }
                #region [把完成的Lot从Lot列表被移除]
                var RemoveLotList = port.List_Lot.Where(r => r.Value.LotProcessStatus == eLotProcessStatus.Complete).ToList();
                foreach (var RemoveLot in RemoveLotList)
                {
                    port.List_Lot.Remove(RemoveLot.Key);
                }
                //if (port.List_Lot.Count > 10)
                //{
                //    var lotRemove = port.List_Lot.ElementAt(0);
                //    port.List_Lot.Remove(lotRemove.Key);
                //}
                #endregion
                #region [把完成的subLot从subLot列表被移除]
                var RemoveSubLotList = EAPEnvironment.commonLibrary.commonModel.List_ProcessLotKL.Where(r => r.Value.LotProcessStatus == eLotProcessStatus.Complete).ToList();
                foreach (var RemoveSubLot in RemoveSubLotList)
                {
                    EAPEnvironment.commonLibrary.commonModel.List_ProcessLotKL.Remove(RemoveSubLot.Key);
                }
                for (int i = EAPEnvironment.commonLibrary.commonModel.List_ProcessLotKL.Count; EAPEnvironment.commonLibrary.commonModel.List_ProcessLotKL.Count > 100; i--)
                {
                    var lotRemove = EAPEnvironment.commonLibrary.commonModel.List_ProcessLotKL.ElementAt(0);
                    EAPEnvironment.commonLibrary.commonModel.List_ProcessLotKL.Remove(lotRemove.Key);
                }
                #endregion
                bool clear = true;
                foreach (var lotList in lot.LotList)
                {
                    if (lotList.LotProcessStatus != eLotProcessStatus.Complete)
                    {
                        clear = false;
                    }
                }
                if (clear)
                {
                    port.List_Lot.Clear();
                }
                port.List_Lot.Add(lot.JobID, lot);

                string msg = string.Format("JobID<{0}>建置资料完成.", lot.JobID);
                LogMsg(Log.LogLevel.Info, msg);

                return true;
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                err = string.Format("JobID[{0}]大板资料失败，拒绝上机.", lotinfo.JobID);
                LogMsg(Log.LogLevel.Warn, err);
                return false;
            }
        }
      
    }

}
