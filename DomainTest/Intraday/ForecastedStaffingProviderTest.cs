using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Intraday
{
	[DomainTest]
	public class ForecastedStaffingProviderTest
	{
		public ForecastedStaffingProvider Target;
		public FakeScenarioRepository ScenarioRepository;
		public FakeSkillRepository SkillRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;

		private const int minutesPerInterval = 15;
		
		[Test]
		public void ShouldReturnStaffingForTwoIntervals()
		{
			IntervalLengthFetcher.Has(minutesPerInterval);
			var scenario = ScenarioFactory.CreateScenario("scenariorita", true, true).WithId();
			ScenarioRepository.Has(scenario);

			var skill = createSkill(minutesPerInterval, "skill");
			SkillRepository.Has(skill);

			var skillDay = skill.CreateSkillDayWithDemand(scenario, DateOnly.Today, TimeSpan.FromMinutes(60));
			SkillDayRepository.Add(skillDay);

			var vm = Target.Load(new[] { skill.Id.Value });

			var staffingIntervals = skillDay.SkillStaffPeriodViewCollection(TimeSpan.FromMinutes(minutesPerInterval));
			vm.DataSeries.Time.Length.Should().Be.EqualTo(2);
			vm.DataSeries.Time.First().Should().Be.EqualTo(staffingIntervals.First().Period.StartDateTime);
			vm.DataSeries.Time.Last().Should().Be.EqualTo(staffingIntervals.Last().Period.StartDateTime);
			vm.DataSeries.ForecastedStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.ForecastedStaffing.First().Should().Be.GreaterThan(0d);
			vm.DataSeries.ForecastedStaffing.Last().Should().Be.GreaterThan(0d);
		}

		[Test]
		public void ShouldHandleDifferentIntervalLengthBetweenSkillAndView()
		{
			IntervalLengthFetcher.Has(minutesPerInterval);
			var scenario = ScenarioFactory.CreateScenario("scenariorita", true, true).WithId();
			ScenarioRepository.Has(scenario);

			var skill = createSkill(30, "skill");
			SkillRepository.Has(skill);

			var skillDay = skill.CreateSkillDayWithDemand(scenario, DateOnly.Today, TimeSpan.FromMinutes(60));
			SkillDayRepository.Add(skillDay);

			var vm = Target.Load(new[] { skill.Id.Value });

			var staffingIntervals = skillDay.SkillStaffPeriodViewCollection(TimeSpan.FromMinutes(minutesPerInterval));
			vm.DataSeries.Time.Length.Should().Be.EqualTo(staffingIntervals.Count);
			vm.DataSeries.ForecastedStaffing.Length.Should().Be.EqualTo(staffingIntervals.Count);
		}

		[Test]
		public void ShouldSummariseStaffingOfSkills()
		{
			IntervalLengthFetcher.Has(minutesPerInterval);
			var scenario = ScenarioFactory.CreateScenario("scenariorita", true, true).WithId();
			ScenarioRepository.Has(scenario);

			var skill1 = createSkill(minutesPerInterval, "skill1");
			var skill2 = createSkill(minutesPerInterval, "skill2");
			SkillRepository.Has(skill1);
			SkillRepository.Has(skill2);

			var skillDay1 = skill1.CreateSkillDayWithDemand(scenario, DateOnly.Today, TimeSpan.FromMinutes(60));
			var skillDay2 = skill2.CreateSkillDayWithDemand(scenario, DateOnly.Today, TimeSpan.FromMinutes(30));
			SkillDayRepository.Add(skillDay1);
			SkillDayRepository.Add(skillDay2);

			var vm = Target.Load(new[] { skill1.Id.Value, skill2.Id.Value });

			var staffingIntervals1 = skillDay1.SkillStaffPeriodViewCollection(TimeSpan.FromMinutes(minutesPerInterval));
			var staffingIntervals2 = skillDay2.SkillStaffPeriodViewCollection(TimeSpan.FromMinutes(minutesPerInterval));

			vm.DataSeries.ForecastedStaffing.First().Should().Be.EqualTo(staffingIntervals1.First().FStaff + staffingIntervals2.First().FStaff);
			vm.DataSeries.ForecastedStaffing.Last().Should().Be.EqualTo(staffingIntervals1.Last().FStaff + staffingIntervals2.Last().FStaff);
		}

		private ISkill createSkill(int intervalLength, string skillName)
		{
			var activity = new Activity("activity_" + skillName).WithId();
			var skill =
				new Skill(skillName, skillName, Color.Empty, intervalLength, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony))
				{
					Activity = activity,
					TimeZone = TimeZoneInfo.Utc
				}.WithId();
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, new TimePeriod(8, 0, 8, 30));

			return skill;
		}
	}
}