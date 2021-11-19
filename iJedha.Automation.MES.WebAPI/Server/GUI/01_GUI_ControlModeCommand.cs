using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Environment;
using iJedha.Automation.EAP.Model;
using iJedha.Automation.EAP.ModelBase;
using System;
using System.Web.Http;

namespace iJedha.Automation.EAP.WebAPI
{
    public class GUI_ControlModeCommandController : ApiController
    {
        /// <summary>
        /// GUI向EAP送出切换模式的命令
        /// </summary>
        /// <param name="ApiRequest"></param>
        /// <returns></returns>
        [HttpPost]
        public MessageModelBase.ApiResult GUI_ControlModeCommand(MessageModelBase.ApiRequest ApiRequest)
        {
            MessageModelBase.ApiResult ri = new MessageModelBase.ApiResult();
            try
            {
                ErrorCodeModelBase errm = new ErrorCodeModelBase();
                #region [解析MES发送的消息]
                object apiobject;
                new Serialize().DeSerializeJSON(ApiRequest.Content, new MessageModel.GUI_ControlModeCommand().GetType(), out apiobject);
                MessageModel.GUI_ControlModeCommand GuiControlMode = (MessageModel.GUI_ControlModeCommand)apiobject;
                #endregion
                #region [Trace Log]
                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("WebAPI Message<{0}> Recv OK, Content<{1}>", System.Reflection.MethodBase.GetCurrentMethod().Name, ApiRequest.Content));
                #endregion

                string errMsg = string.Empty;
                var em = EAP.Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByName(GuiControlMode.EqpName);
                if (em == null)
                {
                    errMsg = string.Format("GUI通知<{0}>控制模式切换检查异常：设备不存在，切换失败.", GuiControlMode.EqpName);
                    BaseComm.ErrorHandleRule("E2301", errMsg, ref errm);
                    ri.bSucc = false;
                    ri.strCode = "2301";
                    ri.strMsg = errMsg;
                    ri.DataTime = DateTime.Now;
                    return ri;
                }
                else
                {
                    #region [给设备下发控制模式切换命令]
                    new HostService.HostService().ControlModeCommand(GuiControlMode.EqpName, GuiControlMode.ControlMode);
                    #endregion

                    BaseComm.LogMsg(Log.LogLevel.Info, string.Format("GUI通知<{0}>控制模式切换.", GuiControlMode.EqpName));
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
