using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Future
{
	public class ForecastVolumeApplier : IForecastVolumeApplier
	{
		public void Apply(IWorkload workload, ITaskOwnerPeriod historicalData, IEnumerable<ITaskOwner> futureWorkloadDays)
		{
			if (!historicalData.TaskOwnerDayCollection.Any())
				return;

			var totalVolume = new TotalVolume();
			var indexes = new IVolumeYear[]
			{
				new MonthOfYear(historicalData, new MonthOfYearCreator()),
				new WeekOfMonth(historicalData, new WeekOfMonthCreator()),
				new DayOfWeeks(historicalData, new DaysOfWeekCreator())
			};
			totalVolume.Create(historicalData, futureWorkloadDays, indexes, new IOutlier[] { }, 0, 0, false, workload);
		}
	}
}