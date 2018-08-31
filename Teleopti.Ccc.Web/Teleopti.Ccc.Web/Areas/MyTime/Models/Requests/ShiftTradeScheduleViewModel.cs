﻿using System;
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
		public DateTime PeriodStart { get; set; }
		public DateTime PeriodEnd { get; set; }
		public int ContractTimeMinutes { get; set; }
		public int ToleranceBlanceInMinutes { get; set; }
		public int ToleranceBalanceOutMinutes { get; set; }
	}

	public class ShiftTradeToleranceInfoViewModel
	{
		public bool IsNeedToCheck { get; set; }
		public IList<ContractTimeInfoViewModel> MyInfos { get; set; }
		public IList<ContractTimeInfoViewModel> PersonToInfos { get; set; }
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