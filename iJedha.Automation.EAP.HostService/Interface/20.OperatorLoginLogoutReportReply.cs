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
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool OperatorLoginLogoutReportReply(string transactionId, string eqpID,string retureCode)
        {
            bool rtn = false;

            try
            {
                SocketMessageStructure.OperatorLoginLogoutReportReply msg = new SocketMessageStructure.OperatorLoginLogoutReportReply();
                msg.HEADER.MESSAGENAME = eSocketCommand.OperatorLoginLogoutReportReply.GetEnumDescription();
                msg.HEADER.TRANSACTIONID = transactionId;
                msg.BODY.eqp_id = eqpID;
                msg.BODY.return_code = retureCode;

                EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(msg.BODY.eqp_id);
                if (em == null)
                {
                    string errMsg = string.Format("Equipment ID<{0}> Function Name<OperatorLoginLogoutReportReply> Find Error", msg.BODY.eqp_id);
                    msg.RETURN.RETURNCODE = "1";
                    msg.RETURN.RETURNMESSAGE = errMsg;
                    BaseComm.LogMsg(Log.LogLevel.Error, errMsg);

                    return rtn;
                }
                else
                {
                    msg.RETURN.RETURNCODE = "0";
                    msg.RETURN.RETURNMESSAGE = string.Empty;
                }

                byte[] trxData = Extensions.GetTrxData(msg.WriteToXml());
                EAPEnvironment.Dic_TCPSocketAp[eqpID].SendData(new socket.SendInfo("", trxData));

                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("<{2}>Socket Message<{0}> Send OK , Content<{1}>",
                        System.Reflection.MethodBase.GetCurrentMethod().Name, msg.WriteToXml(), eqpID));


                rtn = true;
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
            }
            finally
            {

            }
            return rtn;
        }
    }
}
