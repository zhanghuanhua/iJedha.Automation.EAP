using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace iJedha.SocketMessageStructure
{
	[Serializable]
 [XmlRoot("message")] //Version V1.0 - 2020/10/01
	public class InitialDataReply : Message
	{

		[Serializable]
		public class TrxBody : Body
		{

			public string eqp_id { get; set; }

			public string control_mode { get; set; }

			public string operation_mode { get; set; }

			public string eqp_status { get; set; }

			public string recipe_name { get; set; }

			public string recipe_path { get; set; }

			public string cam_path { get; set; }

			public string job_id { get; set; }

			public string total_panel_count { get; set; }

			public string process_panel_count { get; set; }

			public TrxBody()
			{
				eqp_id = string.Empty;
				control_mode = string.Empty;
				operation_mode = string.Empty;
				eqp_status = string.Empty;
				recipe_name = string.Empty;
				recipe_path = string.Empty;
				cam_path = string.Empty;
				job_id = string.Empty;
				total_panel_count = string.Empty;
				process_panel_count = string.Empty;
			}
		}

       [XmlElement("header")]
		public new Header HEADER { get { return _hearer; } set { _hearer = value; } }

             [XmlElement("body")]
		public TrxBody BODY { get; set; }

             [XmlElement("return")]
		public new Return RETURN { get { return _return; } set { _return = value; } }

		public InitialDataReply()
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
