﻿using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common
{
	public class SchedulePeriodCloseCalculator
	{
		private readonly IScheduleContractTimeCalculator _scheduleContractTimeCalculator;
		private readonly IScheduleTargetTimeCalculator _scheduleTargetTimeCalculator;
		private readonly ISchedulePeriod _previousPeriod;
		private readonly ISchedulePeriod _period;

		public SchedulePeriodCloseCalculator(IScheduleContractTimeCalculator scheduleContractTimeCalculator,
		                                     IScheduleTargetTimeCalculator scheduleTargetTimeCalculator,
		                                     ISchedulePeriod previousPeriod, ISchedulePeriod period)
		{
			_scheduleContractTimeCalculator = scheduleContractTimeCalculator;
			_scheduleTargetTimeCalculator = scheduleTargetTimeCalculator;
			_previousPeriod = previousPeriod;
			_period = period;
		}

		public void CalculateBalanceOut()
		{
			_previousPeriod.BalanceOut = TimeSpan.Zero;
			_previousPeriod.BalanceOut = _scheduleContractTimeCalculator.CalculateContractTime() - _scheduleTargetTimeCalculator.CalculateTargetTime();
			_period.BalanceIn = _previousPeriod.BalanceOut;
		}
	}
}
