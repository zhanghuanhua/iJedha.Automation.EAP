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
    public partial class ProductionConditionCheckInspectCheck
    {
        /// <summary>
        /// 生产条件检查
        /// </summary>
        /// <param name="_DowryObj"></param>
        public void Invoke(object _DowryObj)
        {
            try
            {
                if (!Environment.EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckInspectCheckStart)
                {
                    return;
                }
                ErrorCodeModelBase errm = new ErrorCodeModelBase();
               // LotModel lm = (LotModel)_DowryObj;
                Dictionary<EquipmentModel, LotModel> Dic_el = (Dictionary<EquipmentModel, LotModel>)_DowryObj;
                LotModel lm = new LotModel();
                EquipmentModel em = new EquipmentModel();
                bool isCheckNg = false;
                foreach (var item in Dic_el)
                {
                    lm = item.Value;
                    em = item.Key;
                }
                //EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckStart = true;
                #region[一台设备是一个Main设备，机械钻机使用]
                if (EAPEnvironment.commonLibrary.lineModel.isMainEqpID)
                {
                    //如果MES下载的当前MainEqpID的钻机不是Idle状态，拒绝下载生产任务
                    var GetCurrentEq = (from n in EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel()
                                        where (n.EQID == lm.MainEqpID)
                                        select n).FirstOrDefault();

                    if (GetCurrentEq != null)
                    {
                        //如果设备状态不是Idle，说明设备未准备好，拒绝生产
                        if (GetCurrentEq.EQStatus != eEQSts.Idle)
                        {
                            string ErrorMsg = string.Format("Lot<{0}>上机检查异常：设备<{1}>当前状态<{2}>,未Ready，拒绝生产.", lm.LotID, GetCurrentEq.EQName, GetCurrentEq.EQStatus);
                            BaseComm.ErrorHandleRule("E2111", ErrorMsg, ref errm);
                            isCheckNg = true;
                        }
                    }
                    else
                    {
                        //如果找不到设备物件，记录错误信息
                        string ErrorMsg = string.Format("Lot<{0}>上机检查异常：找不到设备ID<{1}>，拒绝生产.", lm.LotID, lm.MainEqpID);
                        BaseComm.ErrorHandleRule("E2113", ErrorMsg, ref errm);
                        isCheckNg = true;
                    }
                    //如果上面检查错误，记录错误信息，并return出去
                    if (isCheckNg)
                    {
                        string ErrorMsg = string.Format("Lot<{0}>上机检查异常：生产条件检查失败，拒绝生产.", lm.LotID);
                        BaseComm.ErrorHandleRule("E2114", ErrorMsg, ref errm);
                        return;
                    }
                    else
                    {
                        //如果检查设备生产条件通过，开启下载生产任务线程
                        if (!EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckStart)
                        {
                            var MainEq = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.EQID == lm.MainEqpID).FirstOrDefault();
                            List<MessageModel.Param> parameterModel;
                            //如果设定下载参数，则下载设备参数给对应设备，否则不下载参数给设备
                            if (MainEq.isRecipeParameterDownload)
                            {
                                parameterModel = MainEq.NewEQParameter = Environment.EAPEnvironment.commonLibrary.commonModel.GetParameterModel(MainEq, lm);
                            }
                            else
                            {
                                parameterModel = new List<MessageModel.Param>();
                            }
                            new HostService.HostService().JobDataDownload(MainEq, lm, lm.LocalPortStation, parameterModel);

                            Dictionary<EquipmentModel, LotModel> el = new Dictionary<EquipmentModel, LotModel>();
                            el.Add(em, lm);
                            //开启线程
                            if (EAPEnvironment.EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_JobDataDownload, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_JobDataDownload, 4000, true, el))
                            {
                                EAPEnvironment.commonLibrary.isProcessOK = true;
                                BaseComm.LogMsg(Log.LogLevel.Info, string.Format("Lot<{0}>确认设备数据中......", lm.LotID));
                                EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckStart = true;
                                EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckTime = DateTime.Now;
                            }
                        }
                    }

                    
                }
                #endregion
                else
                {
                    #region [确认设备是否Online]
                    //获取未连线设备物件
                    var eq1 = (from n in EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel()
                               where n.ConnectMode == EAP.Model.eConnectMode.DISCONNECT || (n.ControlMode != eControlMode.REMOTE)
                               select n).FirstOrDefault();
                    if (eq1 != null)
                    {
                        if (eq1.isCheckConnect && eq1.isCheckControlMode)
                        {
                            string ErrorMsg = string.Format("Lot<{0}>上机检查异常：存在离线设备<{1}>，拒绝生产.", lm.LotID, eq1.EQName);
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
                            //获取正在跑的投板机设备物件
                            var eq2 = (from n in EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel()
                                       where (n.Type == eEquipmentType.L && n.EQStatus == eEQSts.Run)
                                       select n).FirstOrDefault();
                            if (eq2 != null)
                            {
                                string ErrorMsg = string.Format("Lot<{0}>上机检查异常：设备<{1}>投板机正在投板，拒绝生产.", lm.LotID, eq2.EQName);
                                BaseComm.ErrorHandleRule("E2111", ErrorMsg, ref errm);
                                isCheckNg = true;
                            }
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
                            if (eq3 != null)
                            {
                                string ErrorMsg = string.Format("Lot<{0}>上机检查异常：设备<{1}>机况<{2}>异常，拒绝生产.", lm.LotID, eq3.EQName, eq3.EQStatus);
                                BaseComm.ErrorHandleRule("E2110", ErrorMsg, ref errm);
                                isCheckNg = true;
                            }
                        }
                    }
                    #endregion

                    #region [检查有问题则下NG命令]
                    if (isCheckNg)
                    {
                        //如果检查失败，给设备下载错误提示信息
                        string ErrorMsg = string.Format("E0002:Lot[{0}]产品[{1}]数量[{2}]帐料参数检查失败.", lm.LotID, lm.PN, lm.LotQty);
                        BaseComm.LogMsg(Log.LogLevel.Warn, ErrorMsg);

                        foreach (var item in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
                        {
                            new HostService.HostService().CIMMessageCommand(item.EQID, "10", ErrorMsg, DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                        }
                        return;
                    }
                    #endregion

                    #region [下载生产任务给设备]
                    if (!EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckStart)
                    {
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
                            List<MessageModel.Param> parameterModel;
                            //如果设定下载参数，则下载设备参数给对应设备，否则不下载参数给设备
                            if (_v.isRecipeParameterDownload)
                            {
                                parameterModel = _v.NewEQParameter = Environment.EAPEnvironment.commonLibrary.commonModel.GetParameterModel(_v, lm);
                            }
                            else
                            {
                                parameterModel = new List<MessageModel.Param>();
                            }
                            new HostService.HostService().JobDataDownload(_v, lm, lm.LocalPortStation, parameterModel);
                        }
                        Dictionary<EquipmentModel, LotModel> el = new Dictionary<EquipmentModel, LotModel>();
                        el.Add(em, lm);
                        //开启任务下载线程
                        if (EAPEnvironment.EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_JobDataDownload, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_JobDataDownload, 4000, true, el))
                        {
                            EAPEnvironment.commonLibrary.isProcessOK = true;
                            BaseComm.LogMsg(Log.LogLevel.Info, string.Format("Lot<{0}>确认设备数据中......", lm.LotID));
                            EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckStart = true;
                            EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckTime = DateTime.Now;
                        }
                    }
                    #endregion
                }
                
                #region 关掉线程
                Environment.EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckInspectCheckInitial();
                EAPEnvironment.EAPAp.DeleteThreadWork(ConstLibrary.CONST_DLL_RULE_ProductionConditionCheckInspectCheck);
                #endregion

            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return;
            }

        }


    }
}
