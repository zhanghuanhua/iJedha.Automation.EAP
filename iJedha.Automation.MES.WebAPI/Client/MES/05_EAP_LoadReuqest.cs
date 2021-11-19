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
        /// 上料请求
        /// </summary>
        public void EAP_LoadRequest(MessageModel.LoadRequest _data, EquipmentModel em, int Retrytime, out string err)
        {
            ErrorCodeModelBase errm = new ErrorCodeModelBase();
            err = "";
            try
            {
                if (EAPEnvironment.commonLibrary.HostConnectMode == EAP.LibraryBase.eHostConnectMode.DISCONNECT)
                {
                    err = $"MES连线状态为{EAP.LibraryBase.eHostConnectMode.DISCONNECT}.";
                    return;
                }
                #region  [Web API调用]
                string _outdata;
                if (ConvertJSON(_data, out _outdata))
                {
                    LogMsg(Log.LogLevel.Trace, string.Format("WebAPI Message<{0}> Send OK, Content<{1}>", System.Reflection.MethodBase.GetCurrentMethod().Name, _outdata));
                    History.EAP_EQP_EVENTHISTORY(em, eEventName.EQP_LOADREQUEST, eEventFlow.EAP2MES, $"WebAPI Message<{System.Reflection.MethodBase.GetCurrentMethod().Name}> Send OK", em.CurrentLotID, _data.PortID);
                }
                if (!EAPEnvironment.commonLibrary.baseLib.apiClientParaLibrary.Client_Enable)
                {
                    err = "WebAPI服务器设定关闭，停止消息发送";
                    LogMsg(Log.LogLevel.Warn, err);
                    return;
                }
                var Client = new iJedha.Automation.EAP.Core.WebAPIClient();
                string _indata = Client.SendMessage(EAPEnvironment.commonLibrary.baseLib.apiClientParaLibrary.RemoteUrlString, System.Reflection.MethodBase.GetCurrentMethod().Name, _outdata);

                object returnobject;
                if (new Serialize().DeSerializeJSON(_indata, new MessageModelBase.ApiResult().GetType(), out returnobject))
                {
                    LogMsg(Log.LogLevel.Trace, string.Format("WebAPI Message<{0}> Recv OK, Content<{1}>", System.Reflection.MethodBase.GetCurrentMethod().Name, _indata));
                }
                MessageModelBase.ApiResult returnInfo = (MessageModelBase.ApiResult)returnobject;
                #endregion

                if (returnInfo.strCode != "0000")
                {
                    string errMsg = string.Format("MES回复, 接口名称<{0}>, MES错误代码<{1}>, MES错误描述<{2}>", System.Reflection.MethodBase.GetCurrentMethod().Name, returnInfo.strCode, returnInfo.strMsg);
                    err = errMsg;
                    EAP.Environment.BaseComm.ErrorHandleRule("E3001", errMsg, ref errm);
                    History.EAP_EQP_EVENTHISTORY(em, eEventName.EQP_LOADREQUEST, eEventFlow.MES2EAP, errMsg, em.CurrentLotID, _data.PortID);
                    return;
                }
                History.EAP_EQP_EVENTHISTORY(em, eEventName.EQP_LOADREQUEST, eEventFlow.MES2EAP, "", em.CurrentLotID, _data.PortID);
            }
            catch (Exception e)
            {
                #region [超时及异常处理]
                string errMsg;
                if (e.Message.ToString() == ConstLibrary.CONST_WEBAPI_TIMEOUT_KEYWORD)
                {
                    errMsg = string.Format("第{0}次发送<{1}>超时", Retrytime, System.Reflection.MethodBase.GetCurrentMethod().Name);
                    BaseComm.LogMsg(EAP.Core.Log.LogLevel.Warn, errMsg);
                    if (Retrytime < EAPEnvironment.commonLibrary.baseLib.apiClientParaLibrary.RetryCount)
                    {
                        Retrytime++;
                        EAP_LoadRequest(_data, em, Retrytime, out errMsg);
                    }
                    else
                    {
                        errMsg = string.Format("EAP发送<{0}>失败", System.Reflection.MethodBase.GetCurrentMethod().Name);
                        BaseComm.ErrorHandleRule("E0005", errMsg, ref errm);
                    }
                }
                else
                {
                    StackTrace st = new StackTrace(new StackFrame(true));
                    StackFrame sf = st.GetFrame(0);
                    errMsg = string.Format("EAP发送<{0}>异常：程序出错，代码行数<{1}>，ErrMsg<{2}>",
                    System.Reflection.MethodBase.GetCurrentMethod().Name, sf.GetFileLineNumber(), e.Message);
                    err = errMsg;
                    BaseComm.ErrorHandleRule("E0001", errMsg, ref errm);
                    errMsg = errMsg = string.Format("MES API服务端连接断开.");
                    BaseComm.ErrorHandleRule("E0004", errMsg, ref errm);
                    EAPEnvironment.commonLibrary.HostConnectMode = EAP.LibraryBase.eHostConnectMode.DISCONNECT;
                }

                #endregion
            }
        }
    }
}
