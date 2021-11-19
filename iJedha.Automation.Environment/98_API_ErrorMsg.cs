using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Environment;
using iJedha.Automation.EAP.Library;
using iJedha.Automation.EAP.Model;
using iJedha.Automation.EAP.ModelBase;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iJedha.Automation.EAP.Environment
{
    public partial class WebAPIServerClient : BaseComm
    {
        /// <summary>
        /// 发送异常信息到WebAPIServer
        /// </summary>
        public void API_ErrorMsg(MessageModel.ErrorMsg _data, int Retrytime)
        {
            ErrorCodeModelBase errm = new ErrorCodeModelBase();
            try
            {
                string _outdata;
                if (ConvertJSON(_data, out _outdata))
                {
                    LogMsg(Log.LogLevel.Trace, string.Format("WebAPI Message<{0}> Send OK，content<{1}>", System.Reflection.MethodBase.GetCurrentMethod().Name, _outdata));
                }
                if (!EAPEnvironment.commonLibrary.webAPIParaLibrary.Client_Enable)
                {
                    return;
                }
                #region [发送ERROR 信息]
                var Client = new APIClient.Client();
                Client.Url = string.Format("{0}/api/API_ErrorMsg/API_ErrorMsg", EAPEnvironment.commonLibrary.webAPIParaLibrary.WebAPIServerUrlString);
                Client.Prams.Add("From", "EAP");
                Client.Prams.Add("Message", System.Reflection.MethodBase.GetCurrentMethod().Name);
                Client.Prams.Add("DateTime", string.Format("{0:yyyyMMddHHmmss}", DateTime.Now));
                Client.Prams.Add("Content", _outdata);
                Client.Post();
                #endregion

                string _indata = Client.Result;
                object returnobject;
                if (new EAP.Core.Serialize().DeSerializeJSON(_indata, new MessageModel.ApiResult().GetType(), out returnobject))
                {
                    LogMsg(Log.LogLevel.Trace, string.Format("WebAPI Message<{0}> Recv OK, Content<{1}>", System.Reflection.MethodBase.GetCurrentMethod().Name, _indata));
                }
                MessageModel.ApiResult returnInfo = (MessageModel.ApiResult)returnobject;

                if (returnInfo.strCode != "0000")
                {
                    string errMsg = string.Format("MES回复, 接口名称<{0}>, 错误代码<{1}>, 错误描述<{2}>",
                        System.Reflection.MethodBase.GetCurrentMethod().Name, returnInfo.strCode, returnInfo.strMsg);
                    LogMsg(Log.LogLevel.Warn, errMsg);
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                string errMsg;
                if (ex.Message.ToString() == ConstLibrary.CONST_WEBAPI_TIMEOUT_KEYWORD)
                {
                    errMsg = string.Format("第{0}次发送<{1}>超时", Retrytime, System.Reflection.MethodBase.GetCurrentMethod().Name);
                    BaseComm.LogMsg(EAP.Core.Log.LogLevel.Warn, errMsg);
                    if (Retrytime < EAPEnvironment.commonLibrary.webAPIParaLibrary.RemoteRetrytime)
                    {
                        Retrytime++;
                        API_ErrorMsg(_data, Retrytime);
                    }
                    else
                    {
                        errMsg = string.Format("EAP发送<{0}>失败", System.Reflection.MethodBase.GetCurrentMethod().Name);
                        EAP.Environment.BaseComm.ErrorHandleRule("E0005", errMsg, ref errm);
                    }
                }
                else
                {
                    StackTrace st = new StackTrace(new StackFrame(true));
                    StackFrame sf = st.GetFrame(0);
                    errMsg = string.Format("EAP发送<{0}>异常：程序出错，代码行数<{1}>，ErrMsg<{2}>",
                    System.Reflection.MethodBase.GetCurrentMethod().Name, sf.GetFileLineNumber(), ex.Message);
                    BaseComm.LogMsg(EAP.Core.Log.LogLevel.Warn, errMsg);
                }
            }
        }
    }
}
