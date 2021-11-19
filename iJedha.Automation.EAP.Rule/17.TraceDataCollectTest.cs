//******************************************************************
//   系统名称 : iJedha.Automation.EAP.Rule
//   文件概要 : TraceDataCollect
//   作    者 : 张明哲
//   <更新履历>
//   1.0.0.0    2020/2/6 10:40:34
//******************************************************************
using System;
namespace iJedha.Automation.EAP.Rule
{
    public partial class TraceDataCollectTest
    {
        /// <summary>
        ///定时触发功能：设备Trace Data收集 + 定时删除过期Lot
        /// </summary>
        /// <param name="_DowryObj"></param>
        public void Invoke(object _DowryObj)
        {
            try
            {
                //foreach (EquipmentModel em in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
                //{
                //    if (EAPEnvironment.EAPAp.TimeComparisonSeconds(em.TraceDataCollectTimeTest, 10))
                //    {
                //        new HostService.HostService().TraceDataRequest(em.EQID, em.SUBEQID);
                //        em.TraceDataCollectTimeTest = DateTime.Now;
                //    }
                //}
               
            }
            catch (Exception ex)
            {
                Environment.BaseComm.LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                
            }
            finally
            {
            }
        }
    }
}
