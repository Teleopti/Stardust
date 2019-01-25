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
	public class CascadingResourceCalculationClosedSkillsTest : ResourceCalculationScenario
	{
		public ResourceCalculateWithNewContext Target;

		[Test]
		public void ShouldNotConsiderSkillsWithoutOpenHours()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var prioritizedSkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsClosed();
			var prioritizedSkillDay = prioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var nonPrioritizedSkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var nonPrioritizedSkillDay = nonPrioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(prioritizedSkill, nonPrioritizedSkill);
			var ass = new PersonAssignment(agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass }, new[] { prioritizedSkillDay, nonPrioritizedSkillDay }, false, false));

			nonPrioritizedSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldMoveResourcesOutsideOpenHoursFromPrimarySkill()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var prioritizedSkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var prioritizedSkillDay = prioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var nonPrioritizedSkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 10);
			var nonPrioritizedSkillDay = nonPrioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(prioritizedSkill, nonPrioritizedSkill);
			var ass = new PersonAssignment(agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(8, 10));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass }, new[] { prioritizedSkillDay, nonPrioritizedSkillDay }, false, false));

			prioritizedSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			nonPrioritizedSkillDay.SkillStaffPeriodCollection.Last().AbsoluteDifference
				.Should().Be.EqualTo(0);
		}


		[Test]
		public void ShouldShovelAllResourcesFromClosedPrimarySkillNoMatterDemandOnSubskill()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var primarySkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var subskill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 10);
			var primarySkillDay = primarySkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var subskillDay = subskill.CreateSkillDayWithDemand(scenario, dateOnly, 0.5);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(primarySkill, subskill);
			var ass = new PersonAssignment(agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(9, 10));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass }, new[] { primarySkillDay, subskillDay }, false, false));

			subskillDay.SkillStaffPeriodCollection.Last().CalculatedResource
				.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldShovelAllResourcesFromClosedPrimarySkillEvenIfSubskillIsOverstaffed()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var primarySkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var subskill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 10);
			var primarySkillDay = primarySkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var subskillDay = subskill.CreateSkillDayWithDemand(scenario, dateOnly, 0.5);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(primarySkill, subskill);
			var ass = new PersonAssignment(agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(9, 10));
			var agentOnSubskill = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(subskill);
			var assOnSubskill = new PersonAssignment(agentOnSubskill, scenario, dateOnly).WithLayer(activity, new TimePeriod(9, 10));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass, assOnSubskill }, new[] { primarySkillDay, subskillDay }, false, false));

			subskillDay.SkillStaffPeriodCollection.Last().CalculatedResource
				.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldShovelAllResourcesFromClosedPrimarySkillToSubskillWithDemand()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var primarySkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var subskill1 = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 10);
			var subskill2 = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 10);
			var primarySkillDay = primarySkill.CreateSkillDayWithDemand(scenario, dateOnly, 100);
			var subskill1Day = subskill1.CreateSkillDayWithDemand(scenario, dateOnly, 0.5);
			var subskill2Day = subskill2.CreateSkillDayWithDemand(scenario, dateOnly, 0);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(primarySkill, subskill1, subskill2);
			var ass = new PersonAssignment(agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(9, 10));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass }, new[] { primarySkillDay, subskill1Day, subskill2Day }, false, false));

			var skillDay1Resources = subskill1Day.SkillStaffPeriodCollection.Last().CalculatedResource;
			var skillDay2Resources = subskill2Day.SkillStaffPeriodCollection.Last().CalculatedResource;
			(skillDay1Resources + skillDay2Resources).Should().Be.EqualTo(1);
			skillDay1Resources.Should().Be.GreaterThan(0.5);
			skillDay2Resources.Should().Be.GreaterThan(0);
		}

		[Test]
		public void ShouldShovelResourcesFromClosedPrimarySkillToSubskillWithDemandBeforeSubskillWithNoDemand()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var primarySkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var subskill1 = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 10);
			var subskill2 = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 10);
			var primarySkillDay = primarySkill.CreateSkillDayWithDemand(scenario, dateOnly, 100);
			var subskill1Day = subskill1.CreateSkillDayWithDemand(scenario, dateOnly, 0);
			var subskill2Day = subskill2.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(primarySkill, subskill1, subskill2);
			var ass = new PersonAssignment(agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(9, 10));
			var agentOnSubskill = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(subskill1);
			var assOnSubskill = new PersonAssignment(agentOnSubskill, scenario, dateOnly).WithLayer(activity, new TimePeriod(9, 10));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass, assOnSubskill }, new[] { primarySkillDay, subskill1Day, subskill2Day }, false, false));

			var skillDay1Resources = subskill1Day.SkillStaffPeriodCollection.Last().CalculatedResource;
			var skillDay2Resources = subskill2Day.SkillStaffPeriodCollection.Last().CalculatedResource;
			(skillDay1Resources + skillDay2Resources).Should().Be.EqualTo(2);
			skillDay1Resources.Should().Be.EqualTo(1);
			skillDay2Resources.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldShovelResourcesFromClosedPrimarySkill_ResultingInAllSubSkillsOverStaffed()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var primarySkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var subskill1 = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 10);
			var subskill2 = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 10);
			var primarySkillDay = primarySkill.CreateSkillDayWithDemand(scenario, dateOnly, 100);
			var subskill1Day = subskill1.CreateSkillDayWithDemand(scenario, dateOnly, 0.9); //0.1 overstaffed
			var subskill2Day = subskill2.CreateSkillDayWithDemand(scenario, dateOnly, 0.1); //0.1 understaffed
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(primarySkill, subskill1, subskill2);
			var ass = new PersonAssignment(agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(9, 10));
			var agentOnSubskill = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(subskill1);
			var assOnSubskill = new PersonAssignment(agentOnSubskill, scenario, dateOnly).WithLayer(activity, new TimePeriod(9, 10));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass, assOnSubskill }, new[] { primarySkillDay, subskill1Day, subskill2Day }, false, false));

			var skillDay1Resources = subskill1Day.SkillStaffPeriodCollection.Last().CalculatedResource;
			var skillDay2Resources = subskill2Day.SkillStaffPeriodCollection.Last().CalculatedResource;
			(skillDay1Resources + skillDay2Resources).Should().Be.EqualTo(2);
			skillDay1Resources.Should().Be.GreaterThan(1.4);
			skillDay2Resources.Should().Be.GreaterThan(0.5);
		}

		[Test]
		public void ShouldShovelAllResourcesFromClosedPrimaryToSubSkillEvenIfNoneIsUnderstaffed()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var primarySkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var subskill1 = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 10);
			var subskill2 = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 10);
			var primarySkillDay = primarySkill.CreateSkillDayWithDemand(scenario, dateOnly, 100);
			var subskill1Day = subskill1.CreateSkillDayWithDemand(scenario, dateOnly, 0);
			var subskill2Day = subskill2.CreateSkillDayWithDemand(scenario, dateOnly, 0);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(primarySkill, subskill1, subskill2);
			var ass = new PersonAssignment(agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(9, 10));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass }, new[] { primarySkillDay, subskill1Day, subskill2Day }, false, false));

			var skillDay1Resources = subskill1Day.SkillStaffPeriodCollection.Last().CalculatedResource;
			var skillDay2Resources = subskill2Day.SkillStaffPeriodCollection.Last().CalculatedResource;
			skillDay1Resources.Should().Be.EqualTo(0.5);
			skillDay2Resources.Should().Be.EqualTo(0.5);
		}

		[Test]
		public void ShouldShovelAllResourcesWhenAgentsBelongToDifferentSkillSets()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var nonCascadingSkill1 = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsClosed();
			var nonCascadingSkill2 = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsClosed();
			var primarySkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var subskill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 10);
			var primarySkillDay = primarySkill.CreateSkillDayWithDemand(scenario, dateOnly, 100);
			var subskillDay = subskill.CreateSkillDayWithDemand(scenario, dateOnly, 0);
			var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(primarySkill, subskill, nonCascadingSkill1);
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly).WithLayer(activity, new TimePeriod(9, 10));
			var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(primarySkill, subskill, nonCascadingSkill2);
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly).WithLayer(activity, new TimePeriod(9, 10));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass1, ass2 }, new[] { primarySkillDay, subskillDay }, false, false));

			subskillDay.SkillStaffPeriodCollection.Last().CalculatedResource
				.Should().Be.EqualTo(2);
		}


		[Test]
		public void ShouldNotLooseResourcesWhenDemandOnSubSkillIsCloseToNumberOfResources()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var primarySkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsClosed();
			var subskill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(9, 10);
			var primarySkillDay = primarySkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var subskillDay = subskill.CreateSkillDayWithDemand(scenario, dateOnly, 0.99); //fails when close to 1
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(primarySkill, subskill);
			var ass = new PersonAssignment(agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(9, 10));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass }, new[] { primarySkillDay, subskillDay }, false, false));

			subskillDay.SkillStaffPeriodCollection.First().CalculatedResource
				.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldConsiderSkillSetsAlsoWhenPrimarySkillIsClosed([Values(0, 100)] int demandOnSubskill1, [Values(0, 100)] int demandOnSubskill2)
		{
			const int numberOfB1Agents = 3;
			const int numberOfB2Agents = 5;
			
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skillA = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsClosed();
			var skillB1 = new Skill("B1").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 10);
			var skillB2 = new Skill("B2").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 10);
			var skillADay = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 100);
			var skillB1Day = skillB1.CreateSkillDayWithDemand(scenario, dateOnly, demandOnSubskill1); //either over or understaffed depending on inparam
			var skillB2Day = skillB2.CreateSkillDayWithDemand(scenario, dateOnly, demandOnSubskill2); //either over or understaffed depending on inparam
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