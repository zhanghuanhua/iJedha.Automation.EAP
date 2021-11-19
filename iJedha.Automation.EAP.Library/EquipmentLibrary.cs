//******************************************************************
//   系统名称 : iJedha.Automation.EAP.Library
//   文件概要 : EquipmentLibary
//   作    者 : 张明哲
//   <更新履历>
//   1.0.0.0    2020/2/6 10:40:34
//******************************************************************
using iJedha.Automation.EAP.LibraryBase;
using iJedha.Automation.EAP.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace iJedha.Automation.EAP.Library
{
    /// <summary>
    /// 设备模块库
    /// </summary>
    public class EquipmentLibary
    {
        /// <summary>
        /// Line模块集合
        /// </summary>
        private ConcurrentDictionary<string, LineModel> Dic_LineModel { get; set; }
        /// <summary>
        /// 设备模块集合
        /// </summary>
        private ConcurrentDictionary<string, EquipmentModel> Dic_EquipmentModel { get; set; }
        /// <summary>
        /// 设备HSMS参数集合
        /// </summary>
        private ConcurrentDictionary<string, HSMSParaLibraryBase> Dic_HSMSParaLibrary { get; set; }
        /// <summary>
        /// 设备Socket参数集合
        /// </summary>
        private ConcurrentDictionary<string, SocketParaLibraryBase> Dic_SocketParaLibrary { get; set; }
        /// <summary>
        /// 设备动态事件库集合
        /// </summary>
        private ConcurrentDictionary<string, Socket_DynamicLibraryBase> Dic_DynamicLibrary { get; set; }
        /// <summary>
        /// 构建
        /// </summary>
        public EquipmentLibary()
        {
            Dic_LineModel = new ConcurrentDictionary<string, LineModel>();
            Dic_EquipmentModel = new ConcurrentDictionary<string, EquipmentModel>();
            Dic_DynamicLibrary = new ConcurrentDictionary<string, Socket_DynamicLibraryBase>();
            Dic_HSMSParaLibrary = new ConcurrentDictionary<string, HSMSParaLibraryBase>();
            Dic_SocketParaLibrary = new ConcurrentDictionary<string, SocketParaLibraryBase>();
        }
        /// <summary>
        /// 新增设备模型
        /// </summary>
        /// <param name="eqp"></param>
        /// <returns></returns>
        public bool AddEquipmentModel(EquipmentModel eqp)
        {
            try
            {
                if (!Dic_EquipmentModel.ContainsKey(eqp.EQID))
                {
                    Dic_EquipmentModel.TryAdd(eqp.EQID, eqp);
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 新增Line模型
        /// </summary>
        /// <param name="eqp"></param>
        /// <returns></returns>
        public bool AddLineModel(LineModel linemod)
        {
            try
            {
                if (!Dic_LineModel.ContainsKey(linemod.LineName))
                {
                    Dic_LineModel.TryAdd(linemod.LineName, linemod);
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// 根据设备编号取得设备模型
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public EquipmentModel GetEquipmentModelByID(string id)
        {
            EquipmentModel ret = null;
            try
            {
                if (Dic_EquipmentModel.ContainsKey(id))
                {
                    ret = Dic_EquipmentModel[id];
                }

                return ret;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// 根据Line编号取得Line模型
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public LineModel GetLineModelByID(string id)
        {
            LineModel ret = null;
            try
            {
                if (Dic_LineModel.ContainsKey(id))
                {
                    ret = Dic_LineModel[id];
                }

                return ret;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public EquipmentModel GetEquipmentModelByIP(string ip)
        {
            try
            {
                return (from m in Dic_EquipmentModel.Values where m.EQID == 
                       (from n in Dic_SocketParaLibrary.Values where n.RemoteIP == ip select n.EQID).FirstOrDefault() select m).FirstOrDefault();

            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public EquipmentModel GetEquipmentModelByIP(string ip, int port)
        {
            try
            {
                if (port.ToString().Length==5 || port<=5000)
                {
                   
                    return (from m in Dic_EquipmentModel.Values
                            where m.EQID ==
                            (from n in Dic_SocketParaLibrary.Values where n.RemoteIP == ip select n.EQID).FirstOrDefault()
                            select m).FirstOrDefault();
                }
                else
                {
                    return (from m in Dic_EquipmentModel.Values
                            where m.EQID ==
                            (from n in Dic_SocketParaLibrary.Values where n.RemoteIP == ip && n.RemotePort == port select n.EQID).FirstOrDefault()
                            select m).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        /// <summary>
        /// 根据设备名称取得设备模型
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public EquipmentModel GetEquipmentModelByName(string name)
        {
            EquipmentModel ret = null;
            try
            {
                return (from n in Dic_EquipmentModel.Values where n.EQName == name select n).FirstOrDefault();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// 根据设备ID取得设备模型
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public EquipmentModel GetEquipmentModelByEqID(string ID)
        {
            EquipmentModel ret = null;
            try
            {
                return (from n in Dic_EquipmentModel.Values where n.EQID == ID select n).FirstOrDefault();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        /// <summary>
        /// 根据设备类型取得设备模型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public EquipmentModel GetEquipmentModelByType(eEquipmentType type)
        {
            EquipmentModel ret = (from n in Dic_EquipmentModel.Values where n.Type == type select n).FirstOrDefault();
            return ret;
        }

        /// <summary>
        /// 根据设备No取得设备模型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public EquipmentModel GetEquipmentModelByNo(int No)
        {
            EquipmentModel ret = (from n in Dic_EquipmentModel.Values where n.EQNo == No select n).FirstOrDefault();
            return ret;
        }
        /// <summary>
        /// 根据设备类型取得设备模型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public EquipmentModel GetEquipmentModelByLotID(string LotID)
        {
            EquipmentModel ret = (from n in Dic_EquipmentModel.Values
                                  from p in n.List_Port.Values
                                  from l in p.List_Lot.Values
                                  where l.LotID == LotID
                                  select n).FirstOrDefault();
            return ret;
        }
        /// <summary>
        /// 取得全部设备模型
        /// </summary>
        /// <returns></returns>
        public IList<EquipmentModel> GetAllEquipmentModel()
        {
            try
            {
                return Dic_EquipmentModel.Values.OrderBy(a => a.EQNo).ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        /// <summary>
        /// 检查设备模型是否存在
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool CheckEquipmentModelExist(string id)
        {
            try
            {
                if (!Dic_EquipmentModel.ContainsKey(id))
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// 新增HSMS配置
        /// </summary>
        /// <param name="hsms"></param>
        /// <returns></returns>
        public bool AddHSMSParaLibrary(HSMSParaLibraryBase hsms)
        {
            try
            {
                if (!Dic_HSMSParaLibrary.ContainsKey(hsms.EQID))
                {
                    Dic_HSMSParaLibrary.TryAdd(hsms.EQID, hsms);
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// 获取"HSMSParaLibrary"设定
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public HSMSParaLibraryBase GetHSMSParaLibrary(string id)
        {
            HSMSParaLibraryBase ret = null;
            try
            {
                if (Dic_HSMSParaLibrary.ContainsKey(id))
                {
                    ret = Dic_HSMSParaLibrary[id];
                }

                return ret;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        /// <summary>
        /// 新增Socket配置
        /// </summary>
        /// <param name="hsms"></param>
        /// <returns></returns>
        public bool AddSocketParaLibrary(SocketParaLibraryBase socket)
        {
            try
            {
                if (!Dic_SocketParaLibrary.ContainsKey(socket.EQID))
                {
                    Dic_SocketParaLibrary.TryAdd(socket.EQID, socket);
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// 获取"SocketParaLibrary"设定
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public SocketParaLibraryBase GetSocketParaLibrary(string id)
        {
            SocketParaLibraryBase ret = null;
            try
            {
                if (Dic_SocketParaLibrary.ContainsKey(id))
                {
                    ret = Dic_SocketParaLibrary[id];
                }

                return ret;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        /// <summary>
        /// 新增动态库
        /// </summary>
        /// <param name="dl"></param>
        /// <returns></returns>
        public bool AddDynamic(Socket_DynamicLibraryBase dl)
        {
            try
            {
                if (!Dic_DynamicLibrary.ContainsKey(dl.KeyID))
                {
                    Dic_DynamicLibrary.TryAdd(dl.KeyID, dl);
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// 取得指定设备的动态事件库
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Socket_DynamicLibraryBase GetDynamicLibrary(string id)
        {
            Socket_DynamicLibraryBase ret = null;
            try
            {
                if (Dic_DynamicLibrary.ContainsKey(id))
                {
                    ret = Dic_DynamicLibrary[id];
                }

                return ret;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        
        /// <summary>
        /// 检查指定设备是否联线
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool CheckEquipmentConnected(string id)
        {
            try
            {
                if (GetEquipmentModelByID(id).ConnectMode == Model.eConnectMode.DISCONNECT)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }

        }
        /// <summary>
        /// 检查所有设备
        /// </summary>
        /// <returns></returns>
        public bool CheckAllEquipmentConnected()
        {
            try
            {
                foreach (EquipmentModel em in Dic_EquipmentModel.Values)
                {
                    if (em.ConnectMode == Model.eConnectMode.DISCONNECT)
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
