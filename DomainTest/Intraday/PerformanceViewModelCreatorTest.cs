using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics.Transformer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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
	public class PerformanceViewModelCreatorTest : ISetup
	{
		public PerformanceViewModelCreator Target;
		public MutableNow Now;
		public FakeUserTimeZone TimeZone;
		public FakeScenarioRepository ScenarioRepository;
		public FakeSkillRepository SkillRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeSkillCombinationResourceRepository SkillCombinationResourceRepository;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;
		const int minutesPerInterval = 15;
		public FakeIntradayMonitorDataLoader IntradayMonitorDataLoader;
		private StaffingCalculatorServiceFacade _staffingCalculatorService;

		private readonly ServiceAgreement _slaTwoHours =
			new ServiceAgreement(new ServiceLevel(new Percent(1), 7200), new Percent(0), new Percent(1));

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

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 15), false);
			var skillDay = createSkillDay(skill, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 15), false);

			var scheduledStaffingList = createScheduledStaffing(skillDay);

			createStatistics(latestStatsTime, latestStatsTime.AddMinutes(minutesPerInterval), latestStatsTime);

			SkillRepository.Has(skill);
			SkillDayRepository.Add(skillDay);
			SkillCombinationResourceRepository.PersistSkillCombinationResource(DateTime.MinValue,scheduledStaffingList);

			var result = Target.Load(new Guid[] { skill.Id.Value });

			var esl = calculateEsl(scheduledStaffingList, skillDay, skillDay.WorkloadDayCollection.First().TaskPeriodList.First().Tasks, 0);

			result.DataSeries.EstimatedServiceLevels.Length.Should().Be.EqualTo(1);
			Math.Round(result.DataSeries.EstimatedServiceLevels.First().Value, 5).Should().Be.EqualTo(Math.Round(esl * 100, 5));
		}

		[Test]
		[Toggle(Toggles.Wfm_Intraday_SupportSkillTypeEmail_44002)]
		public void ShouldReturnEslFromStartOfOpenHour()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			var latestStatsTime = new DateTime(2016, 8, 26, 7, 0, 0, DateTimeKind.Utc);


			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);
			var skill = StaffingViewModelCreatorTestHelper.CreateEmailSkill(15, "skill", new TimePeriod(8, 0, 9, 0));

			var skillDay = StaffingViewModelCreatorTestHelper.CreateSkillDay(skill, scenario, userNow, new TimePeriod(8, 0, 9, 0), false, _slaTwoHours, false);

			var scheduledStaffingList = createScheduledStaffing(skillDay);

			createStatistics(latestStatsTime, latestStatsTime.AddMinutes(5*minutesPerInterval), latestStatsTime.AddMinutes(5 * minutesPerInterval));

			SkillRepository.Has(skill);
			SkillDayRepository.Add(skillDay);
			SkillCombinationResourceRepository.PersistSkillCombinationResource(DateTime.MinValue, scheduledStaffingList);

			var result = Target.Load(new Guid[] { skill.Id.Value });

			var esl = calculateEsl(scheduledStaffingList, skillDay, skillDay.WorkloadDayCollection.First().TaskPeriodList.First().Tasks, 0);

			result.DataSeries.EstimatedServiceLevels.Length.Should().Be.EqualTo(5);
			result.DataSeries.EstimatedServiceLevels.First().Should().Be.EqualTo(null);
			result.DataSeries.EstimatedServiceLevels[3].Should().Be.EqualTo(null);
			Math.Round(result.DataSeries.EstimatedServiceLevels.Last().Value, 5).Should().Be.EqualTo(Math.Round(esl * 100, 5));
		}

		[Test]
		public void ShouldReturnEslForWhenSkillIsClosedTomorrow()
		{
			var userNowFriday = new DateTime(2017, 1, 13, 8, 0, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNowFriday, TimeZone.TimeZone()));
			var latestStatsTimeFriday = new DateTime(2017, 1, 13, 8, 0, 0, DateTimeKind.Utc);

			var skillClosedOnWeekends = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 15), true);
			var skillDayFriday = createSkillDay(skillClosedOnWeekends, userNowFriday, new TimePeriod(8, 0, 8, 15), false);
			var skillDaySaturday = createSkillDay(skillClosedOnWeekends, userNowFriday.AddDays(1), new TimePeriod(), false);

			var scheduledStaffingList = createScheduledStaffing(skillDayFriday);

			createStatistics(latestStatsTimeFriday, latestStatsTimeFriday.AddMinutes(minutesPerInterval), latestStatsTimeFriday);

			SkillRepository.Has(skillClosedOnWeekends);
			SkillDayRepository.Add(skillDayFriday);
			SkillDayRepository.Add(skillDaySaturday);
			SkillCombinationResourceRepository.PersistSkillCombinationResource(DateTime.MinValue, scheduledStaffingList);

			var result = Target.Load(new Guid[] { skillClosedOnWeekends.Id.Value });

			var esl = calculateEsl(scheduledStaffingList, skillDayFriday, skillDayFriday.WorkloadDayCollection.First().TaskPeriodList.First().Tasks, 0);

			result.DataSeries.EstimatedServiceLevels.Length.Should().Be.EqualTo(1);
			Math.Round(result.DataSeries.EstimatedServiceLevels.First().Value, 5).Should().Be.EqualTo(Math.Round(esl * 100, 5));
		}

		[Test]
		public void ShouldHandleMergedForecastIntervals()
		{

			var userNow = new DateTime(2016, 8, 26, 8, 30, 0, DateTimeKind.Utc);
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false);
			var skillDay = createSkillDay(skill, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false);

			skillDay.WorkloadDayCollection.First().MergeTemplateTaskPeriods(skillDay.WorkloadDayCollection.First().TaskPeriodList);

			SkillRepository.Has(skill);
			SkillDayRepository.Has(skillDay);

			var scheduledStaffingList = createScheduledStaffing(skillDay);
			createStatistics(latestStatsTime.AddMinutes(-minutesPerInterval), userNow, latestStatsTime);
			SkillCombinationResourceRepository.PersistSkillCombinationResource(DateTime.MinValue, scheduledStaffingList);

			var result = Target.Load(new Guid[] { skill.Id.Value });

			var esl = calculateEsl(scheduledStaffingList, skillDay, skillDay.WorkloadDayCollection.First().TaskPeriodList.First().Tasks / 2, 0);

			result.DataSeries.EstimatedServiceLevels.Length.Should().Be.EqualTo(2);
			Math.Round(result.DataSeries.EstimatedServiceLevels.First().Value, 5).Should().Be.EqualTo(Math.Round(esl * 100, 5));
		}

		[Test]
		public void ShouldReturnEslInCorrectOrder()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);

			var skill1 = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false);
			var skillDay1 = createSkillDay(skill1, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false);
			var skill2 = createSkill(minutesPerInterval, "skill", new TimePeriod(7, 45, 8, 30), false);
			var skillDay2 = createSkillDay(skill2, Now.UtcDateTime(), new TimePeriod(7, 45, 8, 30), false);

			var scheduledStaffingList = new List<SkillCombinationResource>
			{
				new SkillCombinationResource {
					SkillCombination = new [] {skill1.Id.Value},
					StartDateTime = userNow,
					EndDateTime = userNow.AddMinutes(minutesPerInterval),
					Resource = 19
				},
				new SkillCombinationResource {
					SkillCombination =  new [] {skill1.Id.Value},
					StartDateTime = userNow.AddMinutes(minutesPerInterval),
					EndDateTime = userNow.AddMinutes(2*minutesPerInterval),
					Resource = 18
				},
				new SkillCombinationResource {
					SkillCombination =  new [] {skill2.Id.Value},
					StartDateTime = userNow.AddMinutes(-minutesPerInterval),
					EndDateTime = userNow,
					Resource = 20
				},
				new SkillCombinationResource {
					SkillCombination =  new [] {skill2.Id.Value},
					StartDateTime = userNow,
					EndDateTime = userNow.AddMinutes(minutesPerInterval),
					Resource = 21
				}
			};

			SkillRepository.Has(skill1, skill2);
			SkillDayRepository.Has(skillDay1, skillDay2);
			SkillCombinationResourceRepository.PersistSkillCombinationResource(DateTime.MinValue, scheduledStaffingList);

			createStatistics(userNow.AddMinutes(-minutesPerInterval), userNow.AddMinutes(minutesPerInterval), latestStatsTime);

			var result = Target.Load(new[] { skill1.Id.Value, skill2.Id.Value });

			var forecastedCallsSkill1 = skillDay1.WorkloadDayCollection.First().TaskPeriodList.First().Tasks;
			var forecastedCallsSkill2 = skillDay2.WorkloadDayCollection.First().TaskPeriodList[1].Tasks;

			var eslSkill1First = _staffingCalculatorService.ServiceLevelAchievedOcc(
				scheduledStaffingList[0].Resource,
				skillDay1.SkillDataPeriodCollection.First().ServiceAgreement.ServiceLevel.Seconds,
				forecastedCallsSkill1,
				skillDay1.WorkloadDayCollection.First().TaskPeriodList[0].TotalAverageTaskTime.TotalSeconds +
				skillDay1.WorkloadDayCollection.First().TaskPeriodList[0].TotalAverageAfterTaskTime.TotalSeconds,
				TimeSpan.FromMinutes(minutesPerInterval),
				skillDay1.SkillDataPeriodCollection.First().ServiceAgreement.ServiceLevel.Percent.Value,
				skillDay1.SkillStaffPeriodCollection[0].FStaff,
				1);
			var eslSkill2Second = _staffingCalculatorService.ServiceLevelAchievedOcc(
				scheduledStaffingList[3].Resource,
				skillDay2.SkillDataPeriodCollection.First().ServiceAgreement.ServiceLevel.Seconds,
				forecastedCallsSkill2,
				skillDay2.WorkloadDayCollection.First().TaskPeriodList[1].TotalAverageTaskTime.TotalSeconds +
				skillDay2.WorkloadDayCollection.First().TaskPeriodList[1].TotalAverageAfterTaskTime.TotalSeconds,
				TimeSpan.FromMinutes(minutesPerInterval),
				skillDay2.SkillDataPeriodCollection.First().ServiceAgreement.ServiceLevel.Percent.Value,
				skillDay2.SkillStaffPeriodCollection[1].FStaff,
				1);

			var answeredWithinServiceLevelSkill1 = forecastedCallsSkill1 * eslSkill1First;
			var answeredWithinServiceLevelSkill2 = forecastedCallsSkill2 * eslSkill2Second;

			var expectedEslSummary = (answeredWithinServiceLevelSkill1 + answeredWithinServiceLevelSkill2) /
											 (forecastedCallsSkill1 + forecastedCallsSkill2);

			result.DataSeries.EstimatedServiceLevels.Length.Should().Be.EqualTo(2);
			Math.Round(result.DataSeries.EstimatedServiceLevels[1].Value, 5).Should().Be.EqualTo(Math.Round(expectedEslSummary * 100, 5));
		}

		[Test]
		public void ShouldReturnEslForNewZealandTimezone()
		{
			TimeZone.IsNewZealand();
			var userNow = new DateTime(2016, 8, 26, 6, 0, 0, DateTimeKind.Local);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			var latestStatsTimeLocal = new DateTime(2016, 8, 26, 5, 45, 0, DateTimeKind.Local);

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(0, 0, 24, 0), false);
			createSkillDaysYesterdayTodayTomorrow(skill, userNow);

			createStatistics(latestStatsTimeLocal.Date, latestStatsTimeLocal.Date.AddDays(1), latestStatsTimeLocal);

			SkillRepository.Has(skill);

			var result = Target.Load(new Guid[] { skill.Id.Value });

			result.DataSeries.Time.Length.Should().Be.EqualTo(96);
			result.DataSeries.EstimatedServiceLevels.Length.Should().Be.EqualTo(96);

			var latestStatsIntervalPosition = new IntervalBase(latestStatsTimeLocal, (60 / minutesPerInterval) * 24).Id;
			result.DataSeries.EstimatedServiceLevels.First().Should().Be.GreaterThan(0);
			result.DataSeries.EstimatedServiceLevels[latestStatsIntervalPosition].Should().Be.GreaterThan(0);
			result.DataSeries.EstimatedServiceLevels[latestStatsIntervalPosition + 1].Should().Be.EqualTo(null);
		}

		[Test]
		public void ShouldReturnEslForHawaiiTimezone()
		{
			TimeZone.IsHawaii();
			var userNow = new DateTime(2016, 8, 26, 20, 0, 0, DateTimeKind.Local);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			var latestStatsTimeLocal = new DateTime(2016, 8, 26, 19, 45, 0, DateTimeKind.Local);

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(0, 0, 24, 0), false);
			createSkillDaysYesterdayTodayTomorrow(skill, userNow);

			createStatistics(latestStatsTimeLocal.Date, latestStatsTimeLocal.Date.AddDays(1), latestStatsTimeLocal);

			SkillRepository.Has(skill);

			var result = Target.Load(new Guid[] { skill.Id.Value });

			result.DataSeries.Time.Length.Should().Be.EqualTo(96);
			result.DataSeries.EstimatedServiceLevels.Length.Should().Be.EqualTo(96);

			var latestStatsIntervalPosition = new IntervalBase(latestStatsTimeLocal, (60 / minutesPerInterval) * 24).Id;
			result.DataSeries.EstimatedServiceLevels.First().Should().Be.GreaterThan(0);
			result.DataSeries.EstimatedServiceLevels[latestStatsIntervalPosition].Should().Be.GreaterThan(0);
			result.DataSeries.EstimatedServiceLevels[latestStatsIntervalPosition + 1].Should().Be.EqualTo(null);
		}

		[Test]
		public void ShouldReturnEslDaySummary()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 30, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false);
			var skillDay = createSkillDay(skill, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false);

			var scheduledStaffingList = createScheduledStaffing(skillDay);

			createStatistics(latestStatsTime.AddMinutes(-minutesPerInterval), latestStatsTime.AddMinutes(minutesPerInterval), latestStatsTime);

			SkillRepository.Has(skill);
			SkillDayRepository.Add(skillDay);
			SkillCombinationResourceRepository.PersistSkillCombinationResource(DateTime.MinValue, scheduledStaffingList);

			var result = Target.Load(new Guid[] { skill.Id.Value });

			var forecastedCallsInterval1 = skillDay.WorkloadDayCollection.First().TaskPeriodList.First().Tasks;
			var eslInterval1 = calculateEsl(scheduledStaffingList, skillDay, forecastedCallsInterval1, 0);
			var answeredCallsWithinSlInterval1 = forecastedCallsInterval1 * eslInterval1;

			var forecastedCallsInterval2 = skillDay.WorkloadDayCollection.First().TaskPeriodList.Last().Tasks;
			var eslInterval2 = calculateEsl(scheduledStaffingList, skillDay, forecastedCallsInterval2, 1);
			var answeredCallsWithinSlInterval2 = forecastedCallsInterval2 * eslInterval2;

			var expectedEslSummary = (answeredCallsWithinSlInterval1 + answeredCallsWithinSlInterval2) /
											 (forecastedCallsInterval1 + forecastedCallsInterval2);

			result.DataSeries.EstimatedServiceLevels.Length.Should().Be.EqualTo(2);
			Math.Round(result.Summary.EstimatedServiceLevel, 5).Should().Be.EqualTo(Math.Round(expectedEslSummary * 100, 5));
		}

		[Test]
		public void ShouldReturnStatisticsForOneSkill()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);

			fakeScenarioAndIntervalLength();
			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 15), false);

			IntradayMonitorDataLoader.AddInterval(new IncomingIntervalModel()
			{
				IntervalDate = latestStatsTime.Date,
				IntervalId = new IntervalBase(latestStatsTime, (60 / minutesPerInterval) * 24).Id,
				CalculatedCalls = 20,
				AnsweredCalls = 16,
				AnsweredCallsWithinSL = 16,
				SpeedOfAnswer = 10,
				AbandonedCalls = 4,
				AbandonedRate = 0.2d,
				ServiceLevel = 0.8d
			});

			SkillRepository.Has(skill);

			var result = Target.Load(new Guid[] { skill.Id.Value });

			result.DataSeries.Time.Length.Should().Be.EqualTo(1);
			result.DataSeries.AverageSpeedOfAnswer.Length.Should().Be.EqualTo(1);
			result.DataSeries.AbandonedRate.Length.Should().Be.EqualTo(1);
			result.DataSeries.ServiceLevel.Length.Should().Be.EqualTo(1);
			result.Summary.ServiceLevel.Should().Be.GreaterThan(0);
			result.Summary.AverageSpeedOfAnswer.Should().Be.GreaterThan(0);
			result.Summary.AbandonRate.Should().Be.GreaterThan(0);
			result.LatestActualIntervalStart.Should().Be.EqualTo(latestStatsTime);
			result.LatestActualIntervalEnd.Should().Be.EqualTo(latestStatsTime.AddMinutes(minutesPerInterval));
			result.PerformanceHasData.Should().Be.EqualTo(true);
		}

		[Test]
		public void ShouldReturnEslUpUntilLatestStatsTime()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false);
			var skillDay = createSkillDay(skill, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false);

			var scheduledStaffingList = createScheduledStaffing(skillDay);

			IntradayMonitorDataLoader.AddInterval(new IncomingIntervalModel()
			{
				IntervalDate = latestStatsTime.Date,
				IntervalId = new IntervalBase(latestStatsTime, (60 / minutesPerInterval) * 24).Id,
				CalculatedCalls = 22,
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
			SkillCombinationResourceRepository.PersistSkillCombinationResource(DateTime.MinValue, scheduledStaffingList);

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

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 15), false);
			var skillDay = createSkillDay(skill, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 15), false);

			createStatistics(latestStatsTime, latestStatsTime.AddMinutes(minutesPerInterval), latestStatsTime);

			SkillRepository.Has(skill);
			SkillDayRepository.Add(skillDay);

			var result = Target.Load(new Guid[] { skill.Id.Value });

			result.DataSeries.EstimatedServiceLevels.Length.Should().Be.EqualTo(1);
			result.DataSeries.EstimatedServiceLevels.First().Should().Be.EqualTo(0d);
			result.Summary.EstimatedServiceLevel.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldHandleEslSummaryWithZeroForecast()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			fakeScenarioAndIntervalLength();
			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 15), false);
			var scenario = fakeScenarioAndIntervalLength();
			var skillDay = skill.CreateSkillDayWithDemandOnInterval(scenario, new DateOnly(userNow), 0, new Tuple<TimePeriod, double>(new TimePeriod(8, 0, 8, 15), 0)).WithId();


			IntradayMonitorDataLoader.AddInterval(new IncomingIntervalModel()
			{
				IntervalDate = userNow.Date,
				IntervalId = new IntervalBase(userNow, (60 / minutesPerInterval) * 24).Id,
				ForecastedCalls = 0,
				CalculatedCalls = 1
			});

			SkillRepository.Has(skill);
			SkillDayRepository.Has(skillDay);

			var result = Target.Load(new Guid[] { skill.Id.Value });

			result.DataSeries.EstimatedServiceLevels.Length.Should().Be.EqualTo(1);
			result.DataSeries.EstimatedServiceLevels.First().Should().Be.EqualTo(null);
			result.Summary.EstimatedServiceLevel.Should().Be.EqualTo(0);
		}


		[Test]
		public void ShouldReturnNoEslWhenNoForecast()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			fakeScenarioAndIntervalLength();
			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 15), false);
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

			var skill1 = createSkill(minutesPerInterval, "skill1", new TimePeriod(8, 0, 8, 15), false);
			var skill2 = createSkill(minutesPerInterval, "skill2", new TimePeriod(8, 0, 8, 15), false);
			var skillDay1 = createSkillDay(skill1, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 15), false);
			var skillDay2 = createSkillDay(skill2, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 15), false);
			var scheduledStaffingList = new List<SkillCombinationResource>();
			var scheduledStaffingList1 = createScheduledStaffing(skillDay1);
			var scheduledStaffingList2 = createScheduledStaffing(skillDay2);
			scheduledStaffingList.AddRange(scheduledStaffingList1);
			scheduledStaffingList.AddRange(scheduledStaffingList2);

			SkillRepository.Has(skill1);
			SkillRepository.Has(skill2);
			SkillDayRepository.Add(skillDay1);
			SkillDayRepository.Add(skillDay2);
			SkillCombinationResourceRepository.PersistSkillCombinationResource(DateTime.MinValue, scheduledStaffingList);
			createStatistics(latestStatsTime, latestStatsTime.AddMinutes(minutesPerInterval), latestStatsTime);

			var result = Target.Load(new Guid[] { skill1.Id.Value, skill2.Id.Value });

			var forecastedCallsSkill1 = skillDay1.WorkloadDayCollection.First().TaskPeriodList.First().Tasks;
			var forecastedCallsSkill2 = skillDay2.WorkloadDayCollection.First().TaskPeriodList.First().Tasks;

			var eslSkill1 = calculateEsl(scheduledStaffingList1, skillDay1, forecastedCallsSkill1, 0);
			var eslSkill2 = calculateEsl(scheduledStaffingList2, skillDay2, forecastedCallsSkill2, 0);

			var answeredWithinServiceLevelSkill1 = forecastedCallsSkill1 * eslSkill1;
			var answeredWithinServiceLevelSkill2 = forecastedCallsSkill2 * eslSkill2;

			var expectedEslSummary = (answeredWithinServiceLevelSkill1 + answeredWithinServiceLevelSkill2) /
											 (forecastedCallsSkill1 + forecastedCallsSkill2);

			result.DataSeries.EstimatedServiceLevels.Length.Should().Be.EqualTo(1);
			Math.Round(result.DataSeries.EstimatedServiceLevels.First().Value, 5).Should().Be.EqualTo(Math.Round(expectedEslSummary * 100, 5));
			Math.Round(result.Summary.EstimatedServiceLevel, 5).Should().Be.EqualTo(Math.Round(expectedEslSummary * 100, 5));

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
			result.PerformanceHasData.Should().Be.EqualTo(false);
		}

		[Test]
		public void ShouldReturnEslWhenSkillDataPeriodsHaveDuplicates()
		{
			var userNow = new DateTime(2016, 8, 26, 0, 30, 0, DateTimeKind.Utc);
			var latestStatsTime = new DateTime(2016, 8, 26, 0, 0, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var scenario = fakeScenarioAndIntervalLength();
			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(0, 0, 0, 30), false);
			var skillDay = createSkillDay(skill, userNow, new TimePeriod(0, 0, 0, 30), true);

			var scheduledStaffingList = createScheduledStaffing(skillDay);

			IntradayMonitorDataLoader.AddInterval(new IncomingIntervalModel()
			{
				IntervalDate = latestStatsTime.Date,
				IntervalId = new IntervalBase(latestStatsTime, (60 / minutesPerInterval) * 24).Id,
				CalculatedCalls = 22,
				ForecastedCalls = 20
			});

			SkillRepository.Has(skill);
			SkillDayRepository.Add(skillDay);
			SkillCombinationResourceRepository.PersistSkillCombinationResource(DateTime.MinValue, scheduledStaffingList);

			var result = Target.Load(new Guid[] { skill.Id.Value });

			result.DataSeries.EstimatedServiceLevels.Length.Should().Be.EqualTo(1);
			result.DataSeries.EstimatedServiceLevels.First().Should().Be.GreaterThan(0d);
		}
		
		[Test]
		public void ShouldReturnPerformanceDataForSpecifiedDate()
		{
			DateTime testDate = new DateTime(2016, 8, 26, 0, 30, 0, DateTimeKind.Utc);

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(0, 0, 0, 30), false);
			var latestStatsTime = new DateTime(2016, 8, 26, 0, 0, 0, DateTimeKind.Utc);
			var skillDay = createSkillDay(skill, testDate, new TimePeriod(0, 0, 0, 30), true);

			var scheduledStaffingList = createScheduledStaffing(skillDay);

			IntradayMonitorDataLoader.ShouldCompareDate = true;
			IntradayMonitorDataLoader.AddInterval(new IncomingIntervalModel()
			{
				IntervalDate = latestStatsTime.Date,
				IntervalId = new IntervalBase(latestStatsTime, (60 / minutesPerInterval) * 24).Id,
				CalculatedCalls = 22,
				ForecastedCalls = 20
			});

			SkillRepository.Has(skill);
			SkillDayRepository.Add(skillDay);
			SkillCombinationResourceRepository.PersistSkillCombinationResource(DateTime.MinValue, scheduledStaffingList);

			var result = Target.Load(new Guid[] { skill.Id.Value }, testDate);

			result.Should().Not.Be.Null();
			result.LatestActualIntervalStart.Should().Have.Value();
			result.LatestActualIntervalStart.Should().Be.EqualTo(latestStatsTime);
			
		}

		[Test]
		public void ShouldNotReturnPerformanceDataForSpecifiedDate()
		{
			DateTime testDate = new DateTime(2016, 8, 26, 0, 30, 0, DateTimeKind.Utc);

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(0, 0, 0, 30), false);
			var latestStatsTime = new DateTime(2016, 8, 26, 0, 0, 0, DateTimeKind.Utc);
			var skillDay = createSkillDay(skill, testDate, new TimePeriod(0, 0, 0, 30), true);

			var scheduledStaffingList = createScheduledStaffing(skillDay);

			IntradayMonitorDataLoader.ShouldCompareDate = true;
			IntradayMonitorDataLoader.AddInterval(new IncomingIntervalModel()
			{
				IntervalDate = latestStatsTime.Date,
				IntervalId = new IntervalBase(latestStatsTime, (60 / minutesPerInterval) * 24).Id,
				CalculatedCalls = 22,
				ForecastedCalls = 20
			});

			SkillRepository.Has(skill);
			SkillDayRepository.Add(skillDay);
			SkillCombinationResourceRepository.PersistSkillCombinationResource(DateTime.MinValue, scheduledStaffingList);

			var result = Target.Load(new Guid[] { skill.Id.Value }, testDate.AddDays(+1));

			result.LatestActualIntervalStart.Should().Not.Have.Value();
		}

		[Test]
		public void ShouldReturnPerformanceDataForSpecifiedDayOffset()
		{
			Now.Is(new DateTime(2016, 8, 26, 0, 30, 0, DateTimeKind.Utc));

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(0, 0, 0, 30), false);
			var latestStatsTime = new DateTime(2016, 8, 27, 0, 0, 0, DateTimeKind.Utc);
			var skillDay = createSkillDay(skill, Now.UtcDateTime().AddDays(1), new TimePeriod(0, 0, 0, 30), true);

			var scheduledStaffingList = createScheduledStaffing(skillDay);

			IntradayMonitorDataLoader.ShouldCompareDate = true;
			IntradayMonitorDataLoader.AddInterval(new IncomingIntervalModel()
			{
				IntervalDate = latestStatsTime.Date,
				IntervalId = new IntervalBase(latestStatsTime, (60 / minutesPerInterval) * 24).Id,
				CalculatedCalls = 22,
				ForecastedCalls = 20
			});

			SkillRepository.Has(skill);
			SkillDayRepository.Add(skillDay);

			SkillCombinationResourceRepository.PersistSkillCombinationResource(DateTime.MinValue, scheduledStaffingList);

			var result = Target.Load(new Guid[] { skill.Id.Value }, 1);

			result.Should().Not.Be.Null();
			result.LatestActualIntervalStart.Should().Have.Value();
			result.LatestActualIntervalStart.Should().Be.EqualTo(latestStatsTime);

		}

		private double calculateEsl(IList<SkillCombinationResource> scheduledStaffingList, ISkillDay skillDay, double forecastedCallsSkill, int intervalPosition)
		{
			return _staffingCalculatorService.ServiceLevelAchievedOcc(
				scheduledStaffingList[intervalPosition].Resource,
				skillDay.SkillDataPeriodCollection.First().ServiceAgreement.ServiceLevel.Seconds,
				forecastedCallsSkill,
				skillDay.WorkloadDayCollection.First().TaskPeriodList[intervalPosition].TotalAverageTaskTime.TotalSeconds +
				skillDay.WorkloadDayCollection.First().TaskPeriodList[intervalPosition].TotalAverageAfterTaskTime.TotalSeconds,
				TimeSpan.FromMinutes(minutesPerInterval),
				skillDay.SkillDataPeriodCollection.First().ServiceAgreement.ServiceLevel.Percent.Value,
				skillDay.SkillStaffPeriodCollection[intervalPosition].FStaff,
				1);
		}

		private IList<SkillCombinationResource> createScheduledStaffing(ISkillDay skillDay)
		{
			var scheduledStats = new List<SkillCombinationResource>();
			var shiftStartTime = skillDay.SkillStaffPeriodCollection.First().Period.StartDateTime;
			var shiftEndTime = skillDay.SkillStaffPeriodCollection.Last().Period.EndDateTime;

			var random = new Random();

			for (DateTime intervalTime = shiftStartTime;
						 intervalTime < shiftEndTime;
						 intervalTime = intervalTime.AddMinutes(minutesPerInterval))
			{
				scheduledStats.Add(new SkillCombinationResource
				{
					SkillCombination = new [] {skillDay.Skill.Id.GetValueOrDefault()},
					StartDateTime = intervalTime,
					EndDateTime = intervalTime.AddMinutes(minutesPerInterval),
					Resource = 18 * random.Next(100, 110) / 100d
				});
			}
			return scheduledStats;
		}

		private ISkill createSkill(int intervalLength, string skillName, TimePeriod openHours, bool isClosedOnWeekends)
		{
			var activity = new Activity("activity_" + skillName).WithId();
			var skill =
				new Skill(skillName, skillName, Color.Empty, intervalLength, new SkillTypePhone(new Description("SkillTypeInboundTelephony"), ForecastSource.InboundTelephony))
				{
					Activity = activity,
					TimeZone = TimeZoneInfo.Utc
				}.WithId();
			if (isClosedOnWeekends)
				WorkloadFactory.CreateWorkloadClosedOnWeekendsWithOpenHours(skill, openHours);
			else
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

		private ISkillDay createSkillDay(ISkill skill, DateTime userNow, TimePeriod openHours, bool addSkillDataPeriodDuplicate)
		{
			var scenario = fakeScenarioAndIntervalLength();
			var random = new Random();
			var agents = 19 * random.Next(100, 110) / 100d;

			ISkillDay skillDay;
			if (addSkillDataPeriodDuplicate)
				skillDay =
					skill.CreateSkillDayWithDemandOnIntervalWithSkillDataPeriodDuplicate(scenario, new DateOnly(userNow), 3,
						new Tuple<TimePeriod, double>(openHours, 3)).WithId();
			else
				skillDay =
					skill.CreateSkillDayWithDemandOnInterval(scenario, new DateOnly(userNow), agents, new Tuple<TimePeriod, double>(openHours, 3)).WithId();

			var index = 0;

			var workloadDay = skillDay.WorkloadDayCollection.First();
			skillDay.Lock();
			for (TimeSpan intervalStart = openHours.StartTime; intervalStart < openHours.EndTime; intervalStart = intervalStart.Add(TimeSpan.FromMinutes(skill.DefaultResolution)))
			{
				workloadDay.TaskPeriodList[index].Tasks = random.Next(195, 210);
				workloadDay.TaskPeriodList[index].AverageTaskTime = TimeSpan.FromSeconds(120);
				workloadDay.TaskPeriodList[index].AverageAfterTaskTime = TimeSpan.FromSeconds(140);
				index++;
			}
			skillDay.Release();
			
			return skillDay;
		}

		private void createSkillDaysYesterdayTodayTomorrow(ISkill skill, DateTime userNow)
		{
			var skillDayYesterday = createSkillDay(skill, userNow.AddDays(-1), new TimePeriod(0, 0, 24, 0), false);
			var skillDayToday = createSkillDay(skill, userNow, new TimePeriod(0, 0, 24, 0), false);
			var skillDayTomorrow = createSkillDay(skill, userNow.AddDays(1), new TimePeriod(0, 0, 24, 0), false);

			SkillDayRepository.Add(skillDayYesterday);
			SkillDayRepository.Add(skillDayToday);
			SkillDayRepository.Add(skillDayTomorrow);

			var scheduledStaffingList = new List<SkillCombinationResource>();
			scheduledStaffingList.AddRange(createScheduledStaffing(skillDayYesterday));
			scheduledStaffingList.AddRange(createScheduledStaffing(skillDayToday));
			scheduledStaffingList.AddRange(createScheduledStaffing(skillDayTomorrow));

			SkillCombinationResourceRepository.PersistSkillCombinationResource(DateTime.MinValue, scheduledStaffingList);
		}

		private Scenario fakeScenarioAndIntervalLength()
		{
			IntervalLengthFetcher.Has(minutesPerInterval);
			var scenario = ScenarioRepository.LoadAll().SingleOrDefault();
			if (scenario == null)
			{
				scenario = ScenarioFactory.CreateScenario("scenariorita", true, true).WithId();
				ScenarioRepository.Has(scenario);
			}

			return (Scenario)scenario;
		}

		private void createStatistics(DateTime startTime, DateTime endTime, DateTime latestStatsTime)
		{
			for (DateTime currentTime = startTime; currentTime < endTime; currentTime = currentTime.AddMinutes(minutesPerInterval))
			{
				var shouldHaveStats = currentTime <= latestStatsTime;
				IntradayMonitorDataLoader.AddInterval(new IncomingIntervalModel()
				{
					IntervalDate = currentTime.Date,
					IntervalId = new IntervalBase(currentTime, (60 / minutesPerInterval) * 24).Id,
					CalculatedCalls = shouldHaveStats ? (double?)22d : null,
					ForecastedCalls = 20
				});
			}
		}
	}
}