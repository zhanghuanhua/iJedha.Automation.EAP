//******************************************************************
//   系统名称 : iJedha.Automation.EAP.Rule
//   文件概要 : PPSelectCheck
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
using System.Threading;
/// <summary>
/// 触发功能：
/// </summary>
namespace iJedha.Automation.EAP.Rule
{
    public partial class ProductionConditionCheck
    {
        /// <summary>
        /// 生产条件检查
        /// </summary>
        /// <param name="_DowryObj"></param>
        public void Invoke(object _DowryObj)
        {
            try
            {
                if (!Environment.EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckStart)
                {
                    return;
                }
                ErrorCodeModelBase errm = new ErrorCodeModelBase();
                Dictionary<MessageModel.LotInfo, CommModel> Dic_Message = (Dictionary<MessageModel.LotInfo, CommModel>)_DowryObj;
                MessageModel.LotInfo lotInfo = new MessageModel.LotInfo();
                PortModel port = new PortModel();
                LotModel lm = new LotModel();
                EquipmentModel em = new EquipmentModel();
                //赋值
                foreach (var item in Dic_Message)
                {
                    lotInfo = item.Key;
                    port = item.Value.pm;
                    lm = item.Value.lm;
                    em = item.Value.em;
                }
                var a = lotInfo.MainEqpID;
                #region[前处理塞孔喷涂，非连线工序逻辑]
                if (lm.IsConnectionStatus == eConnectionStatus.Disconnection.GetEnumDescription())
                {
                    DisconnectionFlow(errm, lotInfo, port, em);
                    //************************待续

                }
                #endregion
                else
                {
                    #region[MES下载生产任务，有开始设备设定逻辑]
                    if (!string.IsNullOrEmpty(EAPEnvironment.commonLibrary.lineModel.StartEquipmentID) && EAPEnvironment.commonLibrary.isMESDownloadData)
                    {
                        isMESDownloadData(lotInfo, em, port, errm);
                    }
                    #endregion
                    else
                    {
                        bool isCheckNg = false;
                        #region[一台设备是一个Main设备，机械钻机使用]
                        if (EAPEnvironment.commonLibrary.lineModel.isMainEqpID)
                        {
                            //如果MES下载的当前MainEqpID的钻机不是Idle状态，拒绝下载生产任务
                            var GetCurrentEq = (from n in EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel()
                                                where (n.EQID == lotInfo.MainEqpID)
                                                select n).FirstOrDefault();

                            if (GetCurrentEq != null)
                            {
                                if (EAPEnvironment.commonLibrary.lineModel.isCheckEquipmentStatus)
                                {
                                    if (GetCurrentEq.EQStatus != eEQSts.Idle)
                                    {
                                        string ErrorMsg = string.Format("Lot<{0}>上机检查异常：设备<{1}>当前状态<{2}>,未Ready，拒绝生产.", lotInfo.LotID, GetCurrentEq.EQName, GetCurrentEq.EQStatus);
                                        BaseComm.ErrorHandleRule("E2111", ErrorMsg, ref errm);
                                        isCheckNg = true;
                                    }
                                }
                            }
                            else
                            {
                                string ErrorMsg = string.Format("Lot<{0}>上机检查异常：找不到设备ID<{1}>，拒绝生产.", lotInfo.LotID, lotInfo.MainEqpID);
                                BaseComm.ErrorHandleRule("E2111", ErrorMsg, ref errm);
                                isCheckNg = true;
                            }
                            if (isCheckNg == false)
                            {
                                isMainEqpIDFlow(lotInfo, port, em);
                            }

                        }
                        #endregion
                        else
                        {
                            EquipmentModel skipEm = new EquipmentModel();
                            //获取需要跳过的设备物件
                            if (!string.IsNullOrEmpty(lotInfo.SkipSubEqpID))
                            {
                                skipEm = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByEqID(lotInfo.SkipSubEqpID);
                            }

                            #region [确认设备是否Online]
                            //获取不是Online的设备
                            var eq1 = (from n in EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel()
                                       where n.ConnectMode == EAP.Model.eConnectMode.DISCONNECT || (n.ControlMode != eControlMode.REMOTE)
                                       select n).FirstOrDefault();
                            if (eq1 != null)
                            {
                                if (eq1.isCheckConnect && eq1.isCheckControlMode)
                                {
                                    string ErrorMsg = string.Format("Lot<{0}>上机检查异常：存在离线设备<{1}>，拒绝生产.", lotInfo.LotID, eq1.EQName);
                                    BaseComm.ErrorHandleRule("E2104", ErrorMsg, ref errm);
                                    isCheckNg = true;
                                }
                            }
                            #endregion

                            #region [确认设备状态:1.检查投板机是否正在投板；2.检查设备状态是否正常]
                            if (isCheckNg == false)
                            {
                                #region [多Load设备时确认设备状态，目前融铆合使用]
                                if (EAPEnvironment.commonLibrary.lineModel.isMultiLoadEquipment)
                                {
                                    //判断是否检查投板机正在投板状态
                                    if (EAPEnvironment.commonLibrary.lineModel.isCheckLoadStatus)
                                    {
                                        //获取正在生产的投板机
                                        var portStatus = (from q in em.List_Port.Values where q.PortID == ePortID.L02 && q.PortStatus == ePortStatus.LOADCOMPLETE select q).FirstOrDefault();
                                        if (portStatus != null)
                                        {
                                            return;

                                        }
                                    }
                                    //判断是否检查设备状态
                                    if (EAPEnvironment.commonLibrary.lineModel.isCheckEquipmentStatus)
                                    {
                                        if (isCheckNg == false)
                                        {
                                            if (em.EQStatus == eEQSts.Down)
                                            {
                                                string ErrorMsg = string.Format("Lot<{0}>上机检查异常：设备<{1}>机况<{2}>异常，拒绝生产.", lotInfo.LotID, em.EQName, em.EQStatus);
                                                BaseComm.ErrorHandleRule("E2110", ErrorMsg, ref errm);
                                                isCheckNg = true;
                                            }
                                        }
                                    }
                                }
                                #endregion
                                else
                                {
                                    #region [叠板压合确认设备状态]
                                    if (EAPEnvironment.commonLibrary.lineModel.LineType == "叠板压合")
                                    {
                                        //判断是否检查投板机正在投板状态
                                        if (EAPEnvironment.commonLibrary.lineModel.isCheckLoadStatus)
                                        {
                                            //获取正在生产的投板机
                                            var portStatus = (from q in em.List_Port.Values where q.PortID == ePortID.L02 && q.PortStatus == ePortStatus.LOADCOMPLETE select q).FirstOrDefault();
                                            if (portStatus != null)
                                            {
                                                return;

                                            }
                                        }

                                        //判断是否检查设备状态
                                        if (EAPEnvironment.commonLibrary.lineModel.isCheckEquipmentStatus)
                                        {
                                            if (isCheckNg == false)
                                            {
                                                if (em.EQStatus == eEQSts.Down)
                                                {
                                                    string ErrorMsg = string.Format("Lot<{0}>上机检查异常：设备<{1}>机况<{2}>异常，拒绝生产.", lotInfo.LotID, em.EQName, em.EQStatus);
                                                    BaseComm.ErrorHandleRule("E2110", ErrorMsg, ref errm);
                                                    isCheckNg = true;
                                                }
                                            }
                                        }
                                    }
                                    #endregion
                                    #region [冲孔连棕化设备状态确认]
                                    else if (EAPEnvironment.commonLibrary.lineModel.LineType == "冲孔连棕化")
                                    {
                                        //判断是否检查投板机正在投板状态
                                        if (EAPEnvironment.commonLibrary.lineModel.isCheckLoadStatus)
                                        {
                                            //获取正在生产的投板机
                                            var portStatus = (from q in em.List_Port.Values where q.PortID == ePortID.L02 && q.PortStatus == ePortStatus.LOADCOMPLETE select q).FirstOrDefault();
                                            //isDownloadInnerLot
                                            //获取正在跑的投板机
                                            var eq2 = (from n in EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel()
                                                       where (n.Type == eEquipmentType.L && portStatus != null)
                                                       select n).FirstOrDefault();
                                            if (eq2 != null)
                                            {
                                                string ErrorMsg = string.Format("Lot<{0}>上机检查异常：设备<{1}>投板机正在投板，拒绝生产.", lotInfo.LotID, eq2.EQName);
                                                BaseComm.ErrorHandleRule("E2112", ErrorMsg, ref errm);
                                                isCheckNg = true;
                                            }
                                        }

                                        if (isCheckNg == false)
                                        {
                                            //判断是否检查设备状态
                                            if (EAPEnvironment.commonLibrary.lineModel.isCheckEquipmentStatus)
                                            {
                                                var SubEqpIDList = (from q in lotInfo.InnerLotID select q.SubEqpID).ToList();
                                                foreach (var SubEqpID in SubEqpIDList)
                                                {
                                                    //获取和MES下载相同内层SubEqpID的侧向放板机状态Down的设备物件
                                                    var eq3 = (from n in EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel()
                                                               where n.EQStatus == eEQSts.Down && n.EQID == SubEqpID
                                                               select n).FirstOrDefault();
                                                    if (eq3 != null)
                                                    {
                                                        string ErrorMsg = string.Format("Lot<{0}>上机检查异常：设备<{1}>机况<{2}>异常，拒绝生产.", lotInfo.LotID, eq3.EQName, eq3.EQStatus);
                                                        BaseComm.ErrorHandleRule("E2110", ErrorMsg, ref errm);
                                                        isCheckNg = true;
                                                    }
                                                }

                                                //获取Down的设备
                                                var eq4 = (from n in EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel()
                                                           where n.EQStatus == eEQSts.Down && !n.isDownloadInnerLot
                                                           select n).FirstOrDefault();
                                                if (eq4 != null)
                                                {
                                                    string ErrorMsg = string.Format("Lot<{0}>上机检查异常：设备<{1}>机况<{2}>异常，拒绝生产.", lotInfo.LotID, eq4.EQName, eq4.EQStatus);
                                                    BaseComm.ErrorHandleRule("E2110", ErrorMsg, ref errm);
                                                    isCheckNg = true;
                                                }
                                            }
                                        }
                                    }
                                    #endregion
                                    else
                                    {
                                        //判断是否检查投板机正在投板状态
                                        if (EAPEnvironment.commonLibrary.lineModel.isCheckLoadStatus)
                                        {
                                            //获取正在生产的投板机
                                            var portStatus = (from q in em.List_Port.Values where q.PortID == ePortID.L02 && q.PortStatus == ePortStatus.LOADCOMPLETE select q).FirstOrDefault();
                                            //获取正在跑的投板机
                                            var eq2 = (from n in EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel()
                                                       where (n.Type == eEquipmentType.L && portStatus != null)
                                                       select n).FirstOrDefault();
                                            if (eq2 != null)
                                            {
                                                string ErrorMsg = string.Format("Lot<{0}>上机检查异常：设备<{1}>投板机正在投板，拒绝生产.", lotInfo.LotID, eq2.EQName);
                                                BaseComm.ErrorHandleRule("E2112", ErrorMsg, ref errm);
                                                isCheckNg = true;
                                            }
                                        }
                                        if (isCheckNg == false)
                                        {
                                            //判断是否检查设备状态
                                            if (EAPEnvironment.commonLibrary.lineModel.isCheckEquipmentStatus)
                                            {
                                                //获取Down的设备
                                                var eq3 = (from n in EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel()
                                                           where n.EQStatus == eEQSts.Down
                                                           select n).FirstOrDefault();
                                                if (eq3 != null)
                                                {
                                                    string ErrorMsg = string.Format("Lot<{0}>上机检查异常：设备<{1}>机况<{2}>异常，拒绝生产.", lotInfo.LotID, eq3.EQName, eq3.EQStatus);
                                                    BaseComm.ErrorHandleRule("E2110", ErrorMsg, ref errm);
                                                    isCheckNg = true;
                                                }
                                            }
                                        }
                                    }

                                }

                            }
                            #endregion

                            #region [检查有问题则下NG命令]
                            if (isCheckNg)
                            {
                                string ErrorMsg = string.Format("E0002:Lot[{0}]产品[{1}]数量[{2}]帐料参数检查失败.", lotInfo.LotID, lotInfo.PN, lotInfo.PanelTotalQty);
                                BaseComm.LogMsg(Log.LogLevel.Warn, ErrorMsg);

                                foreach (var item in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.isDisconnectionDataDownload))
                                {
                                    new HostService.HostService().CIMMessageCommand(item.EQID, "10", ErrorMsg, DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                                }
                                return;
                            }
                            #endregion

                            #region 关掉线程，如果清线检查失败，再次打开线程
                            Environment.EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckInitial();
                            EAPEnvironment.EAPAp.DeleteThreadWork(ConstLibrary.CONST_DLL_RULE_ProductionConditionCheck);
                            #endregion

                            #region [根据Lot进行清线，Lot不同时，全线清线【上PIN使用】]
                            if (EAPEnvironment.commonLibrary.lineModel.isAllrocessCompletionByLot)
                            {
                                #region 批次不同时，暂停一个节拍时间
                                int LotTime = EAPEnvironment.commonLibrary.customizedLibrary.ProductionConditonCheckLotTime * 1000;
                                if (!string.IsNullOrEmpty(Environment.EAPEnvironment.commonLibrary.OldLotID) && Environment.EAPEnvironment.commonLibrary.OldLotID != lotInfo.LotID)
                                {
                                    BaseComm.LogMsg(Log.LogLevel.Warn, string.Format("线体<{0}>下载Lot ID不一致，请等待<{1}s>节拍时间", lotInfo.MainEqpID, EAPEnvironment.commonLibrary.customizedLibrary.ProductionConditonCheckLotTime), false);
                                    Thread.Sleep(LotTime);
                                    Environment.EAPEnvironment.commonLibrary.OldLotID = lotInfo.LotID;
                                }
                                #endregion
                                isAllrocessCompletionByLotFlow(lotInfo, port, em);

                            }
                            else if (EAPEnvironment.commonLibrary.lineModel.isAllrocessCompletionByWorkOrder)
                            {
                                #region 工单不同时，暂停一个节拍时间
                                int WorkOrderTime = EAPEnvironment.commonLibrary.customizedLibrary.ProductionConditonCheckWorkOrderTime * 1000;
                                if (!string.IsNullOrEmpty(Environment.EAPEnvironment.commonLibrary.OldWorkOrder) && Environment.EAPEnvironment.commonLibrary.OldWorkOrder != lotInfo.WorkOrder)
                                {
                                    BaseComm.LogMsg(Log.LogLevel.Warn, string.Format("线体<{0}>下载工单不一致，请等待<{1}s>节拍时间", lotInfo.MainEqpID, EAPEnvironment.commonLibrary.customizedLibrary.ProductionConditonCheckWorkOrderTime), false);
                                    Thread.Sleep(WorkOrderTime);
                                    Environment.EAPEnvironment.commonLibrary.OldWorkOrder = lotInfo.WorkOrder;
                                }
                                #endregion
                                isAllrocessCompletionByWorkOrderFlow(lotInfo, port, em);
                            }
                            #endregion
                            else
                            {
                                #region [如果时同型号，直接下载生产任务给设备]
                                //判断是否同料号，并且是否为第一次进行生产条件检查
                                if (Environment.EAPEnvironment.commonLibrary.OldPN == lotInfo.PN && EAPEnvironment.commonLibrary.isNormalCheckData)
                                {
                                    #region 批次不同时，暂停一个节拍时间
                                    int LotTime = EAPEnvironment.commonLibrary.customizedLibrary.ProductionConditonCheckLotTime * 1000;
                                    if (Environment.EAPEnvironment.commonLibrary.OldLotID != lotInfo.LotID)
                                    {
                                        BaseComm.LogMsg(Log.LogLevel.Warn, string.Format("线体<{0}>下载Lot ID不一致，请等待<{1}s>节拍时间", lotInfo.MainEqpID, EAPEnvironment.commonLibrary.customizedLibrary.ProductionConditonCheckLotTime), false);
                                        Thread.Sleep(LotTime);
                                        Environment.EAPEnvironment.commonLibrary.OldLotID = lotInfo.LotID;
                                    }
                                    #endregion
                                    //线体多Load设备时设定  多投板机设备检查回复逻辑，目前融铆合使用
                                    if (EAPEnvironment.commonLibrary.lineModel.isMultiLoadEquipment)
                                    {
                                        isMultiLoadEquipmentFlow(lm, port, lotInfo, em);

                                    }
                                    else
                                    {
                                        isNormalEquipmentFlow(port, lotInfo, em, skipEm);

                                    }
                                }
                                #endregion
                                else
                                {
                                    #region [如果不同型号，则需要进行配方检查。 检查当前配方和配方参数是否和LotInfo内的一致]
                                    isRecipeParameterCheckFlow(isCheckNg, port, lotInfo, em, errm);

                                    #endregion
                                }
                                EAPEnvironment.commonLibrary.isNormalCheckData = true;
                            }
                        }
                    }
                }
                #region 关掉线程，如果清线检查失败，再次打开线程
                //Environment.EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckInitial();
                //EAPEnvironment.EAPAp.DeleteThreadWork(ConstLibrary.CONST_DLL_RULE_ProductionConditionCheck);
                #endregion

            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return;
            }

        }
        /// <summary>
        /// 前处理塞孔喷涂，非连线工序逻辑
        /// </summary>
        /// <param name="errm"></param>
        /// <param name="lotInfo"></param>
        /// <param name="port"></param>
        /// <param name="em"></param>
        public void DisconnectionFlow(ErrorCodeModelBase errm, MessageModel.LotInfo lotInfo, PortModel port, EquipmentModel em)
        {
            try
            {
                bool isCheckNg = false;
                #region [确认设备是否Online]
                //获取不是Online的设备
                var eq1 = (from n in EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel()
                           where n.ConnectMode == EAP.Model.eConnectMode.DISCONNECT || (n.ControlMode != eControlMode.REMOTE)
                           select n).FirstOrDefault();
                if (eq1 != null)
                {
                    if (eq1.isCheckConnect && eq1.isCheckControlMode)
                    {
                        string ErrorMsg = string.Format("Lot<{0}>上机检查异常：存在离线设备<{1}>，拒绝生产.", lotInfo.LotID, eq1.EQName);
                        BaseComm.ErrorHandleRule("E2104", ErrorMsg, ref errm);
                        isCheckNg = true;
                    }
                }
                #endregion

                #region[确认设备状态:1.检查投板机是否正在投板]
                //判断是否检查投板机正在投板状态
                if (EAPEnvironment.commonLibrary.lineModel.isCheckLoadStatus)
                {
                    //获取设备时投板机，并且正在跑的设备
                    var eq2 = (from n in EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel()
                               where (n.Type == eEquipmentType.L && n.EQStatus == eEQSts.Run)
                               select n).FirstOrDefault();
                    if (eq2 != null)
                    {
                        string ErrorMsg = string.Format("Lot<{0}>上机检查异常：设备<{1}>投板机正在投板，拒绝生产.", lotInfo.LotID, eq2.EQName);
                        BaseComm.ErrorHandleRule("E2112", ErrorMsg, ref errm);
                        isCheckNg = true;
                    }
                }
                //判断是否检查设备状态
                if (EAPEnvironment.commonLibrary.lineModel.isCheckEquipmentStatus)
                {
                    if (isCheckNg == false)
                    {
                        //获取非连线设备，并且设备状态是Down的设备
                        var eq3 = (from n in EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel()
                                   where n.EQStatus == eEQSts.Down && n.isDisconnectionDataDownload
                                   select n).FirstOrDefault();
                        if (eq3 != null)
                        {
                            string ErrorMsg = string.Format("Lot<{0}>上机检查异常：设备<{1}>机况<{2}>异常，拒绝生产.", lotInfo.LotID, eq3.EQName, eq3.EQStatus);
                            BaseComm.ErrorHandleRule("E2110", ErrorMsg, ref errm);
                            isCheckNg = true;
                        }
                    }
                }
                #endregion

                #region [检查有问题则下NG命令]
                if (isCheckNg)
                {
                    string ErrorMsg = string.Format("E0002:Lot[{0}]产品[{1}]数量[{2}]帐料参数检查失败.", lotInfo.LotID, lotInfo.PN, lotInfo.PanelTotalQty);
                    BaseComm.LogMsg(Log.LogLevel.Warn, ErrorMsg);

                    foreach (var item in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
                    {
                        new HostService.HostService().CIMMessageCommand(item.EQID, "10", ErrorMsg, DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                    }
                    return;
                }
                #endregion

                #region 关掉线程，如果清线检查失败，再次打开线程
                Environment.EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckInitial();
                EAPEnvironment.EAPAp.DeleteThreadWork(ConstLibrary.CONST_DLL_RULE_ProductionConditionCheck);
                #endregion

                #region [如果时同型号，直接下载生产任务给设备]
                //判断是否同料号，并且是否为第一次进行生产条件检查
                if (Environment.EAPEnvironment.commonLibrary.OldPN == lotInfo.PN && EAPEnvironment.commonLibrary.isNormalCheckData)
                {
                    #region 批次不同时，暂停一个节拍时间
                    int LotTime = EAPEnvironment.commonLibrary.customizedLibrary.ProductionConditonCheckLotTime * 1000;
                    if (Environment.EAPEnvironment.commonLibrary.OldLotID != lotInfo.LotID)
                    {
                        BaseComm.LogMsg(Log.LogLevel.Warn, string.Format("线体<{0}>下载Lot ID不一致，请等待<{1}s>节拍时间", lotInfo.MainEqpID, EAPEnvironment.commonLibrary.customizedLibrary.ProductionConditonCheckLotTime), false);
                        Thread.Sleep(LotTime);
                        Environment.EAPEnvironment.commonLibrary.OldLotID = lotInfo.LotID;
                    }
                    #endregion
                    if (!EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckStart)
                    {
                        //获取投板机设备物件
                        EAPEnvironment.commonLibrary.isTransferStatus = false;
                        foreach (var _v in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
                        {
                            #region[如果非连线工艺时，不设定下载设备，不下载生产任务给设备]
                            if (_v.isDisconnectionDataDownload)
                            {
                                EAPEnvironment.commonLibrary.isTransferStatus = true;//开启传送功能
                            }
                            else
                            {
                                continue;
                            }
                            #endregion

                            List<MessageModel.Param> parameterModel;
                            //是否下载参数信息给设备
                            if (_v.isRecipeParameterDownload)
                            {
                                parameterModel = _v.NewEQParameter = Environment.EAPEnvironment.commonLibrary.commonModel.GetParameterModel(_v, port.List_Lot.Where(r => r.Key == lotInfo.LotID).FirstOrDefault().Value);
                            }
                            else
                            {
                                parameterModel = new List<MessageModel.Param>();
                            }
                            //下载生产任务
                            new HostService.HostService().JobDataDownload(_v, port.List_Lot.Where(r => r.Key == lotInfo.LotID).FirstOrDefault().Value, port.List_Lot.Where(r => r.Key == lotInfo.LotID).FirstOrDefault().Value.LocalPortStation, parameterModel);
                        }
                        Dictionary<EquipmentModel, LotModel> el = new Dictionary<EquipmentModel, LotModel>();
                        el.Add(em, port.List_Lot.Where(r => r.Key == lotInfo.LotID).FirstOrDefault().Value);
                        //开启生产任务下载处理线程
                        if (EAPEnvironment.EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_JobDataDownload, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_JobDataDownload, 4000, true, el))
                        {
                            EAPEnvironment.commonLibrary.isProcessOK = true;
                            BaseComm.LogMsg(Log.LogLevel.Info, string.Format("Lot<{0}>确认设备数据中......", lotInfo.LotID));
                            EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckStart = true;
                            EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckTime = DateTime.Now;
                        }
                    }
                }
                #endregion
                else
                {
                    #region [检查当前配方和配方参数是否和LotInfo内的一致]

                    bool isPPIDSame = true;
                    bool isCheckAllProcessCompletion = true;
                    if (string.IsNullOrEmpty(Environment.EAPEnvironment.commonLibrary.OldPN) || string.IsNullOrEmpty(Environment.EAPEnvironment.commonLibrary.OldLotID))
                    {
                        Environment.EAPEnvironment.commonLibrary.OldPN = lotInfo.PN;
                        Environment.EAPEnvironment.commonLibrary.OldLotID = lotInfo.LotID;
                    }
                    else
                    {
                        //Load Complete次数检查。目前冲孔连棕化，PP裁切机使用
                        if (EAPEnvironment.commonLibrary.lineModel.isLoadCompleteCountCheck)
                        {
                            EAPEnvironment.commonLibrary.commonModel.LoadCompleteCount.Clear();
                        }
                        //抓取设定节拍时间
                        int PNTime = EAPEnvironment.commonLibrary.customizedLibrary.ProductionConditonCheckPNTime * 1000;

                        //料号不同时，暂停一个节拍时间
                        if (Environment.EAPEnvironment.commonLibrary.OldPN != lotInfo.PN)
                        {
                            BaseComm.LogMsg(Log.LogLevel.Warn, string.Format("线体<{0}>下载PN不一致，请等待<{1}s>节拍时间", lotInfo.MainEqpID, EAPEnvironment.commonLibrary.customizedLibrary.ProductionConditonCheckPNTime), false);
                            Thread.Sleep(PNTime);
                            Environment.EAPEnvironment.commonLibrary.OldPN = lotInfo.PN;
                        }
                        if (isCheckNg == false)
                        {
                            foreach (var vem in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.isDisconnectionDataDownload))
                            {
                                if (vem.OldEQParameter.Count == 0)
                                {
                                    //如果设备没有参数，不做处理
                                }
                                else
                                {
                                    if (vem.isCheckRecipeParameter)//是否检查配方参数
                                    {
                                        foreach (var item in vem.OldEQParameter)
                                        {
                                            //获取相同参数名称的参数列表
                                            MessageModel.Param Para = lotInfo.ParamList.Where(r => r.SubEqpID == vem.EQID && r.ParamName == item.ParamName).FirstOrDefault();
                                            if (Para == null) continue;
                                            //检查参数是否相同
                                            if (item.ParamValue != Para.ParamValue)
                                            {
                                                string ErrorMsg = string.Format("Lot<{0}>上机检查异常：线体生产中，设备<{1}> 参数名称<{2}>Value<{3}>与Lot中的参数名称<{4}>Value<{5}>不一致，检查清线.",
                                                        lotInfo.LotID, vem.EQName, item.ParamName, item.ParamValue, Para.ParamName, Para.ParamValue);
                                                BaseComm.ErrorHandleRule("E2115", ErrorMsg, ref errm);
                                                isCheckNg = true;

                                                if (!vem.isProcessCompletion)
                                                {
                                                    vem.isRecipeChange = true;
                                                    isPPIDSame = false;
                                                }
                                                if (vem.isAllrocessCompletion)
                                                {
                                                    isCheckAllProcessCompletion = false;
                                                }
                                            }
                                        }
                                    }

                                }
                            }
                        }
                    }
                    #endregion

                    #region  下货回复检查，清线检查
                    if (!EquipmentJobDataRequestCheck(isPPIDSame, lotInfo, port, isCheckAllProcessCompletion, em))
                    {
                        BaseComm.LogMsg(Log.LogLevel.Warn, string.Format("线体<{0}>未能清线", lotInfo.MainEqpID), false);
                    }
                    #endregion
                }
                EAPEnvironment.commonLibrary.isNormalCheckData = true;
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return;
            }
        }
        /// <summary>
        /// [一台设备是一个Main设备，机械钻机使用
        /// </summary>
        /// <param name="lotInfo"></param>
        /// <param name="port"></param>
        /// <param name="em"></param>
        public void isMainEqpIDFlow(MessageModel.LotInfo lotInfo, PortModel port, EquipmentModel em)
        {
            try
            {
                if (!EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckStart)
                {
                    var MainEq = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.EQID == lotInfo.MainEqpID).FirstOrDefault();
                    List<MessageModel.Param> parameterModel;
                    //是否下载参数信息给设备
                    if (MainEq.isRecipeParameterDownload)
                    {
                        parameterModel = MainEq.NewEQParameter = Environment.EAPEnvironment.commonLibrary.commonModel.GetParameterModel(MainEq, port.List_Lot.Where(r => r.Key == lotInfo.LotID).FirstOrDefault().Value);
                    }
                    else
                    {
                        parameterModel = new List<MessageModel.Param>();
                    }
                    //下载生产任务
                    new HostService.HostService().JobDataDownload(MainEq, port.List_Lot.Where(r => r.Key == lotInfo.LotID).FirstOrDefault().Value, port.List_Lot.Where(r => r.Key == lotInfo.LotID).FirstOrDefault().Value.LocalPortStation, parameterModel);

                    Dictionary<EquipmentModel, LotModel> el = new Dictionary<EquipmentModel, LotModel>();
                    el.Add(em, port.List_Lot.Where(r => r.Key == lotInfo.LotID).FirstOrDefault().Value);
                    //开启生产任务下载处理线程
                    if (EAPEnvironment.EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_JobDataDownload, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_JobDataDownload, 4000, true, el))
                    {
                        EAPEnvironment.commonLibrary.isProcessOK = true;
                        BaseComm.LogMsg(Log.LogLevel.Info, string.Format("Lot<{0}>确认设备数据中......", lotInfo.LotID));
                        EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckStart = true;
                        EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckTime = DateTime.Now;
                    }
                }

                #region 关掉线程，如果清线检查失败，再次打开线程
                Environment.EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckInitial();
                EAPEnvironment.EAPAp.DeleteThreadWork(ConstLibrary.CONST_DLL_RULE_ProductionConditionCheck);
                #endregion
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return;
            }
        }
        /// <summary>
        /// 根据Lot进行清线，Lot不同时，全线清线【上PIN使用】
        /// </summary>
        /// <param name="lotInfo"></param>
        /// <param name="port"></param>
        /// <param name="em"></param>
        public void isAllrocessCompletionByLotFlow(MessageModel.LotInfo lotInfo, PortModel port, EquipmentModel em)
        {
            try
            {
                #region [检查当前配方和配方参数是否和LotInfo内的一致]
                bool isPPIDSame = true;
                bool isCheckAllProcessCompletion = false;

                #endregion
                #region  下货回复检查，清线检查
                if (!EquipmentJobDataRequestCheck(isPPIDSame, lotInfo, port, isCheckAllProcessCompletion, em))
                {
                    BaseComm.LogMsg(Log.LogLevel.Warn, string.Format("线体<{0}>未能清线", lotInfo.MainEqpID), false);
                }
                #endregion
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return;
            }
        }

