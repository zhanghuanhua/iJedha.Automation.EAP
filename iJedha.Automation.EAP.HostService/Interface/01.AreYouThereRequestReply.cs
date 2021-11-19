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
        public static void AreYouThereRequestReply(string transactionId, string eqpname)
        {
            try
            {

                EquipmentModel em = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByName(eqpname);
                if (em == null)
                {
                    //error log
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment Name<{0}> Function Name<AreYouThereRequestReply> Find Error", eqpname));
                    return;
                }

                AreYouThereRequestReply msg = new AreYouThereRequestReply();
                msg.HEADER.MESSAGENAME = eSocketCommand.AreYouThereRequestReply.GetEnumDescription();
                msg.HEADER.TRANSACTIONID = transactionId;
                msg.BODY.eqp_id = em.EQID;
                msg.BODY.return_code = "1";
                msg.RETURN.RETURNCODE = "0";
                msg.RETURN.RETURNMESSAGE = string.Empty;


                byte[] trxData = Extensions.GetTrxData(msg.WriteToXml());

                EAPEnvironment.Dic_TCPSocketAp[em.EQID].SendData(new socket.SendInfo("", trxData));

                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("<{2}>Socket Message<{0}> Send OK , Content<{1}>",
                        System.Reflection.MethodBase.GetCurrentMethod().Name, msg.WriteToXml(), eqpname));
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
