using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Future
{
	public class ForecastVolumeApplier : IForecastVolumeApplier
	{
		public void Apply(IWorkload workload, ITaskOwnerPeriod taskOwnerPeriod, IEnumerable<ITaskOwner> futureWorkloadDays)
		{
			var totalVolume = new TotalVolume();
			VolumeYear volumeMonthYear = new MonthOfYear(taskOwnerPeriod, new MonthOfYearCreator());
			VolumeYear volumeWeekYear = new WeekOfMonth(taskOwnerPeriod, new WeekOfMonthCreator());
			VolumeYear volumeDayYear = new DayOfWeeks(taskOwnerPeriod, new DaysOfWeekCreator());
			var indexes = new List<IVolumeYear> { volumeMonthYear, volumeWeekYear, volumeDayYear };
			totalVolume.Create(taskOwnerPeriod, futureWorkloadDays, indexes, new IOutlier[] { }, 0, 0, false, workload);
		}
	}
}