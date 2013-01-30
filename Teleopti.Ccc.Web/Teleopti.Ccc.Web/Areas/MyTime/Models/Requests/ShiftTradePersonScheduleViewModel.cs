using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class ShiftTradePersonScheduleViewModel
	{
		public IEnumerable<ShiftTradeScheduleLayerViewModel> ScheduleLayers { get; set; }
		public string Name { get; set; }
		public int MinutesSinceTimeLineStart { get; set; }
	}
}