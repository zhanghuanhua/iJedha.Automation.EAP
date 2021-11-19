using iJedha.Automation.EAP.Core;
using iJedha.WCFMessageStructure;
using System;
using System.ServiceModel;

namespace iJedha.Automation.EAP.Environment
{
    public partial class WCFReport : BaseComm
    {
        public void EAP_EAPModeRequest(ModeChangeRequest _data)
        {
            iJedha.MES.WcfService.IWcfServiceContract client = null;
            try
            {
                if (EAPEnvironment.commonLibrary.HostwcfParaLibrary.HostConnectMode == Library.eHostConnectMode.DISCONNECT)
                {
                    //string sMsg = string.Format("SEND <{0}> skip, MES WCF is disconnect", System.Reflection.MethodBase.GetCurrentMethod().Name);
                    //BaseComm.LogMsg(Log.LogLevel.Warn, sMsg);
                    return;
                }

                string _outdata;
                if (ConvertJSON(_data, out _outdata))
                    LogMsg(Log.LogLevel.Trace, string.Format("WCF Message<{0}> Send OK, Content<{1}>", System.Reflection.MethodBase.GetCurrentMethod().Name, _outdata));

                using (ChannelFactory<iJedha.MES.WcfService.IWcfServiceContract> channel = new ChannelFactory<iJedha.MES.WcfService.IWcfServiceContract>(new NetTcpBinding(SecurityMode.None), new EndpointAddress(EAPEnvironment.commonLibrary.HostwcfParaLibrary.RemoteUrlString)))
                {
                    client = channel.CreateChannel();
                    //(client as ICommunicationObject).Closed += delegate
                    //{

                    //};
                    iJedha.MES.WcfService.ReturnInformation returnInfo = client.EAP_ModeChangeRequest(_data.MainEqpID, _data.SubEqpID, _data.Mode);

                    string _indata;
                    if (ConvertJSON(returnInfo, out _indata))
                        LogMsg(Log.LogLevel.Trace, string.Format("WCF Message<{0}> Recv OK, Content<{1}>", System.Reflection.MethodBase.GetCurrentMethod().Name, _indata));

                    if (returnInfo.CheckSuccessCode != "0")
                    {
                        LogMsg(Log.LogLevel.Error, string.Format("WCF Message<{0}> CheckErrorCode<{1}> CheckErrorMsg<{2}>", System.Reflection.MethodBase.GetCurrentMethod().Name, returnInfo.CheckSuccessCode, returnInfo.CheckErrorMsg));
                    }

                }
            }
            catch (TimeoutException e)
            {
                (client as ICommunicationObject).Abort();
                string err;
                Environment.EAPEnvironment.WcfClient_MES.reStart(out err);
                LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0} {1}", e.ToString(), e.StackTrace.ToString()));
            }
            catch (CommunicationException e)
            {
                (client as ICommunicationObject).Abort();
                string err;
                Environment.EAPEnvironment.WcfClient_MES.reStart(out err);
                LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0} {1}", e.ToString(), e.StackTrace.ToString()));
            }
            catch (Exception e)
            {
                (client as ICommunicationObject).Close();
                string err;
                Environment.EAPEnvironment.WcfClient_MES.reStart(out err);
                LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0} {1}", e.ToString(), e.StackTrace.ToString()));
            }
        }
    }
}
