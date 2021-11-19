//******************************************************************
//   系统名称 : iJedha.Automation.EAP.Environment
//   文件概要 : EAPEnvironment
//   作    者 : 张明哲
//   <更新履历>
//   1.0.0.0    2020/2/6 10:40:34
//******************************************************************
using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Library;
using iJedha.Automation.EAP.LibraryBase;
using iJedha.Automation.EAP.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using iJedha.Automation.EAP.MQService;
using System.Net.NetworkInformation;
using System.Linq;
using System.Threading;
using iJedha.Automation.AMS.Service;
using iJedha.Automation.EAP.Serial;
using System.IO.Ports;

namespace iJedha.Automation.EAP.Environment
{

    public partial class EAPEnvironment
    {
        #region
        public static eSolution sysSolution;
        public static string StartupPath;
        public static string LineNo;
        public static string LineName;
        #endregion

        #region [Interface]
        public static ConcurrentDictionary<string, socket.SocketBasic> Dic_TCPSocketAp { get; set; }
        public static WebAPIServer WebAPIServerAp;
        public static RBMQService MQPublisherAp;
        public static AMSService AMSServiceAp;
        public static SerialPortService SerialPortServiceAp;
        #endregion
        public static BaseComm EAPAp;
        public static CommonLibrary commonLibrary;
        public static sFlowControl FlowControl;

        /// <summary>
        /// 构造
        /// </summary>
        public EAPEnvironment()
        {
        }
        /// <summary>
        /// 构造
        /// </summary>
        public EAPEnvironment(string solution)
        {
            sysSolution = (eSolution)Enum.Parse(typeof(eSolution), solution);
        }
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="startuppath"></param>
        /// <param name="lineno"></param>
        /// <param name="linename"></param>
        /// <param name="solution"></param>
        public EAPEnvironment(string startuppath, string lineno, string linename, string solution)
        {
            sysSolution = (eSolution)Enum.Parse(typeof(eSolution), solution);
            StartupPath = startuppath;
            LineNo = lineno;
            LineName = linename;
        }
        #region [Start Controller]
        public Func<string> Start = () =>
        {
            string ErrMsg = string.Empty;
            try
            {
                if (!SetupConfigFile())
                {
                    ErrMsg = string.Format("Config File.{0} Setup Error", ConstLibrary.CFG_Config_FILE_NAME);
                }
                else
                {
                    if (!SetupConfigLibrary())//读取"ConfigLibrary"文档
                    {
                        ErrMsg = string.Format("ConfigLibrary File.{0} Setup Error", ConstLibrary.CFG_Equipment_FILE_NAME);
                    }
                    
                    else if (!SetupEquipmentLibrary())//读取"EquipmentLibrary"文档
                    {
                        ErrMsg = string.Format("Equipment File.{0} Setup Error", ConstLibrary.CFG_Equipment_FILE_NAME);
                    }
                    else if (!SetupPortEntity())//读取PortEntity文档
                    {
                        ErrMsg = string.Format("Customized File.{0} Setup Error", ConstLibrary.CFG_PortEntity_FILE_NAME);
                    }
                    else if (!SetupDynamicLibrary())//读取"DynamicLibrary"文档
                    {
                        ErrMsg = string.Format("Dynamic File.{0} Setup Error", ConstLibrary.CFG_Dynamic_FILE_NAME);
                    }
                    else if (!SetupErrorCodeLibrary())//读取"ErrorCode"文档
                    {
                        ErrMsg = string.Format("ErrorCode File.{0} Setup Error", ConstLibrary.CFG_ErrorCode_FILE_NAME);
                    }
                    else if (!SetupCustomizedLibrary())//读取"CustomizedLibrary"文档
                    {
                        ErrMsg = string.Format("Customized File.{0} Setup Error", ConstLibrary.CFG_Customized_FILE_NAME);
                    }
                    else if (!SetupScenarioLibrary())//读取"ScenarioLibrary"文档，未使用
                    {
                        ErrMsg = string.Format("Scenario File.{0} Setup Error", ConstLibrary.CFG_Scenario_FILE_NAME);
                    }
                   
                }
                if (ErrMsg == string.Empty)
                {
                    EAPAp = new BaseComm(commonLibrary.MDLN);
                    EAPAp.SystemParameterSetup = SetupBrick;
                    if (EAPAp.EnvironmentSetup(out ErrMsg))
                    {
                        SystemStart(out ErrMsg);
                    }
                }

                return ErrMsg;
            }
            catch (Exception e)
            {
                return e.ToString();
            }
            finally
            {
            }
        };
        #endregion

