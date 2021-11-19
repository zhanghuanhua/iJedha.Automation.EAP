//******************************************************************
//   系统名称 : iJedha.Automation.EAP.WebAPIService
//   文件概要 : EAPEnvironment
//   作    者 : 张明哲
//   <更新履历>
//   1.0.0.0    2020/2/6 10:40:34
//******************************************************************
using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Environment;
using iJedha.Automation.EAP.Model;
using iJedha.Automation.EAP.ModelBase;
using System;
using System.Web.Http;

namespace iJedha.Automation.EAP.WebAPI
{
    public class AGV_TransferCompleteReportController : ApiController
    {
        /// <summary>
        /// AGV搬送完成上报
        /// </summary>
        /// <param name="ApiRequest"></param>
        /// <returns></returns>
        [HttpPost]
        public RCSResult AGV_TransferCompleteReport(MessageModelBase.ApiRequest ApiRequest)
        {
            RCSResult ri = new RCSResult();
            try
            {
                ErrorCodeModelBase errm = new ErrorCodeModelBase()  ;
                #region [解析MES发送的消息]
                object apiobject;
                new Serialize().DeSerializeJSON(ApiRequest.Content, new MessageModel.TransferComplete().GetType(), out apiobject);
                MessageModel.TransferComplete transferComplete = (MessageModel.TransferComplete)apiobject;
                #endregion
                #region [Trace Log]
                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("WebAPI Message<{0}> Recv OK, Content<{1}>", System.Reflection.MethodBase.GetCurrentMethod().Name, ApiRequest.Content));
                #endregion
                string errMsg = string.Empty;
                var em = EAP.Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(transferComplete.eqp_id);
                if (EAPEnvironment.commonLibrary.lineModel.isMainEqpID)
                {
                    ri.Code = "0";
                    ri.Msg = "允许派送.";
                    ri.Succ = true;
                    ri.DataTime = DateTime.Now;
                    ri.Content = "OK";
                }
                else
                {
                    if (em == null)
                    {
                        errMsg = string.Format("设备<{0}>检查异常：设备不存在，派送失败.", transferComplete.eqp_id);
                        BaseComm.ErrorHandleRule("E2301", errMsg, ref errm);
                        ri.Succ = false;
                        ri.Code = "E2301";
                        ri.Msg = errMsg;
                        ri.DataTime = DateTime.Now;
                        ri.Content = "NG";
                        return ri;
                    }
                    else
                    {
                        #region [给设备下发RemoteControlCommand命令]
                        new HostService.HostService().RemoteControlCommand(em.EQID, transferComplete.port_id, eRemoteCommand.AGVTransferComplete.GetEnumDescription());
                        BaseComm.LogMsg(Log.LogLevel.Info, "AGV搬送成功.");
                        bool outValue;

                        if (EAPEnvironment.commonLibrary.Dic_IsCallAgv.ContainsKey(transferComplete.port_id))
                        {
                            EAPEnvironment.commonLibrary.Dic_IsCallAgv.TryRemove(transferComplete.port_id, out outValue);
                        }

                        PortModel pm = em.GetPortModelByPortID(transferComplete.port_id);
                        if (pm != null)
                        {
                            ri.Code = "0";
                            ri.Msg = "AGV搬送成功.";
                            ri.Succ = true;
                            ri.DataTime = DateTime.Now;
                            ri.Content = "OK";

                            pm.PortStatus = ePortStatus.LOADCOMPLETE;
                        }
                        else
                        {
                            ri.Code = "0001";
                            ri.Msg = $"找不到Port ID为<{transferComplete.port_id}>的Port信息.";
                            ri.Succ = true;
                            ri.DataTime = DateTime.Now;
                            ri.Content = "NG";
                        }
                        #endregion
                    }
                }
                History.EAP_EQP_EVENTHISTORY(em, eEventName.AGV_TRANSFERCOMPLETEREPORT, eEventFlow.AGV2EAP, ApiRequest.Content, em.CurrentLotID, transferComplete.port_id);
                return ri;
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                ri.Succ = false;
                ri.Code = "E0001";
                ri.Msg = "程式出错.";
                ri.DataTime = DateTime.Now;
                ri.Content = "NG";
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
