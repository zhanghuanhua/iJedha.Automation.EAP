using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace iJedha.SocketMessageStructure
{
	[Serializable]
	[XmlRoot("message")] //Version V1.0 - 2020/10/01
	public class JobDataDownload : Message
	{

		[Serializable]
		public class cpanel
		{

			public string panel_id { get; set; }

			public cpanel()
			{
				panel_id = string.Empty;
			}
		}


		[Serializable]
		public class crecipe_parameter
		{

			public string item_name { get; set; }

			public string item_value { get; set; }

			public crecipe_parameter()
			{
				item_name = string.Empty;
				item_value = string.Empty;
			}
		}


		[Serializable]
		public class TrxBody : Body
		{

			public string eqp_id { get; set; }

			public string port_id { get; set; }

			public string job_id { get; set; }

			public string total_panel_count { get; set; }

			public string panel_size { get; set; }

			public string recipe_id { get; set; }

			public string recipe_path { get; set; }

			public string cam_path { get; set; }

			public string part_no { get; set; }

			public string layer_count { get; set; }

			[XmlArray("panel_list")]
			[XmlArrayItem("panel")]
			public List<cpanel> panel_list { get; set; }

			[XmlArray("recipe_parameter_list")]
			[XmlArrayItem("recipe_parameter")]
			public List<crecipe_parameter> recipe_parameter_list { get; set; }

			public TrxBody()
			{
				eqp_id = string.Empty;
				port_id = string.Empty;
				job_id = string.Empty;
				total_panel_count = string.Empty;
				panel_size = string.Empty;
				recipe_id = string.Empty;
				recipe_path = string.Empty;
				cam_path = string.Empty;
				part_no = string.Empty;
				layer_count = string.Empty;
				panel_list = new List<cpanel>();
				recipe_parameter_list = new List<crecipe_parameter>();
			}
		}

		
        [XmlElement("header")]
        public new Header HEADER { get { return _hearer; } set { _hearer = value; } }

        [XmlElement("body")]
        public TrxBody BODY { get; set; }

        [XmlElement("return")]
        public new Return RETURN { get { return _return; } set { _return = value; } }

        public JobDataDownload()
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
