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
    public class GUI_CloseJobDataController : ApiController
    {
        /// <summary>
        /// GUI远程下载生产任务给设备 
        /// </summary>
        /// <param name="ApiRequest"></param>
        /// <returns></returns>
        [HttpPost]
        public MessageModelBase.ApiResult GUI_CloseJobData(MessageModelBase.ApiRequest ApiRequest)
        {
            MessageModelBase.ApiResult ri = new MessageModelBase.ApiResult();
            try
            {
                ErrorCodeModelBase errm = new ErrorCodeModelBase();
                #region [解析MES发送的消息]
                object apiobject;
                new Serialize().DeSerializeJSON(ApiRequest.Content, new MessageModel.GUI_CloseJobData().GetType(), out apiobject);
                MessageModel.GUI_CloseJobData GuiCloseJobData = (MessageModel.GUI_CloseJobData)apiobject;
                #endregion
                #region [Trace Log]
                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("WebAPI Message<{0}> Recv OK, Content<{1}>", System.Reflection.MethodBase.GetCurrentMethod().Name, ApiRequest.Content));
                #endregion

                string errMsg = string.Empty;
                LotModel lm = EAPEnvironment.commonLibrary.commonModel.GetLotModelByLotID(GuiCloseJobData.JobID);
                Lot lot = EAPEnvironment.commonLibrary.commonModel.GetLotModelBySubLotID(GuiCloseJobData.JobID);
                if (lm == null)
                {
                    //error log
                    BaseComm.LogMsg(Log.LogLevel.Error, $"GUI删除生产任务检查异常：任务<{GuiCloseJobData.JobID}>不存在，删除失败.");
                    errMsg = $"GUI删除生产任务检查异常：任务<{GuiCloseJobData.JobID}>不存在，删除失败.";
                    BaseComm.ErrorHandleRule("E2302", errMsg, ref errm);
                    ri.bSucc = false;
                    ri.strCode = "2302";
                    ri.strMsg = errMsg;
                    ri.DataTime = DateTime.Now;
                    return ri; ;
                }
                else
                {
                    if (EAPEnvironment.commonLibrary.lineModel.isSubLot)
                    {
                        Environment.EAPEnvironment.commonLibrary.commonModel.RemoveProcessSubLotModel(lot);
                    }
                    else
                    {
                        Environment.EAPEnvironment.commonLibrary.commonModel.RemoveProcessLotModel(lm);
                    }
                    ri.strCode = "0000";
                    ri.strMsg = "成功.";
                    ri.bSucc = true;
                    ri.DataTime = DateTime.Now;

                    return ri;
                }
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
