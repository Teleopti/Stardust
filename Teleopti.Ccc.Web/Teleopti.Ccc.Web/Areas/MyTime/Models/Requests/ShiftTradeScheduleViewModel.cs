using System.Collections.Generic;

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
		public string PeriodStart { get; set; }
		public string PeriodEnd { get; set; }
		public double ContractTimeMinutes { get; set; }
		public double NegativeToleranceMinutes { get; set; }
		public double PositiveToleranceMinutes { get; set; }
		public double RealScheduleNegativeGap { get; set; }
		public double RealSchedulePositiveGap { get; set; }
	}

	public class ShiftTradeToleranceInfoViewModel
	{
		public bool IsNeedToCheck { get; set; }
		public IList<ContractTimeInfoViewModel> MyInfos { get; set; }
		public IList<ContractTimeInfoViewModel> PersonToInfos { get; set; }
	}

	public class ShiftTradeMultiScheduleViewModel
	{
		public string Date { get; set; }
		public ShiftTradeAddPersonScheduleViewModel MySchedule { get; set; }
		public ShiftTradeAddPersonScheduleViewModel PersonToSchedule { get; set; }
		public bool IsSelectable { get; set; }
		public string UnselectableReason { get; set; }
	}
}