        /// <summary>
        /// 根据工单进行清线，工单不同时，清线
        /// </summary>
        /// <param name="lotInfo"></param>
        /// <param name="port"></param>
        /// <param name="em"></param>
        public void isAllrocessCompletionByWorkOrderFlow(MessageModel.LotInfo lotInfo, PortModel port, EquipmentModel em)
        {
            try
            {
                #region [检查当前配方和配方参数是否和LotInfo内的一致]
                bool isPPIDSame = false;
                bool isCheckAllProcessCompletion = true;

                #endregion
                #region  下货回复检查，清线检查
                if (!EquipmentJobDataRequestCheck(isPPIDSame, lotInfo, port, isCheckAllProcessCompletion, em))
                {
                    BaseComm.LogMsg(Log.LogLevel.Warn, string.Format("线体<{0}>未能清线", lotInfo.MainEqpID), false);
                }
                #endregion
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return;
            }
        }


        /// <summary>
        /// 线体多Load设备时设定  多投板机设备检查回复逻辑，目前融铆合使用
        /// </summary>
        /// <param name="lm"></param>
        /// <param name="port"></param>
        /// <param name="lotInfo"></param>
        /// <param name="em"></param>
        public void isMultiLoadEquipmentFlow(LotModel lm, PortModel port, MessageModel.LotInfo lotInfo, EquipmentModel em)
        {
            try
            {
                //用批次信息查找设备物件
                EquipmentModel DownLoadEn = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByLotID(lm.LotID);
                if (DownLoadEn != null)
                {
                    List<MessageModel.Param> parameterModel;
                    //是否下载参数信息给设备
                    if (DownLoadEn.isRecipeParameterDownload)
                    {
                        parameterModel = DownLoadEn.NewEQParameter = Environment.EAPEnvironment.commonLibrary.commonModel.GetParameterModel(DownLoadEn, port.List_Lot.Where(r => r.Key == lotInfo.LotID).FirstOrDefault().Value);
                    }
                    else
                    {
                        parameterModel = new List<MessageModel.Param>();
                    }
                    //下载生产任务
                    new HostService.HostService().JobDataDownload(DownLoadEn, port.List_Lot.Where(r => r.Key == lotInfo.LotID).FirstOrDefault().Value, port.List_Lot.Where(r => r.Key == lotInfo.LotID).FirstOrDefault().Value.LocalPortStation, parameterModel);
                    //开启生产任务下载处理线程
                    if (!EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckStart)
                    {
                        Dictionary<EquipmentModel, LotModel> el = new Dictionary<EquipmentModel, LotModel>();
                        el.Add(em, port.List_Lot.Where(r => r.Key == lotInfo.LotID).FirstOrDefault().Value);
                        if (EAPEnvironment.EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_JobDataDownload, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_JobDataDownload, 4000, true, el))
                        {
                            EAPEnvironment.commonLibrary.isProcessOK = true;
                            BaseComm.LogMsg(Log.LogLevel.Info, string.Format("Lot<{0}>确认设备数据中......", lotInfo.LotID));
                            EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckStart = true;
                            EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckTime = DateTime.Now;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return;
            }
        }
        /// <summary>
        /// Normal设备检查回复逻辑
        /// </summary>
        /// <param name="port"></param>
        /// <param name="lotInfo"></param>
        /// <param name="em"></param>
        /// <param name="skipEm"></param>
        public void isNormalEquipmentFlow(PortModel port, MessageModel.LotInfo lotInfo, EquipmentModel em, EquipmentModel skipEm)
        {
            try
            {
                if (!EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckStart)
                {
                    //获取投板机设备物件
                    var LoadEquipment = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.Type == eEquipmentType.L).ToList();
                    EAPEnvironment.commonLibrary.isTransferStatus = false;
                    foreach (var _v in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
                    {
                        #region[如果是isMaterialLoadComplete设定请求物料Lot信息，不下载生产任务给设备]
                        if (_v.isMaterialLoadComplete)
                        {
                            continue;
                        }
                        #endregion
                        #region [如果设备是扫描底盘下载生产任务]
                        if (_v.isCarrierDownloadData)
                        {
                            continue;
                        }
                        #endregion
                        #region[如果设备是投板机，投板机数量大于1，但是和当前请求的设备ID不一致，不下载生产任务。避免投板机生产任务重复下发]【叠板压合】
                        if (LoadEquipment.Count > 1)
                        {
                            if (em.EQID != _v.EQID)
                            {
                                if (_v.Type == eEquipmentType.L)
                                {
                                    _v.isMultiLoad = true;//不同设备ID，是投板机设备时，多投板机Flag为True
                                    continue;
                                }
                                else
                                    _v.isMultiLoad = false;//不同设备ID，不是投板机设备时，多投板机Flag为False
                            }
                            else
                                _v.isMultiLoad = false;//相同设备时，多投板机Flag为False
                        }
                        #endregion

                        #region[如果SkipSubEqpID有值，不下载生产任务给后面设备]
                        if (skipEm.EQID != null)
                        {
                            EAPEnvironment.commonLibrary.isTransferStatus = true;//开启传送功能
                            if (_v.EQNo >= skipEm.EQNo)
                            {
                                _v.isSkipEquipment = true;//如果设备No>跳过的设备No，跳过Flag为True
                                continue;
                            }
                            else _v.isSkipEquipment = false;
                        }
                        else _v.isSkipEquipment = false;
                        #endregion

                        List<MessageModel.Param> parameterModel;
                        //是否下载参数信息给设备
                        if (_v.isRecipeParameterDownload)
                        {
                            parameterModel = _v.NewEQParameter = Environment.EAPEnvironment.commonLibrary.commonModel.GetParameterModel(_v, port.List_Lot.Where(r => r.Key == lotInfo.LotID).FirstOrDefault().Value);
                        }
                        else
                        {
                            parameterModel = new List<MessageModel.Param>();
                        }
                        //下载生产任务
                        new HostService.HostService().JobDataDownload(_v, port.List_Lot.Where(r => r.Key == lotInfo.LotID).FirstOrDefault().Value, port.List_Lot.Where(r => r.Key == lotInfo.LotID).FirstOrDefault().Value.LocalPortStation, parameterModel);
                    }
                    Dictionary<EquipmentModel, LotModel> el = new Dictionary<EquipmentModel, LotModel>();
                    el.Add(em, port.List_Lot.Where(r => r.Key == lotInfo.LotID).FirstOrDefault().Value);
                    //开启生产任务下载处理线程
                    if (EAPEnvironment.EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_JobDataDownload, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_JobDataDownload, 4000, true, el))
                    {
                        EAPEnvironment.commonLibrary.isProcessOK = true;
                        BaseComm.LogMsg(Log.LogLevel.Info, string.Format("Lot<{0}>确认设备数据中......", lotInfo.LotID));
                        EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckStart = true;
                        EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckTime = DateTime.Now;
                    }
                }
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return;
            }
        }
        /// <summary>
        /// MES下载正常处理逻辑
        /// </summary>
        /// <param name="port"></param>
        /// <param name="lotInfo"></param>
        /// <param name="em"></param>
        /// <param name="startEq"></param>
        public void isMESDownloadNormalEquipmentFlow(PortModel port, MessageModel.LotInfo lotInfo, EquipmentModel em, EquipmentModel startEq)
        {
            try
            {
                if (!EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckStart)
                {
                    //获取投板机设备物件
                    EAPEnvironment.commonLibrary.isTransferStatus = false;
                    foreach (var _v in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.EQNo >= startEq.EQNo))
                    {

                        List<MessageModel.Param> parameterModel;
                        //是否下载参数信息给设备
                        if (_v.isRecipeParameterDownload)
                        {
                            parameterModel = _v.NewEQParameter = Environment.EAPEnvironment.commonLibrary.commonModel.GetParameterModel(_v, port.List_Lot.Where(r => r.Key == lotInfo.LotID).FirstOrDefault().Value);
                        }
                        else
                        {
                            parameterModel = new List<MessageModel.Param>();
                        }
                        //下载生产任务
                        new HostService.HostService().JobDataDownload(_v, port.List_Lot.Where(r => r.Key == lotInfo.LotID).FirstOrDefault().Value, port.List_Lot.Where(r => r.Key == lotInfo.LotID).FirstOrDefault().Value.LocalPortStation, parameterModel);
                    }
                    Dictionary<EquipmentModel, LotModel> el = new Dictionary<EquipmentModel, LotModel>();
                    el.Add(em, port.List_Lot.Where(r => r.Key == lotInfo.LotID).FirstOrDefault().Value);
                    //开启生产任务下载处理线程
                    if (EAPEnvironment.EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_JobDataDownload, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_JobDataDownload, 4000, true, el))
                    {
                        EAPEnvironment.commonLibrary.isProcessOK = true;
                        BaseComm.LogMsg(Log.LogLevel.Info, string.Format("Lot<{0}>确认设备数据中......", lotInfo.LotID));
                        EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckStart = true;
                        EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckTime = DateTime.Now;
                    }
                }
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return;
            }
        }
        /// <summary>
        /// 如果不同型号，则需要进行配方检查。 检查当前配方和配方参数是否和LotInfo内的一致
        /// </summary>
        /// <param name="isCheckNg"></param>
        /// <param name="port"></param>
        /// <param name="lotInfo"></param>
        /// <param name="em"></param>
        /// <param name="errm"></param>
        public void isRecipeParameterCheckFlow(bool isCheckNg, PortModel port, MessageModel.LotInfo lotInfo, EquipmentModel em, ErrorCodeModelBase errm)
        {
            try
            {
                bool isPPIDSame = true;
                bool isCheckAllProcessCompletion = true;
                if (string.IsNullOrEmpty(Environment.EAPEnvironment.commonLibrary.OldPN) || string.IsNullOrEmpty(Environment.EAPEnvironment.commonLibrary.OldLotID))
                {
                    Environment.EAPEnvironment.commonLibrary.OldPN = lotInfo.PN;
                    Environment.EAPEnvironment.commonLibrary.OldLotID = lotInfo.LotID;
                }
                else
                {
                    //Load Complete次数检查。目前冲孔连棕化，PP裁切机使用
                    if (EAPEnvironment.commonLibrary.lineModel.isLoadCompleteCountCheck)
                    {
                        EAPEnvironment.commonLibrary.commonModel.LoadCompleteCount.Clear();
                    }
                    //抓取设定节拍时间
                    int PNTime = EAPEnvironment.commonLibrary.customizedLibrary.ProductionConditonCheckPNTime * 1000;

                    //料号不同时，暂停一个节拍时间
                    if (Environment.EAPEnvironment.commonLibrary.OldPN != lotInfo.PN)
                    {
                        BaseComm.LogMsg(Log.LogLevel.Warn, string.Format("线体<{0}>下载PN不一致，请等待<{1}s>节拍时间", lotInfo.MainEqpID, EAPEnvironment.commonLibrary.customizedLibrary.ProductionConditonCheckPNTime), false);
                        Thread.Sleep(PNTime);
                        Environment.EAPEnvironment.commonLibrary.OldPN = lotInfo.PN;
                    }

                    if (isCheckNg == false)
                    {
                        foreach (var vem in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
                        {
                            if (vem.OldEQParameter.Count == 0)
                            {
                                //如果设备没有参数，不做处理
                            }
                            else
                            {
                                if (vem.isCheckRecipeParameter)//是否检查配方参数
                                {
                                    foreach (var item in vem.OldEQParameter)
                                    {
                                        //获取相同参数名称的参数列表
                                        MessageModel.Param Para = lotInfo.ParamList.Where(r => r.SubEqpID == vem.EQID && r.ParamName == item.ParamName).FirstOrDefault();
                                        if (Para == null) continue;
                                        //检查参数是否相同
                                        if (item.ParamValue != Para.ParamValue)
                                        {
                                            string ErrorMsg = string.Format("Lot<{0}>上机检查异常：线体生产中，设备<{1}> 参数名称<{2}>Value<{3}>与Lot中的参数名称<{4}>Value<{5}>不一致，检查清线.",
                                                    lotInfo.LotID, vem.EQName, item.ParamName, item.ParamValue, Para.ParamName, Para.ParamValue);
                                            BaseComm.ErrorHandleRule("E2115", ErrorMsg, ref errm);
                                            isCheckNg = true;

                                            if (!vem.isProcessCompletion)
                                            {
                                                vem.isRecipeChange = true;
                                                isPPIDSame = false;
                                            }
                                            if (vem.isAllrocessCompletion)
                                            {
                                                isCheckAllProcessCompletion = false;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                #region  下货回复检查，清线检查
                if (!EquipmentJobDataRequestCheck(isPPIDSame, lotInfo, port, isCheckAllProcessCompletion, em))
                {
                    BaseComm.LogMsg(Log.LogLevel.Warn, string.Format("线体<{0}>未能清线", lotInfo.MainEqpID), false);
                }
                #endregion
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return;
            }
        }
        /// <summary>
        /// 如果不同型号，则需要进行配方检查。 检查当前配方和配方参数是否和LotInfo内的一致
        /// </summary>
        /// <param name="isCheckNg"></param>
        /// <param name="port"></param>
        /// <param name="lotInfo"></param>
        /// <param name="em"></param>
        /// <param name="errm"></param>
        /// <param name="startEq"></param>
        public void isMESDownloadRecipeParameterCheckFlow(bool isCheckNg, PortModel port, MessageModel.LotInfo lotInfo, EquipmentModel em, ErrorCodeModelBase errm, EquipmentModel startEq)
        {
            try
            {
                bool isPPIDSame = true;
                bool isCheckAllProcessCompletion = true;
                if (string.IsNullOrEmpty(Environment.EAPEnvironment.commonLibrary.OldPN) || string.IsNullOrEmpty(Environment.EAPEnvironment.commonLibrary.OldLotID))
                {
                    Environment.EAPEnvironment.commonLibrary.OldPN = lotInfo.PN;
                    Environment.EAPEnvironment.commonLibrary.OldLotID = lotInfo.LotID;
                }
                else
                {
                    //抓取设定节拍时间
                    int PNTime = EAPEnvironment.commonLibrary.customizedLibrary.ProductionConditonCheckPNTime * 1000;

                    //料号不同时，暂停一个节拍时间
                    if (Environment.EAPEnvironment.commonLibrary.OldPN != lotInfo.PN)
                    {
                        BaseComm.LogMsg(Log.LogLevel.Warn, string.Format("线体<{0}>下载PN不一致，请等待<{1}s>节拍时间", lotInfo.MainEqpID, EAPEnvironment.commonLibrary.customizedLibrary.ProductionConditonCheckPNTime), false);
                        Thread.Sleep(PNTime);
                        Environment.EAPEnvironment.commonLibrary.OldPN = lotInfo.PN;
                    }

                    if (isCheckNg == false)
                    {
                        foreach (var vem in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.EQNo >= startEq.EQNo))
                        {
                            if (vem.OldEQParameter.Count == 0)
                            {
                                //如果设备没有参数，不做处理
                            }
                            else
                            {
                                if (vem.isCheckRecipeParameter)//是否检查配方参数
                                {
                                    foreach (var item in vem.OldEQParameter)
                                    {
                                        //获取相同参数名称的参数列表
                                        MessageModel.Param Para = lotInfo.ParamList.Where(r => r.SubEqpID == vem.EQID && r.ParamName == item.ParamName).FirstOrDefault();
                                        if (Para == null) continue;
                                        //检查参数是否相同
                                        if (item.ParamValue != Para.ParamValue)
                                        {
                                            string ErrorMsg = string.Format("Lot<{0}>上机检查异常：线体生产中，设备<{1}> 参数名称<{2}>Value<{3}>与Lot中的参数名称<{4}>Value<{5}>不一致，检查清线.",
                                                    lotInfo.LotID, vem.EQName, item.ParamName, item.ParamValue, Para.ParamName, Para.ParamValue);
                                            BaseComm.ErrorHandleRule("E2115", ErrorMsg, ref errm);
                                            isCheckNg = true;

                                            if (!vem.isProcessCompletion)
                                            {
                                                vem.isRecipeChange = true;
                                                isPPIDSame = false;
                                            }
                                            if (vem.isAllrocessCompletion)
                                            {
                                                isCheckAllProcessCompletion = false;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                #region  下货回复检查，清线检查
                if (!MESDownloadJobDataRequestCheck(isPPIDSame, lotInfo, port, isCheckAllProcessCompletion, em, startEq))
                {
                    BaseComm.LogMsg(Log.LogLevel.Warn, string.Format("线体<{0}>未能清线", lotInfo.MainEqpID), false);
                }
                #endregion
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return;
            }
        }
        /// <summary>
        /// MES下载生产任务，有开始设备设定逻辑
        /// </summary>
        /// <param name="lotInfo"></param>
        /// <param name="em"></param>
        /// <param name="port"></param>
        /// <param name="errm"></param>
        public void isMESDownloadData(MessageModel.LotInfo lotInfo, EquipmentModel em, PortModel port, ErrorCodeModelBase errm)
        {
            try
            {
                EquipmentModel startEq = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByEqID(EAPEnvironment.commonLibrary.lineModel.StartEquipmentID);
                if (startEq == null)
                {
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<isMESDownloadData> Find Error", em.EQID));
                    return;
                }

                bool isCheckNg = false;

                #region [确认设备是否Online]
                //获取不是Online的设备
                var eq1 = (from n in EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel()
                           where (n.ConnectMode == EAP.Model.eConnectMode.DISCONNECT || (n.ControlMode != eControlMode.REMOTE))
                           && n.EQNo >= startEq.EQNo
                           select n).FirstOrDefault();
                if (eq1 != null)
                {
                    if (eq1.isCheckConnect && eq1.isCheckControlMode)
                    {
                        string ErrorMsg = string.Format("Lot<{0}>上机检查异常：存在离线设备<{1}>，拒绝生产.", lotInfo.LotID, eq1.EQName);
                        BaseComm.ErrorHandleRule("E2104", ErrorMsg, ref errm);
                        isCheckNg = true;
                    }
                }
                #endregion

                #region [确认设备状态:1.检查投板机是否正在投板；2.检查设备状态是否正常]

                if (isCheckNg == false)
                {
                    //判断是否检查投板机正在投板状态
                    if (EAPEnvironment.commonLibrary.lineModel.isCheckLoadStatus)
                    {
                        var eq2 = (from n in EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel()
                                   where (n.EQID == EAPEnvironment.commonLibrary.lineModel.StartEquipmentID && n.EQStatus == eEQSts.Run)
                                   select n).FirstOrDefault();
                        if (eq2 != null)
                        {
                            string ErrorMsg = string.Format("Lot<{0}>上机检查异常：设备<{1}>投板机正在投板，拒绝生产.", lotInfo.LotID, eq2.EQName);
                            BaseComm.ErrorHandleRule("E2112", ErrorMsg, ref errm);
                            isCheckNg = true;
                        }
                    }
                    //判断是否检查设备状态
                    if (EAPEnvironment.commonLibrary.lineModel.isCheckEquipmentStatus)
                    {
                        if (isCheckNg == false)
                        {
                            var eq3 = (from n in EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel()
                                       where n.EQStatus == eEQSts.Down && n.EQNo >= startEq.EQNo
                                       select n).FirstOrDefault();
                            if (eq3 != null)
                            {
                                string ErrorMsg = string.Format("Lot<{0}>上机检查异常：设备<{1}>机况<{2}>异常，拒绝生产.", lotInfo.LotID, eq3.EQName, eq3.EQStatus);
                                BaseComm.ErrorHandleRule("E2110", ErrorMsg, ref errm);
                                isCheckNg = true;
                            }
                        }
                    }
                }
                #endregion

                #region [检查有问题则下NG命令]
                if (isCheckNg)
                {
                    string ErrorMsg = string.Format("E0002:Lot[{0}]产品[{1}]数量[{2}]帐料参数检查失败.", lotInfo.LotID, lotInfo.PN, lotInfo.PanelTotalQty);
                    BaseComm.LogMsg(Log.LogLevel.Warn, ErrorMsg);

                    foreach (var item in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.EQNo >= startEq.EQNo))
                    {
                        new HostService.HostService().CIMMessageCommand(item.EQID, "10", ErrorMsg, DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                    }
                    return;
                }
                #endregion

                #region 关掉线程，如果清线检查失败，再次打开线程
                Environment.EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckInitial();
                EAPEnvironment.EAPAp.DeleteThreadWork(ConstLibrary.CONST_DLL_RULE_ProductionConditionCheck);
                #endregion


                #region [如果时同型号，直接下载生产任务给设备]
                //判断是否同料号，并且是否为第一次进行生产条件检查
                if (Environment.EAPEnvironment.commonLibrary.OldPN == lotInfo.PN && EAPEnvironment.commonLibrary.isNormalCheckData)
                {
                    #region 批次不同时，暂停一个节拍时间
                    int LotTime = EAPEnvironment.commonLibrary.customizedLibrary.ProductionConditonCheckLotTime * 1000;
                    if (Environment.EAPEnvironment.commonLibrary.OldLotID != lotInfo.LotID)
                    {
                        BaseComm.LogMsg(Log.LogLevel.Warn, string.Format("线体<{0}>下载Lot ID不一致，请等待<{1}s>节拍时间", lotInfo.MainEqpID, EAPEnvironment.commonLibrary.customizedLibrary.ProductionConditonCheckLotTime), false);
                        Thread.Sleep(LotTime);
                        Environment.EAPEnvironment.commonLibrary.OldLotID = lotInfo.LotID;
                    }
                    #endregion
                    isMESDownloadNormalEquipmentFlow(port, lotInfo, em, startEq);
                }
                #endregion
                else
                {
                    #region [如果不同型号，则需要进行配方检查。 检查当前配方和配方参数是否和LotInfo内的一致]
                    isMESDownloadRecipeParameterCheckFlow(isCheckNg, port, lotInfo, em, errm, startEq);

                    #endregion
                }
                EAPEnvironment.commonLibrary.isNormalCheckData = true;
            }
            catch (Exception ex)
            {

            }
        }
        /// <summary>
        /// 下货回复检查，清线检查
        /// </summary>
        /// <param name="isPPIDSame"></param>
        /// <param name="lotInfo"></param>
        /// <param name="port"></param>
        /// <param name="isCheckAllProcessCompletion"></param>
        /// <param name="em"></param>
        /// <returns></returns>
        private bool EquipmentJobDataRequestCheck(bool isPPIDSame, MessageModel.LotInfo lotInfo, PortModel port, bool isCheckAllProcessCompletion, EquipmentModel em)
        {
            try
            {
                #region [检查清线]
                var lot = port.List_Lot.Where(r => r.Key == lotInfo.LotID).FirstOrDefault();
                Dictionary<MessageModel.LotInfo, CommModel> Dic_Message = new Dictionary<MessageModel.LotInfo, CommModel>();
                CommModel cm = new CommModel();
                EquipmentModel skipEm = new EquipmentModel();
                if (!string.IsNullOrEmpty(lotInfo.SkipSubEqpID))
                {
                    skipEm = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByEqID(lotInfo.SkipSubEqpID);
                }

                cm.pm = port;
                cm.lm = lot.Value;
                //cm.em = em;
                Dic_Message.Add(lotInfo, cm);

                #region 清整线
                if (!isCheckAllProcessCompletion)
                {
                    if (!EAPEnvironment.commonLibrary.commonModel.InAllProcessCompletionCheckStart)
                    {
                        if (EAPEnvironment.EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_AllProcessCompletionCheck, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_AllProcessCompletionCheck, 5000, true, Dic_Message))
                        {
                            EAPEnvironment.commonLibrary.isProcessOK = false;
                            BaseComm.LogMsg(Log.LogLevel.Info, string.Format("线体<{0}>确认清线......", EAPEnvironment.commonLibrary.LineName));
                            EAPEnvironment.commonLibrary.commonModel.InAllProcessCompletionCheckStart = true;
                            EAPEnvironment.commonLibrary.commonModel.InAllProcessCompletionCheckTime = DateTime.Now;
                        }
                    }
                }
                #endregion

                #region 清单机
                else
                {
                    if (!isPPIDSame)
                    {
                        if (!EAPEnvironment.commonLibrary.commonModel.InEquipmentJobDataRequestCheckStart)
                        {
                            if (EAPEnvironment.EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_EquipmentJobDataRequest, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_EquipmentJobDataRequest, 5000, true, Dic_Message))
                            {
                                EAPEnvironment.commonLibrary.isProcessOK = false;
                                BaseComm.LogMsg(Log.LogLevel.Info, string.Format("线体<{0}>确认设备清线......", EAPEnvironment.commonLibrary.LineName));
                                EAPEnvironment.commonLibrary.commonModel.InEquipmentJobDataRequestCheckStart = true;
                                EAPEnvironment.commonLibrary.commonModel.InEquipmentJobDataRequestCheckTime = DateTime.Now;
                            }
                        }
                    }
                    #endregion
                    #region [配方/配方参数相同，不需清线，直接下载生产任务]
                    else
                    {
                        if (EAPEnvironment.commonLibrary.lineModel.isMultiLoadEquipment)
                        {
                            if (!EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckStart)
                            {
                                EquipmentModel DownLoadEn = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByLotID(lot.Value.LotID);
                                if (DownLoadEn != null)
                                {
                                    DownLoadEn.NewEQParameter = Environment.EAPEnvironment.commonLibrary.commonModel.GetParameterModel(DownLoadEn, lot.Value);
                                    //下载生产任务
                                    new HostService.HostService().JobDataDownload(DownLoadEn, lot.Value, "", DownLoadEn.NewEQParameter);
                                    Dictionary<EquipmentModel, LotModel> el = new Dictionary<EquipmentModel, LotModel>();
                                    el.Add(em, lot.Value);
                                    //开启生产任务下载处理线程
                                    if (EAPEnvironment.EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_JobDataDownload, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_JobDataDownload, 4000, true, el))
                                    {
                                        EAPEnvironment.commonLibrary.isProcessOK = true;
                                        BaseComm.LogMsg(Log.LogLevel.Info, string.Format("LotID<{0}>确认设备数据中......", lot.Value.LotID));
                                        EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckStart = true;
                                        EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckTime = DateTime.Now;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (!EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckStart)
                            {
                                EAPEnvironment.commonLibrary.isTransferStatus = false;
                                var LoadEquipment = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.Type == eEquipmentType.L).ToList();
                                foreach (var _v in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
                                {
                                    #region[如果是isMaterialLoadComplete设定请求物料Lot信息，不下载生产任务给设备]
                                    if (_v.isMaterialLoadComplete)
                                    {
                                        continue;
                                    }
                                    #endregion

                                    #region [如果设备是扫描底盘下载生产任务]
                                    if (_v.isCarrierDownloadData)
                                    {
                                        continue;
                                    }
                                    #endregion

                                    #region[如果设备是投板机，投板机数量大于1，但是和当前请求的设备ID不一致，不下载生产任务。避免投板机生产任务重复下发]
                                    if (LoadEquipment.Count > 1)
                                    {
                                        if (em.EQID != _v.EQID)
                                        {
                                            if (_v.Type == eEquipmentType.L)
                                            {
                                                _v.isMultiLoad = true;//不同设备ID，是投板机设备时，多投板机Flag为True
                                                continue;
                                            }
                                            else
                                                _v.isMultiLoad = false;//不同设备ID，不是投板机设备时，多投板机Flag为False
                                        }
                                        else
                                            _v.isMultiLoad = false;//相同设备时，多投板机Flag为False
                                    }
                                    #endregion

                                    #region[如果SkipSubEqpID有值，不下载生产任务给后面设备]
                                    if (skipEm.EQID != null)
                                    {
                                        EAPEnvironment.commonLibrary.isTransferStatus = true;//开启传送功能
                                        if (_v.EQNo >= skipEm.EQNo)
                                        {
                                            _v.isSkipEquipment = true;//如果设备No>跳过的设备No，跳过Flag为True
                                            continue;
                                        }
                                        else _v.isSkipEquipment = false;
                                    }
                                    else _v.isSkipEquipment = false;
                                    #endregion

                                    #region[如果非连线工艺时，不设定下载设备，不下载生产任务给设备]
                                    if (lot.Value.IsConnectionStatus == eConnectionStatus.Disconnection.GetEnumDescription())
                                    {
                                        if (_v.isDisconnectionDataDownload)
                                        {
                                            EAPEnvironment.commonLibrary.isTransferStatus = true;//开启传送功能
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                    }

                                    #endregion

                                    _v.NewEQParameter = Environment.EAPEnvironment.commonLibrary.commonModel.GetParameterModel(_v, lot.Value);
                                    //下载生产任务
                                    new HostService.HostService().JobDataDownload(_v, lot.Value, "", _v.NewEQParameter);
                                }
                                Dictionary<EquipmentModel, LotModel> el = new Dictionary<EquipmentModel, LotModel>();
                                el.Add(em, lot.Value);
                                //开启生产任务下载处理线程
                                if (EAPEnvironment.EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_JobDataDownload, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_JobDataDownload, 4000, true, el))
                                {
                                    EAPEnvironment.commonLibrary.isProcessOK = true;
                                    BaseComm.LogMsg(Log.LogLevel.Info, string.Format("LotID<{0}>确认设备数据中......", lot.Value.LotID));
                                    EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckStart = true;
                                    EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckTime = DateTime.Now;
                                }
                            }
                        }
                        #endregion
                    }
                }
                #endregion
                return true;
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return false;
            }
        }
        /// <summary>
        /// MES下载，下货回复检查，清线检查
        /// </summary>
        /// <param name="isPPIDSame"></param>
        /// <param name="lotInfo"></param>
        /// <param name="port"></param>
        /// <param name="isCheckAllProcessCompletion"></param>
        /// <param name="em"></param>
        /// <param name="startEq"></param>
        /// <returns></returns>
        private bool MESDownloadJobDataRequestCheck(bool isPPIDSame, MessageModel.LotInfo lotInfo, PortModel port, bool isCheckAllProcessCompletion, EquipmentModel em, EquipmentModel startEq)
        {
            try
            {
                #region [检查清线]
                var lot = port.List_Lot.Where(r => r.Key == lotInfo.LotID).FirstOrDefault();
                Dictionary<MessageModel.LotInfo, CommModel> Dic_Message = new Dictionary<MessageModel.LotInfo, CommModel>();
                CommModel cm = new CommModel();
                EquipmentModel skipEm = new EquipmentModel();
                if (!string.IsNullOrEmpty(lotInfo.SkipSubEqpID))
                {
                    skipEm = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByEqID(lotInfo.SkipSubEqpID);
                }

                cm.pm = port;
                cm.lm = lot.Value;
                //cm.em = em;
                Dic_Message.Add(lotInfo, cm);

                #region 清整线
                if (!isCheckAllProcessCompletion)
                {
                    if (!EAPEnvironment.commonLibrary.commonModel.InAllProcessCompletionCheckStart)
                    {
                        if (EAPEnvironment.EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_AllProcessCompletionCheck, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_AllProcessCompletionCheck, 5000, true, Dic_Message))
                        {
                            EAPEnvironment.commonLibrary.isProcessOK = false;
                            BaseComm.LogMsg(Log.LogLevel.Info, string.Format("线体<{0}>确认清线......", EAPEnvironment.commonLibrary.LineName));
                            EAPEnvironment.commonLibrary.commonModel.InAllProcessCompletionCheckStart = true;
                            EAPEnvironment.commonLibrary.commonModel.InAllProcessCompletionCheckTime = DateTime.Now;
                        }
                    }
                }
                #endregion

                #region 清单机
                else
                {
                    if (!isPPIDSame)
                    {
                        if (!EAPEnvironment.commonLibrary.commonModel.InEquipmentJobDataRequestCheckStart)
                        {
                            if (EAPEnvironment.EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_EquipmentJobDataRequest, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_EquipmentJobDataRequest, 5000, true, Dic_Message))
                            {
                                EAPEnvironment.commonLibrary.isProcessOK = false;
                                BaseComm.LogMsg(Log.LogLevel.Info, string.Format("线体<{0}>确认设备清线......", EAPEnvironment.commonLibrary.LineName));
                                EAPEnvironment.commonLibrary.commonModel.InEquipmentJobDataRequestCheckStart = true;
                                EAPEnvironment.commonLibrary.commonModel.InEquipmentJobDataRequestCheckTime = DateTime.Now;
                            }
                        }
                    }
                    #endregion
                    #region [配方/配方参数相同，不需清线，直接下载生产任务]
                    else
                    {
                        if (!EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckStart)
                        {
                            EAPEnvironment.commonLibrary.isTransferStatus = false;
                            var LoadEquipment = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.Type == eEquipmentType.L).ToList();
                            foreach (var _v in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.EQNo >= startEq.EQNo))
                            {

                                #region[如果非连线工艺时，不设定下载设备，不下载生产任务给设备]
                                if (lot.Value.IsConnectionStatus == eConnectionStatus.Disconnection.GetEnumDescription())
                                {
                                    if (_v.isDisconnectionDataDownload)
                                    {
                                        EAPEnvironment.commonLibrary.isTransferStatus = true;//开启传送功能
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }

                                #endregion

                                _v.NewEQParameter = Environment.EAPEnvironment.commonLibrary.commonModel.GetParameterModel(_v, lot.Value);
                                //下载生产任务
                                new HostService.HostService().JobDataDownload(_v, lot.Value, "", _v.NewEQParameter);
                            }
                            Dictionary<EquipmentModel, LotModel> el = new Dictionary<EquipmentModel, LotModel>();
                            el.Add(em, lot.Value);
                            //开启生产任务下载处理线程
                            if (EAPEnvironment.EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_JobDataDownload, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_JobDataDownload, 4000, true, el))
                            {
                                EAPEnvironment.commonLibrary.isProcessOK = true;
                                BaseComm.LogMsg(Log.LogLevel.Info, string.Format("LotID<{0}>确认设备数据中......", lot.Value.LotID));
                                EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckStart = true;
                                EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckTime = DateTime.Now;
                            }
                        }

                        #endregion
                    }
                }
                #endregion
                return true;
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return false;
            }
        }

    }
}
