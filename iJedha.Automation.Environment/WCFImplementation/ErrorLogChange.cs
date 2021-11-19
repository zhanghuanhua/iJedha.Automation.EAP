using iJedha.Automation.EAP.Core;
using iJedha.Automation.MES.WcfServer;
using iJedha.WCFMessageStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iJedha.Automation.EAP.Environment
{
    public partial class WCFImplementation : BaseComm
    {
        public static void ErrorLogChange(DebugOutEventArgs msg)
        {
            LogMsg(Log.LogLevel.Warn, msg.Message);
        }
    }
}
