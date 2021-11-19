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
        /// 人员登录确认指令
        /// </summary>
        /// <param name="opID"></param>
        /// <param name="transactionId"></param>
        /// <param name="eqpID"></param>
        /// <param name="result"></param>
        public void OperatorLoginConfirm(string opID, string transactionId, string eqpID, string result)
        {
            try
            {
                var em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(eqpID);
                if (em == null)
                {
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<OperatorLoginConfirm> Find Error", eqpID));
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
                SocketMessageStructure.OperatorLoginConfirm msg = new SocketMessageStructure.OperatorLoginConfirm();
                msg.HEADER.MESSAGENAME = eSocketCommand.OperatorLoginConfirm.GetEnumDescription();
                msg.HEADER.TRANSACTIONID = transactionId;
                msg.BODY.eqp_id = eqpID;
                msg.BODY.result = result;
                msg.BODY.operator_id = opID;
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
