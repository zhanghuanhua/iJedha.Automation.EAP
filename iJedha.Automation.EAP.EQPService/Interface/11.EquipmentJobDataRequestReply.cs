//******************************************************************
//   系统名称 : iJedha.Automation.EAP.SocketService
//   文件概要 : 
//   作    者 : 张明哲
//   <更新履历>
//   1.0.0.0    2020/10/1 14:28:28
//******************************************************************
using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Environment;
using iJedha.Automation.EAP.Library;
using iJedha.Automation.EAP.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace iJedha.Automation.EAP.EQPService
{
    public partial class EquipmentJobDataRequestReply : BaseComm
    {
        public static BaseComm EAPAp;
        public void EventHandle(string evtXml)
        {
            try
            {

                #region Decode Message
                SocketMessageStructure.EquipmentJobDataRequestReply rpy = new SocketMessageStructure.EquipmentJobDataRequestReply();
                if (new Serialize<SocketMessageStructure.EquipmentJobDataRequestReply>().DeSerializeXML(evtXml, out rpy) == false)
                {
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Socket Message<{0}> Decode Error, Content<{1}>", "EquipmentJobDataRequestReply", evtXml));
                    return;
                }
                #endregion

                #region 记录设备回复RETURNCODE NG的Log
                //if (!rpy.RETURN.RETURNCODE.Equals("0"))
                //{
                //    BaseComm.LogMsg(Log.LogLevel.Warn, string.Format("设备<{0}>回复<{1}> 结果NG,回复内容<{2}> ", rpy.BODY.eqp_id.Trim(),
                //       "EquipmentJobDataRequestReply", rpy.RETURN.RETURNMESSAGE));
                //    return;
                //}
                #endregion

                #region Record Log
                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("<{2}>Socket Message<{0}> Recv OK , Content<{1}>",
                "EquipmentJobDataRequestReply", rpy.WriteToXml(), rpy.BODY.eqp_id.Trim()));
                #endregion

                #region EAP记录当前任务信息
                EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(rpy.BODY.eqp_id.Trim());
                if (em == null)
                {
                    //error log
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<EquipmentJobDataRequestReply> Find Error", rpy.BODY.eqp_id.Trim()));
                    return;
                }
                #endregion

                #region 清完线的设备Flag改成true，没有清完的设备Flag改成false
                if (rpy.BODY.process_panel_count == rpy.BODY.total_panel_count && em.LastLotID == rpy.BODY.job_id.Trim())
                {
                    em.isProcessCompletion = true;
                    BaseComm.LogMsg(Log.LogLevel.Info, string.Format("设备<{0}> 完成清机",em.EQName));
                }
                else
                {
                    em.isProcessCompletion = false;
                    BaseComm.LogMsg(Log.LogLevel.Warn, string.Format("设备<{0}> 未完成清机，最后一个Lot<{1}>,当前上报Lot<{2}>,Lot总数量<{3}>,Lot完成数量<{4}>",
                        em.EQName,em.LastLotID,rpy.BODY.job_id.Trim(), rpy.BODY.total_panel_count, rpy.BODY.process_panel_count));
                }

                #endregion
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", "EquipmentJobDataRequestReply", ex.Message.ToString(), ex.StackTrace.ToString()));
            }
            finally
            {

            }
        }


    }
}
