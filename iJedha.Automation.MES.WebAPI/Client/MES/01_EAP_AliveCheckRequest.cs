using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Environment;
using iJedha.Automation.EAP.Library;
using iJedha.Automation.EAP.Model;
using iJedha.Automation.EAP.ModelBase;
using System;
using System.Diagnostics;

namespace iJedha.Automation.EAP.WebAPI
{
    public partial class WebAPIReport : BaseComm
    {
        /// <summary>
        /// EAP对MES定时检查联机
        /// </summary>
        /// <param name="_data"></param>
        public void EAP_AliveCheckRequest(MessageModel.AliveCheckRequest _data, int Retrytime)
        {
            ErrorCodeModelBase errm = new ErrorCodeModelBase();
            try
            {
                #region  [Web API调用]
                string _outdata;
                if (ConvertJSON(_data, out _outdata))
                {
                    LogMsg(Log.LogLevel.Trace, string.Format("WebAPI Message<{0}> Send OK, Content<{1}>", System.Reflection.MethodBase.GetCurrentMethod().Name, _outdata));
                }
                if (!EAPEnvironment.commonLibrary.baseLib.apiClientParaLibrary.Client_Enable)
                {
                    LogMsg(Log.LogLevel.Warn, string.Format("WebAPI服务器设定关闭，停止消息发送"));
                    return;
                }
                
                var Client = new iJedha.Automation.EAP.Core.WebAPIClient();
                string _indata = Client.SendMessage(EAPEnvironment.commonLibrary.baseLib.apiClientParaLibrary.RemoteUrlString, System.Reflection.MethodBase.GetCurrentMethod().Name, _outdata);

                #region 回复超时
                if (_indata.Contains("操作超时"))
                {
                    string errMsg;
                    errMsg = string.Format("第{0}次发送<{1}>超时", Retrytime, System.Reflection.MethodBase.GetCurrentMethod().Name);
                    BaseComm.LogMsg(EAP.Core.Log.LogLevel.Warn, errMsg);
                    if (Retrytime < EAPEnvironment.commonLibrary.baseLib.apiClientParaLibrary.RetryCount)
                    {
                        Retrytime++;
                        EAP_AliveCheckRequest(_data, Retrytime);
                        return;
                    }
                    else
                    {
                        errMsg = string.Format("EAP发送<{0}>失败", System.Reflection.MethodBase.GetCurrentMethod().Name);
                        BaseComm.ErrorHandleRule("E0005", errMsg, ref errm);
                        return;
                    }
                }
                #endregion
                object returnobject;
                if (new Serialize().DeSerializeJSON(_indata, new MessageModelBase.ApiResult().GetType(), out returnobject))
                {
                    LogMsg(Log.LogLevel.Trace, string.Format("WebAPI Message<{0}> Recv OK, Content<{1}>", System.Reflection.MethodBase.GetCurrentMethod().Name, _indata));
                }
                MessageModelBase.ApiResult returnInfo = (MessageModelBase.ApiResult)returnobject;
                #endregion

                #region  [回传结果处理]
                if (returnInfo.strCode != "0000")
                {
                    string errMsg = string.Format("MES回复, 接口名称<{0}>, 错误代码<{1}>, 错误描述<{2}><{3}>", System.Reflection.MethodBase.GetCurrentMethod().Name, returnInfo.strCode, _data.MainEqpID, returnInfo.strMsg);
                    LogMsg(Log.LogLevel.Warn, errMsg);
                    return;
                }
                EAPEnvironment.commonLibrary.HostConnectMode = EAP.LibraryBase.eHostConnectMode.CONNECT;
                #endregion
            }
            catch (Exception e)
            {
                string errMsg;

                StackTrace st = new StackTrace(new StackFrame(true));
                StackFrame sf = st.GetFrame(0);
                errMsg = string.Format("EAP发送<{0}>异常：程序出错，代码行数<{1}>，ErrMsg<{2}>",
                System.Reflection.MethodBase.GetCurrentMethod().Name, sf.GetFileLineNumber(), e.Message);
                BaseComm.ErrorHandleRule("E0001", errMsg, ref errm);
                errMsg = errMsg = string.Format("MES API服务端连接断开.");
                BaseComm.ErrorHandleRule("E0004", errMsg, ref errm);
                EAPEnvironment.commonLibrary.HostConnectMode = EAP.LibraryBase.eHostConnectMode.DISCONNECT;

            }
        }

    }
}
