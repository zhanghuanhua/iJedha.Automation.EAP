using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace iJedha.SocketMessageStructure
{
	[Serializable]
 [XmlRoot("message")] //Version V1.0 - 2020/10/01
	public class RGVDispatchCommand : Message
	{

		[Serializable]
		public class TrxBody : Body
		{

			public string eqp_id { get; set; }
			public string job_id { get; set; }
            public string part_id { get; set; }
            public string to_id { get; set; }

			public TrxBody()
			{
				eqp_id = string.Empty;
				job_id = string.Empty;
                part_id = string.Empty;
                to_id = string.Empty;
			}
		}

       [XmlElement("header")]
		public new Header HEADER { get { return _hearer; } set { _hearer = value; } }

             [XmlElement("body")]
		public TrxBody BODY { get; set; }

             [XmlElement("return")]
		public new Return RETURN { get { return _return; } set { _return = value; } }

		public RGVDispatchCommand()
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
