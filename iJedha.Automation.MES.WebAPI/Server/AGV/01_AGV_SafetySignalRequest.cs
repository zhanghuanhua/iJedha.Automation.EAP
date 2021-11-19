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
    public class AGV_SafetySignalRequestController : ApiController
    {
        /// <summary>
        /// AGV请求EAP安全信号
        /// </summary>
        /// <param name="ApiRequest"></param>
        /// <returns></returns>
        [HttpPost]
        public RCSResult AGV_SafetySignalRequest(MessageModelBase.ApiRequest ApiRequest)
        {

            RCSResult ri = new RCSResult();
            MessageModelBase.ApiResult aa = new MessageModelBase.ApiResult();
            try
            {
                ErrorCodeModelBase errm = new ErrorCodeModelBase();
                #region [解析MES发送的消息]
                object apiobject;
                new Serialize().DeSerializeJSON(ApiRequest.Content, new MessageModel.SafetySignal().GetType(), out apiobject);
                MessageModel.SafetySignal safetySignal = (MessageModel.SafetySignal)apiobject;
                #endregion
                #region [Trace Log]
                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("WebAPI Message<{0}> Recv OK, Content<{1}>", System.Reflection.MethodBase.GetCurrentMethod().Name, ApiRequest.Content));

                #endregion
                string errMsg = string.Empty;
                var em = EAP.Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(safetySignal.eqp_id);
                if (em == null)
                {
                    errMsg = string.Format("设备<{0}>检查异常：设备不存在，派送失败.", safetySignal.eqp_id);
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

                    #region [反馈结果给AGV]
                    PortModel pm = em.GetPortModelByPortID(safetySignal.port_id.Trim());
                    if (pm!=null)
                    {
                        if (EAPEnvironment.commonLibrary.lineModel.isMainEqpID)
                        {
                            ri.Code = "0000";
                            ri.Msg = "允许派送.";
                            ri.Succ = true;
                            ri.DataTime = DateTime.Now;
                            ri.Content = "OK";
                        }
                        else
                        {
                            // if ((pm.PortStatus == ePortStatus.LOADREQUEST && em.EQStatus == eEQSts.Run) ||
                            //(pm.PortStatus == ePortStatus.LOADREQUEST && em.EQStatus == eEQSts.Idle) ||
                            //(pm.PortStatus == ePortStatus.LOADREQUEST && em.EQStatus == eEQSts.Ready))
                            if (pm.PortStatus == ePortStatus.LOADREQUEST || pm.PortStatus == ePortStatus.UNLOADREQUEST || pm.PortStatus==ePortStatus.UNLOADCOMPLETE) //&&( em.EQStatus == eEQSts.Run ||em.EQStatus == eEQSts.Idle || em.EQStatus == eEQSts.Ready||em.EQStatus==eEQSts.Down) )||
                                                                                                                        // && (em.EQStatus == eEQSts.Run || em.EQStatus == eEQSts.Idle || em.EQStatus == eEQSts.Ready || em.EQStatus == eEQSts.Down)
                            {
                                ri.Code = "0000";
                                ri.Msg = "允许派送.";
                                ri.Succ = true;
                                ri.DataTime = DateTime.Now;
                                ri.Content = "OK";
                            }
                            else
                            {
                                ri.Code = "0001";
                                ri.Msg = $"目前设备端口状态[{pm.PortStatus}],设备状态[{em.EQStatus}]不允许派送.";
                                ri.Succ = false;
                                ri.DataTime = DateTime.Now;
                                ri.Content = "NG";
                            }
                        }
                    }
                    else
                    {
                        BaseComm.LogMsg(Log.LogLevel.Error, string.Format("找不到Port ID<{0}>", safetySignal.port_id.Trim()));
                    }
                    #endregion
                }
                History.EAP_EQP_EVENTHISTORY(em, eEventName.AGV_SAFETYSIGNALREQUEST, eEventFlow.AGV2EAP, ApiRequest.Content, em.CurrentLotID, safetySignal.port_id);
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
    public class RCSResult
    {
        public RCSResult()
        {

        }
        public bool Succ { get; set; }
        public string Code { get; set; }
        public string Msg { get; set; }
        public DateTime DataTime { get; set; }
        public object Content { get; set; }
    }
}
