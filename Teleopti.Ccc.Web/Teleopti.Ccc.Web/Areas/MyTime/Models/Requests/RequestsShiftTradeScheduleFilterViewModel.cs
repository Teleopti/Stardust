using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class RequestsShiftTradeScheduleFilterViewModel
	{
		public IEnumerable<string> DayOffShortNames { get; set; }
		public IEnumerable<string> HourTexts { get; set; }
	}
}
