using iJedha.Automation.EAP.Environment;
using iJedha.Automation.EAP.Library;
using iJedha.Automation.EAP.ModelBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace iJedha.Automation.EAP.UI
{
    public partial class WebAPIServerTest : Form
    {
        public ErrorCodeModelBase errm = new ErrorCodeModelBase();
        public WebAPIServerTest()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        private void MES_TrackInInfo_Click(object sender, EventArgs e)
        {
            try
            {
                MES_TrackInInfo(1);
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public void MES_TrackInInfo(int Retrytime)
        {
            try
            {
                var Client = new APIClient.Client();
                Client.Url = string.Format("{0}/api/MES_TrackInInfo/MES_TrackInInfo", EAPEnvironment.commonLibrary.webAPIParaLibrary.LocalUrlString);
                Client.Prams.Add("From", "EAP");
                Client.Prams.Add("Message", "");
                Client.Prams.Add("DateTime", "");
                Client.Prams.Add("Content", "");
                Client.Post();
                string _indata = Client.Result;
            }
            catch (Exception e)
            {
                string errMsg;
                if (e.Message.ToString() == ConstLibrary.CONST_WEBAPI_TIMEOUT_KEYWORD)
                {
                    errMsg = string.Format("第{0}次发送<{1}>超时", Retrytime, System.Reflection.MethodBase.GetCurrentMethod().Name);
                    BaseComm.LogMsg(EAP.Core.Log.LogLevel.Warn, errMsg);
                    if (Retrytime < EAPEnvironment.commonLibrary.webAPIParaLibrary.RemoteRetrytime)
                    {
                        Retrytime++;
                        MES_EqpReleaseReport(Retrytime);
                    }
                    else
                    {
                        errMsg = string.Format("EAP发送<{0}>失败", System.Reflection.MethodBase.GetCurrentMethod().Name);
                        BaseComm.LogMsg(EAP.Core.Log.LogLevel.Fatal, errMsg);
                    }
                }
                else
                {
                    StackTrace st = new StackTrace(new StackFrame(true));
                    StackFrame sf = st.GetFrame(0);
                    errMsg = string.Format("EAP发送<{0}>异常：程序出错，代码行数<{1}>，ErrMsg<{2}>",
                    System.Reflection.MethodBase.GetCurrentMethod().Name, sf.GetFileLineNumber(), e.Message);
                    BaseComm.ErrorHandleRule("E0001", errMsg, ref errm);
                }
            }
        }
        private void MES_TrackOutInfo_Click(object sender, EventArgs e)
        {
            try
            {
                MES_TrackOutInfo(1);
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public void MES_TrackOutInfo(int Retrytime)
        {
            try
            {
                var Client = new APIClient.Client();
                Client.Url = string.Format("{0}/api/MES_TrackOutInfo/MES_TrackOutInfo", EAPEnvironment.commonLibrary.webAPIParaLibrary.LocalUrlString);
                Client.Prams.Add("From", "EAP");
                Client.Prams.Add("Message", "");
                Client.Prams.Add("DateTime", "");
                Client.Prams.Add("Content", "");
                Client.Post();
                string _indata = Client.Result;
            }
            catch (Exception e)
            {
                string errMsg;
                if (e.Message.ToString() == ConstLibrary.CONST_WEBAPI_TIMEOUT_KEYWORD)
                {
                    errMsg = string.Format("第{0}次发送<{1}>超时", Retrytime, System.Reflection.MethodBase.GetCurrentMethod().Name);
                    BaseComm.LogMsg(EAP.Core.Log.LogLevel.Warn, errMsg);
                    if (Retrytime < EAPEnvironment.commonLibrary.webAPIParaLibrary.RemoteRetrytime)
                    {
                        Retrytime++;
                        MES_TrackOutInfo(Retrytime);
                    }
                    else
                    {
                        errMsg = string.Format("EAP发送<{0}>失败", System.Reflection.MethodBase.GetCurrentMethod().Name);
                        BaseComm.LogMsg(EAP.Core.Log.LogLevel.Fatal, errMsg);
                    }
                }
                else
                {
                    StackTrace st = new StackTrace(new StackFrame(true));
                    StackFrame sf = st.GetFrame(0);
                    errMsg = string.Format("EAP发送<{0}>异常：程序出错，代码行数<{1}>，ErrMsg<{2}>",
                    System.Reflection.MethodBase.GetCurrentMethod().Name, sf.GetFileLineNumber(), e.Message);
                    BaseComm.ErrorHandleRule("E0001", errMsg, ref errm);
                }
            }
        }
        private void MES_ModeChangeRequest_Click(object sender, EventArgs e)
        {
            try
            {
                MES_ModeChangeRequest(1);
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public void MES_ModeChangeRequest(int Retrytime)
        {
            try
            {
                var Client = new APIClient.Client();
                Client.Url = string.Format("{0}/api/MES_ModeChangeRequest/MES_ModeChangeRequest", EAPEnvironment.commonLibrary.webAPIParaLibrary.LocalUrlString);
                Client.Prams.Add("From", "EAP");
                Client.Prams.Add("Message", "");
                Client.Prams.Add("DateTime", "");
                Client.Prams.Add("Content", "");
                Client.Post();
                string _indata = Client.Result;
            }
            catch (Exception e)
            {
                string errMsg;
                if (e.Message.ToString() == ConstLibrary.CONST_WEBAPI_TIMEOUT_KEYWORD)
                {
                    errMsg = string.Format("第{0}次发送<{1}>超时", Retrytime, System.Reflection.MethodBase.GetCurrentMethod().Name);
                    BaseComm.LogMsg(EAP.Core.Log.LogLevel.Warn, errMsg);
                    if (Retrytime < EAPEnvironment.commonLibrary.webAPIParaLibrary.RemoteRetrytime)
                    {
                        Retrytime++;
                        MES_TrackOutInfo(Retrytime);
                    }
                    else
                    {
                        errMsg = string.Format("EAP发送<{0}>失败", System.Reflection.MethodBase.GetCurrentMethod().Name);
                        BaseComm.LogMsg(EAP.Core.Log.LogLevel.Fatal, errMsg);
                    }
                }
                else
                {
                    StackTrace st = new StackTrace(new StackFrame(true));
                    StackFrame sf = st.GetFrame(0);
                    errMsg = string.Format("EAP发送<{0}>异常：程序出错，代码行数<{1}>，ErrMsg<{2}>",
                    System.Reflection.MethodBase.GetCurrentMethod().Name, sf.GetFileLineNumber(), e.Message);
                    BaseComm.ErrorHandleRule("E0001", errMsg, ref errm);
                }
            }
        }
        private void MES_EqpHoldReport_Click(object sender, EventArgs e)
        {
            try
            {
                MES_EqpHoldReport(1);
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public void MES_EqpHoldReport(int Retrytime)
        {
            try
            {
                var Client = new APIClient.Client();
                Client.Url = string.Format("{0}/api/MES_EqpHoldReport/MES_EqpHoldReport", EAPEnvironment.commonLibrary.webAPIParaLibrary.LocalUrlString);
                Client.Prams.Add("From", "EAP");
                Client.Prams.Add("Message", "");
                Client.Prams.Add("DateTime", "");
                Client.Prams.Add("Content", "");
                Client.Post();
                string _indata = Client.Result;
            }
            catch (Exception e)
            {
                string errMsg;
                if (e.Message.ToString() == ConstLibrary.CONST_WEBAPI_TIMEOUT_KEYWORD)
                {
                    errMsg = string.Format("第{0}次发送<{1}>超时", Retrytime, System.Reflection.MethodBase.GetCurrentMethod().Name);
                    BaseComm.LogMsg(EAP.Core.Log.LogLevel.Warn, errMsg);
                    if (Retrytime < EAPEnvironment.commonLibrary.webAPIParaLibrary.RemoteRetrytime)
                    {
                        Retrytime++;
                        MES_EqpHoldReport(Retrytime);
                    }
                    else
                    {
                        errMsg = string.Format("EAP发送<{0}>失败", System.Reflection.MethodBase.GetCurrentMethod().Name);
                        BaseComm.LogMsg(EAP.Core.Log.LogLevel.Fatal, errMsg);
                    }
                }
                else
                {
                    StackTrace st = new StackTrace(new StackFrame(true));
                    StackFrame sf = st.GetFrame(0);
                    errMsg = string.Format("EAP发送<{0}>异常：程序出错，代码行数<{1}>，ErrMsg<{2}>",
                    System.Reflection.MethodBase.GetCurrentMethod().Name, sf.GetFileLineNumber(), e.Message);
                    BaseComm.ErrorHandleRule("E0001", errMsg, ref errm);
                }
            }
        }
        private void MES_EqpReleaseReport_Click(object sender, EventArgs e)
        {
            try
            {
                MES_EqpReleaseReport(1);
            }
            catch (Exception)
            {

                throw;
            }
        }
        public void MES_EqpReleaseReport(int Retrytime)
        {
            try
            {
                var Client = new APIClient.Client();
                Client.Url = string.Format("{0}/api/MES_EqpReleaseReport/MES_EqpReleaseReport", EAPEnvironment.commonLibrary.webAPIParaLibrary.LocalUrlString);
                Client.Prams.Add("From", "EAP");
                Client.Prams.Add("Message", "");
                Client.Prams.Add("DateTime", "");
                Client.Prams.Add("Content", "");
                Client.Post();
                string _indata = Client.Result;
            }
            catch (Exception e)
            {
                string errMsg;
                if (e.Message.ToString() == ConstLibrary.CONST_WEBAPI_TIMEOUT_KEYWORD)
                {
                    errMsg = string.Format("第{0}次发送<{1}>超时", Retrytime, System.Reflection.MethodBase.GetCurrentMethod().Name);
                    BaseComm.LogMsg(EAP.Core.Log.LogLevel.Warn, errMsg);
                    if (Retrytime < EAPEnvironment.commonLibrary.webAPIParaLibrary.RemoteRetrytime)
                    {
                        Retrytime++;
                        MES_EqpReleaseReport(Retrytime);
                    }
                    else
                    {
                        errMsg = string.Format("EAP发送<{0}>失败", System.Reflection.MethodBase.GetCurrentMethod().Name);
                        BaseComm.LogMsg(EAP.Core.Log.LogLevel.Fatal, errMsg);
                    }
                }
                else
                {
                    StackTrace st = new StackTrace(new StackFrame(true));
                    StackFrame sf = st.GetFrame(0);
                    errMsg = string.Format("EAP发送<{0}>异常：程序出错，代码行数<{1}>，ErrMsg<{2}>",
                    System.Reflection.MethodBase.GetCurrentMethod().Name, sf.GetFileLineNumber(), e.Message);
                    BaseComm.ErrorHandleRule("E0001", errMsg, ref errm);
                }
            }
        }
        private void MES_TransferModeRequest_Click(object sender, EventArgs e)
        {
            try
            {
                MES_TransferModeRequest(1);
            }
            catch (Exception ex)
            {//-2146233088

                throw;
            }
        }
        public void MES_TransferModeRequest(int Retrytime)
        {
            try
            {
                var Client = new APIClient.Client();
                Client.Url = string.Format("{0}/api/MES_TransferModeRequest/MES_TransferModeRequest", EAPEnvironment.commonLibrary.webAPIParaLibrary.LocalUrlString);
                Client.Prams.Add("From", "EAP");
                Client.Prams.Add("Message", "");
                Client.Prams.Add("DateTime", "");
                Client.Prams.Add("Content", "");
                Client.Post();
                string _indata = Client.Result;
            }
            catch (Exception e)
            {
                string errMsg;
                if (e.Message.ToString() == ConstLibrary.CONST_WEBAPI_TIMEOUT_KEYWORD)
                {
                    errMsg = string.Format("第{0}次发送<{1}>超时", Retrytime, System.Reflection.MethodBase.GetCurrentMethod().Name);
                    BaseComm.LogMsg(EAP.Core.Log.LogLevel.Warn, errMsg);
                    if (Retrytime < EAPEnvironment.commonLibrary.webAPIParaLibrary.RemoteRetrytime)
                    {
                        Retrytime++;
                        MES_TransferModeRequest(Retrytime);
                    }
                    else
                    {
                        errMsg = string.Format("EAP发送<{0}>失败", System.Reflection.MethodBase.GetCurrentMethod().Name);
                        BaseComm.LogMsg(EAP.Core.Log.LogLevel.Fatal, errMsg);
                    }
                }
                else
                {
                    StackTrace st = new StackTrace(new StackFrame(true));
                    StackFrame sf = st.GetFrame(0);
                    errMsg = string.Format("EAP发送<{0}>异常：程序出错，代码行数<{1}>，ErrMsg<{2}>",
                    System.Reflection.MethodBase.GetCurrentMethod().Name, sf.GetFileLineNumber(), e.Message);
                    BaseComm.ErrorHandleRule("E0001", errMsg, ref errm);
                }
            }
        }
        private void MES_FirstInspResult_Click(object sender, EventArgs e)
        {
            try
            {
                MES_FirstInspResult(1);
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public void MES_FirstInspResult(int Retrytime)
        {
            try
            {
                var Client = new APIClient.Client();
                Client.Url = string.Format("{0}/api/MES_FirstInspResult/MES_FirstInspResult", EAPEnvironment.commonLibrary.webAPIParaLibrary.LocalUrlString);
                Client.Prams.Add("From", "EAP");
                Client.Prams.Add("Message", "");
                Client.Prams.Add("DateTime", "");
                Client.Prams.Add("Content", "");
                Client.Post();
                string _indata = Client.Result;
            }
            catch (Exception e)
            {
                string errMsg;
                if (e.Message.ToString() == ConstLibrary.CONST_WEBAPI_TIMEOUT_KEYWORD)
                {
                    errMsg = string.Format("第{0}次发送<{1}>超时", Retrytime, System.Reflection.MethodBase.GetCurrentMethod().Name);
                    BaseComm.LogMsg(EAP.Core.Log.LogLevel.Warn, errMsg);
                    if (Retrytime < EAPEnvironment.commonLibrary.webAPIParaLibrary.RemoteRetrytime)
                    {
                        Retrytime++;
                        MES_FirstInspResult(Retrytime);
                    }
                    else
                    {
                        errMsg = string.Format("EAP发送<{0}>失败", System.Reflection.MethodBase.GetCurrentMethod().Name);
                        BaseComm.LogMsg(EAP.Core.Log.LogLevel.Fatal, errMsg);
                    }
                }
                else
                {
                    StackTrace st = new StackTrace(new StackFrame(true));
                    StackFrame sf = st.GetFrame(0);
                    errMsg = string.Format("EAP发送<{0}>异常：程序出错，代码行数<{1}>，ErrMsg<{2}>",
                    System.Reflection.MethodBase.GetCurrentMethod().Name, sf.GetFileLineNumber(), e.Message);
                    BaseComm.ErrorHandleRule("E0001", errMsg, ref errm);
                }
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            try
            {
                XGJ_GetTraceData(1);
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public void XGJ_GetTraceData(int Retrytime)
        {
            try
            {
                var Client = new APIClient.Client();
                Client.Url = string.Format("{0}/api/XGJ_GetTraceData/XGJ_GetTraceData", EAPEnvironment.commonLibrary.webAPIParaLibrary.LocalUrlString);
                Client.Prams.Add("From", "EAP");
                Client.Prams.Add("Message", "");
                Client.Prams.Add("DateTime", "");
                Client.Prams.Add("Content", "{\"SubEqpID\":\"H4-PNLYHJT-001\",\"Timeout\":\"10\"}");
                Client.Post();
                string _indata = Client.Result;
            }
            catch (Exception e)
            {
                string errMsg;
                if (e.Message.ToString() == ConstLibrary.CONST_WEBAPI_TIMEOUT_KEYWORD)
                {
                    errMsg = string.Format("第{0}次发送<{1}>超时", Retrytime, System.Reflection.MethodBase.GetCurrentMethod().Name);
                    BaseComm.LogMsg(EAP.Core.Log.LogLevel.Warn, errMsg);
                    if (Retrytime < EAPEnvironment.commonLibrary.webAPIParaLibrary.RemoteRetrytime)
                    {
                        Retrytime++;
                        XGJ_GetTraceData(Retrytime);
                    }
                    else
                    {
                        errMsg = string.Format("EAP发送<{0}>失败", System.Reflection.MethodBase.GetCurrentMethod().Name);
                        BaseComm.LogMsg(EAP.Core.Log.LogLevel.Fatal, errMsg);
                    }
                }
                else
                {
                    StackTrace st = new StackTrace(new StackFrame(true));
                    StackFrame sf = st.GetFrame(0);
                    errMsg = string.Format("EAP发送<{0}>异常：程序出错，代码行数<{1}>，ErrMsg<{2}>",
                    System.Reflection.MethodBase.GetCurrentMethod().Name, sf.GetFileLineNumber(), e.Message);
                    BaseComm.ErrorHandleRule("E0001", errMsg, ref errm);
                }
            }
        }
    }
}
