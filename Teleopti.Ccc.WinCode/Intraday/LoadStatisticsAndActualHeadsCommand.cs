﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Intraday
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
		public virtual void Execute(DateOnly dateOnly, ISkill skill, IList<ISkillStaffPeriod> skillStaffPeriods)
		{
			if (!skill.WorkloadCollection.Any()) return;
			var statisticTasks = new List<IStatisticTask>();
			var period = new DateOnlyPeriod(dateOnly, dateOnly).ToDateTimePeriod(skill.TimeZone);
			period = period.ChangeEndTime(skill.MidnightBreakOffset.Add(TimeSpan.FromHours(1)));

			var skillDays = skillStaffPeriods.Select(s => (ISkillDay) s.Parent).Distinct();
			foreach (var workload in skill.WorkloadCollection)
			{
				var workloadDays = _workloadDayHelper.GetWorkloadDaysFromSkillDays(skillDays, workload);
				var tasks = _statisticRepository.LoadSpecificDates(workload.QueueSourceCollection, period).ToList();

				new Statistic(workload).Match(workloadDays, tasks);
				foreach (var workloadDay in workloadDays)
				{
					statisticTasks.AddRange(workloadDay.OpenTaskPeriodList.Select(t => t.StatisticTask));
				}
			}

			var activeAgentCounts = _statisticRepository.LoadActiveAgentCount(skill, period);

			var taskPeriods = Statistic.CreateTaskPeriodsFromPeriodized(skillStaffPeriods);
			var provider = new QueueStatisticsProvider(statisticTasks,
			                                           new QueueStatisticsCalculator(
			                                           	skill.WorkloadCollection.First().QueueAdjustments));
			foreach (var taskPeriod in taskPeriods)
			{
				Statistic.UpdateStatisticTask(provider.GetStatisticsForPeriod(taskPeriod.Period), taskPeriod);
			}
			Statistic.Match(skillStaffPeriods, taskPeriods, activeAgentCounts);
		}
	}
}