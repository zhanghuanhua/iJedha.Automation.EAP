//******************************************************************
//   系统名称 : iJedha.Automation.EAP.Rule
//   文件概要 : SocketMsgHandle
//   作    者 : 张明哲
//   <更新履历>
//   1.0.0.0    2020/10/11 14:00:14
//******************************************************************

using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Environment;
using iJedha.Automation.EAP.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace iJedha.Automation.EAP.Rule
{
    public partial class SocketMsgHandle
    {
        public void Invoke(object _DowryObj)
        {
            try
            {
                foreach (EquipmentModel em in EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
                {
                    if (em.SocketMsgBuffer.Count == 0) continue;
                    //List<byte>  currentData=new List<byte>();
                    //if (em.qSocketMsgBuffer.TryDequeue(out currentData))
                    //{
                    //    int startIdx = currentData.IndexOf((byte)Extensions.C_HEAD_CODE);
                    //    int endIdx = currentData.LastIndexOf((byte)Extensions.C_TAIL_CODE);

                    //    if (endIdx > startIdx && startIdx >= 0 && endIdx >= 0)
                    //    {
                    //        byte[] recvCmd = new byte[endIdx - startIdx + 1];      // Receive Command 包含StartCode & EndCode
                    //        currentData.CopyTo(startIdx, recvCmd, 0, endIdx - startIdx + 1);
                    //        Task.Run(() => new EQPService.EQPService().HandleSocketEvent(recvCmd));                            
                    //    }

                    //}
                    int startIdx = em.SocketMsgBuffer.IndexOf((byte)Extensions.C_HEAD_CODE);
                    int endIdx = em.SocketMsgBuffer.IndexOf((byte)Extensions.C_TAIL_CODE);
                    lock (em.SocketMsgBuffer)
                    {
                        if (endIdx > startIdx && startIdx >= 0 && endIdx >= 0)
                        {
                            /*
                            byte[] recvCmd = new byte[endIdx - startIdx - 1];      // Receive Command 不包含StartCode & EndCode
                            EAPEnvironment.commonLibrary.SocketMsgBuffer.CopyTo(startIdx + 1, recvCmd, 0, endIdx - startIdx - 1);
                            */

                            byte[] recvCmd = new byte[endIdx - startIdx + 1];      // Receive Command 包含StartCode & EndCode
                            em.SocketMsgBuffer.CopyTo(startIdx, recvCmd, 0, endIdx - startIdx + 1);

                            Task.Run(() => new EQPService.EQPService().HandleSocketEvent(recvCmd));

                            em.SocketMsgBuffer.RemoveRange(startIdx, endIdx - startIdx + 1);
                        }
                        else if (startIdx > endIdx && startIdx >= 0 && endIdx >= 0)
                        {
                            em.SocketMsgBuffer.RemoveRange(0, startIdx);
                        }
                        else if (startIdx == -1)
                        {
                            em.SocketMsgBuffer.Clear();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Core.Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return;
            }
            finally
            {

            }
        }
    }
}
