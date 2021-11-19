using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Library;
using iJedha.Automation.EAP.Model;
using iJedha.Customized.MessageStructure;
using System;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;

namespace iJedha.Automation.EAP.Environment
{
    public partial class WCFReport : BaseComm
    {
        public void EAP_EqpModeRequest(EqpModeRequest _data)
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
                    iJedha.MES.WcfServiceInterface.ReturnModeInformation returnInfo = client.EAP_EqpModeRequest(_data.MainEqpID);

                    string _indata;
                    if (ConvertJSON(returnInfo, out _indata))
                        LogMsg(Log.LogLevel.Trace, string.Format("WCF Message<{0}> Recv OK, Content<{1}>", System.Reflection.MethodBase.GetCurrentMethod().Name, _indata));

                    if (returnInfo.CheckSuccessCode != "0")
                    {
                        string errMsg = string.Format("MES回复, 接口名称<{0}>, 错误代码<{1}>, 错误描述<{2}><{3}>", System.Reflection.MethodBase.GetCurrentMethod().Name, returnInfo.CheckSuccessCode, _data.MainEqpID, returnInfo.CheckErrorMsg);
                        LogMsg(Log.LogLevel.Warn, errMsg);
                    }
                    else
                    {
                        string errMessage = string.Empty;
                        switch (returnInfo.ProductionMode)
                        {
                            case "Manual":
                                {
                                    EAPEnvironment.commonLibrary.commonModel.PreMode = EAPEnvironment.commonLibrary.commonModel.CurMode;
                                    EAPEnvironment.commonLibrary.commonModel.CurMode = Model.eProductMode.Manual;
                                    #region 通知投收板机切Local
                                    foreach (var em in EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
                                    {
                                        if (em.ControlMode == eControlMode.ONLINE_REMOTE && (em.Type == "L" || em.Type == "U"))
                                        {
                                            em.ControlModeChangeResult = eCheckResult.wait;
                                            new HSMSReport().S2F41(em, ConstLibrary.CONST_COMMAND_Local);
                                        }
                                    }
                                    #endregion
                                }
                                break;
                            case "Semi": 
                                {
                                    bool isLocal = false;
                                    foreach (var em in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
                                    {
                                        if (em.ControlMode != eControlMode.ONLINE_REMOTE && (em.Type == "L" || em.Type == "U"))
                                        {
                                            em.ControlModeChangeResult = eCheckResult.wait;
                                            new HSMSReport().S2F41(em, ConstLibrary.CONST_COMMAND_Remote);
                                            isLocal = true;
                                        }
                                    }
                                    if (isLocal)
                                    {
                                        EAPEnvironment.commonLibrary.commonModel.isModeChanging = true;
                                        //检查设备Remote
                                        if (new BaseComm().InitialThreadWork("EQPOnlineCheck", ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE01, 5000, false, eProductMode.Semi))
                                        {
                                            BaseComm.LogMsg(Log.LogLevel.Info, string.Format("MES切换<{0}>模式切换进行中.", returnInfo.ProductionMode));
                                        }
                                    }
                                    else
                                    {
                                        EAPEnvironment.commonLibrary.commonModel.PreMode = EAPEnvironment.commonLibrary.commonModel.CurMode;
                                        EAPEnvironment.commonLibrary.commonModel.CurMode = EAP.Model.eProductMode.Semi;
                                        BaseComm.LogMsg(Log.LogLevel.Info, string.Format("MES切换<{0}>模式成功.", returnInfo.ProductionMode));
                                    }
                                }
                                break;
                            case "Auto1":
                                {
                                    bool isLocal = false;
                                    foreach (var em in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
                                    {
                                        if (em.ControlMode != eControlMode.ONLINE_REMOTE && (em.Type == "L" || em.Type == "U"))
                                        {
                                            em.ControlModeChangeResult = eCheckResult.wait;
                                            new HSMSReport().S2F41(em, ConstLibrary.CONST_COMMAND_Remote);
                                            isLocal = true;
                                        }
                                    }
                                    if (isLocal)
                                    {
                                        EAPEnvironment.commonLibrary.commonModel.isModeChanging = true;
                                        //检查设备Remote
                                        if (new BaseComm().InitialThreadWork("EQPOnlineCheck", ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE01, 5000, false, eProductMode.Auto1))
                                        {
                                            BaseComm.LogMsg(Log.LogLevel.Info, string.Format("MES切换<{0}>模式切换进行中.", returnInfo.ProductionMode));
                                        }
                                    }
                                    else
                                    {
                                        EAPEnvironment.commonLibrary.commonModel.PreMode = EAPEnvironment.commonLibrary.commonModel.CurMode;
                                        EAPEnvironment.commonLibrary.commonModel.CurMode = EAP.Model.eProductMode.Auto1;
                                        BaseComm.LogMsg(Log.LogLevel.Info, string.Format("MES切换<{0}>模式成功.", returnInfo.ProductionMode));
                                    }
                                }
                                break;
                            case "Auto2":
                                {
                                    bool isLocal = false;
                                    foreach (var em in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
                                    {
                                        if (em.ControlMode != eControlMode.ONLINE_REMOTE && (em.Type == "L" || em.Type == "U"))
                                        {
                                            em.ControlModeChangeResult = eCheckResult.wait;
                                            new HSMSReport().S2F41(em, ConstLibrary.CONST_COMMAND_Remote);
                                            isLocal = true;
                                        }
                                    }
                                    if (isLocal)
                                    {
                                        EAPEnvironment.commonLibrary.commonModel.isModeChanging = true;
                                        //检查设备Remote
                                        if (new BaseComm().InitialThreadWork("EQPOnlineCheck", ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE01, 5000, false, eProductMode.Auto2))
                                        {
                                            BaseComm.LogMsg(Log.LogLevel.Info, string.Format("MES切换<{0}>模式切换进行中.", returnInfo.ProductionMode));
                                        }
                                    }
                                    else
                                    {
                                        EAPEnvironment.commonLibrary.commonModel.PreMode = EAPEnvironment.commonLibrary.commonModel.CurMode;
                                        EAPEnvironment.commonLibrary.commonModel.CurMode = EAP.Model.eProductMode.Auto2;
                                        BaseComm.LogMsg(Log.LogLevel.Info, string.Format("MES切换<{0}>模式成功.", returnInfo.ProductionMode));
                                    }
                                }
                                break;
                            default:
                                break;
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
