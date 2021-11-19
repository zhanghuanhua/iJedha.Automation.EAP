//******************************************************************
//   系统名称 : iJedha.Automation.EAP.SocketService
//   文件概要 : 
//   作    者 : 张明哲
//   <更新履历>
//   1.0.0.0    2020/10/1 14:28:28
//******************************************************************
using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Environment;
using iJedha.Automation.EAP.Model;
using System;

namespace iJedha.Automation.EAP.EQPService
{
    public partial class TrayStatusReport : BaseComm
    {
        public void EventHandle(string evtXml)
        {
            try
            {
                #region Decode Message
                SocketMessageStructure.TrayStatusReport msg = new SocketMessageStructure.TrayStatusReport();
                if (new Serialize<SocketMessageStructure.TrayStatusReport>().DeSerializeXML(evtXml, out msg) == false)
                {
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Socket Message<{0}> Decode Error, Content<{1}>", "TrayStatusReport", evtXml));
                    return;
                }
                #endregion

                #region Record Log
                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("<{2}>Socket Message<{0}> Recv OK , Content<{1}>",
                "TrayStatusReport", msg.WriteToXml(), msg.BODY.eqp_id.Trim()));
                #endregion

                #region 处理接收到的底盘和任务名称，根据底盘状态进行绑定和解绑
                EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(msg.BODY.eqp_id.Trim());
                if (em==null)
                {
                    //error log
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<TrayStatusReport> Find Error", msg.BODY.eqp_id.Trim()));
                    return;
                }
                else
                {
                    #region 连线检查
                    if (em.isCheckConnect && em.isCheckControlMode)
                    {
                        if (em.ConnectMode == eConnectMode.DISCONNECT || em.ControlMode != eControlMode.REMOTE)
                        {
                            BaseComm.LogMsg(Log.LogLevel.Error, $"设备ID[{em.EQID}],连线状态为[{em.ConnectMode.ToString()}],控制模式为[{em.ControlMode.ToString()}]");
                            return;
                        }
                    }
                    #endregion

                    //最后一底盘需要进行底盘和Batch ID绑定
                    switch (msg.BODY.tray_status)
                    {
                        case "1"://绑定
                            //if (em.TrayStatusInfo.ContainsKey(msg.BODY.tray_id.Trim()))
                            //{
                            //    em.TrayStatusInfo.Remove(msg.BODY.tray_id.Trim());
                            //}
                            //em.TrayStatusInfo.Add(msg.BODY.tray_id.Trim(), msg.BODY.job_id.Trim());
                            break;
                        case "2"://解绑
                            em.TrayStatusInfo.Remove(msg.BODY.tray_id.Trim());
                            break;
                        case "3"://最后一底盘
                            if (em.TrayStatusInfo.ContainsKey(msg.BODY.tray_id.Trim()))
                            {
                                em.TrayStatusInfo.Remove(msg.BODY.tray_id.Trim());
                            }
                            em.TrayStatusInfo.Add(msg.BODY.tray_id.Trim(), msg.BODY.job_id.Trim());
                            break;
                        default:
                            BaseComm.LogMsg(Log.LogLevel.Warn, string.Format("Equipment ID<{0}> 上报底盘状态<{1}>超出规定范围", msg.BODY.eqp_id.Trim(), msg.BODY.tray_status));
                            break;
                    }
                    
                }
                #endregion

                #region Reply Message
                HostService.HostService.TrayStatusReportReply(msg.HEADER.TRANSACTIONID, msg.BODY.eqp_id.Trim());
                #endregion
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", "TrayStatusReport", ex.Message.ToString(), ex.StackTrace.ToString()));
            }
            finally
            {

            }
        }
    }
}
