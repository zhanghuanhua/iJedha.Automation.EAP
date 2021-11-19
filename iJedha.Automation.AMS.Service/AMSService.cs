using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace iJedha.Automation.AMS.Service
{
    public class AMSService
    {
        public class cInfoParam
        {
            public string ParameterName;
            public string ParameterValue;
        }

        public AMS_WebIF _webInfo = new AMS_WebIF();

        private string _url;
        /// <summary>
        /// 设定web service URL 
        /// </summary>
        public string URL
        {
            get
            {
                return _url;
            }
            set
            {
                _url = value;
                _webInfo.Url = _url;
            }
        }

        public AMSService(string url)
        {
            _url = url;
            _webInfo.Url = _url;
        }

        #region WebService's Interface
        /// <summary>
        /// 警報發送 / Trigger the Alarm (Record the alarm to be sent)
        /// </summary>
        /// <param name="factoryID">廠別</param>
        /// <param name="subSystemID">子系統</param>
        /// <param name="eqpID">機台代碼</param>
        /// <param name="alarmInfoID">警報代碼</param>
        /// <param name="alarmSubject">警報標題</param>
        /// <param name="alarmContent">警報詳細內容</param>
        /// <param name="alarmContentType">警報訊息內容類型</param>
        /// <param name="receiveType">資料取得方式</param>
        /// <param name="filterParam">事件篩選條件</param>
        /// <param name="infoParam">事件訊息參數</param>
        /// <param name="oErrMsg">回傳錯誤訊息</param>
        /// <returns></returns>
        public bool AlarmSendWithParam(string factoryID, string subSystemID, string eqpID, string alarmInfoID
                                        , string alarmSubject, string alarmContent, string alarmContentType
                                        , string receiveType, List<cInfoParam> filterParam, List<cInfoParam> infoParam
                                        , out string oErrMsg)
        {
            AlarmSendWithParamMessage param;
            Dictionary<string, string> reqFilterParams = new Dictionary<string, string>();
            List<cInfoParam> reqInfoParams = new List<cInfoParam>();
            string strFilterParams = string.Empty, strInfoParams = string.Empty;

            oErrMsg = string.Empty;
            try
            {
                if (filterParam != null && filterParam.Count > 0)
                {
                    for (int i = 0; i < filterParam.Count; i++)
                    {
                        if (!reqFilterParams.ContainsKey(filterParam[i].ParameterName))
                        {
                            reqFilterParams.Add(filterParam[i].ParameterName, filterParam[i].ParameterValue);
                        }
                    }
                }
                if (infoParam != null && infoParam.Count > 0)
                {
                    for (int i = 0; i < infoParam.Count; i++)
                    {
                        reqInfoParams.Add(new cInfoParam() { ParameterName = infoParam[i].ParameterName, ParameterValue = infoParam[i].ParameterValue });
                    }
                }
                strFilterParams = JsonConvert.SerializeObject(reqFilterParams);
                strInfoParams = JsonConvert.SerializeObject(reqInfoParams);

                param = new AlarmSendWithParamMessage()
                {
                    FactoryID = factoryID,
                    SubSystemID = subSystemID,
                    EqpID = eqpID,
                    AlarmInfoID = alarmInfoID,
                    AlarmSubject = alarmSubject,
                    AlarmContent = alarmContent,
                    AlarmContentType = alarmContentType,
                    ReceiveType = receiveType,
                    FilterParam = strFilterParams,
                    InfoParam = strInfoParams,
                };

                ReturnMessage rtn = _webInfo.AlarmSendWithParam(param);
                if(rtn.returnStatus == ReturnStatus.OK)
                    return true;
                else
                {
                    oErrMsg = $"Method<{System.Reflection.MethodBase.GetCurrentMethod().Name}> AMS return NG. RtnCode=[{rtn.ReturnCode}] RtnMsg=[{rtn.ReturnErrorMessage}].";
                    return false;
                }
            }
            catch (Exception ex)
            {
                oErrMsg = $"Method<{System.Reflection.MethodBase.GetCurrentMethod().Name}> Exception=[{ex.ToString()}].";
                return false;
            }
        }

        /// <summary>
        /// 警報中止 / Terminate an Alarm Message
        /// </summary>
        /// <param name="factoryID">廠別</param>
        /// <param name="subSystemID">子系統</param>
        /// <param name="eqpID">機台代碼</param>
        /// <param name="alarmInfoID">警報代碼</param>
        /// <param name="filterParam">事件篩選條件</param>
        /// <param name="oErrMsg">回傳錯誤訊息</param>
        /// <returns></returns>
        public bool AlarmTerminate(string factoryID, string subSystemID, string eqpID, string alarmInfoID
                                    , List<cInfoParam> filterParam
                                    , out string oErrMsg)
        {
            AlarmTerminateInfo param;
            Dictionary<string, string> reqFilterParams = new Dictionary<string, string>();
            string strFilterParams = string.Empty;

            oErrMsg = string.Empty;
            try
            {
                if (filterParam != null && filterParam.Count > 0)
                {
                    for (int i = 0; i < filterParam.Count; i++)
                    {
                        if (!reqFilterParams.ContainsKey(filterParam[i].ParameterName))
                        {
                            reqFilterParams.Add(filterParam[i].ParameterName, filterParam[i].ParameterValue);
                        }
                    }
                }
                strFilterParams = JsonConvert.SerializeObject(reqFilterParams);

                param = new AlarmTerminateInfo()
                {
                    FactoryID = factoryID,
                    SubSystemID = subSystemID,
                    EqpID = eqpID,
                    AlarmInfoID = alarmInfoID,
                    FilterParam = strFilterParams,
                };

                ReturnMessage rtn = _webInfo.AlarmTerminate(param);
                if (rtn.returnStatus == ReturnStatus.OK)
                    return true;
                else
                {
                    oErrMsg = $"Method<{System.Reflection.MethodBase.GetCurrentMethod().Name}> AMS return NG. RtnCode=[{rtn.ReturnCode}] RtnMsg=[{rtn.ReturnErrorMessage}].";
                    return false;
                }
            }
            catch (Exception ex)
            {
                oErrMsg = $"Method<{System.Reflection.MethodBase.GetCurrentMethod().Name}> Exception=[{ex.ToString()}].";
                return false;
            }
        }

        /// <summary>
        /// 檢查AMS是否存活 / Check Web Interface is Alive or not
        /// </summary>
        /// <param name="oErrMsg">回傳錯誤訊息</param>
        /// <returns></returns>
        public bool CheckAlive(out string oErrMsg)
        { 
            bool rtn = false;

            oErrMsg = string.Empty;
            try
            {
                rtn = _webInfo.CheckAlive();
                return rtn;
            }
            catch (Exception ex)
            {
                oErrMsg = $"Method<{System.Reflection.MethodBase.GetCurrentMethod().Name}> Exception=[{ex.ToString()}].";
                return false;
            }
        }
        #endregion
    }
}
