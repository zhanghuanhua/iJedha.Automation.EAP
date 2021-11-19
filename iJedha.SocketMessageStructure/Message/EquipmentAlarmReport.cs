using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace iJedha.SocketMessageStructure
{
	[Serializable]
	[XmlRoot("message")] //Version V1.0 - 2020/10/01
	public class EquipmentAlarmReport : Message
	{

		[Serializable]
		public class calarm
		{

			public string alarm_code { get; set; }

			public calarm()
			{
				alarm_code = string.Empty;
			}
		}


		[Serializable]
		public class TrxBody : Body
		{

			public string eqp_id { get; set; }

			public string report_type { get; set; }

			public string alarm_type { get; set; }

			[XmlArray("alarm_list")]
			[XmlArrayItem("alarm")]
			public List<calarm> alarm_list { get; set; }

			public TrxBody()
			{
				eqp_id = string.Empty;
				report_type = string.Empty;
				alarm_type = string.Empty;
				alarm_list = new List<calarm>();
			}
		}

        [XmlElement("header")]
        public new Header HEADER { get { return _hearer; } set { _hearer = value; } }

        [XmlElement("body")]
        public TrxBody BODY { get; set; }

        [XmlElement("return")]
        public new Return RETURN { get { return _return; } set { _return = value; } }

        public EquipmentAlarmReport()
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
