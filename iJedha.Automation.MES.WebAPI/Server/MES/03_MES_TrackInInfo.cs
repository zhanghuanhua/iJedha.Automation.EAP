//******************************************************************
//   系统名称 : iJedha.Automation.EAP.WebAPIService
//   文件概要 : EAPEnvironment
//   作    者 : 张明哲
//   <更新履历>
//   1.0.0.0    2020/2/6 10:40:34
//******************************************************************
using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Environment;
using iJedha.Automation.EAP.Library;
using iJedha.Automation.EAP.LibraryBase;
using iJedha.Automation.EAP.Model;
using iJedha.Automation.EAP.ModelBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace iJedha.Automation.EAP.WebAPI
{
    public class MES_TrackInInfoController : ApiController
    {

        /// <summary>
        /// MES主动下载生产任务给EAP
        /// </summary>
        /// <param name="ApiRequest"></param>
        /// <returns></returns>
        [HttpPost]
        public MessageModelBase.ApiResult MES_TrackInInfo(MessageModelBase.ApiRequest ApiRequest)
        {
            MessageModelBase.ApiResult ri = new MessageModelBase.ApiResult();
            try
            {
                if (EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckStart)
                {
                    EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckInitial();
                    EAPEnvironment.EAPAp.DeleteThreadWork(ConstLibrary.CONST_DLL_RULE_ProductionConditionCheck);
                }

                ErrorCodeModelBase errm = new ErrorCodeModelBase();
                string err;
                PortModel port;
                EquipmentModel em = new EquipmentModel();
                LotModel lot;
                #region [解析MES发送的消息]
                object lotinfoobject;
                new Serialize().DeSerializeJSON(ApiRequest.Content.ToString(), new MessageModel.LotInfo().GetType(), out lotinfoobject);
                MessageModel.LotInfo lotinfo = (MessageModel.LotInfo)lotinfoobject;
                #endregion
                #region [Trace Log]
                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("WebAPI Message<{0}> Recv OK, Content<{1}>", System.Reflection.MethodBase.GetCurrentMethod().Name, ApiRequest.Content));
                
                #endregion

                //取得当前Lot ID
                EAPEnvironment.commonLibrary.commonModel.currentProcessLotID = lotinfo.LotID;
                //根据不同条件取得设备
                if (EAPEnvironment.commonLibrary.lineModel.isMainEqpID)//如果下载MainEqpID和设备ID一致，取得设备物件如下方法，用于机械钻机、铣机设备
                {
                    em = EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.EQID == lotinfo.MainEqpID).FirstOrDefault();
                }
                //如果是给多台设备下载生产任务，取得设备物件如下方法。
                if (!string.IsNullOrEmpty(EAPEnvironment.commonLibrary.lineModel.StartEquipmentID))
                {
                    em = EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.EQID == EAPEnvironment.commonLibrary.lineModel.StartEquipmentID).FirstOrDefault();
                }
                History.EAP_EQP_EVENTHISTORY(em, eEventName.MES_TRACKININFO, eEventFlow.MES2EAP, ApiRequest.Content, em.CurrentLotID, lotinfo.PortID);
                #region [Check LotInfo]
                if (!CheckLotInfo(lotinfo, em, out err, out port))
                {
                    //0:OK 1:NG
                    BaseComm.ErrorHandleRule("E2103", string.Format("Lot<{0}>批次资料下载检查异常：{1}...，拒绝生产.",
                        lotinfo.LotID, err), ref errm);

                    ri.bSucc = false;
                    ri.strCode = "2103";
                    ri.strMsg = err;
                    ri.DataTime = DateTime.Now;
                    return ri;
                }
                #endregion

                #region [handle Info]
                if (!HandleLotInfo(port, lotinfo, out lot, out err))
                {
                    BaseComm.ErrorHandleRule("E2107", string.Format("Lot<{0}>建制资料异常：{1}...，拒绝生产.",
                        lotinfo.LotID, err), ref errm);

                    ri.bSucc = false;
                    ri.strCode = "2107";
                    ri.strMsg = err;
                    ri.DataTime = DateTime.Now;
                    return ri;
                }

                #region 初始化完成清线Flag
                EAPEnvironment.commonLibrary.isProcessOK = false;
                #endregion

                #region 初始化设备回复状态
                foreach (var item in EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
                {
                    item.JobDataDownloadChangeResult = eCheckResult.nothing;
                    item.RetryCount = 0;
                }
                #endregion

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

                //#region 计算完工信号次数(冲孔连棕化使用)
                //Environment.EAPEnvironment.commonLibrary.UnloadEquipmentProcessCompleteReportCount = 0;
                //if (Environment.EAPEnvironment.commonLibrary.lineModel.isInnerLotPanelList)
                //{
                //    int TotalCount = 0;
                //    foreach (var item in lotinfo.InnerLotID)
                //    {
                //        TotalCount += int.Parse(item.LoadQty);
                //    }
                //    do
                //    {
                //        Environment.EAPEnvironment.commonLibrary.UnloadEquipmentProcessCompleteReportCount++;
                //        TotalCount = TotalCount - int.Parse(lotinfo.UnloadQty);
                //    } while (TotalCount > 0);
                //}

                //#endregion

                EAPEnvironment.commonLibrary.isMESDownloadData = true;


                #region  手动下载开关
                if (Environment.EAPEnvironment.commonLibrary.lineModel.isManualJobDataDoanload)
                {
                    ri.bSucc = true;
                    ri.strCode = "0000";
                    ri.strMsg = "";
                    ri.DataTime = DateTime.Now;
                    return ri;
                }
                #endregion

                if (Environment.EAPEnvironment.commonLibrary.lineModel.isLoadCompleteCountCheck)
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
                            BaseComm.LogMsg(Log.LogLevel.Info, string.Format("线体<{0}>正在检查投板机上报Load Complete情况......", lotinfo.MainEqpID));
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
                    cm.em = new EquipmentModel();

                    Dic_Message.Add(lotinfo, cm);
                    if (!EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckStart)
                    {
                        if (EAPEnvironment.EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_ProductionConditionCheck, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_ProductionConditionCheck, 4000, true, Dic_Message))
                        {
                            BaseComm.LogMsg(Log.LogLevel.Info, string.Format("线体<{0}>正在检查生产条件......", lotinfo.MainEqpID));
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
                BaseComm.LogMsg(Log.LogLevel.Info, string.Format("MES下载生产任务<{0}>OK.", lotinfo.LotID));
                ri.strCode = "0000";
                ri.strMsg = "成功.";
                ri.bSucc = true;
                ri.DataTime = DateTime.Now;
                //}
                return ri;

            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                ri.bSucc = false;
                ri.strCode = "E0001";
                ri.strMsg = "程式出错.";
                ri.DataTime = DateTime.Now;
                return ri;
            }
            finally
            {
                #region [Trace Log]
                string _outdata;
                if (BaseComm.ConvertJSON(ri, out _outdata))
                {
                    BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("WebAPI Message<{0}> Send OK, Content<{1}>", System.Reflection.MethodBase.GetCurrentMethod().Name, _outdata));
                }
                #endregion
            }

        }

        /// <summary>
        /// 检查Lot信息
        /// </summary>
        /// <param name="em"></param>
        /// <param name="_data"></param>
        /// <param name="lotInformation"></param>
        /// <param name="err"></param>
        /// <returns></returns>
        public bool CheckLotInfo(MessageModel.LotInfo lotinfo, EquipmentModel em, out string err, out PortModel port)
        {
            try
            {
                #region [比对Port]
                if (em != null)
                {
                    port = em.GetPortModelByPortID(lotinfo.PortID);
                    if (port == null)
                    {
                        err = string.Format("异常项目：<{0}>，异常值：<{1}>", "PortID", lotinfo.PortID);
                        return false;
                    }
                }
                else
                {
                    port = null;
                }

                #endregion

                #region [比对LotID]
                if (lotinfo.LotID == string.Empty)
                {
                    err = string.Format("异常项目：<{0}>，异常值：<{1}>", "LotID", lotinfo.LotID);
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
                if (int.Parse(lotinfo.PanelTotalQty) == 0)
                {
                    err = string.Format("异常项目：<{0}>，异常值：<{1}>", "PanelTotalQty", lotinfo.PanelTotalQty);
                    return false;
                }
                //if (int.Parse(lotinfo.JobTotalQty) == 0)
                //{
                //    err = string.Format("异常项目：<{0}>，异常值：<{1}>", "JobTotalQty", lotinfo.JobTotalQty);
                //    return false;
                //}
                #endregion

                err = string.Empty;
                string msg = string.Format("MES下载生产任务Lot<{0}>接收检查完成.", lotinfo.LotID);
                BaseComm.LogMsg(Log.LogLevel.Info, msg);
                return true;
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                err = string.Format("Lot<{0}>资料检查失败：程式出错.", lotinfo.LotID);
                BaseComm.LogMsg(Log.LogLevel.Warn, err);
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
        public bool HandleLotInfo(PortModel port, MessageModel.LotInfo lotinfo, out LotModel lot, out string err)
        {
            if (port == null)
            {
                port = PortModel.pm;
            }

            lot = new LotModel();
            err = string.Empty;
            port.DeepInitial_UDRQ(lotinfo.LotID);
            try
            {
                lot.MainEqpID = lotinfo.MainEqpID;
                lot.LotID = lotinfo.LotID;
                lot.LotStatus = lotinfo.LotStatus;
                lot.LoadQty = lotinfo.LoadQty;
                lot.UnloadQty = lotinfo.UnloadQty;
                lot.LotStartQty = lotinfo.LotStartQty;
                lot.LotMatchQty = lotinfo.LotMatchQty;
                lot.PnlStartSN = lotinfo.PnlStartSN;
                lot.PN = lotinfo.PN;
                lot.ProductRev = lotinfo.ProductRev;
                lot.PanelTotalQty = int.Parse(lotinfo.PanelTotalQty);

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
                lot.WorkOrder = lotinfo.WorkOrder;
                lot.WorkOrderPNLQty = lotinfo.WorkOrderPNLQty;

                if (string.IsNullOrEmpty(lotinfo.InnerLotTotalQty))
                {
                    BaseComm.LogMsg(Log.LogLevel.Warn, $"lotinfo.InnerLotTotalQty值为[{lotinfo.InnerLotTotalQty}]");
                }
                int InnerQty;
                int.TryParse(lotinfo.InnerLotTotalQty, out InnerQty);
                lot.InnerLotTotalQty = InnerQty;


                // lot.LocalEQStation = em.EQName;
                lot.LocalPortStation = port.PortID.ToString();
                lot.PortID= port.PortID.ToString(); 
                lot.ProcessTime = DateTime.Now;

                //lot.MatchDummyQty = lotinfo.MatchDummyQty;
                lot.DataSource = eDataSource.Auto;
                port.LotFlag = false;

                //ot.RunType = (eRunType)int.Parse(lotinfo.RunType);
                if (lotinfo.FirstInspect == null)
                {
                    lotinfo.FirstInspect = new List<MessageModel.FirstInspect>();
                    lot.FirstInspFlag = false;
                    //if (lot.RunType == eRunType.FirstInspExt)
                    //{
                    //    lot.RunType = eRunType.Normal;
                    //}
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

                EAPEnvironment.commonLibrary.MaterialPortIDBinding.Clear();

                //EAPEnvironment.commonLibrary.MESInnerLotSubEqInfo = "";
                //EAPEnvironment.commonLibrary.PunchingList = new List<string>();
                //EAPEnvironment.commonLibrary.LayerLevel = new List<string>();
                //EAPEnvironment.commonLibrary.MESInnerLotSubEqList = new List<string>();

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
                    //EAPEnvironment.commonLibrary.PunchingList.Add(string.Format("{0}:{1}", innerLot.InnerLayer, innerLot.IsSkipPunching));
                    //EAPEnvironment.commonLibrary.LayerLevel.Add(innerLot.InnerLayer);
                    //EAPEnvironment.commonLibrary.MESInnerLotSubEqInfo += v.SubEqpID;
                    //EAPEnvironment.commonLibrary.MESInnerLotSubEqList.Add(v.SubEqpID);

                    lot.PunchingList.Add(string.Format("{0}:{1}", innerLot.InnerLayer, innerLot.IsSkipPunching));
                    lot.LayerLevel.Add(innerLot.InnerLayer);
                    lot.MESInnerLotSubEqInfo += v.SubEqpID;
                    lot.MESInnerLotSubEqList.Add(v.SubEqpID);

                    lot.InnerLotList.Add(innerLot);
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
                var aa = GetEqUseParameter(lotinfo.ParamList);

                foreach (var v in aa)
                {
                    EquipmentModel subem = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(v.SubEqpID);
                    if (subem == null)
                    {
                        continue;
                    }
                    SubEqp eqp = new SubEqp();
                    ParameterModel pm = new ParameterModel();
                    eqp.SubEqpID = v.SubEqpID;

                    pm.ItemName = v.ParamName;// GetExchangeParameterName(v.ParamName, v.SubEqpID);
                    //pm.ItemName = GetDataCode(v.ParamName, v.SubEqpID);//v.ParamName;
                    pm.ItemValue = v.ParamValue.Contains("/") ? v.ParamValue.Replace('/', '\\') : v.ParamValue;
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
                //如果不需要过账，把上一个Lot清除
                if (!EAPEnvironment.commonLibrary.lineModel.isNeedPost)
                {
                    port.List_Lot.Clear();
                    EAPEnvironment.commonLibrary.commonModel.RemoveFrontProcessLotModel(lot);
                }
                if (port.List_Lot.ContainsKey(lot.LotID))
                {
                    port.List_Lot.Remove(lot.LotID);
                }
                port.List_Lot.Add(lot.LotID, lot);

                string msg = string.Format("Lot<{0}>建置资料完成.", lotinfo.LotID);
                BaseComm.LogMsg(Log.LogLevel.Info, msg);
                //add Process Lot
                Environment.EAPEnvironment.commonLibrary.commonModel.AddProcessLotModel((LotModel)lot.Clone());
                return true;
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                err = string.Format("Lot<{0}>建置资料失败，拒绝上机.", lotinfo.LotID);
                BaseComm.LogMsg(Log.LogLevel.Warn, err);
                return false;
            }
        }
        /// <summary>
        /// 获取EAP使用的参数列表
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
                            BaseComm.LogMsg(Log.LogLevel.Warn, $"设备[{eq.EQName}]的设备参数[{dynamicParameter.Name}]不在MES下载的参数中,请确认MES参数是否正确.");
                            lisPa.Add(new MessageModel.Param { SubEqpID = eq.EQID, ParamName = dynamicParameter.Name, ParamValue = dynamicParameter.Alias, ParamType = "" });
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
    }

}
