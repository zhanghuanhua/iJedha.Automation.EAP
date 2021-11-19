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
    public partial class RGVDispatchResultReport : BaseComm
    {
        public void EventHandle(string evtXml)
        {
            try
            {
                #region Decode Message
                SocketMessageStructure.RGVDispatchResultReport msg = new SocketMessageStructure.RGVDispatchResultReport();
                if (new Serialize<SocketMessageStructure.RGVDispatchResultReport>().DeSerializeXML(evtXml, out msg) == false)
                {
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Socket Message<{0}> Decode Error, Content<{1}>", "RGVDispatchResultReport", evtXml));
                    return;
                }
                #endregion

                #region Record Log
                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("<{2}>Socket Message<{0}> Recv OK , Content<{1}>",
                "RGVDispatchResultReport", msg.WriteToXml(), msg.BODY.eqp_id.Trim()));
                

                EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(msg.BODY.eqp_id.Trim());
                if (em == null)
                {
                    //error log
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<RGVDispatchResultReport> Find Error", msg.BODY.eqp_id.Trim()));
                    return;
                }
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
                //记录派送状态
                switch (msg.BODY.result)
                {
                    case "1"://1:完成 2:失败 3:不存在
                        BaseComm.LogMsg(Log.LogLevel.Info, string.Format("Equipment ID<{0}> RGV派送成功.", msg.BODY.eqp_id.Trim()));
                        break;
                    case "2":
                        BaseComm.LogMsg(Log.LogLevel.Warn, string.Format("Equipment ID<{0}> RGV派送失败.", msg.BODY.eqp_id.Trim()));
                        break;
                    case "3":
                        BaseComm.LogMsg(Log.LogLevel.Warn, string.Format("Equipment ID<{0}> RGV不存在.", msg.BODY.eqp_id.Trim()));
                        break;
                    default:
                        BaseComm.LogMsg(Log.LogLevel.Warn, string.Format("Equipment ID<{0}> RGV Error", msg.BODY.eqp_id.Trim()));
                        break;
                }
                #endregion
                #region Reply Message
                HostService.HostService.RGVDispatchResultReportReply(msg.HEADER.TRANSACTIONID, msg.BODY.eqp_id.Trim());
                #endregion

            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", "RGVDispatchResultReport", ex.Message.ToString(), ex.StackTrace.ToString()));
            }
            finally
            {

            }
        }
    }
}
