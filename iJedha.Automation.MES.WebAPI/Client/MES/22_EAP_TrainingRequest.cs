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
        /// 在制品上机
        /// </summary>
        /// <param name="_data"></param>
        /// <param name="lot"></param>
        public void EAP_TrainingRequest(MessageModel.TrainingRequest _data,  int Retrytime,string transactionID)
        {
            ErrorCodeModelBase errm = new ErrorCodeModelBase();
            try
            {
                if (EAPEnvironment.commonLibrary.HostConnectMode == EAP.LibraryBase.eHostConnectMode.DISCONNECT)
                {
                    BaseComm.LogMsg(Log.LogLevel.Warn,$"MES连接状态为{EAP.LibraryBase.eHostConnectMode.DISCONNECT}");
                    //MES断线，直接回复设备验证通过。
                    new HostService.HostService().OperatorLoginConfirm(_data.Employee, transactionID, _data.SubEqpID, "1");//0:NG 1:OK
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

                EquipmentModel em = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(_data.SubEqpID);
                if (em==null)
                {
                    //error log
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<EAP_TrainingRequest> Find Error", _data.SubEqpID));
                }
                
                if (returnInfo.strCode != "0000")
                {
                    string errMsg = string.Format("E3002:MES回复, 接口名称[{0}], 错误代码[{1}], 错误描述[{2}][{3}]", System.Reflection.MethodBase.GetCurrentMethod().Name, returnInfo.strCode, _data.SubEqpID, returnInfo.strMsg);
                    EAP.Environment.BaseComm.ErrorHandleRule("E3002", errMsg, ref errm);
                    //如果失败，通知设备登录失败
                    new HostService.HostService().CIMMessageCommand(em.EQID,"10",errMsg, DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                    new HostService.HostService().OperatorLoginConfirm(_data.Employee, transactionID, _data.SubEqpID, "0");//0:NG 1:OK
                }
                else
                {
                    LogMsg(Log.LogLevel.Info, string.Format("MES反馈人员<{0}>可以在<{1}>上机.", _data.Employee, _data.SubEqpID));
                    //如果成功，通知设备登录成功
                    new HostService.HostService().OperatorLoginConfirm(_data.Employee, transactionID, _data.SubEqpID, "1");//0:NG 1:OK
                    
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
                        EAP_TrainingRequest(_data, Retrytime, transactionID);
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
