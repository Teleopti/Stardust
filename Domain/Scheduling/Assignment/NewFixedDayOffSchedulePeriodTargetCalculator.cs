using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class NewFixedDayOffSchedulePeriodTargetCalculator : ISchedulePeriodTargetCalculator
	{
		private readonly IScheduleMatrixPro _matrix;

		public NewFixedDayOffSchedulePeriodTargetCalculator(IScheduleMatrixPro matrix)
		{
			_matrix = matrix;
		}

		public TimeSpan PeriodTarget(bool seasonality)
		{
			IVirtualSchedulePeriod schedulePeriod = _matrix.SchedulePeriod;
		    TimeSpan periodTarget = schedulePeriod.PeriodTarget();
		    TimeSpan seasonability = TimeSpan.Zero;
            if(seasonality)
		        seasonability =  TimeSpan.FromSeconds(periodTarget.TotalSeconds*schedulePeriod.Seasonality.Value);
			TimeSpan extraAndBalance = schedulePeriod.Extra.Add(schedulePeriod.BalanceOut).Subtract(schedulePeriod.BalanceIn);
            return periodTarget.Add(seasonability).Add(extraAndBalance);
		}
	}
}