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
        /// Trace Initialization Send
        /// </summary>
        /// <param name="Equipment"></param>
        /// <param name="SysBytes"></param>
        public void S2F23(EquipmentModel em, int trid, string stimer, bool enable, uint SysBytes = 0)
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
                DynamicLibraryBase dl = EAPEnvironment.commonLibrary.equipmentLibary.GetDynamicLibrary(em.EQID);
                string variableIDType = EAPEnvironment.commonLibrary.equipmentLibary.GetVariableIDType(em.EQID, eVariableID.SVID.ToString());

                TS2F23 SxFy = new TS2F23();
                SxFy.S2F23_L1.TRID = trid;
                SxFy.S2F23_L1.DSPER = stimer;                                      //em.SampleTimer.ToString().PadLeft(6, '0');   //抽样的频率
                SxFy.S2F23_L1.TOTSMP = enable ? 65535 : 0;                         //抽样的最大数量
                SxFy.S2F23_L1.REPGSZ = enable ? 1 : 0;                             //抽样的分组数量
                if (enable)
                {
                    List<TS2F23_L1_L1> listData = new List<TS2F23_L1_L1>();
                    IList<TraceDataModelBase> list_tm;
                    if (em.List_DefineTraceDateSpec.Count > 0)
                    {
                        list_tm = em.GetAllDefineTraceData(trid);
                    }
                    else
                    {
                        list_tm = dl.GetAllTraceDataModel(trid);
                    }

                    if (list_tm.Count == 0) return;

                    foreach (var _v in list_tm)
                    {
                        TS2F23_L1_L1 t = new TS2F23_L1_L1();
                        t.SVID = int.Parse(_v.ID);
                        t.SVTYPE = variableIDType;
                        listData.Add(t);
                    }
                    SxFy.S2F23_L1.S2F23_L1_L1 = listData;
                }
                SxFy.Ack_item = enable.ToString() + "," + trid;
                BaseComm.SendSECSMessage(em.EQID, SxFy);
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0} {1}", ex.ToString(), ex.StackTrace.ToString()));
            }
        }
    }
}
