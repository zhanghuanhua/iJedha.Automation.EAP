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
    public partial class AreYouThereRequestReply : BaseComm
    {
        public void EventHandle(string evtXml)
        {
            try
            {
                #region Decode Message
                SocketMessageStructure.AreYouThereRequestReply msg = new SocketMessageStructure.AreYouThereRequestReply();
                if (new Serialize<SocketMessageStructure.AreYouThereRequestReply>().DeSerializeXML(evtXml, out msg) == false)
                {
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Socket Message<{0}> Decode Error, Content<{1}>", "AreYouThereRequest", evtXml));
                    return;
                }
                #endregion

                EquipmentModel em = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(msg.BODY.eqp_id.Trim());
                if (em == null)
                {
                    //error log
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<AreYouThereRequestReply>Find Error", msg.BODY.eqp_id.Trim()));
                    return;
                }
                #region 记录设备回复RETURNCODE NG的Log
                //if (!msg.RETURN.RETURNCODE.Equals("0"))
                //{
                //    BaseComm.LogMsg(Log.LogLevel.Warn, string.Format("设备<{0}>回复<{1}> 结果NG,回复内容<{2}> ", msg.BODY.eqp_id.Trim(),
                //       "DateTimeSyncReply", msg.RETURN.RETURNMESSAGE));
                //    return;
                //}
                #endregion
                #region Record Log
                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("<{2}>Socket Message<{0}> Recv OK , Content<{1}>",
                msg.HEADER.MESSAGENAME, msg.WriteToXml(), em.EQName));
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
