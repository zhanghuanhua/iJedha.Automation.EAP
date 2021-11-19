using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using iJedha.Automation.EAP.Environment;
using iJedha.Automation.EAP.Model;
using static iJedha.Automation.EAP.Core.Log;
using System.Threading;
using System.ServiceModel;
using iJedha.Automation.EAP.Core;


namespace iJedha.Automation.EAP.UI
{
    public partial class frmMain : Form
    {
        internal bool WaitClose = false;
        public bool EndPrg = false;
        private frmLogout logout;
        private frmWebAPIParameter frmWeb = new frmWebAPIParameter();
        public EAPEnvironment StartProject;

        public delegate void AddLogHandler(ListViewItem item);
        private static Thread _LogThread;
        public static ServiceHost host;
        public bool stopControl = false;

        /// <summary>
        /// 构建
        /// </summary>
        /// <param name="soluction"></param>
        public frmMain(string soluction)
        {
            try
            {
                Func<EAPEnvironment, string> StartEAP = (EAP) =>
                {
                    EAP = new EAPEnvironment(soluction);
                    string ErrMsg = EAP.Start.EndInvoke(EAP.Start.BeginInvoke(null, EAP.Start));

                    if (ErrMsg != string.Empty)
                    {
                        EAPEnvironment.ExitSystem();
                    }
                    return ErrMsg;
                };

                InitializeComponent();
                #region [环境初始化]
                string Err = StartEAP.EndInvoke(StartEAP.BeginInvoke(StartProject, null, StartProject));//跳转至Environment.Environment.Start

                if (string.IsNullOrEmpty(Err))
                {
                    _LogThread = new Thread(new ThreadStart(this.BeginLogEvent));
                    _LogThread.IsBackground = true;
                    _LogThread.Start();


                    GlobalData.LanguageChange(eLanguage.Chinese.ToString());
                    SetLanguage();
                    solutiontoolStripStatusLabel.Text = soluction;
                    EAPEnvironment.commonLibrary.Version = Application.ProductVersion;
                    this.notifyIcon.Text = string.Format("iJDEAP : <{0}> <{1}>", EAPEnvironment.commonLibrary.LineName, Application.ProductVersion);
                    this.notifyIcon.MouseDoubleClick += new MouseEventHandler(notifyIcon_MouseDoubleClick);
                    this.notifyIcon.Icon = UI.Properties.Resources.EAP1;
                    logout = new frmLogout(this);
                    Environment.BaseComm.LogMsg(Core.Log.LogLevel.Info, string.Format("{0} iJDEAP初始化建置完成.", EAPEnvironment.commonLibrary.LineName));
                }
                else
                {
                    MessageBox.Show(Err);
                    Process.GetCurrentProcess().Kill();
                }
                #endregion
            }
            catch (Exception ex)
            {
                Environment.BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
            }
        }

        /// <summary>
        /// 构建
        /// </summary>
        /// <param name="appPath"></param>
        /// <param name="lineNo"></param>
        /// <param name="lineName"></param>
        /// <param name="soluction"></param>
        public frmMain(string appPath, string lineNo, string lineName, string soluction)
        {
            try
            {
                Func<EAPEnvironment, string> StartEAP = (EAP) =>
                {
                    EAP = new EAPEnvironment(appPath, lineNo, lineName, soluction);
                    string ErrMsg = EAP.Start.EndInvoke(EAP.Start.BeginInvoke(null, EAP.Start));

                    if (ErrMsg != string.Empty)
                    {
                        EAPEnvironment.ExitSystem();
                    }
                    return ErrMsg;
                };

                InitializeComponent();

                #region [环境初始化]
                string Err = StartEAP.EndInvoke(StartEAP.BeginInvoke(StartProject, null, StartProject));

                if (string.IsNullOrEmpty(Err))
                {
                    _LogThread = new Thread(new ThreadStart(this.BeginLogEvent));
                    _LogThread.IsBackground = true;
                    _LogThread.Start();

                    GlobalData.LanguageChange(eLanguage.Chinese.ToString());
                    SetLanguage();
                    solutiontoolStripStatusLabel.Text = soluction;
                    EAPEnvironment.commonLibrary.Version = Application.ProductVersion;
                    this.notifyIcon.Text = string.Format("iJDEAP : <{0}> <{1}>", EAPEnvironment.commonLibrary.LineName, Application.ProductVersion);
                    this.notifyIcon.MouseDoubleClick += new MouseEventHandler(notifyIcon_MouseDoubleClick);
                    this.notifyIcon.Icon = UI.Properties.Resources.EAP1;
                    logout = new frmLogout(this);
                    Environment.BaseComm.LogMsg(Core.Log.LogLevel.Info, string.Format("{0} iJDEAP初始化建置完成.", EAPEnvironment.commonLibrary.LineName));

                }
                else
                {
                    MessageBox.Show(Err);
                    Process.GetCurrentProcess().Kill();
                }
                #endregion
            }
            catch (Exception ex)
            {
                Environment.BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
            }
        }

