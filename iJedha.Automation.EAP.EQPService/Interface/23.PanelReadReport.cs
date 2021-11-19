//******************************************************************
//   系统名称 : iJedha.Automation.EAP.SocketService
//   文件概要 : 
//   作    者 : 张明哲
//   <更新履历>
//   1.0.0.0    2020/10/1 14:28:28
//******************************************************************
using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Environment;
using iJedha.Automation.EAP.Model;
using System;
using System.Linq;

namespace iJedha.Automation.EAP.EQPService
{
    public partial class PanelReadReport : BaseComm
    {
        public void EventHandle(string evtXml)
        {
            try
            {
                #region Decode Message
                SocketMessageStructure.PanelReadReport msg = new SocketMessageStructure.PanelReadReport();
                if (new Serialize<SocketMessageStructure.PanelReadReport>().DeSerializeXML(evtXml, out msg) == false)
                {
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Socket Message<{0}> Decode Error, Content<{1}>", "PanelReadReport", evtXml));
                    return;
                }
                #endregion

                #region Record Log
                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("<{2}>Socket Message<{0}> Recv OK , Content<{1}>",
                "PanelReadReport", msg.WriteToXml(), msg.BODY.eqp_id.Trim()));
                #endregion



                #region EAP记录设备当前读板信息
                //根据设备ID获取设备信息
                EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(msg.BODY.eqp_id.Trim());

                if (em == null)
                {
                    //error log
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<PanelReadReport> Find Error", msg.BODY.eqp_id.Trim()));
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
                HostService.HostService.PanelReadReportReply(msg.HEADER.TRANSACTIONID, msg.BODY.eqp_id.Trim());
                #endregion

                if (string.IsNullOrEmpty(msg.BODY.panel_id.Trim()) || msg.BODY.panel_id.Trim().ToUpper().Equals("ERROR"))
                {
                    string errMsg = string.Format("E0003:Equipment ID[{0}] 读取ID [{1}]Error", msg.BODY.eqp_id.Trim(), msg.BODY.panel_id);
                    new HostService.HostService().CIMMessageCommand(em.EQID, "10", errMsg, msg.HEADER.TRANSACTIONID);

                    BaseComm.LogMsg(Log.LogLevel.Error, errMsg);
                    return;
                }
                #endregion

               
                #region 获取下货资料
                if (EAPEnvironment.commonLibrary.lineModel.LineType != "开料线")
                {
                    if (EAPEnvironment.commonLibrary.lineModel.isInnerLotPanelList)
                    {
                        LotModel lm = EAPEnvironment.commonLibrary.commonModel.GetLotModelByInnerPanelID(msg.BODY.panel_id.Trim());
                        if (lm == null)
                        {
                            #region 如果设备时投收板机，在Lot_List内找不到读取的Panel ID时，说明混批，要通知MES进行批次Hold
                            string errMsg = string.Format("Equipment ID<{0}> Function Name<PanelReadReport> LotModel Find Error", msg.BODY.eqp_id.Trim());
                            BaseComm.LogMsg(Log.LogLevel.Error, errMsg);
                            #endregion

                            if (em.Type == eEquipmentType.U)
                            {
                                new HostService.HostService().RemoteControlCommand(em.EQID, "", eRemoteCommand.PanelNG.GetEnumDescription());
                            }
                            else
                            {
                                #region [投板机发现混批，通知投板机停止投板]
                                var LoadEm = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByType(eEquipmentType.L);
                                if (LoadEm != null)
                                {
                                    if (LoadEm.isCheckPanelID)
                                    {
                                        new HostService.HostService().RemoteControlCommand(LoadEm.EQID, "", eRemoteCommand.Stop.GetEnumDescription());
                                    }
                                    
                                }
                                #endregion
                            }
                            return;
                        }
                        if (em.Type == eEquipmentType.U)
                        {
                            new HostService.HostService().RemoteControlCommand(em.EQID, "", eRemoteCommand.PanelOK.GetEnumDescription());
                            #region [陪镀板判断逻辑]

                            #endregion
                        }

                        string NowLayer = string.Empty;
                        #region [发现棕化线顺序不符，通知侧向水平式放板停止投板]
                        if (lm.InnerLotList.Count != 0)
                        {
                            //设备是否检查层别
                            if (em.isCheckLayerNo)
                            {
                                //用Panel ID获取内层设备ID，获取设备信息
                                var GetSubEq = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByEqID((from q in lm.InnerLotList from w in q.ListPanel where w.PanelID == msg.BODY.panel_id.Trim() select q.SubEqpID).FirstOrDefault());

                                if (GetSubEq != null)
                                {
                                    EAPEnvironment.commonLibrary.CCDGetSubEqList += GetSubEq.EQID;
                                    //判断MES下载的所有内层设备ID组合是否包含从设备上报Panel ID获得的设备ID组合
                                    if (lm.MESInnerLotSubEqInfo.Contains(EAPEnvironment.commonLibrary.CCDGetSubEqList))
                                    {
                                        //判断是否为第一个侧向放板机
                                        if (EAPEnvironment.commonLibrary.isFirstSubEq)
                                        {
                                            EAPEnvironment.commonLibrary.isFirstSubEq = false;
                                            string compare = string.Empty;
                                            //防呆，总长度要大于CCD扫描上报的设备ID长度才可进行拆解
                                            if (lm.MESInnerLotSubEqInfo.Length > EAPEnvironment.commonLibrary.CCDGetSubEqList.Length)
                                            {
                                                compare = lm.MESInnerLotSubEqInfo.Substring(0, EAPEnvironment.commonLibrary.CCDGetSubEqList.Length);
                                            }
                                            //如果第一个侧向放板机的设备ID和MES下载的不一致，报错
                                            if (EAPEnvironment.commonLibrary.CCDGetSubEqList!=compare)
                                            {
                                                SendRemoteCommand(lm, msg);
                                                return;
                                            }
                                            else
                                            {
                                                //记录当前层别信息
                                                EAPEnvironment.commonLibrary.FrontLayer = (from q in lm.InnerLotList from w in q.ListPanel where w.PanelID == msg.BODY.panel_id.Trim() select q.InnerLayer).FirstOrDefault();
                                            }
                                        }
                                        else
                                        {
                                            //判断CCD扫描上报设备ID组合是否和MES下载设备ID组合一致
                                            if (lm.MESInnerLotSubEqInfo.Equals(EAPEnvironment.commonLibrary.CCDGetSubEqList))
                                            {
                                                var LoadEm = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByType(eEquipmentType.L);
                                                EAPEnvironment.commonLibrary.CCDGetSubEqList = string.Empty;
                                                EAPEnvironment.commonLibrary.FrontLayer = string.Empty;
                                                EAPEnvironment.commonLibrary.isFirstSubEq = true;
                                                //如果CCD上报的ID和MES下载的设备ID一致，通知投板机开始投放下一套板
                                                foreach (var eqID in lm.MESInnerLotSubEqList)
                                                {
                                                    new HostService.HostService().RemoteControlCommand(eqID, ePortID.L01.ToString(), eRemoteCommand.Start.GetEnumDescription());
                                                }
                                                
                                            }
                                            else
                                            {
                                                //记录当前层别信息
                                                EAPEnvironment.commonLibrary.FrontLayer = (from q in lm.InnerLotList from w in q.ListPanel where w.PanelID == msg.BODY.panel_id.Trim() select q.InnerLayer).FirstOrDefault();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        SendRemoteCommand(lm, msg);
                                        return;
                                    }
                                }
                            }
                            else
                            {

                            }
                        }
                        #endregion
                    }
                    else
                    {
                        //根据设备上报的板ID获取批次信息
                        LotModel lm = EAPEnvironment.commonLibrary.commonModel.GetLotModelByPanelID(msg.BODY.panel_id.Trim());
                        if (lm == null)
                        {
                            #region 如果设备时投收板机，在Lot_List内找不到读取的Panel ID时，说明混批，要通知MES进行批次Hold
                            string errMsg = string.Format("Equipment ID<{0}> Function Name<PanelReadReport> LotModel Find Error", msg.BODY.eqp_id.Trim());
                            BaseComm.LogMsg(Log.LogLevel.Error, errMsg);
                            #region [投板机发现混批，通知投板机停止投板]
                            var LoadEm = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByType(eEquipmentType.L);
                            if (LoadEm != null)
                            {
                                if (LoadEm.isCheckPanelID)
                                {
                                    new HostService.HostService().RemoteControlCommand(LoadEm.EQID, "", eRemoteCommand.Stop.GetEnumDescription());
                                }
                                
                            }
                            #endregion
                            return;
                            #endregion
                        }
                        #region [判断读取工单和前一工单是否相同，如果不同，则通知后面设备进行切换生产任务]
                        var ChangeEq = EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.EQNo > em.EQNo).ToList();
                        if (em.isCCDChangeData)
                        {
                            //如果没有工单号（第一次读取），工单号赋值
                            if (string.IsNullOrEmpty(em.OldWorkOrder))
                            {
                                em.OldWorkOrder = lm.WorkOrder;
                                //foreach (var vem in ChangeEq)
                                //{
                                //    //new HostService.HostService().JobDataModifyCommand(vem.EQName, lm.LotID, "", "", eModifyType.Change);
                                //    new HostService.HostService().JobDataModifyCommand(vem.EQName, lm.LotID, lm.PanelTotalQty.ToString(), 
                                //        (lm.PanelTotalQty-EAPEnvironment.commonLibrary.ScrapPanelCount).ToString(), eModifyType.Update);
                                //}
                            }
                            //如果前一工单和后一工单不一致，工单号赋值并通知后面设备更新任务数量
                            if (em.OldWorkOrder != lm.WorkOrder)
                            {
                                em.OldWorkOrder = lm.WorkOrder;
                                foreach (var vem in ChangeEq)
                                {
                                    //new HostService.HostService().JobDataModifyCommand(vem.EQName, lm.LotID, "", "", eModifyType.Change);
                                    new HostService.HostService().JobDataModifyCommand(vem.EQName, lm.LotID, lm.PanelTotalQty.ToString(),
                                        (lm.PanelTotalQty - EAPEnvironment.commonLibrary.ScrapPanelCount).ToString(), eModifyType.Update);
                                }
                            }
                            string cimMsg = $"Update JobCount Success.";
                            new HostService.HostService().CIMMessageCommand(msg.BODY.eqp_id.Trim(),"10", cimMsg, DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                        }
                        #endregion
                    }
                }
                #endregion

                #region 判断是否存入Queue isQPanelList
                if (EAPEnvironment.commonLibrary.lineModel.isQPanelList)
                {
                    if (!string.IsNullOrEmpty(msg.BODY.panel_id.Trim()))
                    {
                        EAPEnvironment.commonLibrary.qPanelIDList.Enqueue(msg.BODY.panel_id.Trim());
                    }
                }
                #endregion

                #region 判断是否执行下载任务
                if (em.isCCDDownloadData)
                {
                    //PortModel pm = em.GetPortModelByPortID(lm.LocalPortStation);
                    //if (pm == null)
                    //{
                    //    //error log
                    //    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<PanelReadReport> PortModel Find Error", msg.BODY.eqp_id.Trim()));
                    //    return;
                    //}
                    //string[] strSplit = em.CCDDownloadDataEQ.Split(',');
                    //foreach (var str in strSplit)
                    //{
                    //    emo = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(str);
                    //    if (emo == null)
                    //    {
                    //        //error log
                    //        BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<PanelReadReport> Find Error", str));
                    //        return;
                    //    }
                    //    List<MessageModel.Param> parameterModel = emo.NewEQParameter= Environment.EAPEnvironment.commonLibrary.commonModel.GetParameterModel(emo,lm); 
                    //    if (emo.EQStatus != eEQSts.Down)
                    //    {
                    //        new HostService.HostService().JobDataDownload(emo, lm, lm.LocalPortStation,parameterModel);
                    //    }
                    //    else
                    //    {
                    //        string errorMsg = string.Format("设备<{0}>机况<{1}>异常，拒绝生产.", emo.EQID, emo.EQStatus);
                    //        BaseComm.LogMsg(Log.LogLevel.Error, errorMsg);
                    //        return;
                    //    }

                    //}
                }
                #endregion
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", "PanelReadReport", ex.Message.ToString(), ex.StackTrace.ToString()));
            }
            finally
            {

            }
        }
        /// <summary>
        /// 层别不一致，进行错误信息记录。并给投板机下载远程控制命令停机
        /// </summary>
        /// <param name="lm"></param>
        /// <param name="msg"></param>
        public void SendRemoteCommand(LotModel lm, SocketMessageStructure.PanelReadReport msg)
        {
            try
            {
                string NowLayer = string.Empty;
                //用Panel ID获取当前读取的是哪一层别
                NowLayer = (from q in lm.InnerLotList from w in q.ListPanel where w.PanelID == msg.BODY.panel_id.Trim() select q.InnerLayer).FirstOrDefault();
                //用设备类型获取投板机信息
                var LoadEm = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByType(eEquipmentType.L);
                if (LoadEm != null)
                {
                    if (LoadEm.isCheckPanelID)
                    {
                        new HostService.HostService().RemoteControlCommand(LoadEm.EQID, "", eRemoteCommand.Stop.GetEnumDescription());
                    }
                    
                }
                string errMsg = string.Format("Lot顺序与芯板叠构顺序不一致，前一层别<{0}>，现层别<{1}>.", EAPEnvironment.commonLibrary.FrontLayer, NowLayer);
                BaseComm.LogMsg(Log.LogLevel.Error, errMsg);
                //初始化状态
                EAPEnvironment.commonLibrary.CCDGetSubEqList = string.Empty;
                EAPEnvironment.commonLibrary.FrontLayer = string.Empty;
                EAPEnvironment.commonLibrary.isFirstSubEq = true;
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", "PanelReadReport", ex.Message.ToString(), ex.StackTrace.ToString()));
            }
        }
       
    }
}
