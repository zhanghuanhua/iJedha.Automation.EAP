//******************************************************************
//   系统名称 : iJedha.Automation.EAP.WebAPIService
//   文件概要 : EAPEnvironment
//   作    者 : 张明哲
//   <更新履历>
//   1.0.0.0    2020/2/6 10:40:34
//******************************************************************
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
    public class MES_FirstInspResultController : ApiController
    {
        /// <summary>
        /// MES向EAP送出RGV配送命令
        /// </summary>
        /// <param name="ApiRequest"></param>
        /// <returns></returns>
        [HttpPost]
        public MessageModelBase.ApiResult MES_FirstInspResult(MessageModelBase.ApiRequest ApiRequest)
        {
            MessageModelBase.ApiResult ri = new MessageModelBase.ApiResult();
            try
            {
                ErrorCodeModelBase errm = new ErrorCodeModelBase();
                #region [解析MES发送的消息]
                object apiobject;
                new Serialize().DeSerializeJSON(ApiRequest.Content, new MessageModel.FirstInspResult().GetType(), out apiobject);
                MessageModel.FirstInspResult firstResult = (MessageModel.FirstInspResult)apiobject;
                #endregion
                #region [Trace Log]
                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("WebAPI Message<{0}> Recv OK, Content<{1}>", System.Reflection.MethodBase.GetCurrentMethod().Name, ApiRequest.Content));
                #endregion
                string errMsg = string.Empty;
                var lm = Environment.EAPEnvironment.commonLibrary.commonModel.GetLotModelByInspectLotID(firstResult.LotID);
                
                if (lm == null)
                {
                    errMsg = string.Format("MES通知首件结果检查异常：任务<{0}>不存在，检查失败.", firstResult.LotID);
                    BaseComm.ErrorHandleRule("E1002", errMsg, ref errm);
                    ri.bSucc = false;
                    ri.strCode = "1002";
                    ri.strMsg = errMsg;
                    return ri;
                }
                else
                {
                    #region [通知需首件设备首件结果]
                    #region [如果是手动过账单机设备，取得设备模块使用MainEqpID查找]
                    if (!EAPEnvironment.commonLibrary.lineModel.isNeedPost)
                    {
                        var vem = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.EQID == firstResult.MainEqpID).FirstOrDefault();
                        if (firstResult.InspResult.ToUpper() == "PASS")
                        {
                            new HostService.HostService().RemoteControlCommand(vem.EQID, "", eRemoteCommand.InspectOK.GetEnumDescription());
                            //new HostService.HostService().CIMMessageCommand(vem.EQID, "0", "The result of first article inspection  is OK!", DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                            EAPEnvironment.commonLibrary.isInspecResultOK = true;
                            #region 正在检查生产条件
                            //if (!EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckInspectCheckStart)
                            //{
                            //    Dictionary<EquipmentModel, LotModel> el = new Dictionary<EquipmentModel, LotModel>();
                            //    el.Add(vem, lm);
                            //    if (EAPEnvironment.EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_ProductionConditionCheckInspectCheck, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_ProductionConditionCheckInspectCheck, 4000, true, el))
                            //    {
                            //        BaseComm.LogMsg(Log.LogLevel.Info, string.Format("线体<{0}>正在检查生产条件......", EAPEnvironment.commonLibrary.LineName));
                            //        EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckInspectCheckStart = true;
                            //        EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckInspectCheckTime = DateTime.Now;
                            //    }
                            //}
                            #endregion

                        } 
                        if (firstResult.InspResult.ToUpper() == "FAIL")
                        {
                            new HostService.HostService().RemoteControlCommand(vem.EQID, "", eRemoteCommand.InspectNG.GetEnumDescription());
                            //new HostService.HostService().CIMMessageCommand(vem.EQID, "0", "The result of first article is NG!", DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                        }
                    }
                    #endregion
                    else
                    {
                        var aa = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.isInspectEquipment == true).ToList();
                        foreach (var vem in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.isInspectEquipment == true).ToList())
                        {
                            if (firstResult.InspResult.ToUpper() == "PASS")
                            {
                                new HostService.HostService().RemoteControlCommand(vem.EQID, "", eRemoteCommand.InspectOK.GetEnumDescription());
                                EAPEnvironment.commonLibrary.isInspecResultOK = true;
                                #region 正在检查生产条件
                                if (!EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckInspectCheckStart)
                                {
                                    Dictionary<EquipmentModel, LotModel> el = new Dictionary<EquipmentModel, LotModel>();
                                    el.Add(vem, lm);
                                    if (EAPEnvironment.EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_ProductionConditionCheckInspectCheck, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_ProductionConditionCheckInspectCheck, 4000, true, el))
                                    {
                                        BaseComm.LogMsg(Log.LogLevel.Info, string.Format("线体<{0}>正在检查生产条件......", EAPEnvironment.commonLibrary.LineName));
                                        EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckInspectCheckStart = true;
                                        EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckInspectCheckTime = DateTime.Now;
                                    }
                                }
                                #endregion

                            }
                            if (firstResult.InspResult.ToUpper() == "FAIL")
                            {
                                new HostService.HostService().RemoteControlCommand(vem.EQID, "", eRemoteCommand.InspectNG.GetEnumDescription());
                            }
                        }
                    }
                    
                    #endregion
                    BaseComm.LogMsg(Log.LogLevel.Info, string.Format("MES通知首件结果检查成功."));
                    ri.strCode = "0000";
                    ri.strMsg = "成功.";
                    ri.bSucc = true;
                }
                return ri;

            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                ri.bSucc = false;
                ri.strCode = "E0001";
                ri.strMsg = "程式出错.";
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
