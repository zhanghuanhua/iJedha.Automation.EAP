using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Environment;
using iJedha.Automation.EAP.Model;
using iJedha.Automation.EAP.ModelBase;
using System;
using System.Web.Http;

namespace iJedha.Automation.EAP.WebAPI
{
    public class GUI_TrackInCommandController : ApiController
    {
        /// <summary>
        /// GUI手动Track In
        /// </summary>
        /// <param name="ApiRequest"></param>
        /// <returns></returns>
        [HttpPost]
        public MessageModelBase.ApiResult GUI_TrackInCommand(MessageModelBase.ApiRequest ApiRequest)
        {
            MessageModelBase.ApiResult ri = new MessageModelBase.ApiResult();
            try
            {
                ErrorCodeModelBase errm = new ErrorCodeModelBase();
                #region [解析MES发送的消息]
                object apiobject;
                new Serialize().DeSerializeJSON(ApiRequest.Content, new MessageModel.GUI_TrackInCommand().GetType(), out apiobject);
                MessageModel.GUI_TrackInCommand GuiTrackIn = (MessageModel.GUI_TrackInCommand)apiobject;
                #endregion
                #region [Trace Log]
                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("WebAPI Message<{0}> Recv OK, Content<{1}>", System.Reflection.MethodBase.GetCurrentMethod().Name, ApiRequest.Content));
                #endregion

                string errMsg = string.Empty;
                var em = EAP.Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByName(GuiTrackIn.EqpName);
               
                if (em == null)
                {
                    errMsg = string.Format("GUI手动上机检查异常：设备<{0}>不存在，切换失败.", GuiTrackIn.EqpName );
                    BaseComm.ErrorHandleRule("E2301", errMsg, ref errm);
                    ri.bSucc = false;
                    ri.strCode = "2301";
                    ri.strMsg = errMsg;
                    ri.DataTime = DateTime.Now;
                    return ri;
                }
                else
                {
                    string err="";
                    if (EAPEnvironment.commonLibrary.lineModel.isSubLot)
                    {
                        Lot subLot = EAPEnvironment.commonLibrary.commonModel.GetLotModelBySubLotID(GuiTrackIn.JobID.Trim());
                        new WebAPIReport().EAP_LotTrackIn(new MessageModel.LotTrackIn()
                        {
                            MainEqpID = Environment.EAPEnvironment.commonLibrary.MDLN,
                            SubEqpID = em.EQID,
                            PortID = GuiTrackIn.PortID,
                            LotID = GuiTrackIn.JobID
                        }, subLot, 1,out err);
                    }
                    else
                    {
                        if (EAPEnvironment.commonLibrary.lineModel.isNeedPost)
                        {
                            LotModel lot = EAPEnvironment.commonLibrary.commonModel.GetLotModelByLotID(GuiTrackIn.JobID.Trim());
                            new WebAPIReport().EAP_LotTrackIn(new MessageModel.LotTrackIn()
                            {
                                MainEqpID = Environment.EAPEnvironment.commonLibrary.MDLN,
                                SubEqpID = em.EQID,
                                PortID = GuiTrackIn.PortID,
                                LotID = GuiTrackIn.JobID
                            }, lot, 1,out err);
                        }
                        else
                        {
                            EAPEnvironment.commonLibrary.ShowLotInfoMessage = eLotinfo.PartUp.ToString();
                        }
                    }
                    if (string.IsNullOrEmpty(err))
                    {
                        BaseComm.LogMsg(Log.LogLevel.Info, string.Format("GUI手动上机检查成功."));
                        ri.strCode = "0000";
                        ri.strMsg = "成功.";
                        ri.bSucc = true;
                        ri.DataTime = DateTime.Now;
                    }
                    else
                    {
                        BaseComm.LogMsg(Log.LogLevel.Info, err);
                        ri.strCode = "0001";
                        ri.strMsg = err;
                        ri.bSucc = false;
                        ri.DataTime = DateTime.Now;
                    }
                   
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
