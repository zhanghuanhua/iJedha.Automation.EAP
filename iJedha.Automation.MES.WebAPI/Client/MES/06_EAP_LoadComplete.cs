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

namespace iJedha.Automation.EAP.WebAPI
{
    public partial class WebAPIReport : BaseComm
    {
        /// <summary>
        /// 上料请求完成
        /// </summary>
        /// <param name="_data"></param>
        public void EAP_LoadComplete(MessageModel.LoadComplete _data, EquipmentModel em, int Retrytime)
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
                    History.EAP_EQP_EVENTHISTORY(em, eEventName.EQP_LOADREQUEST, eEventFlow.EAP2MES, $"WebAPI Message<{System.Reflection.MethodBase.GetCurrentMethod().Name}> Send OK", em.CurrentLotID, _data.PortID);
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

                if (returnInfo.strCode != "0000")
                {
                    string errMsg = string.Format("MES回复, 接口名称<{0}>, 错误代码<{1}>, 错误描述<{2}><{3}>", System.Reflection.MethodBase.GetCurrentMethod().Name, returnInfo.strCode, em.EQName, returnInfo.strMsg);
                    EAP.Environment.BaseComm.ErrorHandleRule("E3007", errMsg, ref errm);
                    History.EAP_EQP_EVENTHISTORY(em, eEventName.EQP_LOADREQUEST, eEventFlow.MES2EAP, errMsg, em.CurrentLotID, _data.PortID);
                    return;
                }
                else
                {
                    //PP裁切机目前讨论是MES主动下载生产任务，如后续变动，再拿掉注释
                    if (em.isMultiPort)
                    {
                        //if (!EAPEnvironment.commonLibrary.commonModel.InLoadCompleteReportCheckStart)
                        //{
                        //    MessageModel.LotInfoRequest lotRequest = new MessageModel.LotInfoRequest();
                        //    lotRequest.MainEqpID = EAPEnvironment.commonLibrary.LineName;
                        //    lotRequest.SubEqpID = em.EQID;
                        //    lotRequest.PortID = _data.PortID;
                        //    lotRequest.CarrierID = _data.CarrierID;

                        //    new WebAPIReport().EAP_LotInfoRequest(lotRequest, em, 1);
                        //}
                    }
                    else
                    {
                        if (em.isMaterialLoadComplete)//融铆合PP请求物料信息
                        {
                            if (_data.PortID==ePortID.L01.ToString())//暂存口
                            {
                                MessageModel.MaterialLotInfoRequest lotRequest = new MessageModel.MaterialLotInfoRequest();
                                lotRequest.MainEqpID = EAPEnvironment.commonLibrary.MDLN;
                                lotRequest.SubEqpID = em.EQID;
                                lotRequest.PortID = _data.PortID;

                                new WebAPIReport().EAP_MaterialLotInfoRequest(lotRequest, em, 1);
                            }
                            else
                            {
                                #region 融铆合PP上料口上报LoadComplete时，触发物料上机功能
                                //获取设备对应上报Carrier ID
                                var cID = (from q in Environment.EAPEnvironment.commonLibrary.commonModel.Dic_CarrierIDList where q.Key == em.EQID select q.Value).FirstOrDefault();
                                new WebAPIReport().EAP_MaterialSetUp(new MessageModel.MaterialSetUp()
                                {
                                    SubEqpID = em.EQID,
                                    MaterialSN = "",
                                    CarrierID = cID
                                }, 1);
                                #endregion
                            }

                        }
                        else
                        {
                            string err;
                            //暂存口才向MES请求任务信息
                            if (_data.PortID==ePortID.L01.ToString())
                            {
                                MessageModel.LotInfoRequest lotRequest = new MessageModel.LotInfoRequest();
                                lotRequest.MainEqpID = EAPEnvironment.commonLibrary.MDLN;
                                lotRequest.SubEqpID = em.EQID;
                                lotRequest.PortID = _data.PortID;
                                lotRequest.CarrierID = _data.CarrierID;

                                new WebAPIReport().EAP_LotInfoRequest(lotRequest, em, 1,out err);
                            }
                            
                        }
                        
                    }
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
                        EAP_LoadComplete(_data, em, Retrytime);
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
