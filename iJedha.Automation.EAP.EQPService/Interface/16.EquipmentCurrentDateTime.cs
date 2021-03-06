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
    public partial class EquipmentCurrentDateTime : BaseComm
    {
        public void EventHandle(string evtXml)
        {
            try
            {
                #region Decode Message
                SocketMessageStructure.EquipmentCurrentDateTime msg = new SocketMessageStructure.EquipmentCurrentDateTime();
                if (new Serialize<SocketMessageStructure.EquipmentCurrentDateTime>().DeSerializeXML(evtXml, out msg) == false)
                {
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Socket Message<{0}> Decode Error, Content<{1}>", "EquipmentCurrentDateTime", evtXml));
                    return;
                }
                #endregion

                #region Record Log
                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("<{2}>Socket Message<{0}> Recv OK , Content<{1}>",
                "EquipmentCurrentDateTime", msg.WriteToXml(), msg.BODY.eqp_id.Trim()));
                #endregion
                #region 更新设备当前时间
                EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(msg.BODY.eqp_id.Trim());
                if (em==null)
                {
                    //error log
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<EquipmentCurrentDateTime> Find Error", msg.BODY.eqp_id.Trim()));
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

                if (UpdataCurrentDatetime(em, msg.BODY.date_time))
                {
                    #region Reply Message
                    HostService.HostService.EquipmentCurrentDateTimeReply(msg.HEADER.TRANSACTIONID, msg.BODY.eqp_id.Trim());
                    #endregion
                }

                #endregion
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", "EquipmentCurrentDateTime", ex.Message.ToString(), ex.StackTrace.ToString()));
            }
            finally
            {

            }
        }
        /// <summary>
        /// 更新时间
        /// </summary>
        /// <param name="em"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public bool UpdataCurrentDatetime(EquipmentModel em, string dateTime)
        {
            try
            {
                if (dateTime.Length > 14)
                {
                    if (dateTime.Substring(0, 14) != DateTime.Now.ToString("yyyyMMddHHmmss"))
                    {
                        new HostService.HostService().DateTimeSyncCommand(em.EQID);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Environment.BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return false;
            }
        }
    }
}
