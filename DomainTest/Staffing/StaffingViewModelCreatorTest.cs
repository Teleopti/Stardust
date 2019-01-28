using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday.To_Staffing;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Staffing
{
	[DomainTest]
	[AllTogglesOn]
	public class StaffingViewModelCreatorTest
	{
		public StaffingViewModelCreator Target;
		public FakeSkillCombinationResourceRepository SkillCombinationResourceRepository;
		public FakeSkillForecastReadModelRepository SkillForecastReadModelRepository;
		public FakeSkillRepository SkillRepository;
		public MutableNow Now;
		public FakeUserTimeZone TimeZone;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;
		private const int minutesPerInterval = 15;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble(new FakeUserTimeZone(TimeZoneInfo.Utc)).For<IUserTimeZone>();
		}

		[Test]
		public void ShouldCalculateCorrectForecastingWithoutShrinkage()
		{
			TimeZone.IsSweden();
			IntervalLengthFetcher.Has(minutesPerInterval);
			  var userNow = new DateTime(2016, 8, 26, 8, 15, 0);
			var userNowUtc = TimeZoneInfo.ConvertTimeToUtc(userNow, TimeZone.TimeZone());
			Now.Is(userNowUtc);

			var opensAtUtc = TimeZoneInfo.ConvertTimeToUtc(new DateTime(2016, 8, 26, 8, 0, 0), TimeZone.TimeZone());
			var closesAtUtc = TimeZoneInfo.ConvertTimeToUtc(new DateTime(2016, 8, 26, 8, 30, 0), TimeZone.TimeZone());
			var openHours = new DateTimePeriod(opensAtUtc, closesAtUtc).TimePeriod(TimeZoneInfo.Utc);

			var act = ActivityFactory.CreateActivity("act");
			var skill = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill1", openHours, false, act);
			SkillRepository.Has(skill);

			SkillSetupHelper.PopulateStaffingReadModels(skill, userNowUtc, userNowUtc.AddMinutes(minutesPerInterval), 2, SkillCombinationResourceRepository);
			SkillSetupHelper.PopulateStaffingReadModels(skill, userNowUtc.AddMinutes(-minutesPerInterval), userNowUtc, 10, SkillCombinationResourceRepository);

			SkillSetupHelper.PopulateForecastReadModels(skill, userNowUtc.AddMinutes(-minutesPerInterval), userNowUtc.AddMinutes(minutesPerInterval), 3, SkillForecastReadModelRepository);
			  var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() }, new DateOnly(2016, 8, 26), false);

			vm.DataSeries.Time.Length.Should().Be.EqualTo(96);
			vm.DataSeries.ForecastedStaffing[24].Should().Be.EqualTo(3);
			vm.DataSeries.ForecastedStaffing[25].Should().Be.EqualTo(3);
			vm.DataSeries.ScheduledStaffing[24].Should().Be.EqualTo(10);
			vm.DataSeries.ScheduledStaffing[25].Should().Be.EqualTo(2);
		}
		
		[Test]
		public void ShouldCalculateCorrectForecastingForMultipleSkills()
		{
			TimeZone.IsSweden();
			IntervalLengthFetcher.Has(minutesPerInterval);
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0);
			var userNowUtc = TimeZoneInfo.ConvertTimeToUtc(userNow, TimeZone.TimeZone());
			Now.Is(userNowUtc);

			var opensAtUtc = TimeZoneInfo.ConvertTimeToUtc(new DateTime(2016, 8, 26, 8, 0, 0), TimeZone.TimeZone());
			var closesAtUtc = TimeZoneInfo.ConvertTimeToUtc(new DateTime(2016, 8, 26, 8, 30, 0), TimeZone.TimeZone());
			var openHours = new DateTimePeriod(opensAtUtc, closesAtUtc).TimePeriod(TimeZoneInfo.Utc);

			var act = ActivityFactory.CreateActivity("act");
			var act2 = ActivityFactory.CreateActivity("act");
			var skill = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill1", openHours, false, act);
			var skill2 = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill1", openHours, false, act2);
			SkillRepository.Has(skill);
			SkillRepository.Has(skill2);

			var skillList = new HashSet<Guid> {skill.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault()};
			SkillSetupHelper.PopulateStaffingReadModels(skillList, userNowUtc.AddMinutes(-minutesPerInterval), userNowUtc, 10, SkillCombinationResourceRepository);
			SkillSetupHelper.PopulateStaffingReadModels(skillList, userNowUtc, userNowUtc.AddMinutes(minutesPerInterval), 2, SkillCombinationResourceRepository);
			
			SkillSetupHelper.PopulateForecastReadModels(skill, userNowUtc.AddMinutes(-minutesPerInterval), userNowUtc.AddMinutes(minutesPerInterval), 3, SkillForecastReadModelRepository);
			SkillSetupHelper.PopulateForecastReadModels(skill2, userNowUtc.AddMinutes(-minutesPerInterval), userNowUtc.AddMinutes(minutesPerInterval), 3, SkillForecastReadModelRepository);

			var skillIdArray = new List<ISkill> {skill, skill2}.Select(s => s.Id.GetValueOrDefault()).ToArray();
			var vm = Target.Load(skillIdArray, new DateOnly(2016, 8, 26), false);

			vm.DataSeries.Time.Length.Should().Be.EqualTo(96);
			vm.DataSeries.ForecastedStaffing[24].Should().Be.EqualTo(6);
			vm.DataSeries.ForecastedStaffing[25].Should().Be.EqualTo(6);
			vm.DataSeries.ScheduledStaffing[24].Should().Be.EqualTo(10);
			vm.DataSeries.ScheduledStaffing[25].Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldReturnOnCorrectDay()
		{
			TimeZone.IsSweden();
			IntervalLengthFetcher.Has(minutesPerInterval);
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0);
			var userNowUtc = TimeZoneInfo.ConvertTimeToUtc(userNow, TimeZone.TimeZone());
			Now.Is(userNowUtc);

			var opensAtUtc = TimeZoneInfo.ConvertTimeToUtc(new DateTime(2016, 8, 26, 0, 0, 0), TimeZone.TimeZone());
			var closesAtUtc = TimeZoneInfo.ConvertTimeToUtc(new DateTime(2016, 8, 27, 0, 0, 0), TimeZone.TimeZone());
			var openHours = new DateTimePeriod(opensAtUtc, closesAtUtc).TimePeriod(TimeZoneInfo.Utc);

			var act = ActivityFactory.CreateActivity("act");
			var skill = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill1", openHours, false, act, TimeSpan.FromHours(opensAtUtc.Hour));
			SkillRepository.Has(skill);

			var val = 1;
			for (var startTime = opensAtUtc.AddHours(-1) ; startTime < closesAtUtc.AddHours(1); startTime = startTime.AddMinutes(15) )
			{
				SkillSetupHelper.PopulateForecastReadModels(skill, startTime, startTime.AddMinutes(15), val+5, SkillForecastReadModelRepository);
				SkillSetupHelper.PopulateStaffingReadModels(skill, startTime, startTime.AddMinutes(15), val++, SkillCombinationResourceRepository);
			}

			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() }, new DateOnly(2016, 8, 26), false);

			vm.DataSeries.Time.Length.Should().Be.EqualTo(96);
			vm.DataSeries.ScheduledStaffing.Length.Should().Be(96);
			vm.DataSeries.ForecastedStaffing.Length.Should().Be(96);
			vm.DataSeries.ScheduledStaffing[0].Should().Be.EqualTo(5);
			vm.DataSeries.ScheduledStaffing[1].Should().Be.EqualTo(6);
			vm.DataSeries.ScheduledStaffing[95].Should().Be.EqualTo(100);

			vm.DataSeries.ForecastedStaffing[0].Should().Be.EqualTo(10);
			vm.DataSeries.ForecastedStaffing[1].Should().Be.EqualTo(11);
			vm.DataSeries.ForecastedStaffing[95].Should().Be.EqualTo(105);
		}

		[Test]
		public void ShouldCalculateCorrectForecastingWithShrinkage()
		{
			TimeZone.IsSweden();
			IntervalLengthFetcher.Has(minutesPerInterval);
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0);
			var userNowUtc = TimeZoneInfo.ConvertTimeToUtc(userNow, TimeZone.TimeZone());
			Now.Is(userNowUtc);

			var opensAtUtc = TimeZoneInfo.ConvertTimeToUtc(new DateTime(2016, 8, 26, 8, 0, 0), TimeZone.TimeZone());
			var closesAtUtc = TimeZoneInfo.ConvertTimeToUtc(new DateTime(2016, 8, 26, 8, 30, 0), TimeZone.TimeZone());
			var openHours = new DateTimePeriod(opensAtUtc, closesAtUtc).TimePeriod(TimeZoneInfo.Utc);

			var act = ActivityFactory.CreateActivity("act");
			var skill = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill1", openHours, false, act);
			SkillRepository.Has(skill);

			SkillSetupHelper.PopulateStaffingReadModels(skill, userNowUtc, userNowUtc.AddMinutes(minutesPerInterval), 2, SkillCombinationResourceRepository);
			SkillSetupHelper.PopulateStaffingReadModels(skill, userNowUtc.AddMinutes(-minutesPerInterval), userNowUtc, 10, SkillCombinationResourceRepository);

			SkillSetupHelper.PopulateForecastReadModels(skill, userNowUtc.AddMinutes(-minutesPerInterval), userNowUtc.AddMinutes(minutesPerInterval), 3, SkillForecastReadModelRepository,4);
			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() }, new DateOnly(2016, 8, 26), true);

			vm.DataSeries.Time.Length.Should().Be.EqualTo(96);
			vm.DataSeries.ForecastedStaffing[24].Should().Be.EqualTo(4);
			vm.DataSeries.ForecastedStaffing[25].Should().Be.EqualTo(4);
			vm.DataSeries.ScheduledStaffing[24].Should().Be.EqualTo(10);
			vm.DataSeries.ScheduledStaffing[25].Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldHandleDaylightSavingOff()
		{
			TimeZone.IsSweden();
			IntervalLengthFetcher.Has(minutesPerInterval);
			var userNow = new DateTime(2018, 10, 28, 8, 15, 0);
			var userNowUtc = TimeZoneInfo.ConvertTimeToUtc(userNow, TimeZone.TimeZone());
			Now.Is(userNowUtc);

			var opensAtUtc = TimeZoneInfo.ConvertTimeToUtc(new DateTime(2018, 10, 28, 0, 0, 0), TimeZone.TimeZone());
			var closesAtUtc = TimeZoneInfo.ConvertTimeToUtc(new DateTime(2018, 10, 29, 0, 0, 0), TimeZone.TimeZone());
		 // to avoid daylighttimesavingchangedate
			var openHours = new DateTimePeriod(opensAtUtc.AddMonths(-1), opensAtUtc.AddMonths(-1).AddDays(1)).TimePeriod(TimeZoneInfo.Utc);

			var act = ActivityFactory.CreateActivity("act");
			var skill = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill1", openHours, false, act, TimeSpan.FromHours(opensAtUtc.Hour));
			SkillRepository.Has(skill);

			var val = 1;
			for (var startTime = opensAtUtc.AddHours(-1); startTime < closesAtUtc.AddHours(1); startTime = startTime.AddMinutes(15))
			{
				SkillSetupHelper.PopulateForecastReadModels(skill, startTime, startTime.AddMinutes(15), val + 5, SkillForecastReadModelRepository);
				SkillSetupHelper.PopulateStaffingReadModels(skill, startTime, startTime.AddMinutes(15), val++, SkillCombinationResourceRepository);
			}

			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() }, new DateOnly(2018, 10, 28), false);

			vm.DataSeries.Time.Length.Should().Be.EqualTo(100);
			vm.DataSeries.ScheduledStaffing.Length.Should().Be(100);
			vm.DataSeries.ForecastedStaffing.Length.Should().Be(100);
			vm.DataSeries.ScheduledStaffing[0].Should().Be.EqualTo(5);
			vm.DataSeries.ScheduledStaffing[1].Should().Be.EqualTo(6);
			vm.DataSeries.ScheduledStaffing[99].Should().Be.EqualTo(104);

			vm.DataSeries.ForecastedStaffing[0].Should().Be.EqualTo(10);
			vm.DataSeries.ForecastedStaffing[1].Should().Be.EqualTo(11);
			vm.DataSeries.ForecastedStaffing[99].Should().Be.EqualTo(109);
		}

		[Test]
		public void ShouldHandleDaylightSavingOn()
		{
			TimeZone.IsSweden();
			IntervalLengthFetcher.Has(minutesPerInterval);
			var userNow = new DateTime(2019, 03, 31, 8, 15, 0);
			var userNowUtc = TimeZoneInfo.ConvertTimeToUtc(userNow, TimeZone.TimeZone());
			Now.Is(userNowUtc);

			var opensAtUtc = TimeZoneInfo.ConvertTimeToUtc(new DateTime(2019, 03, 31, 0, 0, 0), TimeZone.TimeZone());
			var closesAtUtc = TimeZoneInfo.ConvertTimeToUtc(new DateTime(2019, 04, 1, 0, 0, 0), TimeZone.TimeZone());
			// to avoid daylighttimesavingchangedate
			var openHours = new DateTimePeriod(opensAtUtc.AddMonths(-1), opensAtUtc.AddMonths(-1).AddDays(1)).TimePeriod(TimeZoneInfo.Utc);

			var act = ActivityFactory.CreateActivity("act");
			var skill = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill1", openHours, false, act, TimeSpan.FromHours(opensAtUtc.Hour));
			SkillRepository.Has(skill);

			var val = 1;
			for (var startTime = opensAtUtc.AddHours(-1); startTime < closesAtUtc.AddHours(2); startTime = startTime.AddMinutes(15))
			{
				SkillSetupHelper.PopulateForecastReadModels(skill, startTime, startTime.AddMinutes(15), val + 5, SkillForecastReadModelRepository);
				SkillSetupHelper.PopulateStaffingReadModels(skill, startTime, startTime.AddMinutes(15), val++, SkillCombinationResourceRepository);
			}

			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() }, new DateOnly(2019, 03, 31), false);

			vm.DataSeries.Time.Length.Should().Be.EqualTo(92);
			vm.DataSeries.ScheduledStaffing.Length.Should().Be(92);
			vm.DataSeries.ForecastedStaffing.Length.Should().Be(92);
			vm.DataSeries.ScheduledStaffing[0].Should().Be.EqualTo(5);
			vm.DataSeries.ScheduledStaffing[1].Should().Be.EqualTo(6);
			vm.DataSeries.ScheduledStaffing[91].Should().Be.EqualTo(96);

			vm.DataSeries.ForecastedStaffing[0].Should().Be.EqualTo(10);
			vm.DataSeries.ForecastedStaffing[1].Should().Be.EqualTo(11);
			vm.DataSeries.ForecastedStaffing[91].Should().Be.EqualTo(101);
		}
	}

	
}
