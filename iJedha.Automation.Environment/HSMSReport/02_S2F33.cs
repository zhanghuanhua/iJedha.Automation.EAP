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
        /// Define Report
        /// </summary>
        /// <param name="Equipment"></param>
        /// <param name="SysBytes"></param>
        public void S2F33(EquipmentModel em, eDynamicLinkStep step, uint SysBytes = 0)
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
                string dataIDType = EAPEnvironment.commonLibrary.equipmentLibary.GetVariableIDType(em.EQID, eVariableID.DATAID.ToString());
                TS2F33 SxFy = new TS2F33();
                if (step == eDynamicLinkStep.DELETE)
                {
                    SxFy.L1 = 2;
                    SxFy.S2F33_L1.DATAID = em.DATAID;
                    SxFy.S2F33_L1.DATATYPE = dataIDType;
                    SxFy.S2F33_L1.L1 = 0;
                }
                else if (step == eDynamicLinkStep.INITIAL)
                {
                    SxFy.L1 = 2;
                    SxFy.S2F33_L1.DATAID = em.GenerateDataID();
                    SxFy.S2F33_L1.DATATYPE = dataIDType;
                    List<TS2F33_L1_L1> listReport = new List<TS2F33_L1_L1>();
                    DynamicLibraryBase dl = EAPEnvironment.commonLibrary.equipmentLibary.GetDynamicLibrary(em.EQID);
                    string rtpIDType = EAPEnvironment.commonLibrary.equipmentLibary.GetVariableIDType(em.EQID, eVariableID.RPTID.ToString());
                    string vIDType = EAPEnvironment.commonLibrary.equipmentLibary.GetVariableIDType(em.EQID, eVariableID.VID.ToString());
                    IList<ReportModelBase> list_tm = dl.GetAllReportLink();
                    foreach (var _r in list_tm)
                    {
                        TS2F33_L1_L1 t = new TS2F33_L1_L1();
                        t.S2F33_L1_L1_L1.RPTID = int.Parse(_r.ID);
                        t.S2F33_L1_L1_L1.RPTTYPE = rtpIDType;
                        List<TS2F33_L1_L1_L1_L1> listSV = new List<TS2F33_L1_L1_L1_L1>();
                        foreach (var _v in _r.List_VariableLink)
                        {
                            TS2F33_L1_L1_L1_L1 tt = new TS2F33_L1_L1_L1_L1();
                            tt.VID = int.Parse(_v);
                            tt.VTYPE = vIDType;
                            listSV.Add(tt);
                        }
                        t.S2F33_L1_L1_L1.S2F33_L1_L1_L1_L1 = listSV;
                        listReport.Add(t);
                    }
                    SxFy.S2F33_L1.S2F33_L1_L1 = listReport;
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
