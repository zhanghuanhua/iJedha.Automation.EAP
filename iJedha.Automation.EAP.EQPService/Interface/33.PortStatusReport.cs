//******************************************************************
//   系统名称 : iJedha.Automation.EAP.SocketService
//   文件概要 : 
//   作    者 : 张明哲
//   <更新履历>
//   1.0.0.0    2020/10/1 14:28:28
//******************************************************************
using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Environment;
using iJedha.Automation.EAP.Library;
using iJedha.Automation.EAP.Model;
using iJedha.Automation.EAP.WebAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace iJedha.Automation.EAP.EQPService
{
    public partial class PortStatusReport : BaseComm
    {
        public void EventHandle(string evtXml)
        {
            try
            {
                #region Decode Message
                SocketMessageStructure.PortStatusReport msg = new SocketMessageStructure.PortStatusReport();
                if (new Serialize<SocketMessageStructure.PortStatusReport>().DeSerializeXML(evtXml, out msg) == false)
                {
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Socket Message<{0}> Decode Error, Content<{1}>", "PortStatusReport", evtXml));
                    return;
                }
                #endregion
                //SetPortEntity();
                #region Record Log
                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("<{2}>Socket Message<{0}> Recv OK , Content<{1}>",
                "PortStatusReport", msg.WriteToXml(), msg.BODY.eqp_id.Trim()));
                #endregion

                #region 设备上报后，EAP做处理。存储Port状态，并上报给MES
                EquipmentModel em = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(msg.BODY.eqp_id.Trim());
                if (em == null)
                {
                    //error log
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}>  Function Name<PortStatusReport> Find Error", msg.BODY.eqp_id.Trim()));
                    return;
                }

                #region 连线检查
                if (em.isCheckConnect && em.isCheckControlMode)
                {
                    if (em.ConnectMode == eConnectMode.DISCONNECT || em.ControlMode != eControlMode.REMOTE)
                    {
                        BaseComm.LogMsg(Log.LogLevel.Error, $"设备ID[{em.EQID}],连线状态为[{em.ConnectMode.ToString()}],控制模式为[{em.ControlMode.ToString()}]");
                        return;
                    }
                }
                #endregion

                #region Reply Message
                HostService.HostService.PortStatusReportReply(msg.HEADER.TRANSACTIONID, msg.BODY.eqp_id.Trim());
                #endregion
                //获取Port信息
                PortModel pm = em.GetPortModelByPortID(msg.BODY.port_id.Trim());
                if (pm == null)
                {
                    //error log
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}>  Function Name<PortStatusReport> PortModel Find Error", msg.BODY.eqp_id.Trim()));
                    return;
                }

                switch (msg.BODY.port_status)
                {
                    case "1"://请求上料
                        pm.PortStatus = ePortStatus.LOADREQUEST;
                        SetPortValue((ePortID)System.Enum.Parse(typeof(ePortID), msg.BODY.port_id), (ePortStatus)System.Enum.Parse(typeof(ePortStatus), msg.BODY.port_status), em.EQID);
                        SetPortEntity(em.EQID);

                        string errr = "";
                        //广合设备的投板机暂存口或收板机的空托盘上料口需要卡控连续叫料问题
                        if (em.EqVendor.Equals("广合设备") && (msg.BODY.port_id.Trim() == ePortID.L01.ToString() || msg.BODY.port_id.Trim() == ePortID.U04.ToString()))
                        {
                            //第一次上报时，添加Flag，并且直接上报MES呼叫AGV
                            if (!EAPEnvironment.commonLibrary.Dic_IsCallAgv.ContainsKey(msg.BODY.port_id.Trim()))
                            {
                                new WebAPIReport().EAP_LoadRequest(new MessageModel.LoadRequest()
                                {
                                    MainEqpID = EAPEnvironment.commonLibrary.MDLN,
                                    SubEqpID = em.EQID,
                                    PortID = msg.BODY.port_id.Trim(),
                                    RequestType = eRequestType.Empty.ToString(),
                                    LoadedQty = EAPEnvironment.commonLibrary.LastLotCount
                                }, em, 1, out errr);
                                if (string.IsNullOrEmpty(errr))
                                {
                                    EAPEnvironment.commonLibrary.Dic_IsCallAgv.TryAdd(msg.BODY.port_id.Trim(), true);
                                }

                            }
                            else
                            //如果不是第一次上报，需卡住
                            {
                                if (EAPEnvironment.commonLibrary.Dic_IsCallAgv[msg.BODY.port_id.Trim()])
                                {
                                    BaseComm.LogMsg(Log.LogLevel.Warn, "目前已存在呼叫AGV任务.");
                                }
                                else
                                {
                                    bool outValue;
                                    new WebAPIReport().EAP_LoadRequest(new MessageModel.LoadRequest()
                                    {
                                        MainEqpID = EAPEnvironment.commonLibrary.MDLN,
                                        SubEqpID = em.EQID,
                                        PortID = msg.BODY.port_id.Trim(),
                                        RequestType = eRequestType.Empty.ToString(),
                                        LoadedQty = EAPEnvironment.commonLibrary.LastLotCount
                                    }, em, 1, out errr);
                                    if (string.IsNullOrEmpty(errr))
                                    {
                                        EAPEnvironment.commonLibrary.Dic_IsCallAgv.TryRemove(msg.BODY.port_id.Trim(), out outValue);
                                        EAPEnvironment.commonLibrary.Dic_IsCallAgv.TryAdd(msg.BODY.port_id.Trim(), true);
                                    }
                                }
                            }
                        }
                        else
                        {
                            new WebAPIReport().EAP_LoadRequest(new MessageModel.LoadRequest()
                            {
                                MainEqpID = EAPEnvironment.commonLibrary.MDLN,
                                SubEqpID = em.EQID,
                                PortID = msg.BODY.port_id.Trim(),
                                RequestType = eRequestType.Empty.ToString(),
                                LoadedQty = EAPEnvironment.commonLibrary.LastLotCount
                            }, em, 1, out errr);
                        }

                        #region [广合收板机下料口请求叫料时，把下料口任务移到出料口。用于出料口请求下料时使用LotID]
                        if (em.EqVendor.Equals("广合设备") && em.Type == eEquipmentType.U && msg.BODY.port_id.Equals("U02"))
                        {
                            var portU01 = em.GetPortModelByPortID(ePortID.U01.ToString());//出料口
                            var portU02 = em.GetPortModelByPortID(ePortID.U02.ToString());//下料口
                            if (portU01 != null && portU02 != null)
                            {
                                portU01.List_Lot.Clear();
                                foreach (var item in portU02.List_Lot)
                                {
                                    portU01.List_Lot.Add(item.Key, item.Value);
                                }
                            }
                            portU02.List_Lot.Clear();
                        }

                        #endregion

                        break;
                    case "2"://上料完成
                        pm.PortStatus = ePortStatus.LOADCOMPLETE;
                        SetPortValue((ePortID)System.Enum.Parse(typeof(ePortID), msg.BODY.port_id), (ePortStatus)System.Enum.Parse(typeof(ePortStatus), msg.BODY.port_status), em.EQID);
                        SetPortEntity(em.EQID);

                        #region 开料机之前逻辑，暂时用不到
                        if (Environment.EAPEnvironment.commonLibrary.lineModel.LineType.Equals("开料线"))
                        {
                            //开料机暂存口上报上料完成，不执行动作
                            if (msg.BODY.port_id.Trim() == ePortID.L01.ToString())
                            {
                                return;
                            }
                            //上料口上报上料完成，把暂存口的Port信息转移到上料口
                            if (msg.BODY.port_id.Trim() == ePortID.L02.ToString())
                            {
                                var portL01 = em.GetPortModelByPortID(ePortID.L01.ToString());
                                var portL02 = em.GetPortModelByPortID(ePortID.L02.ToString());
                                if (portL01 != null && portL02 != null)
                                {
                                    portL02.List_Lot.Clear();
                                    foreach (var item in portL01.List_Lot)
                                    {
                                        portL02.List_Lot.Add(item.Key, item.Value);
                                    }
                                }
                                portL01.List_Lot.Clear();
                            }

                        }
                        #endregion

                        #region [针对多端口设备或者多端口线逻辑，需比对MES下载Lot信息和端口数量是否一致]
                        if (em.Type == eEquipmentType.L || em.Type == eEquipmentType.T)
                        {
                            if (em.isMultiPort)
                            {
                                //    if (EAPEnvironment.commonLibrary.commonModel.LoadCompleteCount.ContainsKey(msg.BODY.port_id.Trim()))
                                //    {
                                //        EAPEnvironment.commonLibrary.commonModel.LoadCompleteCount.Remove(msg.BODY.port_id.Trim());
                                //    }
                                //    EAPEnvironment.commonLibrary.commonModel.LoadCompleteCount.Add(msg.BODY.port_id.Trim(), msg.BODY.port_id.Trim());
                            }
                            else
                            {

                                if (EAPEnvironment.commonLibrary.commonModel.LoadCompleteCount.ContainsKey(em.EQID))
                                {
                                    EAPEnvironment.commonLibrary.commonModel.LoadCompleteCount.Remove(em.EQID);
                                }
                                EAPEnvironment.commonLibrary.commonModel.LoadCompleteCount.Add(em.EQID, em.EQID);
                            }

                        }
                        #endregion

                        #region [如果是冲孔连棕化或者PP裁切机时，需设定是否上报给MESLoadComplete事件]
                        if (Environment.EAPEnvironment.commonLibrary.lineModel.isLoadCompleteCountCheck)
                        {
                            if (em.isReportLoadComplete)
                            {
                                new WebAPIReport().EAP_LoadComplete(new MessageModel.LoadComplete()
                                {
                                    MainEqpID = EAPEnvironment.commonLibrary.MDLN,
                                    SubEqpID = em.EQID,
                                    PortID = msg.BODY.port_id.Trim(),
                                    CarrierID = ""
                                }, em, 1);
                            }
                        }
                        #endregion
                        else
                        {
                            //获取设备对应上报Carrier ID
                            var cID = (from q in Environment.EAPEnvironment.commonLibrary.commonModel.Dic_CarrierIDList where q.Key == em.EQID select q.Value).FirstOrDefault();
                            new WebAPIReport().EAP_LoadComplete(new MessageModel.LoadComplete()
                            {
                                MainEqpID = EAPEnvironment.commonLibrary.MDLN,
                                SubEqpID = em.EQID,
                                PortID = msg.BODY.port_id.Trim(),
                                CarrierID = cID
                            }, em, 1);
                        }

                        break;
                    case "3"://请求下料
                        pm.PortStatus = ePortStatus.UNLOADREQUEST;
                        SetPortValue((ePortID)System.Enum.Parse(typeof(ePortID), msg.BODY.port_id), (ePortStatus)System.Enum.Parse(typeof(ePortStatus), msg.BODY.port_status), em.EQID);
                        SetPortEntity(em.EQID);
                        //开料机退料时清除所有任务信息    暂时不启用
                        if (em.EqNameKey == ConstLibrary.CONST_ACL)
                        {
                            if (msg.BODY.port_id.Trim() == ePortID.L02.ToString())
                            {
                                EAPEnvironment.commonLibrary.commonModel.List_ProcessLotKL.Clear();
                                pm.List_Lot.Clear();
                            }
                        }
                        //如果是PP裁切机，上报UnloadRequest时，清除当前上报数量，清掉PP物料Lot ID
                        if (em.isMultiPort)
                        {
                            EAPEnvironment.commonLibrary.commonModel.LoadCompleteCount.Remove(msg.BODY.port_id.Trim());
                            pm.MaterialLotID_PP = string.Empty;
                        }
                        //退空托盘，上报MES空RequestType
                        //if (msg.BODY.port_id.Trim() == ePortID.L04.ToString())
                        //{
                        //    new WebAPIReport().EAP_UnLoadRequest(new MessageModel.UnLoadRequest()
                        //    {
                        //        MainEqpID = EAPEnvironment.commonLibrary.MDLN,
                        //        SubEqpID = em.EQID,
                        //        PortID = msg.BODY.port_id.Trim(),
                        //        CarrierID = pm.CarrierID,
                        //        RequestType = eRequestType.Empty.ToString(),
                        //        LotID = ""
                        //    }, em, 1, out errr);
                        //}
                        //投板机上报UnloadRequest时，从Port.List_Lot内清除当前Lot
                        LotModel lm = EAPEnvironment.commonLibrary.commonModel.GetLotModelByLotID(pm.List_Lot.Count == 0 ? em.CurrentLotID : (from q in pm.List_Lot select q.Key).FirstOrDefault());
                        if (lm != null)
                        {
                            lm.LotProcessStatus = eLotProcessStatus.Complete;
                            if (em.Type == eEquipmentType.L)
                            {
                                pm.DeepInitial_UDRQ(lm.LotID);
                                new WebAPIReport().EAP_UnLoadRequest(new MessageModel.UnLoadRequest()
                                {
                                    MainEqpID = EAPEnvironment.commonLibrary.MDLN,
                                    SubEqpID = em.EQID,
                                    PortID = msg.BODY.port_id.Trim(),
                                    CarrierID = pm.CarrierID,
                                    RequestType = eRequestType.Full.ToString(),
                                    LotID = em.CurrentLotID
                                }, em, 1, out errr);
                            }
                            else
                            {
                                if (EAPEnvironment.commonLibrary.lineModel.LineType == "开料线")
                                {
                                    new WebAPIReport().EAP_UnLoadRequest(new MessageModel.UnLoadRequest()
                                    {
                                        MainEqpID = EAPEnvironment.commonLibrary.MDLN,
                                        SubEqpID = em.EQID,
                                        PortID = msg.BODY.port_id.Trim(),
                                        CarrierID = pm.CarrierID,
                                        RequestType = eRequestType.Full.ToString(),
                                        //Port有lot时，取Port内的LotID，Port没lot时，取当前任务作为LotID
                                        LotID = pm.List_SubLot.Count == 0 ? em.CurrentLotID : (from q in pm.List_SubLot select q.Key).FirstOrDefault()
                                    }, em, 1, out errr);
                                }
                                else
                                {
                                    //广合设备的投板机空托盘退料或收板机的下料口需要卡控连续呼叫AGV问题
                                    if (em.EqVendor.Equals("广合设备") && msg.BODY.port_id.Trim() == ePortID.U01.ToString())
                                    {
                                        if (!SetUnloadRequestDoubleFlag(em, pm, msg)) return;
                                    }
                                    else
                                    {
                                        new WebAPIReport().EAP_UnLoadRequest(new MessageModel.UnLoadRequest()
                                        {
                                            MainEqpID = EAPEnvironment.commonLibrary.MDLN,
                                            SubEqpID = em.EQID,
                                            PortID = msg.BODY.port_id.Trim(),
                                            CarrierID = pm.CarrierID,
                                            RequestType = eRequestType.Full.ToString(),
                                            //Port有lot时，取Port内的LotID，Port没lot时，取当前任务作为LotID
                                            LotID = pm.List_Lot.Count == 0 ? em.CurrentLotID : (from q in pm.List_Lot select q.Key).FirstOrDefault()
                                        }, em, 1, out errr);
                                    }
                                }

                            }
                        }
                        else
                        {
                            if (em.EqVendor.Equals("广合设备") && msg.BODY.port_id.Trim() == ePortID.L04.ToString())
                            {
                                SetUnloadRequestDoubleFlag(em, pm, msg);
                            }
                            else
                            {
                                string errMsg = string.Format("Equipment ID<{0}>  Function Name<PortStatusReport> LotModel Error,可能未上报EquipmentJobDataProcessReport[开始生产任务]", msg.BODY.eqp_id.Trim());
                                BaseComm.LogMsg(Log.LogLevel.Warn, errMsg);
                                History.EAP_EQP_EVENTHISTORY(em, eEventName.EQP_UNLOADREQUEST, eEventFlow.EQP2EAP, errMsg, pm.List_Lot.Count == 0 ? em.CurrentLotID : (from q in pm.List_Lot select q.Key).FirstOrDefault(), msg.BODY.port_id);
                            }

                        }


                        //收板机出料位上报UnloadRequest时，上报TrackOut
                        if (msg.BODY.port_id.Trim() == ePortID.U01.ToString())
                        {
                            #region 开料线TrackOut逻辑
                            if (EAPEnvironment.commonLibrary.lineModel.LineType == "开料线")
                            {
                                Lot subLot = EAPEnvironment.commonLibrary.commonModel.GetLotModelBySubLotID(pm.List_SubLot.Count == 0 ? em.CurrentLotID : (from q in pm.List_SubLot select q.Key).FirstOrDefault());

                                if (subLot == null)
                                {
                                    string errMsg = string.Format("Equipment ID<{0}>  Function Name<PortStatusReport> LotModel Error,可能未上报EquipmentJobDataProcessReport[开始生产任务]", msg.BODY.eqp_id.Trim());
                                    //error log
                                    BaseComm.LogMsg(Log.LogLevel.Warn, errMsg);
                                    History.EAP_EQP_EVENTHISTORY(em, eEventName.EQP_UNLOADREQUEST, eEventFlow.EQP2EAP, errMsg, pm.List_SubLot.Count == 0 ? em.CurrentLotID : (from q in pm.List_SubLot select q.Key).FirstOrDefault(), msg.BODY.port_id);
                                    return;
                                }
                                //LotModel lm = EAPEnvironment.commonLibrary.commonModel.GetLotModelBySubLot(subLot);
                                //if (lm != null)
                                //{
                                new WebAPIReport().EAP_LotTrackOut(new MessageModel.LotTrackOut()
                                {
                                    MainEqpID = EAPEnvironment.commonLibrary.MDLN,
                                    SubEqpID = em.EQID,
                                    PortID = msg.BODY.port_id.Trim(),
                                    LotID = pm.List_SubLot.Count == 0 ? em.CurrentLotID : (from q in pm.List_SubLot select q.Key).FirstOrDefault(),
                                    PanelTotalQty = "",
                                    NGFlag = false,//先记录false
                                    PanelList = GetPanelList(subLot),
                                    WIPDataList = GetTrackOutWipData(subLot),
                                    JobID = "",
                                    JobTotalQty = subLot.LoadQty,
                                    PN = subLot.PN
                                }, subLot, em, 1, out errr);
                                //}

                                #endregion
                            }
                            else
                            {
                                //LotModel lm = EAPEnvironment.commonLibrary.commonModel.GetLotModelByLotID(pm.List_Lot.Count == 0 ? em.CurrentLotID : (from q in pm.List_Lot select q.Key).FirstOrDefault());
                                //lm.LotProcessStatus = eLotProcessStatus.Complete;
                                if (lm == null)
                                {
                                    string errMsg = string.Format("Equipment ID<{0}>  Function Name<PortStatusReport> LotModel Find Error,可能未上报EquipmentJobDataProcessReport[开始生产任务]", msg.BODY.eqp_id.Trim());
                                    //error log
                                    BaseComm.LogMsg(Log.LogLevel.Error, errMsg);
                                    History.EAP_EQP_EVENTHISTORY(em, eEventName.EQP_UNLOADREQUEST, eEventFlow.EQP2EAP, errMsg, pm.List_Lot.Count == 0 ? em.CurrentLotID : (from q in pm.List_Lot select q.Key).FirstOrDefault(), msg.BODY.port_id);
                                    return;
                                }
                                string err = "";
                                #region 分板线TrackOut逻辑
                                if (Environment.EAPEnvironment.commonLibrary.lineModel.LineType.Equals("分板线"))
                                {

                                    var lotID = (from n in Environment.EAPEnvironment.commonLibrary.commonModel.TrackOutItems from I in n.tLotList select I.tLotID).FirstOrDefault();
                                    new WebAPIReport().EAP_LotTrackOut(new MessageModel.LotTrackOut()
                                    {
                                        MainEqpID = EAPEnvironment.commonLibrary.MDLN,
                                        SubEqpID = em.EQID,
                                        PortID = msg.BODY.port_id.Trim(),
                                        LotID = lotID,
                                        PanelTotalQty = "",
                                        NGFlag = false,//先记录false
                                        PanelList = GetPanelList(lotID),
                                        WIPDataList = new List<MessageModel.WipData>(),
                                        JobID = "",//Batch ID
                                        JobTotalQty = lm.UnloadQty,
                                        PN = lm.PN
                                    }, lm, em, 1, out err);
                                }
                                #endregion
                                else if (Environment.EAPEnvironment.commonLibrary.lineModel.LineType.Equals("冲孔连棕化"))
                                {
                                    if (lm == null)
                                    {
                                        string errMsg = string.Format("Equipment ID<{0}>  Function Name<PortStatusReport> LotModel Find Error,可能未上报EquipmentJobDataProcessReport[开始生产任务]", msg.BODY.eqp_id.Trim());
                                        //error log
                                        BaseComm.LogMsg(Log.LogLevel.Error, errMsg);
                                        History.EAP_EQP_EVENTHISTORY(em, eEventName.EQP_UNLOADREQUEST, eEventFlow.EQP2EAP, errMsg, pm.List_Lot.Count == 0 ? em.CurrentLotID : (from q in pm.List_Lot select q.Key).FirstOrDefault(), msg.BODY.port_id);
                                        return;
                                    }
                                    lm.LotProcessStatus = eLotProcessStatus.Complete;
                                    //做Track Out
                                    if (new WebAPIReport().EAP_LotTrackOut(new MessageModel.LotTrackOut()
                                    {
                                        MainEqpID = EAPEnvironment.commonLibrary.MDLN,
                                        SubEqpID = em.EQID,
                                        PortID = msg.BODY.port_id.Trim(),
                                        LotID = pm.List_Lot.Count == 0 ? em.CurrentLotID : (from q in pm.List_Lot select q.Key).FirstOrDefault(),
                                        PanelTotalQty = SetTrackOutCount(lm, EAPEnvironment.commonLibrary.CurrentLotCount),//EAPEnvironment.commonLibrary.CurrentLotCount,
                                        NGFlag = false,//先记录false
                                        PanelList = new List<MessageModel.Panel>(),//无需上报Panel List
                                        WIPDataList = GetTrackOutWipData(lm),
                                        JobID = "",//Batch ID
                                        JobTotalQty = SetTrackOutCount(lm, EAPEnvironment.commonLibrary.CurrentLotCount),//EAPEnvironment.commonLibrary.CurrentLotCount,
                                        PN = lm.PN
                                    }, lm, em, 1, out err))
                                    {
                                        //删除当前Lot之前所有Lot信息
                                        Environment.EAPEnvironment.commonLibrary.commonModel.RemoveFrontProcessLotModel(lm);
                                    }
                                }
                                else
                                {
                                    if (new WebAPIReport().EAP_LotTrackOut(new MessageModel.LotTrackOut()
                                    {
                                        MainEqpID = EAPEnvironment.commonLibrary.MDLN,
                                        SubEqpID = em.EQID,
                                        PortID = msg.BODY.port_id.Trim(),
                                        LotID = pm.List_Lot.Count == 0 ? em.CurrentLotID : (from q in pm.List_Lot select q.Key).FirstOrDefault(),
                                        PanelTotalQty = lm.PanelTotalQty.ToString(),
                                        NGFlag = false,//先记录false
                                        PanelList = GetPanelList(lm),
                                        WIPDataList = GetTrackOutWipData(lm),
                                        JobID = "",//Batch ID
                                        JobTotalQty = lm.PanelTotalQty.ToString(),
                                        PN = lm.PN
                                    }, lm, em, 1, out err))
                                    {
                                        Environment.EAPEnvironment.commonLibrary.commonModel.RemoveProcessLotModel(lm);
                                    }
                                }
                            }
                        }

                        break;
                    case "4"://下料完成
                        SetPortValue((ePortID)System.Enum.Parse(typeof(ePortID), msg.BODY.port_id), (ePortStatus)System.Enum.Parse(typeof(ePortStatus), msg.BODY.port_status), em.EQID);
                        SetPortEntity(em.EQID);
                        pm.PortStatus = ePortStatus.UNLOADCOMPLETE;
                        //冲孔连棕化Track Out逻辑，上报的JobTotalQty要和设备上报完工信号的数量一致
                        if (Environment.EAPEnvironment.commonLibrary.lineModel.LineType.Equals("冲孔连棕化"))
                        {
                            ////获取Lot物件
                            //LotModel lm = EAPEnvironment.commonLibrary.commonModel.GetLotModelByLotID(em.CurrentCompleteLotID.Trim());
                            ////lm.LotProcessStatus = eLotProcessStatus.Complete;
                            //if (lm == null)
                            //{
                            //    //error log
                            //    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}>  Function Name<PortStatusReport> LotModel Find Error,可能未上报EquipmentJobDataProcessReport[开始生产任务]", msg.BODY.eqp_id.Trim()));
                            //    return;
                            //}
                            //lm.LotProcessStatus = eLotProcessStatus.Complete;
                            ////做Track Out
                            //new WebAPIReport().EAP_LotTrackOut(new MessageModel.LotTrackOut()
                            //{
                            //    MainEqpID = EAPEnvironment.commonLibrary.MDLN,
                            //    SubEqpID = em.EQID,
                            //    PortID = msg.BODY.port_id.Trim(),
                            //    LotID = em.CurrentCompleteLotID,
                            //    PanelTotalQty = EAPEnvironment.commonLibrary.CurrentLotCount,
                            //    NGFlag = false,//先记录false
                            //    PanelList = new List<MessageModel.Panel>(),//无需上报Panel List
                            //    WIPDataList = GetTrackOutWipData(lm),
                            //    JobID = "",//Batch ID
                            //    JobTotalQty = EAPEnvironment.commonLibrary.CurrentLotCount,
                            //    PN = lm.PN
                            //}, lm, em, 1);
                            ////删除当前Lot之前所有Lot信息
                            //Environment.EAPEnvironment.commonLibrary.commonModel.RemoveFrontProcessLotModel(lm);
                        }
                        new WebAPIReport().EAP_UnLoadComplete(new MessageModel.UnLoadComplete()
                        {
                            MainEqpID = EAPEnvironment.commonLibrary.MDLN,
                            SubEqpID = em.EQID,
                            PortID = msg.BODY.port_id.Trim(),
                            CarrierID = ""
                        }, em, 1);

                        break;
                    default:
                        BaseComm.LogMsg(Log.LogLevel.Warn, string.Format("Equipment ID<{0}>  Function Name<PortStatusReport> PortStatus Error", msg.BODY.eqp_id.Trim()));
                        break;
                }
                #endregion
                eEventName eventName = eEventName.UNKNOW;
                string eventValue = "";
                switch (msg.BODY.port_status)
                {
                    case "1":
                        eventName = eEventName.EQP_LOADREQUEST;
                        eventValue = "上料请求";
                        break;
                    case "2":
                        eventName = eEventName.EQP_LOADREQUEST;
                        eventValue = "上料完成";
                        break;
                    case "3":
                        eventName = eEventName.EQP_UNLOADREQUEST;
                        eventValue = "下料请求";
                        break;
                    case "4":
                        eventName = eEventName.EQP_UNLOADCOMPLETE;
                        eventValue = "下料完成";
                        break;
                    default:
                        break;
                }
                History.EAP_EQP_EVENTHISTORY(em, eventName, eEventFlow.EQP2EAP, eventValue, pm.List_Lot.Count == 0 ? em.CurrentLotID : (from q in pm.List_Lot select q.Key).FirstOrDefault(), msg.BODY.port_id);

            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", "PortStatusReport", ex.Message.ToString(), ex.StackTrace.ToString()));
            }
            finally
            {

            }
        }
        public bool SetUnloadRequestDoubleFlag(EquipmentModel em, PortModel pm, SocketMessageStructure.PortStatusReport msg)
        {
            string errr;
            try
            {
                //第一次上报时，添加Flag，并且直接上报MES呼叫AGV
                if (!EAPEnvironment.commonLibrary.Dic_IsCallAgv.ContainsKey(msg.BODY.port_id.Trim()))
                {
                    new WebAPIReport().EAP_UnLoadRequest(new MessageModel.UnLoadRequest()
                    {
                        MainEqpID = EAPEnvironment.commonLibrary.MDLN,
                        SubEqpID = em.EQID,
                        PortID = msg.BODY.port_id.Trim(),
                        CarrierID = pm.CarrierID,
                        RequestType = eRequestType.Full.ToString(),
                        //Port有lot时，取Port内的LotID，Port没lot时，取当前任务作为LotID
                        LotID = pm.List_Lot.Count == 0 ? em.CurrentLotID : (from q in pm.List_Lot select q.Key).FirstOrDefault()
                    }, em, 1, out errr);

                    if (string.IsNullOrEmpty(errr))
                    {
                        EAPEnvironment.commonLibrary.Dic_IsCallAgv.TryAdd(msg.BODY.port_id.Trim(), true);
                    }
                }
                else
                //如果不是第一次上报，需卡住
                {
                    if (EAPEnvironment.commonLibrary.Dic_IsCallAgv[msg.BODY.port_id.Trim()])
                    {
                        BaseComm.LogMsg(Log.LogLevel.Warn, "目前已存在呼叫AGV任务.");
                        return false;
                    }
                    else
                    {
                        bool outValue;
                        new WebAPIReport().EAP_UnLoadRequest(new MessageModel.UnLoadRequest()
                        {
                            MainEqpID = EAPEnvironment.commonLibrary.MDLN,
                            SubEqpID = em.EQID,
                            PortID = msg.BODY.port_id.Trim(),
                            CarrierID = pm.CarrierID,
                            RequestType = eRequestType.Full.ToString(),
                            //Port有lot时，取Port内的LotID，Port没lot时，取当前任务作为LotID
                            LotID = pm.List_Lot.Count == 0 ? em.CurrentLotID : (from q in pm.List_Lot select q.Key).FirstOrDefault()
                        }, em, 1, out errr);
                        if (string.IsNullOrEmpty(errr))
                        {
                            EAPEnvironment.commonLibrary.Dic_IsCallAgv.TryRemove(msg.BODY.port_id.Trim(), out outValue);
                            EAPEnvironment.commonLibrary.Dic_IsCallAgv.TryAdd(msg.BODY.port_id.Trim(), true);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return false;
            }
           
        }
        public List<MessageModel.Panel> GetPanelList(LotModel lm)
        {
            List<MessageModel.Panel> lstPanel = new List<MessageModel.Panel>();
            try
            {
                #region 获取Panel List

                foreach (var item in lm.PanelList)
                {
                    MessageModel.Panel mPanel = new MessageModel.Panel();
                    mPanel.PanelID = item.PanelID;
                    mPanel.StripList = item.StripIDList;
                    mPanel.OutCode = item.OutPanelID;
                    mPanel.BatchPnlList = item.BatchIDList;
                    mPanel.HolePnlID = item.HolePnlID;
                    lstPanel.Add(mPanel);
                }
                #endregion
                return lstPanel;
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return lstPanel;
            }
        }
        /// <summary>
        /// 开料线获取Panel列表
        /// </summary>
        /// <param name="lm"></param>
        /// <returns></returns>
        public List<MessageModel.Panel> GetPanelList(Lot lm)
        {
            List<MessageModel.Panel> lstPanel = new List<MessageModel.Panel>();
            try
            {
                #region 获取Panel List

                foreach (var item in lm.PanelList)
                {
                    MessageModel.Panel mPanel = new MessageModel.Panel();
                    mPanel.PanelID = item.PanelID;
                    mPanel.StripList = item.StripIDList;
                    mPanel.OutCode = item.OutPanelID;
                    mPanel.BatchPnlList = item.BatchIDList;
                    mPanel.HolePnlID = item.HolePnlID;
                    lstPanel.Add(mPanel);
                }
                #endregion
                return lstPanel;
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return lstPanel;
            }
        }
        /// <summary>
        /// 分板线获取Panel列表
        /// </summary>
        /// <param name="lotID"></param>
        /// <returns></returns>
        public List<MessageModel.Panel> GetPanelList(string lotID)
        {
            List<MessageModel.Panel> lstPanel = new List<MessageModel.Panel>();
            try
            {
                var tPanelList = (from n in Environment.EAPEnvironment.commonLibrary.commonModel.TrackOutItems from I in n.tLotList from a in I.toutPnlList where a.tOutPnlID.Contains(lotID) select a).ToList();
                #region 获取Panel List

                foreach (var item in tPanelList)
                {
                    MessageModel.Panel mPanel = new MessageModel.Panel();
                    mPanel.PanelID = item.tPanelID;
                    mPanel.OutCode = item.tOutPnlID;
                    lstPanel.Add(mPanel);
                }
                #endregion
                return lstPanel;
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return lstPanel;
            }
        }
        public List<MessageModel.WipData> GetTrackOutWipData(LotModel lot)
        {
            List<MessageModel.WipData> List_wip = new List<MessageModel.WipData>();
            try
            {
                foreach (var v in lot.LotParameterList)
                {
                    MessageModel.WipData data = new MessageModel.WipData();
                    if (v.ServiceName != "TrackOutLot")
                    {
                        continue;
                    }
                    EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(v.SubEqpID);
                    string tracevaule = string.Empty;
                    var vv = (from t in em.List_KeyTraceDataSpec where t.WIPDataName == v.WIPDataName select t).FirstOrDefault();
                    if (vv != null)
                    {
                        data.ServiceName = "TrackOutLot";
                        data.ItemMaxValue = v.ItemMaxValue;
                        data.ItemMinValue = v.ItemMinValue;
                        data.DataType = v.DataType;
                        data.DefaultValue = vv.DefaultValue;
                        data.TraceFactor = v.TraceFactor;
                        data.SubEqpID = v.SubEqpID;
                        data.WIPDataName = v.WIPDataName;
                        List_wip.Add(data);
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
        public List<MessageModel.WipData> GetTrackOutWipData(Lot lot)
        {
            List<MessageModel.WipData> List_wip = new List<MessageModel.WipData>();
            try
            {
                foreach (var v in lot.LotParameterList)
                {
                    MessageModel.WipData data = new MessageModel.WipData();
                    if (v.ServiceName != "TrackOutLot")
                    {
                        continue;
                    }
                    EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(v.SubEqpID);
                    string tracevaule = string.Empty;
                    var vv = (from t in em.List_KeyTraceDataSpec where t.WIPDataName == v.WIPDataName select t).FirstOrDefault();
                    if (vv != null)
                    {
                        data.ServiceName = "TrackOutLot";
                        data.ItemMaxValue = v.ItemMaxValue;
                        data.ItemMinValue = v.ItemMinValue;
                        data.DataType = v.DataType;
                        data.DefaultValue = vv.DefaultValue;
                        data.TraceFactor = v.TraceFactor;
                        data.SubEqpID = v.SubEqpID;
                        data.WIPDataName = v.WIPDataName;
                        List_wip.Add(data);
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
        public string SetTrackOutCount(LotModel lm, string processCount)
        {
            string count = "";
            try
            {
                if (string.IsNullOrEmpty(processCount) || processCount == "0")
                {
                    BaseComm.LogMsg(Log.LogLevel.Warn, $"设备上报生产数量为[{processCount}]");
                    return count;
                }
                if (lm.InnerLotList.Count != 0)
                {
                    count = (int.Parse(processCount) / lm.InnerLotList.Count()).ToString();
                }

                return count;
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return count;
            }

        }
        /// <summary>
        /// 记录Port上报状态
        /// </summary>
        /// <param name="portID"></param>
        /// <param name="portStatus"></param>
        /// <param name="eqpID"></param>
        public void SetPortValue(ePortID portID, ePortStatus portStatus, string eqpID)
        {
            try
            {
                switch (portID)
                {
                    case ePortID.L01:
                        EAPEnvironment.commonLibrary.portEntity.L01 = portStatus;
                        EAPEnvironment.commonLibrary.portEntity.eap_id = eqpID;
                        break;
                    case ePortID.L02:
                        EAPEnvironment.commonLibrary.portEntity.L02 = portStatus;
                        EAPEnvironment.commonLibrary.portEntity.eap_id = eqpID;
                        break;
                    case ePortID.L03:
                        EAPEnvironment.commonLibrary.portEntity.L03 = portStatus;
                        EAPEnvironment.commonLibrary.portEntity.eap_id = eqpID;
                        break;
                    case ePortID.L04:
                        EAPEnvironment.commonLibrary.portEntity.L04 = portStatus;
                        EAPEnvironment.commonLibrary.portEntity.eap_id = eqpID;
                        break;
                    case ePortID.U01:
                        EAPEnvironment.commonLibrary.portEntity.U01 = portStatus;
                        EAPEnvironment.commonLibrary.portEntity.eap_id = eqpID;
                        break;
                    case ePortID.U02:
                        EAPEnvironment.commonLibrary.portEntity.U02 = portStatus;
                        EAPEnvironment.commonLibrary.portEntity.eap_id = eqpID;
                        break;
                    case ePortID.U03:
                        EAPEnvironment.commonLibrary.portEntity.U03 = portStatus;
                        EAPEnvironment.commonLibrary.portEntity.eap_id = eqpID;
                        break;
                    case ePortID.U04:
                        EAPEnvironment.commonLibrary.portEntity.U04 = portStatus;
                        EAPEnvironment.commonLibrary.portEntity.eap_id = eqpID;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));

            }
        }
        /// <summary>
        /// 生成PortEntity文件
        /// </summary>
        /// <param name="eapID"></param>
        public void SetPortEntity(string eapID)
        {
            try
            {
                string directoryPath = $@"{EAPEnvironment.commonLibrary.StartupPath}\PortEntity";//定义一个路径变量
                string filePath = $"{eapID}_PortEntity.xml";//定义一个文件路径变量
                if (!Directory.Exists(directoryPath))//如果路径不存在
                {
                    Directory.CreateDirectory(directoryPath);//创建一个路径的文件夹
                }
                StreamWriter sw = new StreamWriter(Path.Combine(directoryPath, filePath));
                var xml = EAPEnvironment.commonLibrary.portEntity.WriteToXml();
                sw.Write(xml);
                sw.Flush();
                sw.Close();
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));

            }
        }


    }
}
