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
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// EAP to DB  Interface
/// </summary>
namespace iJedha.Automation.EAP.Environment
{
    public partial class History
    {
        /// <summary>
        /// Save Process Data
        /// </summary>
        /// <param name="em"></param>
        /// <param name="vList"></param>
        public static void EAP_EQP_PROCESSDATA_FILE(List<SocketMessageStructure.FileProcessDataReport.cproc_data> listData, EquipmentModel em)
        {
            try
            {
                if (EAPEnvironment.commonLibrary.MQConnectedStatus == false)
                {
                    return;
                }

                //sql语句
                string _sql = string.Empty;
                foreach (var item in listData)
                {
                    var subItem = item.data_value.Split('\t').ToArray();
                    string date = subItem[0];//日期
                    string p = subItem[15];//良率
                    

                    #region Oracle

                    _sql = "insert into EAP_EQP_PROCESS(MAIN_EQP_ID,MAIN_EQP_NAME,SUB_EQP_ID,SUB_EQP_NAME,DATE_TIME,PRODUCT_ID,LOT_ID,TEST_TYPE,EMPLOYEE_ID,INSRES,RDSON,TESTVOL,FZ_KL_COUNT,FZ_DL_COUNT,FZ_KDL_COUNT,TESTPASSQTY,TESTTOTALQTY,ACTUALPASSQTY,ACTUALFAILQTY,STARTIME,ENDTIME,REASON,NGCONTENT,NGRES,TEST_SHIEF)" +
                                "values(" +
                                "'" + EAPEnvironment.commonLibrary.MDLN + "'," +
                                "'" + EAPEnvironment.commonLibrary.LineName + "'," +
                                "'" + em.EQID + "'," +
                                "'" + em.EQName + "'," +
                                "TO_DATE('" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "','YYYY/MM/DD HH24:MI:SS')," +
                                "'" + subItem[1] + "'," +//生产编号
                                "'" + subItem[2] + "'," +//批量号
                                "'" + subItem[13] + "'," +//测试类型
                                "'" + subItem[3] + "'," +//员工编号
                                "'" + Convert.ToInt32(subItem[6].Replace("M"," ").Trim()) + "'," +//绝缘电阻 Ohm
                                "'" + Convert.ToInt32(subItem[4]) + "'," +//导通电阻 Ohm
                                "'" + Convert.ToInt32(subItem[5]) + "'," +//绝缘电压 V
                                "'" + Convert.ToInt32(subItem[8]) + "'," +//开路
                                "'" + Convert.ToInt32(subItem[9]) + "'," +//短路
                                "'" + Convert.ToInt32(subItem[10]) + "'," +//开/短路
                                "'" + Convert.ToInt32(subItem[7]) + "'," +//好板
                                "'" + Convert.ToInt32(subItem[14]) + "'," +//总数
                                "'" + Convert.ToInt32(subItem[11]) + "'," +//Pass数量
                                "'" + Convert.ToInt32(subItem[12]) + "'," +//Fail数量
                                "TO_DATE('" + string.Format(subItem[16], "yyyy/MM/dd HH:mm:ss") + "','YYYY/MM/DD HH24:MI:SS')," +   //起始时间
                                "TO_DATE('" + string.Format(subItem[17], "yyyy/MM/dd HH:mm:ss") + "','YYYY/MM/DD HH24:MI:SS')," +   //终止时间
                                "'" + string.Empty + "'," +
                                "'" + string.Empty + "'," +
                                "'" + string.Empty + "'," +
                                "'" + string.Empty + "');";
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
