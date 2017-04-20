using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Intraday
{
	[DomainTest]
	[Toggle(Toggles.Staffing_ReadModel_UseSkillCombination_xx), Toggle(Toggles.StaffingActions_RemoveScheduleForecastSkillChangeReadModel_43388)]
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
		public void ShouldUseSpecifiecDateTimePeriod()
		{
			TimeZone.IsSweden();
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			var scheduledStartTime = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var act = ActivityRepository.Has("act");
			var skill = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, act);
			SkillRepository.Has(skill);

			var skillDayToday = SkillSetupHelper.CreateSkillDay(skill, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false);
			var skillDayTomorrow = SkillSetupHelper.CreateSkillDay(skill, scenario, Now.UtcDateTime().AddDays(1), new TimePeriod(8, 0, 8, 30), false);
			SkillDayRepository.Has(skillDayToday, skillDayTomorrow);

			var userToday = TimeZoneHelper.ConvertFromUtc(Now.UtcDateTime(), TimeZone.TimeZone());
			var userTomorrow = TimeZoneHelper.ConvertFromUtc(Now.UtcDateTime().AddDays(1), TimeZone.TimeZone());
			var userDateTimePeriod = new DateOnlyPeriod(new DateOnly(userToday), new DateOnly(userTomorrow));

			SkillSetupHelper.PopulateStaffingReadModels(skill, scheduledStartTime, scheduledStartTime.AddMinutes(minutesPerInterval), 15,ScheduleForecastSkillReadModelRepository, SkillCombinationResourceRepository);
			SkillSetupHelper.PopulateStaffingReadModels(skill, scheduledStartTime.AddMinutes(minutesPerInterval), scheduledStartTime.AddMinutes(minutesPerInterval * 2), 10, ScheduleForecastSkillReadModelRepository, SkillCombinationResourceRepository);

			SkillSetupHelper.PopulateStaffingReadModels(skill, scheduledStartTime.AddDays(1), scheduledStartTime.AddDays(1).AddMinutes(minutesPerInterval), 7, ScheduleForecastSkillReadModelRepository, SkillCombinationResourceRepository);
			SkillSetupHelper.PopulateStaffingReadModels(skill, scheduledStartTime.AddDays(1).AddMinutes(minutesPerInterval), scheduledStartTime.AddDays(1).AddMinutes(minutesPerInterval * 2), 3, ScheduleForecastSkillReadModelRepository, SkillCombinationResourceRepository);

			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() }, userDateTimePeriod).ToList();

			var staffingIntervalsToday = skillDayToday.SkillStaffPeriodViewCollection(TimeSpan.FromMinutes(minutesPerInterval));
			var staffingIntervalsTomorrow = skillDayTomorrow.SkillStaffPeriodViewCollection(TimeSpan.FromMinutes(minutesPerInterval));

			vm.Count.Should().Be.EqualTo(2);

			vm.First().DataSeries.Time.Length.Should().Be.EqualTo(2);
			vm.First().DataSeries.Time.First().Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(staffingIntervalsToday.First().Period.StartDateTime, TimeZone.TimeZone()));
			vm.First().DataSeries.Time.Last().Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(staffingIntervalsToday.Last().Period.StartDateTime, TimeZone.TimeZone()));
			vm.First().DataSeries.ForecastedStaffing.Length.Should().Be.EqualTo(2);
			vm.First().DataSeries.ForecastedStaffing.First().Should().Be.GreaterThan(0d);
			vm.First().DataSeries.ForecastedStaffing.Last().Should().Be.GreaterThan(0d);
			vm.First().DataSeries.ScheduledStaffing.Length.Should().Be.EqualTo(2);
			vm.First().DataSeries.ScheduledStaffing.First().Should().Be.EqualTo(15);
			vm.First().DataSeries.ScheduledStaffing.Last().Should().Be.EqualTo(10);
			vm.First().StaffingHasData.Should().Be.EqualTo(true);

			vm.Second().DataSeries.Time.Length.Should().Be.EqualTo(2);
			vm.Second().DataSeries.Time.First().Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(staffingIntervalsTomorrow.First().Period.StartDateTime, TimeZone.TimeZone()));
			vm.Second().DataSeries.Time.Last().Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(staffingIntervalsTomorrow.Last().Period.StartDateTime, TimeZone.TimeZone()));
			vm.Second().DataSeries.ForecastedStaffing.Length.Should().Be.EqualTo(2);
			vm.Second().DataSeries.ForecastedStaffing.First().Should().Be.GreaterThan(0d);
			vm.Second().DataSeries.ForecastedStaffing.Last().Should().Be.GreaterThan(0d);
			vm.Second().DataSeries.ScheduledStaffing.Length.Should().Be.EqualTo(2);
			vm.Second().DataSeries.ScheduledStaffing.First().Should().Be.EqualTo(7);
			vm.Second().DataSeries.ScheduledStaffing.Last().Should().Be.EqualTo(3);
			vm.Second().StaffingHasData.Should().Be.EqualTo(true);
		}


		[Test]
		public void ShouldUseSpecifiecDateTime()
		{
			TimeZone.IsSweden();
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			var scheduledStartTime = new DateTime(2016, 8, 27, 8, 0, 0, DateTimeKind.Utc);

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var act = ActivityRepository.Has("act");
			var skill = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, act);
			SkillRepository.Has(skill);

			var skillDay = SkillSetupHelper.CreateSkillDay(skill, scenario, Now.UtcDateTime().AddDays(1), new TimePeriod(8, 0, 8, 30), false);
			SkillDayRepository.Has(skillDay);

			var userTomorrow = TimeZoneHelper.ConvertFromUtc(Now.UtcDateTime().AddDays(1), TimeZone.TimeZone());
			var userTomorrowDateOnly = new DateOnly(userTomorrow);

			SkillSetupHelper.PopulateStaffingReadModels(skill, scheduledStartTime, scheduledStartTime.AddMinutes(minutesPerInterval), 15, ScheduleForecastSkillReadModelRepository, SkillCombinationResourceRepository);
			SkillSetupHelper.PopulateStaffingReadModels(skill, scheduledStartTime.AddMinutes(minutesPerInterval), scheduledStartTime.AddMinutes(minutesPerInterval*2), 10, ScheduleForecastSkillReadModelRepository, SkillCombinationResourceRepository);
			
			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() }, userTomorrowDateOnly);

			var staffingIntervals = skillDay.SkillStaffPeriodViewCollection(TimeSpan.FromMinutes(minutesPerInterval));
			vm.DataSeries.Time.Length.Should().Be.EqualTo(2);
			vm.DataSeries.Time.First().Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(staffingIntervals.First().Period.StartDateTime, TimeZone.TimeZone()));
			vm.DataSeries.Time.Last().Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(staffingIntervals.Last().Period.StartDateTime, TimeZone.TimeZone()));
			vm.DataSeries.ForecastedStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.ForecastedStaffing.First().Should().Be.GreaterThan(0d);
			vm.DataSeries.ForecastedStaffing.Last().Should().Be.GreaterThan(0d);
			vm.DataSeries.ScheduledStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.ScheduledStaffing.First().Should().Be.EqualTo(15);
			vm.DataSeries.ScheduledStaffing.Last().Should().Be.EqualTo(10);
			vm.StaffingHasData.Should().Be.EqualTo(true);
		}

		[Test]
		public void ShouldReturnForecastedStaffing()
		{
			TimeZone.IsSweden();
			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher,ScenarioRepository);
			var act = ActivityRepository.Has("act");
			var skill = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, act);
			SkillRepository.Has(skill);

			var skillDay = SkillSetupHelper.CreateSkillDay(skill, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false);
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

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var act = ActivityRepository.Has("act");
			var skill = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, act);
			var skillDay = SkillSetupHelper.CreateSkillDay(skill, scenario, userNow, new TimePeriod(8, 0, 8, 30), false);
			var actualCalls = SkillSetupHelper.CreateStatistics(skillDay, latestStatsTime, TimeZone);

			SkillRepository.Has(skill);
			SkillDayRepository.Has(skillDay);
			IntradayQueueStatisticsLoader.Has(actualCalls);

			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() });

			var deviationWithReforecast = SkillSetupHelper.CalculateAverageDeviation(actualCalls, skillDay, TimeZone);

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

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var act = ActivityRepository.Has("act");
			var skill = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), true, act);
			var skillDayFriday = SkillSetupHelper.CreateSkillDay(skill, scenario, userNowFriday, new TimePeriod(8, 0, 8, 30), false);
			var skillDaySaturday = SkillSetupHelper.CreateSkillDay(skill, scenario, userNowFriday.AddDays(1), new TimePeriod(), false);
			var actualCalls = SkillSetupHelper.CreateStatistics(skillDayFriday, latestStatsTimeFriday, TimeZone);

			SkillRepository.Has(skill);
			SkillDayRepository.Has(skillDayFriday);
			SkillDayRepository.Has(skillDaySaturday);
			IntradayQueueStatisticsLoader.Has(actualCalls);

			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() });

			var deviationWithReforecast = SkillSetupHelper.CalculateAverageDeviation(actualCalls, skillDayFriday, TimeZone);

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

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var act = ActivityRepository.Has("act");
			var skill = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, act);
			var skillDay = SkillSetupHelper.CreateSkillDay(skill, scenario, userNow, new TimePeriod(8, 0, 8, 30), false);
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

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var act = ActivityRepository.Has("act");
			var skill = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, act);
			SkillRepository.Has(skill);

			var skillDay = SkillSetupHelper.CreateSkillDay(skill, scenario, userNow, new TimePeriod(8, 0, 8, 30), false);
			SkillDayRepository.Has(skillDay);

			IntradayQueueStatisticsLoader.Has(SkillSetupHelper.CreateStatistics(skillDay, latestStatsTime, TimeZone));

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
			SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);

			var skill = SkillSetupHelper.CreateEmailSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30));
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

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var act = ActivityRepository.Has("act");
			var skill = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, act);
			SkillRepository.Has(skill);

			var skillDay = SkillSetupHelper.CreateSkillDay(skill, scenario, userNow, new TimePeriod(8, 0, 8, 30), false);
			SkillDayRepository.Has(skillDay);

			IntradayQueueStatisticsLoader.Has(SkillSetupHelper.CreateStatistics(skillDay, latestStatsTime, TimeZone));

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

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var act = ActivityRepository.Has("act");
			var skill = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, act);
			SkillRepository.Has(skill);

			var skillDay = SkillSetupHelper.CreateSkillDay(skill, scenario, userNow, new TimePeriod(8, 0, 8, 30), false);

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

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var act = ActivityRepository.Has("act");
			var skill = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, act);
			var skillDay = SkillSetupHelper.CreateSkillDay(skill, scenario, userNow, new TimePeriod(8, 0, 8, 30), false);
			skillDay.SkillStaffPeriodCollection[0].Payload.Efficiency = new Percent(0.50);
			skillDay.SkillStaffPeriodCollection[1].Payload.Efficiency = new Percent(0.50);

			var efficency = skillDay.SkillStaffPeriodCollection.Select(s => s.Payload.Efficiency).First();
			var actualIntervals = SkillSetupHelper.CreateStatistics(skillDay, latestStatsTime, TimeZone);

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

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var act = ActivityRepository.Has("act");
			var skill = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill", openHours, false, act);
			var skillDay = skill.CreateSkillDayWithDemandOnInterval(scenario, new DateOnly(userNow), 1, new Tuple<TimePeriod, double>(openHours, 1)).WithId();
			skillDay.WorkloadDayCollection.First().TaskPeriodList[0].Tasks = 0.00008;
			skillDay.WorkloadDayCollection.First().TaskPeriodList[1].Tasks = 2;
			skillDay.WorkloadDayCollection.Last().TaskPeriodList[2].Tasks = 2;

			var actualIntervals = SkillSetupHelper.CreateStatistics(skillDay, latestStatsTime, TimeZone);

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

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var act = ActivityRepository.Has("act");
			var skill = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill", new TimePeriod(0, 0, 24, 0), false, act);
			SkillRepository.Has(skill);

			IntradayQueueStatisticsLoader.Has(SkillSetupHelper.CreateSkillDaysYesterdayTodayTomorrow(skill, scenario, userNow,
				TimeZoneHelper.ConvertToUtc(latestStatsTimeLocal, TimeZone.TimeZone()), SkillDayRepository, TimeZone));

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

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var act = ActivityRepository.Has("act");
			var skill = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill", new TimePeriod(0, 0, 24, 0), false, act);
			SkillRepository.Has(skill);

			IntradayQueueStatisticsLoader.Has(SkillSetupHelper.CreateSkillDaysYesterdayTodayTomorrow(skill, scenario, userNow,
				TimeZoneHelper.ConvertToUtc(latestStatsTimeLocal, TimeZone.TimeZone()), SkillDayRepository, TimeZone));

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

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var act = ActivityRepository.Has("act");
			var skill = SkillSetupHelper.CreateSkill(30, "skill", new TimePeriod(8, 0, 9, 0), false, act);
			var skillDay = SkillSetupHelper.CreateSkillDay(skill, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 9, 0), false);

			var actualCalls = SkillSetupHelper.CreateStatistics(skillDay, latestStatsTime, TimeZone);

			SkillRepository.Has(skill);
			IntradayQueueStatisticsLoader.Has(actualCalls);
			SkillDayRepository.Has(skillDay);

			var actualStaffing = SkillSetupHelper.GetActualStaffing(skillDay, actualCalls);
			var averageDeviation = SkillSetupHelper.CalculateAverageDeviation(actualCalls, skillDay,TimeZone);

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
			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var act = ActivityRepository.Has("act");
			var skill1 = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill1", new TimePeriod(8, 0, 8, 30), false, act);
			var skill2 = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill2", new TimePeriod(8, 0, 8, 30), false, act);
			SkillRepository.Has(skill1, skill2);

			var skillDay1 = SkillSetupHelper.CreateSkillDay(skill1, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false);
			var skillDay2 = SkillSetupHelper.CreateSkillDay(skill2, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false);
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

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var act = ActivityRepository.Has("act");
			var skill = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 45), false, act);
			var skillDay = SkillSetupHelper.CreateSkillDay(skill, scenario, userNow, new TimePeriod(8, 0, 8, 45), false);

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

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var act = ActivityRepository.Has("act");
			var skill = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 45), false, act);
			var skillDay = SkillSetupHelper.CreateSkillDay(skill, scenario, userNow, new TimePeriod(8, 0, 8, 45), false);

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
			SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);

			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			var act = ActivityRepository.Has("act");
			var skill = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, act);

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
			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var act = ActivityRepository.Has("act");
			var skillWithReforecast1 = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill with reforecast 1", new TimePeriod(7, 0, 8, 15), false, act);
			var skillWithReforecast2 = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill with reforecast 2", new TimePeriod(7, 0, 8, 15), false, act);
			var skillWithoutReforecast = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill without reforecast", new TimePeriod(7, 45, 8, 15), false, act);

			var skillDayWithReforecast1 = SkillSetupHelper.CreateSkillDay(skillWithReforecast1, scenario, Now.UtcDateTime(), new TimePeriod(7, 0, 8, 15), false);
			var skillDayWithReforecast2 = SkillSetupHelper.CreateSkillDay(skillWithReforecast2, scenario, Now.UtcDateTime(), new TimePeriod(7, 0, 8, 15), false);
			var skillDayWithoutReforecast = SkillSetupHelper.CreateSkillDay(skillWithoutReforecast, scenario, Now.UtcDateTime(), new TimePeriod(7, 45, 8, 15), false);

			var actualCalls1 = SkillSetupHelper.CreateStatistics(skillDayWithReforecast1, latestStatsTime, TimeZone);
			var actualCalls2 = SkillSetupHelper.CreateStatistics(skillDayWithReforecast2, latestStatsTime, TimeZone);

			var actualCallsTotal = new List<SkillIntervalStatistics>();
			actualCallsTotal.AddRange(actualCalls1);
			actualCallsTotal.AddRange(actualCalls2);

			SkillRepository.Has(skillWithReforecast1, skillWithReforecast2, skillWithoutReforecast);
			SkillDayRepository.Has(skillDayWithReforecast1, skillDayWithReforecast2, skillDayWithoutReforecast);
			IntradayQueueStatisticsLoader.Has(actualCallsTotal);

			var deviationWithReforecast1 = SkillSetupHelper.CalculateAverageDeviation(actualCalls1, skillDayWithReforecast1, TimeZone);
			var deviationWithReforecast2 = SkillSetupHelper.CalculateAverageDeviation(actualCalls2, skillDayWithReforecast2, TimeZone);
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

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var act = ActivityRepository.Has("act");
			var skill = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, act);
			var skillDay = SkillSetupHelper.CreateSkillDay(skill, scenario, userNow, new TimePeriod(8, 0, 8, 30), false);
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
			skillStats.AddRange(SkillSetupHelper.CreateStatistics(skillDay, latestStatsTime, TimeZone));

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

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var act = ActivityRepository.Has("act");
			var skill = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 45), false, act);
			var skillDay = SkillSetupHelper.CreateSkillDay(skill, scenario, userNow, new TimePeriod(8, 0, 8, 45), false);
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

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var act = ActivityRepository.Has("act");
			var skill = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill", new TimePeriod(0, 0, 0, 30), false, act);
			var skillDay = SkillSetupHelper.CreateSkillDay(skill, scenario, userNow, new TimePeriod(0, 0, 0, 30), true);
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

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var act = ActivityRepository.Has("act");
			var skill1 = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill1", new TimePeriod(8, 0, 8, 30), false, act);
			var skill2 = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill2", new TimePeriod(8, 0, 8, 30), false, act);

			var skillDay1 = SkillSetupHelper.CreateSkillDay(skill1, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false);
			var skillDay2 = SkillSetupHelper.CreateSkillDay(skill2, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false);

			var actualCalls1 = SkillSetupHelper.CreateStatistics(skillDay1, latestStatsTime, TimeZone);
			var actualCalls2 = SkillSetupHelper.CreateStatistics(skillDay2, latestStatsTime, TimeZone);

			var actualCallsTotal = new List<SkillIntervalStatistics>();
			actualCallsTotal.AddRange(actualCalls1);
			actualCallsTotal.AddRange(actualCalls2);

			SkillRepository.Has(skill1, skill2);
			SkillDayRepository.Has(skillDay1, skillDay2);
			IntradayQueueStatisticsLoader.Has(actualCallsTotal);

			var actualStaffingSkill1 = SkillSetupHelper.GetActualStaffing(skillDay1, actualCalls1);
			var actualStaffingSkill2 = SkillSetupHelper.GetActualStaffing(skillDay2, actualCalls2);

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

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var act = ActivityRepository.Has("act");
			var phoneSkill = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill1", new TimePeriod(8, 0, 8, 30), false, act);
			var emailSkill = SkillSetupHelper.CreateEmailSkill(minutesPerInterval, "skill2", new TimePeriod(8, 0, 8, 30));

			var skillDayPhone = SkillSetupHelper.CreateSkillDay(phoneSkill, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false);
			var skillDayEmail = SkillSetupHelper.CreateSkillDay(emailSkill, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false);

			var actualPhoneStats = SkillSetupHelper.CreateStatistics(skillDayPhone, latestStatsTime, TimeZone);
			var actualEmailStats = SkillSetupHelper.CreateStatistics(skillDayEmail, latestStatsTime, TimeZone);

			var actualStatsTotal = new List<SkillIntervalStatistics>();
			actualStatsTotal.AddRange(actualPhoneStats);
			actualStatsTotal.AddRange(actualEmailStats);

			SkillRepository.Has(phoneSkill, emailSkill);
			SkillDayRepository.Has(skillDayPhone, skillDayEmail);
			IntradayQueueStatisticsLoader.Has(actualStatsTotal);

			var actualStaffingSkill1 = SkillSetupHelper.GetActualStaffing(skillDayPhone, actualPhoneStats);

			var vm = Target.Load(new[] { phoneSkill.Id.GetValueOrDefault(), emailSkill.Id.GetValueOrDefault() });

			var deviationWithReforecast = SkillSetupHelper.CalculateAverageDeviation(actualPhoneStats, skillDayPhone, TimeZone);

			vm.DataSeries.ActualStaffing.First().Should().Be.EqualTo(actualStaffingSkill1);
			vm.DataSeries.ActualStaffing.Last().Should().Be.EqualTo(null);
			vm.DataSeries.ForecastedStaffing.First().Should().Be.EqualTo(3);
			vm.DataSeries.UpdatedForecastedStaffing.Last().Should().Be.EqualTo(vm.DataSeries.ForecastedStaffing.Last() * deviationWithReforecast);
		}

		[Test]
		public void ShouldReturnScheduledStaffing()
		{
			TimeZone.IsSweden();
			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var userNow = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			var act = ActivityRepository.Has("act");
			var skill = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill1", new TimePeriod(8, 0, 8, 30), false, act);
			SkillRepository.Has(skill);
			SkillDayRepository.Has(SkillSetupHelper.CreateSkillDay(skill, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false));

			SkillSetupHelper.PopulateStaffingReadModels(skill, userNow, userNow.AddMinutes(minutesPerInterval), 5.7, ScheduleForecastSkillReadModelRepository, SkillCombinationResourceRepository);

			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() });

			vm.DataSeries.Time.Length.Should().Be.EqualTo(2);
			vm.DataSeries.Time.First().Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(userNow, TimeZone.TimeZone()));
			vm.DataSeries.Time.Second().Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(userNow.AddMinutes(minutesPerInterval), TimeZone.TimeZone()));
			vm.DataSeries.ScheduledStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.ScheduledStaffing.First().Should().Be.EqualTo(5.7);
			vm.DataSeries.ScheduledStaffing.Second().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnScheduledStaffingForTwoSkills()
		{
			TimeZone.IsSweden();
			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var act = ActivityRepository.Has("act");
			var skill1 = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill1", new TimePeriod(8, 0, 8, 30), false, act);
			var skill2 = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill2", new TimePeriod(8, 0, 8, 30), false, act);
			var userNow = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			SkillRepository.Has(skill1, skill2);
			SkillDayRepository.Has(SkillSetupHelper.CreateSkillDay(skill1, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false));
			SkillDayRepository.Has(SkillSetupHelper.CreateSkillDay(skill2, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false));

			SkillSetupHelper.PopulateStaffingReadModels(skill1, userNow, userNow.AddMinutes(minutesPerInterval), 5.7, ScheduleForecastSkillReadModelRepository, SkillCombinationResourceRepository);
			SkillSetupHelper.PopulateStaffingReadModels(skill2, userNow, userNow.AddMinutes(minutesPerInterval), 7.7, ScheduleForecastSkillReadModelRepository, SkillCombinationResourceRepository);


			var vm = Target.Load(new[] { skill1.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault() });

			vm.DataSeries.Time.Length.Should().Be.EqualTo(2);
			vm.DataSeries.Time.First().Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(userNow, TimeZone.TimeZone()));
			vm.DataSeries.ScheduledStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.ScheduledStaffing.First().Should().Be.EqualTo(5.7 + 7.7);
			vm.DataSeries.ScheduledStaffing.Second().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnScheduledStaffingWithShrinkage()
		{
			TimeZone.IsSweden();
			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var act = ActivityRepository.Has("act");
			var skill = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill1", new TimePeriod(8, 0, 8, 30), false, act);
			skill.SetCascadingIndex(1);
			var skill2 = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill2", new TimePeriod(8, 0, 8, 30), false, act);
			skill2.SetCascadingIndex(2);
			SkillRepository.Has(skill, skill2);
			var userNow = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var skillday = skill.CreateSkillDayWithDemand(scenario, new DateOnly(userNow), 20);
			skillday.SkillDataPeriodCollection.ForEach(x => x.Shrinkage = new Percent(0.2)); 

			var skillday2 = skill2.CreateSkillDayWithDemand(scenario, new DateOnly(userNow), 20);
			skillday2.SkillDataPeriodCollection.ForEach(x => x.Shrinkage = new Percent(0.1));  
		
			SkillDayRepository.Has(skillday, skillday2);

			var skillCombinationResources = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = userNow,
					EndDateTime = userNow.AddMinutes(minutesPerInterval),
					Resource = 34,
					SkillCombination = new[] {skill.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault()}
				},
				new SkillCombinationResource
				{
					StartDateTime = userNow.AddMinutes(minutesPerInterval),
					EndDateTime = userNow.AddMinutes(minutesPerInterval * 2),
					Resource = 34,
					SkillCombination = new[] {skill.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault()}
				}
			};
			SkillCombinationResourceRepository.AddSkillCombinationResource(DateTime.UtcNow, skillCombinationResources);

			var vm = Target.Load(new[] {skill.Id.GetValueOrDefault()}, null, true);

			vm.DataSeries.Time.Length.Should().Be.EqualTo(2);
			vm.DataSeries.Time.First().Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(userNow, TimeZone.TimeZone()));
			vm.DataSeries.ScheduledStaffing.Length.Should().Be.EqualTo(2);
			Math.Round(vm.DataSeries.ScheduledStaffing.First().Value,2).Equals(25.00);

			var vm2 = Target.Load(new[] { skill2.Id.GetValueOrDefault() }, null, true);

			vm2.DataSeries.Time.Length.Should().Be.EqualTo(2);
			vm2.DataSeries.Time.First().Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(userNow, TimeZone.TimeZone()));
			vm2.DataSeries.ScheduledStaffing.Length.Should().Be.EqualTo(2);
			Math.Round(vm2.DataSeries.ScheduledStaffing.First().Value, 2).Should().Be.EqualTo(9.00);
		}

		[Test]
		public void ShouldReturnForecastWithShrinkage()
		{
			TimeZone.IsSweden();
			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var act = ActivityRepository.Has("act");
			var skill = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill1", new TimePeriod(8, 0, 8, 30), false, act);
			skill.SetCascadingIndex(1);
			var skill2 = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill2", new TimePeriod(8, 0, 8, 30), false, act);
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
			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var act = ActivityRepository.Has("act");
			var skill = SkillSetupHelper.CreateSkill(minutesPerInterval * 2, "skill1", new TimePeriod(8, 0, 8, 30), false, act);
			skill.SetCascadingIndex(1);
			var skill2 = SkillSetupHelper.CreateSkill(minutesPerInterval * 2, "skill2", new TimePeriod(8, 0, 8, 30), false, act);
			skill2.SetCascadingIndex(2);
			SkillRepository.Has(skill, skill2);
			var userNow = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var skillday = skill.CreateSkillDayWithDemand(scenario, new DateOnly(userNow), 20);
			skillday.SkillDataPeriodCollection.ForEach(x => x.Shrinkage = new Percent(0.2));

			var skillday2 = skill2.CreateSkillDayWithDemand(scenario, new DateOnly(userNow), 20);
			skillday2.SkillDataPeriodCollection.ForEach(x => x.Shrinkage = new Percent(0.1));

			SkillDayRepository.Has(skillday, skillday2);

			var skillCombinationResources = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = userNow,
					EndDateTime = userNow.AddMinutes(skill.DefaultResolution),
					Resource = 34,
					SkillCombination = new[] {skill.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault()}
				}
			};
			SkillCombinationResourceRepository.AddSkillCombinationResource(DateTime.UtcNow, skillCombinationResources);

			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() }, null, true);

			vm.DataSeries.Time.Length.Should().Be.EqualTo(2);
			vm.DataSeries.Time.First().Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(userNow, TimeZone.TimeZone()));
			vm.DataSeries.ScheduledStaffing.Length.Should().Be.EqualTo(2);
			Math.Round(vm.DataSeries.ScheduledStaffing.First().Value, 2).Equals(25.00);
			Math.Round(vm.DataSeries.ScheduledStaffing.Last().Value, 2).Equals(25.00);

			var vm2 = Target.Load(new[] { skill2.Id.GetValueOrDefault() }, null, true);

			vm2.DataSeries.Time.Length.Should().Be.EqualTo(2);
			vm2.DataSeries.Time.First().Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(userNow, TimeZone.TimeZone()));
			vm2.DataSeries.ScheduledStaffing.Length.Should().Be.EqualTo(2);
			Math.Round(vm2.DataSeries.ScheduledStaffing.First().Value, 2).Should().Be.EqualTo(9.00);
			Math.Round(vm2.DataSeries.ScheduledStaffing.Last().Value, 2).Should().Be.EqualTo(9.00);
		}

		[Test]
		public void ShouldReturnForecastWithShrinkageSplit()
		{
			TimeZone.IsSweden();
			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var act = ActivityRepository.Has("act");
			var skill = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill1", new TimePeriod(8, 0, 9, 0), false, act);
			skill.SetCascadingIndex(1);
			var skill2 = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill2", new TimePeriod(8, 0, 9, 0), false, act);
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

		[Test][Ignore("Not valid anymore?")]
		public void ShouldReturnScheduledStaffingStartingBeforeForecasted()
		{
			TimeZone.IsSweden();
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			var scheduledStartTime = new DateTime(2016, 8, 26, 7, 45, 0, DateTimeKind.Utc);
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var act = ActivityRepository.Has("act");
			var skill = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, act);
			var skillDay = SkillSetupHelper.CreateSkillDay(skill, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false);

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

			SkillSetupHelper.PopulateStaffingReadModels(skill, scheduledStartTime, scheduledStartTime.AddMinutes(1 * skill.DefaultResolution), 4.9, ScheduleForecastSkillReadModelRepository, SkillCombinationResourceRepository);
			SkillSetupHelper.PopulateStaffingReadModels(skill, scheduledStartTime.AddMinutes(1 * skill.DefaultResolution), scheduledStartTime.AddMinutes(2 * skill.DefaultResolution), 4.9, ScheduleForecastSkillReadModelRepository, SkillCombinationResourceRepository);
			SkillSetupHelper.PopulateStaffingReadModels(skill, scheduledStartTime.AddMinutes(2 * skill.DefaultResolution), scheduledStartTime.AddMinutes(3 * skill.DefaultResolution), 4.9, ScheduleForecastSkillReadModelRepository, SkillCombinationResourceRepository);

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

		[Test][Ignore("Not valid anymore?")]
		public void ShouldReturnScheduledStaffingEndingAfterForecasted()
		{
			TimeZone.IsSweden();
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			var scheduledStartTime = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var act = ActivityRepository.Has("act");
			var skill = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, act);
			var skillDay = SkillSetupHelper.CreateSkillDay(skill, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false);

			SkillRepository.Has(skill);
			SkillDayRepository.Has(skillDay);
			IntradayQueueStatisticsLoader.Has(SkillSetupHelper.CreateStatistics(skillDay, latestStatsTime,TimeZone));

			SkillSetupHelper.PopulateStaffingReadModels(skill, scheduledStartTime, scheduledStartTime.AddMinutes(3*minutesPerInterval), 4.9, ScheduleForecastSkillReadModelRepository,SkillCombinationResourceRepository).ToList();

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

		[Test][Ignore("Not valid anymore?")]
		public void ShouldReturnScheduledStaffingStartingAfterForecasted()
		{
			TimeZone.IsSweden();
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			var scheduledStartTime = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var act = ActivityRepository.Has("act");
			var skill = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, act);
			var skillDay = SkillSetupHelper.CreateSkillDay(skill, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false);

			SkillRepository.Has(skill);
			SkillDayRepository.Has(skillDay);

			var skillStats = SkillSetupHelper.CreateStatistics(skillDay, latestStatsTime, TimeZone);

			IntradayQueueStatisticsLoader.Has(skillStats);

			SkillSetupHelper.PopulateStaffingReadModels(skill, scheduledStartTime, scheduledStartTime.AddMinutes(minutesPerInterval), 8.3, ScheduleForecastSkillReadModelRepository, SkillCombinationResourceRepository);

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
		[Ignore("Not valid anymore?")]
		public void ShouldReturnScheduledStaffingEndingBeforeForecasted()
		{
			TimeZone.IsSweden();
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			var scheduledStartTime = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var act = ActivityRepository.Has("act");
			var skill = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, act);
			var skillDay = SkillSetupHelper.CreateSkillDay(skill, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false);

			SkillRepository.Has(skill);
			SkillDayRepository.Has(skillDay);

			var skillStats = SkillSetupHelper.CreateStatistics(skillDay, latestStatsTime, TimeZone);

			IntradayQueueStatisticsLoader.Has(skillStats);

			SkillSetupHelper.PopulateStaffingReadModels(skill, scheduledStartTime, scheduledStartTime.AddMinutes(minutesPerInterval), 5.7, ScheduleForecastSkillReadModelRepository, SkillCombinationResourceRepository);

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
			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var act = ActivityRepository.Has("act");
			var skill = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill1", new TimePeriod(8, 0, 8, 30), false, act);
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			SkillRepository.Has(skill);
			SkillDayRepository.Has(SkillSetupHelper.CreateSkillDay(skill, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false));
			SkillSetupHelper.PopulateStaffingReadModels(skill, userNow, userNow.AddMinutes(minutesPerInterval), 15, ScheduleForecastSkillReadModelRepository, SkillCombinationResourceRepository);
			SkillSetupHelper.PopulateStaffingReadModels(skill, userNow.AddMinutes(-minutesPerInterval), userNow, 10, ScheduleForecastSkillReadModelRepository, SkillCombinationResourceRepository);

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
			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var act = ActivityRepository.Has("act");
			var skill = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, act);
			SkillRepository.Has(skill);

			var skillDay = SkillSetupHelper.CreateSkillDay(skill, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false);
			SkillDayRepository.Has(skillDay);

			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() });

			vm.StaffingHasData.Should().Be.EqualTo(true);
			vm.DataSeries.ScheduledStaffing.IsEmpty().Should().Be.EqualTo(true);
		}


		

	}
}