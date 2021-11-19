using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Library;
using iJedha.Automation.EAP.Model;
using iJedha.Customized.MessageStructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;

namespace iJedha.Automation.EAP.Environment
{
    public partial class WCFReport : BaseComm
    {
        public void EAP_LotTrackInRequest(LotTrackInRequest _data, LotModel lot)
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
                    iJedha.MES.WcfServiceInterface.TrackInformation returnInfo = client.EAP_LotTrackInRequest(_data.MainEqpID, _data.SubEqpID, _data.PortID, _data.CarriedID, _data.WIPDataList);

                    string _indata;
                    if (ConvertJSON(returnInfo, out _indata))
                        LogMsg(Log.LogLevel.Trace, string.Format("WCF Message<{0}> Recv OK, Content<{1}>", System.Reflection.MethodBase.GetCurrentMethod().Name, _indata));

                    EquipmentModel em = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByName(_data.SubEqpID);
                    if (returnInfo.CheckSuccessCode != "0")
                    {
                        string errMsg = string.Format("MES回复, 接口名称<{0}>, 错误代码<{1}>, 错误描述<{2}><{3}>", System.Reflection.MethodBase.GetCurrentMethod().Name, returnInfo.CheckSuccessCode, _data.SubEqpID, returnInfo.CheckErrorMsg);
                        Environment.BaseComm.ErrorHandleRule("E3003", errMsg);
                        //0:OK 1:NG
                        if (lot.InputCarrierFamily != "L-Rack")
                            new HSMSReport().S2F41(em, ConstLibrary.CONST_COMMAND_LoadStart, new List<string> { "1" });
                        else
                            new HSMSReport().S2F41(em, ConstLibrary.CONST_COMMAND_CarrierStart, new List<string> { "1", "", "", "" });
                    }
                    else
                    {                    
                        lock (lot)
                        {
                            lot.LotProcessStatus = eLotProcessStatus.Ready;
                            lot.ProcessTime = DateTime.Now;
                        }
                        //add Process Lot
                        Environment.EAPEnvironment.commonLibrary.commonModel.AddProcessLotModel((LotModel)lot.Clone());

                        LogMsg(Log.LogLevel.Info, string.Format("Lot<{0}>产品<{1}>数量<{2}>上机完成.", lot.LotID, lot.PN, lot.PanelTotalQty));

                        //new MQReport().MQ_LotInformation(lot.LotID, lot.PN, lot.PanelTotalQty.ToString(), "上机完成");

                        #region 特殊设备
                        bool isSelectPP = false;
                        foreach (var v in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
                        {
                            if (v.isPPSelect && !string.IsNullOrEmpty(v.CheckPPID))
                            {
                                if (v.ParseKey == eParseKey.ATO)
                                {
                                    new Environment.HSMSReport().S2F41(v, Library.ConstLibrary.CONST_COMMAND_PPSelect,
                                        new List<string> { lot.LotID, "", "", v.CheckPPID, lot.ProcessQty.ToString(), "", "", "" });
                                    BaseComm.LogMsg(Log.LogLevel.Info, string.Format("设备<{0}>PPID<{1}>切换中......", v.EQName, v.CheckPPID));
                                }
                                CheckContent content = new CheckContent();
                                content.PPSelectCheckResult = eParameterCheckResult.wait;
                                EAPEnvironment.commonLibrary.commonModel.InParameterCheckResult.TryAdd(v.EQID, content);
                                isSelectPP = true;
                            }
                        }
                        #endregion
                        if (isSelectPP)
                        {
                            if (new BaseComm().InitialThreadWork("PPSelectCheck", ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE06, 2000, true, lot))
                            {
                                EAPEnvironment.commonLibrary.commonModel.InPPSelectCheckStart = true;
                                EAPEnvironment.commonLibrary.commonModel.InPPSelectCheckTime = DateTime.Now;
                            }
                            if (!EAPEnvironment.commonLibrary.commonModel.InPPSelectCheckStart)
                            {
                                new BaseComm().DeleteThreadWork("PPSelectCheck");
                            }
                        }
                        else
                        {
                            //0:OK 1:NG
                            #region iJDEAP_0004 计算投板数量 = 产品数量 + Dummy数量
                            int inputCount = lot.ProcessQty;
                            if (Environment.EAPEnvironment.commonLibrary.customizedLibrary.DummyTestCount != 0)
                            {
                                inputCount = inputCount + EAPEnvironment.commonLibrary.customizedLibrary.DummyTestCount;
                            }
                            #endregion
                            if (lot.InputCarrierFamily != "L-Rack")
                                new HSMSReport().S2F41(em, ConstLibrary.CONST_COMMAND_LoadStart, new List<string> { "0" });
                            LogMsg(Log.LogLevel.Info, string.Format("Lot<{0}>允许上机<{1}>.", lot.LotID, inputCount.ToString()));
                            EquipmentModel uem = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByType("U");
                            if (uem != null)
                            {
                                #region iJDEAP_0004 计算收板数量 = 产品数量 + Dummy数量 - 动态检数量
                                double outputCount = lot.ProcessQty;
                                if (Environment.EAPEnvironment.commonLibrary.customizedLibrary.DummyTestCount != 0)
                                {
                                    outputCount = outputCount + EAPEnvironment.commonLibrary.customizedLibrary.DummyTestCount;
                                }
                                if (Environment.EAPEnvironment.commonLibrary.customizedLibrary.DynamicTestCount != 0)
                                {
                                    outputCount = outputCount - EAPEnvironment.commonLibrary.customizedLibrary.DynamicTestCount;
                                }
                                else if (Environment.EAPEnvironment.commonLibrary.customizedLibrary.DynamicTestPercent != 0)
                                {
                                    outputCount = outputCount - Convert.ToInt16(lot.PanelTotalQty * EAPEnvironment.commonLibrary.customizedLibrary.DynamicTestPercent * 0.01);
                                }
                                #endregion
                                if (int.Parse(lot.StaticFirstInspQty) > 0)
                                    new Environment.HSMSReport().S2F41(uem, ConstLibrary.CONST_COMMAND_TrailLotStart, new List<string> { "0", lot.LotID, lot.StaticFirstInspQty, outputCount.ToString() });
                                else
                                    new Environment.HSMSReport().S2F41(uem, ConstLibrary.CONST_COMMAND_CarrierStart, new List<string> { "0", lot.LotID, outputCount.ToString(), outputCount.ToString() });
                                LogMsg(Log.LogLevel.Info, string.Format("Lot<{0}>允许下机<{1}>.", lot.LotID, outputCount.ToString()));


                                if (lot.InputCarrierFamily == "L-Rack")
                                {
                                    Environment.BaseComm.LogMsg(Log.LogLevel.Info, string.Format("设备<{0}><{1}><{2}>上料完成.", em.EQName, _data.PortID, _data.CarriedID));
                                    new WCFReport().EAP_LoadComplete(new LoadComplete()
                                    {
                                        MainEqpID = EAPEnvironment.commonLibrary.LineName,
                                        SubEqpID = em.EQName,
                                        PortID = _data.PortID,
                                        CarriedID = _data.CarriedID
                                    });
                                }
                            }
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
    }
}
