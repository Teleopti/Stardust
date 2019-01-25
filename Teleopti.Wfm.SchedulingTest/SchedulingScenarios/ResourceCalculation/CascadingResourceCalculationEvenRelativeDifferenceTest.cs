using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.ResourceCalculation
{
	[DomainTest]
	public class CascadingResourceCalculationEvenRelativeDifferenceTest : ResourceCalculationScenario
	{
		public ResourceCalculateWithNewContext Target;

		[Test]
		public void ShouldTryToMakeSubSkillHaveSameRelativeDifferenceAfterShoveling_BothSkillSameDiffAtStart([Values(2, 100)] double skillB2Demand, [Values(true, false)] bool primarySkillOpened)
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skillA = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1);
			if(primarySkillOpened)
			{
				skillA.IsOpenBetween(8, 9);
			}
			else
			{
				skillA.IsClosed();
			}
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
		public void ShouldTryToMakeSubSkillHaveSameRelativeDifferenceAfterShoveling_SkillsDiffrerentDiffAtStart([Values(true, false)] bool primarySkillOpened)
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skillA = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1);
			if (primarySkillOpened)
			{
				skillA.IsOpenBetween(8, 9);
			}
			else
			{
				skillA.IsClosed();
			}
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 0);
			var skillB1 = new Skill("B1").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillDayB1 = skillB1.CreateSkillDayWithDemand(scenario, dateOnly, 8);
			var skillB2 = new Skill("B2").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillDayB2 = skillB2.CreateSkillDayWithDemand(scenario, dateOnly, 7);
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

		[Test]
		public void ShouldNotRemoveSingleSkilledAgentWhenShovling()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skillA = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 0);
			var skillB1 = new Skill("B1").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillDayB1 = skillB1.CreateSkillDayWithDemand(scenario, dateOnly, 1.1); //här ligger redan en resurs = ~9%
			var skillB2 = new Skill("B2").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillDayB2 = skillB2.CreateSkillDayWithDemand(scenario, dateOnly, 10); //innan shovling = -100% rel diff
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA, skillB1, skillB2);
			var ass = new PersonAssignment(agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));
			var singleSkilledAgent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillB1);
			var singleAss = new PersonAssignment(singleSkilledAgent, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass, singleAss }, new[] { skillDayA, skillDayB1, skillDayB2 }, false, false));

			skillDayB1.SkillStaffPeriodCollection.First().CalculatedResource
				.Should().Be.GreaterThanOrEqualTo(1);
		}

		[Test]
		public void ShouldNotShovelMoreResourcesThanAvailableInPrimarySkill()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skillA = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 0);
			var skillB1 = new Skill("B1").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillDayB1 = skillB1.CreateSkillDayWithDemand(scenario, dateOnly, 1.1);
			var skillB2 = new Skill("B2").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillDayB2 = skillB2.CreateSkillDayWithDemand(scenario, dateOnly, 10); //innan shovling = -100% rel diff
			var skillB3 = new Skill("B3").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillDayB3 = skillB3.CreateSkillDayWithDemand(scenario, dateOnly, 10); //innan shovling = -100% rel diff
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA, skillB1, skillB2, skillB3);
			var ass = new PersonAssignment(agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(8, 9));
			var singleSkilledAgent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillB1);
			var singleAss = new PersonAssignment(singleSkilledAgent, scenario, dateOnly).WithLayer(activity, new TimePeriod(8, 9));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass, singleAss }, new[] { skillDayA, skillDayB1, skillDayB2, skillDayB3 }, false, false));

			(skillDayB2.SkillStaffPeriodCollection.First().CalculatedResource + skillDayB3.SkillStaffPeriodCollection.First().CalculatedResource)
				.Should().Be.LessThanOrEqualTo(1);
		}

		[Test]
		public void ShouldNotShovelEqualResourcesToSubskill()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skillA = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 0);
			var skillB1 = new Skill("B1").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillDayB1 = skillB1.CreateSkillDayWithDemand(scenario, dateOnly, 1.1); 
			var skillB2 = new Skill("B2").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillDayB2 = skillB2.CreateSkillDayWithDemand(scenario, dateOnly, 10); //innan shovling = -100% rel diff
			var skillB3 = new Skill("B3").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillDayB3 = skillB3.CreateSkillDayWithDemand(scenario, dateOnly, 10); //innan shovling = -100% rel diff
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA, skillB1, skillB2, skillB3);
			var ass = new PersonAssignment(agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(8, 9));
			var singleSkilledAgent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillB1);
			var singleAss = new PersonAssignment(singleSkilledAgent, scenario, dateOnly).WithLayer(activity, new TimePeriod(8, 9));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass, singleAss }, new[] { skillDayA, skillDayB1, skillDayB2, skillDayB3 }, false, false));

			skillDayB2.SkillStaffPeriodCollection.First().CalculatedResource
				.Should().Be.EqualTo(skillDayB3.SkillStaffPeriodCollection.First().CalculatedResource);
		}

		[Test]
		public void ShouldShovelAllResourceToOneSubSkill()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skillA = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 0);
			var skillB1 = new Skill("B1").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillDayB1 = skillB1.CreateSkillDayWithDemand(scenario, dateOnly, 10);
			var skillB2 = new Skill("B2").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillDayB2 = skillB2.CreateSkillDayWithDemand(scenario, dateOnly, 10); 
			var skillB3 = new Skill("B3").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillDayB3 = skillB3.CreateSkillDayWithDemand(scenario, dateOnly, 10);
			var asses = new List<IPersonAssignment>();
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA, skillB1, skillB2, skillB3);
			asses.Add(new PersonAssignment(agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(8, 9)));
			for (var i = 0; i < 10; i++)
			{
				var agentSingleSkilledB1 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillB1);
				asses.Add(new PersonAssignment(agentSingleSkilledB1, scenario, dateOnly).WithLayer(activity, new TimePeriod(8, 9)));
			}
			for (var i = 0; i < 6; i++)
			{
				var agentSingleSkilledB2 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillB2);
				asses.Add(new PersonAssignment(agentSingleSkilledB2, scenario, dateOnly).WithLayer(activity, new TimePeriod(8, 9)));
			}
			var agentSingleSkilledB3 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillB3);
			asses.Add(new PersonAssignment(agentSingleSkilledB3, scenario, dateOnly).WithLayer(activity, new TimePeriod(8, 9)));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, asses, new[] { skillDayA, skillDayB1, skillDayB2, skillDayB3 }, false, false));

			skillDayB3.SkillStaffPeriodCollection.First().CalculatedResource
				.Should().Be.EqualTo(2); //all of "agent" shoveled here
		}
	}
}