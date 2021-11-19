using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace iJedha.SocketMessageStructure
{
	[Serializable]
 [XmlRoot("message")] //Version V1.0 - 2020/10/01
	public class EquipmentCurrentStatus : Message
	{

		[Serializable]
		public class TrxBody : Body
		{

			public string eqp_id { get; set; }

			public string sub_eqp_id { get; set; }

			public string eqp_status { get; set; }

			public string green_towner { get; set; }

			public string yellow_towner { get; set; }

			public string red_towner { get; set; }

			public TrxBody()
			{
				eqp_id = string.Empty;
				sub_eqp_id = string.Empty;
				eqp_status = string.Empty;
				green_towner = string.Empty;
				yellow_towner = string.Empty;
				red_towner = string.Empty;
			}
		}

       [XmlElement("header")]
		public new Header HEADER { get { return _hearer; } set { _hearer = value; } }

             [XmlElement("body")]
		public TrxBody BODY { get; set; }

             [XmlElement("return")]
		public new Return RETURN { get { return _return; } set { _return = value; } }

		public EquipmentCurrentStatus()
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
