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
using System;
using System.Collections.Generic;
using System.Linq;
/// <summary>
/// 触发功能：
/// </summary>
namespace iJedha.Automation.EAP.Rule
{
    public partial class AllProcessCompletionCheck
    {
        /// <summary>
        /// 检查清线是否完成
        /// </summary>
        /// <param name="_DowryObj"></param>
        public void Invoke(object _DowryObj)
        {
            try
            {
                if (!Environment.EAPEnvironment.commonLibrary.commonModel.InAllProcessCompletionCheckStart)
                {
                    return;
                }
                //获取线程参数
                Dictionary<MessageModel.LotInfo, CommModel> Dic_Message = (Dictionary<MessageModel.LotInfo, CommModel>)_DowryObj;
                #region [定义物件，并进行赋值]
                MessageModel.LotInfo lotInfo = new MessageModel.LotInfo();
                PortModel port = new PortModel();
                LotModel lm = new LotModel();
                EquipmentModel em = new EquipmentModel();
                foreach (var item in Dic_Message)
                {
                    lotInfo = item.Key;
                    port = item.Value.pm;
                    lm = item.Value.lm;
                    em = item.Value.em;
                }
                #endregion
                #region [清线完成逻辑]
                if (EAPEnvironment.commonLibrary.isAllProcessOK)
                {
                    #region 杀掉当前线程
                    Environment.EAPEnvironment.commonLibrary.commonModel.InAllProcessCompletionCheckInitial();
                    EAPEnvironment.EAPAp.DeleteThreadWork(ConstLibrary.CONST_DLL_RULE_AllProcessCompletionCheck);
                    #region [ 融铆合线体多Load设备逻辑]
                    if (EAPEnvironment.commonLibrary.lineModel.isMultiLoadEquipment)
                    {
                        //获取设备参数
                        List<MessageModel.Param> parameterModel = em.NewEQParameter = Environment.EAPEnvironment.commonLibrary.commonModel.GetParameterModel(em, lm); ;
                        //下载生产任务
                        new HostService.HostService().JobDataDownload(em, lm, lm.LocalPortStation, parameterModel);
                        em.isRecipeChange = false;

                        if (!EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckStart)
                        {
                            //定义线程参数，并赋值
                            Dictionary<EquipmentModel, LotModel> el = new Dictionary<EquipmentModel, LotModel>();
                            el.Add(em, lm);
                            //开启任务下载线程
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
                    #endregion

                    else
                    {
                        //获取投板机设备物件
                        var LoadEquipment = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.Type == eEquipmentType.L).ToList();
                        EquipmentModel skipEm = new EquipmentModel();
                        //如果MES有下载跳过不生产设备ID，用跳过不生产设备ID获取设备物件
                        if (!string.IsNullOrEmpty(lm.SkipSubEqpID))
                        {
                            skipEm = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByEqID(lm.SkipSubEqpID);
                        }
                        #region 清线完成，下载生产任务
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
                            //获取设备配方参数
                            List<MessageModel.Param> parameterModel = _v.NewEQParameter = Environment.EAPEnvironment.commonLibrary.commonModel.GetParameterModel(_v, lm); ;
                            //下载生产任务
                            new HostService.HostService().JobDataDownload(_v, lm, lm.LocalPortStation, parameterModel);
                            _v.isRecipeChange = false;
                        }
                        if (!EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckStart)
                        {
                            //定义线程参数，并赋值
                            Dictionary<EquipmentModel, LotModel> el = new Dictionary<EquipmentModel, LotModel>();
                            el.Add(em, lm);
                            //开启生产任务下载线程
                            if (EAPEnvironment.EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_JobDataDownload, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_JobDataDownload, 4000, true, el))
                            {
                                EAPEnvironment.commonLibrary.isProcessOK = true;
                                BaseComm.LogMsg(Log.LogLevel.Info, string.Format("Lot<{0}>确认设备数据中......", lotInfo.LotID));
                                EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckStart = true;
                                EAPEnvironment.commonLibrary.commonModel.InJobDataDownloadCheckTime = DateTime.Now;
                            }
                        }
                        #endregion
                    }
                }
                #endregion
                else
                {
                    //清线未完成逻辑，再次打开条件检查线程，检查清线情况
                    if (!EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckStart)
                    {
                        if (EAPEnvironment.EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_ProductionConditionCheck, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_ProductionConditionCheck, 4000, true, Dic_Message))
                        {
                            //NormalCheckData=false，说明不是首次执行条件检查线程
                            EAPEnvironment.commonLibrary.isNormalCheckData = false;
                            BaseComm.LogMsg(Log.LogLevel.Info, string.Format("线体<{0}>正在检查生产条件......", lotInfo.MainEqpID));
                            EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckStart = true;
                            EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckTime = DateTime.Now;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Environment.BaseComm.LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return;
            }
            finally
            {

            }
        }

    }
}
