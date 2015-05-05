﻿using System.Collections.Generic;
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
			var statisticTasks = _statisticRepository.LoadDailyStatisticForSpecificDates(workload.QueueSourceCollection,
				dateRange.ToDateTimePeriod(workload.Skill.TimeZone), workload.Skill.Id.Value, workload.Skill.MidnightBreakOffset);

			var calculator = new QueueStatisticsCalculator(workload.QueueAdjustments);
			var result = from t in statisticTasks
				select calculateTasksForDailyStatistics(t, calculator);
			return result.ToArray(); //perf: no deferred execution here!
		}

		private static DailyStatistic calculateTasksForDailyStatistics(IStatisticTask statisticTask, QueueStatisticsCalculator calculator)
		{
			calculator.Calculate(statisticTask);

			return new DailyStatistic(new DateOnly(statisticTask.Interval), (int) statisticTask.StatCalculatedTasks,
				statisticTask.StatAverageTaskTimeSeconds,
				statisticTask.StatAverageAfterTaskTimeSeconds);
		}
	}
}