//******************************************************************
//   系统名称 : iJedha.Automation.EAP.Rule
//   文件概要 : PPSelectCheck
//   作    者 : 张明哲
//   <更新履历>
//   1.0.0.0    2020/2/6 10:40:34
//******************************************************************
using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Environment;
using System;
using System.Linq;
/// <summary>
/// 触发功能：
/// </summary>
namespace iJedha.Automation.EAP.Rule
{
    public partial class SocketReconnect
    {
        /// <summary>
        /// Socket Client重连
        /// </summary>
        /// <param name="_DowryObj"></param>
        public void Invoke(object _DowryObj)
        {
            //获取未连线的设备列表
            var ems = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.ConnectMode == iJedha.Automation.EAP.Model.eConnectMode.DISCONNECT).ToList();
            //检查重连时间是否达到设定重连时间
            if (EAPEnvironment.EAPAp.TimeComparisonSeconds(EAPEnvironment.commonLibrary.commonModel.InSocketReconnectCheckTime, EAPEnvironment.commonLibrary.reconnectSec))
            {
                EAPEnvironment.commonLibrary.commonModel.InSocketReconnectCheckTime = DateTime.Now;
                foreach (var v in ems)
                {
                    socket.SocketBasic socket = null;
                    try
                    {
                        //通过设备ID获取Socket连线信息
                        socket = Environment.EAPEnvironment.Dic_TCPSocketAp[v.EQID];
                        lock (socket)
                        {
                            if (socket == null) return;
                            v.SetConnectCount += 1;
                            //Socket重连方法
                            if (Environment.EAPEnvironment.commonLibrary.GetSocketCommunicationStatus(v.socketType, socket)) return;
                            socket.Close();
                            socket.Run();
                        }
                    }
                    catch (Exception ex)
                    {
                        Environment.BaseComm.LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                    }
                    finally
                    {
                        if (Environment.EAPEnvironment.commonLibrary.GetSocketCommunicationStatus(v.socketType, socket) == false)
                        {
                            //重连次数>20此，重连时间改成10s
                            if (v.SetConnectCount > 20)
                            {
                                EAPEnvironment.commonLibrary.reconnectSec = 10;
                            }
                            //重连次数>50此，重连时间改成30s
                            if (v.SetConnectCount > 50)
                            {
                                EAPEnvironment.commonLibrary.reconnectSec = 30;
                            }
                        }
                    }
                }
            }

        }
    }
}
