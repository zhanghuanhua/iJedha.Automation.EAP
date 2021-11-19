//******************************************************************
//   系统名称 : iJedha.Automation.EAP.HistoryService
//   文件概要 : History
//   作    者 : 张明哲
//   <更新履历>
//   1.0.0.0    2020/2/6 10:40:34
//******************************************************************
using iJedha.Automation.EAP.LibraryBase;
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
        /// Save Trace Data
        /// </summary>
        /// <param name="em"></param>
        /// <param name="dl"></param>
        public static void EAP_EQP_TRACE(EquipmentModel em, Socket_DynamicLibraryBase dl)
        {
            try
            {
                if (EAPEnvironment.commonLibrary.MQConnectedStatus == false)
                {
                    return;
                }
                /*
                 * MAIN_EQP_ID
                 * MAIN_EQP_NAME
                 * SUB_EQP_ID
                 * SUB_EQP_NAME
                 * DATE_TIME
                 * ITEM_ID
                 * ITEM_NAME
                 * ITEM_VALUE
                 */
                 
                //string occurTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffff");
                string _sql = string.Empty;
                foreach (var item in dl.Dic_TraceDataModel.Values)
                {
                    #region Oracle
                    _sql = "insert into EAP_EQP_TRACE(MAIN_EQP_ID,MAIN_EQP_NAME,SUB_EQP_ID,SUB_EQP_NAME,DATE_TIME,ITEM_ID,ITEM_NAME,ITEM_VALUE)" +
                                "values(" +
                                "'" + EAPEnvironment.commonLibrary.MDLN + "'," +
                                "'" + EAPEnvironment.commonLibrary.LineName + "'," +
                                "'" + em.EQID + "'," +
                                "'" + em.EQName + "'," +
                                "TO_DATE('" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "','YYYY/MM/DD HH24:MI:SS')," +
                                "'" + item.ID + "'," +
                                "'" + item.Name + "'," +
                                "'" + item.Value + "');";
                    #endregion
                    if (!EAPEnvironment.MQPublisherAp.MQ_DBData(Environment.EAPEnvironment.commonLibrary.baseLib.rbmqParaLibrary.QueueName, _sql))
                    {

                    }
                }
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
            }
        }
    }
}
