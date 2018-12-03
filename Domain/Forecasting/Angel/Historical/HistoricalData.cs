using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel.LegacyWrappers;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Historical
{
	public class HistoricalData : IHistoricalData
	{
		private readonly IDailyStatisticsProvider _dailyStatisticsProvider;

		public HistoricalData(IDailyStatisticsProvider dailyStatisticsProvider)
		{
			_dailyStatisticsProvider = dailyStatisticsProvider;
		}

		public TaskOwnerPeriod Fetch(IWorkload workload, DateOnlyPeriod period)
		{
			var statistics = _dailyStatisticsProvider.LoadDailyStatistics(workload, period);

			var dailyStatistics =
				period.DayCollection()
					.Select(day => statistics.FirstOrDefault(s => s.Date == day))
					.Where(foundDailyStatistics => foundDailyStatistics.Date > DateOnly.MinValue)
					.ToList();
			return new TaskOwnerPeriod(DateOnly.MinValue, dailyStatistics.Convert(workload), TaskOwnerPeriodType.Other);
		}
	}
}