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
    public class GUI_JobDataDownloadCommandController : ApiController
    {
        /// <summary>
        /// GUI远程下载生产任务给设备 
        /// </summary>
        /// <param name="ApiRequest"></param>
        /// <returns></returns>
        [HttpPost]
        public MessageModelBase.ApiResult GUI_JobDataDownloadCommand(MessageModelBase.ApiRequest ApiRequest)
        {
            MessageModelBase.ApiResult ri = new MessageModelBase.ApiResult();
            try
            {
                ErrorCodeModelBase errm = new ErrorCodeModelBase();
                #region [解析MES发送的消息]
                object apiobject;
                new Serialize().DeSerializeJSON(ApiRequest.Content, new MessageModel.GUI_JobDataDownloadCommand().GetType(), out apiobject);
                MessageModel.GUI_JobDataDownloadCommand GuiJobDataDownload = (MessageModel.GUI_JobDataDownloadCommand)apiobject;
                #endregion
                #region [Trace Log]
                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("WebAPI Message<{0}> Recv OK, Content<{1}>", System.Reflection.MethodBase.GetCurrentMethod().Name, ApiRequest.Content));
                #endregion

                string errMsg = string.Empty;
                var em = EAP.Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByName(GuiJobDataDownload.EqpName);
                if (em == null)
                {
                    errMsg = string.Format("GUI通知<{0}>生产任务修改检查异常：设备不存在，切换失败.", GuiJobDataDownload.EqpName);
                    BaseComm.ErrorHandleRule("E2301", errMsg, ref errm);
                    ri.bSucc = false;
                    ri.strCode = "2301";
                    ri.strMsg = errMsg;
                    ri.DataTime = DateTime.Now;
                    return ri;
                }
                else
                {
                    #region [给设备下发生产任务命令]
                    Environment.EAPEnvironment.commonLibrary.isGUITriggerDataDownload = true;
                    if (EAPEnvironment.commonLibrary.lineModel.isSubLot)
                    {
                        var subLot = Environment.EAPEnvironment.commonLibrary.commonModel.GetLotModelBySubLotID(GuiJobDataDownload.JobID);
                        if (subLot == null)
                        {
                            BaseComm.LogMsg(Log.LogLevel.Error, string.Format("GUI下载生产任务失败，不能找到Lot资讯，LotID<{0}>.", GuiJobDataDownload.JobID));
                            BaseComm.ErrorHandleRule("E1001", errMsg, ref errm);
                            ri.strCode = "1001";
                            ri.strMsg = string.Format("GUI下载生产任务失败，不能找到Lot资讯，LotID<{0}>.", GuiJobDataDownload.JobID);
                            ri.bSucc = false;
                            ri.DataTime = DateTime.Now;
                        }

                        BaseComm.LogMsg(Log.LogLevel.Info, string.Format("GUI通知<{0}>生产任务成功.", GuiJobDataDownload.EqpName));
                        if (!EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckStart)
                        {
                            foreach (var _v in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.JobDataDownloadChangeResult != eCheckResult.ok))
                            {

                                List<MessageModel.Param> parameterModel = Environment.EAPEnvironment.commonLibrary.commonModel.GetParameterModel_KL(_v, subLot);

                                new HostService.HostService().JobDataDownload(_v, subLot, GuiJobDataDownload.PortID, parameterModel);
                            }
                            if (EAPEnvironment.EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_JobDataDownloadKL, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_JobDataDownloadKL, 4000, true, subLot))
                            {
                                EAPEnvironment.commonLibrary.isProcessOK = true;
                                BaseComm.LogMsg(Log.LogLevel.Info, string.Format("Lot<{0}>确认设备数据中......", GuiJobDataDownload.JobID));
                                EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckStart = true;
                                EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckTime = DateTime.Now;
                            }
                        }
                    }
                    else
                    {
                        var lm = Environment.EAPEnvironment.commonLibrary.commonModel.GetLotModelByLotID(GuiJobDataDownload.JobID);
                        if (lm == null)
                        {
                            BaseComm.LogMsg(Log.LogLevel.Error, string.Format("GUI下载生产任务失败，不能找到Lot资讯，LotID<{0}>.", GuiJobDataDownload.JobID));
                            BaseComm.ErrorHandleRule("E1001", errMsg, ref errm);
                            ri.strCode = "1001";
                            ri.strMsg = string.Format("GUI下载生产任务失败，不能找到Lot资讯，LotID<{0}>.", GuiJobDataDownload.JobID);
                            ri.bSucc = false;
                            ri.DataTime = DateTime.Now;
                        }

                        BaseComm.LogMsg(Log.LogLevel.Info, string.Format("GUI通知<{0}>生产任务成功.", GuiJobDataDownload.EqpName));
                        if (!EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckStart)
                        {
                            if (lm.IsConnectionStatus==eConnectionStatus.Disconnection.GetEnumDescription())
                            {
                                foreach (var _v in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => (r.JobDataDownloadChangeResult != eCheckResult.ok) &&r.isDisconnectionDataDownload))
                                {
                                    _v.RetryCount = 0;
                                    #region[如果是isMaterialLoadComplete设定请求物料Lot信息，不下载生产任务给设备]
                                    if (_v.isMaterialLoadComplete)
                                    {
                                        continue;
                                    }
                                    #endregion
                                    #region [如果设备是扫描底盘下载生产任务]
                                    if (_v.isCarrierDownloadData)
                                    {
                                        continue;
                                    }
                                    #endregion
                                    List<MessageModel.Param> parameterModel = Environment.EAPEnvironment.commonLibrary.commonModel.GetParameterModel(_v, lm);

                                    new HostService.HostService().JobDataDownload(_v, lm, GuiJobDataDownload.PortID, parameterModel);
                                }
                            }
                            foreach (var _v in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.JobDataDownloadChangeResult != eCheckResult.ok))
                            {
                                _v.RetryCount = 0;
                                #region[如果是isMaterialLoadComplete设定请求物料Lot信息，不下载生产任务给设备]
                                if (_v.isMaterialLoadComplete)
                                {
                                    continue;
                                }
                                #endregion
                                #region [如果设备是扫描底盘下载生产任务]
                                if (_v.isCarrierDownloadData)
                                {
                                    continue;
                                }
                                #endregion
                                List<MessageModel.Param> parameterModel =Environment.EAPEnvironment.commonLibrary.commonModel.GetParameterModel(_v, lm);

                                new HostService.HostService().JobDataDownload(_v, lm, GuiJobDataDownload.PortID, parameterModel);
                            }
                            Dictionary<EquipmentModel, LotModel> el = new Dictionary<EquipmentModel, LotModel>();
                            el.Add(em,lm);
                            if (EAPEnvironment.EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_JobDataDownload, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_JobDataDownload, 4000, true, el))
                            {
                                EAPEnvironment.commonLibrary.isProcessOK = true;
                                BaseComm.LogMsg(Log.LogLevel.Info, string.Format("Lot<{0}>确认设备数据中......", GuiJobDataDownload.JobID));
                                EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckStart = true;
                                EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckTime = DateTime.Now;
                            }
                        }
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
