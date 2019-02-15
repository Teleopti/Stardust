using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.TestCommon.FakeData
{
	public static class SkillDayFactory
	{
		public static ISkillDay CreateSkillDay(DateOnly dt)
		{
			ISkill skill = new Skill("skill1", "skill1", Color.FromArgb(255), 15, SkillTypeFactory.CreateSkillTypePhone());
			return CreateSkillDay(skill, dt);
		}

		public static ISkillDay CreateSkillDay(ISkill skill, DateOnly dt, IScenario scenario)
		{
			IList<ISkillDataPeriod> skillDataPeriods = new List<ISkillDataPeriod>();
			skillDataPeriods.Add(
				new SkillDataPeriod(
					new ServiceAgreement(
						new ServiceLevel(
							new Percent(0.8), 20),
								new Percent(0.5),
								new Percent(0.7)),
								new SkillPersonData(),
								TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(dt.Date.Add(TimeSpan.FromHours(4)), dt.Date.Add(TimeSpan.FromHours(19)),skill.TimeZone)));

			if (!skill.Id.HasValue) skill.SetId(Guid.NewGuid());

			return new SkillDay(dt, skill, scenario, WorkloadDayFactory.GetWorkloadDaysForTest(dt.Date, skill), skillDataPeriods);
		}

		public static ISkillDay CreateSkillDay(ISkill skill, IWorkload workload, DateOnly date, IScenario scenario,
			bool alwaysMakeWorkloadDayOpen = true, ServiceAgreement? serviceAgreement = null, DateTimePeriod? openHours = null)
		{
			var agreement = serviceAgreement ?? new ServiceAgreement(
								new ServiceLevel(
									new Percent(0.8), 20),
								new Percent(0.5),
								new Percent(0.7));


			var skillDataPeriodsOpenHours = openHours ?? TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
				date.Date.Add(TimeSpan.FromHours(4)),
				date.Date.Add(TimeSpan.FromHours(19)), skill.TimeZone);
			
			IList<ISkillDataPeriod> skillDataPeriods = new List<ISkillDataPeriod>();
			skillDataPeriods.Add(new SkillDataPeriod(agreement,new SkillPersonData(), skillDataPeriodsOpenHours));

			if (!skill.Id.HasValue) skill.SetId(Guid.NewGuid());

			return new SkillDay(date, skill, scenario,
				WorkloadDayFactory.GetWorkloadDaysForTest(date.Date, date.Date, workload, alwaysMakeWorkloadDayOpen),
				skillDataPeriods);
		}

		public static ISkillDay CreateSkillDay(ISkill skill, DateOnly dt)
		{
			return CreateSkillDay(skill, dt, ScenarioFactory.CreateScenarioAggregate());
		}

		public static ISkillDay CreateSkillDay(ISkill skill, DateTime dt, IWorkload workload, IWorkload workload2)
		{
			IScenario scenario = ScenarioFactory.CreateScenarioAggregate();
			IList<ISkillDataPeriod> skillDataPeriods = new List<ISkillDataPeriod>();
			skillDataPeriods.Add(
				new SkillDataPeriod(
					new ServiceAgreement(
						new ServiceLevel(
							new Percent(0.8), 20),
								new Percent(0.5),
								new Percent(0.7)),
								new SkillPersonData(),
								new DateTimePeriod(dt.Add(TimeSpan.FromHours(4)), dt.Add(TimeSpan.FromHours(19)))));

			if(!skill.Id.HasValue) skill.SetId(Guid.NewGuid());

			var date = new DateOnly(TimeZoneHelper.ConvertFromUtc(dt, skill.TimeZone));
			ISkillDay skillDay = new SkillDay(date, skill, scenario,
											 WorkloadDayFactory.GetWorkloadDaysForTest(dt, workload, workload2),
											 skillDataPeriods);
			return skillDay;
		}

		public static ISkillSkillStaffPeriodExtendedDictionary CreateSkillDaysForActivityDividerTest(IDictionary<string, ISkill> skills)
		{
			DateTime dateTime = new DateTime(2008, 1, 2, 10, 00, 00, DateTimeKind.Utc);

			return getSkillStaffDictionary(skills, dateTime);
		}

		public static ISkillSkillStaffPeriodExtendedDictionary CreateSkillDaysForActivityDividerTest(IDictionary<string, ISkill> skills, DateTime skillDayDate)
		{
			return getSkillStaffDictionary(skills, skillDayDate);
		}

		private static ISkillSkillStaffPeriodExtendedDictionary getSkillStaffDictionary(IDictionary<string, ISkill> skills, DateTime skillDayDate)
		{
			ISkillSkillStaffPeriodExtendedDictionary retDic = new SkillSkillStaffPeriodExtendedDictionary();

			retDic.Add(skills["PhoneA"], PrepareSkillDay(skills["PhoneA"], skillDayDate, 5));
			retDic.Add(skills["PhoneB"], PrepareSkillDay(skills["PhoneB"], skillDayDate, 10));
			retDic.Add(skills["PhoneC"], PrepareSkillDay(skills["PhoneC"], skillDayDate, 0));

			return retDic;
		}

		public static ISkillStaffPeriodDictionary PrepareSkillDay(ISkill skill,DateTime dateTime, double forecastValue)
		{
			ISkillStaffPeriod skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(skill, dateTime, forecastValue, 0d);

			return new SkillStaffPeriodDictionary(skill) { { skillStaffPeriod.Period, skillStaffPeriod } };
		}

		public static TaskOwnerHelper GenerateStatistics(DateOnly startDate, IWorkload workload)
		{
			IList<IStatisticTask> statisticTasks = new List<IStatisticTask>();
			IList<IWorkloadDay> workloadDays = new List<IWorkloadDay>();
			var currentDate = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);

			for (int i = 0; i < 24; i++)
			{
				DateTime date = currentDate.AddMonths(i);

				StatisticTask statisticTask1 = new StatisticTask();
				if (i > 6)
				{
					statisticTask1.Interval = date.AddHours(8);
					statisticTask1.StatAbandonedTasks = 0;
					statisticTask1.StatAnsweredTasks = 40;
					statisticTask1.StatAverageAfterTaskTimeSeconds = 100;
					statisticTask1.StatAverageTaskTimeSeconds = 10;
					statisticTask1.StatOfferedTasks = 40;
					statisticTask1.StatCalculatedTasks = 40;
				}

				StatisticTask statisticTask2 = new StatisticTask();
				statisticTask2.Interval = date.AddHours(9);
				statisticTask2.StatAbandonedTasks = 0;
				statisticTask2.StatAnsweredTasks = 60;
				statisticTask2.StatAverageAfterTaskTimeSeconds = 50;
				statisticTask2.StatAverageTaskTimeSeconds = 20;
				statisticTask2.StatOfferedTasks = 60;
				statisticTask2.StatCalculatedTasks = 60;

				StatisticTask statisticTask3 = new StatisticTask();
				statisticTask3.Interval = date.AddHours(10);
				statisticTask3.StatAbandonedTasks = 0;
				statisticTask3.StatAnsweredTasks = 50;
				statisticTask3.StatAverageAfterTaskTimeSeconds = 50;
				statisticTask3.StatAverageTaskTimeSeconds = 20;
				statisticTask3.StatOfferedTasks = 50;
				statisticTask3.StatCalculatedTasks = 50;

				statisticTasks.Add(statisticTask1);
				statisticTasks.Add(statisticTask2);
				statisticTasks.Add(statisticTask3);

				IWorkloadDay workloadDay = WorkloadDayFactory.GetWorkloadDaysForTest(date, date, workload)[0];
				workloadDays.Add(workloadDay);
			}
			TaskOwnerHelper period = new TaskOwnerHelper(workloadDays);
			period.BeginUpdate();
			new Statistic(workload).Match(workloadDays.OfType<IWorkloadDayBase>(), statisticTasks);
			period.EndUpdate();

			return period;
		}

		public static TaskOwnerHelper GenerateMockedStatistics(DateOnly startDate, IWorkload workload)
		{
			IList<IWorkloadDay> workloadDays = new List<IWorkloadDay>();
			var currentDate = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);

			for (int i = 0; i < 24; i++)
			{
				DateTime date = currentDate.AddMonths(i);
			
				IWorkloadDay workloadDay = new WorkloadDay();
				workloadDay.Create(new DateOnly(date),workload,new List<TimePeriod>{new TimePeriod(8,0,8,15)});
				
				workloadDay.Tasks = 30;
				workloadDay.AverageAfterTaskTime = TimeSpan.FromSeconds(3);
				workloadDay.AverageTaskTime=TimeSpan.FromSeconds(2);

				if (i > 6)
				{
					workloadDay.TaskPeriodList[0].StatisticTask.StatAbandonedTasks = 0;
					workloadDay.TaskPeriodList[0].StatisticTask.StatAnsweredTasks = 110;
					workloadDay.TaskPeriodList[0].StatisticTask.StatAverageAfterTaskTimeSeconds = 10;
					workloadDay.TaskPeriodList[0].StatisticTask.StatAverageTaskTimeSeconds=20;
					workloadDay.TaskPeriodList[0].StatisticTask.StatCalculatedTasks = 110;
					workloadDay.TaskPeriodList[0].StatisticTask.StatOfferedTasks = 110;
				}
				else
				{
					workloadDay.TaskPeriodList[0].StatisticTask.StatAbandonedTasks = 0;
					workloadDay.TaskPeriodList[0].StatisticTask.StatAnsweredTasks = 80;
					workloadDay.TaskPeriodList[0].StatisticTask.StatAverageAfterTaskTimeSeconds = 20;
					workloadDay.TaskPeriodList[0].StatisticTask.StatAverageTaskTimeSeconds = 30;
					workloadDay.TaskPeriodList[0].StatisticTask.StatCalculatedTasks = 80;
					workloadDay.TaskPeriodList[0].StatisticTask.StatOfferedTasks = 80;
				}

				workloadDay.Initialize();
				workloadDays.Add(workloadDay);
			}
			return new TaskOwnerHelper(workloadDays);
		}

		public static TaskOwnerHelper GenerateStatisticsForWeekTests(DateTime startDate, IWorkload workload)
		{
			IList<IWorkloadDay> workloadDays = new List<IWorkloadDay>();
			IList<IStatisticTask> statisticTasks = new List<IStatisticTask>();
			DateTime date = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
			
			for (int i = 0; i < 10; i++)
			{
				if (i == 4 || i==9)
					date = date.AddDays(2);
				else
					date = date.AddDays(7);

				StatisticTask statisticTask1 = new StatisticTask();
				if (i > 6)
				{
					statisticTask1.Interval = date.AddHours(8);
					statisticTask1.StatAbandonedTasks = 0;
					statisticTask1.StatAnsweredTasks = 40;
					statisticTask1.StatAverageAfterTaskTimeSeconds = 100;
					statisticTask1.StatAverageTaskTimeSeconds = 10;
					statisticTask1.StatCalculatedTasks = 40;
					statisticTask1.StatOfferedTasks = 40;
				}

				StatisticTask statisticTask2 = new StatisticTask();
				statisticTask2.Interval = date.AddHours(9);
				statisticTask2.StatAbandonedTasks = 0;
				statisticTask2.StatAnsweredTasks = 60;
				statisticTask2.StatAverageAfterTaskTimeSeconds = 50;
				statisticTask2.StatAverageTaskTimeSeconds = 20;
				statisticTask2.StatCalculatedTasks = 60;
				statisticTask2.StatOfferedTasks = 60;

				StatisticTask statisticTask3 = new StatisticTask();
				statisticTask3.Interval = date.AddHours(10);
				statisticTask3.StatAbandonedTasks = 0;
				statisticTask3.StatAnsweredTasks = 50;
				statisticTask3.StatAverageAfterTaskTimeSeconds = 50;
				statisticTask3.StatAverageTaskTimeSeconds = 20;
				statisticTask3.StatCalculatedTasks = 50;
				statisticTask3.StatOfferedTasks = 50;

				statisticTasks.Add(statisticTask1);
				statisticTasks.Add(statisticTask2);
				statisticTasks.Add(statisticTask3);

				IWorkloadDay workloadDay = WorkloadDayFactory.GetWorkloadDaysForTest(date, date, workload)[0];
				workloadDays.Add(workloadDay);

			}
			TaskOwnerHelper period = new TaskOwnerHelper(workloadDays);
			period.BeginUpdate();
			new Statistic(workload).Match(workloadDays.OfType<IWorkloadDayBase>(), statisticTasks);
			period.EndUpdate();

			return period;
		}

		public static TaskOwnerHelper GenerateStatisticsForDayTests(DateTime startDate, IWorkload workload)
		{
			IList<ITaskOwner> workloadDays = new List<ITaskOwner>();
			IList<IStatisticTask> statisticTasks = new List<IStatisticTask>();
			DateTime date = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);

			for (int i = 0; i < 14; i++)
			{
				date = date.AddDays(1);

				StatisticTask statisticTask1 = new StatisticTask();
				if (i == 0 )
				{
					statisticTask1.Interval = date.AddHours(8);
					statisticTask1.StatAbandonedTasks = 0;
					statisticTask1.StatAnsweredTasks = 40;
					statisticTask1.StatAverageAfterTaskTimeSeconds = 100;
					statisticTask1.StatAverageTaskTimeSeconds = 10;
					statisticTask1.StatCalculatedTasks = 40;
					statisticTask1.StatOfferedTasks = 40;
				}

				StatisticTask statisticTask2 = new StatisticTask();
				statisticTask2.Interval = date.AddHours(9);
				statisticTask2.StatAbandonedTasks = 0;
				statisticTask2.StatAnsweredTasks = 60;
				statisticTask2.StatAverageAfterTaskTimeSeconds = 50;
				statisticTask2.StatAverageTaskTimeSeconds = 20;
				statisticTask2.StatCalculatedTasks = 60;
				statisticTask2.StatOfferedTasks = 60;

				StatisticTask statisticTask3 = new StatisticTask();
				statisticTask3.Interval = date.AddHours(10);
				statisticTask3.StatAbandonedTasks = 0;
				statisticTask3.StatAnsweredTasks = 50;
				statisticTask3.StatAverageAfterTaskTimeSeconds = 50;
				statisticTask3.StatAverageTaskTimeSeconds = 20;
				statisticTask3.StatCalculatedTasks = 50;
				statisticTask3.StatOfferedTasks= 50;

				statisticTasks.Add(statisticTask1);
				statisticTasks.Add(statisticTask2);
				statisticTasks.Add(statisticTask3);

				IWorkloadDay workloadDay = WorkloadDayFactory.GetWorkloadDaysForTest(date, date, workload)[0];
				workloadDays.Add(workloadDay);

			}
			TaskOwnerHelper period = new TaskOwnerHelper(workloadDays);
			period.BeginUpdate();
			new Statistic(workload).Match(workloadDays.OfType<IWorkloadDayBase>(), statisticTasks);
			period.EndUpdate();

			return period;
		}
	}
}
