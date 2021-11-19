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
        public static void EAP_EQP_ALM(EquipmentModel em, string _alarmID, string _alarmText, string _reportType, string _alarmType)
        {
            try
            {
                if (EAPEnvironment.commonLibrary.MQConnectedStatus == false)
                {
                    return;
                }
                /*MAIN_EQP_ID 
                 * MAIN_EQP_NAME 
                 * SUB_EQP_ID 
                 * SUB_EQP_NAME 
                 * DATE_TIME 
                 * ALARM_ID 
                 * ALARM_TEXT 
                 * ALARM_TYPE   警报方式：发生，解除 report type
                 * ALARM_CATEGORY   警报类型：一般，重大 _alarmType
                 */
                #region oracle
                string _sql = string.Empty;

                    _sql = "insert into EAP_EQP_ALM(MAIN_EQP_ID,MAIN_EQP_NAME,SUB_EQP_ID,SUB_EQP_NAME,DATE_TIME,ALARM_ID,ALARM_TEXT,EQP_REPORT_TIME,ALARM_ACTION,ALARM_CATEGORY)" +
                               "values(" +
                               "'" + EAPEnvironment.commonLibrary.MDLN + "'," +
                               "'" + EAPEnvironment.commonLibrary.LineName + "'," +
                               "'" + em.EQID + "'," +
                               "'" + em.EQName + "'," +
                               "TO_DATE('" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "','YYYY/MM/DD HH24:MI:SS')," +
                               "'" + _alarmID + "'," +
                               "'" + _alarmText + "'," +
                               "TO_DATE('" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "','YYYY/MM/DD HH24:MI:SS')," +
                               "'" + _reportType + "'," +
                               "'" + _alarmType + "');";


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
