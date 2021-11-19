using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iJedha.Automation.EAP.Environment
{
    public partial class WCFImplementation : BaseComm
    {
        public static void HostModeChange(string eqName, string mode)
        {
            Environment.EAPEnvironment.commonLibrary.commonModel.Mode = mode;

            LogMsg(Log.LogLevel.Info, string.Format("MES切换<{0}>模式.", mode));
            //switch (mode.Trim())
            //{
            //    case "Manual":
            //        break;
            //    case "Semi_Auto":
            //        break;
            //    case "Full_Auto":
            //        break;
            //    default:
            //        break;
            //}
        }
    }
}
