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
    public class MES_EqpHoldReportController : ApiController
    {
        /// <summary>
        /// MES向EAP送出Hold命令
        /// </summary>
        /// <param name="ApiRequest"></param>
        /// <returns></returns>
        [HttpPost]
        public MessageModelBase.ApiResult MES_EqpHoldReport(MessageModelBase.ApiRequest ApiRequest)
        {
            
            MessageModelBase.ApiResult ri = new MessageModelBase.ApiResult();
            try
            {
                ErrorCodeModelBase errm = new ErrorCodeModelBase();
                #region [解析MES发送的消息]
                object apiobject;
                new Serialize().DeSerializeJSON(ApiRequest.Content, new MessageModel.EqpHoldInfo().GetType(), out apiobject);
                MessageModel.EqpHoldInfo eapHold = (MessageModel.EqpHoldInfo)apiobject;
                #endregion
                #region [Trace Log]
                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("WebAPI Message<{0}> Recv OK, Content<{1}>", System.Reflection.MethodBase.GetCurrentMethod().Name, ApiRequest.Content));
                #endregion
                string errMsg = string.Empty;
                var em = EAP.Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(eapHold.SubEqpID);
                if (em == null)
                {
                    errMsg = string.Format("MES通知<{0}>Hold检查异常：设备不存在，派送失败.", eapHold.SubEqpID);
                    BaseComm.ErrorHandleRule("E2301", errMsg, ref errm);
                    ri.bSucc = false;
                    ri.strCode = "2301";
                    ri.strMsg = errMsg;
                    ri.DataTime = DateTime.Now;
                    return ri;
                }
                else
                {
                    #region [给设备下发Hold命令]
                    switch (eapHold.IsEqpHold.ToUpper())//TRUE：Hold；FALSE：Release
                    {
                        case "TRUE":
                            new HostService.HostService().RemoteControlCommand(em.EQID, "", eRemoteCommand.Stop.GetEnumDescription());
                            BaseComm.LogMsg(Log.LogLevel.Info, string.Format("MES通知<{0}>Hold成功.", eapHold.SubEqpID));
                            break;
                        case "FALSE":
                            new HostService.HostService().RemoteControlCommand(em.EQID, "", eRemoteCommand.Start.GetEnumDescription());
                            BaseComm.LogMsg(Log.LogLevel.Info, string.Format("MES通知<{0}>解Hold成功.", eapHold.SubEqpID));
                            break;
                        default:
                            break;
                    }
                    #endregion
                    ri.strCode = "0000";
                    ri.strMsg = "成功.";
                    ri.bSucc = true;
                    ri.DataTime = DateTime.Now;
                }
                return ri;

            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                ri.bSucc = false;
                ri.strCode = "E0001";
                ri.strMsg = "程式出错.";
                ri.DataTime = DateTime.Now;
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
