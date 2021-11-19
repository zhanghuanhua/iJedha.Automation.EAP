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
        public static void HoldModeChange(string eqName, bool isHold)
        {
            EquipmentModel em = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByName(eqName);
            //lock (em)
            //{
            //    em.isEquipmentHold = isHold;
            //}
            if (isHold)
            {
                LogMsg(Log.LogLevel.Info, string.Format("MES通知<{0}>停止投片.", em.EQName));
                new HSMSReport().S2F41(em, "Stop");
            }
            else
            {
                LogMsg(Log.LogLevel.Info, string.Format("MES通知<{0}>恢复投片.", em.EQName));
                new HSMSReport().S2F41(em, "Start");
            }
        }
    }
}
