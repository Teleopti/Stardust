using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class ShiftTradeMyScheduleViewModel
	{
		public IEnumerable<ShiftTradeScheduleLayerViewModel> ScheduleLayers { get; set; }

		public int MinutesSinceTimeLineStart { get; set; }
	}
}