using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Environment;
using iJedha.Automation.EAP.Library;
using iJedha.Automation.EAP.Model;
using iJedha.Automation.EAP.ModelBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace iJedha.Automation.EAP.WebAPI
{
    public class GUI_LotInfoRequestController : ApiController
    {
        /// <summary>
        /// GUI通知EAP下机
        /// </summary>
        /// <param name="ApiRequest"></param>
        /// <returns></returns>
        [HttpPost]
        public MessageModelBase.ApiResult GUI_LotInfoRequest(MessageModelBase.ApiRequest ApiRequest)
        {
            MessageModelBase.ApiResult ri = new MessageModelBase.ApiResult();
            try
            {
                ErrorCodeModelBase errm = new ErrorCodeModelBase();
                #region [解析MES发送的消息]
                object apiobject;
                new Serialize().DeSerializeJSON(ApiRequest.Content, new MessageModel.GUI_LotInfoRequest().GetType(), out apiobject);
                MessageModel.GUI_LotInfoRequest GuiLotInfoRequest = (MessageModel.GUI_LotInfoRequest)apiobject;
                #endregion
                #region [Trace Log]
                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("WebAPI Message<{0}> Recv OK, Content<{1}>", System.Reflection.MethodBase.GetCurrentMethod().Name, ApiRequest.Content));
                #endregion

                string errMsg = string.Empty;
                EquipmentModel em = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByName(GuiLotInfoRequest.EqpName);

                if (em == null)
                {
                    //error log
                    errMsg = $"GUI通知EAP请求任务检查异常：设备名称<{GuiLotInfoRequest.EqpName}>不存在，请求任务失败.";
                    BaseComm.LogMsg(Log.LogLevel.Error, errMsg);
                    BaseComm.ErrorHandleRule("E4001", errMsg, ref errm);
                    ri.bSucc = false;
                    ri.strCode = "4001";
                    ri.strMsg = errMsg;
                    ri.DataTime = DateTime.Now;
                    return ri; ;
                }
                else
                {
                    new WebAPIReport().EAP_LotInfoRequest(new MessageModel.LotInfoRequest
                    {
                        MainEqpID = EAPEnvironment.commonLibrary.MDLN,
                        SubEqpID = em.EQID,
                        PortID = GuiLotInfoRequest.PortID,
                        CarrierID = ""
                    }, em, 1, out errMsg);
                    if (!string.IsNullOrEmpty(errMsg))
                    {
                        ri.strCode = "0001";
                        ri.strMsg = errMsg;
                        ri.bSucc = true;
                        ri.DataTime = DateTime.Now;
                        BaseComm.LogMsg(Log.LogLevel.Info, errMsg);
                        return ri;
                    }
                    else
                    {
                        LotModel lot = EAPEnvironment.commonLibrary.commonModel.GetLotModelByLotID(EAPEnvironment.commonLibrary.TupTrackIn.Item2);

                        new WebAPIReport().EAP_LotTrackIn(new MessageModel.LotTrackIn()
                        {
                            MainEqpID = Environment.EAPEnvironment.commonLibrary.MDLN,
                            SubEqpID = em.EQID,
                            PortID = EAPEnvironment.commonLibrary.TupTrackIn.Item1,
                            LotID = EAPEnvironment.commonLibrary.TupTrackIn.Item2
                        }, lot, 1, out errMsg);

                        if (!string.IsNullOrEmpty(errMsg))
                        {
                            ri.strCode = "0001";
                            ri.strMsg = errMsg;
                            ri.bSucc = true;
                            ri.DataTime = DateTime.Now;
                            BaseComm.LogMsg(Log.LogLevel.Info, errMsg);
                        }
                        else
                        {
                            ri.strCode = "0000";
                            ri.strMsg = "成功.";
                            ri.bSucc = true;
                            ri.DataTime = DateTime.Now;
                            BaseComm.LogMsg(Log.LogLevel.Info, string.Format("GUI通知EAP下机成功."));
                        }

                        return ri;
                    }

                }
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                ri.bSucc = false;
                ri.strCode = "E0001";
                ri.strMsg = $"程式出错.{ex.Message}";
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
