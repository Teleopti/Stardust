using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.Forecast
{
	public interface IStatisticLoader
	{
		DateTime Execute(DateTimePeriod period, IWorkloadDay workloadDay, IList<ISkillStaffPeriod> skillStaffPeriods);
	}

	public class StatisticLoader : IStatisticLoader
	{
		private readonly IStatisticRepository _statisticRepository;
		private readonly IStatistic _statistic;

		public StatisticLoader(IStatisticRepository statisticRepository, IStatistic statistic)
		{
			_statisticRepository = statisticRepository;
			_statistic = statistic;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public DateTime Execute(DateTimePeriod period, IWorkloadDay workloadDay, IList<ISkillStaffPeriod> skillStaffPeriods)
		{
			var ret = new DateTime();
			var statisticTasks = new List<IStatisticTask>();
			
			// we return the latest interval from these to know how long we have statistics
			var tasks = _statisticRepository.LoadSpecificDates(workloadDay.Workload.QueueSourceCollection, period).ToList();
			if(tasks.Any())
			{ // is the interval the startime or the endtime of the interval, I guess start
				ret = (from t in tasks select t.Interval).Max();
			}

			_statistic.Match(workloadDay.Workload, new List<IWorkloadDayBase> { workloadDay }, tasks);
			
			statisticTasks.AddRange(workloadDay.OpenTaskPeriodList.Select(t => t.StatisticTask));
			
			var taskPeriods = skillStaffPeriods.CreateTaskPeriodsFromPeriodized();
			var provider = new QueueStatisticsProvider(statisticTasks,
													   new QueueStatisticsCalculator(
														workloadDay.Workload.QueueAdjustments));

			foreach (var taskPeriod in taskPeriods)
			{
				provider.GetStatisticsForPeriod(taskPeriod.Period).ApplyStatisticsTo(taskPeriod);
			}
			// we don't care about active agents
			_statistic.Match(skillStaffPeriods, taskPeriods, new List<IActiveAgentCount>());

			return ret;
		}
	}
}