        /// <summary>
        /// Setup Config File
        /// </summary>
        /// <returns></returns>
        static bool SetupConfigFile()
        {
            try
            {
                #region [Initial]
                EAPEnvironment.commonLibrary = new CommonLibrary();
                if (sysSolution == eSolution.Develop)
                {
                    if (!EAPEnvironment.commonLibrary.InitialCommon(sysSolution))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!EAPEnvironment.commonLibrary.InitialCommon(StartupPath, LineNo, LineName, sysSolution))
                    {
                        return false;
                    }
                }
                #endregion

                #region
                XmlDocument xmldoc = new XmlDocument();
                string filePath = $"{Application.ExecutablePath}.config";
                xmldoc.Load(filePath);
                XmlNode xNode = xmldoc["configuration"]["nlog"].SelectSingleNode("variable");
                xNode.Attributes["value"].Value = commonLibrary.LineName;
                xmldoc.Save(filePath);
                #endregion

                return true;
            }
            catch (Exception ex)
            {
                Environment.BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return false;
            }
        }

        /// <summary>
        /// Setup Config Library
        /// </summary>
        /// <returns></returns>
        static bool SetupConfigLibrary()
        {
            try
            {
                #region [Initial Equipment.xml]
                if (!EAPEnvironment.commonLibrary.InitialConfig())
                {
                    return false;
                }
                #endregion
                return true;
            }
            catch (Exception ex)
            {
                Environment.BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return false;
            }
        }

        /// <summary>
        /// Setup Equipment Library
        /// </summary>
        /// <returns></returns>
        static bool SetupEquipmentLibrary()
        {
            try
            {
                #region [Initial Equipment.xml]
                if (!EAPEnvironment.commonLibrary.InitialEquipment())
                {
                    return false;
                }
                #endregion
                return true;
            }
            catch (Exception ex)
            {
                Environment.BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return false;
            }
        }
        /// <summary>
        /// Setup Dynamic Library
        /// </summary>
        /// <returns></returns>
        static bool SetupDynamicLibrary()
        {
            try
            {
                #region [Initial Dynamic.xml]
                if (!EAPEnvironment.commonLibrary.InitialDynamic())
                {
                    return false;
                }
                #endregion
                return true;
            }
            catch (Exception ex)
            {
                Environment.BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return false;
            }
        }
        /// <summary>
        /// Setup ErrorCode Library
        /// </summary>
        /// <returns></returns>
        static bool SetupErrorCodeLibrary()
        {
            try
            {
                #region [Initial ErrorCode.xml]
                EAPEnvironment.commonLibrary.baseLib.LoadErrorCodeLib(commonLibrary.ErrorCodeLibraryPath);
                #endregion

                return true;
            }
            catch (Exception ex)
            {
                Environment.BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return false;
            }
        }
        /// <summary>
        /// Setup Customized Library
        /// </summary>
        /// <returns></returns>
        static bool SetupCustomizedLibrary()
        {
            try
            {
                #region [Initial Customized.xml]
                if (commonLibrary.CustomizedLibraryPath == string.Empty)
                {
                    return true;
                }
                EAPEnvironment.commonLibrary.InitialTime();
                #endregion

                return true;
            }
            catch (Exception ex)
            {
                Environment.BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return false;
            }
        }

