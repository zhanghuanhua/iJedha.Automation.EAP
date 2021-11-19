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
        /// Save Test Result
        /// </summary>
        /// <param name="em"></param>
        /// <param name="_alarmID"></param>
        /// <param name="_alarmText"></param>
        /// <param name="_alarmLevel"></param>
        /// <param name="_isSet"></param>
        public static void EAP_EQP_TESTRESULT_FILE(Library.TestResult alarmReport,EquipmentModel em)
        {
            try
            {
                if (EAPEnvironment.commonLibrary.MQConnectedStatus == false)
                {
                    return;
                }
                string _sql = "";
                #region SQL Server
                //string _sql = "insert into EAP_EQP_ALM values(" +
                //                "'" + EAPEnvironment.commonLibrary.MDLN + "'," +
                //                "'" + EAPEnvironment.commonLibrary.LineName + "'," +
                //                "'" + em.EQID + "'," +
                //                "'" + em.EQName + "'," +
                //                "'" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffff") + "'," +
                //                "'" + _alarmID + "'," +
                //                "'" + _alarmText+"',"+
                //                "'" + _reportType+"',"+
                //                "'" + _alarmType+"');";
                #endregion
                #region oracle
                //string _sql = "insert into EAP_EQP_ALM(MAIN_EQP_ID,MAIN_EQP_NAME,SUB_EQP_ID,SUB_EQP_NAME,DATE_TIME,ALARM_ID,ALARM_TEXT,ALARM_TYPE,ALARM_CATEGORY)" +
                //                "values(" +
                //                "'" + EAPEnvironment.commonLibrary.MDLN + "'," +
                //                "'" + EAPEnvironment.commonLibrary.LineName + "'," +
                //                "'" + em.EQID + "'," +
                //                "'" + em.EQName + "'," +
                //                "TO_DATE('" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "','YYYY/MM/DD HH24:MI:SS')," +
                //                "'" + _alarmID + "'," +
                //                "'" + _alarmText + "'," +
                //                "'" + _reportType + "'," +
                //                "'" + _alarmType+"');";
                #endregion
                #region Mark


                //    _sql = "insert into EAP_EQP_ALM(MAIN_EQP_ID,MAIN_EQP_NAME,SUB_EQP_ID,SUB_EQP_NAME,DATE_TIME,ALARM_ID,ALARM_TEXT,ALARM_CATEGORY)" +
                //                    "values(" +
                //                    "'" + EAPEnvironment.commonLibrary.MDLN + "'," +
                //                    "'" + EAPEnvironment.commonLibrary.LineName + "'," +
                //                    "'" + em.EQID + "'," +
                //                    "'" + em.EQName + "'," +
                //                    "TO_TIMESTAMP('" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffff") + "','YYYY/MM/DD HH24:MI:SS FF')," +
                //                    "'" + _alarmID + "'," +
                //                    "'" + strReplace(_alarmText) + "'," +
                //                     "'" + _reportType+"',"+
                //                    "'" + _alarmType + "'," + "');";
                //}
                //else
                //{
                //    _sql = "update EAP_EQP_ALM set END_TIME = " +
                //                    "TO_TIMESTAMP('" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffff") + "','YYYY/MM/DD HH24:MI:SS FF')" + " where " +
                //                    "ALARM_ID = " + "'" + _alarmID + "' and " +
                //                    "SUB_EQP_ID = " + "'" + em.EQID + "' and " +
                //                    "date_time = (select date_time from (select date_time from EAP_EQP_ALM  where date_time > sysdate - 7 and end_time is null " + " and " +
                //                    "SUB_EQP_ID = " + "'" + em.EQID + "' and " +
                //                    "ALARM_ID = " + "'" + _alarmID + "'" +
                //                    "order by date_time desc) where rownum = 1); ";
                //}
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
