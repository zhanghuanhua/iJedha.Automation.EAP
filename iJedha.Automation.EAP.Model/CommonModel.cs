//******************************************************************
//   系统名称 : iJedha.Automation.EAP.Model
//   文件概要 : CommonModel
//   作    者 : 张明哲
//   <更新履历>
//   1.0.0.0    2020/2/6 10:40:34
//******************************************************************
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace iJedha.Automation.EAP.Model
{
    /// <summary>
    /// 检查结果
    /// </summary>
    public enum eCheckResult
    {
        /// <summary>
        /// 未回复
        /// </summary>
        nothing = 0,
        /// <summary>
        /// 回复OK
        /// </summary>
        ok = 1,
        /// <summary>
        /// 回复NG
        /// </summary>
        ng = 2,
        /// <summary>
        /// 回复其他指令
        /// </summary>
        other = 3
    }
    /// <summary>
    /// 参数检查结果
    /// </summary>
    //public enum eParameterCheckResult
    //{
    //    nothing = 0,
    //    wait = 1,
    //    ok = 2,
    //    ng = 3
    //}
    /// <summary>
    /// 生产模式
    /// </summary>
    public enum eProductMode
    {
        /// <summary>
        /// 手动模式
        /// </summary>
        Manual = 0,
        /// <summary>
        /// 自动模式
        /// </summary>
        Auto = 1
    }
    /// <summary>
    /// 线体状态
    /// </summary>
    public enum eLineStatus
    {
        /// <summary>
        /// 运行
        /// </summary>
        Run = 0,
        /// <summary>
        /// 待机
        /// </summary>
        Idle = 1,
        /// <summary>
        /// 宕机
        /// </summary>
        Down = 2,
        /// <summary>
        /// 保养
        /// </summary>
        PM = 3
    }
    /// <summary>
    /// 前处理塞孔喷涂，连线/非连线
    /// </summary>
    public enum eConnectionStatus
    {
        /// <summary>
        /// 断线
        /// </summary>
        [Description("0")]
        Disconnection = 0,
        /// <summary>
        /// 连线
        /// </summary>
        [Description("1")]
        Connection = 1
    }
    /// <summary>
    /// 检查内容
    /// </summary>
    public class CheckContent
    {
        public string PPID { get; set; }
        //public eParameterCheckResult ParameterCheckResult { get; set; }
        public CheckContent()
        {
            PPID = string.Empty;
            //ParameterCheckResult = eParameterCheckResult.nothing;
        }
    }
    /// <summary>
    /// 分板线Track Out上报项
    /// </summary>
    public class TrackOutItem
    {
        public string PN { get; set; }
        public List<tLot> tLotList { get; set; }
        public TrackOutItem()
        {
            PN = string.Empty;
            tLotList = new List<tLot>();
        }
    }
    /// <summary>
    /// 分板线上报TrackOut Lot信息
    /// </summary>
    public class tLot
    {
        public string tLotID { get; set; }
        public List<toutPnlID> toutPnlList { get; set; }
        public tLot()
        {
            tLotID = string.Empty;
            toutPnlList = new List<toutPnlID>();
        }
    }
    /// <summary>
    /// 分板线上报TrackOut Panel信息
    /// </summary>
    public class toutPnlID
    {
        public string tOutPnlID { get; set; }
        public string tPanelID { get; set; }
        public toutPnlID()
        {
            tOutPnlID = string.Empty;
            tPanelID = string.Empty;
        }
    }
    /// <summary>
    /// JobDataRequest时，需要带入的参数
    /// </summary>
    public class CommModel
    {
        public EquipmentModel em { get; set; }
        public PortModel pm { get; set; }
        public LotModel lm { get; set; }

        public Lot lot { get; set; }
        public CommModel()
        {
            em = new EquipmentModel();
            pm = new PortModel();
            lm = new LotModel();

            lot = new Lot();
        }
    }
    public class CommonModel
    {/// <summary>
     /// 当前生产模式
     /// </summary>
        public eProductMode CurMode { get; set; }
        /// <summary>
        /// 上一个生产模式
        /// </summary>
        public eProductMode PreMode { get; set; }
        /// <summary>
        /// 上一个生产模式
        /// </summary>
        //public string[] ProductionModeList { get; set; }
        #region [Mode Change]
        #endregion
        /// <summary>
        /// 线体状态
        /// </summary>
        public eLineStatus LineStatus { get; set; }
        /// <summary>
        /// 询问MES状态
        /// </summary>
        public bool ModeChangeNeedRequest { get; set; }
        /// <summary>
        /// 启用"JobDataDownloadCheck"Rule
        /// </summary>
        public bool InJobDataDownloadCheckStart { get; set; }
        /// <summary>
        /// 启用"JobDataDownloadCheck"Rule时间
        /// </summary>
        public DateTime InJobDataDownloadCheckTime { get; set; }
        /// <summary>
        /// 启用"EquipmentJobDataRequestCheck"Rule
        /// </summary>
        public bool InEquipmentJobDataRequestCheckStart { get; set; }
        /// <summary>
        /// 启用"InEquipmentJobDataRequestCheck" Rule时间
        /// </summary>
        public DateTime InEquipmentJobDataRequestCheckTime { get; set; }
        /// <summary>
        /// 启用"AllProcessCompletionCheck"Rule
        /// </summary>
        public bool InAllProcessCompletionCheckStart { get; set; }
        /// <summary>
        /// 启用"AllProcessCompletionCheck" Rule时间
        /// </summary>
        public DateTime InAllProcessCompletionCheckTime { get; set; }
        /// <summary>
        /// 当前生产批次号
        /// </summary>
        public string currentProcessLotID { get; set; }
        /// <summary>
        /// 生产批次清单
        /// </summary>
        public ConcurrentDictionary<string, LotModel> List_ProcessLot { get; set; }

        /// <summary>
        /// 首件生产批次清单
        /// </summary>
        public ConcurrentDictionary<string, LotModel> List_InspectProcessLot { get; set; }

        /// <summary>
        /// 开料线生产批次清单
        /// </summary>
        public Dictionary<string, Lot> List_ProcessLotKL { get; set; }
        /// <summary>
        /// 开料线开始生产批次清单
        /// </summary>
        public Dictionary<string, Lot> List_StartProcessLotKL { get; set; }
        /// <summary>
        /// 生产Panel清单
        /// </summary>
        public List<PanelModel> List_ProcessPanel { get; set; }
        /// <summary>
        /// 检查内容清单
        /// </summary>
        public ConcurrentDictionary<string, CheckContent> InParameterCheckResult { get; set; }
        /// <summary>
        /// 启用"SocketReconnect"Rule时间
        /// </summary>
        public DateTime InSocketReconnectCheckTime { get; set; }
        /// <summary>
        /// 分板线Track Out上报List
        /// </summary>
        public List<TrackOutItem> TrackOutItems { get; set; }
        /// <summary>
        /// 启用"ProductionConditionCheck"Rule
        /// </summary>
        public bool InProductionConditionCheckStart { get; set; }
        /// <summary>
        /// 启用"InProductionConditionCheck" Rule时间
        /// </summary>
        public DateTime InProductionConditionCheckTime { get; set; }
        public bool InProductionConditionCheckInspectCheckStart { get; set; }
        public DateTime InProductionConditionCheckInspectCheckTime { get; set; }
        /// <summary>
        /// 多端口上报LoadComplete数量
        /// </summary>
        public Dictionary<string, string> LoadCompleteCount { get; set; }
        /// <summary>
        /// 启用LoadCompleteReportCheck Rule
        /// </summary>
        public bool InLoadCompleteReportCheckStart { get; set; }
        /// <summary>
        /// 启用LoadCompleteReportCheck Rule时间
        /// </summary>
        public DateTime InLoadCompleteReportCheckTime { get; set; }
        /// <summary>
        /// 启用TrackInCheckPositive Rule
        /// </summary>
        public bool InTrackInCheckPositiveStart { get; set; }
        /// <summary>
        /// 启用TrackInCheckPositive Rule时间
        /// </summary>
        public DateTime InTrackInCheckPositiveTime { get; set; }
        /// <summary>
        /// 启用TrackInCheckNegative Rule
        /// </summary>
        public bool InTrackInCheckNegativeStart { get; set; }
        /// <summary>
        /// 启用TrackInCheckNegative Rule时间
        /// </summary>
        public DateTime InTrackInCheckNegativeTime { get; set; }
        /// <summary>
        /// 启用CheckNextLotFlag Rule
        /// </summary>
        public bool InCheckNextLotFlagStart { get; set; }
        /// <summary>
        /// 启用CheckNextLotFlag Rule时间
        /// </summary>
        public DateTime InCheckNextLotFlagTime { get; set; }
        /// <summary>
        /// 融铆合上料口Carrier ID添加
        /// </summary>
        public Dictionary<string, string> Dic_CarrierIDList { get; set; }
        /// <summary>
        /// 向MES请求生产模式的时间点
        /// </summary>
        public DateTime EqpModeRequestTime { get; set; }

        public CommonModel()
        {
            currentProcessLotID = string.Empty;
            CurMode = eProductMode.Manual;
            PreMode = eProductMode.Manual;
            LineStatus = eLineStatus.Idle;
            List_ProcessLot = new ConcurrentDictionary<string, LotModel>();
            List_ProcessLotKL = new Dictionary<string, Lot>();
            List_StartProcessLotKL = new Dictionary<string, Lot>();
            List_InspectProcessLot = new ConcurrentDictionary<string, LotModel>();
            InParameterCheckResult = new ConcurrentDictionary<string, Model.CheckContent>();
            List_ProcessPanel = new List<PanelModel>();
            TrackOutItems = new List<TrackOutItem>();
            LoadCompleteCount = new Dictionary<string, string>();
            Dic_CarrierIDList = new Dictionary<string, string>();
            EqpModeRequestTime = DateTime.Now;
        }

        /// <summary>
        /// JobDataDownloadCheck初始化
        /// </summary>
        /// <returns></returns>
        public bool InJobDataDownloadCheckInitial()
        {
            try
            {
                InJobDataDownloadCheckStart = false;
                InJobDataDownloadCheckTime = DateTime.Now;
                InParameterCheckResult = new ConcurrentDictionary<string, Model.CheckContent>();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// InProductionConditionCheck初始化
        /// </summary>
        /// <returns></returns>
        public bool InProductionConditionCheckInitial()
        {
            try
            {
                InProductionConditionCheckStart = false;
                InProductionConditionCheckTime = DateTime.Now;
                InParameterCheckResult = new ConcurrentDictionary<string, Model.CheckContent>();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// InProductionConditionCheck初始化
        /// </summary>
        /// <returns></returns>
        public bool InProductionConditionCheckInspectCheckInitial()
        {
            try
            {
                InProductionConditionCheckInspectCheckStart = false;
                InProductionConditionCheckInspectCheckTime = DateTime.Now;
                InParameterCheckResult = new ConcurrentDictionary<string, Model.CheckContent>();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// EquipmentJobDataRequestCheck初始化
        /// </summary>
        /// <returns></returns>
        public bool InEquipmentJobDataRequestCheckInitial()
        {
            try
            {
                InEquipmentJobDataRequestCheckStart = false;
                InEquipmentJobDataRequestCheckTime = DateTime.Now;
                InParameterCheckResult = new ConcurrentDictionary<string, Model.CheckContent>();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// AllProcessCompletionCheck初始化
        /// </summary>
        /// <returns></returns>
        public bool InAllProcessCompletionCheckInitial()
        {
            try
            {
                InAllProcessCompletionCheckStart = false;
                InAllProcessCompletionCheckTime = DateTime.Now;
                InParameterCheckResult = new ConcurrentDictionary<string, Model.CheckContent>();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// LoadCompleteReportCheck初始化
        /// </summary>
        /// <returns></returns>
        public bool InLoadCompleteReportCheckInitial()
        {
            try
            {
                InLoadCompleteReportCheckStart = false;
                InLoadCompleteReportCheckTime = DateTime.Now;
                InParameterCheckResult = new ConcurrentDictionary<string, Model.CheckContent>();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// TrackInCheckPositive初始化
        /// </summary>
        /// <returns></returns>
        public bool InTrackInCheckPositiveCheckInitial()
        {
            try
            {
                InTrackInCheckPositiveStart = false;
                InTrackInCheckPositiveTime = DateTime.Now;
                InParameterCheckResult = new ConcurrentDictionary<string, Model.CheckContent>();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// TrackInCheckNegative初始化
        /// </summary>
        /// <returns></returns>
        public bool InTrackInCheckNegativeCheckInitial()
        {
            try
            {
                InTrackInCheckNegativeStart = false;
                InTrackInCheckNegativeTime = DateTime.Now;
                InParameterCheckResult = new ConcurrentDictionary<string, Model.CheckContent>();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool InCheckNextLotFlagInitial()
        {
            try
            {
                InCheckNextLotFlagStart = false;
                InCheckNextLotFlagTime = DateTime.Now;
                InParameterCheckResult = new ConcurrentDictionary<string, Model.CheckContent>();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// 新增Lot信息
        /// </summary>
        /// <param name="lot"></param>
        /// <returns></returns>
        public bool AddProcessLotModel(LotModel lot)
        {
            try
            {
                if (List_ProcessLot.ContainsKey(lot.LotID))
                {
                    LotModel PreLot = lot;
                    List_ProcessLot.TryRemove(lot.LotID, out PreLot);
                }
                List_ProcessLot.TryAdd(lot.LotID, lot);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 新增首件Lot信息
        /// </summary>
        /// <param name="lot"></param>
        /// <returns></returns>
        public bool AddInspectProcessLotModel(LotModel lot)
        {
            try
            {
                if (List_InspectProcessLot.ContainsKey(lot.LotID))
                {
                    LotModel PreLot = lot;
                    List_InspectProcessLot.TryRemove(lot.LotID, out PreLot);
                }
                List_InspectProcessLot.TryAdd(lot.LotID, lot);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 新增Lot信息 开料线
        /// </summary>
        /// <param name="lot"></param>
        /// <returns></returns>
        public bool AddProcessLotModelKL(Lot lot)
        {
            try
            {
                if (List_ProcessLotKL.ContainsKey(lot.LotID))
                {
                    Lot PreLot = lot;
                    List_ProcessLotKL.Remove(lot.LotID);
                }
                List_ProcessLotKL.Add(lot.LotID, lot);
                //List_ProcessLotKL.OrderBy(r=>r.Value.LotSeq);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// 新增开始生产Lot信息 开料线
        /// </summary>
        /// <param name="lot"></param>
        /// <returns></returns>
        public bool AddStartProcessLotModelKL(Lot lot)
        {
            try
            {
                if (List_StartProcessLotKL.ContainsKey(lot.LotID))
                {
                    Lot PreLot = lot;
                    List_StartProcessLotKL.Remove(lot.LotID);
                }
                List_StartProcessLotKL.Add(lot.LotID, lot);
                //List_ProcessLotKL.OrderBy(r=>r.Value.LotSeq);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// 新增融铆合PP上料口载具信息
        /// </summary>
        /// <param name="lot"></param>
        /// <returns></returns>
        public bool AddCarrierInfo(string eqID, string CarrierID)
        {
            try
            {
                if (Dic_CarrierIDList.ContainsKey(eqID))
                {
                    Dic_CarrierIDList.Remove(eqID);
                }
                Dic_CarrierIDList.Add(eqID, CarrierID);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 删除当前Lot以及当前Lot之前的Lot信息
        /// </summary>
        /// <param name="lotID"></param>
        /// <returns></returns>
        public bool RemoveProcessLotModel(LotModel lot)
        {
            try
            {
                if (List_ProcessLot.ContainsKey(lot.LotID))
                {
                    bool isRun = true;
                    LotModel lm = new LotModel();
                    //当List_ProcessLot中存在Lot的ProcessTime早于lot的物件，进行删除
                    do
                    {
                        //获取比当前时间早的Lot ID
                        string lotid = (from n in List_ProcessLot.Keys where DateTime.Compare(lot.ProcessTime, List_ProcessLot[n].ProcessTime) > 0 select n).FirstOrDefault();
                        if (!string.IsNullOrEmpty(lotid) && List_ProcessLot.Keys.Contains(lotid))
                        {
                            List_ProcessLot.TryRemove(lotid, out lm);
                            //List_ProcessLot.TryRemove(lot.LotID, out lot);
                        }
                        else
                        {
                            isRun = false;
                        }
                    } while (isRun);
                    List_ProcessLot.TryRemove(lot.LotID, out lot);
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// 删除当前Lot之前所有Lot信息
        /// </summary>
        /// <param name="lotID"></param>
        /// <returns></returns>
        public bool RemoveFrontProcessLotModel(LotModel lot)
        {
            try
            {
                //if (List_ProcessLot.ContainsKey(lot.LotID))
                //{
                bool isRun = true;
                LotModel lm = new LotModel();
                //当List_ProcessLot中存在Lot的ProcessTime早于lot的物件，进行删除
                do
                {
                    //获取比当前时间早的Lot ID
                    string lotid = (from n in List_ProcessLot.Keys where DateTime.Compare(lot.ProcessTime, List_ProcessLot[n].ProcessTime) > 0 select n).FirstOrDefault();
                    if (!string.IsNullOrEmpty(lotid) && List_ProcessLot.Keys.Contains(lotid))
                    {
                        List_ProcessLot.TryRemove(lotid, out lm);
                    }
                    else
                    {
                        isRun = false;
                    }
                } while (isRun);
                // }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// 删除SubLot信息
        /// </summary>
        /// <param name="lotID"></param>
        /// <returns></returns>
        public bool RemoveProcessSubLotModel(Lot lot)
        {
            try
            {
                if (List_ProcessLotKL.ContainsKey(lot.LotID))
                {
                    bool isRun = true;
                    //当List_ProcessLot中存在Lot的ProcessTime早于lot的物件，进行删除
                    do
                    {
                        //获取比当前时间早的Lot ID
                        string lotid = (from n in List_ProcessLotKL.Keys where DateTime.Compare(lot.ProcessTime, List_ProcessLotKL[n].ProcessTime) > 0 select n).FirstOrDefault();
                        if (!string.IsNullOrEmpty(lotid) && List_ProcessLotKL.Keys.Contains(lotid))
                        {
                            List_ProcessLotKL.Remove(lotid);
                        }
                        else
                        {
                            isRun = false;
                        }
                    } while (isRun);
                    List_ProcessLotKL.Remove(lot.LotID);
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 删除Panel信息
        /// </summary>
        /// <param name="lotID"></param>
        /// <param name="panelID"></param>
        /// <returns></returns>
        public bool RemoveProcessPanel(string lotID, string panelID)
        {
            try
            {
                LotModel lot = (from l in List_ProcessLot.Values where l.LotID == lotID select l).FirstOrDefault();
                if (lot != null)
                {
                    lot.PanelList.Remove(lot.GetPanelModel(panelID));
                    lot.PanelTotalQty = lot.PanelList.Count;
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool RemoveProcessPanelCount(string lotID, string count)
        {
            try
            {
                LotModel lot = (from l in List_ProcessLot.Values where l.LotID == lotID select l).FirstOrDefault();
                if (lot != null)
                {
                    if (lot.PanelTotalQty >= int.Parse(count))
                    {
                        lot.PanelTotalQty = lot.PanelList.Count - int.Parse(count);
                    }
                    else
                    {
                        lot.PanelTotalQty = 0;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// 删除分板线四合一上报Panel Read产生的Lot信息
        /// </summary>
        /// <param name="lotID"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public bool RemoveProcessPanel_FBX(string lotID)
        {
            try
            {//TrackOutItem
                var tt = (from q in TrackOutItems from w in q.tLotList where w.tLotID == lotID select q).ToList();
                foreach (var item in tt)
                {
                    TrackOutItems.Remove(item);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        /// <summary>
        /// 获取所有批次信息
        /// </summary>
        /// <param name="lotID"></param>
        /// <returns></returns>
        public List<LotModel> GetLotModel()
        {
            List<LotModel> lot = (from l in List_ProcessLot.Values select l).ToList();
            return lot;
        }

        /// <summary>
        /// 根据批次号获取批次信息
        /// </summary>
        /// <param name="lotID"></param>
        /// <returns></returns>
        public LotModel GetLotModelByLotID(string lotID)
        {
            LotModel lot = (from l in List_ProcessLot.Values where l.LotID == lotID select l).FirstOrDefault();
            return lot;
        }
        /// <summary>
        /// 根据SubLot获取批次信息
        /// </summary>
        /// <param name="lotID"></param>
        /// <returns></returns>
        public LotModel GetLotModelBySubLot(Lot subLot)
        {
            LotModel lot = (from q in List_ProcessLot.Values from w in q.LotList where w.LotID == subLot.LotID select q).FirstOrDefault();
            return lot;
        }

        /// <summary>
        /// 根据首件批次号获取批次信息
        /// </summary>
        /// <param name="lotID"></param>
        /// <returns></returns>
        public LotModel GetLotModelByInspectLotID(string lotID)
        {
            LotModel lot = (from l in List_InspectProcessLot.Values where l.LotID == lotID select l).FirstOrDefault();
            return lot;
        }
        /// <summary>
        /// 根据Sub次号获取批次信息
        /// </summary>
        /// <param name="lotID"></param>
        /// <returns></returns>
        public Lot GetLotModelBySubLotID(string lotID)
        {
            Lot lot = (from l in List_ProcessLotKL.Values where l.LotID == lotID select l).FirstOrDefault();
            return lot;
        }

        /// <summary>
        /// 根据Sub次号获取开始批次信息
        /// </summary>
        /// <param name="lotID"></param>
        /// <returns></returns>
        public Lot GetStartLotModelBySubLotID(string lotID)
        {
            Lot lot = (from l in List_StartProcessLotKL.Values where l.LotID == lotID select l).FirstOrDefault();
            return lot;
        }
        /// <summary>
        /// SubLot列表批次信息
        /// </summary>
        /// <param name="lotID"></param>
        /// <returns></returns>
        public List<Lot> GetSubLotID()
        {
            List<Lot> lot = (from l in List_ProcessLotKL.Values select l).ToList();
            return lot;
        }
        /// <summary>
        /// 根据Batch ID(JobID)取得批次信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public LotModel GetLotModelByBatchID(string id)
        {
            LotModel lot = (from n in List_ProcessLot.Values where n.JobID == id select n).FirstOrDefault();
            return lot;
        }

        /// <summary>
        /// 根据子批找母批号取得批次信息
        /// </summary>
        /// <param name="portid"></param>
        /// <param name="lotid"></param>
        /// <returns></returns>
        public LotModel GetLotModelByFirstInspectLot(string FirstInspectLot)
        {
            LotModel lot = (from n in List_InspectProcessLot.Values where n.FirstInspectLot == FirstInspectLot select n).FirstOrDefault();
            return lot;
        }
        /// <summary>
        /// 根据Panel号获取批次信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public LotModel GetLotModelByPanelID(string id)
        {
            LotModel lot = (from l in List_ProcessLot.Values from w in l.ProcessPanelList where w.PanelID.ToString() == id select l).FirstOrDefault();
            return lot;
        }
        /// <summary>
        /// 根据内层Panel号获取批次信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public LotModel GetLotModelByInnerPanelID(string id)
        {
            LotModel lot = (from l in List_ProcessLot.Values from w in l.InnerLotList from q in w.ListPanel where q.PanelID.ToString() == id select l).FirstOrDefault();
            return lot;
        }
        /// <summary>
        /// 根据内层Lot号获取批次信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public LotModel GetLotModelByInnerLotID(string id)
        {
            LotModel lot = (from l in List_ProcessLot.Values from w in l.InnerLotList where w.InnerLotID == id select l).FirstOrDefault();
            return lot;
        }

        /// <summary>
        /// 根据Panel号获取Panel信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public PanelModel GetPanelModelByPanelID(string id)
        {
            PanelModel Panel = (from l in List_ProcessPanel where l.PanelID == id select l).FirstOrDefault();
            return Panel;
        }
        /// <summary>
        /// 根据Panel号移除Panel信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public void RemovePanelModelByPanelID(string id)
        {
            PanelModel Panel = List_ProcessPanel.FirstOrDefault(t => t.PanelID == id);
            List_ProcessPanel.Remove(Panel);
        }
        /// <summary>
        /// 新增Panel信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool AddProcessPanelModel(PanelModel Panel)
        {
            Panel.CreateTime = string.Format("{0:yyyyMMddHHmmss}", DateTime.Now);
            List_ProcessPanel.Add(Panel);
            return true;
        }
        /// <summary>
        /// 根据载具号获取批次信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public LotModel GetLotModelByCarrierID(string id)
        {
            LotModel lot = (from l in List_ProcessLot.Values where l.CarrierID == id select l).FirstOrDefault();
            return lot;
        }
        /// <summary>
        /// 根据载具号获取批次信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="datasource"></param>
        /// <returns></returns>
        public LotModel GetLotModelByCarrierID(string id, string datasource)
        {
            LotModel lot = (from l in List_ProcessLot.Values where l.CarrierID == id && l.DataSource.ToString() == datasource select l).FirstOrDefault();
            return lot;
        }
        /// <summary>
        /// 根据批次状态获取批次信息
        /// </summary>
        /// <param name="lotprocessstatus"></param>
        /// <returns></returns>
        public LotModel GetLotModelByLotStatus(eLotProcessStatus lotprocessstatus)
        {
            LotModel lot = (from l in List_ProcessLot.Values where l.LotProcessStatus == lotprocessstatus select l).FirstOrDefault();
            return lot;
        }
        /// <summary>
        /// 获取任务参数下载Item
        /// </summary>
        /// <param name="em"></param>
        /// <param name="lm"></param>
        /// <returns></returns>
        public List<MessageModel.Param> GetParameterModel(EquipmentModel em, LotModel lm)
        {
            List<MessageModel.Param> ListParameter = new List<MessageModel.Param>();
            try
            {
                if (lm != null)
                {
                    List<SubEqp> pp = (from n in lm.SubEqpList where n.SubEqpID == em.EQID select n).ToList();

                    foreach (var pa in pp)
                    {
                        foreach (var item in pa.EQParameter)
                        {
                            MessageModel.Param _pa = new MessageModel.Param();
                            _pa.ParamName = item.ItemName;
                            _pa.ParamValue = item.ItemValue;
                            _pa.ParamType = item.DataType;
                            _pa.SubEqpID = em.EQID;

                            ListParameter.Add(_pa);
                        }
                    }
                }
                return ListParameter;
            }
            catch (Exception ex)
            {
                return ListParameter;
            }

        }
        /// <summary>
        /// 获取任务参数下载Item.For 开料线
        /// </summary>
        /// <param name="em"></param>
        /// <param name="lm"></param>
        /// <returns></returns>
        public List<MessageModel.Param> GetParameterModel_KL(EquipmentModel em, Lot lm)
        {
            List<MessageModel.Param> ListParameter = new List<MessageModel.Param>();
            try
            {
                if (lm != null)
                {
                    List<SubEqp> pp = (from n in lm.SubEqpList where n.SubEqpID == em.EQID select n).ToList();

                    foreach (var pa in pp)
                    {
                        foreach (var item in pa.EQParameter)
                        {
                            MessageModel.Param _pa = new MessageModel.Param();
                            _pa.ParamName = item.ItemName;
                            _pa.ParamValue = item.ItemValue;
                            _pa.ParamType = item.DataType;
                            _pa.SubEqpID = em.EQID;

                            ListParameter.Add(_pa);
                        }
                    }
                }
                return ListParameter;
            }
            catch (Exception ex)
            {
                return ListParameter;
            }

        }


    }
}
