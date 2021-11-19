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
using System.Linq;

namespace iJedha.Automation.EAP.EQPService
{
    public partial class JobDataDownloadReply : BaseComm
    {
        public void EventHandle(string evtXml)
        {
            try
            {
                #region Decode Message
                SocketMessageStructure.JobDataDownloadReply rpy = new SocketMessageStructure.JobDataDownloadReply();
                if (new Serialize<SocketMessageStructure.JobDataDownloadReply>().DeSerializeXML(evtXml, out rpy) == false)
                {
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Socket Message<{0}> Decode Error, Content<{1}>", "JobDataDownloadReply", evtXml));
                    return;
                }
                #endregion

                #region Record Log
                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("<{2}>Socket Message<{0}> Recv OK , Content<{1}>",
                "JobDataDownloadReply", rpy.WriteToXml(), rpy.BODY.eqp_id.Trim()));
                #endregion

                #region [如果设定档JobDataDownloadReplyCondition=0，直接用JobDataDownloadReply来做回复结果]
                var em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.JobDataDownloadReplyCondition == "0" && r.EQID == rpy.BODY.eqp_id.Trim()).FirstOrDefault();
                if (em!=null)
                {
                    em.JobDataDownloadChangeResult = Model.eCheckResult.ok;
                }
                #endregion

                var equipment = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(rpy.BODY.eqp_id.Trim());
                #region 记录设备回复NG的Log
                if (rpy.BODY.return_code.Equals("0"))
                {
                    BaseComm.LogMsg(Log.LogLevel.Warn, string.Format("设备回复<{0}> 结果NG ", "JobDataDownloadReply"));
                    if (equipment!=null)
                    {
                        equipment.JobDataDownloadChangeResult = Model.eCheckResult.ng;
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", "JobDataDownloadReply", ex.Message.ToString(), ex.StackTrace.ToString()));
            }
            finally
            {

            }
        }
    }
}
