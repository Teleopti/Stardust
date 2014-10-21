using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.HistoricalData
{
	public class HistoricalDataProvider : IHistoricalDataProvider
	{
		private readonly IDailyStatisticsAggregator _dailyStatisticsAggregator;
		private readonly IValidatedVolumeDayRepository _validatedVolumeDayRepository;

		public HistoricalDataProvider(IDailyStatisticsAggregator dailyStatisticsAggregator, IValidatedVolumeDayRepository validatedVolumeDayRepository)
		{
			_dailyStatisticsAggregator = dailyStatisticsAggregator;
			_validatedVolumeDayRepository = validatedVolumeDayRepository;
		}

		public IEnumerable<DailyStatistic> Calculate(IWorkload workload, DateOnlyPeriod period)
		{
			var statistics = _dailyStatisticsAggregator.LoadDailyStatistics(workload, period);
			var validatedDays = _validatedVolumeDayRepository == null ? 
				Enumerable.Empty<IValidatedVolumeDay>() : 
				_validatedVolumeDayRepository.FindRange(period, workload);

			var ret = new List<DailyStatistic>();
			foreach (var day in period.DayCollection())
			{
				var validated = validatedDays.FirstOrDefault(v => v.VolumeDayDate == day);
				if (validated != null)
				{
					ret.Add(new DailyStatistic(day, (int) validated.ValidatedTasks));
				}
				else
				{
					var foundDailyStatistics = statistics.FirstOrDefault(s => s.Date == day);
					if (foundDailyStatistics.Date > DateOnly.MinValue)
					{
						ret.Add(foundDailyStatistics);
					}
				}
			}
			return ret;
		}
	}
}