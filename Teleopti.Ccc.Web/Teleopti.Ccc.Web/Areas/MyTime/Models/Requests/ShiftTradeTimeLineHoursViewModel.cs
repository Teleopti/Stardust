using System;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class ShiftTradeTimeLineHoursViewModel
	{
		public string HourText { get; set; }
		public int LengthInMinutesToDisplay { get; set; }

		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
	}
}