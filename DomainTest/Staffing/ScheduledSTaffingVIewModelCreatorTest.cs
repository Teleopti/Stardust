using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Staffing
{
	[DomainTest]
	public class ScheduledSTaffingVIewModelCreatorTest : ISetup
	{
		public ScheduledStaffingViewModelCreator Target;
		public FakeScenarioRepository ScenarioRepository;
		public FakeSkillRepository SkillRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;
		public FakeIntradayQueueStatisticsLoader IntradayQueueStatisticsLoader;
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
			SkillSetupHelper.PopulateStaffingReadModels(skill, userNow, userNow.AddMinutes(minutesPerInterval), 2, SkillCombinationResourceRepository);
			SkillSetupHelper.PopulateStaffingReadModels(skill, userNow.AddMinutes(-minutesPerInterval), userNow, 10, SkillCombinationResourceRepository);

			var vm = Target.Load(new[] { skill.Id.GetValueOrDefault() });

			vm.DataSeries.Time.Length.Should().Be.EqualTo(2);
			vm.DataSeries.Time.First().Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(userNow.AddMinutes(-minutesPerInterval), TimeZone.TimeZone()));
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
	}
}
