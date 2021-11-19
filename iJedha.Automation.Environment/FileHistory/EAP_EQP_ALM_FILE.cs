//******************************************************************
//   系统名称 : iJedha.Automation.EAP.HistoryService
//   文件概要 : History
//   作    者 : 张明哲
//   <更新履历>
//   1.0.0.0    2020/2/6 10:40:34
//******************************************************************
using iJedha.Automation.EAP.Model;
using System;

/// <summary>
/// EAP to DB  Interface
/// </summary>
namespace iJedha.Automation.EAP.Environment
{
    public partial class History
    {
        /// <summary>
        /// Save Alarm
        /// </summary>
        /// <param name="em"></param>
        /// <param name="_alarmID"></param>
        /// <param name="_alarmText"></param>
        /// <param name="_alarmLevel"></param>
        /// <param name="_isSet"></param>
        public static void EAP_EQP_ALM_FILE(Library.AlarmReport alarmReport,EquipmentModel em)
        {
            try
            {
                if (EAPEnvironment.commonLibrary.MQConnectedStatus == false)
                {
                    return;
                }
                string _sql = "";
                string report_action = string.Empty;
                string alarmType = alarmReport.alarm_type.ToUpper() == "REPORT" ? "1" : "0"; 
                #region oracle
                _sql = "insert into EAP_EQP_ALM(MAIN_EQP_ID, MAIN_EQP_NAME, SUB_EQP_ID, SUB_EQP_NAME, DATE_TIME, ALARM_ID, ALARM_TEXT, EQP_REPORT_TIME, ALARM_ACTION, ALARM_CATEGORY)" +
                                "values(" +
                                "'" + EAPEnvironment.commonLibrary.MDLN + "'," +
                                "'" + EAPEnvironment.commonLibrary.LineName + "'," +
                                "'" + em.EQID + "'," +
                                "'" + em.EQName + "'," +
                                "TO_DATE('" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "','YYYY/MM/DD HH24:MI:SS')," +
                                "'" + alarmReport.alarm_id + "'," +
                                "'" + alarmReport.alarm_text + "'," +
                                "TO_DATE('" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "','YYYY/MM/DD HH24:MI:SS')," +
                                "'" + alarmType + "'," +
                                "'" + alarmReport.alarm_level + "');";
                #endregion
                if (!EAPEnvironment.MQPublisherAp.MQ_DBData(Environment.EAPEnvironment.commonLibrary.baseLib.rbmqParaLibrary.QueueName, _sql))
                {

                }
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
            }
        }
    }
}
