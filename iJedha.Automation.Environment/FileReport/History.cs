using iJedha.Automation.EAP.Library;
using iJedha.Automation.EAP.Model;
using iJedha.Automation.EAP.ModelBase;
using iJedha.HSMSMessageStructure;
using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// EAP to DB  Interface
/// </summary>
namespace iJedha.Automation.EAP.Environment
{
    public partial class History : BaseComm
    {
        public static void History_TraceData(EquipmentModel em, IList<TraceDataModelBase> list_tm, string level,string groupid)
        {
            try
            {
                //路径
                string sFolder = Path.Combine(Environment.EAPEnvironment.commonLibrary.baseLib.fsParaLibrary.FS_LocalPath, Environment.EAPEnvironment.commonLibrary.LineName);
                //文件名
                string sFileName = string.Format("{0:yyyyMMdd HH.mm.ss.ffff}^{1}^{2}.txt", DateTime.Now, ConstLibrary.CONST_FILE_TRACEDATA, em.EQName);
                string _lotID = string.Empty;
                string _recipeID = string.Empty;
                string _panelID = string.Empty;
                foreach (var v in em.List_PPID.Values)
                {
                    _recipeID = v;
                }
                _lotID = EAPEnvironment.commonLibrary.commonModel.currentProcessLotID;
                _panelID = EAPEnvironment.commonLibrary.commonModel.LcurrentProcessPanelID;//add by JS 20191102.1

                if (_panelID.ToUpper() == "ERROR")
                {
                    _panelID = string.Format("{0}_{1:yyyyMMddHHmmss}", _panelID, DateTime.Now);
                }
                string occurTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                string _sql = string.Empty;
                foreach (var v in list_tm)
                {
                    //sql语句
                    _sql = _sql + string.Format("insert into {0}(mainequipment_id,subequipment_id,date_time,item_id,item_name,item_value,lot_id,recipe_id,panel_id,data_level,group_id,e_id)" +
                                "values(" +
                                "'" + EAPEnvironment.commonLibrary.LineName + "'," +
                                "'" + em.EQName + "'," +
                                "TO_DATE('" + occurTime + "','YYYY/MM/DD HH24:MI:SS')," +
                                "'" + v.ID + "'," +
                                "'" + v.Name + "'," +
                                "'" + v.Value + "'," +
                                "'" + _lotID + "'," +
                                "'" + _recipeID + "'," +
                                "'" + _panelID + "'," +
                                "'" + level + "'," +
                                "'" + groupid + "'," +
                                "'" + string.Format("{0}-{1}", EAPEnvironment.commonLibrary.MDLN, em.EQNo) + "');\r\n", EAPEnvironment.commonLibrary.customizedLibrary.TraceDataDBName);
                }


                //new MQReport().MQ_DBData("History", _sql);
                iJedha.Automation.EAP.Core.DataAlgorithm.SaveDoctoTXT(_sql, sFolder, sFileName);
            }
            catch (Exception e)
            {
                LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0} {1}", e.ToString(), e.StackTrace.ToString()));
            }
        }

