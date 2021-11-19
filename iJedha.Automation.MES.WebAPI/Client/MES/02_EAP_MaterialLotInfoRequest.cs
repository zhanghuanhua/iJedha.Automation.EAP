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
        /// 物料批次实体载具上到投板机时，EAP需要知道该载具里面的批次信息
        /// </summary>
        /// <param name="_indata"></param>
        /// <param name="em"></param>
        public void EAP_MaterialLotInfoRequest(MessageModel.MaterialLotInfoRequest _indata, EquipmentModel em, int Retrytime)
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
                if (ConvertJSON(_indata, out _outdata))
                {
                    LogMsg(Log.LogLevel.Info, string.Format("WebAPI Message<{0}> Send OK, Content<{1}>", System.Reflection.MethodBase.GetCurrentMethod().Name, _outdata));
                }
                if (!EAPEnvironment.commonLibrary.baseLib.apiClientParaLibrary.Client_Enable)
                {
                    LogMsg(Log.LogLevel.Warn, string.Format("WebAPI服务器设定关闭，停止消息发送"));
                    return;
                }

                var Client = new iJedha.Automation.EAP.Core.WebAPIClient();
                string _returndata = Client.SendMessage(EAPEnvironment.commonLibrary.baseLib.apiClientParaLibrary.RemoteUrlString, System.Reflection.MethodBase.GetCurrentMethod().Name, _outdata);

                object returnobject;
                if (new Serialize().DeSerializeJSON(_returndata, new MessageModelBase.ApiResult().GetType(), out returnobject))
                {
                    LogMsg(Log.LogLevel.Trace, string.Format("WebAPI Message<{0}> Recv OK, Content<{1}>", System.Reflection.MethodBase.GetCurrentMethod().Name, _returndata));
                }
                MessageModelBase.ApiResult returnInfo = (MessageModelBase.ApiResult)returnobject;
                #endregion

                if (returnInfo.strCode != "0000")
                {
                    string errMsg = string.Format("E3002:MES回复, 接口名称[{0}], 错误代码[{1}], 错误描述[{2}][{3}]", System.Reflection.MethodBase.GetCurrentMethod().Name, returnInfo.strCode, _indata.SubEqpID, returnInfo.strMsg);
                    EAP.Environment.BaseComm.ErrorHandleRule("E3002", errMsg, ref errm);

                    new HostService.HostService().CIMMessageCommand(em.EQID, "10", errMsg, DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                    return;
                }
                
                object lotinfoobject;
                try
                {
                    new EAP.Core.Serialize().DeSerializeJSON(returnInfo.Content.ToString(), new MessageModel.MaterialLotInfo().GetType(), out lotinfoobject);
                }
                catch (Exception e)
                {
                    StackTrace st = new StackTrace(new StackFrame(true));
                    StackFrame sf = st.GetFrame(0);
                    string errMsg = string.Format("EAP发送<{0}>异常：程序出错，代码行数<{1}>，ErrMsg<{2}>",
                    System.Reflection.MethodBase.GetCurrentMethod().Name, sf.GetFileLineNumber(),
                    string.Format("设备<{0}>Lotinfo下载失败：MES传输数据错误.", em.EQID));
                    EAP.Environment.BaseComm.ErrorHandleRule("E0001", errMsg, ref errm);
                    return;
                }

                MessageModel.MaterialLotInfo lotinfo = (MessageModel.MaterialLotInfo)lotinfoobject;
               
                if (em.ControlMode == eControlMode.REMOTE)
                {
                  
                    #region [下载生产任务给PP侧向投板机]
                    new HostService.HostService().JobDataDownload(em, lotinfo.CarrierID, lotinfo.Direction,lotinfo.PortID);
                    #endregion

                    //存储绑定对应设备上的载具ID是什么，供融铆合PP上料口上料时使用
                    EAPEnvironment.commonLibrary.commonModel.AddCarrierInfo(em.EQID,lotinfo.CarrierID);

                    switch (lotinfo.Direction)//1：正向；0：逆向
                    {
                        case "0"://negative
                            //EAPEnvironment.commonLibrary.Negative++;
                            if (EAPEnvironment.commonLibrary.Negative.ContainsKey(em.EQID))
                            {
                                EAPEnvironment.commonLibrary.Negative.Remove(em.EQID);
                            }
                            EAPEnvironment.commonLibrary.Negative.Add(em.EQID, em.EQID);
                            break;
                        case "1"://positive
                            //EAPEnvironment.commonLibrary.Positive++;
                            if (EAPEnvironment.commonLibrary.Positive.ContainsKey(em.EQID))
                            {
                                EAPEnvironment.commonLibrary.Positive.Remove(em.EQID);
                            }
                            EAPEnvironment.commonLibrary.Positive.Add(em.EQID, em.EQID);
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
                        EAP_MaterialLotInfoRequest(_indata, em, Retrytime);
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
