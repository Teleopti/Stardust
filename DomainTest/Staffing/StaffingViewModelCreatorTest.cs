using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
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
	public class StaffingViewModelCreatorTest
	{
		public StaffingViewModelCreator Target;
		public FakeSkillCombinationResourceRepository SkillCombinationResourceRepository;
		public FakeSkillForecastReadModelRepository SkillForecastReadModelRepository;
		public MutableNow Now;
		public FakeUserTimeZone TimeZone;
		private const int minutesPerInterval = 15;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble(new FakeUserTimeZone(TimeZoneInfo.Utc)).For<IUserTimeZone>();
		}

		[Test, Ignore("WIP")]
		public void ShouldCalculateCorrectForecastingWithoutShrinkage()
		{
			TimeZone.IsSweden();

			var userNow = new DateTime(2016, 8, 26, 8, 15, 0);
			var userNowUtc = TimeZoneInfo.ConvertTimeToUtc(userNow, TimeZone.TimeZone());
			Now.Is(userNowUtc);

			var opensAtUtc = TimeZoneInfo.ConvertTimeToUtc(new DateTime(2016, 8, 26, 8, 0, 0), TimeZone.TimeZone());
			var closesAtUtc = TimeZoneInfo.ConvertTimeToUtc(new DateTime(2016, 8, 26, 8, 30, 0), TimeZone.TimeZone());
			var openHours = new DateTimePeriod(opensAtUtc, closesAtUtc).TimePeriod(TimeZoneInfo.Utc);

			//var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var act = ActivityFactory.CreateActivity("act");
			var skill = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill1", openHours, false, act);
			//SkillRepository.Has(skill);

			//SkillDayRepository.Has(SkillSetupHelper.CreateSkillDay(skill, scenario, userNowUtc, openHours, false));
			SkillSetupHelper.PopulateStaffingReadModels(skill, userNowUtc, userNowUtc.AddMinutes(minutesPerInterval), 2, SkillCombinationResourceRepository);
			SkillSetupHelper.PopulateStaffingReadModels(skill, userNowUtc.AddMinutes(-minutesPerInterval), userNowUtc, 10, SkillCombinationResourceRepository);

			SkillSetupHelper.PopulateForecastReadModels(skill, userNowUtc.AddMinutes(-minutesPerInterval), userNowUtc.AddMinutes(minutesPerInterval), 3, SkillForecastReadModelRepository);
			  var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() });

			vm.DataSeries.Time.Length.Should().Be.EqualTo(2);
			vm.DataSeries.ForecastedStaffing[0].Should().Be.EqualTo(3);
			vm.DataSeries.ForecastedStaffing[1].Should().Be.EqualTo(3);
			vm.DataSeries.ScheduledStaffing[0].Should().Be.EqualTo(10);
			vm.DataSeries.ScheduledStaffing[1].Should().Be.EqualTo(2);
		}
	}

	
}
