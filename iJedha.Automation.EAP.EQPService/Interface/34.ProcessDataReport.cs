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
using System.Xml;

namespace iJedha.Automation.EAP.EQPService
{
    public partial class ProcessDataReport : BaseComm
    {
        public void EventHandle(string evtXml)
        {

            try
            {

                #region Decode Message
                SocketMessageStructure.ProcessDataReport msg = new SocketMessageStructure.ProcessDataReport();
                if (new Serialize<SocketMessageStructure.ProcessDataReport>().DeSerializeXML(evtXml, out msg) == false)
                {
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Socket Message<{0}> Decode Error, Content<{1}>", "ProcessDataReport", evtXml));
                    return;
                }
                #endregion

                //根据设备NO获取设备信息
                //EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByNo(1);
                //根据设备ID获取设备模型
                EquipmentModel em = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByEqID(msg.BODY.eqp_id);

                #region Record Log
                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("<{2}>Socket Message<{0}> Recv OK , Content<{1}>",
                "ProcessDataReport", msg.WriteToXml(), em.EQID.Trim()));

                if (em == null)
                {
                    //error log
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<ProcessDataReport> Find Error", em.EQID.Trim()));
                    return;
                }
                #endregion



                #region 连线检查
                if (em.isCheckConnect && em.isCheckControlMode)
                {
                    if (em.ConnectMode == Model.eConnectMode.DISCONNECT || em.ControlMode != eControlMode.REMOTE)
                    {
                        BaseComm.LogMsg(Log.LogLevel.Error, $"设备ID[{em.EQID}],连线状态为[{em.ConnectMode.ToString()}],控制模式为[{em.ControlMode.ToString()}]");
                        return;
                    }
                }
                #endregion



                #region 拆解上报的dic_parameter存到字典内
                Socket_DynamicLibraryBase dl;
                dl = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetDynamicLibrary(em.EQID);
                //Dictionary<string, string> dicPara = new Dictionary<string, string>();
                //List<MessageModel.WIPData> LWipData = new List<MessageModel.WIPData>();
                //List<Tuple<string, string, string>> lstParameter = new List<Tuple<string, string, string>>();
                Dictionary<string, string> dic_parameter = CreateNewDictoinary();
                //ModelBase.Socket_ProcessDataModelBase v;
                #region 如果配置档内容为空，则使用设备上报的内容插入数据库
                if (dl.Dic_ProcessDataModel.Count == 0)
                {
                    //int i = 1;
                    //foreach (var item in msg.BODY.proc_data_list)
                    //{
                    //    //var aa = Tuple.Create(i.ToString(),item.data_item,item.data_value);
                    //    Tuple<string, string, string> tParameter = new Tuple<string, string, string>(i.ToString(), item.data_item, item.data_value);
                    //    lstParameter.Add(tParameter);
                    //    i++;
                    //}
                    foreach (var item in msg.BODY.proc_data_list)
                    {
                        if (dic_parameter.ContainsKey(item.data_item))
                        {
                            dic_parameter[item.data_item] = item.data_value;
                        }
                        else
                        {
                            dic_parameter[item.data_item] = "";
                        }
                    }
                    History.EAP_EQP_PROCESSDATAName(em, dic_parameter);
                }
                #endregion
                //else
                //{
                //    foreach (var item in msg.BODY.proc_data_list)
                //    {
                //        if (em.isUseDataName)
                //        {
                //            //使用参数名称
                //            v = dl.Dic_ProcessDataModel.Values.Where(r => r.Name == item.data_item).FirstOrDefault();
                //        }
                //        else
                //        {
                //            //使用DataCode
                //            v = dl.Dic_ProcessDataModel.Values.Where(r => r.ID == item.data_item).FirstOrDefault();
                //        }
                //        if (v == null)
                //        {
                //            LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<ProcessDataReport> Find Dic_ProcessDataModel Error", em.EQID.Trim()));
                //            return;
                //        }
                //        MessageModel.WIPData wIPData = new MessageModel.WIPData();
                //        wIPData.WipDataName = v.Name;
                //        wIPData.WipDataValue = item.data_value;
                //        LWipData.Add(wIPData);

                //        #region Dic_ProcessDataModel
                //        //给ProcessData赋值
                //        v.Value = item.data_value;
                //        #endregion

                //        dicPara.Add(item.data_item, item.data_value);
                //    }
                //}

                #endregion

                //#region 把首件检设备收集到的点检数据上报给MES  
                //string insLot = string.Empty;
                //string insPnl = string.Empty;
                //string[] insString = msg.BODY.job_id.Trim().Split('-').ToArray();
                //if (insString[0].Length < 17)//小于17码的是Lot
                //{
                //    insLot = insString[0];
                //}
                //else
                //{
                //    insPnl = insString[0];
                //}
                //if (string.IsNullOrEmpty(msg.BODY.job_id.Trim()))
                //{
                //    //Hold AOI需要用Q的Panel ID做上报数据
                //    if (EAPEnvironment.commonLibrary.qPanelIDList.Count != 0)
                //    {
                //        insPnl = EAPEnvironment.commonLibrary.qPanelIDList.Dequeue();
                //    }

                //}
                ////如果是首件检查机，则上报点检数据给MES
                //if (em.Type == eEquipmentType.IE)
                //{
                //    new WebAPIReport().EAP_MES_EquipmentDataCollection(new MessageModel.DataCollection()
                //    {
                //        SubEqpID = em.EQID.Trim(),
                //        WipDataSetUp = "点检数据收集",
                //        WIPDataList = LWipData,
                //        ServiceName = "AdHocWIPData",
                //        LotID = insLot,
                //        PnlID = insPnl
                //    }, 1);
                //}

                //#endregion

                //#region Reply Message
                //HostService.HostService.ProcessDataReportReply(msg.HEADER.TRANSACTIONID, em.EQID.Trim());
                //#endregion
                //#region [如果是没有完工信号的单机设备，以ProcessData信号做为完工信号。由设定档决定是否跑这一段]
                ////if (设定档)
                ////{
                ////    var lm = Environment.EAPEnvironment.commonLibrary.commonModel.GetLotModelByLotID(msg.BODY.job_id.Trim());
                ////    lm.LotProcessStatus = eLotProcessStatus.Complete;

                ////    EAP.Environment.EAPEnvironment.commonLibrary.commonModel.AddProcessLotModel((LotModel)lm.Clone());
                ////}
                //#endregion

                //#region 分板线四合一Panel 读取后记录Panel List，预备给收板机Track Out使用
                //if (Environment.EAPEnvironment.commonLibrary.lineModel.LineType.Equals("分板线"))
                //{
                //    SetTrackOutItems(dicPara, em);
                //}

                


                //#endregion
                //History.EAP_EQP_PROCESSDATA(em, dl);

            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", "ProcessDataReport", ex.Message.ToString(), ex.StackTrace.ToString()));
            }
            finally
            {

            }
        }
        private void SetTrackOutItems(Dictionary<string, string> dicPara, EquipmentModel em)
        {
            try
            {
                TrackOutItem tt = new TrackOutItem();
                //根据分板线四合一上报的ProcessData，分别存入内层码和外层码
                toutPnlID tp = new toutPnlID();
                tp.tPanelID = dicPara["VCR读内层码内容"];
                tp.tOutPnlID = dicPara["VCR复读内容"];
                if (tp.tOutPnlID.Length < 17)
                {
                    string errorMsg = string.Format("Panel ID<{0}>异常，拒绝生产.", tp.tOutPnlID);
                    BaseComm.LogMsg(Log.LogLevel.Error, errorMsg);
                    return;
                }
                tLot tl = new tLot();
                tl.tLotID = tp.tOutPnlID.Substring(0, 14);
                tl.toutPnlList.Add(tp);

                tt.PN = em.CurrentPN;
                tt.tLotList.Add(tl);

                Environment.EAPEnvironment.commonLibrary.commonModel.TrackOutItems.Add(tt);
                //var aa = (from n in Environment.EAPEnvironment.commonLibrary.commonModel.TrackOutItems from I in n.tLotList from a in I.toutPnlList where a.tOutPnlID.Contains(tl.tLotID) select a.tOutPnlID).ToList();

            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", "SetTrackOutItems", ex.Message.ToString(), ex.StackTrace.ToString()));
            }
        }
        private Dictionary<string,string> CreateNewDictoinary()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("Bn", "");//批号
            dic.Add("Pn", "");//料号
            dic.Add("FZ_KL_COUNT", "0");//开路数（默认为0）
            dic.Add("FZ_DL_COUNT", "0");//短路数（默认为0）
            dic.Add("FZ_DZ_COUNT", "0");//低阻数
            dic.Add("StrOpenAndShort", "0");//开短路数
            dic.Add("reason", "");//NG原因（默认为空）
            dic.Add("ngContent", "");//NG坐标内容（默认为空）
            dic.Add("insRes", "0");//绝缘电阻
            dic.Add("rdson", "0");//导通电阻
            dic.Add("testVol", "0");//测试电压
            dic.Add("ngRes", "");//NG的阻值（默认为空）
            dic.Add("StarTime", "");//测试开始时间
            dic.Add("EndTime", "");//测试结束时间
            dic.Add("TestPassQty", "0");//Ok板数量
            dic.Add("TestTotalQty", "0");//总测试板数量
            dic.Add("WorkerId", "");//员工编号
            dic.Add("TestType", "");//测试类型（常规、复测）
            return dic;
        }
    }
}
