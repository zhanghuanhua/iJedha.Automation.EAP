using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Library;
using iJedha.Automation.EAP.Model;
using iJedha.Customized.MessageStructure;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Linq;
using System.Diagnostics;

namespace iJedha.Automation.EAP.Environment
{
    public partial class WCFReport : BaseComm
    {
        public void EAP_LoadComplete(LoadComplete _data)
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
                    iJedha.MES.WcfServiceInterface.ReturnInformation returnInfo = client.EAP_LoadComplete(_data.MainEqpID, _data.SubEqpID, _data.PortID, _data.CarriedID);

                    string _indata;
                    if (ConvertJSON(returnInfo, out _indata))
                        LogMsg(Log.LogLevel.Trace, string.Format("WCF Message<{0}> Recv OK, Content<{1}>", System.Reflection.MethodBase.GetCurrentMethod().Name, _indata));

                    EquipmentModel em = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByName(_data.SubEqpID);
                    if (returnInfo.CheckSuccessCode != "0")
                    {
                        string errMsg = string.Format("MES回复, 接口名称<{0}>, 错误代码<{1}>, 错误描述<{2}><{3}>", System.Reflection.MethodBase.GetCurrentMethod().Name, returnInfo.CheckSuccessCode, _data.SubEqpID, returnInfo.CheckErrorMsg);
                        Environment.BaseComm.ErrorHandleRule("E3007", errMsg);
                        //0:OK 1:NG
                        //new HSMSReport().S2F41(em, ConstLibrary.CONST_COMMAND_CarrierStart, new List<string> { "0", "", "", "" });//因收板机无法将载具退出，所以取消该命令--20191216，伟迪要求
                    }
                    else
                    {
                        if (em.Type == "L" && em.ControlMode == eControlMode.ONLINE_REMOTE) 
                        {
                            LotModel lot = em.GetLotModelByLotID(_data.PortID, EAPEnvironment.commonLibrary.commonModel.currentProcessLotID);
                            if (lot != null)
                            {
                                if (int.Parse(lot.StaticFirstInspQty) > 0)
                                    new Environment.HSMSReport().S2F41(em, ConstLibrary.CONST_COMMAND_TrailLotStart, new List<string> { "0", lot.LotID, lot.StaticFirstInspQty, lot.ProcessQty.ToString() });
                                else
                                    new Environment.HSMSReport().S2F41(em, ConstLibrary.CONST_COMMAND_CarrierStart, new List<string> { "0", lot.LotID, lot.ProcessQty.ToString(), lot.ProcessQty.ToString() });
                                LogMsg(Log.LogLevel.Info, string.Format("Lot<{0}>允许取板<{1}>.", lot.LotID, lot.ProcessQty.ToString()));
                            }
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
