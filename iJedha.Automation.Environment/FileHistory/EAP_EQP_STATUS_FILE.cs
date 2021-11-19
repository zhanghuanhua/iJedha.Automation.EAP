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
        /// Save Status
        /// </summary>
        /// <param name="em"></param>
        /// <param name="_alarmID"></param>
        /// <param name="_alarmText"></param>
        /// <param name="_alarmLevel"></param>
        /// <param name="_isSet"></param>
        public static void EAP_EQP_STATUS_FILE(Library.EquipmentStatus status,EquipmentModel em)
        {
            try
            {
                if (EAPEnvironment.commonLibrary.MQConnectedStatus == false)
                {
                    return;
                }
                string _sql1 = string.Empty;
                string _sql2 = string.Empty;
                string CurrentTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");


                #region oracle
                _sql1 = "update eap_eqp_status " +
                         "set endtime = TO_DATE('" + CurrentTime + "', 'YYYY/MM/DD HH24:MI:SS') " +
                         "WHERE main_eqp_id = '" + EAPEnvironment.commonLibrary.MDLN + "' " +
                         "and sub_eqp_id = '" + em.EQID + "' " +
                         "and date_time = (select date_time " +
                         "from(select date_time " +
                         "from eap_eqp_status " +
                         "where endtime is null " +
                         "and sub_eqp_id = '" + em.EQID + "' " +
                         "and main_eqp_id = '" + EAPEnvironment.commonLibrary.MDLN + "' " +
                         "and date_time < TO_DATE('" + CurrentTime + "', 'YYYY/MM/DD HH24:MI:SS') " +
                         "order by date_time desc) " +
                         "where rownum = 1)";


                if (!EAPEnvironment.MQPublisherAp.MQ_DBData(Environment.EAPEnvironment.commonLibrary.baseLib.rbmqParaLibrary.QueueName, _sql1))
                {

                }
                _sql2 = "insert into EAP_EQP_STATUS (MAIN_EQP_ID,MAIN_EQP_NAME,SUB_EQP_ID,SUB_EQP_NAME,DATE_TIME,EQP_STATUS,GREEN_TOWNER,YELLOW_TOWNER,RED_TOWNER,STARTIME,ENDTIME)" +
                                 "values(" +
                                 "'" + EAPEnvironment.commonLibrary.MDLN + "'," +
                                 "'" + EAPEnvironment.commonLibrary.LineName + "'," +
                                 "'" + em.EQID + "'," +
                                 "'" + em.EQName + "'," +
                                 "TO_DATE('" + CurrentTime + "','YYYY/MM/DD HH24:MI:SS')," +
                                 "'" + status.eq_status + "'," +
                                 "'" + status.green_towner + "'," +
                                 "'" + status.yellow_towner + "'," +
                                 "'" + status.red_towner + "'," +
                                 "TO_DATE('" + CurrentTime + "','YYYY/MM/DD HH24:MI:SS')," +
                                 "'" + string.Empty + "');";
                #endregion
                if (!EAPEnvironment.MQPublisherAp.MQ_DBData(Environment.EAPEnvironment.commonLibrary.baseLib.rbmqParaLibrary.QueueName, _sql2))
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
