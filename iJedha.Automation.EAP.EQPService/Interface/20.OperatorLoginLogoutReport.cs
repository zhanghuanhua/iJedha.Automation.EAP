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
    public partial class OperatorLoginLogoutReport : BaseComm
    {
        public void EventHandle(string evtXml)
        {
            try
            {
                #region Decode Message
                SocketMessageStructure.OperatorLoginLogoutReport msg = new SocketMessageStructure.OperatorLoginLogoutReport();
                if (new Serialize<SocketMessageStructure.OperatorLoginLogoutReport>().DeSerializeXML(evtXml, out msg) == false)
                {
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Socket Message<{0}> Decode Error, Content<{1}>", "OperatorLoginLogoutReport", evtXml));
                    return;
                }
                #endregion

                #region Record Log
                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("<{2}>Socket Message<{0}> Recv OK , Content<{1}>",
                "OperatorLoginLogoutReport", msg.WriteToXml(), msg.BODY.eqp_id.Trim()));
                #endregion



                #region 调用MES接口上传人员刷卡信息
                //根据设备ID获取设备信息
                EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(msg.BODY.eqp_id.Trim());

                if (em != null)
                {
                    if (msg.BODY.identify_type=="3")
                    {
                        //上报MES
                        new WebAPIReport().EAP_TrainingRequest(new MessageModel.TrainingRequest()
                        {
                            SubEqpID = em.EQID,
                            Employee = msg.BODY.operator_id
                        }, 1, msg.HEADER.TRANSACTIONID);
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

                    #region Reply Message
                    HostService.HostService.OperatorLoginLogoutReportReply(msg.HEADER.TRANSACTIONID, msg.BODY.eqp_id.Trim(), "1");
                    #endregion
                }
                else
                {
                    #region Reply Message
                    HostService.HostService.OperatorLoginLogoutReportReply(msg.HEADER.TRANSACTIONID, msg.BODY.eqp_id.Trim(), "0");
                    #endregion
                    //error log
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<OperatorLoginLogoutReport> Find Error", msg.BODY.eqp_id.Trim()));
                    return;
                }
                #endregion
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", "OperatorLoginLogoutReport", ex.Message.ToString(), ex.StackTrace.ToString()));
            }
            finally
            {

            }
        }
    }
}