        /// <summary>
        /// 中英文切换
        /// </summary>
        private void SetLanguage()
        {
            this.viewToolStripMenuItem.Text = GlobalData.GlobalLanguage.Menu_View;
            this.settingToolStripMenuItem.Text = GlobalData.GlobalLanguage.Menu_Setting;
            this.operationToolStripMenuItem.Text = GlobalData.GlobalLanguage.Menu_Operation;
            this.logToolStripMenuItem.Text = GlobalData.GlobalLanguage.Menu_Log;
            this.aboutToolStripMenuItem.Text = GlobalData.GlobalLanguage.Menu_About;
            this.exitToolStripMenuItem.Text = GlobalData.GlobalLanguage.Menu_Exit;

            statustoolStripStatusLabel.Text = GlobalData.GlobalLanguage.Label_Status;
            modetoolStripStatusLabel.Text = GlobalData.GlobalLanguage.Label_Mode;
            nowTimetoolStripStatusLabel.Text = GlobalData.GlobalLanguage.Label_StartTime;
            envirmenttoolStripStatusLabel.Text = GlobalData.GlobalLanguage.Label_Envirment;

            lvwStatusList.Columns[0].Text = GlobalData.GlobalLanguage.Listview_1;
            lvwStatusList.Columns[1].Text = GlobalData.GlobalLanguage.Listview_2;
            lvwStatusList.Columns[2].Text = GlobalData.GlobalLanguage.Listview_3;
            lvwStatusList.Columns[3].Text = GlobalData.GlobalLanguage.Listview_4;
            lvwStatusList.Columns[4].Text = GlobalData.GlobalLanguage.Listview_5;
            lvwStatusList.Columns[5].Text = GlobalData.GlobalLanguage.Listview_6;
            
        }
      
