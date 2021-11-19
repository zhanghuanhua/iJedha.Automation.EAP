//******************************************************************
//   系统名称 : iJedha.Automation.EAP.SocketService
//   文件概要 : 
//   作    者 : 张明哲
//   <更新履历>
//   1.0.0.0    2020/10/1 14:28:28
//******************************************************************
using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Environment;
using iJedha.Automation.EAP.LibraryBase;
using iJedha.Automation.EAP.Model;
using iJedha.Automation.EAP.WebAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace iJedha.Automation.EAP.EQPService
{
    public partial class TraceDataRequestReply : BaseComm
    {
        public void EventHandle(string evtXml)
        {
            try
            {
                #region Decode Message
                SocketMessageStructure.TraceDataRequestReply rpy = new SocketMessageStructure.TraceDataRequestReply();
                if (new Serialize<SocketMessageStructure.TraceDataRequestReply>().DeSerializeXML(evtXml, out rpy) == false)
                {
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Socket Message<{0}> Decode Error, Content<{1}>", "TraceDataRequestReply", evtXml));
                    return;
                }
                #endregion

                #region 记录设备回复RETURNCODE NG的Log
                //if (!rpy.RETURN.RETURNCODE.Equals("0"))
                //{
                //    BaseComm.LogMsg(Log.LogLevel.Warn, string.Format("设备<{0}>回复<{1}> 结果NG,回复内容<{2}> ", rpy.BODY.eqp_id.Trim(),
                //       "TraceDataRequestReply", rpy.RETURN.RETURNMESSAGE));
                //    return;
                //}
                #endregion

                #region Record Log
                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("<{2}>Socket Message<{0}> Recv OK , Content<{1}>",
                "TraceDataRequestReply", rpy.WriteToXml(), rpy.BODY.eqp_id.Trim()));
                #endregion 
                #region 获取EquipmentModel信息
                var em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(rpy.BODY.eqp_id.Trim());
                if (em==null)
                {
                    //error log
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<TraceDataRequestReply> Find Error", rpy.BODY.eqp_id.Trim()));
                    return;
                }
                #endregion
                Socket_DynamicLibraryBase dl;
                dl = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetDynamicLibrary(em.EQID);
                
                #region 拆解上报的Data_list存到字典内 
                Dictionary<string, string> dicPara = new Dictionary<string, string>();
                List<MessageModel.WIPData> LWipData=new List<MessageModel.WIPData>();
                foreach (var item in rpy.BODY.trace_data_list)
                {
                    ModelBase.Socket_TraceDataModelBase socket_TraceDataModelBase;
                    #region 给Dic_TraceDataModel赋值
                    if (em.isUseDataName)
                    {
                        socket_TraceDataModelBase= dl.Dic_TraceDataModel.Values.Where(r => r.Name == item.data_item).FirstOrDefault();
                    }
                    else
                    {
                        socket_TraceDataModelBase = dl.Dic_TraceDataModel.Values.Where(r => r.ID == item.data_item).FirstOrDefault();
                    }
                   
                    if (socket_TraceDataModelBase == null)
                    {
                        continue;
                        //BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<TraceDataRequestReply> 给Dic_TraceDataModel赋值错误", rpy.BODY.eqp_id.Trim()));
                        //return;
                    }
                    //赋值显示
                    socket_TraceDataModelBase.Value = item.data_value;
                    #endregion

                    #region 给List_KeyTraceDataSpec赋值
                    if (em.List_KeyTraceDataSpec.Count!=0)
                    {
                        var _traceData = em.List_KeyTraceDataSpec.Where(r => r.WIPDataName == socket_TraceDataModelBase.Name).FirstOrDefault();
                        if (_traceData == null)
                        {
                            continue;
                        }
                        _traceData.DefaultValue = item.data_value;
                    }
                    #endregion

                    MessageModel.WIPData WipData = new MessageModel.WIPData();
                    dicPara.Add(item.data_item, item.data_value);
                    WipData.WipDataName = item.data_item;
                    WipData.WipDataValue = item.data_value;
                    LWipData.Add(WipData);
                }
                #endregion

                #region 把检测设备收集到的点检数据上报给MES  
                //if (em.Type == eEquipmentType.I)
                //{
                //    new WebAPIReport().EAP_MES_EquipmentDataCollection(new MessageModel.DataCollection()
                //    {
                //        SubEqpID = rpy.BODY.eqp_id.Trim(),
                //        WipDataSetUp = "点检数据收集",
                //        WIPDataList = LWipData,
                //        ServiceName = "AdHocWIPData",
                //        LotID = "",
                //        PnlID = ""
                //    }, 1);
                //}
                #endregion

                #region 存DB
                History.EAP_EQP_TRACE(em, dl);
                #endregion
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", "TraceDataRequestReply", ex.Message.ToString(), ex.StackTrace.ToString()));
            }
            finally
            {

            }
        }
    }
}
