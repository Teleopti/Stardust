using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.Staffing
{
	[DomainTest]
	[ToggleOff(Toggles.WFM_Forecast_Readmodel_80790)]
	public class ScheduledStaffingViewModelCreatorTest : IIsolateSystem
	{
		public ScheduledStaffingViewModelCreator Target;
		public FakeScenarioRepository ScenarioRepository;
		public FakeSkillRepository SkillRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;
		public FakeSkillCombinationResourceRepository SkillCombinationResourceRepository;
		public FakeActivityRepository ActivityRepository;
		public MutableNow Now;
		public FakeUserTimeZone TimeZone;
		private const int minutesPerInterval = 15;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble(new FakeUserTimeZone(TimeZoneInfo.Utc)).For<IUserTimeZone>();
		}

		[Test]
		public void ShouldReturnRelativeDifferenceForASkill()
		{
			TimeZone.IsSweden();

			var userNow = new DateTime(2016, 8, 26, 8, 15, 0);
			var userNowUtc = TimeZoneInfo.ConvertTimeToUtc(userNow, TimeZone.TimeZone());
			Now.Is(userNowUtc);

			var opensAtUtc = TimeZoneInfo.ConvertTimeToUtc(new DateTime(2016, 8, 26, 8, 0, 0), TimeZone.TimeZone());
			var closesAtUtc = TimeZoneInfo.ConvertTimeToUtc(new DateTime(2016, 8, 26, 8, 30, 0), TimeZone.TimeZone());
			var openHours = new DateTimePeriod(opensAtUtc, closesAtUtc).TimePeriod(TimeZoneInfo.Utc);

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var act = ActivityRepository.Has("act");
			var skill = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill1", openHours, false, act);
			SkillRepository.Has(skill);
			
			SkillDayRepository.Has(SkillSetupHelper.CreateSkillDay(skill, scenario, userNowUtc, openHours, false));
			SkillSetupHelper.PopulateStaffingReadModels(skill, userNowUtc, userNowUtc.AddMinutes(minutesPerInterval), 2, SkillCombinationResourceRepository);
			SkillSetupHelper.PopulateStaffingReadModels(skill, userNowUtc.AddMinutes(-minutesPerInterval), userNowUtc, 10, SkillCombinationResourceRepository);

			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() });

			vm.DataSeries.Time.Length.Should().Be.EqualTo(2);
			vm.DataSeries.Time.First().Should().Be.EqualTo(userNow.AddMinutes(-minutesPerInterval));
			var scheduledSeries = vm.DataSeries.ScheduledStaffing;
			var forecastedSeries = vm.DataSeries.ForecastedStaffing;
			var relativeDiffSeries = vm.DataSeries.AbsoluteDifference;
			scheduledSeries.First().Should().Be.EqualTo(10);
			scheduledSeries.Second().Should().Be.EqualTo(2);
			forecastedSeries.First().Should().Be.EqualTo(3);
			forecastedSeries.Second().Should().Be.EqualTo(3);
			relativeDiffSeries.First().Should().Be.EqualTo(7);
			relativeDiffSeries.Second().Should().Be.EqualTo(-1);
			vm.DataSeries.AbsoluteDifference.Length.Should().Be.EqualTo(vm.DataSeries.ForecastedStaffing.Length);
		}
		
		[Test]
		public void ShouldHandleMultipleSkills()
		{
			TimeZone.IsSweden();

			var userNow = new DateTime(2016, 8, 26, 8, 15, 0);
			var userNowUtc = TimeZoneInfo.ConvertTimeToUtc(userNow, TimeZone.TimeZone());
			Now.Is(userNowUtc);

			var opensAtUtc = TimeZoneInfo.ConvertTimeToUtc(new DateTime(2016, 8, 26, 8, 0, 0), TimeZone.TimeZone());
			var closesAtUtc = TimeZoneInfo.ConvertTimeToUtc(new DateTime(2016, 8, 26, 8, 30, 0), TimeZone.TimeZone());
			var openHours = new DateTimePeriod(opensAtUtc, closesAtUtc).TimePeriod(TimeZoneInfo.Utc);

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var act = ActivityRepository.Has("act");
			var act2 = ActivityRepository.Has("act2");
			var skill = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill1", openHours, false, act);
			var skill2 = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill2", openHours, false, act2);
			SkillRepository.Has(skill);
			SkillRepository.Has(skill2);
			
			SkillDayRepository.Has(SkillSetupHelper.CreateSkillDay(skill, scenario, userNowUtc, openHours, false));	
			SkillDayRepository.Has(SkillSetupHelper.CreateSkillDay(skill2, scenario, userNowUtc, openHours, false));

			var skillList = new HashSet<Guid> {skill.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault()};
			SkillSetupHelper.PopulateStaffingReadModels(skillList, userNowUtc, userNowUtc.AddMinutes(minutesPerInterval), 2, SkillCombinationResourceRepository);
			SkillSetupHelper.PopulateStaffingReadModels(skillList, userNowUtc.AddMinutes(-minutesPerInterval), userNowUtc, 10, SkillCombinationResourceRepository);
			
			var vm = Target.Load(skillList.ToArray());

			vm.DataSeries.Time.Length.Should().Be.EqualTo(2);
			vm.DataSeries.Time.First().Should().Be.EqualTo(userNow.AddMinutes(-minutesPerInterval));
			var scheduledSeries = vm.DataSeries.ScheduledStaffing;
			var forecastSeries = vm.DataSeries.ForecastedStaffing;
			var relativeDiffSeries = vm.DataSeries.AbsoluteDifference;
			scheduledSeries.First().Should().Be.EqualTo(10);
			scheduledSeries.Second().Should().Be.EqualTo(2);
			forecastSeries.First().Should().Be.EqualTo(6);
			forecastSeries.Second().Should().Be.EqualTo(6);
			relativeDiffSeries.First().Should().Be.EqualTo(4);
			relativeDiffSeries.Second().Should().Be.EqualTo(-4);
			vm.DataSeries.AbsoluteDifference.Length.Should().Be.EqualTo(vm.DataSeries.ForecastedStaffing.Length);
		}

		[Test]
		public void WhenLoad_ShouldReturnTimesInLocalTimeZone()
		{
			TimeZone.IsSweden();

			var userNow = new DateTime(2016, 8, 26, 8, 15, 0);
			var userNowUtc = TimeZoneInfo.ConvertTimeToUtc(userNow, TimeZone.TimeZone());
			Now.Is(userNowUtc);

			var opensAtUtc = TimeZoneInfo.ConvertTimeToUtc(new DateTime(2016, 8, 26, 8, 0, 0), TimeZone.TimeZone());
			var closesAtUtc = TimeZoneInfo.ConvertTimeToUtc(new DateTime(2016, 8, 26, 8, 30, 0), TimeZone.TimeZone());
			var openHours = new DateTimePeriod(opensAtUtc, closesAtUtc).TimePeriod(TimeZoneInfo.Utc);

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var act = ActivityRepository.Has("act");
			var skill = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill1", openHours, false, act);
			SkillRepository.Has(skill);
			
			SkillDayRepository.Has(SkillSetupHelper.CreateSkillDay(skill, scenario, userNowUtc, openHours, false));
			SkillSetupHelper.PopulateStaffingReadModels(skill, userNowUtc, userNowUtc.AddMinutes(minutesPerInterval), 2, SkillCombinationResourceRepository);
			SkillSetupHelper.PopulateStaffingReadModels(skill, userNowUtc.AddMinutes(-minutesPerInterval), userNowUtc, 10, SkillCombinationResourceRepository);

			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() });

			vm.DataSeries.Time.Length.Should().Be.EqualTo(2);
			vm.DataSeries.Time.First().Should().Be.EqualTo(new DateTime(2016, 8, 26, 8, 0, 0));
			vm.DataSeries.Time.Last().Should().Be.EqualTo(new DateTime(2016, 8, 26, 8, 15, 0));
		}

		[Test]
		public void ShouldCalculateCorrectForecastingWithoutShrinkage()
		{
			TimeZone.IsSweden();

			var userNow = new DateTime(2016, 8, 26, 8, 15, 0);
			var userNowUtc = TimeZoneInfo.ConvertTimeToUtc(userNow, TimeZone.TimeZone());
			Now.Is(userNowUtc);

			var opensAtUtc = TimeZoneInfo.ConvertTimeToUtc(new DateTime(2016, 8, 26, 8, 0, 0), TimeZone.TimeZone());
			var closesAtUtc = TimeZoneInfo.ConvertTimeToUtc(new DateTime(2016, 8, 26, 8, 30, 0), TimeZone.TimeZone());
			var openHours = new DateTimePeriod(opensAtUtc, closesAtUtc).TimePeriod(TimeZoneInfo.Utc);

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var act = ActivityRepository.Has("act");
			var skill = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill1", openHours, false, act);
			SkillRepository.Has(skill);

			SkillDayRepository.Has(SkillSetupHelper.CreateSkillDay(skill, scenario, userNowUtc, openHours, false));
			SkillSetupHelper.PopulateStaffingReadModels(skill, userNowUtc, userNowUtc.AddMinutes(minutesPerInterval), 2, SkillCombinationResourceRepository);
			SkillSetupHelper.PopulateStaffingReadModels(skill, userNowUtc.AddMinutes(-minutesPerInterval), userNowUtc, 10, SkillCombinationResourceRepository);

			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() });

			vm.DataSeries.Time.Length.Should().Be.EqualTo(2);
			vm.DataSeries.ForecastedStaffing[0].Should().Be.EqualTo(3);
			vm.DataSeries.ForecastedStaffing[1].Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldCalculateCorrectForecastingWithShrinkage()
		{
			TimeZone.IsSweden();

			var userNow = new DateTime(2016, 8, 26, 8, 15, 0);
			var userNowUtc = TimeZoneInfo.ConvertTimeToUtc(userNow, TimeZone.TimeZone());
			Now.Is(userNowUtc);

			var opensAtUtc = TimeZoneInfo.ConvertTimeToUtc(new DateTime(2016, 8, 26, 8, 0, 0), TimeZone.TimeZone());
			var closesAtUtc = TimeZoneInfo.ConvertTimeToUtc(new DateTime(2016, 8, 26, 8, 30, 0), TimeZone.TimeZone());
			var openHours = new DateTimePeriod(opensAtUtc, closesAtUtc).TimePeriod(TimeZoneInfo.Utc);

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var act = ActivityRepository.Has("act");
			var skill = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill1", openHours, false, act);
			SkillRepository.Has(skill);

			var skillDay =  SkillSetupHelper.CreateSkillDay(skill, scenario, userNowUtc, openHours, false);
			skillDay.SkillDataPeriodCollection.ForEach(s => { s.Shrinkage = new Percent(0.5); });

			SkillDayRepository.Has(skillDay);
			SkillSetupHelper.PopulateStaffingReadModels(skill, userNowUtc, userNowUtc.AddMinutes(minutesPerInterval), 2, SkillCombinationResourceRepository);
			SkillSetupHelper.PopulateStaffingReadModels(skill, userNowUtc.AddMinutes(-minutesPerInterval), userNowUtc, 10, SkillCombinationResourceRepository);

			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() },null,true);

			vm.DataSeries.Time.Length.Should().Be.EqualTo(2);
			vm.DataSeries.ForecastedStaffing[0].Should().Be.EqualTo(6);
			vm.DataSeries.ForecastedStaffing[1].Should().Be.EqualTo(6);
		}

		[Test]
		public void ShouldHandleDaylightSavings()
		{
			TimeZone.IsSweden();

			var userNow = new DateTime(2018, 10, 28, 0, 0, 0);
			var userNowUtc = TimeZoneInfo.ConvertTimeToUtc(userNow, TimeZone.TimeZone());
			Now.Is(userNowUtc);

			var opensAtUtc = new DateTime(2018, 10, 28, 0, 0, 0, DateTimeKind.Utc);
			var closesAtUtc = new DateTime(2018, 10, 29, 0, 0, 0, DateTimeKind.Utc);
			var openHours = new DateTimePeriod(opensAtUtc, closesAtUtc).TimePeriod(TimeZoneInfo.Utc);

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var act = ActivityRepository.Has("act");
			var skill = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill1", openHours, false, act);
			SkillRepository.Has(skill);

			var skillDay = SkillSetupHelper.CreateSkillDay(skill, scenario, userNowUtc, openHours, false);
			var skillDay2 = SkillSetupHelper.CreateSkillDay(skill, scenario, userNowUtc.AddDays(1), openHours, false);
			var skillDay3 = SkillSetupHelper.CreateSkillDay(skill, scenario, userNowUtc.AddDays(-1), openHours, false);

			SkillDayRepository.Has(skillDay);
			SkillDayRepository.Has(skillDay2);
			SkillDayRepository.Has(skillDay3);
			SkillSetupHelper.PopulateStaffingReadModels(skill, userNowUtc, userNowUtc.AddMinutes(minutesPerInterval), 2, SkillCombinationResourceRepository);
			SkillSetupHelper.PopulateStaffingReadModels(skill, userNowUtc.AddMinutes(-minutesPerInterval), userNowUtc, 10, SkillCombinationResourceRepository);

			var vm1 = Target.Load(new[] { skill.Id.GetValueOrDefault()}, new DateOnly(userNow) , false);
			var vm2 = Target.Load(new[] { skill.Id.GetValueOrDefault()}, new DateOnly(userNow.AddDays(-1)) , false);

			vm1.DataSeries.Time.Length.Should().Be.EqualTo(100);
			vm2.DataSeries.Time.Length.Should().Be.EqualTo(96);
		}
	}
}
