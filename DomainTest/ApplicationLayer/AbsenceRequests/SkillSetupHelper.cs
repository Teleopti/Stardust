using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Intraday.Domain;
using Teleopti.Ccc.Domain.Intraday.To_Staffing;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	public static class SkillSetupHelper
	{
		private const int minutesPerInterval = 15;

		public static Scenario FakeScenarioAndIntervalLength(FakeIntervalLengthFetcher intervalLengthFetcher, FakeScenarioRepository scenarioRepository)
		{
			intervalLengthFetcher.Has(minutesPerInterval);
			var scenario = ScenarioFactory.CreateScenario("scenariorita", true, true).WithId();
			scenarioRepository.Has(scenario);
			return scenario;
		}

		public static ISkill CreateSkill(int intervalLength, string skillName, TimePeriod openHours, bool isClosedOnWeekends, IActivity activity = null)
		{
			if(activity == null)
				activity = new Activity("activity_" + skillName).WithId();
			var skill =
				new Domain.Forecasting.Skill(skillName, skillName, Color.Empty, intervalLength, new SkillTypePhone(new Description("SkillTypeInboundTelephony"), ForecastSource.InboundTelephony))
				{
					TimeZone = TimeZoneInfo.Utc
				}.WithId();
			if (isClosedOnWeekends)
				WorkloadFactory.CreateWorkloadClosedOnWeekendsWithOpenHours(skill, openHours).WithId(Guid.NewGuid());
			else
				WorkloadFactory.CreateWorkloadWithOpenHours(skill, openHours).WithId(Guid.NewGuid());
			skill.Activity = activity;
			return skill;
		}

		public static ISkill CreateEmailSkill(int intervalLength, string skillName, TimePeriod openHours)
		{
			var skill =
				new Domain.Forecasting.Skill(skillName, skillName, Color.Empty, intervalLength, new SkillTypeEmail(new Description("SkillTypeEmail"), ForecastSource.Email))
				{
					TimeZone = TimeZoneInfo.Utc
				}.WithId();
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, openHours).WithId(Guid.NewGuid());

			return skill;
		}

		public static ISkillDay CreateSkillDayWithDemand(ISkill skill, IScenario scenario, DateTime userNow, TimePeriod openHours, double demand )
		{
			
			var random = new Random();
			ISkillDay skillDay;
			skillDay =
					skill.CreateSkillDayWithDemandOnInterval(scenario, new DateOnly(userNow), 3, new Tuple<TimePeriod, double>(openHours, demand)).WithId();

			var index = 0;

			var workloadDay = skillDay.WorkloadDayCollection.First();
			workloadDay.Lock();
			for (TimeSpan intervalStart = openHours.StartTime; intervalStart < openHours.EndTime; intervalStart = intervalStart.Add(TimeSpan.FromMinutes(skill.DefaultResolution)))
			{
				workloadDay.TaskPeriodList[index].Tasks = random.Next(5, 50);
				workloadDay.TaskPeriodList[index].AverageTaskTime = TimeSpan.FromSeconds(120);
				workloadDay.TaskPeriodList[index].AverageAfterTaskTime = TimeSpan.FromSeconds(200);
				index++;
			}
			workloadDay.Release();

			return skillDay;
		}

		public static ISkillDay CreateSkillDay(ISkill skill, IScenario scenario, DateTime userNow, TimePeriod openHours, bool addSkillDataPeriodDuplicate, bool giveDemand = true)
		{
			var demand = 3;
			if (!giveDemand)
				demand = -1;

			var random = new Random();
			ISkillDay skillDay;
			if (addSkillDataPeriodDuplicate)
				skillDay =
					skill.CreateSkillDayWithDemandOnIntervalWithSkillDataPeriodDuplicate(scenario, new DateOnly(userNow), 3,
						new Tuple<TimePeriod, double>(openHours, demand)).WithId();
			else
				skillDay =
					skill.CreateSkillDayWithDemandOnInterval(scenario, new DateOnly(userNow), 3, new Tuple<TimePeriod, double>(openHours, demand)).WithId();

			var index = 0;

			var workloadDay = skillDay.WorkloadDayCollection.First();
			workloadDay.Lock();
			for (TimeSpan intervalStart = openHours.StartTime; intervalStart < openHours.EndTime; intervalStart = intervalStart.Add(TimeSpan.FromMinutes(skill.DefaultResolution)))
			{
				workloadDay.TaskPeriodList[index].Tasks = random.Next(5, 50);
				workloadDay.TaskPeriodList[index].AverageTaskTime = TimeSpan.FromSeconds(120);
				workloadDay.TaskPeriodList[index].AverageAfterTaskTime = TimeSpan.FromSeconds(200);
				index++;
			}
			workloadDay.Release();

			return skillDay;
		}

		public static IList<SkillIntervalStatistics> CreateStatistics(ISkillDay skillDay, DateTime latestStatsTime, FakeUserTimeZone timeZone)
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
				skillStats.Add(new SkillIntervalStatistics
				{
					SkillId = skillDay.Skill.Id.GetValueOrDefault(),
					WorkloadId = skillDay.WorkloadDayCollection.First().Workload.Id.Value,
					StartTime = TimeZoneHelper.ConvertFromUtc(intervalTime, timeZone.TimeZone()),
					Calls = random.Next(5, 50),
					AverageHandleTime = 40d
				});
			}
			return skillStats;
		}

		public static double CalculateAverageDeviation(IList<SkillIntervalStatistics> actualCallsStats, ISkillDay skillDay, FakeUserTimeZone timeZone)
		{
			var listdeviationPerInterval = new List<double>();
			var divisionFactor = skillDay.Skill.DefaultResolution / minutesPerInterval;
			foreach (var actualCallsPerInterval in actualCallsStats)
			{
				var actualCalls = actualCallsPerInterval.Calls;
				var forecastedCalls = skillDay.CompleteSkillStaffPeriodCollection
					.Where(x => x.Period.StartDateTime <= TimeZoneHelper.ConvertToUtc(actualCallsPerInterval.StartTime, timeZone.TimeZone()))
					.Select(t => t.Payload.TaskData.Tasks)
					.Last();
				forecastedCalls = forecastedCalls / divisionFactor;
				var deviationPerInterval = actualCalls / forecastedCalls;
				listdeviationPerInterval.Add(deviationPerInterval);
			}
			var alpha = 0.2d;
			return listdeviationPerInterval.Aggregate((prev, next) => alpha * next + (1 - alpha) * prev);
		}

		public static void PopulateStaffingReadModels(ISkill skill, DateTime scheduledStartTime,
			DateTime scheduledEndTime, double staffing,
			FakeSkillCombinationResourceRepository skillCombinationResourceRepository)
		{
			var skillCombinationResources = new List<SkillCombinationResource>();

			for (var intervalTime = scheduledStartTime;
				intervalTime < scheduledEndTime;
				intervalTime = intervalTime.AddMinutes(minutesPerInterval))
			{
				skillCombinationResources.Add(new SkillCombinationResource
				{
					StartDateTime = intervalTime,
					EndDateTime = intervalTime.AddMinutes(minutesPerInterval),
					Resource = staffing,
					SkillCombination = new HashSet<Guid> { skill.Id.GetValueOrDefault() }
				});
			}
			skillCombinationResourceRepository.AddSkillCombinationResource(DateTime.UtcNow, skillCombinationResources);
		}

		public static void PopulateForecastReadModels(ISkill skill, DateTime scheduledStartTime,
			DateTime scheduledEndTime, double forecastedAgents,
			FakeSkillForecastReadModelRepository skillForecastReadModelRepository)
		{
			skillForecastReadModelRepository.SkillForecasts = new List<SkillForecast>();

			for (var intervalTime = scheduledStartTime;
				intervalTime < scheduledEndTime;
				intervalTime = intervalTime.AddMinutes(minutesPerInterval))
			{
				skillForecastReadModelRepository.SkillForecasts.Add(new SkillForecast
				{
					StartDateTime = intervalTime,
					EndDateTime = intervalTime.AddMinutes(minutesPerInterval),
					 Agents = forecastedAgents,
					SkillId = skill.Id.GetValueOrDefault() 
				});
			}
			
		}
	}
}
