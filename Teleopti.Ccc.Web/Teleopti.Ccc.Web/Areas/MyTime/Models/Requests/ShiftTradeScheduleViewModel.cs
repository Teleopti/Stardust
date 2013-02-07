using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class ShiftTradeScheduleViewModel
	{
		public ShiftTradePersonScheduleViewModel MySchedule { get; set; }

		public IEnumerable<ShiftTradePersonScheduleViewModel> PossibleTradePersons { get; set; }

		public IEnumerable<ShiftTradeTimeLineHoursViewModel> TimeLineHours { get; set; }

		public int TimeLineLengthInMinutes { get; set; }
	}

	public class ShiftTradeSwapDetailsViewModel
	{
		public ShiftTradePersonScheduleViewModel From { get; set; }

		public ShiftTradePersonScheduleViewModel To { get; set; }

		public DateOnly DateFrom { get; set; }

		public DateOnly DateTo { get; set; }
	}
}