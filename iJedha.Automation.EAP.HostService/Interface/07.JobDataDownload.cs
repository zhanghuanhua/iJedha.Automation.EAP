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
using iJedha.SocketMessageStructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace iJedha.Automation.EAP.HostService
{
    public partial class HostService
    {
        /// <summary>
        /// 生产任务下载指令
        /// </summary>
        /// <param name="em"></param>
        /// <param name="lm"></param>
        /// <param name="PortID"></param>
        /// <param name="listParameterModel"></param>
        public void JobDataDownload(EquipmentModel em, LotModel lm, string PortID, List<MessageModel.Param> listParameterModel)
        {
            try
            {
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

                //获取MES下载内层Lot设备ID
                var SubEqpIDList = (from q in lm.InnerLotList select q.SubEqpID).ToList();
                //如果设定了isDownloadInnerLot==true但是设备ID不在SubEqpID里面，不下载生产任务
                if (em.isDownloadInnerLot)
                {
                    if (!SubEqpIDList.Contains(em.EQID))
                    {
                        return;
                    }
                }
                SocketMessageStructure.JobDataDownload msg = new SocketMessageStructure.JobDataDownload();
                msg.HEADER.MESSAGENAME = eSocketCommand.JobDataDownload.GetEnumDescription();
                msg.HEADER.TRANSACTIONID = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                #region [下载首件任务]
                if (lm.FirstInspFlag)
                {
                    #region [首件结果OK，下载剩余生产任务给设备]
                    if (Environment.EAPEnvironment.commonLibrary.isInspecResultOK)
                    {
                        msg.BODY.eqp_id = em.EQID;
                        msg.BODY.port_id = PortID;
                        msg.BODY.job_id = lm.LotID;
                        msg.BODY.total_panel_count = lm.LotQty.ToString();
                        msg.BODY.recipe_id = "";
                        msg.BODY.cam_path = "";
                        msg.BODY.recipe_path = "";

                        var camPath = em.CamPath;
                        if (!string.IsNullOrEmpty(camPath))
                        {
                            msg.BODY.cam_path = camPath.Replace('/', '\\');
                        }
                        //msg.BODY.cam_path = (from q in listParameterModel where q.ParamName.Equals("CAM档全路径") select q.ParamValue).FirstOrDefault();
                        msg.BODY.part_no = lm.PN;
                        msg.BODY.layer_count = lm.Layer;
                        if (em.isDownloadPanelList)
                        {
                            foreach (var item in lm.PanelList)//lm.PanelList
                            {
                                JobDataDownload.cpanel cPanel = new JobDataDownload.cpanel();
                                cPanel.panel_id = item.PanelID;
                                msg.BODY.panel_list.Add(cPanel);
                            }
                        }

                        #region [广合设备下载参数列表]
                        if (em.EqVendor.Equals("广合设备"))
                        {
                            msg.BODY.recipe_parameter_list.Add(new JobDataDownload.crecipe_parameter() { item_name = "TurnStatus", item_value = lm.IsRotate });
                            msg.BODY.recipe_parameter_list.Add(new JobDataDownload.crecipe_parameter() { item_name = "PanelLength", item_value = lm.PNLength });
                            msg.BODY.recipe_parameter_list.Add(new JobDataDownload.crecipe_parameter() { item_name = "PanelWidth", item_value = lm.PNWidth });
                        }
                        #endregion
                        //配方取值
                        foreach (var item in listParameterModel)
                        {
                            if (item.ParamName.Equals("验孔配方名称") || item.ParamName.Equals("配方名称"))
                            {
                                msg.BODY.recipe_id = item.ParamValue;
                            }
                            if (item.ParamName.Equals("配方档全路径"))
                            {
                                msg.BODY.recipe_path = item.ParamValue;
                            }
                            if (item.ParamName.Equals("CAM档全路径") || item.ParamName.Equals("AOI资料路径"))
                            {
                                msg.BODY.cam_path = item.ParamValue;
                            }

                            JobDataDownload.crecipe_parameter cRecipeParameter = new JobDataDownload.crecipe_parameter();
                            if (!em.isUseDataName)//使用DataCode
                            {
                                cRecipeParameter.item_name = GetDataCode(item.ParamName, em.EQID);
                            }
                            else
                            {
                                cRecipeParameter.item_name = item.ParamName;
                            }
                            cRecipeParameter.item_value = item.ParamValue;
                            msg.BODY.recipe_parameter_list.Add(cRecipeParameter);
                        }
                    }
                    #endregion
                    else
                    {
                        msg.BODY.eqp_id = em.EQID;
                        msg.BODY.port_id = PortID;
                        msg.BODY.job_id = lm.LotID;
                        msg.BODY.total_panel_count = lm.FirstInspQty;
                        //msg.BODY.panel_size = lm.l;
                        msg.BODY.recipe_id = "";
                        msg.BODY.recipe_path = "";
                        var camPath = "";
                        if (!string.IsNullOrEmpty(camPath))
                        {
                            msg.BODY.cam_path = camPath.Replace('/', '\\');
                        }
                        //msg.BODY.cam_path = (from q in listParameterModel where q.ParamName.Equals("CAM档全路径") select q.ParamValue).FirstOrDefault();
                        msg.BODY.part_no = lm.PN;
                        msg.BODY.layer_count = lm.Layer;
                        if (em.isDownloadPanelList)
                        {
                            foreach (var item in lm.PanelList)//lm.PanelList
                            {
                                JobDataDownload.cpanel cPanel = new JobDataDownload.cpanel();
                                cPanel.panel_id = item.PanelID;
                                msg.BODY.panel_list.Add(cPanel);
                            }
                        }

                        #region [广合设备下载参数列表]
                        if (em.EqVendor.Equals("广合设备"))
                        {
                            msg.BODY.recipe_parameter_list.Add(new JobDataDownload.crecipe_parameter() { item_name = "TurnStatus", item_value = lm.IsRotate });
                            msg.BODY.recipe_parameter_list.Add(new JobDataDownload.crecipe_parameter() { item_name = "PanelLength", item_value = lm.PNLength });
                            msg.BODY.recipe_parameter_list.Add(new JobDataDownload.crecipe_parameter() { item_name = "PanelWidth", item_value = lm.PNWidth });
                        }
                        #endregion
                        //配方取值
                        foreach (var item in listParameterModel)
                        {
                            if (item.ParamName.Equals("验孔配方名称") || item.ParamName.Equals("配方名称"))
                            {
                                msg.BODY.recipe_id = item.ParamValue;
                            }
                            if (item.ParamName.Equals("配方档全路径"))
                            {
                                msg.BODY.recipe_path = item.ParamValue;
                            }
                            if (item.ParamName.Equals("CAM档全路径") || item.ParamName.Equals("AOI资料路径"))
                            {
                                msg.BODY.cam_path = item.ParamValue;
                            }

                            JobDataDownload.crecipe_parameter cRecipeParameter = new JobDataDownload.crecipe_parameter();

                            if (!em.isUseDataName)//使用DataCode
                            {
                                cRecipeParameter.item_name = GetDataCode(item.ParamName, em.EQID);
                            }
                            else
                            {
                                cRecipeParameter.item_name = item.ParamName;
                            }

                            cRecipeParameter.item_value = item.ParamValue;
                            msg.BODY.recipe_parameter_list.Add(cRecipeParameter);
                        }
                    }
                }
                #endregion

                else
                {
                    #region [非首件下载生产任务]
                    msg.BODY.eqp_id = em.EQID;
                    msg.BODY.port_id = PortID;

                    msg.BODY.job_id = lm.LotID;

                    //冲孔连棕化线的收板机下载收板片数
                    if (EAPEnvironment.commonLibrary.lineModel.LineType == "冲孔连棕化")
                    {
                        if (em.Type == eEquipmentType.U)
                        {
                            msg.BODY.total_panel_count = lm.UnloadQtyGroup;
                        }
                        else
                        {
                            if (em.isDownloadInnerLot)
                            {
                                msg.BODY.total_panel_count = (from q in lm.InnerLotList where q.SubEqpID == em.EQID select q.LoadQty).FirstOrDefault();
                            }
                            else
                            {
                                msg.BODY.total_panel_count = lm.InnerLotTotalQty.ToString();
                            }
                        }
                    }
                    else
                    {
                        msg.BODY.total_panel_count = lm.PanelTotalQty.ToString();
                    }
                    //msg.BODY.panel_size = lm.l;
                    msg.BODY.recipe_id = "";
                    msg.BODY.recipe_path = "";
                    var camPath = "";
                    if (!string.IsNullOrEmpty(camPath))
                    {
                        msg.BODY.cam_path = camPath.Replace('/', '\\');
                    }
                    //msg.BODY.cam_path = (from q in listParameterModel where q.ParamName.Equals("CAM档全路径") select q.ParamValue).FirstOrDefault();
                    msg.BODY.part_no = lm.PN;
                    msg.BODY.layer_count = lm.Layer;

                    var InnerLot = lm.InnerLotList.Where(r => r.SubEqpID == em.EQID).FirstOrDefault();
                    if (InnerLot != null)
                    {
                        #region 使用内层Lot下载
                        if (em.isDownloadInnerLot)
                        {
                            msg.BODY.job_id = InnerLot.InnerLotID.Trim();
                            msg.BODY.layer_count = InnerLot.InnerLayer;
                            msg.BODY.total_panel_count = InnerLot.LoadQty;
                            if (em.isDownloadPanelList)
                            {
                                foreach (var item in InnerLot.ListPanel)//lm.PanelList
                                {
                                    JobDataDownload.cpanel cPanel = new JobDataDownload.cpanel();
                                    cPanel.panel_id = item.PanelID;
                                    msg.BODY.panel_list.Add(cPanel);
                                }
                            }
                            #region
                            /*JobDataDownload.crecipe_parameter cIsSkipPunching = new JobDataDownload.crecipe_parameter();
                            cIsSkipPunching.item_name ="IsSkipPunching";
                            cIsSkipPunching.item_value = InnerLot.IsSkipPunching;
                            msg.BODY.recipe_parameter_list.Add(cIsSkipPunching);

                            JobDataDownload.crecipe_parameter cIsTurnover = new JobDataDownload.crecipe_parameter();
                            cIsTurnover.item_name = "IsTurnover";
                            cIsTurnover.item_value = InnerLot.IsTurnover;
                            msg.BODY.recipe_parameter_list.Add(cIsTurnover);*/
                            #endregion
                        }
                        #endregion
                    }
                    else
                    {
                        if (em.isDownloadPanelList)
                        {
                            foreach (var item in lm.PanelList)//lm.PanelList
                            {
                                JobDataDownload.cpanel cPanel = new JobDataDownload.cpanel();
                                cPanel.panel_id = item.PanelID;
                                msg.BODY.panel_list.Add(cPanel);
                            }
                        }
                    }

                    #region
                    /*
                    JobDataDownload.crecipe_parameter cPNLength = new JobDataDownload.crecipe_parameter();
                    cPNLength.item_name = "PNLength";
                    cPNLength.item_value = lm.PNLength;
                    msg.BODY.recipe_parameter_list.Add(cPNLength);

                    JobDataDownload.crecipe_parameter cPNWidth = new JobDataDownload.crecipe_parameter();
                    cPNWidth.item_name = "PNWidth";
                    cPNWidth.item_value = lm.PNWidth;
                    msg.BODY.recipe_parameter_list.Add(cPNWidth);

                    JobDataDownload.crecipe_parameter cPNThick = new JobDataDownload.crecipe_parameter();
                    cPNThick.item_name = "PNThick";
                    cPNThick.item_value = lm.PNThick;
                    msg.BODY.recipe_parameter_list.Add(cPNThick);

                    JobDataDownload.crecipe_parameter cIsRotate = new JobDataDownload.crecipe_parameter();
                    cIsRotate.item_name =  "IsRotate";
                    cIsRotate.item_value = lm.IsRotate;
                    msg.BODY.recipe_parameter_list.Add(cIsRotate);

                    JobDataDownload.crecipe_parameter cIsTurnoverGroup = new JobDataDownload.crecipe_parameter();
                    cIsTurnoverGroup.item_name =  "IsTurnoverGroup";
                    cIsTurnoverGroup.item_value = lm.IsTurnoverGroup;
                    msg.BODY.recipe_parameter_list.Add(cIsTurnoverGroup);

                    JobDataDownload.crecipe_parameter cIsSkipPunchingGroup = new JobDataDownload.crecipe_parameter();
                    cIsSkipPunchingGroup.item_name =  "IsSkipPunchingGroup";
                    cIsSkipPunchingGroup.item_value = lm.IsSkipPunchingGroup;
                    msg.BODY.recipe_parameter_list.Add(cIsSkipPunchingGroup);
                    */
                    #region[减铜棕化跳过棕化设备时使用，下载只送板不收板给收板机]  &&  [非连线工艺]
                    if (EAPEnvironment.commonLibrary.isTransferStatus)
                    {
                        if (em.Type == eEquipmentType.U)
                        {
                            JobDataDownload.crecipe_parameter cIsTransferStatus = new JobDataDownload.crecipe_parameter();
                            cIsTransferStatus.item_name = "TransferStatus";
                            cIsTransferStatus.item_value = "0";//0：只收板不送板，1：只送板不收板
                            msg.BODY.recipe_parameter_list.Add(cIsTransferStatus);
                            //msg.BODY.recipe_parameter_list.Add(new JobDataDownload.crecipe_parameter() { item_name= "TransferStatus", item_value ="0"});
                        }
                    }
                    else
                    {
                        if (em.Type == eEquipmentType.U)
                        {
                            JobDataDownload.crecipe_parameter cIsTransferStatus = new JobDataDownload.crecipe_parameter();
                            cIsTransferStatus.item_name = "TransferStatus";
                            cIsTransferStatus.item_value = "1";//0：只收板不送板，1：只送板不收板
                            msg.BODY.recipe_parameter_list.Add(cIsTransferStatus);
                            //msg.BODY.recipe_parameter_list.Add(new JobDataDownload.crecipe_parameter() { item_name= "TransferStatus", item_value ="1"});
                        }
                    }
                    #endregion

                    #region[分板线多添加四合一板起始号]PnlStartSN
                    if (em.isPnlStartSN)
                    {
                        JobDataDownload.crecipe_parameter cIsPnlStartSN = new JobDataDownload.crecipe_parameter();
                        cIsPnlStartSN.item_name = "PnlStartSN";
                        cIsPnlStartSN.item_value = lm.PnlStartSN;
                        msg.BODY.recipe_parameter_list.Add(cIsPnlStartSN);
                    }
                    #endregion

                    #region[VCP线投板机和VCP设备下载DummyQty给设备]
                    if (em.isUseDummy)
                    {
                        if (em.isAType)
                        {
                            if (lm.DummyType == "A")
                            {
                                JobDataDownload.crecipe_parameter cIsDummyType = new JobDataDownload.crecipe_parameter();
                                cIsDummyType.item_name = "DummyType";
                                cIsDummyType.item_value = lm.DummyType;
                                msg.BODY.recipe_parameter_list.Add(cIsDummyType);

                                JobDataDownload.crecipe_parameter cIsFrontDummyQty = new JobDataDownload.crecipe_parameter();
                                cIsFrontDummyQty.item_name = "FrontDummyQty";
                                cIsFrontDummyQty.item_value = lm.FrontDummyQty == "" ? "0" : lm.FrontDummyQty;
                                msg.BODY.recipe_parameter_list.Add(cIsFrontDummyQty);

                                JobDataDownload.crecipe_parameter cIsAfterDummyQty = new JobDataDownload.crecipe_parameter();
                                cIsAfterDummyQty.item_name = "AfterDummyQty";
                                cIsAfterDummyQty.item_value = lm.AfterDummyQty == "" ? "0" : lm.AfterDummyQty;
                                msg.BODY.recipe_parameter_list.Add(cIsAfterDummyQty);
                            }
                        }
                        else
                        {
                            JobDataDownload.crecipe_parameter cIsDummyType = new JobDataDownload.crecipe_parameter();
                            cIsDummyType.item_name = "DummyType";
                            cIsDummyType.item_value = lm.DummyType;
                            msg.BODY.recipe_parameter_list.Add(cIsDummyType);

                            JobDataDownload.crecipe_parameter cIsFrontDummyQty = new JobDataDownload.crecipe_parameter();
                            cIsFrontDummyQty.item_name = "FrontDummyQty";
                            cIsFrontDummyQty.item_value = lm.FrontDummyQty == "" ? "0" : lm.FrontDummyQty;
                            msg.BODY.recipe_parameter_list.Add(cIsFrontDummyQty);

                            JobDataDownload.crecipe_parameter cIsAfterDummyQty = new JobDataDownload.crecipe_parameter();
                            cIsAfterDummyQty.item_name = "AfterDummyQty";
                            cIsAfterDummyQty.item_value = lm.AfterDummyQty == "" ? "0" : lm.AfterDummyQty;
                            msg.BODY.recipe_parameter_list.Add(cIsAfterDummyQty);
                        }

                    }
                    #endregion

                    #region[PE冲孔机是否下载冲孔信息]
                    if (em.isPunching)
                    {
                        string PunchingInfo = string.Empty;
                        string LayerInfo = string.Empty;
                        //LayerInfo = string.Join(",", EAPEnvironment.commonLibrary.LayerLevel);
                        //PunchingInfo = string.Join(",", EAPEnvironment.commonLibrary.PunchingList);

                        LayerInfo = string.Join(",", lm.LayerLevel);
                        PunchingInfo = string.Join(",", lm.PunchingList);

                        msg.BODY.recipe_parameter_list.Add(new JobDataDownload.crecipe_parameter { item_name = "层数", item_value = LayerInfo });
                        msg.BODY.recipe_parameter_list.Add(new JobDataDownload.crecipe_parameter { item_name = "层次", item_value = lm.InnerLotList.Count.ToString() });
                        msg.BODY.recipe_parameter_list.Add(new JobDataDownload.crecipe_parameter { item_name = "层次顺序与是否冲孔", item_value = PunchingInfo });
                    }
                    #endregion

                    #region[冲孔棕化线翻面机翻面规则]
                    if (em.isTurnoverGroup)
                    {
                        msg.BODY.recipe_parameter_list.Add(new JobDataDownload.crecipe_parameter() { item_name = "IsTurnoverGroup", item_value = lm.IsTurnoverGroup });
                    }
                    #endregion

                    #region [广合设备下载参数列表]
                    if (em.EqVendor.Equals("广合设备"))
                    {
                        msg.BODY.recipe_parameter_list.Add(new JobDataDownload.crecipe_parameter() { item_name = "TurnStatus", item_value = lm.IsRotate });
                        msg.BODY.recipe_parameter_list.Add(new JobDataDownload.crecipe_parameter() { item_name = "PanelLength", item_value = lm.PNLength });
                        msg.BODY.recipe_parameter_list.Add(new JobDataDownload.crecipe_parameter() { item_name = "PanelWidth", item_value = lm.PNWidth });
                    }
                    #endregion
                    #endregion
                    //配方取值
                    foreach (var item in listParameterModel)
                    {
                        if (item.ParamName.Equals("验孔配方名称") || item.ParamName.Equals("配方名称"))
                        {
                            msg.BODY.recipe_id = item.ParamValue;
                        }
                        if (item.ParamName.Equals("配方档全路径"))
                        {
                            msg.BODY.recipe_path = item.ParamValue;
                        }
                        if (item.ParamName.Equals("CAM档全路径") || item.ParamName.Equals("AOI资料路径"))
                        {
                            msg.BODY.cam_path = item.ParamValue;
                        }
                        JobDataDownload.crecipe_parameter cRecipeParameter = new JobDataDownload.crecipe_parameter();

                        if (!em.isUseDataName)//使用DataCode
                        {
                            cRecipeParameter.item_name = GetDataCode(item.ParamName, em.EQID);
                        }
                        else
                        {
                            cRecipeParameter.item_name = item.ParamName;
                        }

                        cRecipeParameter.item_value = item.ParamValue;
                        msg.BODY.recipe_parameter_list.Add(cRecipeParameter);
                    }

                    #region 回流线下载Batch ID，Batch Count，Workorder到参数列表
                    if (em.isReturnLine)
                    {
                        msg.BODY.recipe_parameter_list.Add(new JobDataDownload.crecipe_parameter() { item_name = "BatchID", item_value = lm.JobID });
                        msg.BODY.recipe_parameter_list.Add(new JobDataDownload.crecipe_parameter() { item_name = "BatchCount", item_value = lm.JobTotalQty });
                        msg.BODY.recipe_parameter_list.Add(new JobDataDownload.crecipe_parameter() { item_name = "WorkOrder", item_value = lm.WorkOrder });
                        msg.BODY.recipe_parameter_list.Add(new JobDataDownload.crecipe_parameter() { item_name = "WorkOrderPNLQty", item_value = lm.WorkOrderPNLQty });
                        if (!string.IsNullOrEmpty(EAPEnvironment.commonLibrary.UseLoadEquipmentNo))
                        {
                            msg.BODY.recipe_parameter_list.Add(new JobDataDownload.crecipe_parameter() { item_name = "LoadEquipmentSequence", item_value = EAPEnvironment.commonLibrary.UseLoadEquipmentNo });
                        }
                    }
                    #endregion
                }
                #endregion
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
        /// <summary>
        /// 参数名称转Data Code
        /// </summary>
        /// <param name="ParameterName"></param>
        /// <param name="EqID"></param>
        /// <returns></returns>
        public string GetDataCode(string ParameterName, string EqID)
        {
            string DataCode = string.Empty;
            try
            {
                Socket_DynamicLibraryBase dl;
                dl = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetDynamicLibrary(EqID);
                foreach (var item in dl.Dic_ParameterModel.Values)
                {
                    if (item.Name == ParameterName)
                    {
                        DataCode = item.ID;
                    }
                }
                return DataCode;
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return DataCode;
            }
        }

    }
}
