using iJedha.HSMSMessageStructure;
using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Library;
using iJedha.Automation.EAP.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iJedha.Automation.EAP.LibraryBase;
using iJedha.Automation.EAP.ModelBase;

namespace iJedha.Automation.EAP.Environment
{
    public partial class HSMSReport
    {
        /// <summary>
        /// Enable/Disable Event Report
        /// </summary>
        /// <param name="Equipment"></param>
        /// <param name="SysBytes"></param>
        public void S2F37(EquipmentModel em, bool enable, eDynamicLinkStep step, uint SysBytes = 0)
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
                TS2F37 SxFy = new TS2F37();
                if (step == eDynamicLinkStep.DELETE)
                {
                    SxFy.S2F37_L1.CEED = 0;
                    SxFy.L1 = 0;
                }
                else if (step == eDynamicLinkStep.INITIAL)
                {
                    SxFy.S2F37_L1.CEED = enable ? 1 : 0;
                    List<TS2F37_L1_L1> listCEID = new List<TS2F37_L1_L1>();
                    DynamicLibraryBase dl = EAPEnvironment.commonLibrary.equipmentLibary.GetDynamicLibrary(em.EQID);
                    string ceIDType = EAPEnvironment.commonLibrary.equipmentLibary.GetVariableIDType(em.EQID, eVariableID.CEID.ToString());

                    IList<EventModelBase> list_tm = dl.GetAllCEIDLink();
                    foreach (var _r in list_tm)
                    {
                        if (_r.isEnable = enable)
                        {
                            TS2F37_L1_L1 t = new TS2F37_L1_L1();
                            t.CEID = int.Parse(_r.ID);
                            t.CETYPE = ceIDType;
                            listCEID.Add(t);
                        }
                    }
                    SxFy.S2F37_L1.S2F37_L1_L1 = listCEID;
                }
                BaseComm.SendSECSMessage(em.EQID, SxFy);
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0} {1}", ex.ToString(), ex.StackTrace.ToString()));
            }
        }
    }
}
