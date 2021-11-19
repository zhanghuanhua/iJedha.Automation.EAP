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
        /// EAP收到投板机读取的载具后,询问MES在制品信息
        /// </summary>
        /// <param name="_indata"></param>
        /// <param name="em"></param>
        /// <param name="Retrytime"></param>
        /// <param name="guiErrMsg"></param>
        public void EAP_LotInfoRequest(MessageModel.LotInfoRequest _indata, EquipmentModel em, int Retrytime,out string guiErrMsg)
        {
            guiErrMsg = "";
            ErrorCodeModelBase errm = new ErrorCodeModelBase();
            try
            {
                //if (EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckStart)
                //{
                //    EAPEnvironment.EAPAp.DeleteThreadWork(ConstLibrary.CONST_DLL_RULE_ProductionConditionCheck);
                //}
                if (EAPEnvironment.commonLibrary.HostConnectMode == EAP.LibraryBase.eHostConnectMode.DISCONNECT)
                {
                    guiErrMsg = $"当前连线状态为{EAP.LibraryBase.eHostConnectMode.DISCONNECT}";
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
                    guiErrMsg = "WebAPI服务器设定关闭，停止消息发送";
                    LogMsg(Log.LogLevel.Warn, guiErrMsg);
                    
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
                    string errMsg = string.Format("E3002:MES回复, 接口名称[{0}], MES错误代码[{1}], MES错误描述[{2}][{3}]", System.Reflection.MethodBase.GetCurrentMethod().Name, returnInfo.strCode, _indata.SubEqpID, returnInfo.strMsg);
                    EAP.Environment.BaseComm.ErrorHandleRule("E3002", errMsg, ref errm);

                    new HostService.HostService().CIMMessageCommand(em.EQID, "10", errMsg, DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                    History.EAP_EQP_EVENTHISTORY(em, eEventName.EAP_LOTINFOREQUEST, eEventFlow.MES2EAP, errMsg, em.CurrentLotID, _indata.PortID);
                    guiErrMsg = errMsg;
                    return;
                }

                string err;
                PortModel port;
                LotModel lot;
                object lotinfoobject;
                try
                {
                    new EAP.Core.Serialize().DeSerializeJSON(returnInfo.Content.ToString(), new MessageModel.LotInfo().GetType(), out lotinfoobject);
                }
                catch (Exception e)
                {
                    StackTrace st = new StackTrace(new StackFrame(true));
                    StackFrame sf = st.GetFrame(0);
                    string errMsg = string.Format("EAP发送<{0}>异常：程序出错，代码行数<{1}>，ErrMsg<{2}>",
                    System.Reflection.MethodBase.GetCurrentMethod().Name, sf.GetFileLineNumber(),
                    string.Format("设备<{0}>Lotinfo下载失败：MES传输数据错误.", em.EQID));
                    EAP.Environment.BaseComm.ErrorHandleRule("E0001", errMsg, ref errm);
                    History.EAP_EQP_EVENTHISTORY(em, eEventName.EAP_LOTINFOREQUEST, eEventFlow.MES2EAP, errMsg, em.CurrentLotID, _indata.PortID);
                    guiErrMsg = errMsg;
                    return;
                }

                MessageModel.LotInfo lotinfo = (MessageModel.LotInfo)lotinfoobject;
                //取得当前Lot ID
                EAPEnvironment.commonLibrary.commonModel.currentProcessLotID = lotinfo.LotID;

                //if (em.ControlMode == eControlMode.REMOTE)
                //{
                #region [Check LotInfo]
                if (!CheckLotInfo(em, _indata, lotinfo, out port, out err))
                {
                    //0:OK 1:NG
                    BaseComm.ErrorHandleRule("E2103", string.Format("Lot<{0}>批次资料下载检查异常：{1}...，拒绝生产.",
                        lotinfo.LotID, err), ref errm);

                    new HostService.HostService().CIMMessageCommand(em.EQID, "10", "E2103:" + err, DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                    History.EAP_EQP_EVENTHISTORY(em, eEventName.EAP_LOTINFOREQUEST, eEventFlow.MES2EAP, err, em.CurrentLotID, _indata.PortID);
                    guiErrMsg = err;
                    return;
                }
                #endregion


                #region [handle Info]
                if (!HandleLotInfo(em, port, lotinfo, out lot, out err))
                {
                    BaseComm.ErrorHandleRule("E2103", string.Format("Lot<{0}>批次资料下载检查异常：{1}...，拒绝生产.",
                        lotinfo.LotID, err), ref errm);

                    new HostService.HostService().CIMMessageCommand(em.EQID, "10", "E2103:" + err, DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                    History.EAP_EQP_EVENTHISTORY(em, eEventName.EAP_LOTINFOREQUEST, eEventFlow.MES2EAP, err, em.CurrentLotID, _indata.PortID);
                    guiErrMsg = err;
                    return;
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
                if (!EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckStart)
                {
                    EAPEnvironment.commonLibrary.isProcessOK = false;
                }
                #endregion

                #region 初始化设备回复状态
                foreach (var item in EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
                {
                    item.JobDataDownloadChangeResult = eCheckResult.nothing;
                    item.RetryCount = 0;
                }
                #endregion
                SetJobDataAutoReply(lotinfo);

                #region  初始化所有线程Start Flag
                EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckInitial();
                EAPEnvironment.EAPAp.DeleteThreadWork(ConstLibrary.CONST_DLL_RULE_ProductionConditionCheck);
                EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckInitial();
                EAPEnvironment.EAPAp.DeleteThreadWork(ConstLibrary.CONST_DLL_RULE_JobDataDownload);
                EAPEnvironment.commonLibrary.commonModel.InEquipmentJobDataRequestCheckInitial();
                EAPEnvironment.EAPAp.DeleteThreadWork(ConstLibrary.CONST_DLL_RULE_EquipmentJobDataRequest);
                EAPEnvironment.commonLibrary.commonModel.InAllProcessCompletionCheckInitial();
                EAPEnvironment.EAPAp.DeleteThreadWork(ConstLibrary.CONST_DLL_RULE_AllProcessCompletionCheck);
                EAPEnvironment.commonLibrary.commonModel.InLoadCompleteReportCheckInitial();
                EAPEnvironment.EAPAp.DeleteThreadWork(ConstLibrary.CONST_DLL_RULE_LoadCompleteReportCheck);
                #endregion

                #region 计算完工信号次数(冲孔连棕化使用)
                Environment.EAPEnvironment.commonLibrary.UnloadEquipmentProcessCompleteReportCount = 0;
                if (Environment.EAPEnvironment.commonLibrary.lineModel.isInnerLotPanelList)
                {
                    int TotalCount = 0;
                    foreach (var item in lotinfo.InnerLotID)
                    {
                        if (string.IsNullOrEmpty(item.LoadQty))
                        {
                            LogMsg(Log.LogLevel.Warn, $"LoadQty值为[{item.LoadQty}]");
                        }
                        TotalCount += int.Parse(item.LoadQty == "" ? "0" : item.LoadQty);
                    }
                    do
                    {
                        Environment.EAPEnvironment.commonLibrary.UnloadEquipmentProcessCompleteReportCount++;
                        if (string.IsNullOrEmpty(lotinfo.UnloadQty))
                        {
                            LogMsg(Log.LogLevel.Warn, $"UnloadQty值为[{lotinfo.UnloadQty}]");
                        }
                        TotalCount = TotalCount - int.Parse(lotinfo.UnloadQty == "" ? "0" : lotinfo.UnloadQty);
                    } while (TotalCount > 0);
                }
                #endregion

                #region  手动下载开关
                if (Environment.EAPEnvironment.commonLibrary.lineModel.isManualJobDataDoanload)
                {
                    //手动时，需记录PortID和LotID，供TrackIn使用
                    EAPEnvironment.commonLibrary.TupTrackIn = new Tuple<string, string>(lot.PortID, lot.LotID);
                    return;
                }
                #endregion

                if (Environment.EAPEnvironment.commonLibrary.lineModel.isLoadCompleteCountCheck)//Load Complete次数检查。目前冲孔连棕化，PP裁切机使用
                {
                    #region 正在检查Load Complete情况
                    Dictionary<MessageModel.LotInfo, CommModel> Dic_Message = new Dictionary<MessageModel.LotInfo, CommModel>();
                    CommModel cm = new CommModel();
                    cm.lm = lot;
                    cm.pm = port;
                    cm.em = em;

                    Dic_Message.Add(lotinfo, cm);
                    if (!EAPEnvironment.commonLibrary.commonModel.InLoadCompleteReportCheckStart)
                    {
                        if (EAPEnvironment.EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_LoadCompleteReportCheck, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_LoadCompleteReportCheck, 4000, true, Dic_Message))
                        {
                            LogMsg(Log.LogLevel.Info, string.Format("线体<{0}>正在检查投板机上报Load Complete情况......", lotinfo.MainEqpID));
                            EAPEnvironment.commonLibrary.commonModel.InLoadCompleteReportCheckStart = true;
                            EAPEnvironment.commonLibrary.commonModel.InLoadCompleteReportCheckTime = DateTime.Now;
                        }
                    }
                    #endregion
                }
                else
                {
                    #region 正在检查生产条件
                    Dictionary<MessageModel.LotInfo, CommModel> Dic_Message = new Dictionary<MessageModel.LotInfo, CommModel>();
                    CommModel cm = new CommModel();
                    cm.lm = lot;
                    cm.pm = port;
                    cm.em = em;

                    Dic_Message.Add(lotinfo, cm);
                    if (!EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckStart)
                    {
                        if (EAPEnvironment.EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_ProductionConditionCheck, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_ProductionConditionCheck, 4000, true, Dic_Message))
                        {
                            LogMsg(Log.LogLevel.Info, string.Format("线体<{0}>正在检查生产条件......", lotinfo.MainEqpID));
                            EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckStart = true;
                            EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckTime = DateTime.Now;
                        }
                    }
                    #endregion
                }

                //显示Lot信息
                if (lot.FirstInspFlag)
                {
                    EAPEnvironment.commonLibrary.MainLotID = lot.FirstInspectLot;
                }
                else
                {
                    EAPEnvironment.commonLibrary.MainLotID = lot.LotID;
                }
                EAPEnvironment.commonLibrary.ShowLotInfoMessage = lot.LotStatus = eLotinfo.WaitingUp.ToString();
                #endregion

                bool isCheckNg = false;
                //}
                //else
                //{
                //    LogMsg(Log.LogLevel.Error, $"Equipment ID<{em.EQID}>,连线状态<{em.ConnectMode}>,控制模式<{em.ControlMode}>.");
                //}
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
                guiErrMsg = errMsg;
                #endregion
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
        public bool CheckLotInfo(EquipmentModel em, MessageModel.LotInfoRequest _data, MessageModel.LotInfo lotinfo, out PortModel port, out string err)
        {
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

                #region [比对LotID]
                if (lotinfo.LotID == string.Empty)
                {
                    err = string.Format("异常项目：[{0}]，异常值：[{1}]", "LotID", lotinfo.LotID);
                    return false;
                }
                #endregion

                #region [比对载具ID]
                //if (_data.CarrierID != lotinfo.CarrierID)
                //{
                //    err = string.Format("异常项目：<{0}>，异常值：<{1}>", "CarrierID", lotinfo.CarrierID);
                //    return false;
                //}
                #endregion

                #region [比对Panel Count]
                if (int.Parse(lotinfo.PanelTotalQty == "" ? "0" : lotinfo.PanelTotalQty) == 0)
                {
                    err = string.Format("异常项目：[{0}]，异常值：[{1}]", "PanelTotalQty", lotinfo.PanelTotalQty);
                    return false;
                }
                //if (int.Parse(lotinfo.JobTotalQty) == 0)
                //{
                //    err = string.Format("异常项目：<{0}>，异常值：<{1}>", "ProcessQty", lotinfo.JobTotalQty);
                //    return false;
                //}
                #endregion

                err = string.Empty;
                string msg = string.Format("MES下载生产任务Lot[{0}]接收检查完成.", lotinfo.LotID);
                LogMsg(Log.LogLevel.Info, msg);
                return true;
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                err = string.Format("Lot[{0}]资料检查失败：程式出错.", lotinfo.LotID);
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
        /// <param name="lotinfo"></param>
        /// <param name="lot"></param>
        /// <param name="err"></param>
        /// <returns></returns>
        public bool HandleLotInfo(EquipmentModel em, PortModel port, MessageModel.LotInfo lotinfo, out LotModel lot, out string err)
        {
            lot = new LotModel();
            err = string.Empty;
            port.DeepInitial_UDRQ(lotinfo.LotID);
            try
            {
                lot.PortID = lotinfo.PortID;
                lot.LotID = lotinfo.LotID;
                lot.LotStatus = lotinfo.LotStatus;
                lot.LoadQty = lotinfo.LoadQty;
                lot.UnloadQty = lotinfo.UnloadQty;
                lot.LotStartQty = lotinfo.LotStartQty;
                lot.LotMatchQty = lotinfo.LotMatchQty;
                lot.PnlStartSN = lotinfo.PnlStartSN;
                lot.PN = lotinfo.PN;
                lot.ProductRev = lotinfo.ProductRev;
                if (string.IsNullOrEmpty(lotinfo.PanelTotalQty))
                {
                    LogMsg(Log.LogLevel.Warn, $"lotinfo.PanelTotalQty值为[{lotinfo.PanelTotalQty}]");
                }
                lot.PanelTotalQty = int.Parse(lotinfo.PanelTotalQty == "" ? "0" : lotinfo.PanelTotalQty);

                lot.IsDummyLot = lotinfo.IsDummyLot == "1" ? eLotType.Dummy : eLotType.Normal;
                lot.Layer = lotinfo.Layer;
                lot.DummyType = lotinfo.DummyType;
                lot.FrontDummyQty = lotinfo.FrontDummyQty;
                lot.AfterDummyQty = lotinfo.AfterDummyQty;
                lot.PNLength = lotinfo.PNLength;
                lot.PNWidth = lotinfo.PNWidth;
                lot.PNThick = lotinfo.PNThick;
                lot.IsRotate = lotinfo.IsRotate;
                lot.IsTurnoverGroup = lotinfo.IsTurnoverGroup;
                lot.IsSkipPunchingGroup = lotinfo.IsSkipPunchingGroup;
                lot.SkipSubEqpID = lotinfo.SkipSubEqpID;
                lot.JobID = lotinfo.JobID;
                lot.LocalEQStation = em.EQName;
                lot.LocalPortStation = port.PortID.ToString();
                lot.ProcessTime = DateTime.Now;
                lot.MaterialPortTaskQty = lotinfo.MaterialPortTaskQty;
                lot.Direction = lotinfo.Direction;
                lot.IsConnectionStatus = lotinfo.IsConnectionStatus;

                lot.WorkOrder = lotinfo.WorkOrder;
                lot.WorkOrderPNLQty = lotinfo.WorkOrderPNLQty;

                if (string.IsNullOrEmpty(lotinfo.InnerLotTotalQty))
                {
                    LogMsg(Log.LogLevel.Warn, $"lotinfo.InnerLotTotalQty值为[{lotinfo.InnerLotTotalQty}]");
                }
                int InnerQty;
                int.TryParse(lotinfo.InnerLotTotalQty, out InnerQty);
                lot.InnerLotTotalQty = InnerQty;

                //lot.MatchDummyQty = lotinfo.MatchDummyQty;
                lot.DataSource = eDataSource.Auto;
                port.LotFlag = false;

                //ot.RunType = (eRunType)int.Parse(lotinfo.RunType);
                if (lotinfo.FirstInspect == null)
                {
                    lotinfo.FirstInspect = new List<MessageModel.FirstInspect>();
                    lot.FirstInspFlag = false;

                }
                foreach (var v in lotinfo.FirstInspect)
                {
                    lot.FirstInspectLot = v.FirstInspectLot;
                    lot.FirstInspQty = v.FirstInspectLotQty;
                    lot.FirstInspFlag = true;
                }


                //取得InnerLotList
                if (lotinfo.InnerLotID == null)
                {
                    lotinfo.InnerLotID = new List<MessageModel.InnerLot>();
                }
                //EAPEnvironment.commonLibrary.MESInnerLotSubEqInfo = string.Empty;
                //EAPEnvironment.commonLibrary.MESInnerLotSubEqList = new List<string>();
                //EAPEnvironment.commonLibrary.PunchingList = new List<string>();
                EAPEnvironment.commonLibrary.LayerLevel = new List<string>();
                lot.MESInnerLotSubEqInfo = "";
                lot.PunchingList = new List<string>();
                lot.LayerLevel = new List<string>();
                lot.MESInnerLotSubEqList = new List<string>();
                foreach (var v in lotinfo.InnerLotID)
                {
                    InnerLotModel innerLot = new InnerLotModel();
                    innerLot.InnerLotID = v.InnerLotID;
                    innerLot.InnerLayer = v.InnerLayer;
                    innerLot.LoadQty = v.LoadQty;
                    innerLot.IsTurnover = v.IsTurnover;
                    innerLot.IsSkipPunching = v.IsSkipPunching;
                    innerLot.SubEqpID = v.SubEqpID;
                    innerLot.MaterialSeq = v.MaterialSeq;
                    int Seq = int.Parse(v.MaterialSeq == "" ? "0" : v.MaterialSeq);
                    if (EAPEnvironment.commonLibrary.MaterialPortIDBinding.ContainsKey(v.InnerLotID))
                    {
                        EAPEnvironment.commonLibrary.MaterialPortIDBinding.Remove(v.InnerLotID);
                    }
                    EAPEnvironment.commonLibrary.MaterialPortIDBinding.Add(v.InnerLotID, Seq);
                    //取得Panel List
                    if (v.PanelList == null)
                    {
                        v.PanelList = new List<MessageModel.Panel>();
                    }
                    int InnerLotPanelSeq = 0;
                    foreach (var p in v.PanelList)
                    {
                        PanelModel panel = new PanelModel();
                        if (p.StripList == null)
                        {
                            p.StripList = new List<MessageModel.Strip>();
                        }
                        if (p.BatchPnlList == null)
                        {
                            p.BatchPnlList = new List<MessageModel.BatchPnl>();
                        }
                        //panel.CarrierID = lotinfo.CarrierID;
                        panel.LotID = lotinfo.LotID;
                        panel.PN = lotinfo.PN;
                        panel.PanelID = p.PanelID;
                        panel.PanelType = ePanelType.OK;
                        panel.SequenceNo = InnerLotPanelSeq + 1;
                        panel.CreateTime = string.Format("{0:yyyyMMddHHmmss}", DateTime.Now);
                        panel.StartTime = "";
                        panel.EndTime = "";
                        panel.StripIDList = p.StripList;
                        panel.BatchIDList = p.BatchPnlList;
                        panel.HolePnlID = p.HolePnlID;
                        innerLot.ListPanel.Add(panel);
                        InnerLotPanelSeq++;
                    }

                    lot.InnerLotList.Add(innerLot);
                    lot.MESInnerLotSubEqInfo += v.SubEqpID;
                    lot.PunchingList.Add(string.Format("{0}:{1}", innerLot.InnerLayer, innerLot.IsSkipPunching));// == "0" ? "1" : innerLot.IsSkipPunching == "1" ? "0" : innerLot.IsSkipPunching));
                    lot.LayerLevel.Add(innerLot.InnerLayer);
                    lot.MESInnerLotSubEqList.Add(v.SubEqpID);
                }
                List<string> intList = new List<string>();
                string subUnloadQtyGroup = string.Empty;
                if (lot.InnerLotList.Count > 0)
                {
                    string[] aUnloadQtyGroup = lotinfo.UnloadQtyGroup.Split(',');
                    foreach (var item in aUnloadQtyGroup)
                    {
                        GetSubUnloadQtyGroup(item, lot.InnerLotList.Count, out subUnloadQtyGroup);
                        intList.Add(subUnloadQtyGroup);
                    }
                    lot.UnloadQtyGroup = string.Join(",", intList);
                }


                //取得配方
                if (lotinfo.ParamList == null)
                {
                    lotinfo.ParamList = new List<MessageModel.Param>();
                }

                var EqUseParameter = GetEqUseParameter(lotinfo.ParamList);


                foreach (var v in EqUseParameter)
                {
                    EquipmentModel subem = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(v.SubEqpID);
                    if (subem == null)
                    {
                        continue;
                    }
                    // subem.NewEQParameter.Add(v);
                    SubEqp eqp = new SubEqp();
                    ParameterModel pm = new ParameterModel();
                    eqp.SubEqpID = v.SubEqpID;

                    pm.ItemName = v.ParamName;// GetExchangeParameterName(v.ParamName, v.SubEqpID);
                    pm.ItemValue = v.ParamValue;
                    pm.DataType = v.ParamType;
                    eqp.EQParameter.Add(pm);
                    lot.SubEqpList.Add(eqp);
                }
                //取得WIP DATA
                if (lotinfo.WIPDataList == null)
                {
                    lotinfo.WIPDataList = new List<MessageModel.WipData>();
                }
                List<string> lst1 = lotinfo.WIPDataList.Select(t => t.SubEqpID).Distinct().ToList();
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
                foreach (var v in lotinfo.WIPDataList)
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
                    lot.LotParameterList.Add(wipData);
                }
                #endregion
                //取得Panel List
                if (lotinfo.PanelList == null)
                {
                    lotinfo.PanelList = new List<MessageModel.Panel>();
                }
                int LotPanelSeq = 0;
                foreach (var v in lotinfo.PanelList)
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
                    lot.PanelList.Add(panel);
                    port.List_Panel.Add(panel.LotID + "_" + panel.SequenceNo, panel);
                    LotPanelSeq++;
                }
                //要处理的Panel
                lot.LotProcessStatus = eLotProcessStatus.Create;
                if (port.List_Lot.ContainsKey(lot.LotID))
                {
                    port.List_Lot.Remove(lot.LotID);
                }
                port.List_Lot.Add(lot.LotID, lot);

                string msg = string.Format("Lot<{0}>建置资料完成.", lotinfo.LotID);
                LogMsg(Log.LogLevel.Info, msg);
                EAPEnvironment.commonLibrary.UseLoadEquipmentNo = em.LoadEquipmentSequence;
                return true;
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                err = string.Format("Lot[{0}]建置资料失败，拒绝上机.", lotinfo.LotID);
                LogMsg(Log.LogLevel.Warn, err);
                return false;
            }
        }
        /// <summary>
        /// 把MES下载的不需下载内层Lot的设备设定成ok,不需要下载任务的设备设定成ok
        /// </summary>
        /// <param name="lotInfo"></param>
        public void SetJobDataAutoReply(MessageModel.LotInfo lotInfo)
        {
            try
            {
                //item.SubEqpID   获取所有需要使用内层下载的设备，并设定成ok
                foreach (var setSubEq in EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => (r.isDownloadInnerLot || r.isNotDownloadJob)))
                {
                    setSubEq.JobDataDownloadChangeResult = eCheckResult.ok;
                }
                //根据MES下载的需要下载内层Lot的设备，设定成nothing
                foreach (var item in lotInfo.InnerLotID)
                {
                    EquipmentModel equipment = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByEqID(item.SubEqpID);
                    if (equipment != null)
                    {
                        equipment.JobDataDownloadChangeResult = eCheckResult.nothing;
                    }
                }
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
            }
        }

        /// <summary>
        /// 转换MDM平台下发的设备参数名称
        /// </summary>
        /// <param name="mdmParameterName"></param>
        /// <param name="EqID"></param>
        /// <returns>ExchangeName</returns>
        public string GetExchangeParameterName(string mdmParameterName, string EqID)
        {
            string ExchangeName = string.Empty;
            try
            {
                bool isSame = false;
                Socket_DynamicLibraryBase dl;
                dl = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetDynamicLibrary(EqID);
                foreach (var item in dl.Dic_ParameterModel.Values)
                {
                    if (string.IsNullOrEmpty(item.Alias))
                    {
                        continue;
                    }
                    if (item.Alias == mdmParameterName)
                    {
                        ExchangeName = item.Name;
                        isSame = true;
                    }
                }
                if (!isSame)
                {
                    ExchangeName = mdmParameterName;
                    LogMsg(Log.LogLevel.Warn, string.Format("设备{0}的{0}_DynamicLibrary.xml内不包含{1}参数", EqID, mdmParameterName));
                }
                return ExchangeName;
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return ExchangeName;
            }
        }
        /// <summary>
        /// 根据内层数，算出MES下载的总任务数，然后下载给棕化线收板机
        /// </summary>
        /// <param name="input"></param>
        /// <param name="count"></param>
        /// <param name="subUnloadQtyGroup"></param>
        public void GetSubUnloadQtyGroup(string input, int count, out string subUnloadQtyGroup)
        {
            subUnloadQtyGroup = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(input) || input == "0")
                {
                    return;
                }
                int intSubUnloadQtyGroup = int.Parse(input);
                subUnloadQtyGroup = (count * intSubUnloadQtyGroup).ToString();
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));

            }

        }

        /// <summary>
        /// 获取对应设备使用的参数，并用设备当前使用的参数和MES下载的参数比对
        /// </summary>
        /// <param name="mdmParameterName"></param>
        /// <param name="EqID"></param>
        /// <returns></returns>
        public List<MessageModel.Param> GetEqUseParameter(List<MessageModel.Param> listParameterModel)
        {

            List<MessageModel.Param> lisPa = new List<MessageModel.Param>();
            try
            {
                Socket_DynamicLibraryBase dl;
                foreach (var eq in EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
                {
                    var eqParameter = listParameterModel.Where(r => r.SubEqpID == eq.EQID).ToList();
                    dl = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetDynamicLibrary(eq.EQID);
                    foreach (var dynamicParameter in dl.Dic_ParameterModel.Values)
                    {
                        var confirmPara = eqParameter.Where(r => r.ParamName.ToUpper() == dynamicParameter.Name.ToUpper()).FirstOrDefault();
                        if (confirmPara == null)
                        {
                            LogMsg(Log.LogLevel.Warn, $"设备[{eq.EQName}]的设备参数[{dynamicParameter.Name}]不在MES下载的参数中,请确认MES参数是否正确.");
                            lisPa.Add(new MessageModel.Param { SubEqpID = eq.EQID, ParamName = dynamicParameter.Name, ParamValue = "", ParamType = "" });
                        }
                        else
                        {
                            lisPa.Add(confirmPara);
                        }
                    }

                }
                return lisPa;
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return lisPa;
            }
        }

        /// <summary>
        /// 参数名称转Data Code
        /// </summary>
        /// <param name="ParameterName"></param>
        /// <param name="EqID"></param>
        /// <returns></returns>
        public string GetDataCode(string ParameterName, string EqID)
        {
            string DataCode = string.Empty;
            try
            {
                Socket_DynamicLibraryBase dl;
                dl = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetDynamicLibrary(EqID);
                foreach (var item in dl.Dic_ParameterModel.Values)
                {
                    if (item.Name == ParameterName)
                    {
                        DataCode = item.ID;
                    }
                }
                return DataCode;
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return DataCode;
            }
        }


    }
}
