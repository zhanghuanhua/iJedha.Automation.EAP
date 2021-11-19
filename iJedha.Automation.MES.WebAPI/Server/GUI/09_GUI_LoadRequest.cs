﻿using iJedha.Automation.EAP.Core;
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
    public class GUI_LoadRequestController : ApiController
    {
        /// <summary>
        /// GUI通知EAP叫料 
        /// </summary>
        /// <param name="ApiRequest"></param>
        /// <returns></returns>
        [HttpPost]
        public MessageModelBase.ApiResult GUI_LoadRequest(MessageModelBase.ApiRequest ApiRequest)
        {
            MessageModelBase.ApiResult ri = new MessageModelBase.ApiResult();
            try
            {
                ErrorCodeModelBase errm = new ErrorCodeModelBase();
                #region [解析MES发送的消息]
                object apiobject;
                new Serialize().DeSerializeJSON(ApiRequest.Content, new MessageModel.GUI_LoadRequest().GetType(), out apiobject);
                MessageModel.GUI_LoadRequest GuiLoadRequest = (MessageModel.GUI_LoadRequest)apiobject;
                #endregion
                #region [Trace Log]
                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("WebAPI Message<{0}> Recv OK, Content<{1}>", System.Reflection.MethodBase.GetCurrentMethod().Name, ApiRequest.Content));
                #endregion

                string errMsg = string.Empty;
                EquipmentModel em = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByName(GuiLoadRequest.EqpName);

                if (em == null)
                {
                    //error log
                    errMsg = $"GUI通知EAP叫料检查异常：设备名称<{GuiLoadRequest.EqpName}>不存在，叫料失败.";
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

                    //广合设备的投板机暂存口或收板机的空托盘上料口需要卡控连续叫料问题
                    if (em.EqVendor.Equals("广合设备") && (GuiLoadRequest.PortID == ePortID.L01.ToString() || GuiLoadRequest.PortID == ePortID.U04.ToString()))
                    {
                        //第一次上报时，添加Flag，并且直接上报MES呼叫AGV
                        if (!EAPEnvironment.commonLibrary.Dic_IsCallAgv.ContainsKey(GuiLoadRequest.PortID))
                        {

                            new WebAPIReport().EAP_LoadRequest(new MessageModel.LoadRequest()
                            {
                                MainEqpID = EAPEnvironment.commonLibrary.MDLN,
                                SubEqpID = em.EQID,
                                PortID = GuiLoadRequest.PortID,
                                RequestType = eRequestType.Empty.ToString(),
                                LoadedQty = EAPEnvironment.commonLibrary.LastLotCount
                            }, em, 1, out err);
                            if (string.IsNullOrEmpty(err))
                            {
                                EAPEnvironment.commonLibrary.Dic_IsCallAgv.TryAdd(GuiLoadRequest.PortID, true);
                                PortModel pm = em.GetPortModelByPortID(GuiLoadRequest.PortID);
                                if (pm!=null)
                                {
                                    pm.PortStatus = ePortStatus.LOADREQUEST;
                                }
                            }
                        }
                        else
                        //如果不是第一次上报，需卡住
                        {
                            if (EAPEnvironment.commonLibrary.Dic_IsCallAgv[GuiLoadRequest.PortID])
                            {
                                err = "目前已存在呼叫AGV任务.";
                            }
                            else
                            {
                                bool outValue;

                                new WebAPIReport().EAP_LoadRequest(new MessageModel.LoadRequest()
                                {
                                    MainEqpID = EAPEnvironment.commonLibrary.MDLN,
                                    SubEqpID = em.EQID,
                                    PortID = GuiLoadRequest.PortID,
                                    RequestType = eRequestType.Empty.ToString(),
                                    LoadedQty = EAPEnvironment.commonLibrary.LastLotCount
                                }, em, 1, out err);
                                if (string.IsNullOrEmpty(err))
                                {
                                    EAPEnvironment.commonLibrary.Dic_IsCallAgv.TryRemove(GuiLoadRequest.PortID, out outValue);
                                    EAPEnvironment.commonLibrary.Dic_IsCallAgv.TryAdd(GuiLoadRequest.PortID, true);
                                }

                            }
                        }
                    }
                    else
                    {
                        new WebAPIReport().EAP_LoadRequest(new MessageModel.LoadRequest()
                        {
                            MainEqpID = EAPEnvironment.commonLibrary.MDLN,
                            SubEqpID = em.EQID,
                            PortID = GuiLoadRequest.PortID,
                            RequestType = eRequestType.Empty.ToString(),
                            LoadedQty = ""
                        }, em, 1, out err);
                    }

                    if (string.IsNullOrEmpty(err))
                    {
                        ri.strCode = "0000";
                        ri.strMsg = "成功.";
                        ri.bSucc = true;
                        ri.DataTime = DateTime.Now;
                        BaseComm.LogMsg(Log.LogLevel.Info, string.Format("GUI通知EAP叫料成功."));
                    }
                    else
                    {
                        ri.strCode = "0001";
                        ri.strMsg = err;
                        ri.bSucc = true;
                        ri.DataTime = DateTime.Now;
                        BaseComm.LogMsg(Log.LogLevel.Warn, err);
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
