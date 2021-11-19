using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Environment;
using iJedha.Automation.EAP.Library;
using iJedha.Automation.EAP.Model;
using iJedha.Automation.EAP.ModelBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace iJedha.Automation.EAP.WebAPI
{
    public class GUI_JobDataDownloadController : ApiController
    {
        /// <summary>
        /// GUI远程下载生产任务给设备 
        /// </summary>
        /// <param name="ApiRequest"></param>
        /// <returns></returns>
        [HttpPost]
        public MessageModelBase.ApiResult GUI_JobDataDownload(MessageModelBase.ApiRequest ApiRequest)
        {
            MessageModelBase.ApiResult ri = new MessageModelBase.ApiResult();
            try
            {
                ErrorCodeModelBase errm = new ErrorCodeModelBase();
                #region [解析MES发送的消息]
                object apiobject;
                new Serialize().DeSerializeJSON(ApiRequest.Content, new MessageModel.GUI_JobDataDownload().GetType(), out apiobject);
                MessageModel.GUI_JobDataDownload GuiJobDataDownload = (MessageModel.GUI_JobDataDownload)apiobject;
                #endregion
                #region [Trace Log]
                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("WebAPI Message<{0}> Recv OK, Content<{1}>", System.Reflection.MethodBase.GetCurrentMethod().Name, ApiRequest.Content));
                #endregion

                string errMsg = string.Empty;
                var em = EAP.Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByName(GuiJobDataDownload.EqpName);
                if (em == null)
                {
                    errMsg = string.Format("GUI通知<{0}>生产任务下载检查异常：设备不存在，下载失败.", GuiJobDataDownload.EqpName);
                    BaseComm.ErrorHandleRule("E2301", errMsg, ref errm);
                    ri.bSucc = false;
                    ri.strCode = "2301";
                    ri.strMsg = errMsg;
                    ri.DataTime = DateTime.Now;
                    return ri;
                }
                else
                {
                    #region [给设备下发生产任务命令]
                    Environment.EAPEnvironment.commonLibrary.isGUITriggerDataDownload = true;
                    if (EAPEnvironment.commonLibrary.lineModel.isSubLot)
                    {
                        var subLot = Environment.EAPEnvironment.commonLibrary.commonModel.GetLotModelBySubLotID(GuiJobDataDownload.JobID);
                        if (subLot == null)
                        {
                            BaseComm.LogMsg(Log.LogLevel.Error, string.Format("GUI下载生产任务失败，不能找到Lot资讯1，LotID<{0}>.", GuiJobDataDownload.JobID));
                            BaseComm.ErrorHandleRule("E1001", errMsg, ref errm);
                            ri.strCode = "1001";
                            ri.strMsg = string.Format("GUI下载生产任务失败，不能找到Lot资讯，LotID<{0}>.", GuiJobDataDownload.JobID);
                            ri.bSucc = false;
                            ri.DataTime = DateTime.Now;
                        }
                        else
                        {
                            new HostService.HostService().JobDataDownload(em, subLot, "", Environment.EAPEnvironment.commonLibrary.commonModel.GetParameterModel_KL(em,  subLot));
                            BaseComm.LogMsg(Log.LogLevel.Info, string.Format("GUI下载<{0}>生产任务成功,生产任务为<{1}>.", GuiJobDataDownload.EqpName, GuiJobDataDownload.JobID));
                        }

                    }
                    else
                    {
                        var lm = Environment.EAPEnvironment.commonLibrary.commonModel.GetLotModelByLotID(GuiJobDataDownload.JobID);
                        if (lm == null)
                        {
                            BaseComm.LogMsg(Log.LogLevel.Error, string.Format("GUI下载生产任务失败，不能找到Lot资讯2，LotID<{0}>.", GuiJobDataDownload.JobID));
                            BaseComm.ErrorHandleRule("E1001", errMsg, ref errm);
                            ri.strCode = "1001";
                            ri.strMsg = string.Format("GUI下载生产任务失败，不能找到Lot资讯，LotID<{0}>.", GuiJobDataDownload.JobID);
                            ri.bSucc = false;
                            ri.DataTime = DateTime.Now;
                        }
                        else
                        {
                            ri.strCode = "0000";
                            ri.strMsg = "成功.";
                            ri.bSucc = true;
                            ri.DataTime = DateTime.Now;

                            lm.LotProcessStatus = eLotProcessStatus.Run;
                            new HostService.HostService().JobDataDownload(em, lm, "", Environment.EAPEnvironment.commonLibrary.commonModel.GetParameterModel(em, lm));
                            BaseComm.LogMsg(Log.LogLevel.Info, string.Format("GUI通知<{0}>生产任务成功,生产任务为<{1}>.", GuiJobDataDownload.EqpName, GuiJobDataDownload.JobID));
                        }
                    }
                }
                #endregion

               

                return ri;

            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                ri.bSucc = false;
                ri.strCode = "E0001";
                ri.strMsg = "程式出错.";
                ri.DataTime = DateTime.Now;
                return ri;
            }
            finally
            {
                #region [Trace Log]
                string _outdata;
                if (BaseComm.ConvertJSON(ri, out _outdata))
                {
                    BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("WebAPI Message<{0}> Send OK, Content<{1}>", System.Reflection.MethodBase.GetCurrentMethod().Name, _outdata));
                }
                #endregion
            }
        }
    }
}
