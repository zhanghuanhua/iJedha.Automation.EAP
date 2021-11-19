using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace iJedha.SocketMessageStructure
{
	[Serializable]
 [XmlRoot("message")] //Version V1.0 - 2020/10/01
	public class OperatorLoginConfirm : Message
	{

		[Serializable]
		public class TrxBody : Body
		{

			public string eqp_id { get; set; }

			public string operator_id { get; set; }

			public string result { get; set; }

			public TrxBody()
			{
				eqp_id = string.Empty;
				operator_id = string.Empty;
				result = string.Empty;
			}
		}

       [XmlElement("header")]
		public new Header HEADER { get { return _hearer; } set { _hearer = value; } }

             [XmlElement("body")]
		public TrxBody BODY { get; set; }

             [XmlElement("return")]
		public new Return RETURN { get { return _return; } set { _return = value; } }

		public OperatorLoginConfirm()
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
