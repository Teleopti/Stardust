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
				dateRange.ToDateTimePeriod(workload.Skill.TimeZone));

			var calculator = new QueueStatisticsCalculator(workload.QueueAdjustments);
			var result = from t in statisticTasks
				group t by
					TimeZoneHelper.ConvertFromUtc(t.Interval, workload.Skill.TimeZone).Subtract(workload.Skill.MidnightBreakOffset).Date
				into g
				select new DailyStatistic(new DateOnly(g.Key), (int) g.Sum(k =>
				{
					calculator.Calculate(k);
					return k.StatCalculatedTasks;
				}));
			return result;
		}
	}
}