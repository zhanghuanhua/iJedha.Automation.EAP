using iJedha.HSMSMessageStructure;
using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Model;
using System;
using System.Collections.Generic;
using iJedha.Automation.EAP.ModelBase;

namespace iJedha.Automation.EAP.Environment
{
    public partial class HSMSReport
    {
        /// <summary>
        /// Formatted Process Program Send
        /// </summary>
        /// <param name="Equipment"></param>
        /// <param name="SysBytes"></param>
        public void S7F23(EquipmentModel em, string ppid, SortedList<string, PPParameterModelBase> list_Body, uint SysBytes = 0)
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
                TS7F23_H SxFy = new TS7F23_H();
                SxFy.S7F23_H_L1.PPID = ppid;
                SxFy.S7F23_H_L1.MDLN = "";
                SxFy.S7F23_H_L1.SOFTREV = "";
                List<TS7F23_H_L1_L1> listName = new List<TS7F23_H_L1_L1>();
                foreach (var v in list_Body)
                {
                    TS7F23_H_L1_L1 t = new TS7F23_H_L1_L1();
                    t.S7F23_H_L1_L1_L1.CCODE = v.Key;
                    List<TS7F23_H_L1_L1_L1_L1> listData = new List<TS7F23_H_L1_L1_L1_L1>();
                    TS7F23_H_L1_L1_L1_L1 tt = new TS7F23_H_L1_L1_L1_L1();
                    tt.PPARM1 = v.Value.Value;
                    tt.PPARMTYPE = v.Value.DefaultUnit;
                    listData.Add(tt);
                    t.S7F23_H_L1_L1_L1.S7F23_H_L1_L1_L1_L1 = listData;
                    listName.Add(t);
                }
                SxFy.S7F23_H_L1.S7F23_H_L1_L1 = listName;

                BaseComm.SendSECSMessage(em.EQID, SxFy);
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0} {1}", ex.ToString(), ex.StackTrace.ToString()));
            }
        }
    }
}
