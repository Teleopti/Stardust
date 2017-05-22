using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

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

		public static ISkill CreateSkill(int intervalLength, string skillName, TimePeriod openHours, bool isClosedOnWeekends, IActivity activity)
		{
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
				new Domain.Forecasting.Skill(skillName, skillName, Color.Empty, intervalLength, new SkillTypeEmail(new Description("SkillTypeEmail"), ForecastSource.InboundTelephony))
				{
					TimeZone = TimeZoneInfo.Utc
				}.WithId();
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, openHours).WithId(Guid.NewGuid());

			return skill;
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
					skill.CreateSkillDayWithDemandOnInterval(scenario, new DateOnly(userNow), 3, ServiceAgreement.DefaultValues(),
						new Tuple<TimePeriod, double>(openHours, demand)).WithId();

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

		public static IList<SkillIntervalStatistics> CreateSkillDaysYesterdayTodayTomorrow(ISkill skill, Scenario scenario,
			DateTime userNow, DateTime latestStatsTime, FakeSkillDayRepository skillDayRepository, FakeUserTimeZone timeZone)
		{
			var skillStats = new List<SkillIntervalStatistics>();
			var timePeriod = new TimePeriod(0, 0, 24, 0);
			var skillYesterday = CreateSkillDay(skill, scenario, userNow.AddDays(-1), timePeriod, false);
			var skillToday = CreateSkillDay(skill, scenario, userNow, timePeriod, false);
			var skillTomorrow = CreateSkillDay(skill, scenario, userNow.AddDays(1), timePeriod, false);
			skillDayRepository.Has(skillYesterday, skillToday, skillTomorrow);
			skillStats.AddRange(CreateStatistics(skillYesterday, latestStatsTime, timeZone));
			skillStats.AddRange(CreateStatistics(skillToday, latestStatsTime, timeZone));
			skillStats.AddRange(CreateStatistics(skillTomorrow, latestStatsTime, timeZone));

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

		public static double GetActualStaffing(ISkillDay skillDay, IList<SkillIntervalStatistics> actualCalls)
		{
			var calculatorService = new StaffingCalculatorServiceFacade();
			var skillData = skillDay.SkillDataPeriodCollection.First().ServiceAgreement;
			var actualStaffingSkill1 = calculatorService.AgentsUseOccupancy(
				skillData.ServiceLevel.Percent.Value,
				(int)skillData.ServiceLevel.Seconds,
				actualCalls.First().Calls,
				actualCalls.First().AverageHandleTime,
				TimeSpan.FromMinutes(minutesPerInterval),
				skillData.MinOccupancy.Value,
				skillData.MaxOccupancy.Value,
				skillDay.Skill.MaxParallelTasks);

			return actualStaffingSkill1;
		}

		public static IEnumerable<SkillStaffingInterval> PopulateStaffingReadModels(ISkill skill, DateTime scheduledStartTime,
			DateTime scheduledEndTime, double staffing,
			FakeScheduleForecastSkillReadModelRepository scheduleForecastSkillReadModelRepository,
			FakeSkillCombinationResourceRepository skillCombinationResourceRepository)
		{
			var skillStaffingIntervals = new List<SkillStaffingInterval>();
			var skillCombinationResources = new List<SkillCombinationResource>();

			for (DateTime intervalTime = scheduledStartTime;
				intervalTime < scheduledEndTime;
				intervalTime = intervalTime.AddMinutes(minutesPerInterval))
			{

				skillStaffingIntervals.Add(new SkillStaffingInterval
				{
					SkillId = skill.Id.GetValueOrDefault(),
					StartDateTime = intervalTime,
					EndDateTime = intervalTime.AddMinutes(minutesPerInterval),
					Forecast = 1,
					ForecastWithShrinkage = 2
				});
				skillCombinationResources.Add(new SkillCombinationResource
				{
					StartDateTime = intervalTime,
					EndDateTime = intervalTime.AddMinutes(minutesPerInterval),
					Resource = staffing,
					SkillCombination = new[] { skill.Id.GetValueOrDefault() }
				});
			}
			scheduleForecastSkillReadModelRepository.Persist(skillStaffingIntervals, DateTime.UtcNow);
			skillCombinationResourceRepository.AddSkillCombinationResource(DateTime.UtcNow, skillCombinationResources);
			return skillStaffingIntervals;
		}
	}
}
