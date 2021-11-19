﻿//******************************************************************
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
        /// 连线初始化
        /// </summary>
        /// <returns></returns>
        public void InitialDataRequest(string eqpID)
        {
            try
            {
                SocketMessageStructure.InitialDataRequest msg = new SocketMessageStructure.InitialDataRequest();
                msg.HEADER.MESSAGENAME = eSocketCommand.InitialDataRequest.GetEnumDescription();
                msg.HEADER.TRANSACTIONID = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                msg.BODY.eqp_id = eqpID;


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