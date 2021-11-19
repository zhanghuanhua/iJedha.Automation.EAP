//******************************************************************
//   系统名称 : iJedha.Automation.EAP.Library
//   文件概要 : CommonLibrary
//   作    者 : 张明哲
//   <更新履历>
//   1.0.0.0    2020/2/6 10:40:34
//******************************************************************
using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.LibraryBase;
using iJedha.Automation.EAP.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using static iJedha.Automation.EAP.Core.socket;

namespace iJedha.Automation.EAP.Library
{
    #region [环境选择]
    public enum eSolution
    {
        Production = 0,
        Test = 1,
        Develop = 2
    }
    #endregion

    #region [CommonLibrary]
    public class CommonLibrary
    {
        public XmlTagHandle xmlHandle;
        protected XmlDocument _xmlDoc;
        /// <summary>
        /// 编号
        /// </summary>
        public string MDLN { get; set; }
        /// <summary>
        /// 线体名
        /// </summary>
        public string LineName { get; set; }
        /// <summary>
        ///  版本号
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// 程序启动路径
        /// </summary>
        public string AppPath { get; set; }
        public string StartupPath { get; set; }
        /// <summary>
        /// AMS路由
        /// </summary>
        public string AMSUrl { get; set; }
        /// <summary>
        /// 配置档路径
        /// </summary>
        public string LibraryPath { get; set; }
        /// <summary>
        /// DynamicLibrary动态库路径
        /// </summary>
        public string DynamicLibraryPath { get; set; }
        /// <summary>
        /// 设备配置档路径
        /// </summary>
        public string EquipmentLibraryPath { get; set; }
        /// <summary>
        /// 错误码配置档路径
        /// </summary>
        public string ErrorCodeLibraryPath { get; set; }
        public string PasswordKeyPath { get; set; }
        public string ConfigLibraryPath { get; set; }
        public string PortEntityPath { get; set; }
        public ConcurrentDictionary<string, PortEntity> Dic_PortEntity { get; set; }
        public string CustomizedLibraryPath { get; set; }
        public string ScenarioLibraryPath { get; set; }
        public eHostConnectMode HostConnectMode { get; set; }
        public eHostConnectMode PreHostConnectMode { get; set; }
        /// <summary>
        /// Log文件夹的大小
        /// </summary>
        public double logSize { get; set; }
        /// <summary>
        /// Log文件夹限定的大小
        /// </summary>
        public double logLimitSize { get; set; }
        /// <summary>
        ///  显示LotID
        /// </summary>
        public string MainLotID { get; set; }
        /// <summary>
        ///  显示Lot InfoMessage
        /// </summary>
        public string ShowLotInfoMessage { get; set; } = "0";
        public bool isWebServerStart { get; set; }
        public bool MQConnectedStatus { get; set; }

        public bool PreMQConnectedStatus { get; set; }
        public IList<string> List_EQLink { get; set; }
        /// <summary>
        /// 判断是否清机完成
        /// </summary>
        public bool isProcessOK { get; set; }
        /// <summary>
        /// 判断是否清线完成
        /// </summary>
        public bool isAllProcessOK { get; set; }
        /// <summary>
        /// 是否开启单独处理Socket重连的Thread
        /// </summary>
        public bool isRunThread { get; set; } = false;
        /// <summary>
        /// MES下载生产任务数量
        /// </summary>
        public int mESTaskCount { get; set; } = 0;
        /// <summary>
        /// Socket重连初始时间4s
        /// </summary>
        public int reconnectSec { get; set; } = 4;
        /// <summary>
        /// GUI触发DataDownload
        /// </summary>
        public bool isGUITriggerDataDownload { get; set; } = false;
        /// <summary>
        /// 开料机完工信号触发为true
        /// </summary>
        public bool isJobDataProcessDataTigger { get; set; } = false;
        /// <summary>
        /// 第一次检查生产任务
        /// </summary>
        public bool isNormalCheckData { get; set; } = true;
        /// <summary>
        /// 是否为MES主动下载
        /// </summary>
        public bool isMESDownloadData { get; set; } = false;

        #region  Socket重连
        public bool GetSocketCommunicationStatus(socket.SocketCommonData.HOSTMODE hostmode, socket.SocketBasic socket)
        {

            if (socket == null) return false;

            if (hostmode == iJedha.Automation.EAP.Core.socket.SocketCommonData.HOSTMODE.HOSTMODE_SERVER)
            {
                return ((TCPSocketServer)socket).m_NowConnectNum > 0 ? true : false;
            }
            else
            {
                return ((TCPSocketClient)socket).CurClientStatus == TCPSocketClient.CONNECT_STATUS.CONNECT_COMPLETE ? true : false;
            }
        }
        #endregion

