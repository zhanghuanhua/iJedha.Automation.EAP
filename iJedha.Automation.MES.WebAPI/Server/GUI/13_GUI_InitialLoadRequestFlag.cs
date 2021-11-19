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
    public class GUI_InitialLoadRequestFlagController : ApiController
    {
        /// <summary>
        /// GUI初始化连续呼叫AGV卡控
        /// </summary>
        /// <param name="ApiRequest"></param>
        /// <returns></returns>
        [HttpPost]
        public MessageModelBase.ApiResult GUI_InitialLoadRequestFlag(MessageModelBase.ApiRequest ApiRequest)
        {
            MessageModelBase.ApiResult ri = new MessageModelBase.ApiResult();
            try
            {
                ErrorCodeModelBase errm = new ErrorCodeModelBase();
                #region [解析MES发送的消息]
                object apiobject;
                new Serialize().DeSerializeJSON(ApiRequest.Content, new MessageModel.GUI_InitialLoadRequestFlag().GetType(), out apiobject);
                MessageModel.GUI_InitialLoadRequestFlag GuiInitial = (MessageModel.GUI_InitialLoadRequestFlag)apiobject;
                #endregion
                #region [Trace Log]
                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("WebAPI Message<{0}> Recv OK, Content<{1}>", System.Reflection.MethodBase.GetCurrentMethod().Name, ApiRequest.Content));
                #endregion

                EAPEnvironment.commonLibrary.Dic_IsCallAgv.Clear();
                
                ri.strCode = "0000";
                ri.strMsg = "成功.";
                ri.bSucc = true;
                ri.DataTime = DateTime.Now;
                BaseComm.LogMsg(Log.LogLevel.Info, string.Format("GUI初始化连续呼叫AGV卡控."));
                return ri;
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                ri.bSucc = false;
                ri.strCode = "E0001";
                ri.strMsg = $"程式出错.{ex.Message}";
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
