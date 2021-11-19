using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace iJedha.FIleMessageStructure
{
	[Serializable]
	public class AlarmReport
	{
		public string eqp_id { get; set; }
		public string alarm_type { get; set; }
		public string alarm_level { get; set; }
		public string alarm_text { get; set; }
		public string DateTime { get; set; }

		public AlarmReport()
		{
			eqp_id = string.Empty;
			alarm_type = string.Empty;
			alarm_level = string.Empty;
			alarm_text = string.Empty;
			DateTime = string.Empty;
		}
	}
}
