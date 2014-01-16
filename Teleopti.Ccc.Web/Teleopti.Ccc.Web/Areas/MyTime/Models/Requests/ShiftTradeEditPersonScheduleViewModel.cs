using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class ShiftTradeEditPersonScheduleViewModel
	{
		public IEnumerable<ShiftTradeEditScheduleLayerViewModel> ScheduleLayers { get; set; }
		public string Name { get; set; }
		public int MinutesSinceTimeLineStart { get; set; }
		public DateTime StartTimeUtc { get; set; }
		public string DayOffText { get; set; }
		public bool HasUnderlyingDayOff { get; set; }
	}
}