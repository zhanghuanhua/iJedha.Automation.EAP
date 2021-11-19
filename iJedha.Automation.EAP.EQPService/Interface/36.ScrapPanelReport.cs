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
    public partial class ScrapPanelReport : BaseComm
    {
        public void EventHandle(string evtXml)
        {

            try
            {
                #region Decode Message
                SocketMessageStructure.ScrapPanelReport msg = new SocketMessageStructure.ScrapPanelReport();
                if (new Serialize<SocketMessageStructure.ScrapPanelReport>().DeSerializeXML(evtXml, out msg) == false)
                {
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Socket Message<{0}> Decode Error, Content<{1}>", "ScrapPanelReport", evtXml));
                    return;
                }
                #endregion

                #region Record Log
                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("<{2}>Socket Message<{0}> Recv OK , Content<{1}>",
                "ScrapPanelReport", msg.WriteToXml(), msg.BODY.eqp_id.Trim()));

                EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(msg.BODY.eqp_id.Trim());
                if (em == null)
                {
                    //error log
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<ScrapPanelReport> Find Error", msg.BODY.eqp_id.Trim()));
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

                EAPEnvironment.commonLibrary.ScrapPanelCount = msg.BODY.panel_count == "" ? int.Parse("0") : int.Parse(msg.BODY.panel_count);
                List<string> lstPanel = new List<string>();
                foreach (var item in msg.BODY.panel_list)
                {
                    lstPanel.Add(item.panel_id);
                }
                //比对上报板子实物数量和上报数量是否一致，不一致记录错误信息
                if (lstPanel.Count!= EAPEnvironment.commonLibrary.ScrapPanelCount)
                {
                    BaseComm.LogMsg(Log.LogLevel.Error, $"报废数量<{msg.BODY.panel_count}>与实际报废板数量<{lstPanel.Count}>不一致.");
                }
                #region Reply Message
                HostService.HostService.ScrapPanelReportReply(msg.HEADER.TRANSACTIONID, msg.BODY.eqp_id.Trim());
                #endregion
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", "ScrapPanelReport", ex.Message.ToString(), ex.StackTrace.ToString()));
            }
            finally
            {

            }
        }
        
    }
}
