using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Model;
using iJedha.Customized.MessageStructure;
using System;
using System.ServiceModel;
using System.Linq;
using iJedha.MES.WcfServiceInterface;
using System.Collections.Generic;
using iJedha.Automation.EAP.ModelBase;
using System.Diagnostics;

namespace iJedha.Automation.EAP.Environment
{
    public partial class WCFReport : BaseComm
    {
        public void EAP_EqpDefineInfo(EqpDefineInfo _data)
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
                    iJedha.MES.WcfServiceInterface.EqpDefineInformation returnInfo = client.EAP_EqpDefineInfo(_data.SubEqpID);

                    string _indata;
                    if (ConvertJSON(returnInfo, out _indata))
                        LogMsg(Log.LogLevel.Trace, string.Format("WCF Message<{0}> Recv OK, Content<{1}>", System.Reflection.MethodBase.GetCurrentMethod().Name, _indata));

                    if (returnInfo.CheckSuccessCode != "0")
                    {
                        string errMsg = string.Format("MES回复, 接口名称<{0}>, 错误代码<{1}>, 错误描述<{2}><{3}>", System.Reflection.MethodBase.GetCurrentMethod().Name, returnInfo.CheckSuccessCode, _data.SubEqpID, returnInfo.CheckErrorMsg);
                        LogMsg(Log.LogLevel.Warn, errMsg);
                    }
                    else
                    {
                        EquipmentModel em = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByName(returnInfo.SubEqpID);
                        if (em == null)
                        {
                            LogMsg(Log.LogLevel.Warn, string.Format("设备<{0}> Define Trace Data Spec下载失败：设备错误.", returnInfo.SubEqpID));
                            em.traceDataSpecScenario = Model.eTraceDataSpecScenario.Step_ScenarioError;
                            return;
                        }

                        if (returnInfo.SubTraceDataList == null)
                        {
                            LogMsg(Log.LogLevel.Warn, string.Format("设备<{0}> Define Trace Data Spec下载失败：资料错误.", em.EQName));
                            em.traceDataSpecScenario = Model.eTraceDataSpecScenario.Step_ScenarioError;
                            return;
                        }


                        if (returnInfo.SubTraceDataList.Count == 0)
                        {
                            LogMsg(Log.LogLevel.Info, string.Format("设备<{0}> Define Trace Data Spec下载线束：无资料下载.", em.EQName));
                            em.traceDataSpecScenario = Model.eTraceDataSpecScenario.Step_EndTraceDataInitial;
                            return;
                        }


                        int i = 1;
                        em.List_DefineTraceDateSpec.Clear();
                        foreach (var v in returnInfo.SubTraceDataList)
                        {
                            em.List_DefineTraceDateSpec.Add(new ModelBase.TraceDataModelBase()
                            {
                                isEnable = true,
                                ID = v.SVID,
                                Name = v.WIPDataName,
                                Rule = v.TraceFactor,
                                TraceID = int.Parse(v.FrequencyGroup),
                                Sequence = i,
                            });
                            i++;
                        }
                        if (em.List_DefineTraceDateSpec.Count > 0)
                        {
                            var v = from n in returnInfo.SubTraceDataList group n by new { n.FrequencyGroup, n.FrequencyTime } into g select new { g.Key };
                            List<Tuple<string, string>> lst = new List<Tuple<string, string>>();
                            foreach (var vv in v)
                            {
                                lst.Add(new Tuple<string, string>(vv.Key.FrequencyGroup, vv.Key.FrequencyTime));
                            }
                            Environment.EAPEnvironment.commonLibrary.baseLib.traceDataGroupLibrary.Initial(em.EQName, lst);
                            LogMsg(Log.LogLevel.Info, string.Format("设备<{0}> Define Trace Data Spec下载完成.", em.EQName));
                        }
                        em.traceDataSpecScenario = Model.eTraceDataSpecScenario.Step_EndGetTraceDataSpec;
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
