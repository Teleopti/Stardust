using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Intraday
{
	[DomainTest]
	[Toggle(Toggles.Staffing_ReadModel_UseSkillCombination_xx)]
	public class StaffingViewModelCreatorTest_useSkillCombinationsTolleOn : ISetup
	{
		public IStaffingViewModelCreator Target;
		public FakeScenarioRepository ScenarioRepository;
		public FakeSkillRepository SkillRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;
		public FakeIntradayQueueStatisticsLoader IntradayQueueStatisticsLoader;
		public FakeScheduleForecastSkillReadModelRepository ScheduleForecastSkillReadModelRepository;
		public FakeSkillCombinationResourceRepository SkillCombinationResourceRepository;
		public FakeActivityRepository ActivityRepository;
		public MutableNow Now;
		public FakeUserTimeZone TimeZone;

		private const int minutesPerInterval = 15;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble(new FakeUserTimeZone(TimeZoneInfo.Utc)).For<IUserTimeZone>();
		}

		[Test]
		public void ShouldReturnForecastedStaffing()
		{
			TimeZone.IsSweden();
			var scenario = fakeScenarioAndIntervalLength();
			var act = ActivityRepository.Has("act");
			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, act);
			SkillRepository.Has(skill);

			var skillDay = createSkillDay(skill, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false);
			SkillDayRepository.Has(skillDay);

			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() });

			var staffingIntervals = skillDay.SkillStaffPeriodViewCollection(TimeSpan.FromMinutes(minutesPerInterval));
			vm.DataSeries.Time.Length.Should().Be.EqualTo(2);
			vm.DataSeries.Time.First().Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(staffingIntervals.First().Period.StartDateTime, TimeZone.TimeZone()));
			vm.DataSeries.Time.Last().Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(staffingIntervals.Last().Period.StartDateTime, TimeZone.TimeZone()));
			vm.DataSeries.ForecastedStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.ForecastedStaffing.First().Should().Be.GreaterThan(0d);
			vm.DataSeries.ForecastedStaffing.Last().Should().Be.GreaterThan(0d);
			vm.StaffingHasData.Should().Be.EqualTo(true);
		}

		[Test]
		public void ShouldReturnUpdatedStaffingForecastFromNowUntilEndOfOpenHour()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var scenario = fakeScenarioAndIntervalLength();
			var act = ActivityRepository.Has("act");
			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, act);
			var skillDay = createSkillDay(skill, scenario, userNow, new TimePeriod(8, 0, 8, 30), false);
			var actualCalls = createStatistics(skillDay, latestStatsTime);

			SkillRepository.Has(skill);
			SkillDayRepository.Has(skillDay);
			IntradayQueueStatisticsLoader.Has(actualCalls);

			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() });

			var deviationWithReforecast = calculateAverageDeviation(actualCalls, skillDay);

			vm.DataSeries.UpdatedForecastedStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.UpdatedForecastedStaffing.First().Should().Be.EqualTo(null);
			var expectedUpdatedForecastOnLastInterval = vm.DataSeries.ForecastedStaffing.Last() * deviationWithReforecast;
			vm.DataSeries.UpdatedForecastedStaffing.Last().Should().Be.EqualTo(expectedUpdatedForecastOnLastInterval);
		}

		[Test]
		public void ShouldReturnUpdatedStaffingForecastWhenSkillIsClosedTomorrow()
		{
			var userNowFriday = new DateTime(2017, 1, 13, 8, 15, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNowFriday, TimeZone.TimeZone()));
			var latestStatsTimeFriday = new DateTime(2017, 1, 13, 8, 0, 0, DateTimeKind.Utc);

			var scenario = fakeScenarioAndIntervalLength();
			var act = ActivityRepository.Has("act");
			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), true, act);
			var skillDayFriday = createSkillDay(skill, scenario, userNowFriday, new TimePeriod(8, 0, 8, 30), false);
			var skillDaySaturday = createSkillDay(skill, scenario, userNowFriday.AddDays(1), new TimePeriod(), false);
			var actualCalls = createStatistics(skillDayFriday, latestStatsTimeFriday);

			SkillRepository.Has(skill);
			SkillDayRepository.Has(skillDayFriday);
			SkillDayRepository.Has(skillDaySaturday);
			IntradayQueueStatisticsLoader.Has(actualCalls);

			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() });

			var deviationWithReforecast = calculateAverageDeviation(actualCalls, skillDayFriday);

			vm.DataSeries.UpdatedForecastedStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.UpdatedForecastedStaffing.First().Should().Be.EqualTo(null);
			var expectedUpdatedForecastOnLastInterval = vm.DataSeries.ForecastedStaffing.Last() * deviationWithReforecast;
			vm.DataSeries.UpdatedForecastedStaffing.Last().Should().Be.EqualTo(expectedUpdatedForecastOnLastInterval);
		}

		[Test]
		public void ShouldReturnHigherUpdatedStaffingWithNegativeCampaign()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var scenario = fakeScenarioAndIntervalLength();
			var act = ActivityRepository.Has("act");
			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, act);
			var skillDay = createSkillDay(skill, scenario, userNow, new TimePeriod(8, 0, 8, 30), false);
			skillDay.WorkloadDayCollection.First().CampaignTasks = new Percent(-0.5);
			var actualCalls = new List<SkillIntervalStatistics>()
				{
					 new SkillIntervalStatistics
					 {
						  SkillId = skill.Id.GetValueOrDefault(),
						  StartTime = TimeZoneHelper.ConvertFromUtc(latestStatsTime, TimeZone.TimeZone()),
						  Calls = skillDay.WorkloadDayCollection.First().TaskPeriodList.First().Tasks,
						  AverageHandleTime = 40d
					 }
				};

			SkillRepository.Has(skill);
			SkillDayRepository.Has(skillDay);
			IntradayQueueStatisticsLoader.Has(actualCalls);

			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() });

			vm.DataSeries.UpdatedForecastedStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.UpdatedForecastedStaffing.First().Should().Be.EqualTo(null);
			vm.DataSeries.UpdatedForecastedStaffing.Last().Should().Be.GreaterThan(vm.DataSeries.ForecastedStaffing.Last());
		}

		[Test]
		public void ShouldReturnStaffingCorrectlyWhenViewingAfterSkillIsClosed()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 45, 0, DateTimeKind.Utc);
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 30, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var scenario = fakeScenarioAndIntervalLength();
			var act = ActivityRepository.Has("act");
			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, act);
			SkillRepository.Has(skill);

			var skillDay = createSkillDay(skill, scenario, userNow, new TimePeriod(8, 0, 8, 30), false);
			SkillDayRepository.Has(skillDay);

			IntradayQueueStatisticsLoader.Has(createStatistics(skillDay, latestStatsTime));

			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() });

			vm.DataSeries.ForecastedStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.ForecastedStaffing.First().Should().Be.GreaterThan(0d);
			vm.DataSeries.ForecastedStaffing.Last().Should().Be.GreaterThan(0d);
			vm.DataSeries.ActualStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.ActualStaffing.First().Should().Be.GreaterThan(0d);
			vm.DataSeries.ActualStaffing.Last().Should().Be.GreaterThan(0d);
			vm.DataSeries.UpdatedForecastedStaffing.Length.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnNoStaffingWhenNoSupportedSkill()
		{
			fakeScenarioAndIntervalLength();

			var skill = createEmailSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30));
			SkillRepository.Has(skill);

			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() });

			vm.DataSeries.Should().Be.EqualTo(null);
			vm.StaffingHasData.Should().Be.EqualTo(false);
		}

		[Test]
		public void ShouldReturnStaffingCorrectlyWhenViewingBeforeSkillIsOpen()
		{
			var userNow = new DateTime(2016, 8, 26, 7, 45, 0, DateTimeKind.Utc);
			var latestStatsTime = new DateTime(2016, 8, 26, 7, 0, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var scenario = fakeScenarioAndIntervalLength();
			var act = ActivityRepository.Has("act");
			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, act);
			SkillRepository.Has(skill);

			var skillDay = createSkillDay(skill, scenario, userNow, new TimePeriod(8, 0, 8, 30), false);
			SkillDayRepository.Has(skillDay);

			IntradayQueueStatisticsLoader.Has(createStatistics(skillDay, latestStatsTime));

			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() });

			vm.DataSeries.ForecastedStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.UpdatedForecastedStaffing.Length.Should().Be.EqualTo(0);
			vm.DataSeries.ActualStaffing.Length.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnUpdatedStaffingForecastCorrectlyWithMoreThanOneWorkload()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var scenario = fakeScenarioAndIntervalLength();
			var act = ActivityRepository.Has("act");
			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, act);
			SkillRepository.Has(skill);

			var skillDay = createSkillDay(skill, scenario, userNow, new TimePeriod(8, 0, 8, 30), false);

			IWorkload workload = new Workload(skill);
			workload.Description = "second workload";
			workload.Name = "second name from factory";
			workload.TemplateWeekCollection.ForEach(x => x.Value.ChangeOpenHours(new List<TimePeriod> { new TimePeriod(8, 0, 8, 30) }));
			var workloadDay = new WorkloadDay();
			workloadDay.CreateFromTemplate(new DateOnly(userNow), workload,
				(IWorkloadDayTemplate)workload.GetTemplate(TemplateTarget.Workload, userNow.DayOfWeek));
			skillDay.AddWorkloadDay(workloadDay);

			skillDay.WorkloadDayCollection.First().TaskPeriodList[0].Tasks = 5;
			skillDay.WorkloadDayCollection.Last().TaskPeriodList[0].Tasks = 10;

			var actualCallsPerSkill = skillDay.WorkloadDayCollection.First().TaskPeriodList[0].Tasks +
									  skillDay.WorkloadDayCollection.Last().TaskPeriodList[0].Tasks;

			SkillDayRepository.Has(skillDay);

			var actualIntervals = new List<SkillIntervalStatistics>
			{
				new SkillIntervalStatistics
				{
					SkillId = skill.Id.GetValueOrDefault(),
					StartTime = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc),
					Calls = actualCallsPerSkill * 1.2d
				}
			};

			IntradayQueueStatisticsLoader.Has(actualIntervals);

			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() });

			vm.DataSeries.UpdatedForecastedStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.UpdatedForecastedStaffing.First().Should().Be.EqualTo(null);
			var expectedUpdatedForecastOnLastInterval = vm.DataSeries.ForecastedStaffing.Last() * 1.2d;
			vm.DataSeries.UpdatedForecastedStaffing.Last().Should().Be.EqualTo(expectedUpdatedForecastOnLastInterval);
		}


		[Test]
		public void ShouldReturnActualStaffingCorrectlyWhenEfficencyIsLessThan100Percent()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);

			var scenario = fakeScenarioAndIntervalLength();
			var act = ActivityRepository.Has("act");
			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, act);
			var skillDay = createSkillDay(skill, scenario, userNow, new TimePeriod(8, 0, 8, 30), false);
			skillDay.SkillStaffPeriodCollection[0].Payload.Efficiency = new Percent(0.50);
			skillDay.SkillStaffPeriodCollection[1].Payload.Efficiency = new Percent(0.50);

			var efficency = skillDay.SkillStaffPeriodCollection.Select(s => s.Payload.Efficiency).First();
			var actualIntervals = createStatistics(skillDay, latestStatsTime);

			IntradayQueueStatisticsLoader.Has(actualIntervals);
			SkillRepository.Has(skill);
			SkillDayRepository.Has(skillDay);

			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() });

			var calculatorService = new StaffingCalculatorServiceFacade();
			var skillData = skillDay.SkillDataPeriodCollection.First().ServiceAgreement;
			var actualStaffingwith100Percentefficiency = calculatorService.AgentsUseOccupancy(
				skillData.ServiceLevel.Percent.Value,
				(int)skillData.ServiceLevel.Seconds,
				actualIntervals.First().Calls,
				actualIntervals.First().AverageHandleTime,
				TimeSpan.FromMinutes(minutesPerInterval),
				skillData.MinOccupancy.Value,
				skillData.MaxOccupancy.Value,
				1);

			vm.DataSeries.ActualStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.ActualStaffing.First().Should().Be.EqualTo(actualStaffingwith100Percentefficiency * 1 / efficency.Value);
		}

		[Test]
		public void ShouldSkipZeroForecastedCallIntervalsWhenCalculatingUpdatedForecast()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 30, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			var openHours = new TimePeriod(8, 0, 8, 45);
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);

			var scenario = fakeScenarioAndIntervalLength();
			var act = ActivityRepository.Has("act");
			var skill = createSkill(minutesPerInterval, "skill", openHours, false, act);
			var skillDay = skill.CreateSkillDayWithDemandOnInterval(scenario, new DateOnly(userNow), 1, new Tuple<TimePeriod, double>(openHours, 1)).WithId();
			skillDay.WorkloadDayCollection.First().TaskPeriodList[0].Tasks = 0.00008;
			skillDay.WorkloadDayCollection.First().TaskPeriodList[1].Tasks = 2;
			skillDay.WorkloadDayCollection.Last().TaskPeriodList[2].Tasks = 2;

			var actualIntervals = createStatistics(skillDay, latestStatsTime);

			SkillRepository.Has(skill);
			SkillDayRepository.Has(skillDay);
			IntradayQueueStatisticsLoader.Has(actualIntervals);

			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() });

			var expectedDeviation = vm.DataSeries.ForecastedStaffing.First() * actualIntervals[1].Calls / skillDay.WorkloadDayCollection.First().TaskPeriodList[1].Tasks;
			vm.DataSeries.UpdatedForecastedStaffing.Last().Should().Be.EqualTo(vm.DataSeries.ForecastedStaffing.Last() * expectedDeviation);
		}

		[Test]
		public void ShouldReturnStaffingForUsersCurrentDayInUsTimeZoneWhenOpen247()
		{
			TimeZone.IsNewYork();
			var userNow = new DateTime(2016, 8, 26, 23, 0, 0, DateTimeKind.Local);
			var latestStatsTimeLocal = new DateTime(2016, 8, 26, 22, 30, 0, DateTimeKind.Local);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var scenario = fakeScenarioAndIntervalLength();
			var act = ActivityRepository.Has("act");
			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(0, 0, 24, 0), false, act);
			SkillRepository.Has(skill);

			IntradayQueueStatisticsLoader.Has(createSkillDaysYesterdayTodayTomorrow(skill, scenario, userNow, TimeZoneHelper.ConvertToUtc(latestStatsTimeLocal, TimeZone.TimeZone())));

			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() });

			vm.DataSeries.ForecastedStaffing.Length.Should().Be.EqualTo(96);
			vm.DataSeries.UpdatedForecastedStaffing.Length.Should().Be.EqualTo(96);
			vm.DataSeries.UpdatedForecastedStaffing[92].Should().Be.GreaterThan(0d);
			vm.DataSeries.UpdatedForecastedStaffing[91].Should().Be.EqualTo(null);
		}

		[Test]
		public void ShouldReturnStaffingForUsersCurrentDayInAustraliaTimeZoneWhenOpen247()
		{
			TimeZone.IsAustralia();
			var userNow = new DateTime(2016, 8, 26, 2, 0, 0, DateTimeKind.Unspecified);
			var latestStatsTimeLocal = new DateTime(2016, 8, 26, 1, 45, 0, DateTimeKind.Unspecified);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var scenario = fakeScenarioAndIntervalLength();
			var act = ActivityRepository.Has("act");
			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(0, 0, 24, 0), false, act);
			SkillRepository.Has(skill);

			IntradayQueueStatisticsLoader.Has(createSkillDaysYesterdayTodayTomorrow(skill, scenario, userNow, TimeZoneHelper.ConvertToUtc(latestStatsTimeLocal, TimeZone.TimeZone())));

			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() });

			vm.DataSeries.ForecastedStaffing.Length.Should().Be.EqualTo(96);
			vm.DataSeries.UpdatedForecastedStaffing.Length.Should().Be.EqualTo(96);
			vm.DataSeries.UpdatedForecastedStaffing[7].Should().Be.EqualTo(null);
			vm.DataSeries.UpdatedForecastedStaffing[8].Should().Be.GreaterThan(0d);
		}

		[Test]
		public void ShouldHandleSkill30MinutesIntervalLengthAndTarget15()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 30, 0, DateTimeKind.Utc);
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var scenario = fakeScenarioAndIntervalLength();
			var act = ActivityRepository.Has("act");
			var skill = createSkill(30, "skill", new TimePeriod(8, 0, 9, 0), false, act);
			var skillDay = createSkillDay(skill, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 9, 0), false);

			var actualCalls = createStatistics(skillDay, latestStatsTime);

			SkillRepository.Has(skill);
			IntradayQueueStatisticsLoader.Has(actualCalls);
			SkillDayRepository.Has(skillDay);

			var actualStaffing = getActualStaffing(skillDay, actualCalls);
			var averageDeviation = calculateAverageDeviation(actualCalls, skillDay);

			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() });

			vm.DataSeries.Time.Length.Should().Be.EqualTo(4);
			vm.DataSeries.ForecastedStaffing.Length.Should().Be.EqualTo(4);
			vm.DataSeries.UpdatedForecastedStaffing.Length.Should().Be.EqualTo(4);
			vm.DataSeries.UpdatedForecastedStaffing.First().Should().Be.EqualTo(null);
			vm.DataSeries.UpdatedForecastedStaffing.Last().Should().Be.EqualTo(vm.DataSeries.ForecastedStaffing.Last() * averageDeviation);
			vm.DataSeries.ActualStaffing.Length.Should().Be.EqualTo(4);
			vm.DataSeries.ActualStaffing.First().Should().Be.EqualTo(actualStaffing);
		}

		[Test]
		public void ShouldSummariseForecastedStaffingForTwoSkills()
		{
			var scenario = fakeScenarioAndIntervalLength();
			var act = ActivityRepository.Has("act");
			var skill1 = createSkill(minutesPerInterval, "skill1", new TimePeriod(8, 0, 8, 30), false, act);
			var skill2 = createSkill(minutesPerInterval, "skill2", new TimePeriod(8, 0, 8, 30), false, act);
			SkillRepository.Has(skill1, skill2);

			var skillDay1 = createSkillDay(skill1, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false);
			var skillDay2 = createSkillDay(skill2, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false);
			SkillDayRepository.Has(skillDay1, skillDay2);

			var vm = Target.Load(new[] { skill1.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault() });

			var staffingIntervals1 = skillDay1.SkillStaffPeriodViewCollection(TimeSpan.FromMinutes(minutesPerInterval));
			var staffingIntervals2 = skillDay2.SkillStaffPeriodViewCollection(TimeSpan.FromMinutes(minutesPerInterval));

			vm.DataSeries.ForecastedStaffing.First().Should().Be.EqualTo(staffingIntervals1.First().FStaff + staffingIntervals2.First().FStaff);
			vm.DataSeries.ForecastedStaffing.Last().Should().Be.EqualTo(staffingIntervals1.Last().FStaff + staffingIntervals2.Last().FStaff);
		}

		[Test]
		public void ShouldShowUpdatedStaffingFromLatestStatsTimeWhenNowIsEarlierForDevTestAndDemoCases()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var scenario = fakeScenarioAndIntervalLength();
			var act = ActivityRepository.Has("act");
			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 45), false, act);
			var skillDay = createSkillDay(skill, scenario, userNow, new TimePeriod(8, 0, 8, 45), false);

			SkillRepository.Has(skill);
			SkillDayRepository.Has(skillDay);
			IntradayQueueStatisticsLoader.Has(new List<SkillIntervalStatistics>
			{
				new SkillIntervalStatistics { SkillId = skill.Id.GetValueOrDefault(), StartTime = latestStatsTime, Calls = 30 }
			});

			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() });

			vm.DataSeries.UpdatedForecastedStaffing.Length.Should().Be.EqualTo(3);
			vm.DataSeries.UpdatedForecastedStaffing.First().Should().Be.EqualTo(null);
			vm.DataSeries.UpdatedForecastedStaffing[1].Should().Be.EqualTo(null);
			vm.DataSeries.UpdatedForecastedStaffing.Last().Should().Be.GreaterThan(0d);
		}

		[Test]
		public void ShouldNotShowUpdatedAndActualStaffingWhenNoStatistics()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var scenario = fakeScenarioAndIntervalLength();
			var act = ActivityRepository.Has("act");
			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 45), false, act);
			var skillDay = createSkillDay(skill, scenario, userNow, new TimePeriod(8, 0, 8, 45), false);

			SkillRepository.Has(skill);
			SkillDayRepository.Has(skillDay);

			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() });

			vm.DataSeries.UpdatedForecastedStaffing.Should().Be.Empty();
			vm.DataSeries.ActualStaffing.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotShowAnyStaffingDataWhenNoForecast()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			fakeScenarioAndIntervalLength();

			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			var act = ActivityRepository.Has("act");
			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, act);

			SkillRepository.Has(skill);
			IntradayQueueStatisticsLoader.Has(new List<SkillIntervalStatistics>
			  {
				  new SkillIntervalStatistics {SkillId = skill.Id.GetValueOrDefault(), StartTime = latestStatsTime, Calls = 30}
			  });

			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() });

			vm.DataSeries.ForecastedStaffing.Should().Be.Empty();
			vm.DataSeries.UpdatedForecastedStaffing.Should().Be.Empty();
			vm.DataSeries.ActualStaffing.Should().Be.Empty();
			vm.StaffingHasData.Should().Be.EqualTo(false);
		}

		[Test]
		public void ShouldCalculateUpdatedForecastedStaffingCorrectlyForThreeSkills()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			var latestStatsTime = new DateTime(2016, 8, 26, 7, 45, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			var scenario = fakeScenarioAndIntervalLength();
			var act = ActivityRepository.Has("act");
			var skillWithReforecast1 = createSkill(minutesPerInterval, "skill with reforecast 1", new TimePeriod(7, 0, 8, 15), false, act);
			var skillWithReforecast2 = createSkill(minutesPerInterval, "skill with reforecast 2", new TimePeriod(7, 0, 8, 15), false, act);
			var skillWithoutReforecast = createSkill(minutesPerInterval, "skill without reforecast", new TimePeriod(7, 45, 8, 15), false, act);

			var skillDayWithReforecast1 = createSkillDay(skillWithReforecast1, scenario, Now.UtcDateTime(), new TimePeriod(7, 0, 8, 15), false);
			var skillDayWithReforecast2 = createSkillDay(skillWithReforecast2, scenario, Now.UtcDateTime(), new TimePeriod(7, 0, 8, 15), false);
			var skillDayWithoutReforecast = createSkillDay(skillWithoutReforecast, scenario, Now.UtcDateTime(), new TimePeriod(7, 45, 8, 15), false);

			var actualCalls1 = createStatistics(skillDayWithReforecast1, latestStatsTime);
			var actualCalls2 = createStatistics(skillDayWithReforecast2, latestStatsTime);

			var actualCallsTotal = new List<SkillIntervalStatistics>();
			actualCallsTotal.AddRange(actualCalls1);
			actualCallsTotal.AddRange(actualCalls2);

			SkillRepository.Has(skillWithReforecast1, skillWithReforecast2, skillWithoutReforecast);
			SkillDayRepository.Has(skillDayWithReforecast1, skillDayWithReforecast2, skillDayWithoutReforecast);
			IntradayQueueStatisticsLoader.Has(actualCallsTotal);

			var deviationWithReforecast1 = calculateAverageDeviation(actualCalls1, skillDayWithReforecast1);
			var deviationWithReforecast2 = calculateAverageDeviation(actualCalls2, skillDayWithReforecast2);
			var noDeviationWithoutReforecast = 1.0d;
			var finalDeviationFactor = (deviationWithReforecast1 + deviationWithReforecast2 + noDeviationWithoutReforecast) / 3;

			var expectedUpdatedStaffing =
				Math.Round(skillDayWithoutReforecast.SkillStaffPeriodCollection.Last().FStaff +
				skillDayWithReforecast1.SkillStaffPeriodCollection.Last().FStaff * deviationWithReforecast1 +
				skillDayWithReforecast2.SkillStaffPeriodCollection.Last().FStaff * deviationWithReforecast2, 3);

			var vm = Target.Load(new[] { skillWithReforecast1.Id.GetValueOrDefault(), skillWithReforecast2.Id.GetValueOrDefault(), skillWithoutReforecast.Id.GetValueOrDefault() });

			vm.DataSeries.UpdatedForecastedStaffing.Length.Should().Be.EqualTo(5);
			vm.DataSeries.UpdatedForecastedStaffing.First().Should().Be.EqualTo(null);
			Math.Round((double)vm.DataSeries.UpdatedForecastedStaffing.Last(), 3).Should().Be.EqualTo(expectedUpdatedStaffing);
			expectedUpdatedStaffing.Should().Be.EqualTo(Math.Round((double)vm.DataSeries.ForecastedStaffing.Last() * finalDeviationFactor, 3));
		}

		[Test]
		public void ShouldReturnActualStaffingUpUntilLatestStatsTimeEvenWhenWeHaveStatsBeforeOpenHours()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var scenario = fakeScenarioAndIntervalLength();
			var act = ActivityRepository.Has("act");
			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, act);
			var skillDay = createSkillDay(skill, scenario, userNow, new TimePeriod(8, 0, 8, 30), false);
			var skillStats = new List<SkillIntervalStatistics>
				{
					 new SkillIntervalStatistics
					 {
						  SkillId = skill.Id.GetValueOrDefault(),
						  StartTime = latestStatsTime.AddMinutes(-minutesPerInterval),
						  Calls = 123,
						  AverageHandleTime = 40d
					 }
				};
			skillStats.AddRange(createStatistics(skillDay, latestStatsTime));

			SkillDayRepository.Has(skillDay);
			SkillRepository.Has(skill);
			IntradayQueueStatisticsLoader.Has(skillStats);

			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() });

			vm.DataSeries.ActualStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.ActualStaffing.First().Should().Be.GreaterThan(0d);
			vm.DataSeries.ActualStaffing.Last().Should().Be.EqualTo(null);
		}

		[Test]
		public void ShouldReturnActualStaffingNotStartingFromStartOfOpen()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 30, 0, DateTimeKind.Utc);
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var scenario = fakeScenarioAndIntervalLength();
			var act = ActivityRepository.Has("act");
			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 45), false, act);
			var skillDay = createSkillDay(skill, scenario, userNow, new TimePeriod(8, 0, 8, 45), false);
			var skillStats = new List<SkillIntervalStatistics>
				{
					 new SkillIntervalStatistics
					 {
						  SkillId = skill.Id.GetValueOrDefault(),
						  StartTime = latestStatsTime,
						  Calls = 123,
						  AverageHandleTime = 40d
					 }
				};

			SkillRepository.Has(skill);
			SkillDayRepository.Has(skillDay);
			IntradayQueueStatisticsLoader.Has(skillStats);

			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() });

			vm.DataSeries.ActualStaffing.Length.Should().Be.EqualTo(3);
			vm.DataSeries.ActualStaffing.First().Should().Be.EqualTo(null);
			vm.DataSeries.ActualStaffing[1].Should().Be.GreaterThan(0d);
			vm.DataSeries.ActualStaffing.Last().Should().Be.EqualTo(null);
		}

		[Test]
		public void ShouldReturnActualStaffingWhenSkillDataPeriodsHaveDuplicates()
		{
			var userNow = new DateTime(2016, 8, 26, 0, 30, 0, DateTimeKind.Utc);
			var latestStatsTime = new DateTime(2016, 8, 26, 0, 0, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var scenario = fakeScenarioAndIntervalLength();
			var act = ActivityRepository.Has("act");
			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(0, 0, 0, 30), false, act);
			var skillDay = createSkillDay(skill, scenario, userNow, new TimePeriod(0, 0, 0, 30), true);
			var skillStats = new List<SkillIntervalStatistics>
				{
					 new SkillIntervalStatistics
					 {
						  SkillId = skill.Id.GetValueOrDefault(),
						  StartTime = latestStatsTime,
						  Calls = 123,
						  AverageHandleTime = 40d
					 }
				};

			SkillRepository.Has(skill);
			SkillDayRepository.Has(skillDay);
			IntradayQueueStatisticsLoader.Has(skillStats);

			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() });

			vm.DataSeries.ActualStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.ActualStaffing.First().Should().Be.GreaterThan(0d);
			vm.DataSeries.ActualStaffing.Last().Should().Be.EqualTo(null);
		}

		[Test]
		public void ShouldSummariseActualStaffingForTwoSkills()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var scenario = fakeScenarioAndIntervalLength();
			var act = ActivityRepository.Has("act");
			var skill1 = createSkill(minutesPerInterval, "skill1", new TimePeriod(8, 0, 8, 30), false, act);
			var skill2 = createSkill(minutesPerInterval, "skill2", new TimePeriod(8, 0, 8, 30), false, act);

			var skillDay1 = createSkillDay(skill1, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false);
			var skillDay2 = createSkillDay(skill2, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false);

			var actualCalls1 = createStatistics(skillDay1, latestStatsTime);
			var actualCalls2 = createStatistics(skillDay2, latestStatsTime);

			var actualCallsTotal = new List<SkillIntervalStatistics>();
			actualCallsTotal.AddRange(actualCalls1);
			actualCallsTotal.AddRange(actualCalls2);

			SkillRepository.Has(skill1, skill2);
			SkillDayRepository.Has(skillDay1, skillDay2);
			IntradayQueueStatisticsLoader.Has(actualCallsTotal);

			var actualStaffingSkill1 = getActualStaffing(skillDay1, actualCalls1);
			var actualStaffingSkill2 = getActualStaffing(skillDay2, actualCalls2);

			var vm = Target.Load(new[] { skill1.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault() });

			vm.DataSeries.ActualStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.ActualStaffing.First().Should().Be.EqualTo(actualStaffingSkill1 + actualStaffingSkill2);
			vm.DataSeries.ActualStaffing.Last().Should().Be.EqualTo(null);
		}

		[Test]
		public void ShouldExcludeUnsupportedSkillsForStaffingCalculation()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var scenario = fakeScenarioAndIntervalLength();
			var act = ActivityRepository.Has("act");
			var phoneSkill = createSkill(minutesPerInterval, "skill1", new TimePeriod(8, 0, 8, 30), false, act);
			var emailSkill = createEmailSkill(minutesPerInterval, "skill2", new TimePeriod(8, 0, 8, 30));

			var skillDayPhone = createSkillDay(phoneSkill, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false);
			var skillDayEmail = createSkillDay(emailSkill, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false);

			var actualPhoneStats = createStatistics(skillDayPhone, latestStatsTime);
			var actualEmailStats = createStatistics(skillDayEmail, latestStatsTime);

			var actualStatsTotal = new List<SkillIntervalStatistics>();
			actualStatsTotal.AddRange(actualPhoneStats);
			actualStatsTotal.AddRange(actualEmailStats);

			SkillRepository.Has(phoneSkill, emailSkill);
			SkillDayRepository.Has(skillDayPhone, skillDayEmail);
			IntradayQueueStatisticsLoader.Has(actualStatsTotal);

			var actualStaffingSkill1 = getActualStaffing(skillDayPhone, actualPhoneStats);

			var vm = Target.Load(new[] { phoneSkill.Id.GetValueOrDefault(), emailSkill.Id.GetValueOrDefault() });

			var deviationWithReforecast = calculateAverageDeviation(actualPhoneStats, skillDayPhone);

			vm.DataSeries.ActualStaffing.First().Should().Be.EqualTo(actualStaffingSkill1);
			vm.DataSeries.ActualStaffing.Last().Should().Be.EqualTo(null);
			vm.DataSeries.ForecastedStaffing.First().Should().Be.EqualTo(3);
			vm.DataSeries.UpdatedForecastedStaffing.Last().Should().Be.EqualTo(vm.DataSeries.ForecastedStaffing.Last() * deviationWithReforecast);
		}

		[Test]
		public void ShouldReturnScheduledStaffing()
		{
			TimeZone.IsSweden();
			fakeScenarioAndIntervalLength();
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			var act = ActivityRepository.Has("act");
			var skill = createSkill(minutesPerInterval, "skill1", new TimePeriod(8, 0, 8, 30), false, act);
			SkillRepository.Has(skill);

			populateStaffingReadModels(skill, userNow, userNow.AddMinutes(minutesPerInterval), 5.7);

			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() });

			vm.DataSeries.Time.Length.Should().Be.EqualTo(1);
			vm.DataSeries.Time.First().Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(userNow, TimeZone.TimeZone()));
			vm.DataSeries.ScheduledStaffing.Length.Should().Be.EqualTo(1);
			vm.DataSeries.ScheduledStaffing.First().Should().Be.EqualTo(5.7);
		}

		[Test]
		public void ShouldReturnScheduledStaffingForTwoSkills()
		{
			TimeZone.IsSweden();
			fakeScenarioAndIntervalLength();
			var act = ActivityRepository.Has("act");
			var skill1 = createSkill(minutesPerInterval, "skill1", new TimePeriod(8, 0, 8, 30), false, act);
			var skill2 = createSkill(minutesPerInterval, "skill2", new TimePeriod(8, 0, 8, 30), false, act);
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			SkillRepository.Has(skill1, skill2);

			populateStaffingReadModels(skill1, userNow, userNow.AddMinutes(minutesPerInterval), 5.7);
			populateStaffingReadModels(skill2, userNow, userNow.AddMinutes(minutesPerInterval), 7.7);


			var vm = Target.Load(new[] { skill1.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault() });

			vm.DataSeries.Time.Length.Should().Be.EqualTo(1);
			vm.DataSeries.Time.First().Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(userNow, TimeZone.TimeZone()));
			vm.DataSeries.ScheduledStaffing.Length.Should().Be.EqualTo(1);
			vm.DataSeries.ScheduledStaffing.First().Should().Be.EqualTo(5.7 + 7.7);
		}

		[Test]
		public void ShouldReturnScheduledStaffingWithShrinkage()
		{
			TimeZone.IsSweden();
			fakeScenarioAndIntervalLength();
			var act = ActivityRepository.Has("act");
			var skill = createSkill(minutesPerInterval, "skill1", new TimePeriod(8, 0, 8, 30), false, act);
			skill.SetCascadingIndex(1);
			var skill2 = createSkill(minutesPerInterval, "skill2", new TimePeriod(8, 0, 8, 30), false, act);
			skill2.SetCascadingIndex(2);
			SkillRepository.Has(skill, skill2);
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var skillStaffingIntervals = new List<SkillStaffingInterval>();
			var skillCombinationResources = new List<SkillCombinationResource>();

			skillStaffingIntervals.Add(new SkillStaffingInterval
									   {
										   SkillId = skill.Id.GetValueOrDefault(),
										   StartDateTime = userNow,
										   EndDateTime = userNow.AddMinutes(minutesPerInterval),
										   Forecast = 1,
										   ForecastWithShrinkage = 2
									   });

			skillStaffingIntervals.Add(new SkillStaffingInterval
									   {
										   SkillId = skill2.Id.GetValueOrDefault(),
										   StartDateTime = userNow,
										   EndDateTime = userNow.AddMinutes(minutesPerInterval),
										   Forecast = 1,
										   ForecastWithShrinkage = 20
									   });


			skillCombinationResources.Add(new SkillCombinationResource
										  {
											  StartDateTime = userNow,
											  EndDateTime = userNow.AddMinutes(minutesPerInterval),
											  Resource = 34,
											  SkillCombination = new[] {skill.Id.GetValueOrDefault(),skill2.Id.GetValueOrDefault()}
										  });

			ScheduleForecastSkillReadModelRepository.Persist(skillStaffingIntervals, DateTime.UtcNow);
			SkillCombinationResourceRepository.AddSkillCombinationResource(DateTime.UtcNow, skillCombinationResources);

			var vm = Target.Load(new[] {skill.Id.GetValueOrDefault()}, null, true);

			vm.DataSeries.Time.Length.Should().Be.EqualTo(1);
			vm.DataSeries.Time.First().Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(userNow, TimeZone.TimeZone()));
			vm.DataSeries.ScheduledStaffing.Length.Should().Be.EqualTo(1);
			vm.DataSeries.ScheduledStaffing.First().Should().Be.EqualTo(14);

			var vm2 = Target.Load(new[] { skill2.Id.GetValueOrDefault() }, null, true);

			vm2.DataSeries.Time.Length.Should().Be.EqualTo(1);
			vm2.DataSeries.Time.First().Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(userNow, TimeZone.TimeZone()));
			vm2.DataSeries.ScheduledStaffing.Length.Should().Be.EqualTo(1);
			vm2.DataSeries.ScheduledStaffing.First().Should().Be.EqualTo(20);
		}

		[Test]
		public void ShouldReturnForecastWithShrinkage()
		{
			TimeZone.IsSweden();
			var scenario = fakeScenarioAndIntervalLength();
			var act = ActivityRepository.Has("act");
			var skill = createSkill(minutesPerInterval, "skill1", new TimePeriod(8, 0, 8, 30), false, act);
			skill.SetCascadingIndex(1);
			var skill2 = createSkill(minutesPerInterval, "skill2", new TimePeriod(8, 0, 8, 30), false, act);
			skill2.SetCascadingIndex(2);
			SkillRepository.Has(skill, skill2);
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var skillCombinationResources = new List<SkillCombinationResource>();

			var skillday = skill.CreateSkillDayWithDemand(scenario, new DateOnly(userNow), 1);
			skillday.SkillDataPeriodCollection.ForEach(x => x.Shrinkage = new Percent(0.2));
			SkillDayRepository.Has(skillday);

			var skillday2 = skill2.CreateSkillDayWithDemand(scenario, new DateOnly(userNow), 1);
			skillday2.SkillDataPeriodCollection.ForEach(x => x.Shrinkage = new Percent(0.5));
			SkillDayRepository.Has(skillday2);

			skillCombinationResources.Add(new SkillCombinationResource
			{
				StartDateTime = userNow,
				EndDateTime = userNow.AddMinutes(minutesPerInterval),
				Resource = 34,
				SkillCombination = new[] { skill.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault() }
			});
			
			SkillCombinationResourceRepository.AddSkillCombinationResource(DateTime.UtcNow, skillCombinationResources);

			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() }, null, true);
			vm.DataSeries.ForecastedStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.ForecastedStaffing.First().Should().Be.EqualTo(1.25);

			var vm2 = Target.Load(new[] { skill2.Id.GetValueOrDefault() }, null, true);

			vm2.DataSeries.ForecastedStaffing.Length.Should().Be.EqualTo(2);
			vm2.DataSeries.ForecastedStaffing.First().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldReturnScheduledStaffingWithShrinkageSplit()
		{
			TimeZone.IsSweden();
			fakeScenarioAndIntervalLength();
			var act = ActivityRepository.Has("act");
			var skill = createSkill(minutesPerInterval, "skill1", new TimePeriod(8, 0, 9, 0), false, act);
			skill.SetCascadingIndex(1);
			var skill2 = createSkill(minutesPerInterval, "skill2", new TimePeriod(8, 0, 9, 0), false, act);
			skill2.SetCascadingIndex(2);
			skill.DefaultResolution = skill2.DefaultResolution = minutesPerInterval*2;
			SkillRepository.Has(skill, skill2);
			var userNow = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var skillStaffingIntervals = new List<SkillStaffingInterval>();
			var skillCombinationResources = new List<SkillCombinationResource>();

			skillStaffingIntervals.Add(new SkillStaffingInterval
			{
				SkillId = skill.Id.GetValueOrDefault(),
				StartDateTime = userNow,
				EndDateTime = userNow.AddMinutes(minutesPerInterval * 2),
				Forecast = 1,
				ForecastWithShrinkage = 2
			});

			skillStaffingIntervals.Add(new SkillStaffingInterval
			{
				SkillId = skill2.Id.GetValueOrDefault(),
				StartDateTime = userNow,
				EndDateTime = userNow.AddMinutes(minutesPerInterval * 2),
				Forecast = 1,
				ForecastWithShrinkage = 20
			});


			skillCombinationResources.Add(new SkillCombinationResource
			{
				StartDateTime = userNow,
				EndDateTime = userNow.AddMinutes(minutesPerInterval * 2),
				Resource = 34,
				SkillCombination = new[] { skill.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault() }
			});

			ScheduleForecastSkillReadModelRepository.Persist(skillStaffingIntervals, DateTime.UtcNow);
			SkillCombinationResourceRepository.AddSkillCombinationResource(DateTime.UtcNow, skillCombinationResources);

			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() }, null, true);

			vm.DataSeries.Time.Length.Should().Be.EqualTo(2);
			vm.DataSeries.Time.First().Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(userNow, TimeZone.TimeZone()));
			vm.DataSeries.ScheduledStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.ScheduledStaffing.First().Should().Be.EqualTo(14);
			vm.DataSeries.ScheduledStaffing.Last().Should().Be.EqualTo(14);

			var vm2 = Target.Load(new[] { skill2.Id.GetValueOrDefault() }, null, true);

			vm2.DataSeries.Time.Length.Should().Be.EqualTo(2);
			vm2.DataSeries.Time.First().Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(userNow, TimeZone.TimeZone()));
			vm2.DataSeries.ScheduledStaffing.Length.Should().Be.EqualTo(2);
			vm2.DataSeries.ScheduledStaffing.First().Should().Be.EqualTo(20);
			vm2.DataSeries.ScheduledStaffing.Last().Should().Be.EqualTo(20);
		}

		[Test]
		public void ShouldReturnForecastWithShrinkageSplit()
		{
			TimeZone.IsSweden();
			var scenario = fakeScenarioAndIntervalLength();
			var act = ActivityRepository.Has("act");
			var skill = createSkill(minutesPerInterval, "skill1", new TimePeriod(8, 0, 9, 0), false, act);
			skill.SetCascadingIndex(1);
			var skill2 = createSkill(minutesPerInterval, "skill2", new TimePeriod(8, 0, 9, 0), false, act);
			skill2.SetCascadingIndex(2);
			skill.DefaultResolution = skill2.DefaultResolution = minutesPerInterval * 2;
			SkillRepository.Has(skill, skill2);
			var userNow = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			var skillCombinationResources = new List<SkillCombinationResource>();

			var skillday = skill.CreateSkillDayWithDemand(scenario, new DateOnly(userNow), 1);
			skillday.SkillDataPeriodCollection.ForEach(x => x.Shrinkage = new Percent(0.2));
			SkillDayRepository.Has(skillday);

			var skillday2 = skill2.CreateSkillDayWithDemand(scenario, new DateOnly(userNow), 1);
			skillday2.SkillDataPeriodCollection.ForEach(x => x.Shrinkage = new Percent(0.5));
			SkillDayRepository.Has(skillday2);

			skillCombinationResources.Add(new SkillCombinationResource
			{
				StartDateTime = userNow,
				EndDateTime = userNow.AddMinutes(minutesPerInterval * 2),
				Resource = 34,
				SkillCombination = new[] { skill.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault() }
			});
			skillCombinationResources.Add(new SkillCombinationResource
			{
				StartDateTime = userNow.AddMinutes(minutesPerInterval * 2),
				EndDateTime = userNow.AddMinutes(minutesPerInterval * 4),
				Resource = 34,
				SkillCombination = new[] { skill.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault() }
			});

			SkillCombinationResourceRepository.AddSkillCombinationResource(DateTime.UtcNow, skillCombinationResources);

			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() }, null, true);

			vm.DataSeries.Time.Length.Should().Be.EqualTo(4);
			vm.DataSeries.Time.First().Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(userNow, TimeZone.TimeZone()));
			vm.DataSeries.ForecastedStaffing.Length.Should().Be.EqualTo(4);
			vm.DataSeries.ForecastedStaffing.First().Should().Be.EqualTo(1.25);
			vm.DataSeries.ForecastedStaffing.Last().Should().Be.EqualTo(1.25);

			var vm2 = Target.Load(new[] { skill2.Id.GetValueOrDefault() }, null, true);

			vm2.DataSeries.Time.Length.Should().Be.EqualTo(4);
			vm2.DataSeries.Time.First().Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(userNow, TimeZone.TimeZone()));
			vm2.DataSeries.ForecastedStaffing.Length.Should().Be.EqualTo(4);
			vm2.DataSeries.ForecastedStaffing.First().Should().Be.EqualTo(2);
			vm2.DataSeries.ForecastedStaffing.Last().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldReturnScheduledStaffingStartingBeforeForecasted()
		{
			TimeZone.IsSweden();
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			var scheduledStartTime = new DateTime(2016, 8, 26, 7, 45, 0, DateTimeKind.Utc);
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);

			var scenario = fakeScenarioAndIntervalLength();
			var act = ActivityRepository.Has("act");
			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, act);
			var skillDay = createSkillDay(skill, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false);

			SkillRepository.Has(skill);
			SkillDayRepository.Has(skillDay);

			var skillStats = new List<SkillIntervalStatistics>
			{
				new SkillIntervalStatistics
				{
					SkillId = skill.Id.GetValueOrDefault(),
					StartTime = TimeZoneHelper.ConvertFromUtc(latestStatsTime, TimeZone.TimeZone()),
					Calls = 123,
					AverageHandleTime = 40d
				}
			};

			IntradayQueueStatisticsLoader.Has(skillStats);

			populateStaffingReadModels(skill, scheduledStartTime, scheduledStartTime.AddMinutes(3*minutesPerInterval), 4.9).ToList();

			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() });

			vm.DataSeries.Time.Length.Should().Be.EqualTo(3);
			vm.DataSeries.Time.First().Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(scheduledStartTime, TimeZone.TimeZone()));
			vm.DataSeries.ScheduledStaffing.Length.Should().Be.EqualTo(3);
			vm.DataSeries.ScheduledStaffing.First().Should().Be.EqualTo(4.9);
			vm.DataSeries.ScheduledStaffing.Last().Should().Be.EqualTo(4.9);
			vm.DataSeries.ForecastedStaffing.Length.Should().Be.EqualTo(3);
			vm.DataSeries.ForecastedStaffing.First().Should().Be.EqualTo(null);
			vm.DataSeries.ForecastedStaffing[1].Should().Be.GreaterThan(0d);
			vm.DataSeries.ForecastedStaffing.Last().Should().Be.GreaterThan(0d);
			vm.DataSeries.UpdatedForecastedStaffing.Length.Should().Be.EqualTo(3);
			vm.DataSeries.UpdatedForecastedStaffing[1].Should().Be.EqualTo(null);
			vm.DataSeries.UpdatedForecastedStaffing.Last().Should().Be.GreaterThan(0d);
			vm.DataSeries.ActualStaffing.Length.Should().Be.EqualTo(3);
			vm.DataSeries.ActualStaffing.First().Should().Be.EqualTo(null);
			vm.DataSeries.ActualStaffing[1].Should().Be.GreaterThan(0d);
			vm.DataSeries.ActualStaffing.Last().Should().Be.EqualTo(null);
		}

		[Test]
		public void ShouldReturnScheduledStaffingEndingAfterForecasted()
		{
			TimeZone.IsSweden();
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			var scheduledStartTime = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);

			var scenario = fakeScenarioAndIntervalLength();
			var act = ActivityRepository.Has("act");
			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, act);
			var skillDay = createSkillDay(skill, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false);

			SkillRepository.Has(skill);
			SkillDayRepository.Has(skillDay);
			IntradayQueueStatisticsLoader.Has(createStatistics(skillDay, latestStatsTime));

			populateStaffingReadModels(skill, scheduledStartTime, scheduledStartTime.AddMinutes(3*minutesPerInterval), 4.9).ToList();

			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() });

			vm.DataSeries.Time.Length.Should().Be.EqualTo(3);
			vm.DataSeries.Time.First().Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(scheduledStartTime, TimeZone.TimeZone()));
			vm.DataSeries.ScheduledStaffing.Length.Should().Be.EqualTo(3);
			vm.DataSeries.ScheduledStaffing.First().Should().Be.EqualTo(4.9);
			vm.DataSeries.ScheduledStaffing.Last().Should().Be.EqualTo(4.9);
			vm.DataSeries.ForecastedStaffing.Length.Should().Be.EqualTo(3);
			vm.DataSeries.ForecastedStaffing.First().Should().Be.GreaterThan(0d);
			vm.DataSeries.ForecastedStaffing[1].Should().Be.GreaterThan(0d);
			vm.DataSeries.ForecastedStaffing.Last().Should().Be.EqualTo(null);
			vm.DataSeries.UpdatedForecastedStaffing.Length.Should().Be.EqualTo(3);
			vm.DataSeries.UpdatedForecastedStaffing[1].Should().Be.GreaterThan(0d);
			vm.DataSeries.UpdatedForecastedStaffing.Last().Should().Be.EqualTo(null);
			vm.DataSeries.ActualStaffing.Length.Should().Be.EqualTo(3);
			vm.DataSeries.ActualStaffing.First().Should().Be.GreaterThan(0d);
			vm.DataSeries.ActualStaffing[1].Should().Be.EqualTo(null);
			vm.DataSeries.ActualStaffing.Last().Should().Be.EqualTo(null);
		}

		[Test]
		public void ShouldReturnScheduledStaffingStartingAfterForecasted()
		{
			TimeZone.IsSweden();
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			var scheduledStartTime = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);

			var scenario = fakeScenarioAndIntervalLength();
			var act = ActivityRepository.Has("act");
			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, act);
			var skillDay = createSkillDay(skill, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false);

			SkillRepository.Has(skill);
			SkillDayRepository.Has(skillDay);

			var skillStats = createStatistics(skillDay, latestStatsTime);

			IntradayQueueStatisticsLoader.Has(skillStats);

			populateStaffingReadModels(skill, scheduledStartTime, scheduledStartTime.AddMinutes(minutesPerInterval), 8.3);

			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() });

			vm.DataSeries.Time.Length.Should().Be.EqualTo(2);
			vm.DataSeries.Time.First().Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(scheduledStartTime.AddMinutes(-minutesPerInterval), TimeZone.TimeZone()));
			vm.DataSeries.ScheduledStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.ScheduledStaffing.First().Should().Be.EqualTo(null);
			vm.DataSeries.ScheduledStaffing.Last().Should().Be.EqualTo(8.3);
			vm.DataSeries.ForecastedStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.ForecastedStaffing.First().Should().Be.GreaterThan(0d);
			vm.DataSeries.ForecastedStaffing.Last().Should().Be.GreaterThan(0d);
			vm.DataSeries.UpdatedForecastedStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.UpdatedForecastedStaffing.First().Should().Be.EqualTo(null);
			vm.DataSeries.UpdatedForecastedStaffing.Last().Should().Be.GreaterThan(0d);
			vm.DataSeries.ActualStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.ActualStaffing.First().Should().Be.GreaterThan(0d);
			vm.DataSeries.ActualStaffing.Last().Should().Be.EqualTo(null);
		}

		[Test]
		public void ShouldReturnScheduledStaffingEndingBeforeForecasted()
		{
			TimeZone.IsSweden();
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			var scheduledStartTime = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);

			var scenario = fakeScenarioAndIntervalLength();
			var act = ActivityRepository.Has("act");
			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, act);
			var skillDay = createSkillDay(skill, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false);

			SkillRepository.Has(skill);
			SkillDayRepository.Has(skillDay);

			var skillStats = createStatistics(skillDay, latestStatsTime);

			IntradayQueueStatisticsLoader.Has(skillStats);

			populateStaffingReadModels(skill, scheduledStartTime, scheduledStartTime.AddMinutes(minutesPerInterval), 5.7);

			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() });

			vm.DataSeries.Time.Length.Should().Be.EqualTo(2);
			vm.DataSeries.Time.First().Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(scheduledStartTime, TimeZone.TimeZone()));
			vm.DataSeries.ScheduledStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.ScheduledStaffing.First().Should().Be.GreaterThan(0d);
			vm.DataSeries.ScheduledStaffing.Last().Should().Be.EqualTo(null);
		}

		[Test]
		public void ShouldReturnScheduledStaffingInCorrectOrder()
		{
			TimeZone.IsSweden();
			fakeScenarioAndIntervalLength();
			var act = ActivityRepository.Has("act");
			var skill = createSkill(minutesPerInterval, "skill1", new TimePeriod(8, 0, 8, 30), false, act);
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			SkillRepository.Has(skill);
			populateStaffingReadModels(skill, userNow, userNow.AddMinutes(minutesPerInterval), 15);
			populateStaffingReadModels(skill, userNow.AddMinutes(-minutesPerInterval), userNow, 10);

			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() });

			vm.DataSeries.Time.Length.Should().Be.EqualTo(2);
			vm.DataSeries.Time.First().Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(userNow.AddMinutes(-minutesPerInterval), TimeZone.TimeZone()));
			vm.DataSeries.ScheduledStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.ScheduledStaffing.First().Should().Be.EqualTo(10);
			vm.DataSeries.ScheduledStaffing.Last().Should().Be.EqualTo(15);
		}


		[Test]
		public void ShouldReturnEmptySeriesForScheduleStaffing()
		{
			TimeZone.IsSweden();
			var scenario = fakeScenarioAndIntervalLength();
			var act = ActivityRepository.Has("act");
			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, act);
			SkillRepository.Has(skill);

			var skillDay = createSkillDay(skill, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false);
			SkillDayRepository.Has(skillDay);

			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() });

			vm.StaffingHasData.Should().Be.EqualTo(true);
			vm.DataSeries.ScheduledStaffing.IsEmpty().Should().Be.EqualTo(true);
		}


		private Scenario fakeScenarioAndIntervalLength()
		{
			IntervalLengthFetcher.Has(minutesPerInterval);
			var scenario = ScenarioFactory.CreateScenario("scenariorita", true, true).WithId();
			ScenarioRepository.Has(scenario);
			return scenario;
		}

		private ISkill createSkill(int intervalLength, string skillName, TimePeriod openHours, bool isClosedOnWeekends, IActivity activity)
		{
			var skill =
				new Skill(skillName, skillName, Color.Empty, intervalLength, new SkillTypePhone(new Description("SkillTypeInboundTelephony"), ForecastSource.InboundTelephony))
				{
					TimeZone = TimeZoneInfo.Utc
				}.WithId();
			if (isClosedOnWeekends)
				WorkloadFactory.CreateWorkloadClosedOnWeekendsWithOpenHours(skill, openHours);
			else
				WorkloadFactory.CreateWorkloadWithOpenHours(skill, openHours);
			skill.Activity = activity;
			return skill;
		}

		private ISkill createEmailSkill(int intervalLength, string skillName, TimePeriod openHours)
		{
			var skill =
				new Skill(skillName, skillName, Color.Empty, intervalLength, new SkillTypeEmail(new Description("SkillTypeEmail"), ForecastSource.InboundTelephony))
				{
					TimeZone = TimeZoneInfo.Utc
				}.WithId();
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, openHours);

			return skill;
		}

		private ISkillDay createSkillDay(ISkill skill, IScenario scenario, DateTime userNow, TimePeriod openHours, bool addSkillDataPeriodDuplicate)
		{
			var random = new Random();
			ISkillDay skillDay;
			if (addSkillDataPeriodDuplicate)
				skillDay =
					skill.CreateSkillDayWithDemandOnIntervalWithSkillDataPeriodDuplicate(scenario, new DateOnly(userNow), 3,
						new Tuple<TimePeriod, double>(openHours, 3)).WithId();
			else
				skillDay =
					skill.CreateSkillDayWithDemandOnInterval(scenario, new DateOnly(userNow), 3,
						new Tuple<TimePeriod, double>(openHours, 3)).WithId();

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

		private IList<SkillIntervalStatistics> createStatistics(ISkillDay skillDay, DateTime latestStatsTime)
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
					StartTime = TimeZoneHelper.ConvertFromUtc(intervalTime, TimeZone.TimeZone()),
					Calls = random.Next(5, 50),
					AverageHandleTime = 40d
				});
			}
			return skillStats;
		}

		private IList<SkillIntervalStatistics> createSkillDaysYesterdayTodayTomorrow(ISkill skill, Scenario scenario, DateTime userNow, DateTime latestStatsTime)
		{
			var skillStats = new List<SkillIntervalStatistics>();
			var timePeriod = new TimePeriod(0, 0, 24, 0);
			var skillYesterday = createSkillDay(skill, scenario, userNow.AddDays(-1), timePeriod, false);
			var skillToday = createSkillDay(skill, scenario, userNow, timePeriod, false);
			var skillTomorrow = createSkillDay(skill, scenario, userNow.AddDays(1), timePeriod, false);
			SkillDayRepository.Has(skillYesterday, skillToday, skillTomorrow);
			skillStats.AddRange(createStatistics(skillYesterday, latestStatsTime));
			skillStats.AddRange(createStatistics(skillToday, latestStatsTime));
			skillStats.AddRange(createStatistics(skillTomorrow, latestStatsTime));

			return skillStats;
		}

		private double calculateAverageDeviation(IList<SkillIntervalStatistics> actualCallsStats, ISkillDay skillDay)
		{
			var listdeviationPerInterval = new List<double>();
			var divisionFactor = skillDay.Skill.DefaultResolution / minutesPerInterval;
			foreach (var actualCallsPerInterval in actualCallsStats)
			{
				var actualCalls = actualCallsPerInterval.Calls;
				var forecastedCalls = skillDay.CompleteSkillStaffPeriodCollection
					.Where(x => x.Period.StartDateTime <= TimeZoneHelper.ConvertToUtc(actualCallsPerInterval.StartTime, TimeZone.TimeZone()))
					.Select(t => t.Payload.TaskData.Tasks)
					.Last();
				forecastedCalls = forecastedCalls / divisionFactor;
				var deviationPerInterval = actualCalls / forecastedCalls;
				listdeviationPerInterval.Add(deviationPerInterval);
			}
			var alpha = 0.2d;
			return listdeviationPerInterval.Aggregate((prev, next) => alpha * next + (1 - alpha) * prev);
		}

		private double getActualStaffing(ISkillDay skillDay, IList<SkillIntervalStatistics> actualCalls)
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

		private IEnumerable<SkillStaffingInterval> populateStaffingReadModels(ISkill skill, DateTime scheduledStartTime, DateTime scheduledEndTime, double staffing)
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
												  SkillCombination = new []{skill.Id.GetValueOrDefault()}
				});
			}
			ScheduleForecastSkillReadModelRepository.Persist(skillStaffingIntervals, DateTime.UtcNow);
			SkillCombinationResourceRepository.AddSkillCombinationResource(DateTime.UtcNow, skillCombinationResources);
			return skillStaffingIntervals;
		}

	}
}