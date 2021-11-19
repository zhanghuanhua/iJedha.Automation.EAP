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
        /// Selected Equipment Status Data
        /// type:
        ///3. Panel；2. Lot；1. Trace Data(频率)
        /// </summary>
        /// <param name="Equipment"></param>
        /// <param name="SysBytes"></param>
        public void S1F3(EquipmentModel em, int type, uint SysBytes = 0)
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
                TS1F3 SxFy = new TS1F3();
                DynamicLibraryBase dl = EAPEnvironment.commonLibrary.equipmentLibary.GetDynamicLibrary(em.EQID);
                string variableIDType = EAPEnvironment.commonLibrary.equipmentLibary.GetVariableIDType(em.EQID, eVariableID.SVID.ToString());
                #region Initial
                if (type == 0)
                {
                    #region Initial
                    IList<VariableModelBase> list_tm = dl.GetAllVariableModel();
                    List<TS1F3_L1> listData = new List<TS1F3_L1>();
                    foreach (var _v in list_tm)
                    {
                        TS1F3_L1 t = new TS1F3_L1();
                        t.SVID = int.Parse(_v.ID);
                        t.SVTYPE = variableIDType;
                        listData.Add(t);
                    }
                    SxFy.S1F3_L1 = listData;
                    SxFy.Ack_item = type.ToString();
                    #endregion
                }
                #endregion
                #region all
                if (type == 1)
                {
                    if (em.List_KeyTraceDateSpec.Count > 0)
                    {
                        List<ParameterModel> list_tm = em.List_KeyTraceDateSpec;
                        List<TS1F3_L1> listData = new List<TS1F3_L1>();
                        foreach (var _v in list_tm)
                        {
                            TS1F3_L1 t = new TS1F3_L1();
                            t.SVID = int.Parse(_v.ItemID);
                            t.SVTYPE = variableIDType;
                            listData.Add(t);
                        }
                        SxFy.S1F3_L1 = listData;
                        SxFy.Ack_item = type.ToString();
                    }
                    else
                    {
                        if (dl.GetAllTraceDataModel(0).Count > 0)
                        {
                            IList<TraceDataModelBase> list_tm = dl.GetAllTraceDataModel(0);
                            List<TS1F3_L1> listData = new List<TS1F3_L1>();
                            foreach (var _v in list_tm)
                            {
                                TS1F3_L1 t = new TS1F3_L1();
                                t.SVID = int.Parse(_v.ID);
                                t.SVTYPE = variableIDType;
                                listData.Add(t);
                            }
                            SxFy.S1F3_L1 = listData;
                            SxFy.Ack_item = type.ToString();
                        }
                    }

                    #region no use
                    //if (em.TraceDataSpecSource == eTraceDataSource.Local)
                    //{
                    //    IList<TraceDataModelBase> list_tm = dl.GetAllTraceDataModel(0);
                    //    List<TS1F3_L1> listData = new List<TS1F3_L1>();
                    //    foreach (var _v in list_tm)
                    //    {
                    //        TS1F3_L1 t = new TS1F3_L1();
                    //        t.SVID = int.Parse(_v.ID);
                    //        t.SVTYPE = variableIDType;
                    //        listData.Add(t);
                    //    }
                    //    SxFy.S1F3_L1 = listData;
                    //    SxFy.Ack_item = type.ToString();
                    //}
                    //else
                    //{
                    //    List<ParameterModel> list_tm = em.List_DefineTraceDateSpec;
                    //    List<TS1F3_L1> listData = new List<TS1F3_L1>();
                    //    foreach (var _v in list_tm)
                    //    {
                    //        TS1F3_L1 t = new TS1F3_L1();
                    //        t.SVID = int.Parse(_v.ItemID);
                    //        t.SVTYPE = variableIDType;
                    //        listData.Add(t);
                    //    }
                    //    SxFy.S1F3_L1 = listData;
                    //    SxFy.Ack_item = type.ToString();
                    //}
                    #endregion
                }
                #endregion
                #region Key
                if (type == 2 || type == 3)
                {
                    List<ParameterModel> list_tm = em.List_KeyTraceDateSpec;
                    List<TS1F3_L1> listData = new List<TS1F3_L1>();
                    foreach (var _v in list_tm)
                    {
                        TS1F3_L1 t = new TS1F3_L1();
                        t.SVID = int.Parse(_v.ItemID);
                        t.SVTYPE = variableIDType;
                        listData.Add(t);
                    }
                    SxFy.S1F3_L1 = listData;
                    SxFy.Ack_item = type.ToString();
                }
                #endregion
                BaseComm.SendSECSMessage(em.EQID, SxFy);
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0} {1}", ex.ToString(), ex.StackTrace.ToString()));
            }
        }
    }
}
