using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics.Transformer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Intraday
{
	[DomainTest]
	[TestWithStaticDependenciesAvoidUse]
	public class MonitorPerformanceProviderTest : ISetup
	{
		public MonitorPerformanceProvider Target;
		public MutableNow Now;
		public FakeUserTimeZone TimeZone;
		public FakeScenarioRepository ScenarioRepository;
		public FakeSkillRepository SkillRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeScheduleForecastSkillReadModelRepository ScheduleForecastSkillReadModelRepository;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;
		const int minutesPerInterval = 15;
		public FakeIntradayMonitorDataLoader IntradayMonitorDataLoader;
		private StaffingCalculatorServiceFacade _staffingCalculatorService;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble(new FakeUserTimeZone(TimeZoneInfo.Utc)).For<IUserTimeZone>();
			_staffingCalculatorService = new StaffingCalculatorServiceFacade();
		}

		[Test]
		public void ShouldReturnEslForOneSkill()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);

			var scenario = fakeScenarioAndIntervalLength();
			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 15));
			var skillDay = createSkillDay(skill, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 15));

			var scheduledStaffingList = createScheduledStaffing(skillDay, latestStatsTime);

			IntradayMonitorDataLoader.AddInterval(new IncomingIntervalModel()
			{
				IntervalDate = latestStatsTime.Date,
				IntervalId = new IntervalBase(latestStatsTime, (60/minutesPerInterval)*24).Id,
				OfferedCalls = 22
			});

			SkillRepository.Has(skill);
			SkillDayRepository.Add(skillDay);
			ScheduleForecastSkillReadModelRepository.Persist(scheduledStaffingList, DateTime.MinValue);

			var result = Target.Load(new Guid[] {skill.Id.Value});

			var esl =_staffingCalculatorService.ServiceLevelAchievedOcc(
				scheduledStaffingList.First().StaffingLevel,
				skillDay.SkillDataPeriodCollection.First().ServiceAgreement.ServiceLevel.Seconds,
				skillDay.WorkloadDayCollection.First().TaskPeriodList.First().Tasks,
				skillDay.WorkloadDayCollection.First().TaskPeriodList.First().TotalAverageTaskTime.TotalSeconds +
				skillDay.WorkloadDayCollection.First().TaskPeriodList.First().TotalAverageAfterTaskTime.TotalSeconds,
				TimeSpan.FromMinutes(minutesPerInterval),
				skillDay.SkillDataPeriodCollection.First().ServiceAgreement.ServiceLevel.Percent.Value,
				skillDay.SkillStaffPeriodCollection.First().FStaff,
				1);

			result.DataSeries.EstimatedServiceLevels.Length.Should().Be.EqualTo(1);
			Math.Round(result.DataSeries.EstimatedServiceLevels.First().Value, 5).Should().Be.EqualTo(Math.Round(esl, 5));
		}

		[Test]
		public void ShouldReturnStatisticsForOneSkill()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);

			fakeScenarioAndIntervalLength();
			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 15));

			IntradayMonitorDataLoader.AddInterval(new IncomingIntervalModel()
			{
				IntervalDate = latestStatsTime.Date,
				IntervalId = new IntervalBase(latestStatsTime, (60 / minutesPerInterval) * 24).Id,
				OfferedCalls = 20,
				AnsweredCalls = 16,
				AnsweredCallsWithinSL = 16,
				SpeedOfAnswer = 10,
				AbandonedCalls = 4,
				AbandonedRate = 0.2d,
				ServiceLevel = 0.8d
			});

			SkillRepository.Has(skill);

			var result = Target.Load(new Guid[] { skill.Id.Value });

			result.DataSeries.AverageSpeedOfAnswer.Length.Should().Be.EqualTo(1);
			result.DataSeries.AbandonedRate.Length.Should().Be.EqualTo(1);
			result.DataSeries.ServiceLevel.Length.Should().Be.EqualTo(1);
			result.Summary.ServiceLevel.Should().Be.GreaterThan(0);
			result.Summary.AverageSpeedOfAnswer.Should().Be.GreaterThan(0);
			result.Summary.AbandonRate.Should().Be.GreaterThan(0);
		}

		[Test]
		public void ShouldReturnEslUpUntilLatestStatsTime()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);

			var scenario = fakeScenarioAndIntervalLength();
			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30));
			var skillDay = createSkillDay(skill, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30));

			var scheduledStaffingList = createScheduledStaffing(skillDay, latestStatsTime);

			IntradayMonitorDataLoader.AddInterval(new IncomingIntervalModel()
			{
				IntervalDate = latestStatsTime.Date,
				IntervalId = new IntervalBase(latestStatsTime, (60 / minutesPerInterval) * 24).Id,
				OfferedCalls = 22,
				ForecastedCalls = 20
			});
			IntradayMonitorDataLoader.AddInterval(new IncomingIntervalModel()
			{
				IntervalDate = latestStatsTime.Date,
				IntervalId = new IntervalBase(latestStatsTime.AddMinutes(minutesPerInterval), (60 / minutesPerInterval) * 24).Id,
				ForecastedCalls = 21
			});

			SkillRepository.Has(skill);
			SkillDayRepository.Add(skillDay);
			ScheduleForecastSkillReadModelRepository.Persist(scheduledStaffingList, DateTime.MinValue);

			var result = Target.Load(new Guid[] { skill.Id.Value });

			result.DataSeries.EstimatedServiceLevels.Length.Should().Be.EqualTo(2);
			result.DataSeries.EstimatedServiceLevels.First().Should().Be.GreaterThan(0d);
			result.DataSeries.EstimatedServiceLevels.Last().Should().Be.EqualTo(null);
		}

		[Test]
		public void ShouldReturnZeroEslWhenNoSchedule()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);

			var scenario = fakeScenarioAndIntervalLength();
			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 15));
			var skillDay = createSkillDay(skill, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 15));

			IntradayMonitorDataLoader.AddInterval(new IncomingIntervalModel()
			{
				IntervalDate = latestStatsTime.Date,
				IntervalId = new IntervalBase(latestStatsTime, (60 / minutesPerInterval) * 24).Id,
				OfferedCalls = 22
			});

			SkillRepository.Has(skill);
			SkillDayRepository.Add(skillDay);

			var result = Target.Load(new Guid[] { skill.Id.Value });

			result.DataSeries.EstimatedServiceLevels.Length.Should().Be.EqualTo(1);
			result.DataSeries.EstimatedServiceLevels.First().Should().Be.EqualTo(0d);
		}

		[Test]
		public void ShouldReturnNoEslWhenNoForecast()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			fakeScenarioAndIntervalLength();
			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 15));
			SkillRepository.Has(skill);

			var result = Target.Load(new Guid[] { skill.Id.Value });

			result.DataSeries.EstimatedServiceLevels.Should().Be.Empty();
		}

		[Test]
		public void ShouldReturnEslForTwoSkills()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);

			var scenario = fakeScenarioAndIntervalLength();
			var skill1 = createSkill(minutesPerInterval, "skill1", new TimePeriod(8, 0, 8, 15));
			var skill2 = createSkill(minutesPerInterval, "skill2", new TimePeriod(8, 0, 8, 15));
			var skillDay1 = createSkillDay(skill1, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 15));
			var skillDay2 = createSkillDay(skill2, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 15));
			var scheduledStaffingList1 = createScheduledStaffing(skillDay1, latestStatsTime);
			var scheduledStaffingList2 = createScheduledStaffing(skillDay2, latestStatsTime);

			SkillRepository.Has(skill1);
			SkillRepository.Has(skill2);
			SkillDayRepository.Add(skillDay1);
			SkillDayRepository.Add(skillDay2);
			ScheduleForecastSkillReadModelRepository.Persist(scheduledStaffingList1, DateTime.MinValue);
			ScheduleForecastSkillReadModelRepository.Persist(scheduledStaffingList2, DateTime.MinValue);
			IntradayMonitorDataLoader.AddInterval(new IncomingIntervalModel()
			{
				IntervalDate = latestStatsTime.Date,
				IntervalId = new IntervalBase(latestStatsTime, (60 / minutesPerInterval) * 24).Id,
				OfferedCalls = 22
			});

			var result = Target.Load(new Guid[] {skill1.Id.Value, skill2.Id.Value});

			var forecastedCallsSkill1 = skillDay1.WorkloadDayCollection.First().TaskPeriodList.First().Tasks;
			var forecastedCallsSkill2 = skillDay2.WorkloadDayCollection.First().TaskPeriodList.First().Tasks;

			var eslSkill1 = calculateEsl(scheduledStaffingList1, skillDay1, forecastedCallsSkill1);
			var eslSkill2 = calculateEsl(scheduledStaffingList2, skillDay2, forecastedCallsSkill2);
			
			var answeredWithinServiceLevelSkill1 = forecastedCallsSkill1*eslSkill1;
			var answeredWithinServiceLevelSkill2 = forecastedCallsSkill2*eslSkill2;

			var expectedEslSummary = (answeredWithinServiceLevelSkill1 + answeredWithinServiceLevelSkill2)/
											 (forecastedCallsSkill1 + forecastedCallsSkill2);

			result.DataSeries.EstimatedServiceLevels.Length.Should().Be.EqualTo(1);
			Math.Round(result.DataSeries.EstimatedServiceLevels.First().Value, 5).Should().Be.EqualTo(Math.Round(expectedEslSummary, 5));
		}

		[Test]
		public void ShouldReturnEmptyViewModelWhenOnlyUnsupportedSkills()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			fakeScenarioAndIntervalLength();
			var skill = createEmailSkill(minutesPerInterval, "email_skill", new TimePeriod(8, 0, 8, 15));
			
			SkillRepository.Has(skill);

			var result = Target.Load(new Guid[] { skill.Id.Value });

			result.DataSeries.AbandonedRate.Should().Be.Empty();
			result.DataSeries.ServiceLevel.Should().Be.Empty();
			result.DataSeries.AverageSpeedOfAnswer.Should().Be.Empty();
			result.DataSeries.EstimatedServiceLevels.Should().Be.Empty();
		}


		private double calculateEsl(IList<SkillStaffingInterval> scheduledStaffingList, ISkillDay skillDay, double forecastedCallsSkill)
		{
			return _staffingCalculatorService.ServiceLevelAchievedOcc(
				scheduledStaffingList.First().StaffingLevel,
				skillDay.SkillDataPeriodCollection.First().ServiceAgreement.ServiceLevel.Seconds,
				forecastedCallsSkill,
				skillDay.WorkloadDayCollection.First().TaskPeriodList.First().TotalAverageTaskTime.TotalSeconds +
				skillDay.WorkloadDayCollection.First().TaskPeriodList.First().TotalAverageAfterTaskTime.TotalSeconds,
				TimeSpan.FromMinutes(minutesPerInterval),
				skillDay.SkillDataPeriodCollection.First().ServiceAgreement.ServiceLevel.Percent.Value,
				skillDay.SkillStaffPeriodCollection.First().FStaff,
				1);
		}

		private IList<SkillStaffingInterval> createScheduledStaffing(ISkillDay skillDay, DateTime latestStatsTime)
		{
			var scheduledStats = new List<SkillStaffingInterval>();
			var shiftStartTime = skillDay.SkillStaffPeriodCollection.First().Period.StartDateTime;
			var shiftEndTime = skillDay.SkillStaffPeriodCollection.Last().Period.EndDateTime;
			if (shiftStartTime > latestStatsTime || shiftEndTime < latestStatsTime)
			{
				return scheduledStats;
			}
			var random = new Random();

			for (DateTime intervalTime = shiftStartTime;
						 intervalTime <= latestStatsTime;
						 intervalTime = intervalTime.AddMinutes(minutesPerInterval))
			{
				scheduledStats.Add(new SkillStaffingInterval
				{
					SkillId = skillDay.Skill.Id.Value,
					StartDateTime = TimeZoneHelper.ConvertFromUtc(intervalTime, TimeZone.TimeZone()),
					EndDateTime = TimeZoneHelper.ConvertFromUtc(intervalTime, TimeZone.TimeZone()).AddMinutes(minutesPerInterval),
					StaffingLevel = 18 * random.Next(100,130) / 100d
				});
			}
			return scheduledStats;
		}

		private ISkill createSkill(int intervalLength, string skillName, TimePeriod openHours)
		{
			var activity = new Activity("activity_" + skillName).WithId();
			var skill =
				new Skill(skillName, skillName, Color.Empty, intervalLength, new SkillTypePhone(new Description("SkillTypeInboundTelephony"), ForecastSource.InboundTelephony))
				{
					Activity = activity,
					TimeZone = TimeZoneInfo.Utc
				}.WithId();
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, openHours);

			return skill;
		}

		private ISkill createEmailSkill(int intervalLength, string skillName, TimePeriod openHours)
		{
			var activity = new Activity("activity_" + skillName).WithId();
			var skill =
				new Skill(skillName, skillName, Color.Empty, intervalLength, new SkillTypeEmail(new Description("SkillTypeEmail"), ForecastSource.InboundTelephony))
				{
					Activity = activity,
					TimeZone = TimeZoneInfo.Utc
				}.WithId();
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, openHours);

			return skill;
		}

		private ISkillDay createSkillDay(ISkill skill, IScenario scenario, DateTime userNow, TimePeriod openHours)
		{
			var random = new Random();
			var agents = 19 * random.Next(100, 130)/100d;
			var skillDay = skill.CreateSkillDayWithDemandOnInterval(scenario, new DateOnly(userNow), agents, new Tuple<TimePeriod, double>(openHours, agents)).WithId();
			var index = 0;

			for (TimeSpan intervalStart = openHours.StartTime; intervalStart < openHours.EndTime; intervalStart = intervalStart.Add(TimeSpan.FromMinutes(skill.DefaultResolution)))
			{
				skillDay.WorkloadDayCollection.First().TaskPeriodList[index].Tasks = random.Next(195,210);
				skillDay.WorkloadDayCollection.First().TaskPeriodList[index].AverageTaskTime = TimeSpan.FromSeconds(120);
				skillDay.WorkloadDayCollection.First().TaskPeriodList[index].AverageAfterTaskTime = TimeSpan.FromSeconds(140);
				index++;
			}

			return skillDay;
		}

		private Scenario fakeScenarioAndIntervalLength()
		{
			IntervalLengthFetcher.Has(minutesPerInterval);
			var scenario = ScenarioFactory.CreateScenario("scenariorita", true, true).WithId();
			ScenarioRepository.Has(scenario);
			return scenario;
		}
	}
}