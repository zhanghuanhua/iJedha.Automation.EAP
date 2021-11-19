//******************************************************************
//   系统名称 : iJedha.Automation.EAP.Rule
//   文件概要 : AliveCheck
//   作    者 : 张明哲
//   <更新履历>
//   1.0.0.0    2020/2/6 10:40:34
//******************************************************************
using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Environment;
using iJedha.Automation.EAP.Model;
using iJedha.Automation.EAP.WebAPI;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
/// <summary>
/// 定时触发功能：连线监控
/// </summary>
namespace iJedha.Automation.EAP.Rule
{
    public partial class AliveCheck
    {
        /// <summary>
        /// 1.定时发送EMS Alive Check
        /// 2.定时发送MES Alive Check
        /// 3.如线上有离线设备自动切Manual Mode
        /// 4.询问MES Mode
        /// </summary>
        /// <param name="_DowryObj"></param>
        public void Invoke(object _DowryObj)
        {
            try
            {
                #region [定时检查EAP与MQ连线健康情况]
                if (EAPEnvironment.MQPublisherAp != null
                    && EAPEnvironment.commonLibrary.baseLib.rbmqParaLibrary.Enable
                    && EAPEnvironment.commonLibrary.baseLib.rbmqParaLibrary.AliveCheckInterval != 0)
                {
                    //超时比对(秒）
                    if (EAPEnvironment.EAPAp.TimeComparisonSeconds(EAPEnvironment.commonLibrary.baseLib.rbmqParaLibrary.AliveCheckTime, EAPEnvironment.commonLibrary.baseLib.rbmqParaLibrary.AliveCheckInterval))
                    {
                        if (!EAPEnvironment.MQPublisherAp.MQ_AliveCheck())
                        {
                            EAPEnvironment.MQServiceStart();
                            EAPEnvironment.commonLibrary.MQConnectedStatus = false;
                            if (EAPEnvironment.MQPublisherAp != null)
                            {
                                EAPEnvironment.MQPublisherAp.Initial();

                                EAPEnvironment.MQPublisherAp.Open();
                            }
                            BaseComm.LogMsg(Log.LogLevel.Info, "MQ服务器重新连接.");
                        }
                        else
                        {
                            EAPEnvironment.commonLibrary.MQConnectedStatus = true;
                            //定时推送连线状态，供GUI显示
                            EAPEnvironment.MQPublisherAp.MQ_StatusCheck(MQ_StatusData());
                            //定时推送Lot信息，供GUI显示
                            EAPEnvironment.MQPublisherAp.MQ_LotInfoStatus(Environment.EAPEnvironment.commonLibrary.MQ_LotInfoStatus());
                            //定时推送设备状态，供GUI显示
                            EAPEnvironment.MQPublisherAp.MQ_EquipmentStatus(Environment.EAPEnvironment.commonLibrary.MQ_EquipmentStatus());
                        }

                        EAPEnvironment.commonLibrary.baseLib.rbmqParaLibrary.AliveCheckTime = DateTime.Now;
                    }
                }
                #endregion

                #region [定时检查EAP与MES连线健康情况]
                if (EAPEnvironment.commonLibrary.isWebServerStart == true
                    && EAPEnvironment.commonLibrary.baseLib.apiClientParaLibrary.Client_Enable
                    && EAPEnvironment.commonLibrary.baseLib.apiClientParaLibrary.AliveCheckInterval != 0)
                {
                    //到达AliveCheckInterval设定时间，向MES请求连线情况
                    if (EAPEnvironment.EAPAp.TimeComparisonSeconds(EAPEnvironment.commonLibrary.baseLib.apiClientParaLibrary.AliveCheckTime, EAPEnvironment.commonLibrary.baseLib.apiClientParaLibrary.AliveCheckInterval))
                    {
                        new WebAPIReport().EAP_AliveCheckRequest(new MessageModel.AliveCheckRequest()
                        {
                            MainEqpID = EAPEnvironment.commonLibrary.MDLN,
                            IPAddress = EAPEnvironment.commonLibrary.baseLib.apiServerParaLibrary.LocalIP
                        }, 1);
                        EAPEnvironment.commonLibrary.baseLib.apiClientParaLibrary.AliveCheckTime = DateTime.Now;
                    }
                }
                #endregion

                #region [定时检查EAP与设备连线健康情况]
                string err = string.Empty;
                if (EAPEnvironment.commonLibrary.customizedLibrary.EQPAliveCheckTime != 0)
                {
                    //到达EQPAliveCheckTime设定时间，向EQP请求连线情况
                    if (EAPEnvironment.EAPAp.TimeComparisonSeconds(EAPEnvironment.commonLibrary.customizedLibrary.EQPAliveTime, EAPEnvironment.commonLibrary.customizedLibrary.EQPAliveCheckTime))
                    {
                        foreach (var v in EAP.Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
                        {
                            if (v.ConnectMode == eConnectMode.CONNECT)
                            {
                                //new HostService.HostService().S1F1_H(v);
                            }
                        }
                        EAPEnvironment.commonLibrary.customizedLibrary.EQPAliveTime = DateTime.Now;
                    }
                }
                #endregion

                #region [定时检查AMS健康情况]
                if (EAPEnvironment.commonLibrary.customizedLibrary.AMSAliveCheckTime != 0)
                {
                    if (EAPEnvironment.EAPAp.TimeComparisonSeconds(EAPEnvironment.commonLibrary.customizedLibrary.AMSAliveTime, EAPEnvironment.commonLibrary.customizedLibrary.AMSAliveCheckTime))
                    {
                        Environment.EAPEnvironment.AMSServiceAp.CheckAlive(out err);
                        EAPEnvironment.commonLibrary.customizedLibrary.AMSAliveTime = DateTime.Now;
                    }
                }
                #endregion

                if (Environment.EAPEnvironment.commonLibrary.HostConnectMode == LibraryBase.eHostConnectMode.CONNECT)
                {
                    #region [如线上有离线设备自动切Manual Mode]
                    //foreach (var v in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
                    //{
                    //    if (v.ConnectMode == eConnectMode.DISCONNECT || (v.ControlMode != eControlMode.REMOTE))
                    //    {
                    //        if (Environment.EAPEnvironment.commonLibrary.commonModel.CurMode != eProductMode.Manual)
                    //        {
                    //            new WebAPIReport().EAP_EqpModeRequest(new MessageModel.EqpModeRequest()
                    //            {
                    //                MainEqpID = Environment.EAPEnvironment.commonLibrary.LineName
                    //            }, 1);
                    //        }
                    //    }
                    //}
                    #endregion

                    #region  [询问MES Mode]
                    //定时询问MES Mode
                    if (EAPEnvironment.EAPAp.TimeComparisonSeconds(EAPEnvironment.commonLibrary.commonModel.EqpModeRequestTime, EAPEnvironment.commonLibrary.customizedLibrary.EqpModeRequestTime))
                    {
                        EAPEnvironment.commonLibrary.commonModel.EqpModeRequestTime = DateTime.Now;
                        //new WebAPIReport().EAP_EqpModeRequest(new MessageModel.EqpModeRequest()
                        //{
                        //    MainEqpID = Environment.EAPEnvironment.commonLibrary.MDLN
                        //}, 1);
                    }
                    #endregion
                }
                return;
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
        /// <summary>
        /// EAP发送基本信息
        /// </summary>
        /// <returns></returns>
        public SortedList<string, string> MQ_StatusData()
        {
            SortedList<string, string> _sortedList = new SortedList<string, string>();
            try
            {
                foreach (var v in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
                {
                    _sortedList.Add(v.EQName, v.ConnectMode.ToString() + "/" + v.ControlMode.ToString());
                }
                _sortedList.Add("生产模式", Environment.EAPEnvironment.commonLibrary.commonModel.CurMode.ToString());
                _sortedList.Add("EAP版本", Application.ProductVersion);
                _sortedList.Add("Log存储量", Environment.EAPEnvironment.commonLibrary.logSize.ToString() + "/" + Environment.EAPEnvironment.commonLibrary.logLimitSize.ToString());

                return _sortedList;
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return _sortedList;
            }
        }

    }
}
