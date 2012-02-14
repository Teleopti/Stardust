using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class NewDynamicDayOffSchedulePeriodTargetCalculator : ISchedulePeriodTargetCalculator
	{
		private readonly IScheduleMatrixPro _matrix;

		public NewDynamicDayOffSchedulePeriodTargetCalculator(IScheduleMatrixPro matrix)
		{
			_matrix = matrix;
		}

		public TimeSpan PeriodTarget(bool seasonality)
		{
			int workDays = 0;
			foreach (var scheduleDayPro in _matrix.EffectivePeriodDays)
			{
				SchedulePartView significant = scheduleDayPro.DaySchedulePart().SignificantPart();
				if (significant != SchedulePartView.DayOff && significant != SchedulePartView.ContractDayOff)
					workDays++;
			}

			IVirtualSchedulePeriod schedulePeriod = _matrix.SchedulePeriod;
			double minutes = schedulePeriod.AverageWorkTimePerDay.TotalMinutes * workDays;

		    double seasonalityValue = 0d;
            if(seasonality)
                seasonalityValue = minutes*schedulePeriod.Seasonality.Value;
            TimeSpan balance = (schedulePeriod.Extra).Add(schedulePeriod.BalanceOut).Subtract(schedulePeriod.BalanceIn);

			return TimeSpan.FromMinutes(minutes + seasonalityValue).Add(balance);
		}
	}
}