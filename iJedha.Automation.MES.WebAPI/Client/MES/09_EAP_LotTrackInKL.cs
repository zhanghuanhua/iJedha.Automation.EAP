using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Environment;
using iJedha.Automation.EAP.Library;
using iJedha.Automation.EAP.Model;
using iJedha.Automation.EAP.ModelBase;
using System;
using System.Diagnostics;
using System.Linq;

namespace iJedha.Automation.EAP.WebAPI
{
    public partial class WebAPIReport : BaseComm
    {
        /// <summary>
        /// 在制品上机
        /// </summary>
        /// <param name="_data"></param>
        /// <param name="lot"></param>
        public void EAP_LotTrackIn(MessageModel.LotTrackIn _data, Lot lot, int Retrytime,out string err)
        {
            ErrorCodeModelBase errm = new ErrorCodeModelBase();
            err = "";
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

                EquipmentModel em = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(_data.SubEqpID);
                EquipmentModel uem = (from n in EAP.Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel() where n.Type == eEquipmentType.U select n).FirstOrDefault();
                PortModel port = em.GetPortModelByPortID(_data.PortID);
                if (returnInfo.strCode != "0000")
                {
                    string errMsg = string.Format("E3002:MES回复, 接口名称[{0}], MES错误代码[{1}], MES错误描述[{2}][{3}]", System.Reflection.MethodBase.GetCurrentMethod().Name, returnInfo.strCode, _data.SubEqpID, returnInfo.strMsg);
                    EAP.Environment.BaseComm.ErrorHandleRule("E3002", errMsg, ref errm);
                    err = errMsg;
                    #region [反馈异常结果]
                    new HostService.HostService().CIMMessageCommand(em.EQID,"10",errMsg,DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                    #endregion

                }
                else
                {
                    #region [下载开始生产命令]
                    new HostService.HostService().RemoteControlCommand(em.EQID, _data.PortID, eRemoteCommand.Start.GetEnumDescription());
                    #endregion

                    lock (lot)
                    {
                        lot.LotProcessStatus = eLotProcessStatus.Run;
                        //lot.ProcessTime = DateTime.Now;
                    }
                    //add Process Lot
                    EAP.Environment.EAPEnvironment.commonLibrary.commonModel.AddProcessLotModelKL((Lot)lot.Clone());
                    
                    new EAP.MQService.RBMQService().MQ_LotInformation(lot.LotID, lot.PN, "", lot.PanelTotalQty.ToString(), "上机完成", "自动上机");
                    EAPEnvironment.commonLibrary.MainLotID = lot.LotID;
                    EAPEnvironment.commonLibrary.ShowLotInfoMessage = lot.LotStatus = eLotinfo.PartUp.ToString();
                    int inputCount = lot.PanelTotalQty;
                    LogMsg(Log.LogLevel.Info, string.Format("Lot<{0}>产品<{1}>数量<{2}>上机完成.", lot.LotID, lot.PN, lot.PanelTotalQty));
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
                        EAP_LotTrackIn(_data, lot, Retrytime,out errMsg);
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
                err = errMsg;
                #endregion
            }
        }
    }
}
