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
using iJedha.Automation.EAP.WebAPI;
using System;

namespace iJedha.Automation.EAP.EQPService
{
    public partial class ToolsStatusReport : BaseComm
    {
        public void EventHandle(string evtXml)
        {
            try
            {
                #region Decode Message
                SocketMessageStructure.ToolsStatusReport msg = new SocketMessageStructure.ToolsStatusReport();
                if (new Serialize<SocketMessageStructure.ToolsStatusReport>().DeSerializeXML(evtXml, out msg) == false)
                {
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Socket Message<{0}> Decode Error, Content<{1}>", "ToolsStatusReport", evtXml));
                    return;
                }
                #endregion

                #region Record Log
                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("<{2}>Socket Message<{0}> Recv OK , Content<{1}>",
                "ToolsStatusReport", msg.WriteToXml(), msg.BODY.eqp_id.Trim()));
                #endregion

                #region EAP调用MES接口把物料信息上报给MES
                EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(msg.BODY.eqp_id.Trim());

                if (em != null)
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
                    //工具状态为1时，需上报MES工具上机
                    if (msg.BODY.tools_status.Equals("1"))
                    {
                        new WebAPIReport().EAP_EquipmentSetUp(new MessageModel.EquipmentSetUp()
                        {
                            SubEqpID = em.EQID,
                            ToolID = msg.BODY.tools_id.Trim()
                        }, 1);
                    }
                }
                else
                {
                    //error log
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<ToolsStatusReport> Find Error", msg.BODY.eqp_id.Trim()));
                    return;
                }
                #endregion

                #region Reply Message
                HostService.HostService.ToolsStatusReportReply(msg.HEADER.TRANSACTIONID, msg.BODY.eqp_id.Trim());
                #endregion
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", "ToolsStatusReport", ex.Message.ToString(), ex.StackTrace.ToString()));
            }
            finally
            {

            }
        }
    }
}
