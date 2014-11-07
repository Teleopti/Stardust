using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Future
{
	public class ForecastVolumeApplier : IForecastVolumeApplier
	{
		public void Apply(IWorkload workload, ITaskOwnerPeriod taskOwnerPeriod, IEnumerable<ITaskOwner> futureWorkloadDays)
		{
			if (!taskOwnerPeriod.TaskOwnerDayCollection.Any())
				return;

			var totalVolume = new TotalVolume();
			var indexes = new IVolumeYear[]
			{
				new MonthOfYear(taskOwnerPeriod, new MonthOfYearCreator()),
				new WeekOfMonth(taskOwnerPeriod, new WeekOfMonthCreator()),
				new DayOfWeeks(taskOwnerPeriod, new DaysOfWeekCreator())
			};
			totalVolume.Create(taskOwnerPeriod, futureWorkloadDays, indexes, new IOutlier[] { }, 0, 0, false, workload);
		}
	}
}