//******************************************************************
//   系统名称 : iJedha.Automation.EAP.Rule
//   文件概要 : SocketMsgHandle
//   作    者 : 张明哲
//   <更新履历>
//   1.0.0.0    2020/10/11 14:00:14
//******************************************************************

using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Environment;
using iJedha.Automation.EAP.LibraryBase;
using iJedha.Automation.EAP.Model;
using iJedha.Automation.EAP.WebAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace iJedha.Automation.EAP.Rule
{
    public partial class RsMsgHandle
    {
        public void Invoke(object _DowryObj)
        {
            try
            {
                if (string.IsNullOrEmpty(EAPEnvironment.receiveData)) return;

                string evtName = GetMsgName(EAPEnvironment.receiveData);
                if (EAPEnvironment.receiveData.Length>5 && SubstringCount(EAPEnvironment.receiveData, @"<?xml")!=1)
                {
                    EAPEnvironment.receiveData = string.Empty;
                }
                if (string.IsNullOrEmpty(evtName) == false)
                {
                    #region Decode Message
                    SocketMessageStructure.ProcessDataReport msg = new SocketMessageStructure.ProcessDataReport();
                    if (new Serialize<SocketMessageStructure.ProcessDataReport>().DeSerializeXML(EAPEnvironment.receiveData, out msg) == false)
                    {
                        BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Socket Message<{0}> Decode Error, Content<{1}>", "ProcessDataReport", EAPEnvironment.receiveData));
                        EAPEnvironment.receiveData = "";
                        return;
                    }
                    #endregion

                    #region Record Log
                    BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("<{2}>Socket Message<{0}> Recv OK , Content<{1}>",
                    "ProcessDataReport", msg.WriteToXml(), msg.BODY.eqp_id.Trim()));

                    EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(msg.BODY.eqp_id.Trim());
                    if (em == null)
                    {
                        //error log
                        BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<ProcessDataReport> Find Error", msg.BODY.eqp_id.Trim()));
                        EAPEnvironment.receiveData = "";
                        return;
                    }
                    #endregion

                    #region 连线检查
                    if (em.isCheckConnect && em.isCheckControlMode)
                    {
                        if (em.ConnectMode == Model.eConnectMode.DISCONNECT || em.ControlMode != eControlMode.REMOTE)
                        {
                            BaseComm.LogMsg(Log.LogLevel.Error, $"设备ID[{em.EQID}],连线状态为[{em.ConnectMode.ToString()}],控制模式为[{em.ControlMode.ToString()}]");
                            EAPEnvironment.receiveData = "";
                            return;
                        }
                    }
                    #endregion

                    #region 拆解上报的Data_list存到字典内 【如何区分批号和板号：小于17码的是批号】
                    Socket_DynamicLibraryBase dl;
                    dl = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetDynamicLibrary(em.EQID);
                    Dictionary<string, string> dicPara = new Dictionary<string, string>();
                    List<MessageModel.WIPData> LWipData = new List<MessageModel.WIPData>();
                    foreach (var item in msg.BODY.proc_data_list)
                    {
                        var v = dl.Dic_ProcessDataModel.Values.Where(r => r.ID == item.data_item).FirstOrDefault();
                        if (v == null)
                        {
                            BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<ProcessDataReport> Find Dic_ProcessDataModel Error", msg.BODY.eqp_id.Trim()));
                            EAPEnvironment.receiveData = "";
                            return;
                        }
                        MessageModel.WIPData wIPData = new MessageModel.WIPData();
                        wIPData.WipDataName = v.Name;
                        wIPData.WipDataValue = item.data_value;
                        LWipData.Add(wIPData);
                        #region Dic_ProcessDataModel

                        v.Value = item.data_value;
                        #endregion

                        dicPara.Add(item.data_item, item.data_value);
                    }
                    #endregion

                    #region 把首件检设备收集到的点检数据上报给MES  
                    string insLot = string.Empty;
                    string insPnl = string.Empty;
                    string[] insString = msg.BODY.job_id.Trim().Split('-').ToArray();
                    if (insString[0].Length < 17)
                    {
                        insLot = insString[0];
                    }
                    else
                    {
                        insPnl = insString[0];
                    }
                    if (string.IsNullOrEmpty(msg.BODY.job_id.Trim()))
                    {
                        if (EAPEnvironment.commonLibrary.qPanelIDList.Count != 0)
                        {
                            insPnl = EAPEnvironment.commonLibrary.qPanelIDList.Dequeue();
                        }

                    }
                    if (em.Type == eEquipmentType.IE)
                    {
                        new WebAPIReport().EAP_MES_EquipmentDataCollection(new MessageModel.DataCollection()
                        {
                            SubEqpID = msg.BODY.eqp_id.Trim(),
                            WipDataSetUp = "点检数据收集",
                            WIPDataList = LWipData,
                            ServiceName = "AdHocWIPData",
                            LotID = insLot,
                            PnlID = insPnl
                        }, 1);
                    }

                    #endregion

                    History.EAP_EQP_PROCESSDATA(em, dl);

                    EAPEnvironment.receiveData = "";
                }

            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Core.Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return;
            }
            finally
            {

            }
        }

        private string GetMsgName(string data)
        {
            try
            {
                if (string.IsNullOrEmpty(data)) return "";

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(data);

                return xmlDoc.SelectSingleNode("//messagename").InnerText;
            }
            catch
            {
                return "";
            }
        }
        public int SubstringCount(string str,string substring)
        {
            try
            {
                string strReplaced = "";
                if (str.Contains(substring))
                {
                    strReplaced = str.Replace(substring, "");
                }
                return (str.Length - strReplaced.Length) / substring.Length;
            } 
            catch (Exception ex)
            {
                return 0;
            }
        }
    }
}