        public static void History_Alarm(EquipmentModel em, string _alarmID, string _alarmText, string _alarmLevel, bool _isSet)
        {

            try
            {
                //路径
                string sFolder = Path.Combine(Environment.EAPEnvironment.commonLibrary.baseLib.fsParaLibrary.FS_LocalPath, Environment.EAPEnvironment.commonLibrary.LineName);
                //文件名
                string sFileName = string.Format("{0:yyyyMMdd HH.mm.ss.ffff}^{1}^{2}.txt", DateTime.Now, ConstLibrary.CONST_FILE_ALARM, em.EQName);

                string _lotID = string.Empty;
                string _recipeID = string.Empty;
                string _panelID = string.Empty;

                foreach (var v in em.List_PPID.Values)
                {
                    _recipeID = v;
                }
                _lotID = EAPEnvironment.commonLibrary.commonModel.currentProcessLotID;
                _panelID = EAPEnvironment.commonLibrary.commonModel.LcurrentProcessPanelID;//add by JS 20191102.1
                if (_panelID.ToUpper() == "ERROR")
                {
                    _panelID = string.Format("{0}_{1:yyyyMMddHHmmss}", _panelID, DateTime.Now);
                }
                //sql语句
                string _sql = string.Empty;
                if (_isSet)
                {

                    _sql = "insert into EquipmentAlarm(EQU_NAME,PROC,ALARM_NAME,STARTTIME,ENDTIME,LOGDATE,EQU_ID,ALM_ID,EQU_TYPE,BU,lot_id,recipe_id,panel_id,CATEGORY_TYPE1)" +
                                    "values(" +
                                    "'" + em.EQName + "'," +
                                    "'" + string.Empty + "'," +
                                    "'" + strReplace(_alarmText) + "'," +
                                    "TO_DATE('" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "','YYYY/MM/DD HH24:MI:SS')," +
                                    "'" + string.Empty + "'," +
                                    "TO_DATE('" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "','YYYY/MM/DD HH24:MI:SS')," +
                                    "'" + em.EQID + "'," +
                                    "'" + _alarmID + "'," +
                                    "'" + string.Empty + "'," +
                                    "'" + "MSAP" + "'," +
                                    "'" + _lotID + "'," +
                                    "'" + _recipeID + "'," +
                                    "'" + _panelID + "'," +
                                    "'" + _alarmLevel + "');";
                }
                else
                {
                    _sql = "update EquipmentAlarm set ENDTIME = " +
                                    "TO_DATE('" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "','YYYY/MM/DD HH24:MI:SS')" + " where " +
                                    "ALM_ID = " + "'" + _alarmID + "' and " +
                                    "EQU_NAME = " + "'" + em.EQName + "' and " +
                                    "ENDTIME is null;";
                }

                //To File
                iJedha.Automation.EAP.Core.DataAlgorithm.SaveDoctoTXT(_sql, sFolder, sFileName);

                //To MQ
                //new MQReport().DB_DBData(System.Reflection.MethodBase.GetCurrentMethod().Name, _sql);
                //new MQReport().MQ_DBData("History", _sql);
                //EAPEnvironment.FtpAP.UpLoadFile(Path.Combine(sFolder, sFileName), EAPEnvironment.commonLibrary.LineName + "\\");
            }
            catch (Exception e)
            {
                LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0} {1}", e.ToString(), e.StackTrace.ToString()));
            }
        }

        public static void History_Event(string _eqName, TS6F11 s6f11)
        {

            try
            {
                #region iJDEAP_0019
                string[] vid = { "", "", "", "", "" };
                int i = 0;
                if (s6f11.S6F11_L1.L1>0)
                {
                    if (s6f11.S6F11_L1.S6F11_L1_L1[0].L1>0)
                    {
                        foreach (var s in s6f11.S6F11_L1.S6F11_L1_L1[0].S6F11_L1_L1_L1.S6F11_L1_L1_L1_L1)
                        {
                            vid[i]= s.V.ToString();
                            i++;
                            if (i > 4) break;                         
                        }
                    }
                }
                #endregion

                //路径
                string sFolder = Path.Combine(Environment.EAPEnvironment.commonLibrary.baseLib.fsParaLibrary.FS_LocalPath, Environment.EAPEnvironment.commonLibrary.LineName);
                //文件名
                string sFileName = string.Format("{0:yyyyMMdd HH.mm.ss.ffff}^{1}^{2}.txt", DateTime.Now, ConstLibrary.CONST_FILE_EVENT, _eqName);

                string _lotID = string.Empty;
                string _panelID = string.Empty;
                _lotID = EAPEnvironment.commonLibrary.commonModel.currentProcessLotID;
                _panelID = EAPEnvironment.commonLibrary.commonModel.LcurrentProcessPanelID;//add by JS 20191102.1
                //if (_eqName.Contains("收板机"))//add by JS 20191102.1
                //    _panelID = EAPEnvironment.commonLibrary.commonModel.UcurrentProcessPanelID;
                //else
                //    _panelID = EAPEnvironment.commonLibrary.commonModel.LcurrentProcessPanelID;
                if (_panelID.ToUpper() == "ERROR")
                {
                    _panelID = string.Format("{0}_{1:yyyyMMddHHmmss}", _panelID, DateTime.Now);
                }
                //sql语句
                string _sql = "insert into EquipmentFlowCollection(mainequipment_id,subequipment_id,date_time,CEID,lot_id,panel_id,dataitem01,dataitem02,dataitem03,dataitem04,dataitem05)" +
                                "values(" +
                                "'" + EAPEnvironment.commonLibrary.LineName + "'," +
                                "'" + _eqName + "'," +
                                "TO_DATE('" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "','YYYY/MM/DD HH24:MI:SS')," +
                                "'" + s6f11.S6F11_L1.CEID.ToString() + "'," +
                                "'" + _lotID + "'," +
                                "'" + _panelID + "'," +
                                "'" + vid[0] + "'," +
                                "'" + vid[1] + "'," +
                                "'" + vid[2] + "'," +
                                "'" + vid[3] + "'," +
                                "'" + vid[4] + "');";
                //new MQReport().MQ_DBData("History", _sql);
                iJedha.Automation.EAP.Core.DataAlgorithm.SaveDoctoTXT(_sql, sFolder, sFileName);

            }
            catch (Exception e)
            {
                LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0} {1}", e.ToString(), e.StackTrace.ToString()));
            }
        }

        public static void History_IMSData(EquipmentModel em, TS6F11 s6f11)
        {

            try
            {
                //路径
                string sFolder = Path.Combine(Environment.EAPEnvironment.commonLibrary.baseLib.fsParaLibrary.FS_LocalPath, Environment.EAPEnvironment.commonLibrary.LineName);
                //文件名
                string sFileName = string.Format("{0:yyyyMMdd HH.mm.ss.ffff}^{1}^{2}.txt", DateTime.Now, ConstLibrary.CONST_FILE_IMSDATA, em.EQName);

                string occurTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                //sql语句
                string _sql = string.Empty;

                foreach (var v in s6f11.S6F11_L1.S6F11_L1_L1[0].S6F11_L1_L1_L1.S6F11_L1_L1_L1_L1)
                {
                    string[] _data = v.V.Split(',');
                    if (_data.Length != 2) continue;

                    _sql = _sql + "insert into EQUIPMENTCHGDATA(MAINEQUIPMENT_ID,SUBEQUIPMENT_ID,DATE_TIME,EQU_STATUS,EQU_MODE,CEID,ITEM_ID,ITEM_NAME,ITEM_VALUE,L_ID,E_ID)" +
                                    "values(" +
                                    "'" + EAPEnvironment.commonLibrary.LineName + "'," +
                                    "'" + em.EQName + "'," +
                                    "TO_DATE('" + occurTime + "','YYYY/MM/DD HH24:MI:SS')," +
                                    "'" + em.EQStatus.ToString() + "'," +
                                    "'" + em.OperationMode.ToString() + "'," +
                                    "'" + s6f11.S6F11_L1.CEID.ToString() + "'," +
                                    "'" + string.Format("{0}_{1}", em.UID, _data[0]) + "'," +
                                    "'" + string.Empty + "'," +
                                    "'" + _data[1] + "'," +
                                    "'" + EAPEnvironment.commonLibrary.UID + "'," +
                                    "'" + em.UID + "');\r\n";

                }
                //new MQReport().MQ_DBData("History", _sql);
                iJedha.Automation.EAP.Core.DataAlgorithm.SaveDoctoTXT(_sql, sFolder, sFileName);

            }
            catch (Exception e)
            {
                LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0} {1}", e.ToString(), e.StackTrace.ToString()));
            }
        }
        public static void History_ProcessData(EquipmentModel em, List<TS6F3_L1_L1_L1_L1> vList)
        {

            try
            {
                //路径 
                string sFolder = Path.Combine(Environment.EAPEnvironment.commonLibrary.baseLib.fsParaLibrary.FS_LocalPath, Environment.EAPEnvironment.commonLibrary.LineName);
                //文件名
                string sFileName = string.Format("{0:yyyyMMdd HH.mm.ss.ffff}^{1}^{2}.txt", DateTime.Now, ConstLibrary.CONST_FILE_PROCESSDATA, em.EQName);
                string _lotID = string.Empty;
                string _panelID = string.Empty;
                _lotID = EAPEnvironment.commonLibrary.commonModel.currentProcessLotID;
                _panelID = EAPEnvironment.commonLibrary.commonModel.LcurrentProcessPanelID;//add by JS 20191102.1

                if (_panelID.ToUpper() == "ERROR")
                {
                    _panelID = string.Format("{0}_{1:yyyyMMddHHmmss}", _panelID, DateTime.Now);
                }

                string occurTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                //sql语句
                string _sql = string.Empty;

                foreach (var v in vList)
                {
                    _sql = _sql + string.Format("insert into {0}(MAINEQUIPMENT_ID,SUBEQUIPMENT_ID,DATE_TIME,ITEM_NAME,ITEM_VALUE,LOT_ID,RECIPE_ID,PANEL_ID,L_ID,E_ID)" +
                                    "values(" +
                                    "'" + EAPEnvironment.commonLibrary.LineName + "'," +
                                    "'" + em.EQName + "'," +
                                    "TO_DATE('" + occurTime + "','YYYY/MM/DD HH24:MI:SS')," +
                                    "'" + v.S6F3_L1_L1_L1_L1_L1.DVNAME1 + "'," +
                                    "'" + v.S6F3_L1_L1_L1_L1_L1.DVVAL1 + "'," +
                                    "'" + _lotID + "'," +
                                    "'" + em.PPID + "'," +
                                    "'" + _panelID + "'," +
                                    "'" + EAPEnvironment.commonLibrary.UID + "'," +
                                    "'" + em.UID + "');\r\n", EAPEnvironment.commonLibrary.customizedLibrary.ProcessDataDBName);

                }
                //new MQReport().MQ_DBData("History", _sql);
                iJedha.Automation.EAP.Core.DataAlgorithm.SaveDoctoTXT(_sql, sFolder, sFileName);

            }
            catch (Exception e)
            {
                LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0} {1}", e.ToString(), e.StackTrace.ToString()));
            }
        }
        public static void History_ProcessData_SiXian(EquipmentModel em, ProcessDataForSiXian ProcessData)//add by JS 20191123
        {

            try
            {
                //路径
                string sFolder = Path.Combine(Environment.EAPEnvironment.commonLibrary.baseLib.fsParaLibrary.FS_LocalPath, Environment.EAPEnvironment.commonLibrary.LineName);
                //文件名
                string sFileName = string.Format("{0:yyyyMMdd HH.mm.ss.ffff}^{1}^{2}.txt", DateTime.Now, ConstLibrary.CONST_FILE_PROCESSDATA, em.EQName);
                //sql语句
                string _sql = string.Empty;

                _sql = _sql + string.Format("insert into {0}(SUBEQUIPMENT_ID,DATE_TIME,TDID,BINCODE,PIECEX,PIECEY,PIECENO,TESTPOINT,CHECKSTATUS,CHECK_TIME)" +
                                "values(" +
                                "'" + ProcessData.subequipment_id + "'," +
                                "TO_DATE('" + ProcessData.date_time + "','YYYY/MM/DD HH24:MI:SS')," +
                                "'" + ProcessData.TwoDID + "'," +
                                "'" + ProcessData.BinCode + "'," +
                                "'" + ProcessData.PieceX + "'," +
                                "'" + ProcessData.PieceY + "'," +
                                "'" + ProcessData.PieceNo + "'," +
                                "'" + ProcessData.TestPoint + "'," +
                                "'" + "0" + "'," +
                                "'" + "" + "');\r\n", "PROCESS_SIXIAN");

                //new MQReport().MQ_DBData("Process_History", _sql);
                iJedha.Automation.EAP.Core.DataAlgorithm.SaveDoctoTXT(_sql, sFolder, sFileName);

            }
            catch (Exception e)
            {
                LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0} {1}", e.ToString(), e.StackTrace.ToString()));
            }
        }     
        public static void EAPGUIErrorMsg(ErrorCodeModelBase errm, string Error_Detail)
        {
            try
            {
                string ErrorCode = string.Empty;
                string Error_Category = string.Empty;
                string Error_Desc = string.Empty;
                string Category_Type = string.Empty;
                string Error_Solve = string.Empty;

                ErrorCode = errm.ErrorCode;
                Error_Category = errm.ErrorCategory;
                Error_Desc = errm.ErrorDesc;
                Category_Type = errm.ErrorCategory;
                Error_Solve = errm.ErrorSolve;
                //sql語句
                string sql = "insert into EAPGUIERRORMSG(ERRORCODE,DATE_TIME,ERROR_CATEGORY,ERROR_DESC,ERROR_DETAIL,CATEGORY_TYPE,ERROR_SOLVE,MAINEQUIPMENT_ID)" +
                             "values(" +
                             "'" + ErrorCode + "'," +
                             "TO_DATE('" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "','YYYY/MM/DD HH24:MI:SS')," +
                             "'" + Error_Category + "'," +
                             "'" + Error_Desc + "'," +
                             "'" + Error_Detail + "'," +
                             "'" + Category_Type + "'," +
                             "'" + Error_Solve.Replace(";",",") + "'," +
                             "'" + EAPEnvironment.commonLibrary.LineName + "');";
                //new MQReport().MQ_DBData("History", sql);//add by JS 20191105.0 
            }
            catch (Exception e)
            {
                LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0} {1}", e.ToString(), e.StackTrace.ToString()));
            }
        }
        public static string strReplace(string str)
        {
            string temp = str.Replace("'", "''");
            return temp;
        }
    }
}