        void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                if (this.Visible == false)
                {
                    this.Show();
                    this.TopLevel = true;
                    this.timer1.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                Environment.BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
            }
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (!WaitClose)
                {
                    e.Cancel = true;
                    if (!logout.Visible)
                    {
                        this.TopMost = false;
                        logout.Show();
                    }
                }
                else
                {
                    string Msg = string.Empty;
                    switch (e.CloseReason)
                    {
                        case CloseReason.None:
                            Msg = "Shut down due to not defined or can not judge";
                            break;
                        case CloseReason.WindowsShutDown:
                            Msg = "Before shutting down the operating system is shutting down all applications.";
                            break;
                        case CloseReason.MdiFormClosing:
                            Msg = "The Multiple Document Interface is closing (MDI) form the parent form.";
                            break;
                        case CloseReason.UserClosing:
                            Msg = "Users, through user interface (UI) close the form, for example, the form window, click on the [Close] button to select the window control menu [Close] or press ALT + F4.";
                            break;
                        case CloseReason.TaskManagerClosing:
                            Msg = "Microsoft Windows Task Manager is closing the application.";
                            break;
                        case CloseReason.FormOwnerClosing:
                            Msg = "Is shutting down the master form.";
                            break;
                        case CloseReason.ApplicationExitCall:
                            Msg = "Has been invoked (Invoke) Application class Exit method.";
                            break;
                    }
                    Environment.BaseComm.LogMsg(Core.Log.LogLevel.Info, $"CloseReason:{EAPEnvironment.commonLibrary.MDLN}:{Msg}");
                    EndPrg = true;

                    if (host != null)
                    {
                        host.Close();
                    }
                    EAPEnvironment.ExitSystem();
                    notifyIcon.Dispose();
                    //Process.GetCurrentProcess().Kill();
                }
            }
            catch (Exception ex)
            {
                Environment.BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
            }
        }

        /// <summary>
        /// 更新EAP主页面的物件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                RefreshLayout();
            }
            catch (Exception ex)
            {
                Environment.BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
            }
        }
        private void RefreshLayout()
        {
            try
            {
                tsslMode.Text = EAPEnvironment.commonLibrary.commonModel.CurMode.ToString();
                tsslLineStatus.Text = EAPEnvironment.commonLibrary.commonModel.LineStatus.ToString();

                #region [根据当前MES Mode切换控件颜色]
                switch (tsslMode.Text)
                {
                    case "Manual":
                        tsslMode.BackColor = Color.MistyRose;
                        break;
                    case "Auto":
                        tsslMode.BackColor = Color.PaleGreen;
                        break;
                    default:
                        break;
                }
                #endregion                

                #region [根据与MQ的连接状态切换控件颜色]
                if (EAPEnvironment.commonLibrary.MQConnectedStatus != EAPEnvironment.commonLibrary.PreMQConnectedStatus)
                {
                    if (EAPEnvironment.commonLibrary.MQConnectedStatus)
                    {
                        tsslMQAlive.Image.Dispose();
                        tsslMQAlive.Image = Properties.Resources.BitOn;
                    }
                    else
                    {
                        tsslMQAlive.Image.Dispose();
                        tsslMQAlive.Image = Properties.Resources.BitOff;
                    }
                    EAPEnvironment.commonLibrary.PreMQConnectedStatus = EAPEnvironment.commonLibrary.MQConnectedStatus;
                }
                #endregion

                #region [根据MES的连接状态切换控件颜色]
                if (EAPEnvironment.commonLibrary.HostConnectMode != EAPEnvironment.commonLibrary.PreHostConnectMode)
                {
                    if (EAPEnvironment.commonLibrary.HostConnectMode == LibraryBase.eHostConnectMode.CONNECT)
                    {
                        tsslIsAlive.Image.Dispose();
                        tsslIsAlive.Image = Properties.Resources.BitOn;
                    }
                    else
                    {
                        tsslIsAlive.Image.Dispose();
                        tsslIsAlive.Image = Properties.Resources.BitOff;
                    }
                    EAPEnvironment.commonLibrary.PreHostConnectMode = EAPEnvironment.commonLibrary.HostConnectMode;
                }
                #endregion

                #region [Status Show]
                {
                    for (int i = 0; i < lvwStatusList.Items.Count; i++)
                    {
                        var name = lvwStatusList.Items[i].Text;
                        EquipmentModel em = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByName(name);

                        LotModel lm = EAPEnvironment.commonLibrary.commonModel.GetLotModelByLotID(em.CurrentLotID);

                        string carrier1 = "";
                        string carrier2 = "";
                        switch (em.ConnectMode)
                        {
                            //特创1期设备不会上报ControlMode，故写在这里
                            case eConnectMode.DISCONNECT:
                                lvwStatusList.Items[i].SubItems[1].Text = "异常";
                                lvwStatusList.Items[i].SubItems[2].Text = "UNKNOW";
                                break;
                            case eConnectMode.CONNECT:
                                lvwStatusList.Items[i].SubItems[1].Text = "正常";
                                lvwStatusList.Items[i].SubItems[2].Text = "本地";
                                break;
                            default:
                                break;
                        }
                        //switch (em.ControlMode)
                        //{
                        //    case eControlMode.UNKNOW:
                        //        lvwStatusList.Items[i].SubItems[2].Text = em.ControlMode.ToString();
                        //        break;
                        //    case eControlMode.LOCAL:
                        //        lvwStatusList.Items[i].SubItems[2].Text = "本地";
                        //        break;
                        //    case eControlMode.REMOTE:
                        //        lvwStatusList.Items[i].SubItems[2].Text = "在线";
                        //        break;
                        //    default:
                        //        break;
                        //}
                        switch (em.EQStatus)
                        {
                            case eEQSts.Unknown:
                                lvwStatusList.Items[i].SubItems[3].Text = "Unknown";
                                break;
                            case eEQSts.Run:
                                lvwStatusList.Items[i].SubItems[3].Text = "运行";
                                break;
                            case eEQSts.Pause:
                                lvwStatusList.Items[i].SubItems[3].Text = "暂停";
                                break;
                            case eEQSts.Idle:
                                lvwStatusList.Items[i].SubItems[3].Text = "待机";
                                break;
                            case eEQSts.Down:
                                lvwStatusList.Items[i].SubItems[3].Text = "宕机";
                                break;
                            case eEQSts.PM:
                                lvwStatusList.Items[i].SubItems[3].Text = "保养";
                                break;
                            case eEQSts.Ready:
                                lvwStatusList.Items[i].SubItems[3].Text = "准备";
                                break;
                            default:
                                break;
                        }
                        //lvwStatusList.Items[i].SubItems[1].Text = em.ConnectMode.ToString();
                        //lvwStatusList.Items[i].SubItems[2].Text = em.ControlMode.ToString();
                        //lvwStatusList.Items[i].SubItems[3].Text = em.EQStatus.ToString();
                        lvwStatusList.Items[i].SubItems[4].Text = em.CurrentLotID;
                        if (EAPEnvironment.commonLibrary.lineModel.LineType == "开料线")
                        {
                            Lot subLot = EAPEnvironment.commonLibrary.commonModel.GetLotModelBySubLotID(em.CurrentLotID);
                            if (subLot != null)
                            {
                                lvwStatusList.Items[i].SubItems[5].Text = subLot.PanelTotalQty.ToString();
                            }
                        }
                        else
                        {
                            if (lm != null)
                            {
                                lvwStatusList.Items[i].SubItems[5].Text = lm.PanelTotalQty.ToString();
                            }
                        }


                        if (em.ConnectMode == eConnectMode.DISCONNECT)
                        {
                            lvwStatusList.Items[i].BackColor = Color.MistyRose;
                        }
                        else
                        {
                            lvwStatusList.Items[i].BackColor = Color.PaleGreen;
                        }
                    }
                }
                #endregion

                lblShowMessage.Text = DisplayMessage;
                #region [显示Lot信息]

                if (EAPEnvironment.commonLibrary.ShowLotInfoMessage == eLotinfo.WaitingUp.ToString()) // 待上机
                {
                    lblLotInfomationMessgae.Text = "Lot" + "<" + EAPEnvironment.commonLibrary.MainLotID + ">" + "待上机...";
                }
                else if (EAPEnvironment.commonLibrary.ShowLotInfoMessage == eLotinfo.PartUp.ToString()) // 上机
                {
                    lblLotInfomationMessgae.Text = "Lot" + "<" + EAPEnvironment.commonLibrary.MainLotID + ">" + "已上机，生产中...";
                }
                else if (EAPEnvironment.commonLibrary.ShowLotInfoMessage == eLotinfo.WaitiongLower.ToString()) // 下机
                {
                    lblLotInfomationMessgae.Text = "Lot" + "<" + EAPEnvironment.commonLibrary.MainLotID + ">" + "已下机...";
                }
                else
                {
                    lblLotInfomationMessgae.Text = "";
                }
                #endregion

                #region [获取Log文件的大小]
                frmLogManagement frmlog = new frmLogManagement();
                frmlog.ReadLogSize();
                #endregion

                #region [设备和EAP连线后，Initial数据]
                var lem = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel();

                foreach (var v in lem)
                {
                    if (v.ConnectMode == eConnectMode.CONNECT)
                    {
                        if (v.ConnectMode != v.OldConnectMode)
                        {
                            //Thread.Sleep(2000);
                            //new HostService.HostService().InitialDataRequest(v.EQID);
                            v.OldConnectMode = v.ConnectMode;
                        }
                    }
                    else
                    {
                        v.OldConnectMode = v.ConnectMode;
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                Environment.BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
            }
        }
        /// <summary>
        ///  Trace,Info,Warn, Error,Fatal
        /// </summary>
        private void BeginLogEvent()
        {
            try
            {
                while (true)
                {
                    try
                    {
                        if (stopControl)
                        {
                            continue;
                        }
                        int count = 0;
                        while (base.IsHandleCreated && (Dic_LogQueue.Count > 0) && count < 20)
                        {
                            ListViewItem item = null;
                            Dic_LogQueue.TryDequeue(out item);
                            this.listBoxLogView.BeginInvoke(new AddLogHandler(this.AddLogData), new object[] { item });
                            count++;
                        }
                        Thread.Sleep(5);
                    }
                    catch
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                Environment.BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
            }
        }

        private void AddLogData(ListViewItem item)
        {
            if (this.listBoxLogView.Items.Count > EAPEnvironment.commonLibrary.logLibrary.LogShowCount)
            {
                this.listBoxLogView.Items.Clear();
            }
            this.listBoxLogView.Items.Insert(0, item);
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            try
            {
                this.Text = this.notifyIcon.Text;
                tsslNowTime.Text = System.DateTime.Now.ToString("G");
                int i = 0;
                foreach (EquipmentModel em in EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
                {
                    PortModel port1 = em.GetPortModelByPortID(ePortID.L01.ToString());
                    PortModel port2 = em.GetPortModelByPortID(ePortID.U01.ToString());
                    string carrier1 = "";
                    string carrier2 = "";
                    if (port1 != null)
                    {
                        carrier1 = port1.CarrierID;
                    }
                    if (port2 != null)
                    {
                        carrier2 = port2.CarrierID;
                    }

                    ListViewItem item = new ListViewItem();
                    item = new ListViewItem(new string[] { em.EQName,em.ConnectMode.ToString(),em.ControlMode .ToString()
                        ,em.EQStatus.ToString(),em.PPID,carrier1,carrier2});

                    if (em.ConnectMode == eConnectMode.DISCONNECT)
                    {
                        item.BackColor = Color.MistyRose;
                    }
                    else
                    {
                        item.BackColor = Color.PaleTurquoise;
                    }
                    lvwStatusList.Items.Add(item);
                    i++;
                }
                toolStripStatusLabel3.Visible = false;
                toolStripStatusLabel2.Visible = false;
                toolStripStatusLabel1.Visible = false;
                tsslLineStatus.Visible = false;
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
            }

        }



        /// <summary>
        /// 限制SML Log显示行数
        /// </summary>
        //private void LimitLine()
        //{
        //    if (this.rtbLog.Lines.Length > EAPEnvironment.commonLibrary.logLibrary.LogShowCount * 5)
        //    {
        //        int moreLines = rtbLog.Lines.Length - EAPEnvironment.commonLibrary.logLibrary.LogShowCount * 5;
        //        string[] lines = rtbLog.Lines;
        //        Array.Copy(lines, moreLines, lines, 0, EAPEnvironment.commonLibrary.logLibrary.LogShowCount * 5);
        //        Array.Resize(ref lines, EAPEnvironment.commonLibrary.logLibrary.LogShowCount * 5);
        //        rtbLog.Lines = lines;
        //    }
        //}

        #region [UI菜单]
        /// <summary>
        /// About
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmInformation info = new frmInformation();
            info.ShowDialog();
            info.Close();
        }
        /// <summary>
        /// 退出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (!WaitClose)
                {
                    if (!logout.Visible)
                    {
                        this.TopMost = false;
                        logout.Show();
                    }
                }
                else
                {
                    this.notifyIcon.Visible = false;
                    string Msg = string.Empty;

                    notifyIcon.Dispose();
                }
            }
            catch (Exception ex)
            {
                Environment.BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
            }
        }
        /// <summary>
        /// HSMS Connect
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void hSMSConnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //frmHSMSParameter para = new frmHSMSParameter();
            //para.ShowDialog();
            //para.Close();
        }
        /// <summary>
        /// 动态链接库
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dynamicEventToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //frmDynamicEvent para = new frmDynamicEvent();
            //para.ShowDialog();
            //para.Close();
        }
        /// <summary>
        /// Trace Data库
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void traceDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmTraceData para = new frmTraceData();
            para.ShowDialog();
            para.Close();
        }
        /// <summary>
        /// 设备库
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void eqToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmEquipment para = new frmEquipment();
            para.ShowDialog();
            para.Close();
        }
        /// <summary>
        /// Line
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmLine para = new frmLine();
            para.ShowDialog();
            para.Close();
        }

        /// <summary>
        /// 英文切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void englishToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GlobalData.LanguageChange(eLanguage.English.ToString());
            SetLanguage();
        }
        /// <summary>
        /// 中文切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chineseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GlobalData.LanguageChange(eLanguage.Chinese.ToString());
            SetLanguage();
        }
        /// <summary>
        /// Recipe Body View
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ppBodyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //frmPPBody para = new frmPPBody();
            //para.ShowDialog();
            //para.Close();
        }
        /// <summary>
        /// 基本控制操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void commonCommandToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //frmCommonCommand para = new frmCommonCommand();
            //para.ShowDialog();
            //para.Close();

        }
        /// <summary>
        /// 远程控制操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void remoteCommandToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmRemoteCommand para = new frmRemoteCommand();
            para.ShowDialog();
            para.Close();
        }

        /// <summary>
        /// Log View
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBoxLogView_DoubleClick(object sender, EventArgs e)
        {
            ListView lv = (ListView)sender;
            string log = lv.FocusedItem.Text;
            frmLogoutDetail Detail = new frmLogoutDetail();
            Detail.LogDetail = log;
            Detail.Show();
        }

        /// <summary>
        /// MQ Connect
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mQConnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmRBMQParameter para = new frmRBMQParameter();
            para.ShowDialog();
            para.Close();
        }
        /// <summary>
        /// Log Management
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void logToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmLogManagement para = new frmLogManagement();
            para.ShowDialog();
            para.Close();
        }
        #endregion

        private void alarmLibraryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmAlarm alarm = new frmAlarm();
            alarm.ShowDialog();
            alarm.Close();
        }
        private void customizedLibraryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmCustomized customized = new frmCustomized();
            customized.ShowDialog();
            customized.Close();
        }

        private void webAPIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmWebAPIParameter para = new frmWebAPIParameter();
            para.ShowDialog();
            para.Close();
        }

        private void toMESToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmWebAPIRequest WebAPIRequest = new frmWebAPIRequest();
            WebAPIRequest.ShowDialog();
            WebAPIRequest.Close();
        }

        private void toEAPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmRemoteCommand para = new frmRemoteCommand();
            para.ShowDialog();
            para.Close();
        }
        private void sOCKETConnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmSOCKETParameter SocketParameter = new frmSOCKETParameter();
            SocketParameter.ShowDialog();
            SocketParameter.Close();
        }

        private void LblLogStopControl_Click(object sender, EventArgs e)
        {
            if (!stopControl)
            {
                stopControl = true;
            }
            else
            {
                stopControl = false;
            }
                
        }
    }
}
