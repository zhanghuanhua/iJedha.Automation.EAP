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

/// <summary>
/// EAP to DB  Interface
/// </summary>
namespace iJedha.Automation.EAP.Environment
{
    public partial class History
    {
        /// <summary>
        /// Save Process Data Use Parameter Name
        /// </summary>
        /// <param name="em"></param>
        /// <param name="vList"></param>

        public static void EAP_EQP_PROCESSDATAName(EquipmentModel em ,Dictionary<string, string> dictionary)
        {
            try
            {
                if (EAPEnvironment.commonLibrary.MQConnectedStatus == false)
                {
                    return;
                }

                //sql语句
                string _sql = string.Empty;

                #region SQL Server
                //_sql = "insert into EAP_EQP_PROCESSDATA values(" +
                //    "'" + EAPEnvironment.commonLibrary.MDLN + "'," +
                //    "'" +EAPEnvironment.commonLibrary.LineName + "'," +
                //    "'" +em.EQID + "'," +
                //    "'" +em.EQName + "'," +
                //    "'" +DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffff") + "'," +
                //    "'" +item.ID + "'," +
                //    "'" +item.Name + "'," +
                //    "'" +item.Value + "');";
                #endregion
                #region Oracle
                _sql = "insert into EAP_EQP_PROCESS(MAIN_EQP_ID,MAIN_EQP_NAME,SUB_EQP_ID,SUB_EQP_NAME,DATE_TIME,PRODUCT_ID,LOT_ID,TEST_TYPE,EMPLOYEE_ID,INSRES,RDSON,TESTVOL,FZ_KL_COUNT,FZ_DL_COUNT,FZ_KDL_COUNT,TESTPASSQTY,TESTTOTALQTY,ACTUALPASSQTY,ACTUALFAILQTY,STARTIME,ENDTIME,REASON,NGCONTENT,NGRES,TEST_SHIEF)" +
                            "values(" +
                            "'" + EAPEnvironment.commonLibrary.MDLN + "'," +
                            "'" + EAPEnvironment.commonLibrary.LineName + "'," +
                            "'" + em.EQID + "'," +
                            "'" + em.EQName + "'," +
                            "TO_DATE('" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "','YYYY/MM/DD HH24:MI:SS')," +
                            "'" + dictionary["Pn"].Trim() + "'," +    //料号
                            "'" + dictionary["Bn"].Trim() + "'," +    //批号
                            "'" + dictionary["TestType"].Trim() + "'," +   //测试类型（常规、复测）
                            "'" + dictionary["WorkerId"].Trim() + "'," +   //员工编号
                            "'" + Convert.ToInt32(dictionary["insRes"].Trim()) + "'," +   //绝缘电阻
                            "'" + Convert.ToInt32(dictionary["rdson"].Trim()) + "'," +    //导通电阻
                            "'" + Convert.ToInt32(dictionary["testVol"].Trim()) + "'," +  //测试电压
                            "'" + Convert.ToInt32(dictionary["FZ_KL_COUNT"].Trim()) + "'," +   //开路数（默认为0）
                            "'" + Convert.ToInt32(dictionary["FZ_DL_COUNT"].Trim()) + "'," +   //短路数（默认为0）
                            "'" + Convert.ToInt32(dictionary["StrOpenAndShort"].Trim()) + "'," +  //开短路数
                            "'" + Convert.ToInt32(dictionary["TestPassQty"].Trim()) + "'," +     //Ok板数量
                            "'" + Convert.ToInt32(dictionary["TestTotalQty"].Trim()) + "'," +    //总测试板数量
                            "'" + string.Empty + "'," +
                            "'" + string.Empty + "'," +
                            "TO_DATE('" + string.Format(dictionary["StarTime"].Trim(), "yyyy/MM/dd HH:mm:ss") + "','YYYY/MM/DD HH24:MI:SS')," +
                            "TO_DATE('" + string.Format(dictionary["EndTime"].Trim(), "yyyy/MM/dd HH:mm:ss") + "','YYYY/MM/DD HH24:MI:SS')," +
                            "'" + dictionary["reason"].Trim() + "'," +     //NG原因（默认为空）
                            "'" + dictionary["ngContent"].Trim() + "'," +  //NG坐标内容（默认为空）
                            "'" + Convert.ToInt32(dictionary["insRes"].Trim()) + "'," +   //绝缘电阻
                            "'" + string.Empty + "');";
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
