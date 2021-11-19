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
        /// Online Data
        /// </summary>
        /// <param name="Equipment"></param>
        /// <param name="SysBytes"></param>
        public void S1F2_H(EquipmentModel em, uint SysBytes = 0)
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

                TS1F2_H SxFy = new TS1F2_H();
                SxFy.L1 = 0;
                SxFy.Systembyte = SysBytes;

                BaseComm.SendSECSMessage(em.EQID, SxFy, "", false, SysBytes.ToString());
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0} {1}", ex.ToString(), ex.StackTrace.ToString()));
            }
        }

        internal void S2F41(EquipmentModel em, object cONST_COMMAND_LotStart, List<string> list)
        {
            throw new NotImplementedException();
        }
    }
}
