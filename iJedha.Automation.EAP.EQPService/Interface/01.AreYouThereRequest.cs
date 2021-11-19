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
    /// <summary>
    /// 
    /// </summary>
    public partial class AreYouThereRequest : BaseComm
    {
        public void EventHandle(string evtXml)
        {
            try
            {
              
                #region Decode Message
                SocketMessageStructure.AreYouThereRequest msg = new SocketMessageStructure.AreYouThereRequest();
                if (new Serialize<SocketMessageStructure.AreYouThereRequest>().DeSerializeXML(evtXml, out msg) == false)
                {
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Socket Message<{0}> Decode Error, Content<{1}>", "AreYouThereRequest", evtXml));
                    return;
                }
                #endregion

                #region Record Log
                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("<{2}>Socket Message<{0}> Recv OK , Content<{1}>",
                msg.HEADER.MESSAGENAME, msg.WriteToXml(), msg.BODY.eqp_id.Trim()));
                #endregion
                //根据设备ID获取设备信息
                EquipmentModel em = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(msg.BODY.eqp_id.Trim());
                if (em == null)
                {
                    //error log
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<AreYouThereRequest> Find Error", msg.BODY.eqp_id.Trim()));
                    return;
                }
                #region 连线检查
                if (em.ConnectMode == eConnectMode.DISCONNECT)
                {
                    BaseComm.LogMsg(Log.LogLevel.Error, $"设备ID[{em.EQID}],连线状态为[{em.ConnectMode.ToString()}]");
                    return;
                }
                #endregion
               

                #region Reply Message
                HostService.HostService.AreYouThereRequestReply(msg.HEADER.TRANSACTIONID, em.EQName);
                #endregion
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", "AreYouThereRequest", ex.Message.ToString(), ex.StackTrace.ToString()));
            }
            finally
            {

            }
        }
    }
}
