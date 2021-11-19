using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Library;
using iJedha.Customized.MessageStructure;
using System;
using System.Diagnostics;
using System.ServiceModel;

namespace iJedha.Automation.EAP.Environment
{
    public partial class WCFReport : BaseComm
    {
        public void EAP_AliveCheckRequest(AliveCheckRequest _data)
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
                    iJedha.MES.WcfServiceInterface.ReturnInformation returnInfo = client.EAP_AliveCheckRequest(_data.MainEqpID, _data.IPAddress);
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
                        bool isUIDNG = false;
                        //TBD
                        foreach (var v in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
                        {
                            if (v.isGenerateHistoryFile && v.OnlineScenarioStep==Model.eOnlineScenarioStep.Initial && v.traceDataSpecScenario == Model.eTraceDataSpecScenario.Step_ScenarioInitial)
                            {
                                if (new BaseComm().InitialThreadWork("TraceDataSpecScenario", ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_SCENARIO, 2000, true, v))
                                {
                                    v.traceDataSpecScenario = Model.eTraceDataSpecScenario.Step_ScenarioStart;
                                    v.traceDataSpecScenarioTime = DateTime.Now;
                                    LogMsg(Log.LogLevel.Info, string.Format("设备<{0}>TraceDataSpec流程开始......", v.EQName));
                                }
                            }

                            if (string.IsNullOrEmpty(v.UID)) isUIDNG = true;
                        }

                        if (isUIDNG)
                        {
                            EAP_GetEqpUID(new EqpUIDRequest() { MainEqpID = EAPEnvironment.commonLibrary.LineName });
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
