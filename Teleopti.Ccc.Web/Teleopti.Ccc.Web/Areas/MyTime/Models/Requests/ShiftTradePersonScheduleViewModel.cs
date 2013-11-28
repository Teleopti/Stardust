using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class ShiftTradePersonScheduleViewModel
	{
		public IEnumerable<ShiftTradeScheduleLayerViewModel> ScheduleLayers { get; set; }
		public string Name { get; set; }
		public int MinutesSinceTimeLineStart { get; set; }
		public DateTime StartTimeUtc { get; set; }
		public string DayOffText { get; set; }
		public bool HasUnderlyingDayOff { get; set; }
		public Guid PersonId { get; set; }
		public DateTime? MinStart { get; set; }
	}
}