using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Environment;
using iJedha.Automation.EAP.Model;
using iJedha.Automation.EAP.ModelBase;
using System;
using System.Web.Http;

namespace iJedha.Automation.EAP.WebAPI
{
    public class GUI_EqInfoRequestController : ApiController
    {
        /// <summary>
        /// GUI手动Track In
        /// </summary>
        /// <param name="ApiRequest"></param>
        /// <returns></returns>
        [HttpPost]
        public MessageModelBase.ApiResult GUI_EqInfoRequest(MessageModelBase.ApiRequest ApiRequest)
        {
            MessageModelBase.ApiResult ri = new MessageModelBase.ApiResult();
            try
            {
                ErrorCodeModelBase errm = new ErrorCodeModelBase();
                #region [解析MES发送的消息]
                object apiobject;
                new Serialize().DeSerializeJSON(ApiRequest.Content, new MessageModel.GUI_EqInfoRequest().GetType(), out apiobject);
                MessageModel.GUI_EqInfoRequest GuiLotInfo = (MessageModel.GUI_EqInfoRequest)apiobject;
                #endregion
                #region [Trace Log]
                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("WebAPI Message<{0}> Recv OK, Content<{1}>", System.Reflection.MethodBase.GetCurrentMethod().Name, ApiRequest.Content));
                #endregion

                string errMsg = string.Empty;
                
               
                if (!EAP.Environment.EAPEnvironment.commonLibrary.LineName.Equals(GuiLotInfo.LineName))
                {
                    errMsg = string.Format("GUI请求信息检查异常：线体<{0}>不存在，请求失败.", GuiLotInfo.LineName );
                    BaseComm.ErrorHandleRule("E2302", errMsg, ref errm);
                    ri.bSucc = false;
                    ri.strCode = "2302";
                    ri.strMsg = errMsg;
                    ri.DataTime = DateTime.Now;
                    return ri;
                }
                else
                {
                    //定时推送Lot信息，供GUI显示
                    EAPEnvironment.MQPublisherAp.MQ_LotInfoStatus(Environment.EAPEnvironment.commonLibrary.MQ_LotInfoStatus());
                    //定时推送设备状态，供GUI显示
                    EAPEnvironment.MQPublisherAp.MQ_EquipmentStatus(Environment.EAPEnvironment.commonLibrary.MQ_EquipmentStatus());

                    BaseComm.LogMsg(Log.LogLevel.Info, string.Format("GUI请求最新设备数据."));
                    ri.strCode = "0000";
                    ri.strMsg = "成功.";
                    ri.bSucc = true;
                    ri.DataTime = DateTime.Now;
                    ri.Content = "";
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
