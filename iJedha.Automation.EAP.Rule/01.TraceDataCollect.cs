//******************************************************************
//   系统名称 : iJedha.Automation.EAP.Rule
//   文件概要 : TraceDataCollect
//   作    者 : 张明哲
//   <更新履历>
//   1.0.0.0    2020/2/6 10:40:34
//******************************************************************
using iJedha.Automation.EAP.Environment;
using iJedha.Automation.EAP.Library;
using iJedha.Automation.EAP.Model;
using System;
namespace iJedha.Automation.EAP.Rule
{
    public partial class TraceDataCollect
    {
        /// <summary>
        ///定时触发功能：设备Trace Data收集 
        /// </summary>
        /// <param name="_DowryObj"></param>
        public void Invoke(object _DowryObj)
        {
            try
            {
                foreach (EquipmentModel em in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
                {
                    //判断连线状态
                    if (em.ConnectMode == eConnectMode.DISCONNECT)
                    {
                        continue;
                    }
                    if (em.isEquipmentHold)
                    {
                        continue;
                    }
                    //判断控制模式
                    if (em.ControlMode != eControlMode.REMOTE)
                    {
                        continue;
                    }
                    //判断是否收集数据
                    if (em.isCheckTraceData==false)
                    {
                        continue;
                    }

                    if (EAPEnvironment.EAPAp.TimeComparisonMin(em.TraceDataCollectTime, em.TraceDataTimer))
                    {
                        new HostService.HostService().TraceDataRequest(em.EQID, em.SUBEQID); 
                        em.TraceDataCollectTime = DateTime.Now;
                    }
                }
               
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
