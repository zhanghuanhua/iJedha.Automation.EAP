//******************************************************************
//   系统名称 : iJedha.Automation.EAP.Model
//   文件概要 : PortModel
//   作    者 : 张明哲
//   <更新履历>
//   1.0.0.0    2020/2/6 10:40:34
//******************************************************************
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace iJedha.Automation.EAP.Model
{
    /// <summary>
    /// 端口状态枚举
    /// </summary>
    public enum ePortStatus
    {
        /// <summary>
        /// 未知
        /// </summary>
        UNKNOW = 0,
        /// <summary>
        /// 上料请求
        /// </summary>
        LOADREQUEST = 1,
        /// <summary>
        /// 上料完成
        /// </summary>
        LOADCOMPLETE = 2,
        /// <summary>
        /// 下料请求
        /// </summary>
        UNLOADREQUEST = 3,
        /// <summary>
        /// 下料完成
        /// </summary>
        UNLOADCOMPLETE = 4
    }
    /// <summary>
    /// 端口编号枚举
    /// </summary>
    public enum ePortID
    {
        /// <summary>
        /// 投板机暂存口(对接AGV)/上料口端口ID:1
        /// </summary>
        L01 = 0,
        /// <summary>
        /// 投板机上料口/上料口端口ID:2
        /// </summary>
        L02 = 1,
        /// <summary>
        /// 投板机陪镀板/上料口端口ID:3
        /// </summary>
        L03 = 2,
        /// <summary>
        /// 投板机空托盘/上料口端口ID:4
        /// </summary>
        L04 = 3,
        /// <summary>
        /// 上料口端口ID:5
        /// </summary>
        L05 = 4,
        /// <summary>
        /// 上料口端口ID:6
        /// </summary>
        L06 = 5,
        /// <summary>
        /// 收板机出料位(对接AGV)/下料口端口ID:1
        /// </summary>
        U01 = 6,
        /// <summary>
        /// 收板机下料位/下料口端口ID:2
        /// </summary>
        U02 = 7,
        /// <summary>
        /// 收板机陪镀板/下料口端口ID:3
        /// </summary>
        U03 = 8,
        /// <summary>
        /// 收板机空托盘/下料口端口ID:4
        /// </summary>
        U04 = 9,
        /// <summary>
        /// 收板机/下料口端口ID:5
        /// </summary>
        U05 = 10,
        /// <summary>
        /// 收板机/下料口端口ID:6
        /// </summary>
        U06 = 11
    }
    /// <summary>
    /// LoadRequest Type枚举
    /// </summary>
    public enum eRequestType
    {
        /// <summary>
        /// 满载
        /// </summary>
        Full = 0,
        /// <summary>
        /// 空载
        /// </summary>
        Empty = 1
    }

    /// <summary>
    /// Port模块
    /// </summary>
    [Serializable]
    public class PortModel
    {
        static PortModel port;
        public static PortModel pm
        {
            get
            {
                if (port == null)
                {
                    port = new PortModel();
                }
                return port;
            }
        }

        [CategoryAttribute("基本信息"), DescriptionAttribute("设备ID")]
        [ReadOnlyAttribute(true)]
        public string EQID { get; set; }
        [CategoryAttribute("基本信息"), DescriptionAttribute("端口ID")]
        [ReadOnlyAttribute(true)]
        public ePortID PortID { get; set; }
        [CategoryAttribute("基本信息"), DescriptionAttribute("端口名称")]
        [ReadOnlyAttribute(true)]
        public ePortID Name { get; set; }
        [CategoryAttribute("基本信息"), DescriptionAttribute("端口状态")]
        [ReadOnlyAttribute(false)]
        public ePortStatus PortStatus { get; set; }
        [CategoryAttribute("基本信息"), DescriptionAttribute("载具ID")]
        [ReadOnlyAttribute(true)]
        public string CarrierID { get; set; }
        [CategoryAttribute("基本信息"), DescriptionAttribute("Panel列表")]
        [ReadOnlyAttribute(true)]
        public Dictionary<string, PanelModel> List_Panel { get; set; }
        [CategoryAttribute("基本信息"), DescriptionAttribute("More Panel List")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public List<PanelModel> List_MorePanel { get; set; }//当生产片数超过帐的数量时，Panel 会被记录
        [CategoryAttribute("基本信息"), DescriptionAttribute("当前端口批次列表")]
        [ReadOnlyAttribute(true)]
        public Dictionary<string, LotModel> List_Lot { get; set; }

        [CategoryAttribute("基本信息"), DescriptionAttribute("当前端口批次列表")]
        [ReadOnlyAttribute(true)]
        public Dictionary<string, Lot> List_SubLot { get; set; }

        [CategoryAttribute("基本信息"), DescriptionAttribute("Lot Flag:True：NG Lot，go to NG area")]
        [ReadOnlyAttribute(true)]
        public bool LotFlag { get; set; }
        [CategoryAttribute("基本信息"), DescriptionAttribute("当前端口上报物料信息")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public string MaterialLotID_PP { get; set; }

        public List<string> LstLotID { get; set; }
        public PortModel()
        {
            PortStatus = ePortStatus.UNKNOW;
            CarrierID = string.Empty;
            List_Lot = new Dictionary<string, LotModel>();
            List_SubLot = new Dictionary<string, Lot>();
            List_Panel = new Dictionary<string, PanelModel>();
            List_MorePanel = new List<PanelModel>();
            MaterialLotID_PP = string.Empty;
            LstLotID = new List<string>();
        }
        /// <summary>
        /// 首检结果NG，做初始化
        /// </summary>
        public void Initial_NG()
        {
            List_Panel = new Dictionary<string, PanelModel>();
            List_Lot = new Dictionary<string, LotModel>();
        }
        /// <summary>
        /// 初始化
        /// </summary>
        public void DeepInitial()
        {
            CarrierID = string.Empty;
            List_Panel = new Dictionary<string, PanelModel>();
            List_Lot = new Dictionary<string, LotModel>();
            List_MorePanel = new List<PanelModel>();
        }
        /// <summary>
        /// 投板机在UnLoad Request时，删除与Lot相关的LotModel与PanelMode
        /// </summary>
        public void DeepInitial_UDRQ(string LotID)
        {
            #region [清除Port中与Lot相关的Panel]
            bool isRun = true;
            do
            {
                string no = (from n in List_Panel.Keys where List_Panel[n].LotID == LotID || n.Split('_')[0] == LotID select n).FirstOrDefault();
                if (!string.IsNullOrEmpty(no) && List_Panel.Keys.Contains(no))
                {
                    List_Panel.Remove(no);
                }
                else
                {
                    isRun = false;
                }
            }
            while (isRun);
            #endregion

            #region [清除Port中的对应的Lot信息]
            isRun = true;
            do
            {
                string lotID = (from n in List_Lot.Keys where List_Lot[n].LotID == LotID select n).FirstOrDefault();
                if (!string.IsNullOrEmpty(lotID) && List_Lot.Keys.Contains(lotID))
                {
                    List_Lot.Remove(lotID);
                }
                else
                {
                    isRun = false;
                }
            } while (isRun);
            #endregion
        }

        /// <summary>
        /// 移除此前所有端口Lot列表和Panel列表
        /// </summary>
        /// <param name="lot"></param>
        public void RemovePortLotInfo(LotModel lot)
        {
            bool isRun = true;
            //当List_Lot中存在Lot的ProcessTime早于lot的物件，进行删除
            //lock (List_Lot)
            //{
                do
                {
                    //获取比当前时间早的Lot ID
                    string lotid = (from n in List_Lot.Keys where DateTime.Compare(lot.ProcessTime, List_Lot[n].ProcessTime) > 0 select n).FirstOrDefault();
                    do
                    {
                        string no = (from n in List_Panel.Keys where List_Panel[n].LotID == lotid || n.Split('_')[0] == lotid select n).FirstOrDefault();
                        if (!string.IsNullOrEmpty(no) && List_Panel.Keys.Contains(no))
                        {
                            List_Panel.Remove(no);
                        }
                        else
                        {
                            isRun = false;
                        }
                    }
                    while (isRun);

                    if (!string.IsNullOrEmpty(lotid) && List_Lot.Keys.Contains(lotid))
                    {
                        LstLotID.Add(lotid);
                        List_Lot.Remove(lotid);
                    }
                    else
                    {
                        isRun = false;
                    }
                } while (isRun);
                LstLotID.Add(lot.LotID);
                List_Lot.Remove(lot.LotID);
            //}
        }

        /// <summary>
        /// 删除Port中的对应Lot信息
        /// </summary>
        public void DeleteLotModel(string LotID)
        {
            if (List_Lot.ContainsKey(LotID))
            {
                List_Lot.Remove(LotID);
            }
        }

    }
}
