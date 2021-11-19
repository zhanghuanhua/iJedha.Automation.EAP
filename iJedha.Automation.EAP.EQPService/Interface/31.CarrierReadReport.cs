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
using iJedha.Automation.EAP.WebAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace iJedha.Automation.EAP.EQPService
{
    public partial class CarrierReadReport : BaseComm
    {
        public void EventHandle(string evtXml)
        {
            try
            {
                bool isEquipmentStatusOK = false;
                #region Decode Message
                SocketMessageStructure.CarrierReadReport msg = new SocketMessageStructure.CarrierReadReport();
                if (new Serialize<SocketMessageStructure.CarrierReadReport>().DeSerializeXML(evtXml, out msg) == false)
                {
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Socket Message<{0}> Decode Error, Content<{1}>", "CarrierReadReport", evtXml));
                    return;
                }
                #endregion

                #region Record Log
                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("<{2}>Socket Message<{0}> Recv OK , Content<{1}>",
                "CarrierReadReport", msg.WriteToXml(), msg.BODY.eqp_id.Trim()));
                #endregion

                #region 上报Carrier ID之后EAP做处理
                EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(msg.BODY.eqp_id.Trim());
                if (em == null)
                {
                    //error log
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<CarrierReadReport> Find Error", msg.BODY.eqp_id.Trim()));
                    return;
                }
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


                #region 开料线需上报LOADCOMPLETE后，扫描Carrier上报EAP_LotInfoRequest

                //if (Environment.EAPEnvironment.commonLibrary.LineName.Equals("开料线"))
                //{
                //    PortModel pm = em.GetPortModelByPortID(ePortID.L01.ToString());
                //    if (pm == null)
                //    {
                //        //error log
                //        BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<CarrierReadReport> PortModel Find Error", msg.BODY.eqp_id.Trim()));
                //        return;
                //    }
                //    if (pm.PortStatus == ePortStatus.LOADCOMPLETE)
                //    {
                //        MessageModel.LotInfoRequest lotRequest = new MessageModel.LotInfoRequest();
                //        lotRequest.MainEqpID = EAPEnvironment.commonLibrary.LineName;
                //        lotRequest.SubEqpID = em.EQID;
                //        lotRequest.PortID = ePortID.L01.ToString();
                //        lotRequest.CarrierID = msg.BODY.carrier_id.Trim();
                //        new WebAPIReport().EAP_LotInfoRequest_KL(lotRequest, em, 1);
                //    }
                //}

                #endregion

                #region 检查设备当前状态是否符合生产条件
                switch (Environment.EAPEnvironment.commonLibrary.lineModel.LineType)
                {
                    #region 叠板压合下货逻辑
                    case "叠板压合":
                        var v = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.isReturnLine).FirstOrDefault();
                        if (v != null)
                        {
                            foreach (var item in v.TrayStatusInfo)
                            {
                                if (item.Key == msg.BODY.carrier_id.Trim())
                                {
                                    var vlot = EAPEnvironment.commonLibrary.commonModel.GetLotModel().Where(r => r.JobID == item.Value).FirstOrDefault();
                                    if (vlot == null)
                                    {
                                        //error log
                                        BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> LotModel<BatchID> Find Error", msg.BODY.eqp_id));
                                        return;
                                    }
                                    var pp = (from n in vlot.SubEqpList where n.SubEqpID == em.EQID select n.EQParameter).ToList();
                                    List<MessageModel.Param> parameterModel = new List<MessageModel.Param>();
                                    foreach (var pa in pp)
                                    {
                                        foreach (var subPa in pa)
                                        {
                                            MessageModel.Param _pa = new MessageModel.Param();
                                            _pa.ParamName = subPa.ItemName;
                                            _pa.ParamValue = subPa.ItemValue;
                                            _pa.ParamType = subPa.DataType;
                                            _pa.SubEqpID = em.EQID;
                                            parameterModel.Add(_pa);
                                        }
                                    }
                                    //回流线需添加TrayID参数
                                    parameterModel.Add(new MessageModel.Param { ParamName = "TrayID", ParamValue = msg.BODY.carrier_id.Trim() });
                                    //下载生产任务给压机
                                    new HostService.HostService().JobDataDownload(em, vlot, vlot.LocalPortStation, parameterModel);
                                    //通知压机上料架可以开始进行压合作业
                                    new HostService.HostService().RemoteControlCommand(em.EQID, vlot.LocalPortStation, eRemoteCommand.Start.GetEnumDescription());
                                }
                            }
                        }
                        break;
                    #endregion

                    //#region 开料线需上报LOADCOMPLETE后，扫描Carrier上报EAP_LotInfoRequest
                    //case "开料线":
                    //    PortModel pm = em.GetPortModelByPortID(ePortID.L01.ToString());
                    //    if (pm == null)
                    //    {
                    //        //error log
                    //        BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<CarrierReadReport> PortModel Find Error", msg.BODY.eqp_id.Trim()));
                    //        return;
                    //    }
                    //    if (pm.PortStatus == ePortStatus.LOADCOMPLETE)
                    //    {
                    //        MessageModel.LotInfoRequest lotRequest = new MessageModel.LotInfoRequest();
                    //        lotRequest.MainEqpID = EAPEnvironment.commonLibrary.LineName;
                    //        lotRequest.SubEqpID = em.EQID;
                    //        lotRequest.PortID = ePortID.L01.ToString();
                    //        lotRequest.CarrierID = msg.BODY.carrier_id.Trim();
                    //        new WebAPIReport().EAP_LotInfoRequest_KL(lotRequest, em, 1);
                    //    }
                    //    break;
                    //#endregion
                    default:
                        string err;
                        //当读取载具ID需要请求资料时，上报MES请求生产任务
                        if (EAPEnvironment.commonLibrary.lineModel.isCarrierIDRequestLotInfo)
                        {
                            MessageModel.LotInfoRequest lotRequest = new MessageModel.LotInfoRequest();
                            lotRequest.MainEqpID = EAPEnvironment.commonLibrary.LineName;
                            lotRequest.SubEqpID = em.EQID;
                            lotRequest.PortID = ePortID.L01.ToString();
                            lotRequest.CarrierID = msg.BODY.carrier_id.Trim();
                            new WebAPIReport().EAP_LotInfoRequest(lotRequest, em, 1, out err);
                        }
                        break;

                }
                #endregion
                #endregion

                #region Reply Message
                HostService.HostService.CarrierReadReportReply(msg.HEADER.TRANSACTIONID, msg.BODY.eqp_id.Trim());
                #endregion
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", "CarrierReadReport", ex.Message.ToString(), ex.StackTrace.ToString()));
            }
            finally
            {

            }
        }


    }

}
