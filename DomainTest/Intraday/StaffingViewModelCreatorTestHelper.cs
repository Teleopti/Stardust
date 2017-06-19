using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Intraday
{
	public static class StaffingViewModelCreatorTestHelper
	{
		public static Scenario FakeScenarioAndIntervalLength(FakeIntervalLengthFetcher intervalLengthFetcher, FakeScenarioRepository scenarioRepository, int minutesPerInterval)
		{
			intervalLengthFetcher.Has(minutesPerInterval);
			var scenario = ScenarioFactory.CreateScenario("scenariorita", true, true).WithId();
			scenarioRepository.Has(scenario);
			return scenario;
		}

		public static ISkill CreateEmailSkill(int intervalLength, string skillName, TimePeriod openHours)
		{
			var activity = new Activity("activity_" + skillName).WithId();
			var skill =
				new Skill(skillName, skillName, Color.Empty, intervalLength, new SkillTypeEmail(new Description("SkillTypeEmail"), ForecastSource.Email))
				{
					Activity = activity,
					TimeZone = TimeZoneInfo.Utc
				}.WithId();
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, openHours).WithId();

			return skill;
		}

		public static ISkill CreateBackOfficeSkill(int intervalLength, string skillName, TimePeriod openHours)
		{
			var activity = new Activity("activity_" + skillName).WithId();
			var skill =
				new Skill(skillName, skillName, Color.Empty, intervalLength, new SkillTypeEmail(new Description("SkillTypeBackoffice"), ForecastSource.Email))
				{
					Activity = activity,
					TimeZone = TimeZoneInfo.Utc
				}.WithId();
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, openHours).WithId();

			return skill;
		}

		public static ISkillDay CreateSkillDay(ISkill skill, IScenario scenario, DateTime userNow, TimePeriod openHours, 
			bool addSkillDataPeriodDuplicate, ServiceAgreement serviceAgreement, bool giveDemand = true)
		{
			var demand = 3;
			if (!giveDemand)
				demand = -1;
				
			var random = new Random();
			ISkillDay skillDay;
			if (addSkillDataPeriodDuplicate)
				skillDay =
					skill.CreateSkillDayWithDemandOnIntervalWithSkillDataPeriodDuplicate(scenario, new DateOnly(userNow), 
					demand, new Tuple<TimePeriod, double>(openHours, demand))
					.WithId();
			else
				skillDay =
					skill.CreateSkillDayWithDemandOnInterval(scenario, new DateOnly(userNow), 
					demand, serviceAgreement, new Tuple<TimePeriod, double>(openHours, demand))
					.WithId();

			var workloadDay = skillDay.WorkloadDayCollection.First();
			workloadDay.Lock();
			for (TimeSpan intervalStart = openHours.StartTime; intervalStart < openHours.EndTime; intervalStart = intervalStart.Add(TimeSpan.FromMinutes(skill.DefaultResolution)))
			{
				var workloadDayTaskPeriod = workloadDay.TaskPeriodList.FirstOrDefault(x => x.Period.StartDateTime.TimeOfDay == intervalStart);
				if (workloadDayTaskPeriod == null)
					continue;
				workloadDayTaskPeriod.Tasks = random.Next(5, 50);
				workloadDayTaskPeriod.AverageTaskTime = TimeSpan.FromSeconds(120);
				workloadDayTaskPeriod.AverageAfterTaskTime = TimeSpan.FromSeconds(200);
			}
			workloadDay.Release();

			return skillDay;
		}

		public static IList<SkillIntervalStatistics> CreateStatistics(ISkillDay skillDay, DateTime latestStatsTime, int minutesPerInterval, TimeZoneInfo timezone)
		{
			var skillStats = new List<SkillIntervalStatistics>();
			var shiftStartTime = skillDay.SkillStaffPeriodCollection.First().Period.StartDateTime;
			var shiftEndTime = skillDay.SkillStaffPeriodCollection.Last().Period.EndDateTime;
			if (shiftStartTime > latestStatsTime || shiftEndTime < latestStatsTime)
			{
				return skillStats;
			}
			var random = new Random();

			for (DateTime intervalTime = shiftStartTime;
				intervalTime <= latestStatsTime;
				intervalTime = intervalTime.AddMinutes(minutesPerInterval))
			{
				var calls = random.Next(5, 50);
				var answeredCalls = (int)(calls * 0.8);
				skillStats.Add(new SkillIntervalStatistics
				{
					SkillId = skillDay.Skill.Id.Value,
					WorkloadId = skillDay.WorkloadDayCollection.First().Workload.Id.Value,
					StartTime = TimeZoneHelper.ConvertFromUtc(intervalTime, timezone),
					Calls = calls,
					AnsweredCalls = answeredCalls,
					HandleTime = 120,
					AverageHandleTime = 120d / answeredCalls
				});
			}
			return skillStats;
		}

		public static IList<SkillIntervalStatistics> CreateStatisticsBasedOnForecastedTasks(ISkillDay skillDay, DateTime latestStatsTime, 
			int minutesPerInterval, TimeZoneInfo timezone)
		{
			var skillStats = new List<SkillIntervalStatistics>();
			var shiftStartTime = skillDay.SkillStaffPeriodCollection.First().Period.StartDateTime;
			var shiftEndTime = skillDay.SkillStaffPeriodCollection.Last().Period.EndDateTime;
			if (shiftStartTime > latestStatsTime || shiftEndTime < latestStatsTime)
			{
				return skillStats;
			}

			var doSplit = skillDay.Skill.DefaultResolution != minutesPerInterval;

			var workloadDay = skillDay.WorkloadDayCollection.First();
			var forecastedTasks = workloadDay.TaskPeriodList
				.Where(x => x.Period.StartDateTime >= shiftStartTime && x.Period.EndDateTime <= latestStatsTime)
				.ToArray();
			var skillId = skillDay.Skill.Id.Value;

			if (doSplit)
			{
				skillStats.AddRange(forecastedTasks
					.SelectMany(taskPeriod => taskPeriod.Split(TimeSpan.FromMinutes(minutesPerInterval))).Select(
						splittedTaskPeriod => getStatsInterval(splittedTaskPeriod.Period.StartDateTime,
							splittedTaskPeriod.TotalTasks,
							splittedTaskPeriod.TotalAverageTaskTime.TotalSeconds + splittedTaskPeriod.AverageAfterTaskTime.TotalSeconds,
							timezone,
							skillId,
							workloadDay.Workload.Id.Value)));
			}
			else
			{
				skillStats.AddRange(forecastedTasks
					.Select(taskPeriod => getStatsInterval(taskPeriod.Period.StartDateTime,
						taskPeriod.TotalTasks,
						taskPeriod.TotalAverageTaskTime.TotalSeconds + taskPeriod.TotalAverageAfterTaskTime.TotalSeconds,
						timezone, skillId,
						workloadDay.Workload.Id.Value)));
			}

			return skillStats;
		}

		private static SkillIntervalStatistics getStatsInterval(DateTime startDateTime, double totalTasks, double totalAverageHandlingTime, TimeZoneInfo timezone, Guid skillId, Guid workloadId)
		{
			var answeredCalls = (int)(totalTasks*0.8);
			var handleTime = answeredCalls * totalAverageHandlingTime;
			return new SkillIntervalStatistics
			{
				SkillId = skillId,
				WorkloadId = workloadId,
				StartTime = TimeZoneHelper.ConvertFromUtc(startDateTime, timezone),
				Calls = totalTasks,
				AnsweredCalls = answeredCalls,
				HandleTime = handleTime,
				AverageHandleTime = totalAverageHandlingTime,
			};
		}

		public static MultisiteSkill CreateMultisiteSkillPhone(int intervalLength, string skillName, TimePeriod openHours, IActivity activity)
		{
			var skill = new MultisiteSkill(skillName, skillName, Color.Empty, intervalLength, new SkillTypePhone(new Description("SkillTypeInboundTelephony"), ForecastSource.InboundTelephony))
			{
				TimeZone = TimeZoneInfo.Utc,
				Activity = activity
			}.WithId();

			var childSkill1 = new ChildSkill(skillName + 1, skillName + 1, Color.Empty,
				skill).WithId();

			var childSkill2 = new ChildSkill(skillName + 2, skillName + 2, Color.Empty,
				skill).WithId();

			childSkill1.Activity = new Activity(skillName  + 1);
			childSkill2.Activity = new Activity(skillName + 2);


			skill.AddChildSkill(childSkill1);
			skill.AddChildSkill(childSkill2);

			WorkloadFactory.CreateWorkloadWithOpenHours(skill, openHours).WithId(Guid.NewGuid());
			WorkloadFactory.CreateWorkloadWithOpenHours(childSkill1, openHours).WithId(Guid.NewGuid());
			WorkloadFactory.CreateWorkloadWithOpenHours(childSkill2, openHours).WithId(Guid.NewGuid());

			return skill;
		}
	}
}