        /// <summary>
        /// Setup Scenario Library
        /// </summary>
        /// <returns></returns>
        static bool SetupScenarioLibrary()
        {
            try
            {
                #region [Initial Scenario.xml]
                if (commonLibrary.ScenarioLibraryPath == string.Empty)
                {
                    return true;
                }
                FlowControl = sFlowModule.InitialScenarioFlowControl();
                if (!FlowControl.LoadScenario(commonLibrary.ScenarioLibraryPath))
                {
                    return false;
                }
                #endregion

                return true;
            }
            catch (Exception ex)
            {
                Environment.BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return false;
            }
        }
        static bool SetupPortEntity()
        {
            try
            {
                #region [Initial PortEntity.xml]
                if (!EAPEnvironment.commonLibrary.InitialPortEntity())
                {
                    return false;
                }
                #endregion
                return true;
            }
            catch (Exception ex)
            {
                Environment.BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return false;
            }
        }
        /// <summary>
        /// Setup Communication Interface
        /// </summary>
        /// <returns></returns>
        static string SetupBrick()
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                MultiSocketServiceStart();
                MQServiceStart();
                WebAPIServerStart();
                SerialPortServiceStart();
                AMSServiceAp = new AMSService(commonLibrary.configLibrary.AMSUrl);

                return sb.ToString();
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
            finally
            {

            }
        }
        /// <summary>
        /// System Start
        /// </summary>
        /// <param name="ErrMsg"></param>
        static void SystemStart(out string ErrMsg)
        {
            try
            {
                ErrMsg = string.Empty;
                StringBuilder errMsg = new StringBuilder();

                //处理Socket消息
                if (!EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_SocketMsgHandle, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_SocketMsgHandle, 200, true))
                {
                    errMsg.Append(string.Format("{0}", ConstLibrary.CONST_DLL_RULE_SocketMsgHandle));
                }
                //设备Trace Data收集
                var ems = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel();
                foreach (var em in ems)
                {
                    em.TraceDataCollectTime = DateTime.Now;
                }
                if (!EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_TraceDataCollect, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_TraceDataCollect, 1000, true))
                {
                    errMsg.Append(string.Format("{0}", ConstLibrary.CONST_DLL_RULE_TraceDataCollect));
                }


                #region 数据收集压力测试
                //设备Trace Data收集
                //var emsTest = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel();
                //foreach (var em in emsTest)
                //{
                //    em.TraceDataCollectTimeTest = DateTime.Now;
                //}
                //if (!EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_TraceDataCollectTest, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_TraceDataCollectTest, 1000, true))
                //{
                //    errMsg.Append(string.Format("{0}", ConstLibrary.CONST_DLL_RULE_TraceDataCollectTest));
                //}
                #endregion

                //MES连线监控
                if (!EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_AliveCheck, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_AliveCheck, 1000, true))
                {
                    errMsg.Append(string.Format("{0}", ConstLibrary.CONST_DLL_RULE_AliveCheck));
                }

                //主设备WIP Data资料收集
                //if (!EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_WIPDataCheck, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_WIPDataCheck, commonLibrary.customizedLibrary.WIPDataCheckTime * 1000, true))
                //{
                //    errMsg.Append(string.Format("{0}", ConstLibrary.CONST_DLL_RULE_WIPDataCheck));
                //}

                //处理串口消息
                if (!EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_RsMsgHandle, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_RsMsgHandle, 200, true))
                {
                    errMsg.Append(string.Format("{0}", ConstLibrary.CONST_DLL_RULE_RsMsgHandle));
                }

                if (errMsg.Length != 0)
                {
                    ErrMsg = string.Format("Register Error({0})", errMsg.ToString());
                    return;
                }

                //开启TCPSocket Server
                foreach (KeyValuePair<string, socket.SocketBasic> tcpSocketServer in Dic_TCPSocketAp)
                {
                    tcpSocketServer.Value.Run();

                }

                //开启WebAPI Server
                if (WebAPIServerAp != null)
                {
                    WebAPIServerAp.StartAPIServer();
                    BaseComm.LogMsg(Log.LogLevel.Info, $"<{commonLibrary.LineName}> <{WebAPIServerAp.IP}:{WebAPIServerAp.Port}> WebAPI通道开启.");
                    commonLibrary.isWebServerStart = true;
                }

                if (commonLibrary.configLibrary.Enable_Set && SerialPortServiceAp != null && SerialPortServiceAp.ComDevice.IsOpen == false)
                {
                    SerialPortServiceAp.ComDevice.Open();
                    //向ComDevice.DataReceived（是一个事件）注册一个方法Com_DataReceived，当端口类接收到信息时时会自动调用Com_DataReceived方法
                    SerialPortServiceAp.ComDevice.DataReceived += Com_DataReceived;
                    BaseComm.LogMsg(Core.Log.LogLevel.Info, string.Format("<{0}> SerialPort服务开启.", commonLibrary.LineName));
                }


                //开启MQ Client
                if (MQPublisherAp != null)
                {
                    if (MQPublisherAp.Initial())
                    {
                        commonLibrary.MQConnectedStatus = true;
                    }
                    else
                    {
                        commonLibrary.MQConnectedStatus = false;
                    }
                    MQPublisherAp.Open();
                    BaseComm.LogMsg(Core.Log.LogLevel.Info, string.Format("<{0}> MQ通道开启.", commonLibrary.LineName, MQPublisherAp.ServerUrlString));
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Start Environment Fail", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ErrMsg = ex.ToString();
            }
        }

        public static string receiveData { get; set; } = "";
        /// <summary>
        /// 一旦ComDevice.DataReceived事件发生，就将从串口接收到的数据显示到接收端对话框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Com_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            do
            {
                int count = SerialPortServiceAp.ComDevice.BytesToRead;
                if (count <= 0)
                    break;
                byte[] readBuffer = new byte[count];

                Application.DoEvents();
                SerialPortServiceAp.ComDevice.Read(readBuffer, 0, count);
                receiveData += System.Text.Encoding.Default.GetString(readBuffer);

            } while (SerialPortServiceAp.ComDevice.BytesToRead > 0);



            //开辟接收缓冲区
            //byte[] ReDatas = new byte[SerialPortServiceAp.ComDevice.BytesToRead];

            //////从串口读取数据
            //SerialPortServiceAp.ComDevice.Read(ReDatas, 0, ReDatas.Length);
            //receiveData = System.Text.Encoding.Default.GetString(ReDatas);
        }

        /// <summary>
        /// Socket Service Start
        /// </summary>
        public static void MultiSocketServiceStart()
        {
            #region [Socket Interface Initial]
            Dic_TCPSocketAp = new ConcurrentDictionary<string, socket.SocketBasic>();
            foreach (EquipmentModel em in commonLibrary.equipmentLibary.GetAllEquipmentModel())
            {
                if (em.Protocol == eProtocol.SOCKET)
                {
                    SocketParaLibraryBase socketParaLibrary = commonLibrary.equipmentLibary.GetSocketParaLibrary(em.EQID);
                    if (socketParaLibrary != null)
                    {
                        if (!socketParaLibrary.Enable)
                        {
                            continue;
                        }
                        if (socketParaLibrary.ConnectType == eConnect_Mode.Passive)
                        {
                            socket.SocketBasic _TCPSocketAp = new socket.TCPSocketServer(socketParaLibrary.LocalIP, Convert.ToUInt16(socketParaLibrary.LocalPort), 10,
                                socket.SocketCommonData.ENCODEMODE.ENCODEMODE_ASCII);
                            _TCPSocketAp.DataReceived += TCPSocketServerAp_DataReceived;
                            _TCPSocketAp.DataSentSuccess += TCPSocketServerAp_DataSentSuccess;
                            _TCPSocketAp.DataSentFailed += TCPSocketServerAp_DataSentFailed;
                            _TCPSocketAp.ListenClientSuccess += TCPSocketServerAp_ListenClientSuccess;
                            _TCPSocketAp.RemoteDisconnected += TCPSocketServerAp_RemoteDisconnected;
                            _TCPSocketAp.AcceptOpen += TCPSocketServerAp_AcceptOpen;
                            Dic_TCPSocketAp.TryAdd(em.EQID, _TCPSocketAp);
                        }
                        else
                        {
                            socket.SocketBasic _TCPSocketAp = new socket.TCPSocketClient(socketParaLibrary.RemoteIP, Convert.ToUInt16(socketParaLibrary.RemotePort),
                                socketParaLibrary.LocalIP, Convert.ToUInt16(socketParaLibrary.LocalPort), socket.SocketCommonData.ENCODEMODE.ENCODEMODE_ASCII);
                            _TCPSocketAp.DataReceived += TCPSocketClientAp_DataReceived;
                            _TCPSocketAp.DataSentSuccess += TCPSocketClientAp_DataSentSuccess;
                            _TCPSocketAp.DataSentFailed += TCPSocketClientAp_DataSentFailed; ;
                            _TCPSocketAp.RemoteDisconnected += TCPSocketClientAp_RemoteDisconnected;
                            _TCPSocketAp.ConnectedSuccess += TCPSocketClientAp_ConnectedSuccess;
                            _TCPSocketAp.ConnectedFailed += TCPSocketClientAp_ConnectedFailed;
                            _TCPSocketAp.ConnectRunning += TCPSocketClientAp_ConnectRunning;

                            Dic_TCPSocketAp.TryAdd(em.EQID, _TCPSocketAp);
                        }
                    }
                }
            }

            #endregion 

        }
        /// <summary>
        /// MQ Service Start
        /// </summary>
        public static void MQServiceStart()
        {
            #region [MQ Interface Initial]
            if (commonLibrary.baseLib.rbmqParaLibrary == null) return;
            if (commonLibrary.baseLib.rbmqParaLibrary.Enable)
            {
                MQPublisherAp = new RBMQService(commonLibrary.baseLib.rbmqParaLibrary.ServerUrlString, commonLibrary.baseLib.rbmqParaLibrary.ExchangeName, commonLibrary.LineName);
                MQPublisherAp.Close();
            }
            #endregion
        }

        /// <summary>
        /// WebAPI Service Start
        /// </summary>
        public static void WebAPIServerStart()
        {
            try
            {
                if (commonLibrary.baseLib.apiServerParaLibrary == null) return;
                if (commonLibrary.baseLib.apiServerParaLibrary.Server_Enable)
                {
                    string errMsg;
                    WebAPIServerAp = new WebAPIServer(commonLibrary.baseLib.apiServerParaLibrary.LocalPort, commonLibrary.baseLib.apiServerParaLibrary.LocalIP, out errMsg);

                }
            }
            catch (Exception exception)
            {
                MessageBox.Show($"{exception.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public static void SerialPortServiceStart()
        {
            try
            {
                if (commonLibrary.configLibrary.Enable_Set)
                {
                    SerialPortServiceAp = new SerialPortService(commonLibrary.configLibrary.PortName, commonLibrary.configLibrary.BaudRate,
                    commonLibrary.configLibrary.DataBits, commonLibrary.configLibrary.StopBits, commonLibrary.configLibrary.Parity);

                }

            }
            catch (Exception ex)
            {

            }
        }


        /// <summary>
        /// WebAPI Service Restart
        /// </summary>
        public static async void ReStartWebAPIServer()
        {
            try
            {
                await WebAPIServerAp.CloseAPIServer();
                WebAPIServerAp.Dispose();

                WebAPIServerStart();
                await WebAPIServerAp.StartAPIServer();
            }
            catch (Exception exception)
            {
                MessageBox.Show($"{exception.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// Check MQ Connected
        /// </summary>
        /// <param name="IP"></param>
        /// <returns></returns>
        public static bool CheckMQConnected(string IP)
        {
            try
            {
                Ping MQServerPing = new Ping();
                PingOptions MQServerPingOptions = new PingOptions();
                MQServerPingOptions.DontFragment = true;
                string data = "";
                byte[] buffer = Encoding.UTF8.GetBytes(data);
                int intTimeout = 120;
                PingReply MQServerPingReply = MQServerPing.Send(IP, intTimeout, buffer, MQServerPingOptions);
                string strInfo = MQServerPingReply.Status.ToString();
                if (strInfo == "Success")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
                throw;
            }
        }
        /// <summary>
        /// 终止程式
        /// </summary>
        public static Action ExitSystem = () =>
        {
            try
            {
                if (EAPAp != null)
                {

                    //中止所有线程执行
                    EAPAp.UnRegAllWorkFromPool();
                    //中止内部通讯
                    EAPAp.TerminalTraffic();
                }
            }
            catch (Exception ex)
            {
                Environment.BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
            }
        };
    }

    public partial class EAPEnvironment
    {
        #region Socket Server Event
        private static void TCPSocketServerAp_AcceptOpen(object sender, socket.ServerEventArgs e)
        {
            BaseComm.LogMsg(Log.LogLevel.Info, string.Format("<{0}> <{1}> Socket通道开启.", commonLibrary.LineName, e.GetHostInfo.ToString()));
        }

        private static void TCPSocketServerAp_DataSentFailed(object sender, socket.SendDataEventArgs e)
        {

        }

        private static void TCPSocketServerAp_DataSentSuccess(object sender, socket.SendDataEventArgs e)
        {

        }

        private static void TCPSocketServerAp_RemoteDisconnected(object sender, socket.ServerEventArgs e)
        {
            try
            {
                Thread.Sleep(200);
                BaseComm.LogMsg(Log.LogLevel.Warn, string.Format("<{0}> <{1}> Socket通道断线.", commonLibrary.LineName, e.GetHostInfo.ToString()));
                EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByIP(e.GetHostIP.ToString(), e.GetHostPort);
                if (em != null)
                {
                    em.ConnectMode = Model.eConnectMode.DISCONNECT;
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show($"{exception.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void TCPSocketServerAp_ListenClientSuccess(object sender, socket.ServerEventArgs e)
        {
            try
            {
                BaseComm.LogMsg(Log.LogLevel.Info, string.Format("<{0}> <{1}> Socket通道连线.", commonLibrary.LineName, e.GetHostInfo.ToString()));
                EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByIP(e.GetHostIP.ToString(), e.GetHostPort);
                if (em != null)
                {
                    em.ConnectMode = Model.eConnectMode.CONNECT;
                }

            }
            catch (Exception exception)
            {
                MessageBox.Show($"{exception.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private static void TCPSocketServerAp_DataReceived(object sender, socket.ReceiveDataEventArgs e)
        {
            try
            {
                EquipmentModel em = commonLibrary.equipmentLibary.GetEquipmentModelByIP(e.GetHostIP.ToString());
                if (em == null) return;
                //Thread.Sleep(300);
                lock (em.SocketMsgBuffer)
                {
                    em.SocketMsgBuffer.AddRange(e.GetByteData);
                }
                //em.qSocketMsgBuffer.Enqueue(e.GetByteData.ToList());

            }
            catch (Exception exception)
            {
                MessageBox.Show($"{exception.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Client
        private static void TCPSocketClientAp_ConnectedFailed(object sender, socket.ServerEventArgs e)
        {
            try
            {

                EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByIP(e.GetHostIP.ToString(), e.GetHostPort);
                BaseComm.LogMsg(Log.LogLevel.Warn, string.Format("<{0}><{1}> <{2}> Socket通道连接失败.", commonLibrary.LineName, em.EQName, e.GetHostInfo.ToString()));
                if (!Environment.EAPEnvironment.commonLibrary.isRunThread)
                {
                    Environment.EAPEnvironment.commonLibrary.isRunThread = true;
                    if (EAPEnvironment.EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_SocketReConnect, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_SocketReConnect, 4000, true))
                    {
                        EAPEnvironment.commonLibrary.commonModel.InSocketReconnectCheckTime = DateTime.Now;
                        BaseComm.LogMsg(Log.LogLevel.Info, string.Format("<{0}> Socket通道断线重连中...", commonLibrary.LineName));
                        em.ConnectMode = Model.eConnectMode.DISCONNECT;
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show($"{exception.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private static void TCPSocketClientAp_ConnectRunning(object sender, EventArgs e)
        {

        }

        private static void TCPSocketClientAp_RemoteDisconnected(object sender, socket.ServerEventArgs e)
        {
            try
            {
                EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByIP(e.GetHostIP.ToString(), e.GetHostPort);
                BaseComm.LogMsg(Log.LogLevel.Warn, string.Format("<{0}><{1}> <{2}> Socket通道断线.", commonLibrary.LineName, em.EQName, e.GetHostInfo.ToString()));
                if (em != null)
                {
                    em.ConnectMode = Model.eConnectMode.DISCONNECT;
                    if (!Environment.EAPEnvironment.commonLibrary.isRunThread)
                    {
                        Environment.EAPEnvironment.commonLibrary.isRunThread = true;
                        if (EAPEnvironment.EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_SocketReConnect, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_SocketReConnect, 4000, true))
                        {
                            BaseComm.LogMsg(Log.LogLevel.Info, string.Format("<{0}> <{1}> Socket通道断线重连中...", commonLibrary.LineName, e.GetHostInfo.ToString()));
                            em.SetConnectCount = 0;
                        }
                    }

                }
            }
            catch (Exception exception)
            {
                MessageBox.Show($"{exception.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void TCPSocketClientAp_DataSentSuccess(object sender, socket.SendDataEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
            }
        }
        private static void TCPSocketClientAp_DataSentFailed(object sender, socket.SendDataEventArgs e)
        {

        }
        private static void TCPSocketClientAp_DataReceived(object sender, socket.ReceiveDataEventArgs e)
        {
            try
            {
                EquipmentModel em = commonLibrary.equipmentLibary.GetEquipmentModelByIP(e.GetHostIP.ToString());
                if (em == null) return;
                lock (em.SocketMsgBuffer)
                {
                    em.SocketMsgBuffer.AddRange(e.GetByteData);
                }
                //em.qSocketMsgBuffer.Enqueue(e.GetByteData.ToList());
            }
            catch (Exception exception)
            {

            }
        }

        private static void TCPSocketClientAp_ConnectedSuccess(object sender, socket.ServerEventArgs e)
        {
            try
            {
                EquipmentModel em = commonLibrary.equipmentLibary.GetEquipmentModelByIP(e.GetHostIP.ToString(), e.GetHostPort);
                if (em != null)
                {
                    em.ConnectMode = Model.eConnectMode.CONNECT;
                    em.SetConnectCount = 0;
                }
                BaseComm.LogMsg(Log.LogLevel.Info, string.Format("<{0}><{1}> <{2}> Socket通道连线.", commonLibrary.LineName, em.EQName, e.GetHostInfo.ToString()));
                var ems = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.ConnectMode == iJedha.Automation.EAP.Model.eConnectMode.DISCONNECT).FirstOrDefault();
                if (ems == null)
                {
                    EAPEnvironment.EAPAp.DeleteThreadWork(ConstLibrary.CONST_DLL_RULE_SocketReConnect);
                    Environment.EAPEnvironment.commonLibrary.isRunThread = false;
                    EAPEnvironment.commonLibrary.reconnectSec = 4;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion




    }

}