        private string stringLink;
        public string StringLink
        {
            get
            {
                return stringLink;
            }
            set
            {
                stringLink = value;
                if (stringLink == string.Empty)
                {
                    return;
                }
                string[] sl = stringLink.Split(',');
                foreach (string s in sl)
                {
                    List_EQLink.Add(s);
                }

            }
        }
        /// <summary>
        /// 读头读取板号获取信息
        /// </summary>
        public string CCDGetSubEqList { get; set; } = string.Empty;
        /// <summary>
        /// MES下载设备信息
        /// </summary>
        public string MESInnerLotSubEqInfo { get; set; } = string.Empty;
        /// <summary>
        /// MES下载内层设备列表
        /// </summary>
        public List<string> MESInnerLotSubEqList { get; set; }
        /// <summary>
        /// 冲孔信息组合
        /// </summary>
        public List<string> PunchingList { get; set; }
        /// <summary>
        /// 层次 0203   0405
        /// </summary>
        public List<string> LayerLevel { get; set; }
        /// <summary>
        /// 是否为层别第一层设备
        /// </summary>
        public bool isFirstSubEq { get; set; } = true;
        /// <summary>
        /// 完工信号总共次数
        /// </summary>
        public int UnloadEquipmentProcessCompleteReportCount { get; set; } = 0;
        /// <summary>
        /// 上报完工信号次数
        /// </summary>
        public int ProcessCompleteReportCount { get; set; } = 0;
        /// <summary>
        /// 前一设备的层别
        /// </summary>
        public string FrontLayer { get; set; } = string.Empty;
        /// <summary>
        /// 首件结果是否OK
        /// </summary>
        public bool isInspecResultOK { get; set; } = false;
        /// <summary>
        /// 首件是否做过TrackIn
        /// </summary>
        public bool isFirstInspectTrackIn { get; set; } = false;
        /// <summary>
        /// 多端口设备上报LoadComplete数量是否和内层Lot数量一致
        /// </summary>
        public bool isSameWithInnerLotCount { get; set; } = false;
        //public int Negative { get; set; } = 0;//逆向PP上报次数
        //public int Positive { get; set; } = 0;//正向PP上报次数
        /// <summary>
        /// 逆向PP上报次数
        /// </summary>
        public Dictionary<string, string> Negative { get; set; }
        /// <summary>
        /// 正向PP上报次数
        /// </summary>
        public Dictionary<string, string> Positive { get; set; }
        /// <summary>
        /// 是否设定为传送设备
        /// </summary>
        public bool isTransferStatus { get; set; }
        public CommonModel commonModel { get; set; }
        public LineModel lineModel { get; set; }
        /// <summary>
        /// 获取EquipmentLibary信息
        /// </summary>
        public EquipmentLibary equipmentLibary { get; set; }
        /// <summary>
        /// 获取CustomizedLibrary信息
        /// </summary>
        public CustomizedLibrary customizedLibrary { get; set; }
        /// <summary>
        /// 获取LogLibrary信息
        /// </summary>
        public LogLibrary logLibrary { get; set; }
        /// <summary>
        /// 获取ConfigLibrary信息
        /// </summary>
        public ConfigLibrary configLibrary { get; set; }
        public PortEntity portEntity { get; set; }

        public AlarmReport alarmReport;
        public EquipmentStatus equipmentStatus;
        public TestResult testResult;
        public LibraryBase.LibraryBase baseLib { get; set; }
        public Socket_DynamicLibraryBase dynamicLibrary { get; set; }
        public string OldPN { get; set; }
        public string OldLotID { get; set; }
        public string OldWorkOrder { get; set; }
        /// <summary>
        /// 开料线最后一个Lot数量，大板数量
        /// </summary>
        public string LastLotCount { get; set; }
        /// <summary>
        /// 停止下一个Lot投板
        /// </summary>
        public bool StopNextLot { get; set; }
        /// <summary>
        /// PP裁切机任务ID和Port ID绑定信息
        /// </summary>
        public string MaterialIDInfo { get; set; }
        /// <summary>
        /// 物料Lot绑定信息
        /// </summary>
        public Dictionary<string, int> MaterialPortIDBinding { get; set; }
        /// <summary>
        /// 当前Lot制程数量
        /// </summary>
        public string CurrentLotCount { get; set; }
        /// <summary>
        /// 压合回流线使用哪一个投板机进行叫料下货
        /// </summary>
        public string UseLoadEquipmentNo { get; set; }
        /// <summary>
        /// Hold AOI 保存板号，ProcessData上报时使用
        /// </summary>
        public Queue<string> qPanelIDList { get; set; }
        /// <summary>
        /// LDI报废板数量
        /// </summary>
        public int ScrapPanelCount { get; set; }
        /// <summary>
        /// 广合设备投板机连续呼叫AGV卡控
        /// </summary>
        public ConcurrentDictionary<string, bool> Dic_IsCallAgv { get; set; }

        public Tuple<string,string> TupTrackIn { get; set; }
        public CommonLibrary()
        {
            xmlHandle = new XmlTagHandle();
            commonModel = new CommonModel();
            customizedLibrary = new CustomizedLibrary();
            equipmentLibary = new EquipmentLibary();
            baseLib = new LibraryBase.LibraryBase();
            dynamicLibrary = new Socket_DynamicLibraryBase();
            logLibrary = new LogLibrary();
            configLibrary = new ConfigLibrary();
            portEntity = new PortEntity();
            alarmReport = new AlarmReport();
            equipmentStatus = new EquipmentStatus();
            testResult = new TestResult();
            List_EQLink = new List<string>();
            HostConnectMode = eHostConnectMode.DISCONNECT;
            PreHostConnectMode = eHostConnectMode.DISCONNECT;
            isProcessOK = false;
            isAllProcessOK = false;
            OldPN = string.Empty;
            OldLotID = string.Empty;
            OldWorkOrder = string.Empty;
            StopNextLot = false;
            Negative = new Dictionary<string, string>();
            Positive = new Dictionary<string, string>();
            isTransferStatus = false;
            lineModel = new LineModel();
            LastLotCount = string.Empty;
            MaterialIDInfo = string.Empty;
            MaterialPortIDBinding = new Dictionary<string, int>();
            CurrentLotCount = string.Empty;
            UseLoadEquipmentNo = string.Empty;
            qPanelIDList = new Queue<string>();
            ScrapPanelCount = 0;
            Dic_PortEntity = new ConcurrentDictionary<string, PortEntity>();
            Dic_IsCallAgv = new ConcurrentDictionary<string, bool>();
            TupTrackIn = new Tuple<string, string>("","");
        }

