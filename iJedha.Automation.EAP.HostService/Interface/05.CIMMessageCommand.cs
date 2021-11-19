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
using System.Collections.Generic;
using System.Linq;

namespace iJedha.Automation.EAP.HostService
{
    public partial class HostService
    {
        /// <summary>
        /// CIM Message下载指令
        /// </summary>
        /// <param name="eqpID"></param>
        /// <param name="intervalSecondTime"></param>
        /// <param name="cimMessage"></param>
        /// <param name="transactionID"></param>
        public void CIMMessageCommand(string eqpID, string intervalSecondTime, string cimMessage, string transactionID)
        {
            try
            {
                var em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(eqpID);
                if (em==null)
                {
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<CIMMessageCommand> Find Error", eqpID));
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

                SocketMessageStructure.CIMMessageCommand msg = new SocketMessageStructure.CIMMessageCommand();
                msg.HEADER.MESSAGENAME = eSocketCommand.CIMMessageCommand.GetEnumDescription();
                msg.HEADER.TRANSACTIONID = transactionID;
                msg.BODY.eqp_id = eqpID;
                msg.BODY.interval_second_time = intervalSecondTime;
                msg.BODY.cim_message = cimMessage;
                msg.RETURN.RETURNCODE = "0";
                msg.RETURN.RETURNMESSAGE = string.Empty;


                byte[] trxData = Extensions.GetTrxData(msg.WriteToXml());
                EAPEnvironment.Dic_TCPSocketAp[eqpID].SendData(new socket.SendInfo("", trxData));

                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("<{2}>Socket Message<{0}> Send OK , Content<{1}>",
                        System.Reflection.MethodBase.GetCurrentMethod().Name, msg.WriteToXml(), eqpID));

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
