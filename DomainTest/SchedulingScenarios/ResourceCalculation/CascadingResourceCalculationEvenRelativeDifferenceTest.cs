using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.ResourceCalculation
{
	[DomainTest]
	[Toggle(Toggles.ResourcePlanner_EvenRelativeDiff_44091)]
	public class CascadingResourceCalculationEvenRelativeDifferenceTest
	{
		public IResourceCalculation Target;

		[Test]
		public void ShouldTryToMakeSubSkillHaveSameRelativeDifferenceAfterShoveling_BothSkillSameDiffAtStart([Values(2, 100)] double skillB2Demand)
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skillA = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 0);
			var skillB1 = new Skill("B1").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillDayB1 = skillB1.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var skillB2 = new Skill("B2").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillDayB2 = skillB2.CreateSkillDayWithDemand(scenario, dateOnly, skillB2Demand);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA, skillB1, skillB2);
			var ass = new PersonAssignment(agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass }, new[] { skillDayA, skillDayB1, skillDayB2 }, false, false));

			var b1RelativeDiff = skillDayB1.SkillStaffPeriodCollection.First().RelativeDifference;
			var b2RelativeDiff = skillDayB2.SkillStaffPeriodCollection.First().RelativeDifference;
			var diffBetweenB1AndB2 = Math.Abs(b1RelativeDiff - b2RelativeDiff);
			diffBetweenB1AndB2.Should().Be.LessThan(0.1);
		}

		[Test]
		public void ShouldTryToMakeSubSkillHaveSameRelativeDifferenceAfterShoveling_SkillsDiffrerentDiffAtStart([Values(2, 100)] double skillB2Demand)
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skillA = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 0);
			var skillB1 = new Skill("B1").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillDayB1 = skillB1.CreateSkillDayWithDemand(scenario, dateOnly, 2);
			var skillB2 = new Skill("B2").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillDayB2 = skillB2.CreateSkillDayWithDemand(scenario, dateOnly, skillB2Demand);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA, skillB1, skillB2);
			var ass = new PersonAssignment(agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));
			var singleSkilledAgent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillB1);
			var singleAss = new PersonAssignment(singleSkilledAgent, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass, singleAss }, new[] { skillDayA, skillDayB1, skillDayB2 }, false, false));

			var b1RelativeDiff = skillDayB1.SkillStaffPeriodCollection.First().RelativeDifference;
			var b2RelativeDiff = skillDayB2.SkillStaffPeriodCollection.First().RelativeDifference;
			var diffBetweenB1AndB2 = Math.Abs(b1RelativeDiff - b2RelativeDiff);
			diffBetweenB1AndB2.Should().Be.LessThan(0.1);
		}
	}
}