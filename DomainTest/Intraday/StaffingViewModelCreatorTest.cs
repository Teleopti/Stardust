using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
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
	public class StaffingViewModelCreatorTest : ISetup
	{
		public IStaffingViewModelCreator Target;
		public FakeScenarioRepository ScenarioRepository;
		public FakeSkillRepository SkillRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;
		public FakeIntradayQueueStatisticsLoader IntradayQueueStatisticsLoader;
		public FakeScheduleForecastSkillReadModelRepository ScheduleForecastSkillReadModelRepository;
		public MutableNow Now;
		public FakeUserTimeZone TimeZone;

		private const int minutesPerInterval = 15;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble(new FakeUserTimeZone(TimeZoneInfo.Utc)).For<IUserTimeZone>();
		}

		[Test]
		public void ShouldUseSpecifiecDateTime()
		{
			TimeZone.IsSweden();
			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, 0);
			SkillRepository.Has(skill);

			var skillDay = StaffingViewModelCreatorTestHelper.CreateSkillDay(skill, scenario, Now.UtcDateTime().AddDays(1), new TimePeriod(8, 0, 8, 30), false, ServiceAgreement.DefaultValues());
			SkillDayRepository.Has(skillDay);

			var userTomorrow = TimeZoneHelper.ConvertFromUtc(Now.UtcDateTime().AddDays(1), TimeZone.TimeZone());
			var userTomorrowDateOnly = new DateOnly(userTomorrow);

			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() }, userTomorrowDateOnly);

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
		public void ShouldReturnForecastedStaffing()
		{
			TimeZone.IsSweden();
			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, 0);
			SkillRepository.Has(skill);

			var skillDay = StaffingViewModelCreatorTestHelper.CreateSkillDay(skill, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false, ServiceAgreement.DefaultValues());
			SkillDayRepository.Has(skillDay);

			var vm = Target.Load(new[] { skill.Id.Value });

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

			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);
			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, 0);
			var skillDay = StaffingViewModelCreatorTestHelper.CreateSkillDay(skill, scenario, userNow, new TimePeriod(8, 0, 8, 30), false, ServiceAgreement.DefaultValues());
			var actualCalls = StaffingViewModelCreatorTestHelper.CreateStatistics(skillDay, latestStatsTime, minutesPerInterval, TimeZone.TimeZone());

			SkillRepository.Has(skill);
			SkillDayRepository.Has(skillDay);
			IntradayQueueStatisticsLoader.Has(actualCalls);

			var vm = Target.Load(new[] { skill.Id.Value });

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

			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);
			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), true, 0);
			var skillDayFriday = StaffingViewModelCreatorTestHelper.CreateSkillDay(skill, scenario, userNowFriday, new TimePeriod(8, 0, 8, 30), false, ServiceAgreement.DefaultValues());
			var skillDaySaturday = StaffingViewModelCreatorTestHelper.CreateSkillDay(skill, scenario, userNowFriday.AddDays(1), new TimePeriod(), false, ServiceAgreement.DefaultValues());
			var actualCalls = StaffingViewModelCreatorTestHelper.CreateStatistics(skillDayFriday, latestStatsTimeFriday, minutesPerInterval, TimeZone.TimeZone());

			SkillRepository.Has(skill);
			SkillDayRepository.Has(skillDayFriday);
			SkillDayRepository.Has(skillDaySaturday);
			IntradayQueueStatisticsLoader.Has(actualCalls);

			var vm = Target.Load(new[] { skill.Id.Value });

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

			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);
			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, 0);
			var skillDay = StaffingViewModelCreatorTestHelper.CreateSkillDay(skill, scenario, userNow, new TimePeriod(8, 0, 8, 30), false, ServiceAgreement.DefaultValues());
			skillDay.WorkloadDayCollection.First().CampaignTasks = new Percent(-0.5);
			var actualCalls = new List<SkillIntervalStatistics>()
				{
					 new SkillIntervalStatistics
					 {
						  SkillId = skill.Id.Value,
						  StartTime = TimeZoneHelper.ConvertFromUtc(latestStatsTime, TimeZone.TimeZone()),
						  Calls = skillDay.WorkloadDayCollection.First().TaskPeriodList.First().Tasks,
						  AverageHandleTime = 40d
					 }
				};

			SkillRepository.Has(skill);
			SkillDayRepository.Has(skillDay);
			IntradayQueueStatisticsLoader.Has(actualCalls);

			var vm = Target.Load(new[] { skill.Id.Value });

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

			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, 0);
			SkillRepository.Has(skill);

			var skillDay = StaffingViewModelCreatorTestHelper.CreateSkillDay(skill, scenario, userNow, new TimePeriod(8, 0, 8, 30), false, ServiceAgreement.DefaultValues());
			SkillDayRepository.Has(skillDay);

			IntradayQueueStatisticsLoader.Has(StaffingViewModelCreatorTestHelper.CreateStatistics(skillDay, latestStatsTime, minutesPerInterval, TimeZone.TimeZone()));

			var vm = Target.Load(new[] { skill.Id.Value });

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
			StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);

			var skill = StaffingViewModelCreatorTestHelper.CreateEmailSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30));
			SkillRepository.Has(skill);

			var vm = Target.Load(new[] { skill.Id.Value });

			vm.DataSeries.Should().Be.EqualTo(null);
			vm.StaffingHasData.Should().Be.EqualTo(false);
		}

		[Test]
		public void ShouldReturnStaffingCorrectlyWhenViewingBeforeSkillIsOpen()
		{
			var userNow = new DateTime(2016, 8, 26, 7, 45, 0, DateTimeKind.Utc);
			var latestStatsTime = new DateTime(2016, 8, 26, 7, 0, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, 0);
			SkillRepository.Has(skill);

			var skillDay = StaffingViewModelCreatorTestHelper.CreateSkillDay(skill, scenario, userNow, new TimePeriod(8, 0, 8, 30), false, ServiceAgreement.DefaultValues());
			SkillDayRepository.Has(skillDay);

			IntradayQueueStatisticsLoader.Has(StaffingViewModelCreatorTestHelper.CreateStatistics(skillDay, latestStatsTime, minutesPerInterval, TimeZone.TimeZone()));

			var vm = Target.Load(new[] { skill.Id.Value });

			vm.DataSeries.ForecastedStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.UpdatedForecastedStaffing.Length.Should().Be.EqualTo(0);
			vm.DataSeries.ActualStaffing.Length.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnUpdatedStaffingForecastCorrectlyWithMoreThanOneWorkload()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, 0);
			SkillRepository.Has(skill);

			var skillDay = StaffingViewModelCreatorTestHelper.CreateSkillDay(skill, scenario, userNow, new TimePeriod(8, 0, 8, 30), false, ServiceAgreement.DefaultValues(), false);

			IWorkload workload = new Workload(skill);
			workload.SetId(Guid.NewGuid());
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
					SkillId = skill.Id.Value,
					WorkloadId = skillDay.WorkloadDayCollection.First().Workload.Id.Value,
					StartTime = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc),
					Calls = actualCallsPerSkill * 0.6d,
					AverageHandleTime = 40d
				},
				new SkillIntervalStatistics
				{
					SkillId = skill.Id.Value,
					WorkloadId = skillDay.WorkloadDayCollection.Last().Workload.Id.Value,
					StartTime = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc),
					Calls = actualCallsPerSkill * 0.6d,
					AverageHandleTime = 50d
				}
			};

			IntradayQueueStatisticsLoader.Has(actualIntervals);

			var vm = Target.Load(new[] { skill.Id.Value });

			vm.DataSeries.UpdatedForecastedStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.UpdatedForecastedStaffing.First().Should().Be.EqualTo(null);
			var expectedUpdatedForecastOnLastInterval = vm.DataSeries.ForecastedStaffing.Last() * 1.2d;
			vm.DataSeries.UpdatedForecastedStaffing.Last().Should().Be.EqualTo(expectedUpdatedForecastOnLastInterval);
			vm.DataSeries.ForecastedStaffing.First().Should().Be.EqualTo(skillDay.SkillStaffPeriodCollection.First().FStaff);
		}

		[Test]
		public void ShouldReturnActualStaffingSameAsForecastedIfSameWorkLoad()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);
			var skill = createSkill(15, "skill", new TimePeriod(8, 0, 8, 30), false, 0);
			var skillDay = StaffingViewModelCreatorTestHelper.CreateSkillDay(skill, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false, ServiceAgreement.DefaultValues(), false);

			var actualCalls = StaffingViewModelCreatorTestHelper.CreateStatisticsBasedOnForecastedTasks(skillDay, latestStatsTime, minutesPerInterval, TimeZone.TimeZone());

			SkillRepository.Has(skill);
			IntradayQueueStatisticsLoader.Has(actualCalls);
			SkillDayRepository.Has(skillDay);

			var vm = Target.Load(new[] { skill.Id.Value });

			Math.Round((double)vm.DataSeries.ActualStaffing.First(), 3)
				.Should().Be.EqualTo(Math.Round((double)vm.DataSeries.ForecastedStaffing.First(),3));
			vm.DataSeries.ActualStaffing.Last().Should().Be.EqualTo(null);
		}

		[Test]
		public void ShouldSkipZeroForecastedCallIntervalsWhenCalculatingUpdatedForecast()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 30, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			var openHours = new TimePeriod(8, 0, 8, 45);
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);

			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);
			var skill = createSkill(minutesPerInterval, "skill", openHours, false, 0);
			var skillDay = skill.CreateSkillDayWithDemandOnInterval(scenario, new DateOnly(userNow), 1, ServiceAgreement.DefaultValues(), new Tuple<TimePeriod, double>(openHours, 1)).WithId();
			skillDay.WorkloadDayCollection.First().TaskPeriodList[0].Tasks = 0.00008;
			skillDay.WorkloadDayCollection.First().TaskPeriodList[1].Tasks = 2;
			skillDay.WorkloadDayCollection.Last().TaskPeriodList[2].Tasks = 2;

			var actualIntervals = StaffingViewModelCreatorTestHelper.CreateStatistics(skillDay, latestStatsTime, minutesPerInterval, TimeZone.TimeZone());

			SkillRepository.Has(skill);
			SkillDayRepository.Has(skillDay);
			IntradayQueueStatisticsLoader.Has(actualIntervals);

			var vm = Target.Load(new[] { skill.Id.Value });

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

			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(0, 0, 24, 0), false, 0);
			SkillRepository.Has(skill);

			IntradayQueueStatisticsLoader.Has(createSkillDaysYesterdayTodayTomorrow(skill, scenario, userNow, TimeZoneHelper.ConvertToUtc(latestStatsTimeLocal, TimeZone.TimeZone())));

			var vm = Target.Load(new[] { skill.Id.Value });

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

			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(0, 0, 24, 0), false, 0);
			SkillRepository.Has(skill);

			IntradayQueueStatisticsLoader.Has(createSkillDaysYesterdayTodayTomorrow(skill, scenario, userNow, TimeZoneHelper.ConvertToUtc(latestStatsTimeLocal, TimeZone.TimeZone())));

			var vm = Target.Load(new[] { skill.Id.Value });

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

			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);
			var skill = createSkill(30, "skill", new TimePeriod(8, 0, 9, 0), false, 0);
			var skillDay = StaffingViewModelCreatorTestHelper.CreateSkillDay(skill, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 9, 0), false, ServiceAgreement.DefaultValues(), false);

			var actualCalls = StaffingViewModelCreatorTestHelper.CreateStatistics(skillDay, latestStatsTime, minutesPerInterval, TimeZone.TimeZone());

			SkillRepository.Has(skill);
			IntradayQueueStatisticsLoader.Has(actualCalls);
			SkillDayRepository.Has(skillDay);

			var actualStaffing = getActualStaffing(skillDay, actualCalls, skill.DefaultResolution);
			var averageDeviation = calculateAverageDeviation(actualCalls, skillDay);

			var vm = Target.Load(new[] { skill.Id.Value });

			vm.DataSeries.Time.Length.Should().Be.EqualTo(4);
			vm.DataSeries.ForecastedStaffing.Length.Should().Be.EqualTo(4);
			vm.DataSeries.UpdatedForecastedStaffing.Length.Should().Be.EqualTo(4);
			vm.DataSeries.UpdatedForecastedStaffing.First().Should().Be.EqualTo(null);
			vm.DataSeries.UpdatedForecastedStaffing.Last().Should().Be.EqualTo(vm.DataSeries.ForecastedStaffing.Last() * averageDeviation);
			vm.DataSeries.ActualStaffing.Length.Should().Be.EqualTo(4);

			Assert.AreEqual(actualStaffing, vm.DataSeries.ActualStaffing.First(), 0.001);
			Assert.AreEqual(actualStaffing, vm.DataSeries.ActualStaffing.ElementAt(1), 0.001);
		}

		[Test]
		public void ShouldHandleActualStaffingWithIncompleteSkillIntervalStats()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 30, 0, DateTimeKind.Utc);
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);
			var skill = createSkill(30, "skill", new TimePeriod(8, 0, 9, 0), false, 0);
			var skillDay = StaffingViewModelCreatorTestHelper.CreateSkillDay(skill, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 9, 0), false, ServiceAgreement.DefaultValues(), false);

			var actualCalls = StaffingViewModelCreatorTestHelper.CreateStatistics(skillDay, latestStatsTime, minutesPerInterval, TimeZone.TimeZone());

			SkillRepository.Has(skill);
			IntradayQueueStatisticsLoader.Has(actualCalls);
			SkillDayRepository.Has(skillDay);

			var actualStaffing = getActualStaffing(skillDay, 
				new List<SkillIntervalStatistics>() {new SkillIntervalStatistics()
				{
					SkillId = actualCalls.First().SkillId,
					Calls = actualCalls.First().Calls*2,
					AnsweredCalls = actualCalls.First().AnsweredCalls*2,
					HandleTime = actualCalls.First().HandleTime*2,
					StartTime = actualCalls.First().StartTime,
					WorkloadId = actualCalls.First().WorkloadId
				}}, 
				skill.DefaultResolution);

			var vm = Target.Load(new[] { skill.Id.Value });

			vm.DataSeries.Time.Length.Should().Be.EqualTo(4);
			vm.DataSeries.ActualStaffing.Length.Should().Be.EqualTo(4);
			Math.Round((double)vm.DataSeries.ActualStaffing.First(), 3).Should().Be.EqualTo(Math.Round(actualStaffing, 3));
			vm.DataSeries.ActualStaffing[1].Should().Be.EqualTo(null);
		}


		[Test]
		public void ShouldHandleRequiredStaffingWithDifferentSkillResolutions()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);

			var skill15 = createSkill(minutesPerInterval, "skill1", new TimePeriod(8, 0, 8, 30), false, 0);
			var skill30 = createSkill(2*minutesPerInterval, "skill2", new TimePeriod(8, 0, 9, 0), false, 0);

			var skillDay15 = StaffingViewModelCreatorTestHelper.CreateSkillDay(skill15, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false, ServiceAgreement.DefaultValues(), false);
			var skillDay30 = StaffingViewModelCreatorTestHelper.CreateSkillDay(skill30, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 9, 0), false, ServiceAgreement.DefaultValues(), false);

			var actualCalls15 = StaffingViewModelCreatorTestHelper.CreateStatistics(skillDay15, latestStatsTime, minutesPerInterval, TimeZone.TimeZone());
			var actualCalls30 = StaffingViewModelCreatorTestHelper.CreateStatistics(skillDay30, latestStatsTime, minutesPerInterval, TimeZone.TimeZone());

			var actualCallsTotal = new List<SkillIntervalStatistics>();
			actualCallsTotal.AddRange(actualCalls15);
			actualCallsTotal.AddRange(actualCalls30);

			var actualStaffing15 = getActualStaffing(skillDay15, actualCalls15, skill15.DefaultResolution);
			var actualStaffing30 = getActualStaffing(skillDay30,
				new List<SkillIntervalStatistics>() {new SkillIntervalStatistics()
				{
					SkillId = actualCalls30.First().SkillId,
					Calls = actualCalls30.First().Calls*2,
					AnsweredCalls = actualCalls30.First().AnsweredCalls*2,
					HandleTime = actualCalls30.First().HandleTime*2,
					StartTime = actualCalls30.First().StartTime,
					WorkloadId = actualCalls30.First().WorkloadId
				}},
				skill30.DefaultResolution);

			SkillRepository.Has(skill15, skill30);
			SkillDayRepository.Has(skillDay15, skillDay30);
			IntradayQueueStatisticsLoader.Has(actualCallsTotal);

			var vm = Target.Load(new[] { skill15.Id.Value, skill30.Id.Value });

			vm.DataSeries.ActualStaffing.Length.Should().Be.EqualTo(4);
			Math.Round((double)vm.DataSeries.ActualStaffing.First(),2).
				Should().Be.EqualTo(Math.Round(actualStaffing15 + actualStaffing30,2));
			vm.DataSeries.ActualStaffing[1].Should().Be.EqualTo(null);
			vm.DataSeries.ActualStaffing.Last().Should().Be.EqualTo(null);
		}

		[Test]
		public void ShouldSummariseForecastedStaffingForTwoSkills()
		{
			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);

			var skill1 = createSkill(minutesPerInterval, "skill1", new TimePeriod(8, 0, 8, 30), false, 0);
			var skill2 = createSkill(minutesPerInterval, "skill2", new TimePeriod(8, 0, 8, 30), false, 0);
			SkillRepository.Has(skill1, skill2);

			var skillDay1 = StaffingViewModelCreatorTestHelper.CreateSkillDay(skill1, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false, ServiceAgreement.DefaultValues());
			var skillDay2 = StaffingViewModelCreatorTestHelper.CreateSkillDay(skill2, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false, ServiceAgreement.DefaultValues());
			SkillDayRepository.Has(skillDay1, skillDay2);

			var vm = Target.Load(new[] { skill1.Id.Value, skill2.Id.Value });

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

			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);
			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 45), false, 0);
			var skillDay = StaffingViewModelCreatorTestHelper.CreateSkillDay(skill, scenario, userNow, new TimePeriod(8, 0, 8, 45), false, ServiceAgreement.DefaultValues());

			SkillRepository.Has(skill);
			SkillDayRepository.Has(skillDay);
			IntradayQueueStatisticsLoader.Has(new List<SkillIntervalStatistics>
			{
				new SkillIntervalStatistics { SkillId = skill.Id.Value, StartTime = latestStatsTime, Calls = 30 }
			});

			var vm = Target.Load(new[] { skill.Id.Value });

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

			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);
			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 45), false, 0);
			var skillDay = StaffingViewModelCreatorTestHelper.CreateSkillDay(skill, scenario, userNow, new TimePeriod(8, 0, 8, 45), false, ServiceAgreement.DefaultValues());

			SkillRepository.Has(skill);
			SkillDayRepository.Has(skillDay);

			var vm = Target.Load(new[] { skill.Id.Value });

			vm.DataSeries.UpdatedForecastedStaffing.Should().Be.Empty();
			vm.DataSeries.ActualStaffing.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotShowAnyStaffingDataWhenNoForecast()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);

			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, 0);

			SkillRepository.Has(skill);
			IntradayQueueStatisticsLoader.Has(new List<SkillIntervalStatistics>
			  {
				  new SkillIntervalStatistics {SkillId = skill.Id.Value, StartTime = latestStatsTime, Calls = 30}
			  });

			var vm = Target.Load(new[] { skill.Id.Value });

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
			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);

			var skillWithReforecast1 = createSkill(minutesPerInterval, "skill with reforecast 1", new TimePeriod(7, 0, 8, 15), false, 0);
			var skillWithReforecast2 = createSkill(minutesPerInterval, "skill with reforecast 2", new TimePeriod(7, 0, 8, 15), false, 0);
			var skillWithoutReforecast = createSkill(minutesPerInterval, "skill without reforecast", new TimePeriod(7, 45, 8, 15), false, 0);

			var skillDayWithReforecast1 = StaffingViewModelCreatorTestHelper.CreateSkillDay(skillWithReforecast1, scenario, Now.UtcDateTime(), new TimePeriod(7, 0, 8, 15), false, ServiceAgreement.DefaultValues());
			var skillDayWithReforecast2 = StaffingViewModelCreatorTestHelper.CreateSkillDay(skillWithReforecast2, scenario, Now.UtcDateTime(), new TimePeriod(7, 0, 8, 15), false, ServiceAgreement.DefaultValues());
			var skillDayWithoutReforecast = StaffingViewModelCreatorTestHelper.CreateSkillDay(skillWithoutReforecast, scenario, Now.UtcDateTime(), new TimePeriod(7, 45, 8, 15), false, ServiceAgreement.DefaultValues());

			var actualCalls1 = StaffingViewModelCreatorTestHelper.CreateStatistics(skillDayWithReforecast1, latestStatsTime, minutesPerInterval, TimeZone.TimeZone());
			var actualCalls2 = StaffingViewModelCreatorTestHelper.CreateStatistics(skillDayWithReforecast2, latestStatsTime, minutesPerInterval, TimeZone.TimeZone());

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

			var vm = Target.Load(new[] { skillWithReforecast1.Id.Value, skillWithReforecast2.Id.Value, skillWithoutReforecast.Id.Value });

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

			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);
			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, 0);
			var skillDay = StaffingViewModelCreatorTestHelper.CreateSkillDay(skill, scenario, userNow, new TimePeriod(8, 0, 8, 30), false, ServiceAgreement.DefaultValues());
			var skillStats = new List<SkillIntervalStatistics>
				{
					 new SkillIntervalStatistics
					 {
						  SkillId = skill.Id.Value,
						  StartTime = latestStatsTime.AddMinutes(-minutesPerInterval),
						  Calls = 123,
						  AverageHandleTime = 40d
					 }
				};
			skillStats.AddRange(StaffingViewModelCreatorTestHelper.CreateStatistics(skillDay, latestStatsTime, minutesPerInterval, TimeZone.TimeZone()));

			SkillDayRepository.Has(skillDay);
			SkillRepository.Has(skill);
			IntradayQueueStatisticsLoader.Has(skillStats);

			var vm = Target.Load(new[] { skill.Id.Value });

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

			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);
			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 45), false, 0);
			var skillDay = StaffingViewModelCreatorTestHelper.CreateSkillDay(skill, scenario, userNow, new TimePeriod(8, 0, 8, 45), false, ServiceAgreement.DefaultValues());
			var skillStats = new List<SkillIntervalStatistics>
				{
					 new SkillIntervalStatistics
					 {
						  SkillId = skill.Id.Value,
						  StartTime = latestStatsTime,
						  Calls = 123,
						  AverageHandleTime = 40d
					 }
				};

			SkillRepository.Has(skill);
			SkillDayRepository.Has(skillDay);
			IntradayQueueStatisticsLoader.Has(skillStats);

			var vm = Target.Load(new[] { skill.Id.Value });

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

			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);
			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(0, 0, 0, 30), false, 0);
			var skillDay = StaffingViewModelCreatorTestHelper.CreateSkillDay(skill, scenario, userNow, new TimePeriod(0, 0, 0, 30), true, ServiceAgreement.DefaultValues());
			var skillStats = new List<SkillIntervalStatistics>
				{
					 new SkillIntervalStatistics
					 {
						  SkillId = skill.Id.Value,
						  StartTime = latestStatsTime,
						  Calls = 123,
						  AverageHandleTime = 40d
					 }
				};

			SkillRepository.Has(skill);
			SkillDayRepository.Has(skillDay);
			IntradayQueueStatisticsLoader.Has(skillStats);

			var vm = Target.Load(new[] { skill.Id.Value });

			vm.DataSeries.ActualStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.ActualStaffing.First().Should().Be.GreaterThan(0d);
			vm.DataSeries.ActualStaffing.Last().Should().Be.EqualTo(null);
		}

		[Test]
		public void ShouldReturnActualStaffingCorrectlyWithMidnightBreakGaps()
		{
			var userNow = new DateTime(2016, 8, 26, 5, 30, 0, DateTimeKind.Utc);
			var latestStatsTime = new DateTime(2016, 8, 26, 5, 30, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);
			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(5, 0, 25, 0), false, 1);
			var skillDayYesterday = StaffingViewModelCreatorTestHelper.CreateSkillDay(skill, scenario, userNow.AddDays(-1), new TimePeriod(5, 0, 25, 0), false, ServiceAgreement.DefaultValues());
			var skillDayToday = StaffingViewModelCreatorTestHelper.CreateSkillDay(skill, scenario, userNow, new TimePeriod(5, 0, 25, 0), false, ServiceAgreement.DefaultValues());
			List<SkillIntervalStatistics> skillStats =
				StaffingViewModelCreatorTestHelper.CreateStatistics(skillDayYesterday, skillDayYesterday.SkillStaffPeriodCollection.Last().Period.EndDateTime, minutesPerInterval, TimeZone.TimeZone()).ToList();
			skillStats.AddRange(StaffingViewModelCreatorTestHelper.CreateStatistics(skillDayToday, latestStatsTime, minutesPerInterval, TimeZone.TimeZone()).ToList());

			SkillRepository.Has(skill);
			SkillDayRepository.Has(skillDayYesterday, skillDayToday);
			IntradayQueueStatisticsLoader.Has(skillStats);

			var vm = Target.Load(new[] { skill.Id.Value });

			vm.DataSeries.ActualStaffing.Length.Should().Be.EqualTo(96);
			vm.DataSeries.ActualStaffing[0].HasValue.Should().Be.EqualTo(true);
			vm.DataSeries.ActualStaffing[1].HasValue.Should().Be.EqualTo(true);
			vm.DataSeries.ActualStaffing[2].HasValue.Should().Be.EqualTo(true);
			vm.DataSeries.ActualStaffing[3].HasValue.Should().Be.EqualTo(true);
			vm.DataSeries.ActualStaffing[4].HasValue.Should().Be.EqualTo(false);
			vm.DataSeries.ActualStaffing[20].HasValue.Should().Be.EqualTo(true);
			vm.DataSeries.ActualStaffing[21].HasValue.Should().Be.EqualTo(true);
			vm.DataSeries.ActualStaffing[22].HasValue.Should().Be.EqualTo(true);
			vm.DataSeries.ActualStaffing[23].HasValue.Should().Be.EqualTo(false);
		}

		[Test]
		public void ShouldSummariseActualStaffingForTwoSkills()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);

			var skill1 = createSkill(minutesPerInterval, "skill1", new TimePeriod(8, 0, 8, 30), false, 0);
			var skill2 = createSkill(minutesPerInterval, "skill2", new TimePeriod(8, 0, 8, 30), false, 0);

			var skillDay1 = StaffingViewModelCreatorTestHelper.CreateSkillDay(skill1, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false, ServiceAgreement.DefaultValues(),false);
			var skillDay2 = StaffingViewModelCreatorTestHelper.CreateSkillDay(skill2, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false, ServiceAgreement.DefaultValues(),false);

			var actualCalls1 = StaffingViewModelCreatorTestHelper.CreateStatistics(skillDay1, latestStatsTime, minutesPerInterval, TimeZone.TimeZone());
			var actualCalls2 = StaffingViewModelCreatorTestHelper.CreateStatistics(skillDay2, latestStatsTime, minutesPerInterval, TimeZone.TimeZone());

			var actualCallsTotal = new List<SkillIntervalStatistics>();
			actualCallsTotal.AddRange(actualCalls1);
			actualCallsTotal.AddRange(actualCalls2);

			SkillRepository.Has(skill1, skill2);
			SkillDayRepository.Has(skillDay1, skillDay2);
			IntradayQueueStatisticsLoader.Has(actualCallsTotal);

			var actualStaffingSkill1 = getActualStaffing(skillDay1, actualCalls1, skill1.DefaultResolution);
			var actualStaffingSkill2 = getActualStaffing(skillDay2, actualCalls2, skill2.DefaultResolution);

			var vm = Target.Load(new[] { skill1.Id.Value, skill2.Id.Value });

			vm.DataSeries.ActualStaffing.Length.Should().Be.EqualTo(2);
			Math.Round((double)vm.DataSeries.ActualStaffing.First(), 2).Should().Be.EqualTo(Math.Round((double)(actualStaffingSkill1 + actualStaffingSkill2), 2));
			vm.DataSeries.ActualStaffing.Last().Should().Be.EqualTo(null);
		}

		[Test]
		public void ShouldExcludeUnsupportedSkillsForStaffingCalculation()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);
			var phoneSkill = createSkill(minutesPerInterval, "skill1", new TimePeriod(8, 0, 8, 30), false, 0);
			var emailSkill = StaffingViewModelCreatorTestHelper.CreateEmailSkill(minutesPerInterval, "skill2", new TimePeriod(8, 0, 8, 30));

			var skillDayPhone = StaffingViewModelCreatorTestHelper.CreateSkillDay(phoneSkill, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false, ServiceAgreement.DefaultValues(), false);
			var skillDayEmail = StaffingViewModelCreatorTestHelper.CreateSkillDay(emailSkill, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false, ServiceAgreement.DefaultValues());

			var actualPhoneStats = StaffingViewModelCreatorTestHelper.CreateStatistics(skillDayPhone, latestStatsTime, minutesPerInterval, TimeZone.TimeZone());
			var actualEmailStats = StaffingViewModelCreatorTestHelper.CreateStatistics(skillDayEmail, latestStatsTime, minutesPerInterval, TimeZone.TimeZone());

			var actualStatsTotal = new List<SkillIntervalStatistics>();
			actualStatsTotal.AddRange(actualPhoneStats);
			actualStatsTotal.AddRange(actualEmailStats);

			SkillRepository.Has(phoneSkill, emailSkill);
			SkillDayRepository.Has(skillDayPhone, skillDayEmail);
			IntradayQueueStatisticsLoader.Has(actualStatsTotal);

			var actualStaffingSkill1 = getActualStaffing(skillDayPhone, actualPhoneStats, phoneSkill.DefaultResolution);
			var forecastedStaffing = skillDayPhone.SkillStaffPeriodCollection.First().FStaff;

			var vm = Target.Load(new[] { phoneSkill.Id.Value, emailSkill.Id.Value });

			var deviationWithReforecast = calculateAverageDeviation(actualPhoneStats, skillDayPhone);

			vm.DataSeries.ActualStaffing.First().Should().Be.EqualTo(Math.Round((double)actualStaffingSkill1, 3));
			vm.DataSeries.ActualStaffing.Last().Should().Be.EqualTo(null);
			vm.DataSeries.ForecastedStaffing.First().Should().Be.EqualTo(skillDayPhone.SkillStaffPeriodCollection.First().FStaff);
			vm.DataSeries.UpdatedForecastedStaffing.Last().Should().Be.EqualTo(vm.DataSeries.ForecastedStaffing.Last() * deviationWithReforecast);
		}

		[Test]
		public void ShouldReturnScheduledStaffing()
		{
			TimeZone.IsSweden();
			StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);
			var skill = createSkill(minutesPerInterval, "skill1", new TimePeriod(8, 0, 8, 30), false, 0);
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var scheduledStaffingList = new List<SkillStaffingInterval>
			{
				new SkillStaffingInterval {
					SkillId = skill.Id.Value,
					StartDateTime = userNow,
					EndDateTime = userNow.AddMinutes(minutesPerInterval),
					StaffingLevel = 5.7d
				}
			};

			SkillRepository.Has(skill);
			ScheduleForecastSkillReadModelRepository.Persist(scheduledStaffingList, DateTime.MinValue);

			var vm = Target.Load(new[] { skill.Id.Value });

			vm.DataSeries.Time.Length.Should().Be.EqualTo(1);
			vm.DataSeries.Time.First().Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(scheduledStaffingList.First().StartDateTime, TimeZone.TimeZone()));
			vm.DataSeries.ScheduledStaffing.Length.Should().Be.EqualTo(1);
			vm.DataSeries.ScheduledStaffing.First().Should().Be.EqualTo(scheduledStaffingList.First().StaffingLevel);
		}

		[Test]
		public void ShouldReturnScheduledStaffingForTwoSkills()
		{
			TimeZone.IsSweden();
			StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);
			var skill1 = createSkill(minutesPerInterval, "skill1", new TimePeriod(8, 0, 8, 30), false, 0);
			var skill2 = createSkill(minutesPerInterval, "skill2", new TimePeriod(8, 0, 8, 30), false, 0);
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var scheduledStaffingList = new List<SkillStaffingInterval>
			{
				new SkillStaffingInterval {
					SkillId = skill1.Id.Value,
					StartDateTime = userNow,
					EndDateTime = userNow.AddMinutes(minutesPerInterval),
					StaffingLevel = 5.7d
				},
				new SkillStaffingInterval {
					SkillId = skill2.Id.Value,
					StartDateTime = userNow,
					EndDateTime = userNow.AddMinutes(minutesPerInterval),
					StaffingLevel = 7.7d
				}
			};

			SkillRepository.Has(skill1, skill2);
			ScheduleForecastSkillReadModelRepository.Persist(scheduledStaffingList, DateTime.MinValue);

			var vm = Target.Load(new[] { skill1.Id.Value, skill2.Id.Value });

			vm.DataSeries.Time.Length.Should().Be.EqualTo(1);
			vm.DataSeries.Time.First().Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(scheduledStaffingList.First().StartDateTime, TimeZone.TimeZone()));
			vm.DataSeries.ScheduledStaffing.Length.Should().Be.EqualTo(1);
			vm.DataSeries.ScheduledStaffing.First().Should().Be.EqualTo(scheduledStaffingList.First().StaffingLevel + scheduledStaffingList.Last().StaffingLevel);
		}

		[Test]
		public void ShouldReturnScheduledStaffingWithShrinkage()
		{
			TimeZone.IsSweden();
			StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);
			var skill = createSkill(minutesPerInterval, "skill1", new TimePeriod(8, 0, 8, 30), false, 0);
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var scheduledStaffingList = new List<SkillStaffingInterval>
			{
				new SkillStaffingInterval {
					SkillId = skill.Id.Value,
					StartDateTime = userNow,
					EndDateTime = userNow.AddMinutes(minutesPerInterval),
					StaffingLevel = 5.7d,
					StaffingLevelWithShrinkage = 6.2d
				}
			};

			SkillRepository.Has(skill);
			ScheduleForecastSkillReadModelRepository.Persist(scheduledStaffingList, DateTime.MinValue);

			var vm = Target.Load(new[] { skill.Id.Value }, null, true);

			vm.DataSeries.Time.Length.Should().Be.EqualTo(1);
			vm.DataSeries.Time.First().Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(scheduledStaffingList.First().StartDateTime, TimeZone.TimeZone()));
			vm.DataSeries.ScheduledStaffing.Length.Should().Be.EqualTo(1);
			vm.DataSeries.ScheduledStaffing.First().Should().Be.EqualTo(scheduledStaffingList.First().StaffingLevelWithShrinkage);
		}

		[Test]
		public void ShouldReturnForecastWithShrinkage()
		{
			TimeZone.IsSweden();
			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);
			var skill = createSkill(minutesPerInterval, "skill1", new TimePeriod(8, 0, 8, 30), false, 0);
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			SkillRepository.Has(skill);
			var skillday = skill.CreateSkillDayWithDemand(scenario, new DateOnly(userNow), 1);
			skillday.SkillDataPeriodCollection.ForEach(x => x.Shrinkage = new Percent(0.5));
			SkillDayRepository.Has(skillday);

			var vm = Target.Load(new[] { skill.Id.Value }, null, true);
			
			vm.DataSeries.ForecastedStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.ForecastedStaffing.First().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldReturnScheduledStaffingWithShrinkageSplit()
		{
			TimeZone.IsSweden();
			StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);
			var skill = createSkill(minutesPerInterval, "skill1", new TimePeriod(8, 0, 8, 30), false, 0);
			skill.DefaultResolution = minutesPerInterval*2;
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var scheduledStaffingList = new List<SkillStaffingInterval>
			{
				new SkillStaffingInterval {
					SkillId = skill.Id.Value,
					StartDateTime = userNow,
					EndDateTime = userNow.AddMinutes(minutesPerInterval*2),
					StaffingLevel = 5.7d,
					StaffingLevelWithShrinkage = 6.2d
				}
			};

			SkillRepository.Has(skill);
			ScheduleForecastSkillReadModelRepository.Persist(scheduledStaffingList, DateTime.MinValue);

			var vm = Target.Load(new[] { skill.Id.Value }, null, true);

			vm.DataSeries.Time.Length.Should().Be.EqualTo(2);
			vm.DataSeries.Time.First().Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(scheduledStaffingList.First().StartDateTime, TimeZone.TimeZone()));
			vm.DataSeries.ScheduledStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.ScheduledStaffing.First().Should().Be.EqualTo(scheduledStaffingList.First().StaffingLevelWithShrinkage);
			vm.DataSeries.ScheduledStaffing.Last().Should().Be.EqualTo(scheduledStaffingList.First().StaffingLevelWithShrinkage);
		}

		[Test]
		public void ShouldReturnScheduledStaffingStartingBeforeForecasted()
		{
			TimeZone.IsSweden();
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			var scheduledStartTime = new DateTime(2016, 8, 26, 7, 45, 0, DateTimeKind.Utc);
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);

			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, 0);
			var skillDay = StaffingViewModelCreatorTestHelper.CreateSkillDay(skill, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false, ServiceAgreement.DefaultValues());

			SkillRepository.Has(skill);
			SkillDayRepository.Has(skillDay);

			var skillStats = new List<SkillIntervalStatistics>
			{
				new SkillIntervalStatistics
				{
					SkillId = skill.Id.Value,
					StartTime = TimeZoneHelper.ConvertFromUtc(latestStatsTime, TimeZone.TimeZone()),
					Calls = 123,
					AverageHandleTime = 40d
				}
			};

			IntradayQueueStatisticsLoader.Has(skillStats);

			var scheduledStaffingList = StaffingViewModelCreatorTestHelper.CreateScheduledStaffing(skill, scheduledStartTime, scheduledStartTime.AddMinutes(3 * minutesPerInterval), minutesPerInterval);

			ScheduleForecastSkillReadModelRepository.Persist(scheduledStaffingList, DateTime.MinValue);

			var vm = Target.Load(new[] { skill.Id.Value });

			vm.DataSeries.Time.Length.Should().Be.EqualTo(3);
			vm.DataSeries.Time.First().Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(scheduledStaffingList.First().StartDateTime, TimeZone.TimeZone()));
			vm.DataSeries.ScheduledStaffing.Length.Should().Be.EqualTo(3);
			vm.DataSeries.ScheduledStaffing.First().Should().Be.EqualTo(scheduledStaffingList.First().StaffingLevel);
			vm.DataSeries.ScheduledStaffing.Last().Should().Be.EqualTo(scheduledStaffingList.Last().StaffingLevel);
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

			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, 0);
			var skillDay = StaffingViewModelCreatorTestHelper.CreateSkillDay(skill, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false, ServiceAgreement.DefaultValues(), false);

			SkillRepository.Has(skill);
			SkillDayRepository.Has(skillDay);
			IntradayQueueStatisticsLoader.Has(StaffingViewModelCreatorTestHelper.CreateStatistics(skillDay, latestStatsTime, minutesPerInterval, TimeZone.TimeZone()));

			var scheduledStaffingList = StaffingViewModelCreatorTestHelper.CreateScheduledStaffing(skill, scheduledStartTime, scheduledStartTime.AddMinutes(3 * minutesPerInterval), minutesPerInterval);

			ScheduleForecastSkillReadModelRepository.Persist(scheduledStaffingList, DateTime.MinValue);

			var vm = Target.Load(new[] { skill.Id.Value });

			vm.DataSeries.Time.Length.Should().Be.EqualTo(3);
			vm.DataSeries.Time.First().Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(scheduledStaffingList.First().StartDateTime, TimeZone.TimeZone()));
			vm.DataSeries.ScheduledStaffing.Length.Should().Be.EqualTo(3);
			vm.DataSeries.ScheduledStaffing.First().Should().Be.EqualTo(scheduledStaffingList.First().StaffingLevel);
			vm.DataSeries.ScheduledStaffing.Last().Should().Be.EqualTo(scheduledStaffingList.Last().StaffingLevel);
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

			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, 0);
			var skillDay = StaffingViewModelCreatorTestHelper.CreateSkillDay(skill, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false, ServiceAgreement.DefaultValues());

			SkillRepository.Has(skill);
			SkillDayRepository.Has(skillDay);

			var skillStats = StaffingViewModelCreatorTestHelper.CreateStatistics(skillDay, latestStatsTime, minutesPerInterval, TimeZone.TimeZone());

			IntradayQueueStatisticsLoader.Has(skillStats);

			var scheduledStaffingList = StaffingViewModelCreatorTestHelper.CreateScheduledStaffing(skill, scheduledStartTime, scheduledStartTime.AddMinutes(minutesPerInterval), minutesPerInterval);

			ScheduleForecastSkillReadModelRepository.Persist(scheduledStaffingList, DateTime.MinValue);

			var vm = Target.Load(new[] { skill.Id.Value });

			vm.DataSeries.Time.Length.Should().Be.EqualTo(2);
			vm.DataSeries.Time.First().Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(scheduledStaffingList.First().StartDateTime.AddMinutes(-minutesPerInterval), TimeZone.TimeZone()));
			vm.DataSeries.ScheduledStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.ScheduledStaffing.First().Should().Be.EqualTo(null);
			vm.DataSeries.ScheduledStaffing.Last().Should().Be.EqualTo(scheduledStaffingList.First().StaffingLevel);
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

			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, 0);
			var skillDay = StaffingViewModelCreatorTestHelper.CreateSkillDay(skill, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false, ServiceAgreement.DefaultValues());

			SkillRepository.Has(skill);
			SkillDayRepository.Has(skillDay);

			var skillStats = StaffingViewModelCreatorTestHelper.CreateStatistics(skillDay, latestStatsTime, minutesPerInterval, TimeZone.TimeZone());

			IntradayQueueStatisticsLoader.Has(skillStats);

			var scheduledStaffingList = StaffingViewModelCreatorTestHelper.CreateScheduledStaffing(skill, scheduledStartTime, scheduledStartTime.AddMinutes(minutesPerInterval), minutesPerInterval);

			ScheduleForecastSkillReadModelRepository.Persist(scheduledStaffingList, DateTime.MinValue);

			var vm = Target.Load(new[] { skill.Id.Value });

			vm.DataSeries.Time.Length.Should().Be.EqualTo(2);
			vm.DataSeries.Time.First().Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(scheduledStaffingList.First().StartDateTime, TimeZone.TimeZone()));
			vm.DataSeries.ScheduledStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.ScheduledStaffing.First().Should().Be.GreaterThan(0d);
			vm.DataSeries.ScheduledStaffing.Last().Should().Be.EqualTo(null);
		}

		[Test]
		public void ShouldReturnScheduledStaffingInCorrectOrder()
		{
			TimeZone.IsSweden();
			StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);
			var skill = createSkill(minutesPerInterval, "skill1", new TimePeriod(8, 0, 8, 30), false, 0);
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var scheduledStaffingList = new List<SkillStaffingInterval>
			{
				new SkillStaffingInterval {
					SkillId = skill.Id.Value,
					StartDateTime = userNow,
					EndDateTime = userNow.AddMinutes(minutesPerInterval),
					StaffingLevel = 15d
				},
				new SkillStaffingInterval {
					SkillId = skill.Id.Value,
					StartDateTime = userNow.AddMinutes(-minutesPerInterval),
					EndDateTime = userNow,
					StaffingLevel = 10d
				}
			};

			SkillRepository.Has(skill);
			ScheduleForecastSkillReadModelRepository.Persist(scheduledStaffingList, DateTime.MinValue);

			var vm = Target.Load(new[] { skill.Id.Value });

			vm.DataSeries.Time.Length.Should().Be.EqualTo(2);
			vm.DataSeries.Time.First().Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(scheduledStaffingList.Last().StartDateTime, TimeZone.TimeZone()));
			vm.DataSeries.ScheduledStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.ScheduledStaffing.First().Should().Be.EqualTo(scheduledStaffingList.Last().StaffingLevel);
			vm.DataSeries.ScheduledStaffing.Last().Should().Be.EqualTo(scheduledStaffingList.First().StaffingLevel);
		}

		[Test]
		public void ShouldReturnEmptySeriesForScheduleStaffing()
		{
			TimeZone.IsSweden();
			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, 0);
			SkillRepository.Has(skill);

			var skillDay = StaffingViewModelCreatorTestHelper.CreateSkillDay(skill, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false, ServiceAgreement.DefaultValues());
			SkillDayRepository.Has(skillDay);

			var vm = Target.Load(new[] { skill.Id.Value });

			vm.StaffingHasData.Should().Be.EqualTo(true);
			vm.DataSeries.ScheduledStaffing.IsEmpty().Should().Be.EqualTo(true);
		}
		
		private ISkill createSkill(int intervalLength, string skillName, TimePeriod openHours, bool isClosedOnWeekends, int midnigthBreakOffset)
		{
			var skill =
				new Skill(skillName, skillName, Color.Empty, intervalLength, new SkillTypePhone(new Description("SkillTypeInboundTelephony"), ForecastSource.InboundTelephony))
				{
					TimeZone = TimeZoneInfo.Utc
				}.WithId();
			if (midnigthBreakOffset != 0)
			{
				skill.MidnightBreakOffset = TimeSpan.FromHours(midnigthBreakOffset);
			}

			IWorkload workload;
			if (isClosedOnWeekends)
				workload = WorkloadFactory.CreateWorkloadClosedOnWeekendsWithOpenHours(skill, openHours);
			else
				workload = WorkloadFactory.CreateWorkloadWithOpenHours(skill, openHours);
			workload.SetId(Guid.NewGuid());

			return skill;
		}


		
		private IList<SkillIntervalStatistics> createSkillDaysYesterdayTodayTomorrow(ISkill skill, Scenario scenario, DateTime userNow, DateTime latestStatsTime)
		{
			var skillStats = new List<SkillIntervalStatistics>();
			var timePeriod = new TimePeriod(0, 0, 24, 0);
			var skillYesterday = StaffingViewModelCreatorTestHelper.CreateSkillDay(skill, scenario, userNow.AddDays(-1), timePeriod, false, ServiceAgreement.DefaultValues());
			var skillToday = StaffingViewModelCreatorTestHelper.CreateSkillDay(skill, scenario, userNow, timePeriod, false, ServiceAgreement.DefaultValues());
			var skillTomorrow = StaffingViewModelCreatorTestHelper.CreateSkillDay(skill, scenario, userNow.AddDays(1), timePeriod, false, ServiceAgreement.DefaultValues());
			SkillDayRepository.Has(skillYesterday, skillToday, skillTomorrow);
			skillStats.AddRange(StaffingViewModelCreatorTestHelper.CreateStatistics(skillYesterday, latestStatsTime, minutesPerInterval, TimeZone.TimeZone()));
			skillStats.AddRange(StaffingViewModelCreatorTestHelper.CreateStatistics(skillToday, latestStatsTime, minutesPerInterval, TimeZone.TimeZone()));
			skillStats.AddRange(StaffingViewModelCreatorTestHelper.CreateStatistics(skillTomorrow, latestStatsTime, minutesPerInterval, TimeZone.TimeZone()));

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

		private double getActualStaffing(ISkillDay skillDay, IList<SkillIntervalStatistics> actualCalls, int resolution)
		{
			var calculatorService = new StaffingCalculatorServiceFacade();
			var skillData = skillDay.SkillDataPeriodCollection.First().ServiceAgreement;
			var actualStaffingSkill1 = calculatorService.AgentsUseOccupancy(
				skillData.ServiceLevel.Percent.Value,
				(int)skillData.ServiceLevel.Seconds,
				actualCalls.Sum(x =>x.Calls),
				actualCalls.Sum(x => x.HandleTime) / actualCalls.Sum(x => x.AnsweredCalls),
				TimeSpan.FromMinutes(resolution),
				skillData.MinOccupancy.Value,
				skillData.MaxOccupancy.Value,
				skillDay.Skill.MaxParallelTasks);

			return actualStaffingSkill1;
		}
	}
}