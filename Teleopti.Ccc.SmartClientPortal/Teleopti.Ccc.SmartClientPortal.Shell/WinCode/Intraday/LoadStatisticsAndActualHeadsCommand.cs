using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Intraday
{
	public class LoadStatisticsAndActualHeadsCommand
	{
		private readonly IStatisticRepository _statisticRepository;
		private readonly WorkloadDayHelper _workloadDayHelper = new WorkloadDayHelper();

		public LoadStatisticsAndActualHeadsCommand(IStatisticRepository statisticRepository)
		{
			_statisticRepository = statisticRepository;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public virtual void Execute(DateOnly dateOnly, ISkill skill, DateTimePeriod relevantPeriod, IList<ISkillStaffPeriod> skillStaffPeriods)
		{
			if (!skill.WorkloadCollection.Any()) return;
			var statisticTasks = new List<IStatisticTask>();

			var skillDays = skillStaffPeriods.Select(s => s.SkillDay).Distinct();
			foreach (var workload in skill.WorkloadCollection)
			{
				var workloadDays = _workloadDayHelper.GetWorkloadDaysFromSkillDays(skillDays, workload);
				var tasks = _statisticRepository.LoadSpecificDates(workload.QueueSourceCollection, relevantPeriod).ToList();

				new Statistic(workload).Match(workloadDays, tasks);
				foreach (var workloadDay in workloadDays)
				{
					statisticTasks.AddRange(workloadDay.OpenTaskPeriodList.Select(t => t.StatisticTask));
				}
			}

			var activeAgentCounts = _statisticRepository.LoadActiveAgentCount(skill, relevantPeriod);

			var taskPeriods = skillStaffPeriods.CreateTaskPeriodsFromPeriodized();
			var provider = new QueueStatisticsProvider(statisticTasks,
			                                           new QueueStatisticsCalculator(
			                                           	skill.WorkloadCollection.First().QueueAdjustments));
			var stat = new Statistic(null);
			foreach (var taskPeriod in taskPeriods)
			{
				provider.GetStatisticsForPeriod(taskPeriod.Period).ApplyStatisticsTo(taskPeriod);
			}
			stat.Match(skillStaffPeriods, taskPeriods, activeAgentCounts);
		}
	}
}