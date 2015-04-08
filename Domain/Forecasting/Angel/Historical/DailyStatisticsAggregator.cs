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
			var statisticTasks = _statisticRepository.LoadHourlyStatisticForSpecificDates(workload.QueueSourceCollection,
				dateRange.ToDateTimePeriod(workload.Skill.TimeZone));

			var calculator = new QueueStatisticsCalculator(workload.QueueAdjustments);
			var result = from t in statisticTasks
				group t by
					TimeZoneHelper.ConvertFromUtc(t.Interval, workload.Skill.TimeZone)
						.Subtract(workload.Skill.MidnightBreakOffset)
						.Date
				into g
				select aggregateDailyNumbers(g, calculator);
			return result.ToArray(); //perf: no deferred execution here!
		}

		private static DailyStatistic aggregateDailyNumbers(IGrouping<DateTime, IStatisticTask> grouping, QueueStatisticsCalculator calculator)
		{
			double sumCalculatedTasks=0;
			double sumAnsweredTasks = 0;
			double totalTimeAnsweredTasks=0;
			double totalAfterTimeAnsweredTasks=0;
			var amountItems=0;

			foreach (var statisticTask in grouping)
			{
				amountItems++;
				calculator.Calculate(statisticTask);
				sumAnsweredTasks += statisticTask.StatAnsweredTasks;
				sumCalculatedTasks += statisticTask.StatCalculatedTasks;
				var answeredTasksWithLowestPossibleOne = Math.Max(statisticTask.StatAnsweredTasks,1);
				totalTimeAnsweredTasks += answeredTasksWithLowestPossibleOne*statisticTask.StatAverageTaskTimeSeconds;
				totalAfterTimeAnsweredTasks += answeredTasksWithLowestPossibleOne*statisticTask.StatAverageAfterTaskTimeSeconds;
			}

			return new DailyStatistic(new DateOnly(grouping.Key), (int) sumCalculatedTasks,
				sumAnsweredTasks > 0 ? totalTimeAnsweredTasks/sumAnsweredTasks : totalTimeAnsweredTasks/amountItems,
				sumAnsweredTasks > 0 ? totalAfterTimeAnsweredTasks/sumAnsweredTasks : totalAfterTimeAnsweredTasks/amountItems);
		}
	}
}