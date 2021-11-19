using iJedha.Automation.EAP.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace iJedha.Automation.EAP.Library
{
    public class DynamicLibrary
    {
        public string EQID { get; set; }
        protected XmlDocument _xmlDoc;
        public ConcurrentDictionary<string, VariableModel> Dic_VariableModel { get; set; }
        public ConcurrentDictionary<string, ReportModel> Dic_ReportLink { get; set; }
        public ConcurrentDictionary<string, EventModel> Dic_EventLink { get; set; }
        public ConcurrentDictionary<string, TraceDataModel> Dic_TraceDataModel { get; set; }
        public ConcurrentDictionary<string, PPParameterModel> Dic_PPParameterModel { get; set; }
        public ConcurrentDictionary<string, CPNameModel> Dic_CPNameModel { get; set; }
        public ConcurrentDictionary<string, RCMDModel> Dic_RCMDLink { get; set; }
        public ConcurrentDictionary<string, AlarmModel> Dic_AlarmModel { get; set; }
        public DynamicLibrary()
        {
            Dic_VariableModel = new ConcurrentDictionary<string, VariableModel>();
            Dic_ReportLink = new ConcurrentDictionary<string, ReportModel>();
            Dic_EventLink = new ConcurrentDictionary<string, EventModel>();
            Dic_TraceDataModel = new ConcurrentDictionary<string, TraceDataModel>();
            Dic_PPParameterModel = new ConcurrentDictionary<string, PPParameterModel>();
            Dic_CPNameModel = new ConcurrentDictionary<string, CPNameModel>();
            Dic_RCMDLink = new ConcurrentDictionary<string, RCMDModel>();
            Dic_AlarmModel = new ConcurrentDictionary<string, AlarmModel>();
        }
        public bool Load(XmlNode node,EquipmentModel em)
        {
            try
            {
                if (em == null || node == null) return false;
                EQID = em.EQID;
                foreach (XmlNode xn in node.ChildNodes)
                {
                    if (xn.NodeType == XmlNodeType.Element)
                    {
                        switch (xn.Name.Trim().ToUpper())
                        {
                            case "KEY":
                                break;
                            case "VID":
                                InitialVID(xn);
                                break;
                            case "RPID":
                                InitialReportID(xn);
                                break;
                            case "CEID":
                                InitialCEID(xn);
                                break;
                            case "CPNAME":
                                InitialCPName(xn);
                                break;
                            case "RCMD":
                                InitialRCMD(xn);
                                break;
                            case "TRACE":
                                InitialTrace(xn);
                                break;
                            case "PPBODY":
                                InitialPPBody(xn);
                                break;
                            case "ALARM":
                                InitialAlarm(xn);
                                break;
                        }
                    }
                }
                return true;

            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public void InitialVID(XmlNode xmlNode)
        {
            try
            {
                Dic_VariableModel.Clear();
                foreach (XmlNode xn in xmlNode.ChildNodes)
                {
                    VariableModel md = new VariableModel();
                    md.ID = xn.Attributes["id"].Value;
                    md.Name = xn.Attributes["name"].Value;
                    md.DefaultUnit = xn.Attributes["defaultunit"].Value;
                    md.DefaultValue = xn.Attributes["defaultvalue"].Value;
                    md.MaxValue = xn.Attributes["maxvalue"].Value;
                    md.MinValue = xn.Attributes["minvalue"].Value;
                    md.isEnable = xn.Attributes["enable"].Value == "O" ? true : false;
                    if (md.isEnable)
                    {
                        if (!Dic_VariableModel.ContainsKey(md.ID))
                            Dic_VariableModel.TryAdd(md.ID, md);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        public VariableModel GetVariableID(string id)
        {
            try
            {
                if (Dic_VariableModel.ContainsKey(id))
                    return Dic_VariableModel[id];

                return null;

            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public IList<VariableModel> GetAllVariableModel()
        {
            try
            {
                if (Dic_VariableModel == null) return null;
                return (from o in Dic_VariableModel.Values where o.isEnable == true select o).OrderBy(a => a.ID).ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public void InitialReportID(XmlNode xmlNode)
        {
            try
            {
                Dic_ReportLink.Clear();
                foreach (XmlNode xn in xmlNode.ChildNodes)
                {
                    ReportModel md = new ReportModel();
                    md.ID = xn.Attributes["id"].Value;
                    md.Name = xn.Attributes["name"].Value;
                    md.StringLink  = xn.Attributes["link"].Value;
                    md.isEnable = xn.Attributes["enable"].Value == "O" ? true : false;

                    if (md.isEnable)
                    {
                        if (!Dic_ReportLink.ContainsKey(md.ID))
                            Dic_ReportLink.TryAdd(md.ID, md);
                    }
                }

            }
            catch (Exception ex)
            {

            }
        }
        public ReportModel GetReportID(string id)
        {
            try
            {
                if (Dic_ReportLink.ContainsKey(id))
                    return Dic_ReportLink[id];

                return null;

            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public IList<ReportModel> GetAllReportLink()
        {
            try
            {
                return (from o in Dic_ReportLink.Values where o.isEnable == true select o).OrderBy(a => a.ID).ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public void InitialCEID(XmlNode xmlNode)
        {
            try
            {
                Dic_EventLink.Clear();
                foreach (XmlNode xn in xmlNode.ChildNodes)
                {
                    EventModel md = new EventModel();
                    md.ID = xn.Attributes["id"].Value;
                    md.Name = xn.Attributes["name"].Value;
                    md.StringLink = xn.Attributes["link"].Value;
                    md.isEnable = xn.Attributes["enable"].Value == "O" ? true : false;

                    if (md.isEnable)
                    {
                        if (!Dic_EventLink.ContainsKey(md.ID))
                            Dic_EventLink.TryAdd(md.ID, md);
                    }
                }

            }
            catch (Exception ex)
            {

            }
        }
        public EventModel GetCEID(string id)
        {
            try
            {
                if (Dic_EventLink.ContainsKey(id))
                    return Dic_EventLink[id];

                return null;

            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public IList<EventModel> GetAllCEIDLink()
        {
            try
            {
                if (Dic_EventLink == null) return null;
                return (from o in Dic_EventLink.Values where o.isEnable == true select o).OrderBy(a => a.ID).ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public void InitialCPName(XmlNode xmlNode)
        {
            try
            {
                Dic_CPNameModel.Clear();
                foreach (XmlNode xn in xmlNode.ChildNodes)
                {
                    CPNameModel md = new CPNameModel();
                    md.ID = xn.Attributes["id"].Value;
                    md.Name = xn.Attributes["name"].Value;
                    md.DefaultUnit = xn.Attributes["defaultunit"].Value;
                    md.DefaultValue = xn.Attributes["defaultvalue"].Value;

                    if (!Dic_CPNameModel.ContainsKey(md.ID))
                        Dic_CPNameModel.TryAdd(md.ID, md);
                }

            }
            catch (Exception ex)
            {

            }
        }
        public CPNameModel GetCPName(string id)
        {
            try
            {
                if (Dic_CPNameModel.ContainsKey(id))
                    return Dic_CPNameModel[id];
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public IList<CPNameModel> GetAllCPName()
        {
            try
            {
                return (from o in Dic_CPNameModel.Values select o).OrderBy(a => a.ID).ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public void InitialRCMD(XmlNode xmlNode)
        {
            try
            {
                Dic_RCMDLink.Clear();
                foreach (XmlNode xn in xmlNode.ChildNodes)
                {
                    RCMDModel md = new RCMDModel();
                    md.ID = xn.Attributes["id"].Value;
                    md.Name = xn.Attributes["name"].Value;
                    md.StringLink = xn.Attributes["link"].Value;
                    md.isEnable = xn.Attributes["enable"].Value == "O" ? true : false;

                    if (md.isEnable)
                    {
                        if (!Dic_RCMDLink.ContainsKey(md.ID))
                            Dic_RCMDLink.TryAdd(md.ID, md);
                    }
                }

            }
            catch (Exception ex)
            {

            }
        }
        public RCMDModel GetRCMDModel(string name)
        {
            try
            {
                return Dic_RCMDLink.Values.FirstOrDefault(a => a.Name == name);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public IList<RCMDModel> GetAllRCMDModel()
        {
            try
            {
                if (Dic_RCMDLink == null) return null;
                return (from o in Dic_RCMDLink.Values where o.isEnable == true select o).OrderBy(a => a.ID).ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public void InitialTrace(XmlNode xmlNode)
        {
            try
            {
                Dic_TraceDataModel.Clear();
                foreach (XmlNode xn in xmlNode.ChildNodes)
                {
                    TraceDataModel md = new TraceDataModel();
                    md.ID = xn.Attributes["id"].Value;
                    md.Name = xn.Attributes["name"].Value;
                    md.DefaultUnit = xn.Attributes["defaultunit"].Value;
                    md.Sequence = int.Parse(xn.Attributes["sequence"].Value);
                    md.TraceID = 0;
                    md.DefaultValue = xn.Attributes["defaultvalue"].Value;
                    md.MaxValue = xn.Attributes["maxvalue"].Value;
                    md.MinValue = xn.Attributes["minvalue"].Value;
                    md.isEnable = xn.Attributes["enable"].Value == "O" ? true : false;

                    if (md.isEnable)
                    {
                        if (!Dic_TraceDataModel.ContainsKey(md.ID))
                            Dic_TraceDataModel.TryAdd(md.ID, md);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        /// <summary>
        /// 0:All 1:Key
        /// </summary>
        /// <param name="trid"></param>
        /// <returns></returns>
        public IList<TraceDataModel> GetAllTraceDataModel(int trid)
        {
            try
            {
                if (Dic_TraceDataModel == null) return null;
                if (trid == 0)
                {
                    return (from o in Dic_TraceDataModel.Values where o.isEnable == true select o).OrderBy(a => a.Sequence).ToList();
                }
                else
                {
                    return (from o in Dic_TraceDataModel.Values where o.isEnable == true && o.TraceID == trid select o).OrderBy(a => a.Sequence).ToList();
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public void InitialPPBody(XmlNode xmlNode)
        {
            try
            {
                Dic_PPParameterModel.Clear();
                foreach (XmlNode xn in xmlNode.ChildNodes)
                {
                    PPParameterModel md = new PPParameterModel();
                    md.ID = xn.Attributes["id"].Value;
                    md.Name = xn.Attributes["name"].Value;
                    md.DefaultUnit = xn.Attributes["defaultunit"].Value;
                    md.Sequence = int.Parse(xn.Attributes["sequence"].Value);
                    md.DefaultValue = xn.Attributes["defaultvalue"].Value;
                    md.MaxValue = xn.Attributes["maxvalue"].Value;
                    md.MinValue = xn.Attributes["minvalue"].Value;
                    md.isEnable = xn.Attributes["enable"].Value == "O" ? true : false;
                    if (md.isEnable)
                    {
                        if (!Dic_PPParameterModel.ContainsKey(md.ID))
                            Dic_PPParameterModel.TryAdd(md.ID, md);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        public IList<PPParameterModel> GetAllPPParameterModel()
        {
            try
            {
                if (Dic_PPParameterModel == null) return null;
                return (from o in Dic_PPParameterModel.Values where o.isEnable == true select o).OrderBy(a => a.Sequence).ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public void InitialAlarm(XmlNode xmlNode)
        {
            try
            {
                Dic_AlarmModel.Clear();
                foreach (XmlNode xn in xmlNode.ChildNodes)
                {
                    AlarmModel md = new AlarmModel();
                    md.ID = xn.Attributes["id"].Value;
                    md.Sequence = int.Parse(xn.Attributes["sequence"].Value);
                    md.AlarmText = xn.Attributes["alarmtext"].Value;
                    md.AlarmType = xn.Attributes["alarmtype"].Value;
                    md.isEnable = xn.Attributes["enable"].Value == "O" ? true : false;
                    if (md.isEnable)
                    {
                        if (!Dic_AlarmModel.ContainsKey(md.ID))
                            Dic_AlarmModel.TryAdd(md.ID, md);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        public IList<AlarmModel> GetAllAlarmModel()
        {
            try
            {
                if (Dic_AlarmModel == null) return null;
                return (from o in Dic_AlarmModel.Values where o.isEnable == true select o).OrderBy(a => a.Sequence).ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public AlarmModel GetAlarm(string id)
        {
            try
            {
                if (Dic_AlarmModel.ContainsKey(id))
                    return Dic_AlarmModel[id];
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        /// <summary>
        /// 新增Event模型
        /// </summary>
        /// <param name="eqp"></param>
        /// <returns></returns>
        public bool AddEventModel(string eqpID, EventModel em)
        {
            try
            {
                if (!Dic_EventLink.ContainsKey(eqpID))
                {
                    Dic_EventLink.TryAdd(eqpID, em);
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
