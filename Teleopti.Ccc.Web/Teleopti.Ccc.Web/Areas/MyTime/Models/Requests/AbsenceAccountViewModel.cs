using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class AbsenceAccountViewModel
	{
		public string AbsenceName { get; set; }
		public DateTime PeriodStartUtc { get; set; }
		public DateTime PeriodEndUtc { get; set; }
		public TimeSpan Accrued { get; set; }
		public TimeSpan Used { get; set; }
		public TimeSpan Remaining { get; set; }
	}
}
