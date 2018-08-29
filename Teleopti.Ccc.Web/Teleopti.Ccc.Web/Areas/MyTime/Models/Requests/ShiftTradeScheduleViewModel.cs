using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
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

	public class ContractTimeInfoViewModel
	{
		public DateOnly PeriodStart { get; set; }
		public DateOnly PeriodEnd { get; set; }
		public int ContractTimeMinutes { get; set; }
		public int ToleranceLackMinutes { get; set; }
		public int ToleranceOverMinutes { get; set; }
	}

	public class ShiftTradeToleranceInfoViewModel
	{
		public bool IsNeedToCheck { get; set; }
		private IList<ContractTimeInfoViewModel> MyInfos { get; set; }
		private IList<ContractTimeInfoViewModel> PersonToInfos { get; set; }
		public int MyToleranceMinutes { get; set; }
		public int PersonToToleranceMInutes { get; set; }
	}

	public class ShiftTradeMultiScheduleViewModel
	{
		public DateTime Date { get; set; }
		public ShiftTradeAddPersonScheduleViewModel MySchedule { get; set; }
		public ShiftTradeAddPersonScheduleViewModel PersonToSchedule { get; set; }
		public bool IsSelectable { get; set; }
		public string UnselectableReason { get; set; }
	}
}