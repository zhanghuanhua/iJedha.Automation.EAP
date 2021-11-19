//******************************************************************
//   系统名称 : iJedha.Automation.EAP.SocketService
//   文件概要 : 
//   作    者 : 张明哲
//   <更新履历>
//   1.0.0.0    2020/10/1 14:28:28
//******************************************************************

using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Environment;
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
        public static bool ScanCodeReportReply(string transactionId, string eqpID, string returnCode)
        {
            bool rtn = false;

            try
            {
                SocketMessageStructure.PanelInReportReply msg = new SocketMessageStructure.PanelInReportReply();
                msg.HEADER.MESSAGENAME = eSocketCommand.PanelInReportReply.GetEnumDescription();
                msg.HEADER.TRANSACTIONID = transactionId;
                msg.BODY.eqp_id = eqpID;
                msg.BODY.return_code = returnCode;
                
                msg.RETURN.RETURNCODE = "0";
                msg.RETURN.RETURNMESSAGE = string.Empty;



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
