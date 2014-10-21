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

			var tempSum = statisticTasks.Sum(x => x.StatAnsweredTasks);
			var tempDate = statisticTasks.First().Interval.Date;

			var ret = new DailyStatistic(new DateOnly(tempDate), (int)tempSum);
			return new[] { ret };
		}
	}
}