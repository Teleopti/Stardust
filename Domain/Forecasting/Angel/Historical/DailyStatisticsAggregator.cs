using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Historical
{
	public class DailyStatisticsAggregator : IDailyStatisticsAggregator
	{
		private readonly IStatisticRepository _statisticRepository;

		public DailyStatisticsAggregator(IStatisticRepository statisticRepository)
		{
			_statisticRepository = statisticRepository;
		}

		public IEnumerable<DailyStatistic> LoadDailyStatistics(IWorkload workload, DateOnlyPeriod dateRange)
		{
			var statisticTasks = _statisticRepository.LoadSpecificDates(workload.QueueSourceCollection,
				dateRange.ToDateTimePeriod(TimeZoneInfo.Utc));

			var result = from t in statisticTasks
						 group t by t.Interval.Date into g
						select new DailyStatistic(new DateOnly(g.Key),(int)g.Sum(k => k.StatAnsweredTasks));
			return result;
		}
	}
}