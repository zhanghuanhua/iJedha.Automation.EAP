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
    public partial class ProductionConditionCheckKL
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
                #region [首次下载生产任务时，生产任务检查逻辑]
                if (!EAPEnvironment.commonLibrary.isJobDataProcessDataTigger)
                {
                    //获取线程带进来的参数
                    Dictionary<MessageModel.LotInfoKL, CommModel> Dic_Message = (Dictionary<MessageModel.LotInfoKL, CommModel>)_DowryObj;
                    #region [定义物件信息，并赋值]
                    MessageModel.LotInfoKL lotInfo = new MessageModel.LotInfoKL();
                    PortModel port = new PortModel();
                    //LotModel lm = new LotModel();
                    EquipmentModel em = new EquipmentModel();
                    Lot subLot = new Lot();
                    foreach (var item in Dic_Message)
                    {
                        lotInfo = item.Key;
                        port = item.Value.pm;
                        //lm = item.Value.lm;
                        em = item.Value.em;
                        subLot = item.Value.lot;
                    }
                    #endregion

                    bool isCheckNg = false;

                    #region [确认设备是否Online]
                    //获取未连线设备物件
                    var eq1 = (from n in EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel()
                               where n.ConnectMode == EAP.Model.eConnectMode.DISCONNECT || (n.ControlMode != eControlMode.REMOTE)
                               select n).FirstOrDefault();
                    //如果不为null，说明有未连线设备，需有错误信息
                    if (eq1 != null)
                    {
                        if (eq1.isCheckConnect && eq1.isCheckControlMode)
                        {
                            string ErrorMsg = string.Format("LotID<{0}>上机检查异常：存在离线设备<{1}>，拒绝生产.", subLot.LotID, eq1.EQName);
                            BaseComm.ErrorHandleRule("E2104", ErrorMsg, ref errm);
                            isCheckNg = true;
                        }
                    }
                    #endregion

                    #region [确认设备状态:1.检查投板机是否正在投板；2.检查设备状态是否正常]  PS：开料线不做投板机是否在投板确认。因为上料后开料机默认都是在投板状态，直到跑完为之
                    if (isCheckNg == false)
                    {
                        //判断是否检查投板机正在投板状态
                        if (EAPEnvironment.commonLibrary.lineModel.isCheckLoadStatus)
                        {
                            ////获取正在跑的投板机物件
                            //var eq2 = (from n in EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel()
                            //           where (n.Type == eEquipmentType.L && n.EQStatus == eEQSts.Run)
                            //           select n).FirstOrDefault();
                            ////如果不为null，说明有正在跑的投板机，需有错误信息
                            //if (eq2 != null)
                            //{
                            //    string ErrorMsg = string.Format("LotID<{0}>上机检查异常：设备<{1}>投板机正在投板，拒绝生产.", subLot.LotID, eq2.EQName);
                            //    BaseComm.ErrorHandleRule("E2111", ErrorMsg, ref errm);
                            //    isCheckNg = true;
                            //}
                        }
                    }
                    if (isCheckNg == false)
                    {
                        //判断是否检查设备状态
                        if (EAPEnvironment.commonLibrary.lineModel.isCheckEquipmentStatus)
                        {
                            //获取设备状态时Down的设备物件
                            var eq3 = (from n in EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel()
                                       where n.EQStatus == eEQSts.Down
                                       select n).FirstOrDefault();
                            //如果不为null，说明有状态未Down的设备，需有错误信息
                            if (eq3 != null)
                            {
                                string ErrorMsg = string.Format("LotID<{0}>上机检查异常：设备<{1}>机况<{2}>异常，拒绝生产.", subLot.LotID, eq3.EQName, eq3.EQStatus);
                                BaseComm.ErrorHandleRule("E2110", ErrorMsg, ref errm);
                                isCheckNg = true;
                            }
                        }
                    }
                    #endregion

                    #region [检查有问题则下NG命令]
                    //如果条件检查失败，给所有设备发送CIM Message提示信息
                    if (isCheckNg)
                    {
                        string ErrorMsg = string.Format("E0002:LotID[{0}]数量[{1}]帐料参数检查失败.", subLot.LotID, subLot.PanelTotalQty);
                        BaseComm.LogMsg(Log.LogLevel.Warn, ErrorMsg);
                        //foreach (var item in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
                        //{
                        //    new HostService.HostService().CIMMessageCommand(item.EQID, "10", ErrorMsg, DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                        //}
                        return;
                    }
                    #endregion

                    #region 关掉线程，如果清线检查失败，再次打开线程
                    Environment.EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckInitial();
                    EAPEnvironment.EAPAp.DeleteThreadWork(ConstLibrary.CONST_DLL_RULE_ProductionConditionCheckKL);
                    // EAPEnvironment.commonLibrary.isNormalCheckData = true;
                    #endregion

                    #region [如果时同型号，直接下载生产任务给设备]
                    if (Environment.EAPEnvironment.commonLibrary.OldPN == subLot.PN && EAPEnvironment.commonLibrary.isNormalCheckData)
                    {
                        #region 批次不同时，暂停一个节拍时间
                        int LotTime = EAPEnvironment.commonLibrary.customizedLibrary.ProductionConditonCheckLotTime * 1000;
                        if (Environment.EAPEnvironment.commonLibrary.OldLotID != subLot.LotID)
                        {
                            BaseComm.LogMsg(Log.LogLevel.Warn, string.Format("线体<{0}>下载Lot ID不一致，请等待<{1}s>节拍时间", EAPEnvironment.commonLibrary.LineName, EAPEnvironment.commonLibrary.customizedLibrary.ProductionConditonCheckLotTime), false);
                            Thread.Sleep(LotTime);
                            Environment.EAPEnvironment.commonLibrary.OldLotID = subLot.LotID;
                        }
                        #endregion
                        if (!EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckStart)
                        {
                            foreach (var _v in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
                            {
                                List<MessageModel.Param> parameterModel;
                                //如果设定不下载参数信息给设备，则不下载参数给设备
                                if (_v.isRecipeParameterDownload)
                                {
                                    parameterModel = _v.NewEQParameter = Environment.EAPEnvironment.commonLibrary.commonModel.GetParameterModel_KL(_v, subLot);
                                }
                                else
                                {
                                    parameterModel = new List<MessageModel.Param>();
                                }
                                new HostService.HostService().JobDataDownload(_v, subLot, "", parameterModel);
                            }
                            //开启开料线任务下载线程
                            if (EAPEnvironment.EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_JobDataDownloadKL, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_JobDataDownloadKL, 4000, true, subLot))
                            {
                                EAPEnvironment.commonLibrary.isProcessOK = true;
                                BaseComm.LogMsg(Log.LogLevel.Info, string.Format("LotID<{0}>确认设备数据中......", subLot.LotID));
                                EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckStart = true;
                                EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckTime = DateTime.Now;
                            }
                        }
                    }
                    #endregion
                    else
                    {
                        #region [检查当前配方和配方参数是否和LotInfo内的一致]
                        //定义参数初始值相同
                        bool isPPIDSame = true;
                        //定义清线初始值完成
                        bool isCheckAllProcessCompletion = true;
                        //检查是否有前一料号，是否有前一批次号。如果没有，把当前批次的料号和批次号赋值进去
                        if (string.IsNullOrEmpty(Environment.EAPEnvironment.commonLibrary.OldPN) || string.IsNullOrEmpty(Environment.EAPEnvironment.commonLibrary.OldLotID))
                        {
                            Environment.EAPEnvironment.commonLibrary.OldPN = subLot.PN;
                            Environment.EAPEnvironment.commonLibrary.OldLotID = subLot.LotID;
                        }
                        else
                        {
                            //定义节拍字段，并抓取设定时间
                            int PNTime = EAPEnvironment.commonLibrary.customizedLibrary.ProductionConditonCheckPNTime * 1000;

                            //如果换料号，需等待节拍时间
                            if (Environment.EAPEnvironment.commonLibrary.OldPN != subLot.PN)
                            {
                                BaseComm.LogMsg(Log.LogLevel.Warn, string.Format("线体<{0}>下载PN不一致，请等待<{1}s>节拍时间", EAPEnvironment.commonLibrary.LineName, EAPEnvironment.commonLibrary.customizedLibrary.ProductionConditonCheckPNTime), false);
                                Thread.Sleep(PNTime);
                                Environment.EAPEnvironment.commonLibrary.OldPN = subLot.PN;
                            }

                            if (isCheckNg == false)
                            {
                                //检查全线设备前参数和当前下载参数是否相同
                                foreach (var vem in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
                                {
                                    vem.isRecipeChange = false;
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
                                                Environment.EAPEnvironment.commonLibrary.commonModel.GetParameterModel_KL(vem, subLot);
                                                //获取参数名称与前一参数相同的 参数物件(ParameterModel)
                                                var parameter = (from q in subLot.SubEqpList.Where(r => r.SubEqpID == vem.EQID) from w in q.EQParameter where w.ItemName == item.ParamName select w).FirstOrDefault();
                                                //比对前一参数值和下载参数值是否相同，如果不同需记录不同信息
                                                if (item.ParamValue != parameter.ItemValue)
                                                {
                                                    string ErrorMsg = string.Format("LotID<{0}>上机检查异常：线体生产中，设备<{1}> 参数名称<{2}>Value<{3}>与Lot中的参数名称<{4}>Value<{5}>不一致，检查清线.",
                                                            subLot.LotID, vem.EQName, item.ParamName, item.ParamValue, parameter.ItemName, parameter.ItemValue);
                                                    BaseComm.ErrorHandleRule("E2115", ErrorMsg, ref errm);
                                                    isCheckNg = true;
                                                    //判断是否有设定清设备Flag
                                                    if (!vem.isProcessCompletion)
                                                    {
                                                        isPPIDSame = false;
                                                        vem.isRecipeChange = true;
                                                    }
                                                    //判断是否有设定清线Flag
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
                        if (!EquipmentJobDataRequestCheck(isPPIDSame, lotInfo, port, em, subLot, isCheckAllProcessCompletion))
                        {
                            BaseComm.LogMsg(Log.LogLevel.Warn, string.Format("线体<{0}>未能清线", EAPEnvironment.commonLibrary.LineName), false);
                        }
                        #endregion
                    }

                    #region 关掉线程，如果清线检查失败，再次打开线程
                    Environment.EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckInitial();
                    EAPEnvironment.EAPAp.DeleteThreadWork(ConstLibrary.CONST_DLL_RULE_ProductionConditionCheckKL);
                    //第一次进入检查生产条件Flag
                    EAPEnvironment.commonLibrary.isNormalCheckData = true;
                    #endregion
                }
                #endregion

                #region [上一Lot完工后，如果还有Lot列表，自动触发生产任务检查逻辑]
                else
                {
                    //获取线程带进来的参数
                    Dictionary<MessageModel.LotInfoKL, CommModel> Dic_Message = (Dictionary<MessageModel.LotInfoKL, CommModel>)_DowryObj;
                    #region [定义物件信息，并赋值]
                    MessageModel.LotInfoKL lotInfo = new MessageModel.LotInfoKL();
                    PortModel port = new PortModel();
                    //LotModel lm = new LotModel();
                    EquipmentModel em = new EquipmentModel();
                    Lot subLot = new Lot();
                    foreach (var item in Dic_Message)
                    {
                        //lotInfo = item.Key;
                        port = item.Value.pm;
                        //lm = item.Value.lm;
                        em = item.Value.em;
                        subLot = item.Value.lot;
                    }
                    #endregion
                    //EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckStart = true;
                    bool isCheckNg = false;

                    #region [确认设备是否Online]
                    //获取未连线设备
                    var eq1 = (from n in EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel()
                               where n.ConnectMode == EAP.Model.eConnectMode.DISCONNECT || (n.ControlMode != eControlMode.REMOTE)
                               select n).FirstOrDefault();
                    //如果不为null，说明有未连线的设备，错报
                    if (eq1 != null)
                    {
                        if (eq1.isCheckConnect && eq1.isCheckControlMode)
                        {
                            string ErrorMsg = string.Format("LotID<{0}>上机检查异常：存在离线设备<{1}>，拒绝生产.", subLot.LotID, eq1.EQName);
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
                            ////获取正在跑的投板机信息
                            //var eq2 = (from n in EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel()
                            //           where (n.Type == eEquipmentType.L && n.EQStatus == eEQSts.Run)
                            //           select n).FirstOrDefault();
                            ////如果不为null，说明有正在跑的投板机，报错
                            //if (eq2 != null)
                            //{
                            //    string ErrorMsg = string.Format("LotID<{0}>上机检查异常：设备<{1}>投板机正在投板，拒绝生产.", subLot.LotID, eq2.EQName);
                            //    BaseComm.ErrorHandleRule("E2111", ErrorMsg, ref errm);
                            //    isCheckNg = true;
                            //}
                        }
                    }
                    if (isCheckNg == false)
                    {
                        //判断是否检查设备状态
                        if (EAPEnvironment.commonLibrary.lineModel.isCheckEquipmentStatus)
                        {
                            //获取状态为Down的设备信息
                            var eq3 = (from n in EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel()
                                       where n.EQStatus == eEQSts.Down
                                       select n).FirstOrDefault();
                            //如果不为null，说明有设备状为Down的设备
                            if (eq3 != null)
                            {
                                string ErrorMsg = string.Format("LotID<{0}>上机检查异常：设备<{1}>机况<{2}>异常，拒绝生产.", subLot.LotID, eq3.EQName, eq3.EQStatus);
                                BaseComm.ErrorHandleRule("E2110", ErrorMsg, ref errm);
                                isCheckNg = true;
                            }
                        }
                    }
                    #endregion

                    #region [检查有问题则下NG命令]
                    if (isCheckNg)
                    {
                        string ErrorMsg = string.Format("E0002:LotID[{0}]数量[{1}]帐料参数检查失败.", subLot.LotID, subLot.PanelTotalQty);
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
                    EAPEnvironment.EAPAp.DeleteThreadWork(ConstLibrary.CONST_DLL_RULE_ProductionConditionCheckKL);
                    // EAPEnvironment.commonLibrary.isNormalCheckData = true;
                    #endregion
                    #region [如果时同型号，直接下载生产任务给设备。]
                    if (Environment.EAPEnvironment.commonLibrary.OldPN == subLot.PN && EAPEnvironment.commonLibrary.isNormalCheckData)
                    {
                        #region 批次不同时，暂停一个节拍时间
                        int LotTime = EAPEnvironment.commonLibrary.customizedLibrary.ProductionConditonCheckLotTime * 1000;
                        if (Environment.EAPEnvironment.commonLibrary.OldLotID != subLot.LotID)
                        {
                            BaseComm.LogMsg(Log.LogLevel.Warn, string.Format("线体<{0}>下载Lot ID不一致，请等待<{1}s>节拍时间", EAPEnvironment.commonLibrary.LineName, EAPEnvironment.commonLibrary.customizedLibrary.ProductionConditonCheckLotTime), false);
                            Thread.Sleep(LotTime);
                            Environment.EAPEnvironment.commonLibrary.OldLotID = subLot.LotID;
                        }
                        #endregion
                        if (!EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckStart)
                        {
                            foreach (var _v in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
                            {
                                List<MessageModel.Param> parameterModel;
                                if (_v.isRecipeParameterDownload)
                                {
                                    parameterModel = _v.NewEQParameter = Environment.EAPEnvironment.commonLibrary.commonModel.GetParameterModel_KL(_v, subLot);
                                }
                                else
                                {
                                    parameterModel = new List<MessageModel.Param>();
                                }


                                new HostService.HostService().JobDataDownload(_v, subLot, "", parameterModel);
                            }
                            if (EAPEnvironment.EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_JobDataDownloadKL, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_JobDataDownloadKL, 4000, true, subLot))
                            {
                                EAPEnvironment.commonLibrary.isProcessOK = true;
                                BaseComm.LogMsg(Log.LogLevel.Info, string.Format("LotID<{0}>确认设备数据中......", subLot.LotID));
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
                            Environment.EAPEnvironment.commonLibrary.OldPN = subLot.PN;
                            Environment.EAPEnvironment.commonLibrary.OldLotID = subLot.LotID;
                        }
                        else
                        {
                            int PNTime = 10;// EAPEnvironment.commonLibrary.customizedLibrary.ProductionConditonCheckPNTime * 1000;

                            if (Environment.EAPEnvironment.commonLibrary.OldPN != subLot.PN)
                            {
                                BaseComm.LogMsg(Log.LogLevel.Warn, string.Format("线体<{0}>下载PN不一致，请等待<{1}s>节拍时间", EAPEnvironment.commonLibrary.LineName, EAPEnvironment.commonLibrary.customizedLibrary.ProductionConditonCheckPNTime), false);
                                Thread.Sleep(PNTime);
                                Environment.EAPEnvironment.commonLibrary.OldPN = subLot.PN;
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
                                                Environment.EAPEnvironment.commonLibrary.commonModel.GetParameterModel_KL(vem, subLot);
                                                var parameter = (from q in subLot.SubEqpList.Where(r => r.SubEqpID == vem.EQID) from w in q.EQParameter where w.ItemName == item.ParamName select w).FirstOrDefault();
                                                if (item.ParamValue != parameter.ItemValue)
                                                {
                                                    string ErrorMsg = string.Format("LotID<{0}>上机检查异常：线体生产中，设备<{1}> 参数名称<{2}>Value<{3}>与Lot中的参数名称<{4}>Value<{5}>不一致，检查清线.",
                                                            subLot.LotID, vem.EQName, item.ParamName, item.ParamValue, parameter.ItemName, parameter.ItemValue);
                                                    BaseComm.ErrorHandleRule("E2115", ErrorMsg, ref errm);
                                                    isCheckNg = true;
                                                    if (!vem.isProcessCompletion)
                                                    {
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
                        if (!EquipmentJobDataRequestCheck(isPPIDSame, lotInfo, port, em, subLot, isCheckAllProcessCompletion))
                        {
                            BaseComm.LogMsg(Log.LogLevel.Warn, string.Format("线体<{0}>未能清线", EAPEnvironment.commonLibrary.LineName), false);
                        }
                        #endregion
                    }

                    //#region 关掉线程，如果清线检查失败，再次打开线程
                    //Environment.EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckInitial();
                    //EAPEnvironment.EAPAp.DeleteThreadWork(ConstLibrary.CONST_DLL_RULE_ProductionConditionCheckKL);
                    EAPEnvironment.commonLibrary.isNormalCheckData = true;
                    //#endregion
                }
                #endregion
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return;
            }

        }
        private bool EquipmentJobDataRequestCheck(bool isPPIDSame, MessageModel.LotInfoKL lotInfo, PortModel port, EquipmentModel em, Lot subLot, bool isCheckAllProcessCompletion)
        {
            try
            {
                #region [检查清线]
                Dictionary<MessageModel.LotInfoKL, CommModel> Dic_Message = new Dictionary<MessageModel.LotInfoKL, CommModel>();
                CommModel cm = new CommModel();

                cm.pm = port;
                // cm.lm = lm;
                cm.em = em;
                cm.lot = subLot;
                Dic_Message.Add(lotInfo, cm);

                #region [开启清整线线程]
                if (!isCheckAllProcessCompletion)
                {
                    if (!EAPEnvironment.commonLibrary.commonModel.InAllProcessCompletionCheckStart)
                    {
                        if (EAPEnvironment.EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_AllProcessCompletionCheckKL, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_AllProcessCompletionCheckKL, 5000, true, Dic_Message))
                        {
                            EAPEnvironment.commonLibrary.isProcessOK = false;
                            BaseComm.LogMsg(Log.LogLevel.Info, string.Format("线体<{0}>确认清线......", EAPEnvironment.commonLibrary.LineName));
                            EAPEnvironment.commonLibrary.commonModel.InAllProcessCompletionCheckStart = true;
                            EAPEnvironment.commonLibrary.commonModel.InAllProcessCompletionCheckTime = DateTime.Now;
                        }
                    }
                }
                #endregion

                #region [开启清单机线程]
                else
                {
                    if (!isPPIDSame)
                    {
                        if (!EAPEnvironment.commonLibrary.commonModel.InEquipmentJobDataRequestCheckStart)
                        {
                            if (EAPEnvironment.EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_EquipmentJobDataRequestKL, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_EquipmentJobDataRequestKL, 5000, true, Dic_Message))
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
                            foreach (var _v in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
                            {

                                _v.NewEQParameter = Environment.EAPEnvironment.commonLibrary.commonModel.GetParameterModel_KL(_v, subLot);

                                new HostService.HostService().JobDataDownload(_v, subLot, "", _v.NewEQParameter);
                            }
                            if (EAPEnvironment.EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_JobDataDownloadKL, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_JobDataDownloadKL, 4000, true, subLot))
                            {
                                EAPEnvironment.commonLibrary.isProcessOK = true;
                                BaseComm.LogMsg(Log.LogLevel.Info, string.Format("LotID<{0}>确认设备数据中......", subLot.LotID));
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
