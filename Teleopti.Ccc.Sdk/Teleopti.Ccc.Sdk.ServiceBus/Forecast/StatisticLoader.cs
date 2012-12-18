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

		public StatisticLoader(IStatisticRepository statisticRepository)
		{
			_statisticRepository = statisticRepository;
		}

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

			new Statistic(workloadDay.Workload).Match(new List<IWorkloadDayBase>{workloadDay}, tasks);
			
			statisticTasks.AddRange(workloadDay.OpenTaskPeriodList.Select(t => t.StatisticTask));
			
			//var activeAgentCounts = statisticRepository.LoadActiveAgentCount(skill, period);

			var taskPeriods = Statistic.CreateTaskPeriodsFromPeriodized(skillStaffPeriods);
			var provider = new QueueStatisticsProvider(statisticTasks,
													   new QueueStatisticsCalculator(
														workloadDay.Workload.QueueAdjustments));
			foreach (var taskPeriod in taskPeriods)
			{
				Statistic.UpdateStatisticTask(provider.GetStatisticsForPeriod(taskPeriod.Period), taskPeriod);
			}
			// we don't care about active agents
			Statistic.Match(skillStaffPeriods, taskPeriods, new List<IActiveAgentCount>());

			return ret;
		}
	}
}