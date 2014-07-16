using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class ShiftTradeScheduleViewModel
	{
		public ShiftTradeScheduleViewModel ()
		{
			//so it is last if we don't get any
			IsLastPage = true;
		}
		public ShiftTradeAddPersonScheduleViewModel MySchedule { get; set; }

		public IEnumerable<ShiftTradeAddPersonScheduleViewModel> PossibleTradeSchedules { get; set; }

		public IEnumerable<ShiftTradeTimeLineHoursViewModel> TimeLineHours { get; set; }

		public int TimeLineLengthInMinutes { get; set; }

		public int PageCount { get; set; }
		public int PageIndex { get; set; }

		public bool IsLastPage { get; set; }
	}
}