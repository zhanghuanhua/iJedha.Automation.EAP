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
        /// Request ON-LINE
        /// </summary>
        /// <param name="Equipment"></param>
        /// <param name="SysBytes"></param>
        public void S1F17(EquipmentModel em, uint SysBytes = 0)
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
                TS1F17 SxFy = new TS1F17();

                BaseComm.SendSECSMessage(em.EQID, SxFy);


            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0} {1}", ex.ToString(), ex.StackTrace.ToString()));
            }
        }
    }
}
