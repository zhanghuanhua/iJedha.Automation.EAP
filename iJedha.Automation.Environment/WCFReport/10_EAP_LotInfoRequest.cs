using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Library;
using iJedha.Automation.EAP.Model;
using iJedha.Automation.EAP.ModelBase;
using iJedha.Customized.MessageStructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace iJedha.Automation.EAP.Environment
{
    public partial class WCFReport : BaseComm
    {
        public void EAP_LotInfoRequest(LotInfoRequest _data, EquipmentModel em)
        {
            iJedha.MES.WcfServiceInterface.IWcfServiceContract client = null;
            try
            {
                if (EAPEnvironment.commonLibrary.HostConnectMode == LibraryBase.eHostConnectMode.DISCONNECT)
                {
                    return;
                }

                string _outdata;
                if (ConvertJSON(_data, out _outdata))
                    LogMsg(Log.LogLevel.Trace, string.Format("WCF Message<{0}> Send OK, Content<{1}>", System.Reflection.MethodBase.GetCurrentMethod().Name, _outdata));

                using (ChannelFactory<iJedha.MES.WcfServiceInterface.IWcfServiceContract> channel = new ChannelFactory<iJedha.MES.WcfServiceInterface.IWcfServiceContract>(new NetTcpBinding(SecurityMode.None), new EndpointAddress(EAPEnvironment.commonLibrary.baseLib.wcfParaLibrary.RemoteUrlString)))
                {
                    client = channel.CreateChannel();
                    iJedha.MES.WcfServiceInterface.LotInformation returnInfo = client.EAP_LotInfoRequest(_data.MainEqpID, _data.SubEqpID, _data.PortID, _data.CarriedID);

                    string _indata;
                    if (ConvertJSON(returnInfo, out _indata))
                        LogMsg(Log.LogLevel.Trace, string.Format("WCF Message<{0}> Recv OK, Content<{1}>", System.Reflection.MethodBase.GetCurrentMethod().Name, _indata));

                    if (returnInfo.CheckSuccessCode != "0")
                    {
                        string errMsg = string.Format("MES回复, 接口名称<{0}>, 错误代码<{1}>, 错误描述<{2}><{3}>", System.Reflection.MethodBase.GetCurrentMethod().Name, returnInfo.CheckSuccessCode, _data.SubEqpID, returnInfo.CheckErrorMsg);
                        Environment.BaseComm.ErrorHandleRule("E3002", errMsg);
                        //0:OK 1:NG
                        if (returnInfo.InputCarrierFamily != "L-Rack")
                            new HSMSReport().S2F41(em, ConstLibrary.CONST_COMMAND_LoadStart, new List<string> { "1" });
                        else
                            new HSMSReport().S2F41(em, ConstLibrary.CONST_COMMAND_CarrierStart, new List<string> { "1", "", "", "" });
                    }
                    else
                    {
                        string err;
                        PortModel port;
                        LotModel lot;

                        //取得当前Lot ID
                        EAPEnvironment.commonLibrary.commonModel.currentProcessLotID = returnInfo.LotID;
                        if (em.ControlMode == eControlMode.ONLINE_REMOTE)
                        {
                            #region Check LotInfo
                            if (!CheckLotInfo(em, _data, returnInfo, out port, out err))
                            {
                                //0:OK 1:NG
                                Environment.BaseComm.ErrorHandleRule("E2011", string.Format("Lot<{0}>MES Lot下载资料检查异常：{1}...，拒绝生产.",
                                    EAPEnvironment.commonLibrary.commonModel.currentProcessLotID, err));
                                if (returnInfo.InputCarrierFamily != "L-Rack")
                                    new HSMSReport().S2F41(em, ConstLibrary.CONST_COMMAND_LoadStart, new List<string> { "1" });
                                else
                                    new HSMSReport().S2F41(em, ConstLibrary.CONST_COMMAND_CarrierStart, new List<string> { "1", "", "", "" });
                                if (em.isTerminalSelect) new HSMSReport().S10F3(em, err);
                                return;
                            }
                            #endregion

                            #region handle Info
                            if (!HandleLotInfo(em, port, returnInfo, out lot, out err))
                            {
                                //0:OK 1:NG
                                if (returnInfo.InputCarrierFamily != "L-Rack")
                                    new HSMSReport().S2F41(em, ConstLibrary.CONST_COMMAND_LoadStart, new List<string> { "1" });
                                else
                                    new HSMSReport().S2F41(em, ConstLibrary.CONST_COMMAND_CarrierStart, new List<string> { "1", "", "", "" });
                                if (em.isTerminalSelect) new HSMSReport().S10F3(em, err);
                                return;
                            }
                            #endregion

                            #region 确认配方或参数 
                            //if (lot.LotType != "0")
                            //{
                            bool isCheckPara = false;
                            EAPEnvironment.commonLibrary.commonModel.InParameterCheckResult = new System.Collections.Concurrent.ConcurrentDictionary<string, CheckContent>();
                            foreach (var v in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
                            {
                                CheckContent content = new CheckContent();
                                var m1 = (from n in lot.SubEqpList where n.SubEqpID == v.EQName select n).FirstOrDefault();
                                if (m1 != null)
                                {
                                    if (!string.IsNullOrEmpty(v.CheckPPID))
                                    {
                                        content.PPID = v.CheckPPID;
                                        v.isCheckPPIDResult = false;
                                        v.List_PPID = new Dictionary<string, string>();
                                        new HSMSReport().S7F19(v);
                                        isCheckPara = true;
                                    }
                                }
                                else
                                    v.CheckPPID = "";

                                var m2 = (from n in lot.LotParameterList where n.EqpID == v.EQName select n).FirstOrDefault();
                                if (m2 != null)
                                {
                                    if (v.List_KeyTraceDateSpec.Count > 0)
                                    {
                                        content.ParameterCheckResult = eParameterCheckResult.wait;
                                        new HSMSReport().S1F3(v, 2);
                                        isCheckPara = true;
                                    }
                                }

                                if (isCheckPara)
                                    EAPEnvironment.commonLibrary.commonModel.InParameterCheckResult.TryAdd(v.EQID, content);
                            }

                            if (new BaseComm().InitialThreadWork("TrackInCheck", ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE04, 2000, true, lot))
                            {
                                LogMsg(Log.LogLevel.Info, string.Format("Lot<{0}>确认设备数据中......", returnInfo.LotID), false);
                                EAPEnvironment.commonLibrary.commonModel.InTrackInCheckStart = true;
                                EAPEnvironment.commonLibrary.commonModel.InTrackInCheckTime = DateTime.Now;
                            }
                            if (!EAPEnvironment.commonLibrary.commonModel.InTrackInCheckStart)
                            {
                                new BaseComm().DeleteThreadWork("TrackInCheck");
                            }
                            //}
                            //else
                            //{

                            //}
                            #endregion
                        }
                    }
                }
            }
            catch (TimeoutException e)
            {
                string errMsg = string.Format("MES回复，接口名称<{0}>超时.", System.Reflection.MethodBase.GetCurrentMethod().Name);
                Environment.BaseComm.ErrorHandleRule("E1003", errMsg);
                LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0} {1}", e.ToString(), e.StackTrace.ToString()));
            }
            catch (FaultException e)
            {
                string errMsg = string.Format("MES回复，接口名称<{0}>, 错误代码<{1}>, 错误描述<{2}>", System.Reflection.MethodBase.GetCurrentMethod().Name, e.HResult, e.Message);
                Environment.BaseComm.ErrorHandleRule("E1004", errMsg);
                LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0} {1}", e.ToString(), e.StackTrace.ToString()));
            }
            catch (CommunicationException e)
            {
                string errMsg = string.Format("MES回复，接口名称<{0}>, 错误代码<{1}>, 错误描述<{2}>", System.Reflection.MethodBase.GetCurrentMethod().Name, e.HResult, e.Message);
                Environment.BaseComm.ErrorHandleRule("E1004", errMsg);
                (client as ICommunicationObject).Abort();
                Environment.EAPEnvironment.WcfClient_MES.reStart();
                LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0} {1}", e.ToString(), e.StackTrace.ToString()));
            }
            catch (Exception e)
            {
                StackTrace st = new StackTrace(new StackFrame(true));
                StackFrame sf = st.GetFrame(0);
                string errMsg = string.Format("EAP发送<{0}>异常：程序出错，代码行数<{1}>，ErrMsg<{2}>",
                    System.Reflection.MethodBase.GetCurrentMethod().Name, sf.GetFileLineNumber(), e.Message);
                Environment.BaseComm.ErrorHandleRule("E0001", errMsg);
                LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0} {1}", e.ToString(), e.StackTrace.ToString()));
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
        public bool CheckLotInfo(EquipmentModel em, LotInfoRequest _data, iJedha.MES.WcfServiceInterface.LotInformation lotInformation, out PortModel port, out string err)
        {
            try
            {

                #region 比对Port
                port = em.GetPortModelByPortID(_data.PortID);
                if (port == null)
                {
                    err = string.Format("异常项目：<{1}>，异常值：<{2}>", "PortID", lotInformation.PortID);
                    return false;
                }
                #endregion

                #region 比对LotID
                if (lotInformation.LotID == string.Empty)
                {
                    err = string.Format("异常项目：<{1}>，异常值：<{2}>", "LotID", lotInformation.LotID);
                    return false;
                }
                #endregion

                #region 比对载具ID
                if (_data.CarriedID != lotInformation.CarriedID)
                {
                    err = string.Format("异常项目：<{1}>，异常值：<{2}>", "CarriedID", lotInformation.CarriedID);
                    return false;
                }
                #endregion

                #region 比对Panel Count
                if (int.Parse(lotInformation.PanelTotalQty) == 0)
                {
                    err = string.Format("异常项目：<{1}>，异常值：<{2}>", "PanelTotalQty", lotInformation.PanelTotalQty);
                    return false;
                }
                if (int.Parse(lotInformation.ProcessQty) == 0)
                {
                    err = string.Format("异常项目：<{1}>，异常值：<{2}>", "ProcessQty", lotInformation.ProcessQty);
                    return false;
                }
                #endregion

                #region 比对数量
                if (lotInformation.ProductList.Count > 0)
                {
                    if (int.Parse(lotInformation.PanelTotalQty) != lotInformation.ProductList.Count)
                    {
                        err = string.Format("异常项目：<{1}>，异常值：<{2}>", "ProductList", lotInformation.ProductList.Count);
                        return false;
                    }
                }
                #endregion

                #region 比对WIPDATA
                if (lotInformation.WIPDataDataList == null)
                {
                    err = string.Format("异常项目：<{1}>，异常值：<{2}>", "WIPDataDataList", "Null");
                    return false;
                }
                #endregion

                #region 比对设备List
                if (lotInformation.SubEqpList == null)
                {
                    err = string.Format("异常项目：<{1}>，异常值：<{2}>", "SubEqpList", "Null");
                    return false;
                }
                #endregion

                err = string.Empty;
                string msg = string.Format("Lot<{0}>资料检查完成.", lotInformation.LotID);
                LogMsg(Log.LogLevel.Info, msg);
                return true;
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0} {1}", ex.ToString(), ex.StackTrace.ToString()));
                err = string.Format("Lot<{0}>资料检查失败：程式出错.", lotInformation.LotID);
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
        public bool HandleLotInfo(EquipmentModel em, PortModel port, iJedha.MES.WcfServiceInterface.LotInformation lotInformation, out LotModel lot, out string err)
        {
            lot = new LotModel();
            err = string.Empty;
            try
            {
                lot.LotID = lotInformation.LotID;
                lot.MainLotID = lotInformation.MainLot;
                lot.CarrierID = lotInformation.CarriedID;
                lot.PanelTotalQty = int.Parse(lotInformation.PanelTotalQty);
                lot.ProcessQty = int.Parse(lotInformation.ProcessQty);
                lot.InputCarrierFamily = lotInformation.InputCarrierFamily;
                lot.OutputCarrierFamily = lotInformation.OutputCarrierFamily;
                lot.PN = lotInformation.PN;
                lot.LocalEQStation = em.EQName;
                lot.LocalPortStation = port.ID;
                lot.LotStatus = lotInformation.LotStatus;
                lot.LotType = lotInformation.LotType;
                lot.CurrentPanelQty = int.Parse(lotInformation.ProcessQty);
                lot.OutputCarrierMaxQty = lotInformation.OutputCarrierMaxQty;
                lot.StaticFirstInspQty = lotInformation.StaticFirstInspQty;
                lot.DataSource = "Auto";
                lot.StaticFirstInspQty = (!string.IsNullOrEmpty(lot.StaticFirstInspQty)) ? lot.StaticFirstInspQty : "0";
                lot.StaticFirstInspMode = (!string.IsNullOrEmpty(lot.StaticFirstInspQty)) ? true : false;
                //lot.CurrentPanelQty = int.Parse(lotInformation.ProcessQty) - int.Parse(lot.StaticFirstInspQty);

                //取得配方
                foreach (var v in lotInformation.SubEqpList)
                {
                    EquipmentModel subem = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByName(v.SubEqpID);
                    if (subem == null) continue;
                    subem.CheckPPID = v.RecipeName;
                    subem.isCheckPPIDResult = false;
                    SubEqp eqp = new SubEqp();
                    eqp.SubEqpID = v.SubEqpID;
                    eqp.RecipeName = v.RecipeName;
                    lot.SubEqpList.Add(eqp);
                }

                //取得WIP DATA
                List<string> lst1 = lotInformation.WIPDataDataList.Select(t => t.SubEqpID).Distinct().ToList();
                foreach (var v in lst1)
                {
                    EquipmentModel subem = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByName(v);
                    if (subem == null) continue;
                    subem.List_KeyTraceDateSpec = new List<ParameterModel>();
                }

                ParameterModel pm;
                foreach (var v in lotInformation.WIPDataDataList)
                {
                    EquipmentModel subem = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByName(v.SubEqpID);
                    if (subem == null) continue;
                    pm = new ParameterModel();
                    pm.EqpID = v.SubEqpID;
                    pm.ItemID = v.SVID;
                    pm.ItemName = v.WIPDataName;
                    pm.ItemValue = v.DefaultValue;
                    pm.ItemMaxValue = v.ItemMaxValue;
                    pm.ItemMinValue = v.ItemMinValue;
                    pm.ItemType = v.DataType;
                    pm.ServiceName = v.ServiceName;
                    pm.Fator = v.TraceFactor;
                    subem.List_KeyTraceDateSpec.Add(pm);
                    lot.LotParameterList.Add(pm);
                }

                //取得Panel List
                foreach (var v in lotInformation.ProductList)
                {
                    lot.PanelList.Add(v.PanelID);
                    port.List_Panel.Add(port.List_Panel.Count, v.PanelID);
                }

                //要处理的Panel
                port.PanelCount = int.Parse(lotInformation.ProcessQty);
                lot.LotProcessStatus = eLotProcessStatus.Create;
                if (port.List_Lot.ContainsKey(lot.CarrierID)) port.List_Lot.Remove(lot.CarrierID);
                port.List_Lot.Add(lot.CarrierID, lot);
                string msg = string.Format("Lot<{0}>建置资料完成.", lotInformation.LotID);
                LogMsg(Log.LogLevel.Info, msg, false);
                return true;
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0} {1}", ex.ToString(), ex.StackTrace.ToString()));
                err = string.Format("Lot<{0}>建置资料失败，拒绝上机.", lotInformation.LotID);
                LogMsg(Log.LogLevel.Warn, err);
                return false;
            }
        }
    }
}
