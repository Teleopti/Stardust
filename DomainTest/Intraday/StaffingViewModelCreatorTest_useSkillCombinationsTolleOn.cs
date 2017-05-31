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

			var skillday = skill.CreateSkillDayWithDemand(scenario, new DateOnly(userNow), 1);
			skillday.SkillDataPeriodCollection.ForEach(x => x.Shrinkage = new Percent(0.2));
			SkillDayRepository.Has(skillday);

			var skillday2 = skill2.CreateSkillDayWithDemand(scenario, new DateOnly(userNow), 1);
			skillday2.SkillDataPeriodCollection.ForEach(x => x.Shrinkage = new Percent(0.5));
			SkillDayRepository.Has(skillday2);

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

			var skillday = skill.CreateSkillDayWithDemand(scenario, new DateOnly(userNow), 1);
			skillday.SkillDataPeriodCollection.ForEach(x => x.Shrinkage = new Percent(0.2));
			SkillDayRepository.Has(skillday);

			var skillday2 = skill2.CreateSkillDayWithDemand(scenario, new DateOnly(userNow), 1);
			skillday2.SkillDataPeriodCollection.ForEach(x => x.Shrinkage = new Percent(0.5));
			SkillDayRepository.Has(skillday2);

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

		[Test]
		public void ShouldReturnRelativeDifferenceForASkill()
		{

			TimeZone.IsSweden();
			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var act = ActivityRepository.Has("act");
			var skill = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill1", new TimePeriod(8, 0, 8, 30), false, act);
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			SkillRepository.Has(skill);
			SkillDayRepository.Has(SkillSetupHelper.CreateSkillDay(skill, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30), false));
			SkillSetupHelper.PopulateStaffingReadModels(skill, userNow, userNow.AddMinutes(minutesPerInterval), 2, ScheduleForecastSkillReadModelRepository, SkillCombinationResourceRepository);
			SkillSetupHelper.PopulateStaffingReadModels(skill, userNow.AddMinutes(-minutesPerInterval), userNow, 10, ScheduleForecastSkillReadModelRepository, SkillCombinationResourceRepository);

			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() });

			vm.DataSeries.Time.Length.Should().Be.EqualTo(2);
			vm.DataSeries.Time.First().Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(userNow.AddMinutes(-minutesPerInterval), TimeZone.TimeZone()));
			var scheduledSeries = vm.DataSeries.ScheduledStaffing;
			var forecastedSeries = vm.DataSeries.ForecastedStaffing;
			var relativeDiffSeries = vm.DataSeries.RelativeDifference;
			scheduledSeries.First().Should().Be.EqualTo(10);
			scheduledSeries.Second().Should().Be.EqualTo(2);
			forecastedSeries.First().Should().Be.EqualTo(3);
			forecastedSeries.Second().Should().Be.EqualTo(3);
			relativeDiffSeries.First().Should().Be.EqualTo(7);
			relativeDiffSeries.Second().Should().Be.EqualTo(-1);
			vm.DataSeries.RelativeDifference.Length.Should().Be.EqualTo(vm.DataSeries.ForecastedStaffing.Length);

		}


	}
}