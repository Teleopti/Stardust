using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel.LegacyWrappers;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Historical
{
	public class HistoricalData : IHistoricalData
	{
		private readonly IDailyStatisticsAggregator _dailyStatisticsAggregator;
		private readonly IValidatedVolumeDayRepository _validatedVolumeDayRepository;

		public HistoricalData(IDailyStatisticsAggregator dailyStatisticsAggregator, IValidatedVolumeDayRepository validatedVolumeDayRepository)
		{
			_dailyStatisticsAggregator = dailyStatisticsAggregator;
			_validatedVolumeDayRepository = validatedVolumeDayRepository;
		}

		public TaskOwnerPeriod Fetch(IWorkload workload, DateOnlyPeriod period)
		{
			var statistics = _dailyStatisticsAggregator.LoadDailyStatistics(workload, period);
			var validatedDays = _validatedVolumeDayRepository.FindRange(period, workload) ?? Enumerable.Empty<IValidatedVolumeDay>();

			var dailyStatistics = new List<DailyStatistic>();
			foreach (var day in period.DayCollection())
			{
				var validated = validatedDays.FirstOrDefault(v => v.VolumeDayDate == day);
				if (validated != null)
				{
					dailyStatistics.Add(new DailyStatistic(day, (int)validated.ValidatedTasks));
				}
				else
				{
					var foundDailyStatistics = statistics.FirstOrDefault(s => s.Date == day);
					if (foundDailyStatistics.Date > DateOnly.MinValue)
					{
						dailyStatistics.Add(foundDailyStatistics);
					}
				}
			}
			return new TaskOwnerPeriod(DateOnly.MinValue, dailyStatistics.Convert(workload), TaskOwnerPeriodType.Other);
		}
	}
}