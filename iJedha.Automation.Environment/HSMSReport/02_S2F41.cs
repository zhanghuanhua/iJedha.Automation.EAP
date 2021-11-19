using iJedha.HSMSMessageStructure;
using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Library;
using iJedha.Automation.EAP.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iJedha.Automation.EAP.ModelBase;
using iJedha.Automation.EAP.LibraryBase;

namespace iJedha.Automation.EAP.Environment
{
    public partial class HSMSReport
    {
        /// <summary>
        /// Host Command Send
        /// </summary>
        /// <param name="Equipment"></param>
        /// <param name="SysBytes"></param>
        public void S2F41(EquipmentModel em, string command, List<string> objValue = null, uint SysBytes = 0)
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

                TS2F41 SxFy = new TS2F41();
                SxFy.S2F41_L1.RCMD = command;
                DynamicLibraryBase dl = EAPEnvironment.commonLibrary.equipmentLibary.GetDynamicLibrary(em.EQID);
                RCMDModelBase rcmd = dl.GetRCMDModel(command);
                if (rcmd == null) return;
                if (rcmd.List_CPNameLink.Count > 0)
                {
                    if (objValue == null || objValue.Count == 0) return;
                    if (objValue.Count() != rcmd.List_CPNameLink.Count) return;
                    List<TS2F41_L1_L1> listCP = new List<TS2F41_L1_L1>();
                    int i = 0;
                    foreach (var _r in rcmd.List_CPNameLink)
                    {
                        TS2F41_L1_L1 t = new TS2F41_L1_L1();
                        if (em.ParseKey == eParseKey.YANBO && command == "PP-SELECT")
                        {
                            //扬博的特殊处理
                            t.S2F41_L1_L1_L1.CPNAAME = objValue[i];
                            t.S2F41_L1_L1_L1.CPVAL = "1";
                            t.S2F41_L1_L1_L1.CPTYPE = dl.GetCPName(_r.ToString()).DefaultUnit;
                        }
                        else
                        {
                            t.S2F41_L1_L1_L1.CPNAAME = dl.GetCPName(_r.ToString()).Name;
                            t.S2F41_L1_L1_L1.CPVAL = objValue[i];
                            t.S2F41_L1_L1_L1.CPTYPE = dl.GetCPName(_r.ToString()).DefaultUnit;
                        }
                        i++;
                        listCP.Add(t);
                    }
                    SxFy.S2F41_L1.S2F41_L1_L1 = listCP;
                }
                else
                {
                    SxFy.S2F41_L1.L1 = 0;
                }
                SxFy.Ack_item = command;
                BaseComm.SendSECSMessage(em.EQID, SxFy);
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0} {1}", ex.ToString(), ex.StackTrace.ToString()));
            }
        }
    }
}
