using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel.LegacyWrappers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Historical
{
	public class HistoricalData : IHistoricalData
	{
		private readonly IDailyStatisticsAggregator _dailyStatisticsAggregator;

		public HistoricalData(IDailyStatisticsAggregator dailyStatisticsAggregator)
		{
			_dailyStatisticsAggregator = dailyStatisticsAggregator;
		}

		public TaskOwnerPeriod Fetch(IWorkload workload, DateOnlyPeriod period)
		{
			var statistics = _dailyStatisticsAggregator.LoadDailyStatistics(workload, period);

			var dailyStatistics =
				period.DayCollection()
					.Select(day => statistics.FirstOrDefault(s => s.Date == day))
					.Where(foundDailyStatistics => foundDailyStatistics.Date > DateOnly.MinValue)
					.ToList();
			return new TaskOwnerPeriod(DateOnly.MinValue, dailyStatistics.Convert(workload), TaskOwnerPeriodType.Other);
		}
	}
}