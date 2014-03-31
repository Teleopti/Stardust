using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class AbsenceAccountViewModel
	{
		public string AbsenceName { get; set; }
		public string TrackerType { get; set; }
		public DateTime PeriodStart { get; set; }
		public DateTime PeriodEnd { get; set; }
		public string Accrued { get; set; }
		public string Used { get; set; }
		public string Remaining { get; set; }
	}
}
