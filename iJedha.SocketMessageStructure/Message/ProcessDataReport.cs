using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace iJedha.SocketMessageStructure
{
	[Serializable]
	[XmlRoot("message")] //Version V1.0 - 2020/10/01
	public class ProcessDataReport : Message
	{

		[Serializable]
		public class cproc_data
		{

			public string data_item { get; set; }

			public string data_value { get; set; }

			public cproc_data()
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

			public string job_id { get; set; }


            [XmlArray("proc_data_list")]
			[XmlArrayItem("proc_data")]
			public List<cproc_data> proc_data_list { get; set; }

			public TrxBody()
			{
				eqp_id = string.Empty;
				sub_eqp_id = string.Empty;
				job_id = string.Empty;
				proc_data_list = new List<cproc_data>();
			}
		}

        [XmlElement("header")]
        public new Header HEADER { get { return _hearer; } set { _hearer = value; } }

        [XmlElement("body")]
        public TrxBody BODY { get; set; }

        [XmlElement("return")]
        public new Return RETURN { get { return _return; } set { _return = value; } }

        public ProcessDataReport()
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
