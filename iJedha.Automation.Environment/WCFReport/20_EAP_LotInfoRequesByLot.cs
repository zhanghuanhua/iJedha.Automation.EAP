using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Library;
using iJedha.Automation.EAP.Model;
using iJedha.Automation.EAP.ModelBase;
using iJedha.Customized.MessageStructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace iJedha.Automation.EAP.Environment
{
    public partial class WCFReport : BaseComm
    {
        public void EAP_LotInfoRequestByLot(LotInfoRequestByLot _data, EquipmentModel em)
        {
            iJedha.MES.WcfServiceInterface.IWcfServiceContract client = null;
            try
            {
                if (EAPEnvironment.commonLibrary.HostConnectMode == LibraryBase.eHostConnectMode.DISCONNECT)
                {
                    return;
                }

                string _outdata;
                if (ConvertJSON(_data, out _outdata))
                    LogMsg(Log.LogLevel.Trace, string.Format("WCF Message<{0}> Send OK, Content<{1}>", System.Reflection.MethodBase.GetCurrentMethod().Name, _outdata));

                using (ChannelFactory<iJedha.MES.WcfServiceInterface.IWcfServiceContract> channel = new ChannelFactory<iJedha.MES.WcfServiceInterface.IWcfServiceContract>(new NetTcpBinding(SecurityMode.None), new EndpointAddress(EAPEnvironment.commonLibrary.baseLib.wcfParaLibrary.RemoteUrlString)))
                {
                    client = channel.CreateChannel();
                    iJedha.MES.WcfServiceInterface.LotInformation returnInfo = client.EAP_LotInfoRequestByLot(_data.MainEqpID, _data.SubEqpID, _data.PortID, _data.LotID);

                    //#region[测试使用]
                    //returnInfo.CarriedID = "B080000001";
                    //returnInfo.CheckErrorMsg = "No Error!";
                    //returnInfo.CheckSuccessCode = "0";
                    //returnInfo.MainEqpID = "叠板回流#1";
                    //returnInfo.InputCarrierFamily = "Box";
                    //returnInfo.LotStatus = "5";
                    //returnInfo.OutputCarrierFamily = "Box";
                    //returnInfo.OutputCarrierMaxQty = "60";
                    //returnInfo.PN = "M510VA01041MQEA";
                    //returnInfo.PanelTotalQty = "3";
                    //returnInfo.PortID = "L01";
                    //returnInfo.ProcessQty = "3";
                    //returnInfo.ProductList = new iJedha.MES.WcfServiceInterface.LotInformation.ProductDataList();
                    //returnInfo.SubEqpList = new iJedha.MES.WcfServiceInterface.LotInformation.SubEqpDataList();
                    //returnInfo.WIPDataDataList = new iJedha.MES.WcfServiceInterface.LotInformation.WIPDataDataInfoList();
                    //#endregion

                    string _indata;
                    if (ConvertJSON(returnInfo, out _indata))
                        LogMsg(Log.LogLevel.Trace, string.Format("WCF Message<{0}> Recv OK, Content<{1}>", System.Reflection.MethodBase.GetCurrentMethod().Name, _indata));

                    if (returnInfo.CheckSuccessCode != "0")
                    {
                        string errMsg = string.Format("MES回复, 接口名称<{0}>, 错误代码<{1}>, 错误描述<{2}><{3}>", System.Reflection.MethodBase.GetCurrentMethod().Name, returnInfo.CheckSuccessCode, _data.SubEqpID, returnInfo.CheckErrorMsg);
                        Environment.BaseComm.ErrorHandleRule("E3002", errMsg);
                        //0:OK 1:NG
                        new HSMSReport().S2F41(em, ConstLibrary.CONST_COMMAND_LotStart, new List<string> { "1" });
                    }
                    else
                    {
                        //取得当前Lot ID
                        EAPEnvironment.commonLibrary.commonModel.currentProcessLotID = returnInfo.LotID;
                        if (em.ControlMode == eControlMode.ONLINE_REMOTE)
                        {
                            LogMsg(Core.Log.LogLevel.Info, string.Format("Lot<{0}>产品<{1}>数量<{2}>帐料参数检查成功.", _data.LotID, returnInfo.PN, returnInfo.PanelTotalQty));
                            new HSMSReport().S2F41(em, ConstLibrary.CONST_COMMAND_LotStart, new List<string> { "0" });
                        }
                    }
                }
            }
            catch (TimeoutException e)
            {
                string errMsg = string.Format("MES回复，接口名称<{0}>超时.", System.Reflection.MethodBase.GetCurrentMethod().Name);
                Environment.BaseComm.ErrorHandleRule("E1003", errMsg);
                LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0} {1}", e.ToString(), e.StackTrace.ToString()));
            }
            catch (FaultException e)
            {
                string errMsg = string.Format("MES回复，接口名称<{0}>, 错误代码<{1}>, 错误描述<{2}>", System.Reflection.MethodBase.GetCurrentMethod().Name, e.HResult, e.Message);
                Environment.BaseComm.ErrorHandleRule("E1004", errMsg);
                LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0} {1}", e.ToString(), e.StackTrace.ToString()));
            }
            catch (CommunicationException e)
            {
                string errMsg = string.Format("MES回复，接口名称<{0}>, 错误代码<{1}>, 错误描述<{2}>", System.Reflection.MethodBase.GetCurrentMethod().Name, e.HResult, e.Message);
                Environment.BaseComm.ErrorHandleRule("E1004", errMsg);
                (client as ICommunicationObject).Abort();
                Environment.EAPEnvironment.WcfClient_MES.reStart();
                LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0} {1}", e.ToString(), e.StackTrace.ToString()));
            }
            catch (Exception e)
            {
                StackTrace st = new StackTrace(new StackFrame(true));
                StackFrame sf = st.GetFrame(0);
                string errMsg = string.Format("EAP发送<{0}>异常：程序出错，代码行数<{1}>，ErrMsg<{2}>",
                    System.Reflection.MethodBase.GetCurrentMethod().Name, sf.GetFileLineNumber(), e.Message);
                Environment.BaseComm.ErrorHandleRule("E0001", errMsg);
                LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0} {1}", e.ToString(), e.StackTrace.ToString()));
            }
        }
    }
}
