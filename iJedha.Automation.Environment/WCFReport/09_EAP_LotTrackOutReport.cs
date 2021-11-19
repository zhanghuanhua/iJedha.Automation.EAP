using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Model;
using iJedha.Customized.MessageStructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;

namespace iJedha.Automation.EAP.Environment
{
    public partial class WCFReport : BaseComm
    {
        public void EAP_LotTrackOutReport(LotTrackOutReport _data, LotModel lot)
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
                    iJedha.MES.WcfServiceInterface.TrackInformation returnInfo = client.EAP_LotTrackOutReport(_data.MainEqpID, _data.SubEqpID, _data.PortID, _data.CarriedID, _data.LotID, _data.PanelTotalQty, _data.ProductList, _data.WIPDataList);

                    string _indata;
                    if (ConvertJSON(returnInfo, out _indata)) 
                        LogMsg(Log.LogLevel.Trace, string.Format("WCF Message<{0}> Recv OK, Content<{1}>", System.Reflection.MethodBase.GetCurrentMethod().Name, _indata));

                    if (returnInfo.CheckSuccessCode != "0")
                    {
                        string errMsg = string.Format("MES回复, 接口名称<{0}>, 错误代码<{1}>, 错误描述<{2}><{3}>", System.Reflection.MethodBase.GetCurrentMethod().Name, returnInfo.CheckSuccessCode, _data.SubEqpID, returnInfo.CheckErrorMsg);
                        Environment.BaseComm.ErrorHandleRule("E3004", errMsg);
                    }
                    else
                    {
                        lock (lot) lot.LotProcessStatus = eLotProcessStatus.Complete;

                        new WCFReport().EAP_UnloadRequest(new UnloadRequest()
                        {
                            MainEqpID = EAPEnvironment.commonLibrary.LineName,
                            SubEqpID = _data.SubEqpID,
                            PortID = _data.PortID,
                            CarriedID = _data.CarriedID,
                            RequesType = _data.ProductList.Count == 0 ? "Empty" : "Full"
                        });

                        LogMsg(Log.LogLevel.Info, string.Format("Lot<{0}>产品<{1}>数量<{2}>下机完成.", _data.LotID, lot.PN, _data.PanelTotalQty));

                        //new MQReport().MQ_LotInformation(_data.LotID, lot.PN, _data.ProductList.Count.ToString(), "下机完成");
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
