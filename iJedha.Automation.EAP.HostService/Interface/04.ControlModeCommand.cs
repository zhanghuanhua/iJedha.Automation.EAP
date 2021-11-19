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
using iJedha.SocketMessageStructure;
using System;

namespace iJedha.Automation.EAP.HostService
{
    public partial class HostService
    {
        /// <summary>
        /// 控制模式指令
        /// </summary>
        /// <param name="eqpName"></param>
        /// <param name="controlMode"></param>
        public void ControlModeCommand(string eqpName, string controlMode)
        {
            try
            {
                EquipmentModel em = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByName(eqpName);
                if (em == null)
                {
                    //error log
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment Name<{0}> Function Name<ControlModeCommand> Find Error", eqpName));
                    return;
                }
                #region 连线检查
                if (em.isCheckConnect && em.isCheckControlMode)
                {
                    if (em.ConnectMode == eConnectMode.DISCONNECT)
                    {
                        BaseComm.LogMsg(Log.LogLevel.Error, $"设备ID[{em.EQID}],连线状态为[{em.ConnectMode.ToString()}],控制模式为[{em.ControlMode.ToString()}]");
                        return;
                    }
                }
                #endregion

                SocketMessageStructure.ControlModeCommand msg = new SocketMessageStructure.ControlModeCommand();
                msg.HEADER.MESSAGENAME = eSocketCommand.ControlModeCommand.GetEnumDescription();
                msg.HEADER.TRANSACTIONID = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                msg.BODY.eqp_id = em.EQID;
                msg.RETURN.RETURNCODE = "0";
                msg.RETURN.RETURNMESSAGE = string.Empty;
                msg.BODY.control_mode = controlMode;
                string _trx = msg.WriteToXml();

                byte[] trxData = Extensions.GetTrxData(msg.WriteToXml());

                EAPEnvironment.Dic_TCPSocketAp[em.EQID].SendData(new socket.SendInfo("", trxData));

                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("<{2}>Socket Message<{0}> Send OK , Content<{1}>",
                        System.Reflection.MethodBase.GetCurrentMethod().Name, msg.WriteToXml(), em.EQID));


            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
            }
            finally
            {

            }
        }
    }
}
