//******************************************************************
//   系统名称 : iJedha.Automation.EAP.SocketService
//   文件概要 : 
//   作    者 : 张明哲
//   <更新履历>
//   1.0.0.0    2020/10/1 14:28:28
//******************************************************************
using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Environment;
using System;

namespace iJedha.Automation.EAP.EQPService
{
    public partial class CIMMessageCommandReply : BaseComm
    {
        public void EventHandle(string evtXml)
        {
            try
            {
                #region Decode Message
                SocketMessageStructure.CIMMessageCommandReply rpy = new SocketMessageStructure.CIMMessageCommandReply();
                if (new Serialize<SocketMessageStructure.CIMMessageCommandReply>().DeSerializeXML(evtXml, out rpy) == false)
                {
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Socket Message<{0}> Decode Error, Content<{1}>", "CIMMessageCommandReply", evtXml));
                    return;
                }
                #endregion
                #region 记录设备回复RETURNCODE NG的Log
                //if (!rpy.RETURN.RETURNCODE.Equals("0"))
                //{
                //    BaseComm.LogMsg(Log.LogLevel.Warn, string.Format("设备<{0}>回复<{1}> 结果NG,回复内容<{2}> ", rpy.BODY.eqp_id.Trim(),
                //        "CIMMessageCommandReply", rpy.RETURN.RETURNMESSAGE));
                //    return;
                //}
                #endregion
                #region Record Log
                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("<{2}>Socket Message<{0}> Recv OK , Content<{1}>",
                "CIMMessageCommandReply", rpy.WriteToXml(), rpy.BODY.eqp_id.Trim()));
                #endregion
                #region 记录设备回复NG的Log
                if (rpy.BODY.return_code.Equals("0"))
                {
                    BaseComm.LogMsg(Log.LogLevel.Warn, string.Format("设备回复<{0}> 结果NG ", "CIMMessageCommandReply"));
                }
                #endregion
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", "CIMMessageCommandReply", ex.Message.ToString(), ex.StackTrace.ToString()));
            }
            finally
            {

            }
        }
    }
}
