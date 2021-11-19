using iJedha.Automation.EAP.Core;
using iJedha.Customized.MessageStructure;

namespace iJedha.Automation.EAP.Environment
{
    public partial class WCFImplementation : BaseComm
    {
        public static void ConnectModeChange(bool isconnected)
        {
            EAPEnvironment.commonLibrary.HostConnectMode = isconnected ? LibraryBase.eHostConnectMode.CONNECT : LibraryBase.eHostConnectMode.DISCONNECT;
            if (isconnected)
            {
                LogMsg(Log.LogLevel.Info, "MES WCF服务端连接成功.");
                //new Environment.WCFReport().EAP_AliveCheckRequest(new AliveCheckRequest()
                //{
                //    MainEqpID = Environment.EAPEnvironment.commonLibrary.LineName,
                //    IPAddress = Environment.EAPEnvironment.commonLibrary.baseLib.wcfParaLibrary.LocalIP
                //});
            }
            else
            {
                string errMsg = string.Format("MES WCF服务端连接断开.");
                Environment.BaseComm.ErrorHandleRule("E0004", errMsg);
            }
        }
    }
}
