using System;
using System.Xml.Serialization;

namespace iJedha.SocketMessageStructure
{
    [Serializable]
    [XmlRoot("message")] //Version V1.0 - 2020/10/01
    public class AbnormalPanelReport : Message
    {
        [Serializable]
        public class TrxBody : Body
        {
            public string eqp_id { get; set; }
            public string panel_id { get; set; }
            public string panel_count { get; set; }
            public string abnormal_code { get; set; }
            public TrxBody()
            {
                eqp_id = string.Empty;
                panel_id = string.Empty;
                panel_count = string.Empty;
                abnormal_code = string.Empty;
            }
        }

        [XmlElement("header")]
        public new Header HEADER { get { return _hearer; } set { _hearer = value; } }

        [XmlElement("body")]
        public TrxBody BODY { get; set; }

        [XmlElement("return")]
        public new Return RETURN { get { return _return; } set { _return = value; } }

        public AbnormalPanelReport()
        {
            this.HEADER.MESSAGENAME = GetType().Name;
            this.BODY = new TrxBody();
        }

        public override Body GetBody()
        {
            return this.BODY;
        }
    }
}
