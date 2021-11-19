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
        /// EAP询问MES设备最新生产模式
        /// </summary>
        /// <param name="_data"></param>
        public void EAP_EqpModeRequest(MessageModel.EqpModeRequest _data, int Retrytime)
        {
            ErrorCodeModelBase errm = new ErrorCodeModelBase();
            try
            {
                if (EAPEnvironment.commonLibrary.HostConnectMode == EAP.LibraryBase.eHostConnectMode.DISCONNECT)
                {
                    return;
                }
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

                object returnobject;
                if (new Serialize().DeSerializeJSON(_indata, new MessageModelBase.ApiResult().GetType(), out returnobject))
                {
                    LogMsg(Log.LogLevel.Trace, string.Format("WebAPI Message<{0}> Recv OK, Content<{1}>", System.Reflection.MethodBase.GetCurrentMethod().Name, _indata));
                }
                MessageModelBase.ApiResult returnInfo = (MessageModelBase.ApiResult)returnobject;
                #endregion
                object ProductModeobject;
                try
                {
                    new EAP.Core.Serialize().DeSerializeJSON(returnInfo.Content.ToString(), new MessageModel.ProductionModeDo().GetType(), out ProductModeobject);
                }
                catch (Exception e)
                {
                    StackTrace st = new StackTrace(new StackFrame(true));
                    StackFrame sf = st.GetFrame(0);
                    string errMsg = string.Format("EAP发送<{0}>异常：程序出错，代码行数<{1}>",
                    System.Reflection.MethodBase.GetCurrentMethod().Name, sf.GetFileLineNumber());
                    EAP.Environment.BaseComm.ErrorHandleRule("E0001", errMsg, ref errm);
                    return;
                }
                MessageModel.ProductionModeDo productMode = (MessageModel.ProductionModeDo)ProductModeobject;
                if (returnInfo.strCode != "0000")
                {
                    string errMsg = string.Format("MES回复, 接口名称<{0}>, 错误代码<{1}>, 错误描述<{2}><{3}>", System.Reflection.MethodBase.GetCurrentMethod().Name, returnInfo.strCode, _data.MainEqpID, returnInfo.strMsg);
                    LogMsg(Log.LogLevel.Warn, errMsg);
                }
                else
                {
                    string errMessage = string.Empty;
                    if (string.IsNullOrEmpty(productMode.ProductionMode) )//如果MES回复空，EAP认定模式为Manual
                    {
                        productMode.ProductionMode = "Manual";
                    }
                    switch ((eProductMode)Enum.Parse(typeof(eProductMode), productMode.ProductionMode))
                    {
                        case eProductMode.Manual:
                            {
                                EAPEnvironment.commonLibrary.commonModel.PreMode = EAPEnvironment.commonLibrary.commonModel.CurMode;
                                EAPEnvironment.commonLibrary.commonModel.CurMode = EAP.Model.eProductMode.Manual;
                                #region [通知投收板机切Local]
                                foreach (var em in EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
                                {
                                    if (em.ControlMode == eControlMode.REMOTE && (em.Type == eEquipmentType.L || em.Type == eEquipmentType.U))
                                    {
                                        new HostService.HostService().ControlModeCommand(em.EQName, "1");//Local
                                    }
                                }
                                #endregion
                            }
                            break;
                        
                        case eProductMode.Auto:
                            {
                                EAPEnvironment.commonLibrary.commonModel.PreMode = EAPEnvironment.commonLibrary.commonModel.CurMode;
                                EAPEnvironment.commonLibrary.commonModel.CurMode = EAP.Model.eProductMode.Auto;
                                #region [通知投收板机切Remote]
                                foreach (var em in EAP.Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
                                {
                                    if (em.ControlMode != eControlMode.REMOTE && em.isControlModeSelect)
                                    {
                                        new HostService.HostService().ControlModeCommand(em.EQName, "2");//Remote
                                    }
                                }
                                #endregion
                            }
                            break;
                        default:
                            break;
                    }
                }

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
                        EAP_EqpModeRequest(_data, Retrytime);
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
