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
using System.Linq;

namespace iJedha.Automation.EAP.EQPService
{
    public partial class EquipmentJobDataProcessReport : BaseComm
    {
        public void EventHandle(string evtXml)
        {
            try
            {
                #region Decode Message
                SocketMessageStructure.EquipmentJobDataProcessReport msg = new SocketMessageStructure.EquipmentJobDataProcessReport();
                if (new Serialize<SocketMessageStructure.EquipmentJobDataProcessReport>().DeSerializeXML(evtXml, out msg) == false)
                {
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Socket Message<{0}> Decode Error, Content<{1}>", "EquipmentJobDataProcessReport", evtXml));
                    return;
                }
                #endregion

                #region Record Log
                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("<{2}>Socket Message<{0}> Recv OK , Content<{1}>",
                "EquipmentJobDataProcessReport", msg.WriteToXml(), msg.BODY.eqp_id.Trim()));
                #endregion

                #region EAP收到设备上报信息后进行记录处理
                //根据设备ID获取设备信息
                EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(msg.BODY.eqp_id.Trim());

                if (em == null)
                {
                    //error log
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<EquipmentJobDataProcessReport>  Find Error", msg.BODY.eqp_id.Trim()));
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
                HostService.HostService.EquipmentJobDataProcessReportReply(msg.HEADER.TRANSACTIONID, msg.BODY.eqp_id.Trim());
                #endregion
                switch (msg.BODY.process_code)
                {
                    case "1"://创建生产任务
                        CreateJob(em, msg);
                        break;
                    case "2"://更新生产任务
                        UpdateJob(em, msg);
                        break;
                    case "3"://删除生产任务
                        DeleteJob(em, msg);
                        break;
                    case "4"://开始生产任务
                        StartJob(em, msg);
                        break;
                    case "5"://完成生产任务
                        CompleteJob(em, msg);
                        break;
                    default:
                        em.ProcessStatus = eProcessCode.UNKNOW;
                        em.JobDataDownloadChangeResult = eCheckResult.other;
                        BaseComm.LogMsg(Log.LogLevel.Info, string.Format("Equipment ID<{0}> 生产任务<UNKNOW>", msg.BODY.eqp_id.Trim()));
                        break;
                }
                #endregion
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", "EquipmentJobDataProcessReport", ex.Message.ToString(), ex.StackTrace.ToString()));
            }
            finally
            {

            }
        }
        /// <summary>
        /// 创建生产任务记录
        /// </summary>
        /// <param name="em"></param>
        /// <param name="msg"></param>
        public void CreateJob(EquipmentModel em, SocketMessageStructure.EquipmentJobDataProcessReport msg)
        {
            try
            {
                //更新状态，记录设备成功接收任务信息
                em.ProcessStatus = eProcessCode.Create;
                em.JobDataDownloadChangeResult = eCheckResult.ok;
                BaseComm.LogMsg(Log.LogLevel.Info, string.Format("Equipment ID<{0}> 创建任务<{1}>OK", msg.BODY.eqp_id.Trim(), msg.BODY.job_id));
                em.LastLotID = msg.BODY.job_id.Trim();
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", "EquipmentJobDataProcessReport_Create", ex.Message.ToString(), ex.StackTrace.ToString()));
            }
        }
        /// <summary>
        /// 更新生产任务记录
        /// </summary>
        /// <param name="em"></param>
        /// <param name="msg"></param>
        public void UpdateJob(EquipmentModel em, SocketMessageStructure.EquipmentJobDataProcessReport msg)
        {
            try
            {
                em.ProcessStatus = eProcessCode.Update;
                BaseComm.LogMsg(Log.LogLevel.Info, string.Format("Equipment ID<{0}> 更新任务<{1}>OK", msg.BODY.eqp_id.Trim(), msg.BODY.job_id));
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", "EquipmentJobDataProcessReport_Update", ex.Message.ToString(), ex.StackTrace.ToString()));
            }
        }
        /// <summary>
        /// 删除生产任务记录
        /// </summary>
        /// <param name="em"></param>
        /// <param name="msg"></param>
        public void DeleteJob(EquipmentModel em, SocketMessageStructure.EquipmentJobDataProcessReport msg)
        {
            try
            {
                em.ProcessStatus = eProcessCode.Delete;
                BaseComm.LogMsg(Log.LogLevel.Info, string.Format("Equipment ID<{0}> 删除任务<{1}>OK", msg.BODY.eqp_id.Trim(), msg.BODY.job_id));
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", "EquipmentJobDataProcessReport_Delete", ex.Message.ToString(), ex.StackTrace.ToString()));
            }
        }
        /// <summary>
        /// 开始生产任务记录
        /// </summary>
        /// <param name="em"></param>
        /// <param name="msg"></param>
        public void StartJob(EquipmentModel em, SocketMessageStructure.EquipmentJobDataProcessReport msg)
        {
            try
            {
                em.ProcessStatus = eProcessCode.Start;
                em.CurrentLotID = msg.BODY.job_id.Trim();
                if (em.isCurrentPN)
                {
                    //分板线四合一只有料号，job_id是料号
                    em.CurrentPN = msg.BODY.job_id;
                }
                em.OldEQParameter.Clear();
                foreach (var item in em.NewEQParameter)
                {
                    em.OldEQParameter.Add(item);
                }
                //如果是开料机，增加开始生产Lot信息
                if (em.EqNameKey == ConstLibrary.CONST_ACL)
                {
                    //在所有Lot里面找出符合条件的Lot信息
                    Lot lm = Environment.EAPEnvironment.commonLibrary.commonModel.GetLotModelBySubLotID(msg.BODY.job_id.Trim());
                    if (lm == null)
                    {
                        //error log
                        BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<EquipmentJobDataProcessReport_StartJob> SubLotModel Find Error.Job ID<{1}>", msg.BODY.eqp_id.Trim(), msg.BODY.job_id.Trim()));
                        return;
                    }
                    //更改Lot状态
                    lm.LotProcessStatus = eLotProcessStatus.Run;
                    //把当前Lot加入到开始生产Lot里面
                    EAP.Environment.EAPEnvironment.commonLibrary.commonModel.AddStartProcessLotModelKL((Lot)lm.Clone());
                }

                if (em.EqVendor.Equals("广合设备") && em.Type == eEquipmentType.U)
                {
                    if (EAPEnvironment.commonLibrary.lineModel.LineType == "开料线")
                    {
                        Lot subLot = EAPEnvironment.commonLibrary.commonModel.GetLotModelBySubLotID(msg.BODY.job_id.Trim());
                        if (subLot == null)
                        {
                            //error log
                            BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<EquipmentJobDataProcessReport_StartJob> SubLotModel Find Error.Job ID<{1}>", msg.BODY.eqp_id.Trim(), msg.BODY.job_id.Trim()));
                            return;
                        }
                        var portL02 = em.GetPortModelByPortID(ePortID.U02.ToString());
                        if (portL02 != null)
                        {
                            portL02.List_SubLot.Clear();
                            portL02.List_SubLot.Add(subLot.LotID, subLot);
                        }
                    }
                    else
                    {
                        //记录广合设备收板机下料口批次信息
                        LotModel lotModel = EAPEnvironment.commonLibrary.commonModel.GetLotModelByLotID(msg.BODY.job_id.Trim());
                        if (lotModel == null)
                        {
                            //error log
                            BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<EquipmentJobDataProcessReport_StartJob> LotModel Find Error.Job ID<{1}>", msg.BODY.eqp_id.Trim(), msg.BODY.job_id.Trim()));
                            return;
                        }
                        var portL02 = em.GetPortModelByPortID(ePortID.U02.ToString());
                        if (portL02 != null)
                        {
                            portL02.List_Lot.Clear();
                            portL02.List_Lot.Add(lotModel.LotID, lotModel);
                        }
                    }
                }
                BaseComm.LogMsg(Log.LogLevel.Info, string.Format("Equipment ID<{0}> 开始生产任务<{1}>", msg.BODY.eqp_id.Trim(), msg.BODY.job_id));
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", "EquipmentJobDataProcessReport_Start", ex.Message.ToString(), ex.StackTrace.ToString()));
            }
        }
        /// <summary>
        /// 完成生产任务记录
        /// </summary>
        /// <param name="em"></param>
        /// <param name="msg"></param>
        public void CompleteJob(EquipmentModel em, SocketMessageStructure.EquipmentJobDataProcessReport msg)
        {
            try
            {
                em.CurrentCompleteLotID = msg.BODY.job_id;
                em.ProcessStatus = eProcessCode.Complete;
                string err = "";
                #region 回流线Track Out逻辑*****-----
                if (em.EQName == "回流线")
                {
                    LotModel lm = Environment.EAPEnvironment.commonLibrary.commonModel.GetLotModelByBatchID(msg.BODY.job_id.Trim());
                    if (lm == null)
                    {
                        //error log
                        BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<EquipmentJobDataProcessReport_CompleteJob1> LotModel Find Error.Batch ID<{1}>", msg.BODY.eqp_id.Trim(), msg.BODY.job_id.Trim()));
                        return;
                    }
                    new WebAPIReport().EAP_LotTrackOut(new MessageModel.LotTrackOut()
                    {
                        MainEqpID = EAPEnvironment.commonLibrary.MDLN,
                        SubEqpID = em.EQID,
                        PortID = "",
                        LotID = em.CurrentLotID,
                        PanelTotalQty = msg.BODY.process_panel_count,
                        NGFlag = false,//先记录false??
                        PanelList = new List<MessageModel.Panel>(),//回流线不需要给Panel List
                        WIPDataList = new List<MessageModel.WipData>(),//?
                        JobID = msg.BODY.job_id.Trim(),
                        JobTotalQty = msg.BODY.process_panel_count,
                        PN = lm.PN,
                        WorkOrder = lm.WorkOrder
                    }, lm, em, 1, out err);
                }
                #endregion
                else
                {
                    #region 清线完成信号
                    if (em.Type == eEquipmentType.U)//收板机清线逻辑
                    {
                        if (EAPEnvironment.commonLibrary.lineModel.LineType == "开料线")
                        {
                            Lot startsubLot = EAPEnvironment.commonLibrary.commonModel.GetStartLotModelBySubLotID(msg.BODY.job_id.Trim());
                            if (startsubLot == null)
                            {
                                //error log
                                BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<EquipmentJobDataProcessReport_CompleteJob2> LotModel Find Error.Job ID<{1}>", msg.BODY.eqp_id.Trim(), msg.BODY.job_id.Trim()));
                                return;
                            }
                            Lot subLot = EAPEnvironment.commonLibrary.commonModel.GetLotModelBySubLotID(msg.BODY.job_id.Trim());
                            if (subLot == null)
                            {
                                //error log
                                BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<EquipmentJobDataProcessReport_CompleteJob3> LotModel Find Error.Job ID<{1}>", msg.BODY.eqp_id.Trim(), msg.BODY.job_id.Trim()));
                                return;
                            }
                            //记录批次状态
                            subLot.LotProcessStatus = eLotProcessStatus.Complete;
                            startsubLot.LotProcessStatus = eLotProcessStatus.Complete;
                            //获取subLot是否全部生产完成
                            var v = Environment.EAPEnvironment.commonLibrary.commonModel.List_StartProcessLotKL.Where(r => r.Value.LotProcessStatus != eLotProcessStatus.Complete).ToList();
                            //如果全部生产完成，数量为0
                            if (v.Count == 0)
                            {
                                Environment.EAPEnvironment.commonLibrary.isAllProcessOK = true;
                                //用subLot获取主Lot信息
                                LotModel lotModel = EAPEnvironment.commonLibrary.commonModel.GetLotModelBySubLot(startsubLot);
                                if (lotModel != null)
                                {//主Lot状态变成完成
                                    lotModel.LotProcessStatus = eLotProcessStatus.Complete;
                                }
                            }
                            else Environment.EAPEnvironment.commonLibrary.isAllProcessOK = false;

                        }
                        else
                        {
                            #region 如果是使用内层Lot，完工信号要达到完工次数，才算清线完成
                            if (EAPEnvironment.commonLibrary.lineModel.isInnerLotPanelList)
                            {
                                EAPEnvironment.commonLibrary.ProcessCompleteReportCount++;
                                if (EAPEnvironment.commonLibrary.ProcessCompleteReportCount == EAPEnvironment.commonLibrary.UnloadEquipmentProcessCompleteReportCount)
                                {
                                    Environment.EAPEnvironment.commonLibrary.isAllProcessOK = true;
                                    EAPEnvironment.commonLibrary.ProcessCompleteReportCount = 0;
                                }
                                else Environment.EAPEnvironment.commonLibrary.isAllProcessOK = false;
                            }
                            #endregion
                            else
                            {
                                LotModel lm = Environment.EAPEnvironment.commonLibrary.commonModel.GetLotModelByLotID(msg.BODY.job_id.Trim());
                                lm.LotProcessStatus = eLotProcessStatus.Complete;

                                var v = Environment.EAPEnvironment.commonLibrary.commonModel.List_ProcessLot.Where(r => (r.Value.LotProcessStatus != eLotProcessStatus.Complete) && (r.Value.LotProcessStatus != eLotProcessStatus.Create)).ToList();
                                if (v.Count == 0) Environment.EAPEnvironment.commonLibrary.isAllProcessOK = true;
                                else Environment.EAPEnvironment.commonLibrary.isAllProcessOK = false;
                            }
                        }
                    }
                    #endregion

                    #region [多投板机清线逻辑-目前融铆合使用]
                    if (EAPEnvironment.commonLibrary.lineModel.isMultiLoadEquipment)
                    {
                        LotModel lm = Environment.EAPEnvironment.commonLibrary.commonModel.GetLotModelByLotID(msg.BODY.job_id.Trim());
                        lm.LotProcessStatus = eLotProcessStatus.Complete;

                        var v = Environment.EAPEnvironment.commonLibrary.commonModel.List_ProcessLot.Where(r => r.Value.LotProcessStatus != eLotProcessStatus.Complete).ToList();
                        if (v.Count == 0) Environment.EAPEnvironment.commonLibrary.isAllProcessOK = true;
                        else Environment.EAPEnvironment.commonLibrary.isAllProcessOK = false;
                    }
                    #endregion
                }
                if (em.Type == eEquipmentType.L)
                {
                    #region 开料机逻辑，开料机完工后，上报完工信号。此时如果EAP存在未做的Lot，继续下载生产任务给开料线*****-----
                    if (em.EqNameKey == ConstLibrary.CONST_ACL)
                    {
                        Lot lm = Environment.EAPEnvironment.commonLibrary.commonModel.GetStartLotModelBySubLotID(msg.BODY.job_id.Trim());
                        PortModel pm;
                        if (lm == null)
                        {
                            //error log
                            BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<EquipmentJobDataProcessReport_CompleteJob4> LotModel Find Error.Job ID<{1}>", msg.BODY.eqp_id.Trim(), msg.BODY.job_id.Trim()));
                            return;
                        }
                        RemoveLoadEquipmentJobData_KL(em, lm, out pm);

                        var ProcessLot = Environment.EAPEnvironment.commonLibrary.commonModel.List_ProcessLotKL.Where(r => r.Value.LotProcessStatus == eLotProcessStatus.Create).ToList();

                        if (ProcessLot.Count != 0)
                        {

                            #region 正在检查生产条件
                            Dictionary<MessageModel.LotInfoKL, CommModel> Dic_Message = new Dictionary<MessageModel.LotInfoKL, CommModel>();
                            CommModel cm = new CommModel();
                            cm.lm = new LotModel();
                            cm.pm = pm;
                            cm.lot = ProcessLot.ElementAt(0).Value;
                            cm.em = em;
                            EAPEnvironment.commonLibrary.isJobDataProcessDataTigger = true;
                            Dic_Message.Add(new MessageModel.LotInfoKL(), cm);
                            if (!EAPEnvironment.commonLibrary.commonModel.InCheckNextLotFlagStart)
                            {
                                if (EAPEnvironment.EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_CheckNextLotFlag, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_CheckNextLotFlag, 10000, true, Dic_Message))
                                {
                                    LogMsg(Log.LogLevel.Info, string.Format("Lot<{0}>检查生产条件......", ProcessLot.ElementAt(0).Value.LotID));
                                    EAPEnvironment.commonLibrary.commonModel.InCheckNextLotFlagStart = true;
                                    EAPEnvironment.commonLibrary.commonModel.InCheckNextLotFlagTime = DateTime.Now;
                                }
                            }
                            #endregion
                        }
                    }
                    #endregion
                    else
                    {
                        LotModel lm = Environment.EAPEnvironment.commonLibrary.commonModel.GetLotModelByLotID(msg.BODY.job_id.Trim());
                        PortModel pm;
                        if (lm == null)
                        {
                            //error log
                            BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<EquipmentJobDataProcessReport_CompleteJob4> LotModel Find Error.Job ID<{1}>", msg.BODY.eqp_id.Trim(), msg.BODY.job_id.Trim()));
                            return;
                        }
                        RemoveLoadEquipmentJobData(em, lm, out pm);

                        string errMsg;
                        //开料机上报完工信号后，需要向MES再次请求生产任务，达到连续生产
                        if (em.EQName == ConstLibrary.CONST_ACL)
                        {
                            new WebAPIReport().EAP_LotInfoRequest(new MessageModel.LotInfoRequest
                            {
                                MainEqpID = EAPEnvironment.commonLibrary.MDLN,
                                SubEqpID = em.EQID,
                                PortID = "L01",
                                CarrierID = ""
                            }, em, 1, out errMsg);

                            if (!string.IsNullOrEmpty(errMsg))
                            {
                                new HostService.HostService().CIMMessageCommand(em.EQID, "10", $"E4005:{errMsg}", DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                            }
                        }
                    }
                }

                BaseComm.LogMsg(Log.LogLevel.Info, string.Format("Equipment ID<{0}> 完成生产任务<{1}>", msg.BODY.eqp_id.Trim(), msg.BODY.job_id));

                EAPEnvironment.commonLibrary.CurrentLotCount = msg.BODY.process_panel_count;
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", "EquipmentJobDataProcessReport_Start", ex.Message.ToString(), ex.StackTrace.ToString()));
            }
        }
        /// <summary>
        /// 移除开料机上料位端口任务，避免堆积
        /// </summary>
        /// <param name="em">设备</param>
        /// <param name="lm">Lot</param>
        /// <param name="pm">PortModel</param>
        /// <returns></returns>
        public List<Lot> RemoveLoadEquipmentJobData_KL(EquipmentModel em, Lot lm, out PortModel pm)
        {
            pm = new PortModel();
            List<Lot> lstLot = new List<Lot>();
            try
            {
                var port = EAPEnvironment.commonLibrary.commonModel.GetLotModelBySubLotID(lm.LotID);
                if (port != null)
                {
                    pm = em.GetPortModelByPortID(port.PortID);
                }
                foreach (var item in pm.List_Lot)
                {
                    item.Value.LotList.RemoveAll(r => r.LotID == lm.LotID);
                    lstLot = item.Value.LotList;
                }
                return lstLot;
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", "RemoveLoadEquipment", ex.Message.ToString(), ex.StackTrace.ToString()));
                return lstLot;
            }
        }
        /// <summary>
        /// 移除投板机暂存位端口任务，避免堆积
        /// </summary>
        /// <param name="em"></param>
        /// <param name="lm"></param>
        /// <param name="pm"></param>
        public void RemoveLoadEquipmentJobData(EquipmentModel em, LotModel lm, out PortModel pm)
        {
            pm = new PortModel();
            try
            {
                pm = em.GetPortModelByPortID(lm.PortID);
                //pm.DeepInitial_UDRQ(lm.LotID);
                pm.RemovePortLotInfo(lm);
                //记录本次删除了哪些批次
                foreach (var item in pm.LstLotID)
                {
                    BaseComm.LogMsg(Log.LogLevel.Info, $"删除批次ID信息:{item}.");
                }
                pm.LstLotID.Clear();
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", "RemoveLoadEquipment", ex.Message.ToString(), ex.StackTrace.ToString()));
            }
        }
    }
}
