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
        /// 下载物料生产任务
        /// </summary>
        /// <param name="em"></param>
        /// <param name="MaterialLotID"></param>
        /// <param name="Direction"></param>
        /// <param name="PortID"></param>
        public void JobDataDownload(EquipmentModel em, string MaterialLotID, string Direction, string PortID)
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
                msg.BODY.port_id = PortID   ;
                msg.BODY.job_id = MaterialLotID;
                msg.BODY.total_panel_count = "";

                JobDataDownload.crecipe_parameter cRecipeParameter = new JobDataDownload.crecipe_parameter();

                cRecipeParameter.item_name = "Direction";
                cRecipeParameter.item_value = Direction;
                msg.BODY.recipe_parameter_list.Add(cRecipeParameter);


                msg.RETURN.RETURNCODE = "0";
                msg.RETURN.RETURNMESSAGE = string.Empty;
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
