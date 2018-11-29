using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Historical
{
	public class DailyStatisticsProvider : IDailyStatisticsProvider
	{
		private readonly IRepositoryFactory _repositoryFactory;

		public DailyStatisticsProvider(IRepositoryFactory repositoryFactory)
		{
			_repositoryFactory = repositoryFactory;
		}

		public IEnumerable<DailyStatistic> LoadDailyStatistics(IWorkload workload, DateOnlyPeriod dateRange)
		{
			var statisticRepository = _repositoryFactory.CreateStatisticRepository();
			var statisticTasks = statisticRepository.LoadDailyStatisticForSpecificDates(workload.QueueSourceCollection,
				dateRange.ToDateTimePeriod(workload.Skill.TimeZone), workload.Skill.TimeZone.Id, workload.Skill.MidnightBreakOffset);

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