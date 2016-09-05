using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Intraday;
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
	public class ForecastedStaffingViewModelCreatorTest : ISetup
	{
		public ForecastedStaffingViewModelCreator Target;
		public ForecastedStaffingProvider ForecastedStaffingProvider;
		public FakeScenarioRepository ScenarioRepository;
		public FakeSkillRepository SkillRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;
		public FakeIntradayQueueStatisticsLoader IntradayQueueStatisticsLoader;
		public MutableNow Now;
		public FakeUserTimeZone TimeZone;

		private const int minutesPerInterval = 15;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble(new FakeUserTimeZone(TimeZoneInfo.Utc)).For<IUserTimeZone>();
			system.UseTestDouble(new FakeUserCulture(CultureInfoFactory.CreateSwedishCulture())).For<IUserCulture>();
		}

		[Test]
		public void ShouldReturnStaffingForTwoIntervals()
		{
			TimeZone.IsSweden();
			IntervalLengthFetcher.Has(minutesPerInterval);
			var scenario = ScenarioFactory.CreateScenario("scenariorita", true, true).WithId();
			ScenarioRepository.Has(scenario);

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30));
			SkillRepository.Has(skill);

			var skillDay = skill.CreateSkillDayWithDemand(scenario, new DateOnly(Now.UtcDateTime()), TimeSpan.FromMinutes(60));
			SkillDayRepository.Add(skillDay);

			var vm = Target.Load(new[] { skill.Id.Value });

			var staffingIntervals = skillDay.SkillStaffPeriodViewCollection(TimeSpan.FromMinutes(minutesPerInterval));
			vm.DataSeries.Time.Length.Should().Be.EqualTo(2);
			vm.DataSeries.Time.First().Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(staffingIntervals.First().Period.StartDateTime, TimeZone.TimeZone()));
			vm.DataSeries.Time.Last().Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(staffingIntervals.Last().Period.StartDateTime, TimeZone.TimeZone()));
			vm.DataSeries.ForecastedStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.ForecastedStaffing.First().Should().Be.GreaterThan(0d);
			vm.DataSeries.ForecastedStaffing.Last().Should().Be.GreaterThan(0d);
		}

		[Test]
		public void ShouldReturnStaffingForUsersCurrentDayInUsTimeZoneWhenOpen247()
		{
			var userNow = new DateTime(2016, 8, 26, 0, 0, 0, DateTimeKind.Local);
			TimeZone.IsNewYork();
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			
			IntervalLengthFetcher.Has(minutesPerInterval);
			var scenario = ScenarioFactory.CreateScenario("scenariorita", true, true).WithId();
			ScenarioRepository.Has(scenario);

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(0, 0, 24, 0));
			SkillRepository.Has(skill);

			var skillDay1 = skill.CreateSkillDayWithDemand(scenario, new DateOnly(userNow.AddDays(-1)), TimeSpan.FromMinutes(1440));
			var skillDay2 = skill.CreateSkillDayWithDemand(scenario, new DateOnly(userNow), TimeSpan.FromMinutes(1440));
			var skillDay3 = skill.CreateSkillDayWithDemand(scenario, new DateOnly(userNow.AddDays(1)), TimeSpan.FromMinutes(1440));
			SkillDayRepository.Add(skillDay1);
			SkillDayRepository.Add(skillDay2);
			SkillDayRepository.Add(skillDay3);

			var vm = Target.Load(new[] { skill.Id.Value });

			vm.DataSeries.Time.Length.Should().Be.EqualTo(96);
			vm.DataSeries.Time.First().Should().Be.EqualTo(userNow);
			vm.DataSeries.Time.Last().Should().Be.EqualTo(userNow.AddDays(1).AddMinutes(-minutesPerInterval));
			vm.DataSeries.ForecastedStaffing.Length.Should().Be.EqualTo(96);
		}

		[Test]
		public void ShouldReturnStaffingForUsersCurrentDayInAustraliaTimeZoneWhenOpen247()
		{
			var userNow = new DateTime(2016, 8, 26, 0, 0, 0, DateTimeKind.Local);
			TimeZone.IsAustralia();
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			IntervalLengthFetcher.Has(minutesPerInterval);
			var scenario = ScenarioFactory.CreateScenario("scenariorita", true, true).WithId();
			ScenarioRepository.Has(scenario);

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(0, 0, 24, 0));
			SkillRepository.Has(skill);

			var skillDay1 = skill.CreateSkillDayWithDemand(scenario, new DateOnly(userNow.AddDays(-1)), TimeSpan.FromMinutes(1440));
			var skillDay2 = skill.CreateSkillDayWithDemand(scenario, new DateOnly(userNow), TimeSpan.FromMinutes(1440));
			var skillDay3 = skill.CreateSkillDayWithDemand(scenario, new DateOnly(userNow.AddDays(1)), TimeSpan.FromMinutes(1440));
			SkillDayRepository.Add(skillDay1);
			SkillDayRepository.Add(skillDay2);
			SkillDayRepository.Add(skillDay3);

			var vm = Target.Load(new[] { skill.Id.Value });

			vm.DataSeries.Time.Length.Should().Be.EqualTo(96);
			vm.DataSeries.Time.First().Should().Be.EqualTo(userNow);
			vm.DataSeries.Time.Last().Should().Be.EqualTo(userNow.AddDays(1).AddMinutes(-minutesPerInterval));
			vm.DataSeries.ForecastedStaffing.Length.Should().Be.EqualTo(96);
		}

		[Test]
		public void ShouldReturnUpdatedStaffingForecastFromNowUntilEndOfDay()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			IntervalLengthFetcher.Has(minutesPerInterval);
			var scenario = ScenarioFactory.CreateScenario("scenariorita", true, true).WithId();
			ScenarioRepository.Has(scenario);

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30));
			SkillRepository.Has(skill);

			var skillDay = skill.CreateSkillDayWithDemand(scenario, new DateOnly(userNow), TimeSpan.FromMinutes(60));
			skillDay.WorkloadDayCollection.First().TaskPeriodList.First().Tasks = 20;
			skillDay.WorkloadDayCollection.First().TaskPeriodList.First().AverageTaskTime = TimeSpan.FromSeconds(100);
			skillDay.WorkloadDayCollection.First().TaskPeriodList.First().AverageAfterTaskTime = TimeSpan.FromSeconds(20);
			SkillDayRepository.Add(skillDay);

			var actualworkloadSeconds = (20 * 120) * 1.2; // 20% more workload than forecasted

			IntradayQueueStatisticsLoader.Has((int?) actualworkloadSeconds);

			var vm = Target.Load(new[] { skill.Id.Value });

			vm.DataSeries.Time.Length.Should().Be.EqualTo(2);
			vm.DataSeries.Time.First().Should().Be.EqualTo(userNow.AddMinutes(-15));
			vm.DataSeries.Time.Last().Should().Be.EqualTo(userNow);

			var forecastedWorkload = skillDay.WorkloadDayCollection.First().Tasks * 
											 (skillDay.WorkloadDayCollection.First().AverageTaskTime.TotalSeconds +
											  skillDay.WorkloadDayCollection.First().AverageAfterTaskTime.TotalSeconds);
			
			var deviationPercent = actualworkloadSeconds / forecastedWorkload;
			var expectedUpdatedForecastOnLastInterval = vm.DataSeries.ForecastedStaffing.Last() * deviationPercent;

			vm.DataSeries.UpdatedForecastedStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.UpdatedForecastedStaffing.First().Should().Be.EqualTo(null);
			vm.DataSeries.UpdatedForecastedStaffing.Last().Should().Be.EqualTo(expectedUpdatedForecastOnLastInterval);
		}

		[Test]
		public void ShouldReturnAllNullForUpdatedStaffingForecastWhenNoStats()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			IntervalLengthFetcher.Has(minutesPerInterval);
			var scenario = ScenarioFactory.CreateScenario("scenariorita", true, true).WithId();
			ScenarioRepository.Has(scenario);

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30));
			SkillRepository.Has(skill);

			var skillDay = skill.CreateSkillDayWithDemand(scenario, new DateOnly(userNow), TimeSpan.FromMinutes(60));
			SkillDayRepository.Add(skillDay);

			IntradayQueueStatisticsLoader.Has(null);

			var vm = Target.Load(new[] { skill.Id.Value });

			vm.DataSeries.UpdatedForecastedStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.UpdatedForecastedStaffing.First().Should().Be.EqualTo(null);
			vm.DataSeries.UpdatedForecastedStaffing.Last().Should().Be.EqualTo(null);
		}

		[Test]
		public void ShouldReturnEmptyDataSeriesWhenNoForecastOnSkill()
		{
			IntervalLengthFetcher.Has(minutesPerInterval);
			var scenario = ScenarioFactory.CreateScenario("scenariorita", true, true).WithId();
			ScenarioRepository.Has(scenario);

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30));
			SkillRepository.Has(skill);

			var vm = Target.Load(new[] { skill.Id.Value });

			vm.DataSeries.Time.Length.Should().Be.EqualTo(0);
			vm.DataSeries.ForecastedStaffing.Length.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldHandleDifferentIntervalLengthBetweenSkillAndView()
		{
			IntervalLengthFetcher.Has(minutesPerInterval);
			var scenario = ScenarioFactory.CreateScenario("scenariorita", true, true).WithId();
			ScenarioRepository.Has(scenario);

			var skill = createSkill(30, "skill", new TimePeriod(8, 0, 8, 30));
			SkillRepository.Has(skill);

			var skillDay = skill.CreateSkillDayWithDemand(scenario, new DateOnly(Now.UtcDateTime()), TimeSpan.FromMinutes(60));
			SkillDayRepository.Add(skillDay);

			var vm = Target.Load(new[] { skill.Id.Value });

			var staffingIntervals = skillDay.SkillStaffPeriodViewCollection(TimeSpan.FromMinutes(minutesPerInterval));
			vm.DataSeries.Time.Length.Should().Be.EqualTo(staffingIntervals.Count);
			vm.DataSeries.ForecastedStaffing.Length.Should().Be.EqualTo(staffingIntervals.Count);
		}

		[Test]
		public void ShouldSummariseStaffingWhenMoreThanOneSkill()
		{
			IntervalLengthFetcher.Has(minutesPerInterval);
			var scenario = ScenarioFactory.CreateScenario("scenariorita", true, true).WithId();
			ScenarioRepository.Has(scenario);

			var skill1 = createSkill(minutesPerInterval, "skill1", new TimePeriod(8, 0, 8, 30));
			var skill2 = createSkill(minutesPerInterval, "skill2", new TimePeriod(8, 0, 8, 30));
			SkillRepository.Has(skill1);
			SkillRepository.Has(skill2);

			var skillDay1 = skill1.CreateSkillDayWithDemand(scenario, new DateOnly(Now.UtcDateTime()), TimeSpan.FromMinutes(60));
			var skillDay2 = skill2.CreateSkillDayWithDemand(scenario, new DateOnly(Now.UtcDateTime()), TimeSpan.FromMinutes(30));
			SkillDayRepository.Add(skillDay1);
			SkillDayRepository.Add(skillDay2);

			var vm = Target.Load(new[] { skill1.Id.Value, skill2.Id.Value });

			var staffingIntervals1 = skillDay1.SkillStaffPeriodViewCollection(TimeSpan.FromMinutes(minutesPerInterval));
			var staffingIntervals2 = skillDay2.SkillStaffPeriodViewCollection(TimeSpan.FromMinutes(minutesPerInterval));

			vm.DataSeries.ForecastedStaffing.First().Should().Be.EqualTo(staffingIntervals1.First().FStaff + staffingIntervals2.First().FStaff);
			vm.DataSeries.ForecastedStaffing.Last().Should().Be.EqualTo(staffingIntervals1.Last().FStaff + staffingIntervals2.Last().FStaff);
		}

		private ISkill createSkill(int intervalLength, string skillName, TimePeriod openHours)
		{
			var activity = new Activity("activity_" + skillName).WithId();
			var skill =
				new Skill(skillName, skillName, Color.Empty, intervalLength, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony))
				{
					Activity = activity,
					TimeZone = TimeZoneInfo.Utc
				}.WithId();
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, openHours);

			return skill;
		}
	}
}