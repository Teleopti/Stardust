using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class ShiftTradeRequestsScheduleViewModel
	{
		public ShiftTradeScheduleViewModel MySchedule { get; set; }

		public IEnumerable<ShiftTradeScheduleViewModel> PossibleTradePersons { get; set; }

		public IEnumerable<ShiftTradeTimeLineHoursViewModel> TimeLineHours { get; set; }

		public int TimeLineLengthInMinutes { get; set; }
	}
}