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
        /// 事件历史记录
        /// </summary>
        /// <param name="em"></param>
        /// <param name="eventName"></param>
        /// <param name="eventFlow"></param>
        /// <param name="eventValue"></param>
        public static void EAP_EQP_EVENTHISTORY(EquipmentModel em,eEventName eventName,eEventFlow eventFlow,string eventValue,string jobID,string portID)
        {

            try
            {
                if (EAPEnvironment.commonLibrary.MQConnectedStatus == false)
                {
                    return;
                }
                #region Oracle
                string _sql = "insert into EAP_EQP_EVENTHISTORY (MAIN_EQP_ID,MAIN_EQP_NAME,SUB_EQP_ID,SUB_EQP_NAME,DATE_TIME,EVENT_NAME,EVENT_FLOW,EVENT_VALUE,JOB_ID,PORT_ID)" +
                                 "values(" +
                                 "'" + EAPEnvironment.commonLibrary.MDLN + "'," +
                                 "'" + EAPEnvironment.commonLibrary.LineName + "'," +
                                 "'" + em.EQID + "'," +
                                 "'" + em.EQName + "'," +
                                 "TO_DATE('" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "','YYYY/MM/DD HH24:MI:SS')," +
                                 "'" + eventName.ToString() + "'," +
                                 "'" + eventFlow.ToString() + "'," +
                                  "'"+eventValue + "'," +
                                  "'" + jobID + "'," +
                                 "'" + portID + "');";
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
