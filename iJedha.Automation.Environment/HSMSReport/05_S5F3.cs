using iJedha.HSMSMessageStructure;
using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iJedha.Automation.EAP.Environment
{
    public partial class HSMSReport
    {
        /// <summary>
        /// Enable/Disable Alarm Send
        /// </summary>
        /// <param name="Equipment"></param>
        /// <param name="SysBytes"></param>
        public void S5F3(EquipmentModel em, bool enable, string alarmID = "", uint SysBytes = 0)
        {
            try
            {
                string TransactionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

                if (!EAPEnvironment.commonLibrary.equipmentLibary.CheckEquipmentConnected(em.EQID))
                {
                    string sMsg = string.Format("SEND <{0}> skip, {1} HSMS is disconnect", TransactionName, em.EQID);
                    BaseComm.LogMsg(Log.LogLevel.Warn, sMsg);
                    return;
                }
                TS5F3 SxFy = new TS5F3();
                SxFy.S5F3_L1.ALED = enable ? 128 : 0;
                SxFy.S5F3_L1.ALID = string.IsNullOrEmpty(alarmID) ? 0 : int.Parse(alarmID);
                SxFy.S5F3_L1.ALTYPE = EAPEnvironment.commonLibrary.equipmentLibary.GetVariableIDType(em.EQID, eVariableID.ALID.ToString());
                BaseComm.SendSECSMessage(em.EQID, SxFy);
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0} {1}", ex.ToString(), ex.StackTrace.ToString()));
            }
        }
    }
}
