using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace iJedha.SocketMessageStructure
{
	[Serializable]
	[XmlRoot("message")] //Version V1.0 - 2020/10/01
	public class TraceDataRequestReply : Message
	{

		[Serializable]
		public class ctrace_data
		{
            public string data_item { get; set; }
            
            public string data_value { get; set; }

			public ctrace_data()
			{
				data_item = string.Empty;
				data_value = string.Empty;
			}
		}


		[Serializable]
		public class TrxBody : Body
		{
            public string eqp_id { get; set; }
            
            public string sub_eqp_id { get; set; }

			[XmlArray("trace_data_list")]
			[XmlArrayItem("trace_data")]
            
            public List<ctrace_data> trace_data_list { get; set; }

			public TrxBody()
			{
				eqp_id = string.Empty;
				sub_eqp_id = string.Empty;
				trace_data_list = new List<ctrace_data>();
			}
		}

		[XmlElement("header")]

		public new Header HEADER { get { return _hearer; } set { _hearer = value; } }

		[XmlElement("body")]
		public TrxBody BODY { get; set; }

        [XmlElement("return")]
        public new Return RETURN { get { return _return; } set { _return = value; } }

        public TraceDataRequestReply()
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
