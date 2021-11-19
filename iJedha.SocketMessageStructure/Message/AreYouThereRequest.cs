using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace iJedha.SocketMessageStructure
{
    [Serializable]
    [XmlRoot("message")] //Version V1.0 - 2020/10/01
    public class AreYouThereRequest : Message
    {

        [Serializable]
        public class TrxBody : Body
        {

            public string eqp_id { get; set; }

            public string server_ip { get; set; }

            public TrxBody()
            {
                eqp_id = string.Empty;
                server_ip = string.Empty;
            }
        }

        [XmlElement("header")]
        public new Header HEADER { get { return _hearer; } set { _hearer = value; } }

        [XmlElement("body")]
        public TrxBody BODY { get; set; }

        [XmlElement("return")]
        public new Return RETURN { get { return _return; } set { _return = value; } }

        public AreYouThereRequest()
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
