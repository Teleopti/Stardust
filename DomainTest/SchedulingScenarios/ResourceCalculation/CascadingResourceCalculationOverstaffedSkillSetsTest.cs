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
	public class CascadingResourceCalculationOverstaffedSkillSetsTest : ResourceCalculationScenario
	{
		public ResourceCalculateWithNewContext Target;

		[Test]
		public void ShouldMoveResourceOnlyWithinSkillSet()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skillA = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var skillADay = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var skillB = new Skill("B").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillBDay = skillB.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var skillC = new Skill("C").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(3).IsOpenBetween(8, 9);
			var skillCDay = skillC.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA, skillC);
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));
			var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA, skillC);
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass1, ass2 }, new[] { skillADay, skillBDay, skillCDay }, false, false));

			skillADay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			skillBDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(-1);
			skillCDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldMoveResourceFromLowestSkillSetFirst()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skillA = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var skillADay = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 0.5);
			var skillB = new Skill("B").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillBDay = skillB.CreateSkillDayWithDemand(scenario, dateOnly, 0.5);
			var skillC = new Skill("C").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(3).IsOpenBetween(8, 9);
			var skillCDay = skillC.CreateSkillDayWithDemand(scenario, dateOnly, 0.5);
			var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA, skillC);
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));
			var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillB, skillC);
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass1, ass2 }, new[] { skillADay, skillBDay, skillCDay }, false, false));

			skillADay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0.5);
			skillBDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			skillCDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldMoveResourcesFromLowestSkillSet_DifferentLowestSkill()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skillA = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var skillADay = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 0.5);
			var skillB = new Skill("B").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillBDay = skillB.CreateSkillDayWithDemand(scenario, dateOnly, 0.5);
			var skillC = new Skill("C").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(3).IsOpenBetween(8, 9);
			var skillCDay = skillC.CreateSkillDayWithDemand(scenario, dateOnly, 0.5);
			var skillD = new Skill("D").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(4).IsOpenBetween(8, 9);
			var skillDDay = skillD.CreateSkillDayWithDemand(scenario, dateOnly, 0);
			var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA, skillC, skillD);
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));
			var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillB, skillC);
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass1, ass2 }, new[] { skillADay, skillBDay, skillCDay, skillDDay }, false, false));

			skillADay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0.5);
			skillBDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			skillCDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			skillDDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotMoveMoreResourcesThanAvailableInSameSkillSet()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skillA = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var skillADay = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var skillB = new Skill("B").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillBDay = skillB.CreateSkillDayWithDemand(scenario, dateOnly, 2);
			var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA, skillB);
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));
			var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA);
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));
			var agent3 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA);
			var ass3 = new PersonAssignment(agent3, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass1, ass2, ass3 }, new[] { skillADay, skillBDay }, false, false));

			skillADay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(1);
			skillBDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldNotMoveMoreResourcesThanAvailableInSameSkillSet_PartOfResourceNeedsToBeMoved()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skillA = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var skillADay = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 0.5);
			var skillB = new Skill("B").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillBDay = skillB.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var skillC = new Skill("C").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(3).IsOpenBetween(8, 9);
			var skillCDay = skillC.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA, skillB, skillC);
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));
			var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA);
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass1, ass2 }, new[] { skillADay, skillBDay, skillCDay }, false, false));

			skillADay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0.5);
			skillBDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			skillCDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldHandleCaseWhenSkillWithinCacsadingChainIsOverstaffed()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skillA = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var skillADay = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var skillB = new Skill("B").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillBDay = skillB.CreateSkillDayWithDemand(scenario, dateOnly, 0.5);
			var skillC = new Skill("C").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(3).IsOpenBetween(8, 9);
			var skillCDay = skillC.CreateSkillDayWithDemand(scenario, dateOnly, 0.5);
			var skillD = new Skill("D").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(4).IsOpenBetween(8, 9);
			var skillDDay = skillD.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA, skillB, skillC, skillD);
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));
			var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA, skillB, skillC, skillD);
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));
			var agent3 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillC);
			var ass3 = new PersonAssignment(agent3, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass1, ass2, ass3 }, new[] { skillADay, skillBDay, skillCDay, skillDDay }, false, false));

			skillADay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			skillBDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			skillCDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0.5);
			skillDDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(-0.5);
		}

		[Test]
		public void ShouldMoveResourcesToHighestFollowingSkillWhenSkillSetsHavingSamePrimarySkill()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skillA = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var skillADay = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var skillB = new Skill("B").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillBDay = skillB.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var skillC = new Skill("C").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(3).IsOpenBetween(8, 9);
			var skillCDay = skillC.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA, skillB);
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));
			var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA, skillC);
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass1, ass2 }, new[] { skillADay, skillBDay, skillCDay }, false, false));

			skillADay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			skillBDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			skillCDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldMoveResourceOnlyWithinSkillSetWhenParallellSubskillsExists()
		{
			const int numberOfB1Agents = 3;
			const int numberOfB2Agents = 5;

			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skillA = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(9, 10);
			var skillB1 = new Skill("B1").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(9, 10);
			var skillB2 = new Skill("B2").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(9, 10);
			var skillADay = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 0);
			var skillB1Day = skillB1.CreateSkillDayWithDemand(scenario, dateOnly, 100);
			var skillB2Day = skillB2.CreateSkillDayWithDemand(scenario, dateOnly, 100);
			var asses = new List<IPersonAssignment>();
			for (var i = 0; i < numberOfB1Agents; i++)
			{
				var skillB1Agent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA, skillB1);
				asses.Add(new PersonAssignment(skillB1Agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(9, 10)));
			}
			for (var i = 0; i < numberOfB2Agents; i++)
			{
				var skillB2Agent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA, skillB2);
				asses.Add(new PersonAssignment(skillB2Agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(9, 10)));
			}

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, asses, new[] { skillADay, skillB1Day, skillB2Day }, false, false));

			skillB1Day.SkillStaffPeriodCollection.Last().CalculatedResource
				.Should().Be.EqualTo(numberOfB1Agents);
			skillB2Day.SkillStaffPeriodCollection.Last().CalculatedResource
				.Should().Be.EqualTo(numberOfB2Agents);
		}
	}
}