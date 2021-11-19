//******************************************************************
//   系统名称 : iJedha.Automation.EAP.SocketService
//   文件概要 : 
//   作    者 : 张明哲
//   <更新履历>
//   1.0.0.0    2020/10/1 14:28:28
//******************************************************************

using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Environment;
using iJedha.Automation.EAP.Model;
using iJedha.SocketMessageStructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace iJedha.Automation.EAP.HostService
{
    public partial class HostService
    {
        /// <summary>
        /// 开料下载生产任务
        /// </summary>
        /// <param name="em"></param>
        /// <param name="lm"></param>
        /// <param name="PortID"></param>
        /// <param name="listParameterModel"></param>
        public void JobDataDownload(EquipmentModel em, Lot lm, string PortID,List<MessageModel.Param> listParameterModel)
        {
            try
            {
                #region 连线检查
                if (em.isCheckConnect && em.isCheckControlMode)
                {
                    if (em.ConnectMode == eConnectMode.DISCONNECT || em.ControlMode != eControlMode.REMOTE)
                    {
                        BaseComm.LogMsg(Log.LogLevel.Error, $"设备ID[{em.EQID}],连线状态为[{em.ConnectMode.ToString()}],控制模式为[{em.ControlMode.ToString()}]");
                        return;
                    }
                }
                #endregion
                SocketMessageStructure.JobDataDownload msg = new SocketMessageStructure.JobDataDownload();
                msg.HEADER.MESSAGENAME = eSocketCommand.JobDataDownload.GetEnumDescription();
                msg.HEADER.TRANSACTIONID = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                msg.BODY.eqp_id = em.EQID;
                msg.BODY.port_id = PortID;
                msg.BODY.job_id = lm.LotID; 
                msg.BODY.total_panel_count = lm.PanelTotalQty.ToString();
                EAPEnvironment.commonLibrary.LastLotCount = lm.LoadQty;
                //msg.BODY.panel_size = lm.l;

                msg.BODY.recipe_id = em.PPID;
                msg.BODY.recipe_path = em.RecipePath;
                msg.BODY.cam_path = em.CamPath;
                msg.BODY.part_no = lm.PN;
                msg.BODY.layer_count = "";
                if (em.isDownloadPanelList)
                {
                    foreach (var item in lm.PanelList)//lm.PanelList
                    {
                        JobDataDownload.cpanel cPanel = new JobDataDownload.cpanel();
                        cPanel.panel_id = item.PanelID;
                        msg.BODY.panel_list.Add(cPanel);
                    }
                }
               
                foreach (var item in listParameterModel)
                {
                    SocketMessageStructure.JobDataDownload.crecipe_parameter cRecipeParameter = new SocketMessageStructure.JobDataDownload.crecipe_parameter();

                    cRecipeParameter.item_name = item.ParamName;
                    cRecipeParameter.item_value = item.ParamValue;
                    msg.BODY.recipe_parameter_list.Add(cRecipeParameter);
                }

                msg.RETURN.RETURNCODE = "0";
                msg.RETURN.RETURNMESSAGE = string.Empty;
                string aa = msg.WriteToXml();
                byte[] trxData = Extensions.GetTrxData(msg.WriteToXml());
                EAPEnvironment.Dic_TCPSocketAp[em.EQID].SendData(new socket.SendInfo("", trxData));

                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("<{2}>Socket Message<{0}> Send OK , Content<{1}>",
                        System.Reflection.MethodBase.GetCurrentMethod().Name, msg.WriteToXml(), em.EQID));
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
            }
            finally
            {

            }
        }

        
    }
}
