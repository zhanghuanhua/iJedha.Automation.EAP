using iJedha.Automation.EAP.Environment;
using iJedha.Automation.EAP.Library;
using iJedha.Automation.EAP.Model;
using iJedha.Customized.MessageStructure;
using System;
using System.Collections.Generic;
using System.Linq;
/// <summary>
/// 
/// </summary>
namespace iJedha.Automation.EAP.Rule
{
    public partial class TrackInCheck : BaseComm
    {
        public void Invoke(object _DowryObj)
        {
            try
            {
                if (!Environment.EAPEnvironment.commonLibrary.commonModel.InTrackInCheckStart) return;
                LotModel lot = (LotModel)_DowryObj;
                int ngCount = 0;
                bool isCheckNg = false;

                #region 确认设备是否Online
                if (isCheckNg == false)
                {
                    var eq1 = (from n in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel()
                               where n.ConnectMode == eConnectMode.DISCONNECT || (n.ControlMode != eControlMode.ONLINE_LOCAL && n.ControlMode != eControlMode.ONLINE_REMOTE)
                               select n).FirstOrDefault();
                    if (eq1 != null)
                    {
                        lot.ErrorMsg = string.Format("Lot<{0}>上机检查异常：存在离线设备<{1}>，拒绝生产.", lot.LotID, eq1.EQName);
                        Environment.BaseComm.ErrorHandleRule("E2012", lot.ErrorMsg);
                        isCheckNg = true;
                    }
                }

                #endregion

                #region 确认Recipe
                if (isCheckNg == false)
                {
                    foreach (var v in Environment.EAPEnvironment.commonLibrary.commonModel.InParameterCheckResult)
                    {
                        EquipmentModel subem = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(v.Key);
                        if (subem == null) continue;
                        if (subem.isCheckPPID && !string.IsNullOrEmpty(v.Value.PPID) && subem.isCheckPPIDResult == false)
                        {
                            if (subem.List_PPID.Count == 0)
                            {
                                #region 确认配方超时
                                if (TimeoutCheck(45))
                                {
                                    lot.ErrorMsg = string.Format("Lot<{0}>上机检查异常：设备<{1}>配方名<{2}>收集超时，拒绝生产.", lot.LotID, subem.EQName, subem.CheckPPID);
                                    Environment.BaseComm.ErrorHandleRule("E2014", lot.ErrorMsg);
                                    isCheckNg = true;
                                }
                                else
                                {
                                    return;
                                }
                                #endregion
                            }

                            if (isCheckNg == false)
                            {
                                if (!subem.List_PPID.ContainsKey(subem.CheckPPID))
                                {
                                    subem.isCheckPPIDResult = false;
                                    lot.ErrorMsg = string.Format("Lot<{0}>上机检查异常：设备<{1}>配方名<{2}>不匹配，标准配方<{3}>，拒绝生产.", lot.LotID, subem.EQName, subem.List_PPID.FirstOrDefault().Key, subem.CheckPPID);
                                    Environment.BaseComm.ErrorHandleRule("E2013", lot.ErrorMsg);
                                    isCheckNg = true;
                                }
                                else
                                {
                                    subem.isCheckPPIDResult = true;
                                    LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Info, string.Format("Lot<{0}>上机检查正常：设备配方<{1}>匹配.", lot.LotID, subem.CheckPPID));
                                }
                            }
                        }
                    }
                }
                #endregion

                #region 确认参数
                if (isCheckNg == false)
                {
                    ngCount = (from n in Environment.EAPEnvironment.commonLibrary.commonModel.InParameterCheckResult where n.Value.ParameterCheckResult == eParameterCheckResult.ng select n).Count();
                    if (ngCount > 0)
                    {
                        isCheckNg = true;
                    }
                }

                if (isCheckNg == false)
                {
                    ngCount = (from n in Environment.EAPEnvironment.commonLibrary.commonModel.InParameterCheckResult where n.Value.ParameterCheckResult == eParameterCheckResult.wait select n).Count();
                    if (ngCount > 0)
                    {
                        string EQNO = (from n in Environment.EAPEnvironment.commonLibrary.commonModel.InParameterCheckResult where n.Value.ParameterCheckResult == eParameterCheckResult.wait select n.Key).FirstOrDefault();
                        EquipmentModel subem = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(EQNO);
                        #region 参数确认超时
                        if (TimeoutCheck(45))
                        {
                            lot.ErrorMsg = string.Format("Lot<{0}>上机检查异常：设备<{1}>参数确认超时，拒绝生产.", lot.LotID, subem.EQName);
                            Environment.BaseComm.ErrorHandleRule("E2016", lot.ErrorMsg);
                            isCheckNg = true;
                        }
                        else
                        {
                            return;
                        }
                        #endregion
                    }
                }
                #endregion

                #region 确认UnloadPort是否存在载具
                //if (isCheckNg == false)
                //{
                //    ngCount = (from n in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel()
                //               from p in n.List_Port.Values
                //               where n.Type == "U" && p.PortStatus == ePortStatus.LOADCOMPLETE && p.CarrierID != string.Empty
                //               select p).Count();
                //    if (ngCount == 0)
                //    {
                //        lot.ErrorMsg = string.Format("Lot<{0}>上机检查异常：收板机上料口,等待上空载具.", lot.LotID);
                //        LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Warn, lot.ErrorMsg);

                //        #region 确认收板机上载具超时
                //        if (TimeoutCheck(300))
                //        {
                //            lot.ErrorMsg = string.Format("Lot<{0}>上机检查异常：收板机上料口,未上空载具，拒绝上机.", lot.LotID);
                //            Environment.BaseComm.ErrorHandleRule("E2017", lot.ErrorMsg);
                //            isCheckNg = true;
                //        }
                //        else
                //        {
                //            return;
                //        }
                //        #endregion
                //    }
                //}
                #endregion

                #region 确认Unload机况
                if (isCheckNg == false)
                {
                    //ngCount = (from n in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel()
                    //           where n.Type == "U" && n.EQStatus == eEQSts.Run
                    //           select n).Count();
                    var eq3 = (from n in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel()
                               where n.Type == "U" && n.EQStatus != eEQSts.Run
                               select n).FirstOrDefault();
                    if (eq3 != null)
                    {
                        lot.ErrorMsg = string.Format("Lot<{0}>上机检查异常：设备<{1}>机况<{2}>异常，拒绝生产.", lot.LotID, eq3.EQName, eq3.EQStatus);
                        Environment.BaseComm.ErrorHandleRule("E2018", lot.ErrorMsg);
                        isCheckNg = true;
                    }
                }
                #endregion

                #region 项目完成检查命令设备
                new iJedha.Automation.EAP.Environment.BaseComm().DeleteTimerWork("TrackInCheck");
                Environment.EAPEnvironment.commonLibrary.commonModel.InTrackInCheckInitial();
                EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByType("L");
                if (isCheckNg == false)
                {
                    if (lot.DataSource == "Auto")
                    {
                        if (EAPEnvironment.commonLibrary.commonModel.CurMode == eProductMode.Auto1 || EAPEnvironment.commonLibrary.commonModel.CurMode == eProductMode.Auto2)
                        {
                            LogMsg(Core.Log.LogLevel.Info, string.Format("Lot<{0}>产品<{1}>数量<{2}>帐料参数检查成功.", lot.LotID, lot.PN, lot.PanelTotalQty));
                            new Environment.WCFReport().EAP_LotTrackInRequest(new LotTrackInRequest()
                            {
                                MainEqpID = Environment.EAPEnvironment.commonLibrary.LineName,
                                SubEqpID = em.EQName,
                                PortID = lot.LocalPortStation,
                                CarriedID = lot.CarrierID,
                                WIPDataList = GetTrackInWipData(lot)
                            }, lot);
                        }
                    }
                    else if (lot.DataSource == "Manual")
                    {
                        lock (lot)
                        {
                            lot.LotProcessStatus = eLotProcessStatus.Ready;
                        }

                        Environment.BaseComm.LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Info, string.Format("Lot<{0}>产品<{1}>数量<{2}>帐料参数检查成功，上机完成.", lot.LotID, lot.PN, lot.PanelTotalQty));
                        new MQReport().MQ_LotInformation(lot.LotID, lot.PN, lot.PanelTotalQty.ToString(), "上机完成");

                        //0:OK 1:NG
                        #region iJDEAP_0004 计算投板数量 = 产品数量 + Dummy数量
                        int inputCount = lot.ProcessQty;
                        if (Environment.EAPEnvironment.commonLibrary.customizedLibrary.DummyTestCount != 0)
                        {
                            inputCount = inputCount + EAPEnvironment.commonLibrary.customizedLibrary.DummyTestCount;
                        }
                        #endregion
                        new Environment.HSMSReport().S2F41(em, ConstLibrary.CONST_COMMAND_LoadStart, new List<string> { "0" });
                        LogMsg(Core.Log.LogLevel.Info, string.Format("Lot<{0}>允许上机<{1}>.", lot.LotID, inputCount.ToString()));
                        EquipmentModel uem = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByType("U");
                        if (uem != null)
                        {
                            #region iJDEAP_0004 计算收板数量 = 产品数量 + Dummy数量 - 动态检数量
                            double outputCount = lot.ProcessQty;
                            if (Environment.EAPEnvironment.commonLibrary.customizedLibrary.DummyTestCount != 0)
                            {
                                outputCount = outputCount + EAPEnvironment.commonLibrary.customizedLibrary.DummyTestCount;
                            }
                            if (Environment.EAPEnvironment.commonLibrary.customizedLibrary.DynamicTestCount != 0)
                            {
                                outputCount = outputCount - EAPEnvironment.commonLibrary.customizedLibrary.DynamicTestCount;
                            }
                            else if (Environment.EAPEnvironment.commonLibrary.customizedLibrary.DynamicTestPercent != 0)
                            {
                                outputCount = outputCount - Convert.ToInt16(lot.PanelTotalQty * EAPEnvironment.commonLibrary.customizedLibrary.DynamicTestPercent * 0.01);
                            }
                            #endregion
                            new Environment.HSMSReport().S2F41(uem, ConstLibrary.CONST_COMMAND_CarrierStart, new List<string> { "0", lot.LotID, outputCount.ToString() });
                            LogMsg(Core.Log.LogLevel.Info, string.Format("Lot<{0}>允许下机<{1}>.", lot.LotID, outputCount.ToString()));
                        }
                    }
                }
                else
                {
                    LogMsg(Core.Log.LogLevel.Warn, string.Format("Lot<{0}>产品<{1}>数量<{2}>帐料参数检查失败.", lot.LotID, lot.PN, lot.PanelTotalQty));
                    //0:OK 1:NG
                    new HSMSReport().S2F41(em, ConstLibrary.CONST_COMMAND_LoadStart, new List<string> { "1" });
                    if (em.isTerminalSelect) new HSMSReport().S10F3(em, "Lot Data Check Paramter NG.");
                }
                #endregion
                return;
            }
            catch (Exception e)
            {
                LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0} {1}", e.ToString(), e.StackTrace.ToString()));
                return;
            }
            finally
            {

            }
        }

        public Dictionary<string, string> GetTrackInWipData(LotModel lot)
        {
            Dictionary<string, string> List_wip = new Dictionary<string, string>();
            try
            {
                foreach (var v in lot.LotParameterList)
                {
                    if (v.ServiceName != "TrackInLot") continue;
                    EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByName(v.EqpID);
                    string tracevaule = string.Empty;
                    var vv = (from t in em.List_KeyTraceDateSpec where t.ItemID == v.ItemID select t).FirstOrDefault();
                    if (vv != null)
                    {
                        tracevaule = vv.Value;
                        List_wip.Add(v.ItemName, tracevaule);
                    }
                }
                return List_wip;
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0} {1}", ex.ToString(), ex.StackTrace.ToString()));
                return List_wip;
            }
        }

        private bool TimeoutCheck(int sec)
        {
            try
            {
                #region 确认上载具超时5分钟
                DateTime dt1 = DateTime.Now;
                DateTime dt2 = Environment.EAPEnvironment.commonLibrary.commonModel.InTrackInCheckTime;
                TimeSpan ts = dt1.Subtract(dt2);
                double second = ts.TotalSeconds;
                if (second >= sec)
                {
                    return true;
                }
                else
                {
                    return false;
                }
                #endregion
            }
            catch (Exception ex)
            {
                return true;
            }
        }

    }
}
