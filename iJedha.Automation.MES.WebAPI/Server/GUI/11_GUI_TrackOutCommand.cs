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
    public class GUI_TrackOutCommandController : ApiController
    {
        /// <summary>
        /// GUI通知EAP下机
        /// </summary>
        /// <param name="ApiRequest"></param>
        /// <returns></returns>
        [HttpPost]
        public MessageModelBase.ApiResult GUI_TrackOutCommand(MessageModelBase.ApiRequest ApiRequest)
        {
            MessageModelBase.ApiResult ri = new MessageModelBase.ApiResult();
            try
            {
                ErrorCodeModelBase errm = new ErrorCodeModelBase();
                #region [解析MES发送的消息]
                object apiobject;
                new Serialize().DeSerializeJSON(ApiRequest.Content, new MessageModel.GUI_TrackOutCommand().GetType(), out apiobject);
                MessageModel.GUI_TrackOutCommand GuiTrackOut = (MessageModel.GUI_TrackOutCommand)apiobject;
                #endregion
                #region [Trace Log]
                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("WebAPI Message<{0}> Recv OK, Content<{1}>", System.Reflection.MethodBase.GetCurrentMethod().Name, ApiRequest.Content));
                #endregion

                string errMsg = string.Empty;
                EquipmentModel em = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByName(GuiTrackOut.EqpName);

                LotModel lm = EAPEnvironment.commonLibrary.commonModel.GetLotModelByLotID(GuiTrackOut.LotID);
                if (lm==null)
                {
                    errMsg = $"GUI通知EAP下机检查异常：任务名称<{GuiTrackOut.LotID}>不存在，下机失败.";
                    BaseComm.LogMsg(Log.LogLevel.Error, errMsg);
                    BaseComm.ErrorHandleRule("E4002", errMsg, ref errm);
                    ri.bSucc = false;
                    ri.strCode = "4002";
                    ri.strMsg = errMsg;
                    ri.DataTime = DateTime.Now;
                    return ri; ;
                }

                if (em == null)
                {
                    //error log
                    errMsg = $"GUI通知EAP下机检查异常：设备名称<{GuiTrackOut.EqpName}>不存在，下机失败.";
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
                    string err = "";
                    if (new WebAPIReport().EAP_LotTrackOut(new MessageModel.LotTrackOut()
                    {
                        MainEqpID = EAPEnvironment.commonLibrary.MDLN,
                        SubEqpID = em.EQID,
                        PortID = "",
                        LotID = GuiTrackOut.LotID,
                        PanelTotalQty = "",
                        NGFlag = false,
                        PanelList = new List<MessageModel.Panel>(),
                        WIPDataList = new List<MessageModel.WipData>(),//?
                        JobID = GuiTrackOut.LotID,
                        JobTotalQty = "",
                        PN = lm.PN,
                        WorkOrder = lm.WorkOrder
                    }, lm, em, 1,out err))
                    {
                        ri.strCode = "0000";
                        ri.strMsg = "成功.";
                        ri.bSucc = true;
                        ri.DataTime = DateTime.Now;
                        BaseComm.LogMsg(Log.LogLevel.Info, string.Format("GUI通知EAP下机成功."));
                        //删除当前Lot之前所有Lot信息
                        Environment.EAPEnvironment.commonLibrary.commonModel.RemoveFrontProcessLotModel(lm);
                    }
                    else
                    {
                        ri.strCode = "0001";
                        ri.strMsg = err;
                        ri.bSucc = false;
                        ri.DataTime = DateTime.Now;
                    }
                    return ri;
                }
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
