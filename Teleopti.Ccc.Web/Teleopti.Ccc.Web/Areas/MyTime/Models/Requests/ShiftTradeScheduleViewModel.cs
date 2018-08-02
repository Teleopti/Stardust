using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class ShiftTradeScheduleViewModel
	{
		public ShiftTradeAddPersonScheduleViewModel MySchedule { get; set; }

		public IEnumerable<ShiftTradeAddPersonScheduleViewModel> PossibleTradeSchedules { get; set; }

		public IEnumerable<ShiftTradeTimeLineHoursViewModel> TimeLineHours { get; set; }

		public int PageCount { get; set; }
	}

	public class ShiftTradeMultiSchedulesViewModel
	{
		public IEnumerable<ShiftTradeMultiScheduleViewModel> MultiSchedulesForShiftTrade { get; set; }
	}

	public class ShiftTradeMultiScheduleViewModel
	{
		public DateTime Date { get; set; }
		public ShiftTradeAddPersonScheduleViewModel MySchedule { get; set; }
		public ShiftTradeAddPersonScheduleViewModel PersonToSchedule { get; set; }
	}
}