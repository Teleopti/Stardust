using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class ShiftTradeRequestsScheduleViewModel
	{
		public ShiftTradeMyScheduleViewModel MySchedule { get; set; }

		public IEnumerable<ShiftTradePersonViewModel> PossibleTradePersons { get; set; }

		public IEnumerable<ShiftTradeTimeLineHoursViewModel> TimeLineHours { get; set; }

		public int TimeLineLengthInMinutes { get; set; }
	}
}