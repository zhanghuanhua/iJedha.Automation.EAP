using iJedha.Automation.EAP.Core;
using iJedha.Customized.MessageStructure;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace iJedha.Automation.EAP.Environment
{
    public partial class WCFReport : BaseComm
    {
        public void EAP_WIPMappingCarrier(WIPMappingCarrier _data)
        {
            iJedha.MES.WcfServiceInterface.IWcfServiceContract client = null;
            try
            {
                if (EAPEnvironment.commonLibrary.HostwcfParaLibrary.HostConnectMode == LibraryBase.eHostConnectMode.DISCONNECT)
                {
                    return;
                }

                string _outdata;
                if (ConvertJSON(_data, out _outdata))
                    LogMsg(Log.LogLevel.Trace, string.Format("WCF Message<{0}> Send OK, Content<{1}>", System.Reflection.MethodBase.GetCurrentMethod().Name, _outdata));


                using (ChannelFactory<iJedha.MES.WcfServiceInterface.IWcfServiceContract> channel = new ChannelFactory<iJedha.MES.WcfServiceInterface.IWcfServiceContract>(new NetTcpBinding(SecurityMode.None), new EndpointAddress(EAPEnvironment.commonLibrary.HostwcfParaLibrary.RemoteUrlString)))
                {
                    client = channel.CreateChannel();
                    iJedha.MES.WcfServiceInterface.ReturnInformation returnInfo = client.EAP_WIPMappingCarrier(_data.MainEqpID, _data.SubEqpID, _data.CarriedID, _data.LotID, _data.ProductList);

                    string _indata;
                    if (ConvertJSON(returnInfo, out _indata))
                        LogMsg(Log.LogLevel.Trace, string.Format("WCF Message<{0}> Recv OK, Content<{1}>", System.Reflection.MethodBase.GetCurrentMethod().Name, _indata));
                    if (returnInfo.CheckSuccessCode != "0")
                    {
                        LogMsg(Log.LogLevel.Warn, string.Format("MES回复<{0}>异常：{1} {2}.", System.Reflection.MethodBase.GetCurrentMethod().Name, returnInfo.CheckSuccessCode, returnInfo.CheckErrorMsg));
                    }
                }
            }
            catch (TimeoutException e)
            {
                LogMsg(Log.LogLevel.Warn, string.Format("MES回复<{0}>超时.", System.Reflection.MethodBase.GetCurrentMethod().Name));
                LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0} {1}", e.ToString(), e.StackTrace.ToString()));
            }
            catch (FaultException e)
            {
                LogMsg(Log.LogLevel.Warn, string.Format("MES回复<{0}>异常：程式出错.", System.Reflection.MethodBase.GetCurrentMethod().Name));
                LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0} {1}", e.ToString(), e.StackTrace.ToString()));
            }
            catch (CommunicationException e)
            {
                LogMsg(Log.LogLevel.Warn, string.Format("MES回复<{0}>异常：通讯中断，尝试重连.", System.Reflection.MethodBase.GetCurrentMethod().Name));
                (client as ICommunicationObject).Abort();
                Environment.EAPEnvironment.WcfClient_MES.reStart();
                LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0} {1}", e.ToString(), e.StackTrace.ToString()));
            }
            catch (Exception e)
            {
                LogMsg(Log.LogLevel.Warn, string.Format("EAP发送<{0}>异常：程式出错.", System.Reflection.MethodBase.GetCurrentMethod().Name));
                LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0} {1}", e.ToString(), e.StackTrace.ToString()));
            }
        }
    }
}