        #region [配置库初始化]
        public bool InitialCommon(string startuppath, string lineno, string linename, eSolution solution)
        {
            try
            {
                #region [Initial]
                MDLN = lineno;
                LineName = linename;
                AppPath = Application.StartupPath;
                StartupPath = startuppath;
                if (solution == eSolution.Production)
                {
                    LibraryPath = @"Library\Production";
                }
                if (solution == eSolution.Test)
                {
                    LibraryPath = @"Library\Test";
                }
                ConfigLibraryPath = Path.Combine(StartupPath, LibraryPath, MDLN + "_" + LineName, ConstLibrary.CFG_Config_FILE_NAME);
                if (!File.Exists(ConfigLibraryPath))
                {
                    MessageBox.Show(string.Format("{0} is not exist", ConfigLibraryPath), "EAP Start Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                //PortEntityPath = Path.Combine(StartupPath, "PortEntity", ConstLibrary.CFG_PortEntity_FILE_NAME);
                //if (!File.Exists(PortEntityPath))
                //{
                //    MessageBox.Show(string.Format("{0} is not exist", PortEntityPath), "EAP Start Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //    return false;
                //}

                EquipmentLibraryPath = Path.Combine(StartupPath, LibraryPath, MDLN + "_" + LineName, ConstLibrary.CFG_Equipment_FILE_NAME);
                if (!File.Exists(EquipmentLibraryPath))
                {
                    MessageBox.Show(string.Format("{0} is not exist", EquipmentLibraryPath), "EAP Start Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                ErrorCodeLibraryPath = Path.Combine(StartupPath, ConstLibrary.CONST_ERRORCODE_PATH, ConstLibrary.CFG_ErrorCode_FILE_NAME);
                if (!File.Exists(ErrorCodeLibraryPath))
                {
                    MessageBox.Show(string.Format("{0} is not exist", ErrorCodeLibraryPath), "EAP Start Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                PasswordKeyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConstLibrary.CONST_BASELIB_PATH, ConstLibrary.CFG_PasswordKey_FILE_NAME);
                if (!File.Exists(PasswordKeyPath))
                {
                    MessageBox.Show(string.Format("{0} is not exist", PasswordKeyPath), "EAP Start Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                ScenarioLibraryPath = Path.Combine(StartupPath, LibraryPath, MDLN + "_" + LineName, ConstLibrary.CFG_Scenario_FILE_NAME);
                if (!File.Exists(ScenarioLibraryPath))
                {
                    ScenarioLibraryPath = string.Empty;
                }

                CustomizedLibraryPath = Path.Combine(StartupPath, LibraryPath, MDLN + "_" + LineName, ConstLibrary.CFG_Customized_FILE_NAME);
                if (!File.Exists(CustomizedLibraryPath))
                {
                    CustomizedLibraryPath = string.Empty;
                }

                #endregion

                if (!baseLib.Initial(ConfigLibraryPath, PasswordKeyPath))
                {
                    return false;
                }

                if (!baseLib.LoadConfigLib())
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return false;
            }
        }
        public bool InitialCommon(eSolution solution)
        {
            try
            {
                #region [Initial]
                string startupPath = Path.Combine(Application.StartupPath, ConstLibrary.CFG_DEV_FILE_NAME);
                if (!File.Exists(startupPath))
                {
                    return false;
                }
                XmlDocument _xmlDoc = new XmlDocument();
                _xmlDoc.Load(startupPath);
                XmlNodeList xnl = _xmlDoc.SelectSingleNode("Config/Startup").ChildNodes;
                if (xnl.Count == 0)
                {
                    MessageBox.Show(string.Format("{0} is read error", startupPath), "EAP Start Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                foreach (XmlNode xn in xnl)
                {
                    MDLN = xn.Attributes["LineNo"].Value;
                    LineName = xn.Attributes["LineName"].Value;
                }
                DirectoryInfo info = new DirectoryInfo(Application.StartupPath);
                StartupPath = info.Parent.FullName;
                if (solution == eSolution.Develop)
                {
                    LibraryPath = @"Library\Develop";
                }

                ConfigLibraryPath = Path.Combine(StartupPath, LibraryPath, MDLN + "_" + LineName, ConstLibrary.CFG_Config_FILE_NAME);
                if (!File.Exists(ConfigLibraryPath))
                {
                    MessageBox.Show(string.Format("{0} is not exist", ConfigLibraryPath), "EAP Start Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                //PortEntityPath = Path.Combine(StartupPath,"PortEntity", ConstLibrary.CFG_PortEntity_FILE_NAME);
                //if (!File.Exists(PortEntityPath))
                //{
                //    MessageBox.Show(string.Format("{0} is not exist", PortEntityPath), "EAP Start Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //    return false;
                //}

                EquipmentLibraryPath = Path.Combine(StartupPath, LibraryPath, MDLN + "_" + LineName, ConstLibrary.CFG_Equipment_FILE_NAME);
                if (!File.Exists(EquipmentLibraryPath))
                {
                    MessageBox.Show(string.Format("{0} is not exist", EquipmentLibraryPath), "EAP Start Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                ErrorCodeLibraryPath = Path.Combine(StartupPath, ConstLibrary.CONST_ERRORCODE_PATH, ConstLibrary.CFG_ErrorCode_FILE_NAME);
                if (!File.Exists(ErrorCodeLibraryPath))
                {
                    MessageBox.Show(string.Format("{0} is not exist", ErrorCodeLibraryPath), "EAP Start Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                PasswordKeyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConstLibrary.CONST_BASELIB_PATH, ConstLibrary.CFG_PasswordKey_FILE_NAME);
                if (!File.Exists(PasswordKeyPath))
                {
                    MessageBox.Show(string.Format("{0} is not exist", PasswordKeyPath), "EAP Start Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                ScenarioLibraryPath = Path.Combine(StartupPath, LibraryPath, MDLN + "_" + LineName, ConstLibrary.CFG_Scenario_FILE_NAME);
                if (!File.Exists(ScenarioLibraryPath))
                {
                    ScenarioLibraryPath = string.Empty;
                }

                CustomizedLibraryPath = Path.Combine(StartupPath, LibraryPath, MDLN + "_" + LineName, ConstLibrary.CFG_Customized_FILE_NAME);
                if (!File.Exists(CustomizedLibraryPath))
                {
                    CustomizedLibraryPath = string.Empty;
                }

                #endregion

                if (!baseLib.Initial(ConfigLibraryPath, PasswordKeyPath))
                {
                    return false;
                }
                if (!baseLib.LoadConfigLib())
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return false;
            }
        }
        /// <summary>
        /// 获取"EquipmentLibrary.xml"中的信息
        /// </summary>
        /// <returns></returns>
        public bool InitialEquipment()
        {
            try
            {
                xmlHandle.SectionPath = EquipmentLibraryPath;
                if (!xmlHandle.ReadConfig())
                {
                    MessageBox.Show(string.Format("{0} is read error", EquipmentLibraryPath), "EAP Start Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                xmlHandle.SectionNode = "Equipment";
                StringLink = xmlHandle.GetValue2("Link", "1");
                lineModel.LineType = xmlHandle.GetValue2("LineType", "");
                lineModel.LineName = LineName;//xmlHandle.GetValue2("LineName", "");
                lineModel.isInnerLotPanelList = bool.Parse(xmlHandle.GetValue2("isInnerLotPanelList", "False"));
                lineModel.isLoadCompleteCountCheck = bool.Parse(xmlHandle.GetValue2("isLoadCompleteCountCheck", "False"));
                lineModel.isSubLot = bool.Parse(xmlHandle.GetValue2("isSubLot", "False"));
                lineModel.isCheckBeforeTrackIn = bool.Parse(xmlHandle.GetValue2("isCheckBeforeTrackIn", "False"));
                lineModel.isMultiLoadEquipment = bool.Parse(xmlHandle.GetValue2("isMultiLoadEquipment", "False"));
                lineModel.isMainEqpID = bool.Parse(xmlHandle.GetValue2("isMainEqpID", "False"));
                lineModel.isNeedPost = bool.Parse(xmlHandle.GetValue2("isNeedPost", "True"));
                lineModel.isCarrierIDRequestLotInfo = bool.Parse(xmlHandle.GetValue2("isCarrierIDRequestLotInfo", "False"));
                lineModel.isAllrocessCompletionByLot = bool.Parse(xmlHandle.GetValue2("isAllrocessCompletionByLot", "False"));
                lineModel.isAllrocessCompletionByWorkOrder = bool.Parse(xmlHandle.GetValue2("isAllrocessCompletionByWorkOrder", "False"));
                lineModel.StartEquipmentID = xmlHandle.GetValue2("StartEquipmentID", string.Empty);
                lineModel.isCheckScanCodeReport = bool.Parse(xmlHandle.GetValue2("isCheckScanCodeReport", "False"));
                lineModel.isQPanelList = bool.Parse(xmlHandle.GetValue2("isQPanelList", "False"));
                lineModel.isCheckEquipmentStatus = bool.Parse(xmlHandle.GetValue2("isCheckEquipmentStatus", "False"));
                lineModel.isCheckLoadStatus = bool.Parse(xmlHandle.GetValue2("isCheckLoadStatus", "False"));
                lineModel.isManualJobDataDoanload = bool.Parse(xmlHandle.GetValue2("isManualJobDataDoanload", "False"));

                #region [Initial EquipmentLibrary]
                foreach (string no in List_EQLink)
                {
                    xmlHandle.SectionNode = "EquipmentLibrary" + "_" + no;
                    EquipmentModel em = new EquipmentModel();
                    em.EQNo = int.Parse(xmlHandle.GetValue2("No", ""));
                    em.EQID = xmlHandle.GetValue2("ID", "");
                    em.EQName = xmlHandle.GetValue2("Name", "");
                    em.Type = (eEquipmentType)Enum.Parse(typeof(eEquipmentType), xmlHandle.GetValue2("Type", ""));
                    em.Protocol = (eProtocol)Enum.Parse(typeof(eProtocol), xmlHandle.GetValue2("Protocol", "SOCKET"));
                    em.PortIDList = xmlHandle.GetValue2("PortID", "");
                    //em.isInitialLink = bool.Parse(xmlHandle.GetValue2("isInitialLink", "False"));
                    em.isCurrentPN = bool.Parse(xmlHandle.GetValue2("isCurrentPN", "False"));
                    em.EqNameKey = xmlHandle.GetValue2("EqNameKey", "");

                    xmlHandle.SectionNode = "EquipmentLibrary" + "_" + no + ".Recipe";
                    em.isCheckRecipeParameter = bool.Parse(xmlHandle.GetValue2("isCheckRecipeParameter", "False"));
                    em.isRecipeParameterDownload = bool.Parse(xmlHandle.GetValue2("isRecipeParameterDownload", "True"));

                    xmlHandle.SectionNode = "EquipmentLibrary" + "_" + no + ".TraceData";
                    em.isCheckTraceData = bool.Parse(xmlHandle.GetValue2("isCheckTraceData", "False"));
                    em.TraceDataTimer = int.Parse(xmlHandle.GetValue2("TraceDataTimer", "0"));
                    //em.isGenerateHistory = bool.Parse(xmlHandle.GetValue2("isGenerateHistoryFile", "False"));

                    #region [Initial TraceDataGroupLibrary]
                    //xmlHandle.SectionNode = "EquipmentLibrary" + "_" + no + ".TraceData.SampleTimer";
                    //XmlNodeList list = null;
                    //xmlHandle.GetValue2("Config." + xmlHandle.SectionNode, out list);
                    //baseLib.traceDataGroupLibrary.Initial(em.EQName, list[0].ChildNodes);
                    #endregion

                    xmlHandle.SectionNode = "EquipmentLibrary" + "_" + no + ".Control";
                    em.isControlModeSelect = bool.Parse(xmlHandle.GetValue2("isControlSelect", "False"));
                    em.isProcessCompletion = bool.Parse(xmlHandle.GetValue2("isProcessCompletion", "True"));
                    em.isAllrocessCompletion = bool.Parse(xmlHandle.GetValue2("isAllrocessCompletion", "False"));
                    em.ProcessCompletionEQ = xmlHandle.GetValue2("ProcessCompletionEQ", "") == "" ? new List<string>() : xmlHandle.GetValue2("ProcessCompletionEQ", "").Split(',').ToList();
                    em.isCCDDownloadData = bool.Parse(xmlHandle.GetValue2("isCCDDownloadData", "False"));
                    em.CCDDownloadDataEQ = xmlHandle.GetValue2("CCDDownloadDataEQ", "");
                    em.isCCDChangeData = bool.Parse(xmlHandle.GetValue2("isCCDChangeData", "False"));
                    em.DataDownloadEQ = xmlHandle.GetValue2("DataDownloadEQ", "") == "" ? new List<string>() : xmlHandle.GetValue2("DataDownloadEQ", "").Split(',').ToList();
                    em.JobDataDownloadReplyCondition = xmlHandle.GetValue2("JobDataDownloadReplyCondition", "");
                    em.isDownloadPanelList = bool.Parse(xmlHandle.GetValue2("isDownloadPanelList", "True"));
                    em.isReportLoadComplete = bool.Parse(xmlHandle.GetValue2("isReportLoadComplete", "False"));
                    em.isDownloadInnerLot = bool.Parse(xmlHandle.GetValue2("isDownloadInnerLot", "False"));
                    em.isNotDownloadJob = bool.Parse(xmlHandle.GetValue2("isNotDownloadJob", "False"));
                    em.isCheckLayerNo = bool.Parse(xmlHandle.GetValue2("isCheckLayerNo", "False"));
                    em.isTurnoverGroup = bool.Parse(xmlHandle.GetValue2("isTurnoverGroup", "False"));
                    em.isInspectEquipment = bool.Parse(xmlHandle.GetValue2("isInspectEquipment", "False"));
                    em.isMultiPort = bool.Parse(xmlHandle.GetValue2("isMultiPort", "False"));
                    em.isCarrierDownloadData = bool.Parse(xmlHandle.GetValue2("isCarrierDownloadData", "False"));
                    em.isMaterialLoadComplete = bool.Parse(xmlHandle.GetValue2("isMaterialLoadComplete", "False"));
                    em.isPnlStartSN = bool.Parse(xmlHandle.GetValue2("isPnlStartSN", "False"));
                    em.isUseDummy = bool.Parse(xmlHandle.GetValue2("isUseDummy", "False"));
                    em.isAType = bool.Parse(xmlHandle.GetValue2("isAType", "False"));
                    em.isDisconnectionDataDownload = bool.Parse(xmlHandle.GetValue2("isDisconnectionDataDownload", "False"));
                    em.isPunching = bool.Parse(xmlHandle.GetValue2("isPunching", "False"));
                    em.isReturnLine = bool.Parse(xmlHandle.GetValue2("isReturnLine", "False"));
                    em.LoadEquipmentSequence = xmlHandle.GetValue2("LoadEquipmentSequence", "");
                    em.isUseDataName = bool.Parse(xmlHandle.GetValue2("isUseDataName", "False"));
                    em.EqVendor = xmlHandle.GetValue2("EqVendor", "");
                    em.isCheckConnect = bool.Parse(xmlHandle.GetValue2("isCheckConnect", "False"));
                    em.isCheckControlMode = bool.Parse(xmlHandle.GetValue2("isCheckControlMode", "False"));



                    #region Initial SocketParaLibrary
                    xmlHandle.SectionNode = "SocketParaLibrary" + "_" + no;
                    em.socketType = (socket.SocketCommonData.HOSTMODE)Enum.Parse(typeof(socket.SocketCommonData.HOSTMODE), xmlHandle.GetValue2("SocketType", "0"));
                    #endregion

                    #region [Initial PortLibrary]
                    try
                    {
                        foreach (var s in em.List_Port)
                        {
                            s.Value.EQID = em.EQID;
                            s.Value.PortID = s.Value.Name = (ePortID)Enum.Parse(typeof(ePortID), s.Key);
                            //if (!Dic_PortEntity.ContainsKey(em.EQID))
                            //{
                            //    continue;
                            //}

                            //if (s.Value.PortID == ePortID.L01)
                            //{
                            //    s.Value.PortStatus = Dic_PortEntity[em.EQID].L01;
                            //}
                            //if (s.Value.PortID == ePortID.U01)
                            //{
                            //    s.Value.PortStatus = Dic_PortEntity[em.EQID].U01;
                            //}

                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                        return false;
                    }
                    #endregion Initial PortLibrary
                    equipmentLibary.AddEquipmentModel(em);
                    equipmentLibary.AddLineModel(lineModel);
                }
                #endregion

                #region [Initial ParaLibrary]
                foreach (EquipmentModel em in equipmentLibary.GetAllEquipmentModel())
                {
                    if (em.Protocol == eProtocol.SECS)
                    {
                        HSMSParaLibraryBase hsmsParaLibrary = new HSMSParaLibraryBase(EquipmentLibraryPath);
                        hsmsParaLibrary.Initial(em.EQNo.ToString(), em.EQID, StartupPath);
                        equipmentLibary.AddHSMSParaLibrary(hsmsParaLibrary);
                    }

                    if (em.Protocol == eProtocol.SOCKET)
                    {
                        SocketParaLibraryBase socketParaLibrary = new SocketParaLibraryBase(EquipmentLibraryPath);
                        socketParaLibrary.Initial(em.EQNo.ToString(), em.EQID);
                        equipmentLibary.AddSocketParaLibrary(socketParaLibrary);
                    }
                }
                #endregion

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return false;
            }
        }
        /// <summary>
        /// 获取"DynamicLibrary_EQ**.xml"中的信息
        /// </summary>
        /// <returns></returns>
        public bool InitialDynamic()
        {
            try
            {
                DynamicLibraryPath = Path.Combine(StartupPath, LibraryPath, MDLN + "_" + LineName);
                foreach (var v in equipmentLibary.GetAllEquipmentModel())
                {
                    string fp = Path.Combine(DynamicLibraryPath, ConstLibrary.CFG_Dynamic_NAME + "_" + v.EQID + ".xml");
                    dynamicLibrary = new Socket_DynamicLibraryBase();
                    if (dynamicLibrary.Initial(v.EQID, fp))
                    {
                        equipmentLibary.AddDynamic(dynamicLibrary);
                    }
                    else
                    {
                        MessageBox.Show(string.Format("{0} is can not initial.", fp), "EAP Start Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return false;
            }
        }
        /// <summary>
        ///获取"ConfigLibrary.xml"中的信息
        /// </summary>
        /// <returns></returns>
        public bool InitialConfig()
        {
            try
            {
                #region [Initial ConfigLibrary]
                xmlHandle.SectionPath = ConfigLibraryPath;
                if (!xmlHandle.ReadConfig())
                {
                    MessageBox.Show(string.Format("{0} is read error", ConfigLibraryPath), "EAP Start Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                #endregion

                #region [Initial AMSParaLibrary]
                xmlHandle.SectionNode = "AMSParaLibrary";
                configLibrary.AMSUrl = xmlHandle.GetValue2("Url", "http://localhost:3455/AMS_Web_Server/AMS_WebIF.asmx");
                #endregion
                #region [Initial SerialPortLibrary]
                xmlHandle.SectionNode = "SerialPortLibrary";
                configLibrary.Enable_Set = bool.Parse(xmlHandle.GetValue2("Enable", "False"));
                configLibrary.PortName = xmlHandle.GetValue2("PortName", "COM1");
                configLibrary.BaudRate = xmlHandle.GetValue2("BaudRate", "9600");
                configLibrary.Parity = xmlHandle.GetValue2("Parity", "0");
                configLibrary.DataBits = xmlHandle.GetValue2("DataBits", "8");
                configLibrary.StopBits = xmlHandle.GetValue2("StopBits", "1");

                #endregion


                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                throw;
            }
        }
        public bool InitialPortEntity()
        {
            try
            {
                #region [Initial PortEntity]
                foreach (var v in equipmentLibary.GetAllEquipmentModel())
                {
                    PortEntityPath = Path.Combine(StartupPath, "PortEntity", $"{v.EQID}_{ConstLibrary.CFG_PortEntity_FILE_NAME}");

                    string txt = "";
                    if (!System.IO.File.Exists(PortEntityPath))//如果路径不存在
                    {
                        continue;
                    }
                    StreamReader sr = new StreamReader(PortEntityPath);
                    while (!sr.EndOfStream)
                    {
                        string str = sr.ReadLine();
                        txt += str + "\n";
                    }
                    object outObj;
                    new Serialize().DeSerializeXML(txt, portEntity.GetType(), out outObj);
                    var objPortEntity = outObj as PortEntity;
                    if (!Dic_PortEntity.ContainsKey(v.EQID))
                    {
                        Dic_PortEntity.TryAdd(v.EQID, objPortEntity);
                    }
                    foreach (var s in v.List_Port)
                    {
                        if (s.Value.PortID == ePortID.L01)
                        {
                            s.Value.PortStatus =  objPortEntity.L01;
                        }
                        if (s.Value.PortID == ePortID.U01)
                        {
                            s.Value.PortStatus = objPortEntity.U01;
                        }
                    }
                }
                #endregion
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return false;
            }
        }
        /// <summary>
        /// 获取"CustomizedLibrary.xml"中的信息
        /// </summary>
        /// <returns></returns>
        public bool InitialTime()
        {
            try
            {
                #region [Initial CustomizedLibrary]
                xmlHandle.SectionPath = CustomizedLibraryPath;
                if (!xmlHandle.ReadConfig())
                {
                    MessageBox.Show(string.Format("{0} is read error", EquipmentLibraryPath), "EAP Start Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                #endregion

                #region [Initial LogLibrary]
                xmlHandle.SectionNode = "LogLibrary";
                logLibrary.WarningLog = bool.Parse(xmlHandle.GetValue2("WarningLog", "False"));
                logLibrary.Disk = xmlHandle.GetValue2("Disk", "D");
                logLibrary.DebugLog = bool.Parse(xmlHandle.GetValue2("DebugLog", "False"));
                logLibrary.InfoLog = bool.Parse(xmlHandle.GetValue2("InfoLog", "Flase"));
                logLibrary.ErrorLog = bool.Parse(xmlHandle.GetValue2("ErrorLog", "Flase"));
                logLibrary.TraceDataLog = bool.Parse(xmlHandle.GetValue2("TraceDataLog", "Flase"));
                logLibrary.LogShowCount = int.Parse(xmlHandle.GetValue2("LogShowCount", "200"));
                logLibrary.CFG_LOG_PATH = logLibrary.Disk + ":\\Log\\EAP";
                #endregion

                #region [Initial MiscLibrary]
                xmlHandle.SectionNode = "CustomizedLibrary";
                customizedLibrary.EQPAliveCheckTime = int.Parse(xmlHandle.GetValue2("EQPAliveCheckTime", "60"));
                customizedLibrary.WIPDataCheckTime = int.Parse(xmlHandle.GetValue2("WIPDataCheckTime", "60"));
                customizedLibrary.JobDataDownloadCheckTime = int.Parse(xmlHandle.GetValue2("JobDataDownloadCheckTime", "180"));
                customizedLibrary.JobDataDownloadRetryCount = int.Parse(xmlHandle.GetValue2("JobDataDownloadRetryCount", "3"));
                customizedLibrary.ProductionConditonCheckPNTime = int.Parse(xmlHandle.GetValue2("ProductionConditonCheckPNTime", "180"));
                customizedLibrary.ProductionConditonCheckLotTime = int.Parse(xmlHandle.GetValue2("ProductionConditonCheckLotTime", "10"));
                customizedLibrary.ProductionConditonCheckWorkOrderTime = int.Parse(xmlHandle.GetValue2("ProductionConditonCheckWorkOrderTime", "180"));

                //customizedLibrary.ProductionModeList = xmlHandle.GetValue2("ProductionModeList", "Manual");
                //commonModel.ProductionModeList = customizedLibrary.ProductionModeList.Split(',');
                customizedLibrary.AMSAliveCheckTime = int.Parse(xmlHandle.GetValue2("AMSAliveCheckTime", "60"));
                customizedLibrary.EqpModeRequestTime = int.Parse(xmlHandle.GetValue2("EqpModeRequestTime", "60"));
                #endregion
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return false;
            }
        }
        #endregion

        #region [配置库更新]
        /// <summary>
        /// 更新"EquipmentLibrary.xml"中的信息
        /// </summary>
        /// <param name="em"></param>
        /// <returns></returns>
        public bool UpdateEquipmentLibrary(EquipmentModel em)
        {
            try
            {
                xmlHandle.SectionPath = EquipmentLibraryPath;
                if (!xmlHandle.ReadConfig())
                {
                    MessageBox.Show(string.Format("{0} is read error", EquipmentLibraryPath), "EAP Start Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                #region [Update EquipmentLibrary]
                xmlHandle.SectionNode = "EquipmentLibrary" + "_" + em.EQNo;
                xmlHandle.SetValue2("EqNameKey", em.EqNameKey);
                // xmlHandle.SetValue2("isInitialLink", em.isInitialLink.ToString());

                xmlHandle.SectionNode = "EquipmentLibrary" + "_" + em.EQNo + ".Recipe";
                //xmlHandle.SetValue2("isCheckRecipeParameter", em.isCheckRecipeParameter.ToString());

                xmlHandle.SectionNode = "EquipmentLibrary" + "_" + em.EQNo + ".TraceData";
                xmlHandle.SetValue2("isCheckTraceData", em.isCheckTraceData.ToString());
                xmlHandle.SetValue2("TraceDataTimer", em.TraceDataTimer.ToString());
                //xmlHandle.SetValue2("isGenerateHistory", em.isGenerateHistory.ToString());

                xmlHandle.SectionNode = "EquipmentLibrary" + "_" + em.EQNo + ".Control";
                xmlHandle.SetValue2("isControlSelect", em.isControlModeSelect.ToString());
                xmlHandle.SetValue2("isProcessCompletion", em.isProcessCompletion.ToString());
                //xmlHandle.SetValue2("ProcessCompletionEQ", em.ProcessCompletionEQ.ToString());
                xmlHandle.SetValue2("isCCDDownloadData", em.isCCDDownloadData.ToString());
                xmlHandle.SetValue2("CCDDownloadDataEQ", em.CCDDownloadDataEQ.ToString());
                //xmlHandle.SetValue2("DataDownloadEQ", em.DataDownloadEQ.ToString());
                xmlHandle.SetValue2("isCCDChangeData", em.isCCDChangeData.ToString());
                xmlHandle.SetValue2("isAllrocessCompletion", em.isAllrocessCompletion.ToString());
                xmlHandle.SetValue2("JobDataDownloadReplyCondition", em.JobDataDownloadReplyCondition).ToString();
                xmlHandle.SetValue2("isDownloadPanelList", em.isDownloadPanelList.ToString());
                xmlHandle.SetValue2("isReportLoadComplete", em.isReportLoadComplete.ToString());
                xmlHandle.SetValue2("isDownloadInnerLot", em.isDownloadInnerLot.ToString());
                xmlHandle.SetValue2("isNotDownloadJob", em.isNotDownloadJob.ToString());
                xmlHandle.SetValue2("isCheckLayerNo", em.isCheckLayerNo.ToString());
                xmlHandle.SetValue2("isTurnoverGroup", em.isTurnoverGroup.ToString());
                xmlHandle.SetValue2("isInspectEquipment", em.isInspectEquipment.ToString());
                xmlHandle.SetValue2("isMultiPort", em.isMultiPort.ToString());
                xmlHandle.SetValue2("isMaterialLoadComplete", em.isMaterialLoadComplete.ToString());
                xmlHandle.SetValue2("isCarrierDownloadData", em.isCarrierDownloadData.ToString());
                xmlHandle.SetValue2("isPnlStartSN", em.isPnlStartSN.ToString());
                xmlHandle.SetValue2("isUseDummy", em.isUseDummy.ToString());
                xmlHandle.SetValue2("isDisconnectionDataDownload", em.isDisconnectionDataDownload.ToString());
                xmlHandle.SetValue2("isPunching", em.isPunching.ToString());
                xmlHandle.SetValue2("isUseDataName", em.isUseDataName.ToString());
                xmlHandle.SetValue2("isCheckConnect", em.isCheckConnect.ToString());
                xmlHandle.SetValue2("isCheckControlMode", em.isCheckControlMode.ToString());


                #endregion

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return false;
            }
        }
        /// <summary>
        /// 更新"EquipmentLibrary.xml"中的信息
        /// </summary>
        /// <param name="em"></param>
        /// <returns></returns>
        public bool UpdateEquipmentLibrary_Line(LineModel lineModel)
        {
            try
            {
                xmlHandle.SectionPath = EquipmentLibraryPath;
                if (!xmlHandle.ReadConfig())
                {
                    MessageBox.Show(string.Format("{0} is read error", EquipmentLibraryPath), "EAP Start Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                #region [Update EquipmentLibrary]
                xmlHandle.SectionNode = "Equipment";
                xmlHandle.SetValue2("LineName", lineModel.LineName);
                xmlHandle.SetValue2("isInnerLotPanelList", lineModel.isInnerLotPanelList.ToString());
                xmlHandle.SetValue2("isLoadCompleteCountCheck", lineModel.isLoadCompleteCountCheck.ToString());
                xmlHandle.SetValue2("isSubLot", lineModel.isSubLot.ToString());
                xmlHandle.SetValue2("isCheckBeforeTrackIn", lineModel.isCheckBeforeTrackIn.ToString());
                xmlHandle.SetValue2("isMultiLoadEquipment", lineModel.isMultiLoadEquipment.ToString());
                xmlHandle.SetValue2("isCheckEquipmentStatusIdle", lineModel.isMainEqpID.ToString());
                xmlHandle.SetValue2("isNeedPost", lineModel.isNeedPost.ToString());
                xmlHandle.SetValue2("isCarrierIDRequestLotInfo", lineModel.isCarrierIDRequestLotInfo.ToString());
                xmlHandle.SetValue2("isAllrocessCompletionByLot", lineModel.isAllrocessCompletionByLot.ToString());
                xmlHandle.SetValue2("isAllrocessCompletionByWorkOrder", lineModel.isAllrocessCompletionByWorkOrder.ToString());
                xmlHandle.SetValue2("StartEquipmentID", lineModel.StartEquipmentID);
                xmlHandle.SetValue2("isCheckScanCodeReport", lineModel.isCheckScanCodeReport.ToString());
                xmlHandle.SetValue2("isQPanelList", lineModel.isQPanelList.ToString());
                xmlHandle.SetValue2("isCheckEquipmentStatus", lineModel.isCheckEquipmentStatus.ToString());
                xmlHandle.SetValue2("isCheckLoadStatus", lineModel.isCheckLoadStatus.ToString());
                xmlHandle.SetValue2("isManualJobDataDoanload", lineModel.isManualJobDataDoanload.ToString());

                #endregion

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return false;
            }
        }
        /// <summary>
        /// 更新CustomizedLibrary
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        public bool UpdateCustomizedLibrary(CustomizedLibrary customized)
        {
            try
            {
                //读取配置档
                xmlHandle.SectionPath = CustomizedLibraryPath;
                if (!xmlHandle.ReadConfig())
                {
                    MessageBox.Show(string.Format("{0} is read error", CustomizedLibraryPath), "EAP Setting Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                #region [Update LogLibrary]
                xmlHandle.SectionNode = "CustomizedLibrary";
                xmlHandle.SetValue2("EQPAliveCheckTime", customized.EQPAliveCheckTime.ToString());
                xmlHandle.SetValue2("WIPDataCheckTime", customized.WIPDataCheckTime.ToString());
                xmlHandle.SetValue2("JobDataDownloadCheckTime", customized.JobDataDownloadCheckTime.ToString());
                xmlHandle.SetValue2("JobDataDownloadRetryCount", customized.JobDataDownloadRetryCount.ToString());
                xmlHandle.SetValue2("ProductionConditonCheckPNTime", customized.ProductionConditonCheckPNTime.ToString());
                xmlHandle.SetValue2("ProductionConditonCheckLotTime", customized.ProductionConditonCheckLotTime.ToString());
                //xmlHandle.SetValue2("ProductionModeList", customized.ProductionModeList.ToString());
                xmlHandle.SetValue2("EqpModeRequestTime", customized.EqpModeRequestTime.ToString());
                xmlHandle.SetValue2("ProductionConditonCheckWorkOrderTime", customized.ProductionConditonCheckWorkOrderTime.ToString());

                #endregion
                
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return false;
            }
        }
        /// <summary>
        /// 修改配置档参数
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        public bool UpdateLogLibrary(LogLibrary log)
        {
            try
            {
                //读取配置档
                xmlHandle.SectionPath = CustomizedLibraryPath;
                if (!xmlHandle.ReadConfig())
                {
                    MessageBox.Show(string.Format("{0} is read error", ConfigLibraryPath), "EAP Setting Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                #region [Update LogLibrary]
                xmlHandle.SectionNode = "LogLibrary";
                xmlHandle.SetValue2("WarningLog", log.WarningLog.ToString());
                xmlHandle.SetValue2("Disk", log.Disk);
                xmlHandle.SetValue2("TraceDataLog", log.TraceDataLog.ToString());
                xmlHandle.SetValue2("InfoLog", log.InfoLog.ToString());
                xmlHandle.SetValue2("ErrorLog", log.ErrorLog.ToString());
                xmlHandle.SetValue2("DebugLog", log.DebugLog.ToString());
                xmlHandle.SetValue2("LogShowCount", log.LogShowCount.ToString());
                #endregion

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return false;
            }
        }

        #endregion
        #endregion
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, SortedList<string, string>> MQ_EquipmentStatus()
        {
            Dictionary<string, SortedList<string, string>> Dic_EquipmentStatus = new Dictionary<string, SortedList<string, string>>();

            try
            {
                foreach (var v in equipmentLibary.GetAllEquipmentModel())
                {
                    SortedList<string, string> _sortedList = new SortedList<string, string>();
                    _sortedList.Add("设备名称", v.EQName);
                    _sortedList.Add("设备ID", v.EQID);
                    _sortedList.Add("设备运行状态", v.EQStatus.ToString());
                    if (v.EquipmentAlarmList.Count > 0) _sortedList.Add("设备警报状态", "1");
                    else _sortedList.Add("设备警报状态", "0");
                    _sortedList.Add("连线状态", v.ConnectMode.ToString());
                    _sortedList.Add("设备当前任务", v.CurrentLotID);
                    Dic_EquipmentStatus.Add(v.EQName, _sortedList);
                }
                return Dic_EquipmentStatus;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return Dic_EquipmentStatus;
            }
        }
        /// <summary>
        /// 当前产线内批次信息发送
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, SortedList<string, string>> MQ_LotInfoStatus()
        {
            Dictionary<string, SortedList<string, string>> Dic_LotInfoStatus = new Dictionary<string, SortedList<string, string>>();

            try
            {
                #region [开料线信息组合方式]
                if (lineModel.LineType == "开料线")
                {
                    foreach (var v in commonModel.GetSubLotID())
                    {
                        SortedList<string, string> _sortedList = new SortedList<string, string>();
                        _sortedList.Add("批次号", v.LotID);
                        _sortedList.Add("产品号", v.PN);
                        _sortedList.Add("批次状态", v.LotProcessStatus.ToString());
                        _sortedList.Add("批次数量", v.PanelTotalQty.ToString());
                        _sortedList.Add("LineID", MDLN);
                        Dic_LotInfoStatus.Add(v.LotID, _sortedList);
                    }
                }
                #endregion
                else
                {
                    foreach (var v in commonModel.GetLotModel())
                    {
                        SortedList<string, string> _sortedList = new SortedList<string, string>();
                        _sortedList.Add("批次号", v.LotID);
                        _sortedList.Add("产品号", v.PN);
                        _sortedList.Add("批次状态", v.LotProcessStatus.ToString());
                        _sortedList.Add("批次数量", v.PanelTotalQty.ToString());
                        _sortedList.Add("LineID", v.MainEqpID);
                        _sortedList.Add("创建时间", v.ProcessTime.ToString("yyyy/MM/dd HH:mm:ss"));
                        Dic_LotInfoStatus.Add(v.LotID, _sortedList);
                    }
                }

                return Dic_LotInfoStatus;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return Dic_LotInfoStatus;
            }
        }
        /// <summary>
        /// 获取MESDownloadInfo和EQPReportInfo值方法
        /// </summary>
        /// <param name="em"></param>
        /// <param name="MESDownloadInfo"></param>
        /// <param name="EQPReportInfo"></param>
        public void GetPPMaterialLotInfo(EquipmentModel em, out string MESDownloadInfo, out string EQPReportInfo)
        {
            try
            {
                MESDownloadInfo = string.Empty;
                EQPReportInfo = string.Empty;
                var orderResult = (from pair in MaterialPortIDBinding orderby pair.Value ascending select pair);

                foreach (var item in orderResult)
                {
                    if (!string.IsNullOrEmpty(item.Key))
                    {
                        MESDownloadInfo += item.Key;
                    }
                }
                foreach (var port in em.List_Port)
                {
                    if (!string.IsNullOrEmpty(port.Value.MaterialLotID_PP))
                    {
                        EQPReportInfo += port.Value.MaterialLotID_PP;
                    }

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                MESDownloadInfo = string.Empty;
                EQPReportInfo = string.Empty;
            }

        }
    }


}
