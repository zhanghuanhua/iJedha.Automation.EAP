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
    public partial class PanelInReport : BaseComm
    {
        public void EventHandle(string evtXml)
        {
            try
            {
                #region Decode Message
                SocketMessageStructure.PanelInReport msg = new SocketMessageStructure.PanelInReport();
                if (new Serialize<SocketMessageStructure.PanelInReport>().DeSerializeXML(evtXml, out msg) == false)
                {
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Socket Message<{0}> Decode Error, Content<{1}>", "PanelInReport", evtXml));
                    return;
                }
                #endregion

                #region Record Log
                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("<{2}>Socket Message<{0}> Recv OK , Content<{1}>",
                "PanelInReport", msg.WriteToXml(), msg.BODY.eqp_id.Trim()));

                #endregion



                #region EAP记录设备当前进板数量
                //根据设备ID获取设备信息
                EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(msg.BODY.eqp_id.Trim());
                
                if (em != null)
                {
                    em.PanelInCount = int.Parse(msg.BODY.panel_in_count == "" ? "0" : msg.BODY.panel_in_count);

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

                    #region Reply Message
                    HostService.HostService.PanelInReportReply(msg.HEADER.TRANSACTIONID, msg.BODY.eqp_id.Trim(), "1");//0:NG 1:OK
                    #endregion
                }
                else
                {
                    #region Reply Message
                    HostService.HostService.PanelInReportReply(msg.HEADER.TRANSACTIONID, msg.BODY.eqp_id.Trim(), "0");
                    #endregion
                    //error log
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<PanelInReport> Find Error", msg.BODY.eqp_id.Trim()));
                    return;
                }
                #endregion

            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", "PanelInReport", ex.Message.ToString(), ex.StackTrace.ToString()));
            }
            finally
            {

            }
        }
    }
}
