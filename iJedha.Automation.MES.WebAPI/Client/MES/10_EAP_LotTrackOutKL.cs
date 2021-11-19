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
        /// 在制品下机
        /// </summary>
        /// <param name="_data"></param>
        /// <param name="lot"></param>
        public void EAP_LotTrackOut(MessageModel.LotTrackOut _data, Lot lot, EquipmentModel em, int Retrytime,out string err)
        {
            ErrorCodeModelBase errm = new ErrorCodeModelBase();
            err = "";
            try
            {
                if (EAPEnvironment.commonLibrary.HostConnectMode == EAP.LibraryBase.eHostConnectMode.DISCONNECT)
                {
                    err = $"MES连线状态为{EAP.LibraryBase.eHostConnectMode.DISCONNECT}";
                    return;
                }

                #region  [Web API调用]
                string _outdata;
                if (ConvertJSON(_data, out _outdata))
                {
                    LogMsg(Log.LogLevel.Trace, string.Format("WebAPI Message<{0}> Send OK, Content<{1}>", System.Reflection.MethodBase.GetCurrentMethod().Name, _outdata));
                    History.EAP_EQP_EVENTHISTORY(em, eEventName.EAP_TRACKOUT, eEventFlow.EAP2MES, $"WebAPI Message<{System.Reflection.MethodBase.GetCurrentMethod().Name}> Send OK", em.CurrentLotID, _data.PortID);
                }
                if (!EAPEnvironment.commonLibrary.baseLib.apiClientParaLibrary.Client_Enable)
                {
                    err = "WebAPI服务器设定关闭，停止消息发送";
                    LogMsg(Log.LogLevel.Warn,err);
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
                    string errMsg = string.Format("MES回复, 接口名称<{0}>, 错误代码<{1}>, 错误描述<{2}><{3}>", System.Reflection.MethodBase.GetCurrentMethod().Name, returnInfo.strCode, _data.SubEqpID, returnInfo.strMsg);
                    err = errMsg;
                    EAP.Environment.BaseComm.ErrorHandleRule("E3004", errMsg, ref errm);
                    History.EAP_EQP_EVENTHISTORY(em, eEventName.EAP_TRACKOUT, eEventFlow.MES2EAP, errMsg, em.CurrentLotID, _data.PortID);
                    return;
                }
                else
                {
                    Environment.EAPEnvironment.commonLibrary.commonModel.RemoveProcessSubLotModel(lot);

                    LogMsg(Log.LogLevel.Info, string.Format("Lot<{0}>产品<{1}>数量<{2}>下机完成.", _data.LotID, lot.PN, _data.JobTotalQty));
                    new EAP.MQService.RBMQService().MQ_LotInformation(_data.LotID, lot.PN, "", lot.PanelTotalQty.ToString(), "上机完成", "自动下机");
                    EAPEnvironment.commonLibrary.MainLotID = _data.LotID;
                    EAPEnvironment.commonLibrary.ShowLotInfoMessage = lot.LotStatus = eLotinfo.WaitiongLower.ToString();
                }

                //lock (lot) lot.LotProcessStatus = eLotProcessStatus.Complete;
                //eRequestType _requestType;
                //_requestType = eRequestType.Full;
                // 8/6 Add 拆批
                //string meslotid;

                //if (returnInfo.Content == null || string.IsNullOrEmpty(returnInfo.Content.ToString()))
                //{
                //    meslotid = _data.LotID;
                //}
                //else
                //{
                //    meslotid = returnInfo.Content.ToString();
                //}

                //new WebAPIReport().EAP_UnLoadRequest(new MessageModel.UnLoadRequest()
                //{
                //    MainEqpID = EAPEnvironment.commonLibrary.LineName,
                //    SubEqpID = _data.SubEqpID,
                //    PortID = _data.PortID,
                //    CarrierID = "",
                //    RequestType = _requestType.ToString(),
                //    LotID = meslotid
                //}, em, 1);

                History.EAP_EQP_EVENTHISTORY(em, eEventName.EAP_TRACKOUT, eEventFlow.MES2EAP, "", em.CurrentLotID, _data.PortID);
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
                        EAP_LotTrackOut(_data, lot,em, Retrytime,out errMsg);
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
