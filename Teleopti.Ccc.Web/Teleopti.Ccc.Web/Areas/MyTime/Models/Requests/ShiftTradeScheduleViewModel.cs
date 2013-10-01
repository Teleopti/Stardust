using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class ShiftTradeScheduleViewModel
	{
		public ShiftTradePersonScheduleViewModel MySchedule { get; set; }

		public IEnumerable<ShiftTradePersonScheduleViewModel> PossibleTradeSchedules { get; set; }

		public IEnumerable<ShiftTradeTimeLineHoursViewModel> TimeLineHours { get; set; }

		public int TimeLineLengthInMinutes { get; set; }
	}
}