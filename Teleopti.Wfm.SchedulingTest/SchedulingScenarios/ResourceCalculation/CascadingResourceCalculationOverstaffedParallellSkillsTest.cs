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
	public class CascadingResourceCalculationOverstaffedParallellSkillsTest : ResourceCalculationScenario
	{
		public ResourceCalculateWithNewContext Target;

		[Test]
		public void ShouldMoveResourceToTwoSkillsWithSameDemand()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skillA = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 0.5);
			var skillB1 = new Skill("B1").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillDayB1 = skillB1.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var skillB2 = new Skill("B2").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillDayB2 = skillB2.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA, skillB1, skillB2);
			var ass = new PersonAssignment(agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new [] {ass}, new[] { skillDayA, skillDayB1, skillDayB2 }, false, false));

			skillDayA.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			skillDayB1.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(-0.75);
			skillDayB2.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(-0.75);
		}

		[Test]
		public void ShouldMoveAllResourcesToOneCascadingSkillIfOnlyThatSkillHasDemand()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skillA = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 0.5);
			var skillB1 = new Skill("B1").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillDayB1 = skillB1.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var skillB2 = new Skill("B2").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillDayB2 = skillB2.CreateSkillDayWithDemand(scenario, dateOnly, 0);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA, skillB1, skillB2);
			var ass = new PersonAssignment(agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass }, new[] { skillDayA, skillDayB1, skillDayB2 }, false, false));

			skillDayA.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			skillDayB1.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(-0.5);
			skillDayB2.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldMoveAllResourcesToOneCascadingSkillIfOtherParallelSkillIsOverstaffed()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skillA = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 0);
			var skillB1 = new Skill("B1").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillDayB1 = skillB1.CreateSkillDayWithDemand(scenario, dateOnly, 10);
			var skillB2 = new Skill("B2Overstaffed").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillDayB2 = skillB2.CreateSkillDayWithDemand(scenario, dateOnly, 0.5);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA, skillB1, skillB2);
			var agentB2 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillB2);
			var ass = new PersonAssignment(agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));
			var assB2 = new PersonAssignment(agentB2, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass, assB2 }, new[] { skillDayA, skillDayB1, skillDayB2 }, false, false));
			
			skillDayA.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			skillDayB1.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(-9);
			skillDayB2.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0.5);
		}

		[Test]
		public void ShouldNotMoveResourcesFromUnderstaffedPrimarySkill()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skillA1 = new Skill("skillA1").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var skillDayA1 = skillA1.CreateSkillDayWithDemand(scenario, dateOnly, 0); //-> lite mer än 1 överbemannat
			var skillA2 = new Skill("skillA2").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var skillDayA2 = skillA2.CreateSkillDayWithDemand(scenario, dateOnly, 2); //-> lite mer än 1 underbemannat
			var skillB = new Skill("skillB").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillDayB = skillB.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var multiSkilledAgent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA1, skillA2, skillB);
			var multiSkilledAss = new PersonAssignment(multiSkilledAgent, scenario, dateOnly).WithLayer(activity, new TimePeriod(8, 9));
			var agentA1 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA1);
			var a1Ass = new PersonAssignment(agentA1, scenario, dateOnly).WithLayer(activity, new TimePeriod(8, 9));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new [] {multiSkilledAss, a1Ass}, new[] { skillDayA1, skillDayA2, skillDayB }, false, false));

			skillDayA1.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.GreaterThanOrEqualTo(1); // should be exactly 1 but fix that in later test. This assert verifies that single skilled guy isn't removed
			skillDayA2.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.LessThanOrEqualTo(-1);
			skillDayB.SkillStaffPeriodCollection.First().AbsoluteDifference
					.Should().Be.IncludedIn(-1, -0.99); // -1 + some resources från A1 that comes from multi skilled guy
		}


		[Test]
		public void ShouldMoveResourcesFromParallelPrimarySkillsWithDifferentDemand()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skillA1 = new Skill("skillA1").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var skillDayA1 = skillA1.CreateSkillDayWithDemand(scenario, dateOnly, 0.2);
			var skillA2 = new Skill("skillA2").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var skillDayA2 = skillA2.CreateSkillDayWithDemand(scenario, dateOnly, 0.3);
			var skillB = new Skill("skillB").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillDayB = skillB.CreateSkillDayWithDemand(scenario, dateOnly, 0.3);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA1, skillA2, skillB);
			var ass = new PersonAssignment(agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(8, 9));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass }, new[] { skillDayA1, skillDayA2, skillDayB }, false, false));

			skillDayA1.SkillStaffPeriodCollection.First().AbsoluteDifference
				  .Should().Be.IncludedIn(0.079, 0.081); //double rounding errors
			skillDayA2.SkillStaffPeriodCollection.First().AbsoluteDifference
				  .Should().Be.IncludedIn(0.119, 0.121); //double rounding errors
			skillDayB.SkillStaffPeriodCollection.First().AbsoluteDifference
				  .Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldOnlyConsiderOpenParalellPrimarySkillsWhenMovingResourcesToSubSkill()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skillA1 = new Skill("skillA1").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(7, 9);
			var skillDayA1 = skillA1.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var skillA2 = new Skill("skillA2").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var skillDayA2 = skillA2.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var skillB = new Skill("skillB").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(7, 9);
			var skillDayB = skillB.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA1, skillA2, skillB);
			var ass = new PersonAssignment(agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(7, 8));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass }, new[] { skillDayA1, skillDayA2, skillDayB }, false, false));

			skillDayA1.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			skillDayB.SkillStaffPeriodCollection.First().AbsoluteDifference
				  .Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldGetResourcesFromSkillSetWhenAllPrimarySkillsAreClosed()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skillA1 = new Skill("skillA1").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var skillDayA1 = skillA1.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var skillA2 = new Skill("skillA2").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var skillDayA2 = skillA2.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var skillB = new Skill("skillB").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(7, 9);
			var skillDayB = skillB.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA1, skillA2, skillB);
			var ass = new PersonAssignment(agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(7, 8));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass }, new[] { skillDayA1, skillDayA2, skillDayB }, false, false));

			skillDayA1.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(-1);
			skillDayB.SkillStaffPeriodCollection.First().AbsoluteDifference
				  .Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldMoveResourcesFromPrimarySkillWhileSubSkillIsUnderStaffed([Values(true, false)] bool b1BeforeB2)
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skillA = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 0);
			var skillB1 = new Skill("B1").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillDayB1 = skillB1.CreateSkillDayWithDemand(scenario, dateOnly, 10);
			var skillB2 = new Skill("B2").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillDayB2 = skillB2.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var asses = new List<IPersonAssignment>();
			for (var i = 0; i < 5; i++)
			{
				var agent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA, skillB1, skillB2);
				asses.Add(new PersonAssignment(agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(8, 9)));
				var agentB1 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillB1);
				asses.Add(new PersonAssignment(agentB1, scenario, dateOnly).WithLayer(activity, new TimePeriod(8, 9)));
			}
			var skillDays = b1BeforeB2 ? new[] {skillDayA, skillDayB1, skillDayB2} : new[] {skillDayA, skillDayB2, skillDayB1};

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, asses, skillDays, false, false));

			skillDayA.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			skillDayB1.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.IncludedIn(-1, 0);
			skillDayB2.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.IncludedIn(-1, 0);
		}

		[Test]
		public void ShouldStopExecutingWhenNoSubSkillIsUnderStaffed([Values(true, false)] bool b1BeforeB2)
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skillA = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 0);
			var skillB1 = new Skill("B1").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillDayB1 = skillB1.CreateSkillDayWithDemand(scenario, dateOnly, 10);
			var skillB2 = new Skill("B2").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillDayB2 = skillB2.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var asses = new List<IPersonAssignment>();
			for (var i = 0; i < 20; i++)
			{
				var agent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA, skillB1, skillB2);
				asses.Add(new PersonAssignment(agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(8, 9)));
			}
			for (var i = 0; i < 5; i++)
			{
				var agentB1 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillB1);
				asses.Add(new PersonAssignment(agentB1, scenario, dateOnly).WithLayer(activity, new TimePeriod(8, 9)));
			}
			var skillDays = b1BeforeB2 ? new[] { skillDayA, skillDayB1, skillDayB2 } : new[] { skillDayA, skillDayB2, skillDayB1 };

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, asses, skillDays, false, false));

			skillDayA.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(14);
			skillDayB1.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			skillDayB2.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotMoveGuysSingleSkilledOnPrimarySkill()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skillA1 = new Skill("skillA1").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var skillDayA1 = skillA1.CreateSkillDayWithDemand(scenario, dateOnly, 0.5);
			var skillA2 = new Skill("skillA2").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var skillDayA2 = skillA2.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var skillB = new Skill("skillB").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillDayB = skillB.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var asses = new List<IPersonAssignment>();
			// These shouldn't be moved//
			var agentA1 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA1);
			asses.Add(new PersonAssignment(agentA1, scenario, dateOnly).WithLayer(activity, new TimePeriod(8, 9)));
			var agentA2 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA2);
			asses.Add(new PersonAssignment(agentA2, scenario, dateOnly).WithLayer(activity, new TimePeriod(8, 9)));
			//                         //
			var multiskilledAgent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA1, skillA2, skillB);
			asses.Add(new PersonAssignment(multiskilledAgent, scenario, dateOnly).WithLayer(activity, new TimePeriod(8, 9)));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, asses, new[] { skillDayA1, skillDayA2, skillDayB }, false, false));

			skillDayA1.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0.5);
			skillDayA2.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			skillDayB.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotGiveNegativeResources_Bug_40021()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skillA1 = new Skill("skillA1").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var skillDayA1 = skillA1.CreateSkillDayWithDemand(scenario, dateOnly, 0.7300000000000002); //gave wrong result
			var skillA2 = new Skill("skillA2").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var skillDayA2 = skillA2.CreateSkillDayWithDemand(scenario, dateOnly, 0);
			var skillB = new Skill("skillB").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillDayB = skillB.CreateSkillDayWithDemand(scenario, dateOnly, 0.27000000000000007); //gave wrong result
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA1, skillA2, skillB);
			var ass = new PersonAssignment(agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(8, 9));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass }, new[] { skillDayA1, skillDayA2, skillDayB }, false, false));

			if (skillDayA2.SkillStaffPeriodCollection.First().CalculatedResource < 0)
			{
				Assert.Fail("Shoveling resources should never give negative calculated resources");
			}
		}

		[Test]
		public void ShouldHandleMultipleSkillSetsContainingSamePrimarySkill()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skillA1 = new Skill("A1").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var skillDayA1 = skillA1.CreateSkillDayWithDemand(scenario, dateOnly, 0);
			var skillA2 = new Skill("A2").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var skillDayA2 = skillA2.CreateSkillDayWithDemand(scenario, dateOnly, 0);
			var skillB1 = new Skill("B1").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillDayB1 = skillB1.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var skillB2 = new Skill("B2").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillDayB2 = skillB2.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA1, skillA2, skillB1);
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));
			var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA1, skillA2, skillB2);
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass1, ass2 }, new[] { skillDayA1, skillDayA2, skillDayB1, skillDayB2 }, false, false));

			skillDayA1.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			skillDayA2.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			skillDayB1.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			skillDayB2.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldHandleMultipleSkillSetsContainingSamePrimarySkill_HighUnderstaffing()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skillA1 = new Skill("A1").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var skillDayA1 = skillA1.CreateSkillDayWithDemand(scenario, dateOnly, 0);
			var skillA2 = new Skill("A2").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var skillDayA2 = skillA2.CreateSkillDayWithDemand(scenario, dateOnly, 0);
			var skillB1 = new Skill("B1").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillDayB1 = skillB1.CreateSkillDayWithDemand(scenario, dateOnly, 10);
			var skillB2 = new Skill("B2").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillDayB2 = skillB2.CreateSkillDayWithDemand(scenario, dateOnly, 10);
			var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA1, skillA2, skillB1);
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));
			var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA1, skillA2, skillB2);
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass1, ass2 }, new[] { skillDayA1, skillDayA2, skillDayB1, skillDayB2 }, false, false));

			skillDayA1.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			skillDayA2.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			skillDayB1.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(-9);
			skillDayB2.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(-9);
		}

		[Test]
		public void ShouldHandleMultipleSkillSetsContainingSamePrimarySkill_MultipleAgents()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skillA1 = new Skill("A1").DefaultResolution(60).For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var skillDayA1 = skillA1.CreateSkillDayWithDemand(scenario, dateOnly, 1); //one agent should be put here
			var skillA2 = new Skill("A2").DefaultResolution(60).For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var skillDayA2 = skillA2.CreateSkillDayWithDemand(scenario, dateOnly, 1); //one agent should be put here
			var skillB1 = new Skill("B1").DefaultResolution(60).For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillDayB1 = skillB1.CreateSkillDayWithDemand(scenario, dateOnly, 2); //one agent should be put here
			var skillB2 = new Skill("B2").DefaultResolution(60).For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillDayB2 = skillB2.CreateSkillDayWithDemand(scenario, dateOnly, 2); //one agent should be put here
			var assignments = new List<IPersonAssignment>();
			for (var i = 0; i < 2; i++)
			{
				var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA1, skillA2, skillB1);
				var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA1, skillA2, skillB2);
				assignments.Add(new PersonAssignment(agent1, scenario, dateOnly).WithLayer(activity, new TimePeriod(8, 9)));
				assignments.Add(new PersonAssignment(agent2, scenario, dateOnly).WithLayer(activity, new TimePeriod(8, 9)));
			}

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, assignments, new[] { skillDayA1, skillDayA2, skillDayB1, skillDayB2 }, false, false));

			skillDayA1.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			skillDayA2.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			skillDayB1.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(-1);
			skillDayB2.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldValueSkillSetsWithMoreSubSkillsHigherWhenCascadingIndexesAreEqualOnOtherSkillSet()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skillGold = new Skill("gold").For(activity).InTimeZone(TimeZoneInfo.Utc).DefaultResolution(30).CascadingIndex(1).WithId().IsOpen(new TimePeriod(10, 0, 10, 30));
			var skillBronze0 = new Skill("silver").For(activity).InTimeZone(TimeZoneInfo.Utc).DefaultResolution(30).CascadingIndex(2).WithId().IsOpen(new TimePeriod(10, 0, 10, 30));
			var skillBronz1 = new Skill("bronze").For(activity).InTimeZone(TimeZoneInfo.Utc).DefaultResolution(30).CascadingIndex(2).WithId().IsOpen(new TimePeriod(10, 0, 10, 30));
			var skillBronze2 = new Skill("bronze1").For(activity).InTimeZone(TimeZoneInfo.Utc).DefaultResolution(30).CascadingIndex(2).WithId().IsOpen(new TimePeriod(10, 0, 10, 30));
			var skillDayGold = skillGold.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var skillDayBronze0 = skillBronze0.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var skillDayBronze1 = skillBronz1.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var skillDayBronze2 = skillBronze2.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agentGoldBronze0 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillGold, skillBronze0);
			var assGoldBronze0 = new PersonAssignment(agentGoldBronze0, scenario, dateOnly).WithLayer(activity, new TimePeriod(10, 0, 10, 30));
			var agentGoldBronze1Bronze2 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillGold, skillBronz1, skillBronze2);
			var assGoldBronze1Bronze2 = new PersonAssignment(agentGoldBronze1Bronze2, scenario, dateOnly).WithLayer(activity, new TimePeriod(10, 0, 10, 30));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new [] {assGoldBronze0, assGoldBronze1Bronze2}, new[] { skillDayGold, skillDayBronze0, skillDayBronze1, skillDayBronze2 }, false, false));

			skillDayBronze1.SkillStaffPeriodCollection.First().AbsoluteDifference.Should().Be.EqualTo(-0.5);
			skillDayBronze2.SkillStaffPeriodCollection.First().AbsoluteDifference.Should().Be.EqualTo(-0.5);
		}

		[Test]
		public void ShouldValueSkillSetsWithLowerSubSkillIndexesHigher()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skillGold = new Skill("gold").For(activity).InTimeZone(TimeZoneInfo.Utc).DefaultResolution(30).CascadingIndex(1).WithId().IsOpen(new TimePeriod(10, 0, 10, 30));
			var skillSilver = new Skill("silver").For(activity).InTimeZone(TimeZoneInfo.Utc).DefaultResolution(30).CascadingIndex(2).WithId().IsOpen(new TimePeriod(10, 0, 10, 30));
			var skillSilver1 = new Skill("silver1").For(activity).InTimeZone(TimeZoneInfo.Utc).DefaultResolution(30).CascadingIndex(2).WithId().IsOpen(new TimePeriod(10, 0, 10, 30));
			var skillBronze = new Skill("bronze").For(activity).InTimeZone(TimeZoneInfo.Utc).DefaultResolution(30).CascadingIndex(3).WithId().IsOpen(new TimePeriod(10, 0, 10, 30));
			var skillDayGold = skillGold.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var skillDaySilver = skillSilver.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var skillDaySilver1 = skillSilver1.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var skillDayBronze = skillBronze.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var asses = new List<IPersonAssignment>();
			var agentGoldSilverSilver1 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillGold, skillSilver, skillSilver1);
			asses.Add(new PersonAssignment(agentGoldSilverSilver1, scenario, dateOnly).WithLayer(activity, new TimePeriod(10, 0, 10, 30)));
			var agentGoldSilverBronze = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillGold, skillSilver, skillBronze);
			asses.Add(new PersonAssignment(agentGoldSilverBronze, scenario, dateOnly).WithLayer(activity, new TimePeriod(10, 0, 10, 30)));
			var singleSkillAgent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillSilver);
			var singleSkillAss = new PersonAssignment(singleSkillAgent, scenario, dateOnly);
			singleSkillAss.AddActivity(activity, new TimePeriod(10, 0, 10, 30));
			asses.Add(singleSkillAss);

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, asses, new[] { skillDayGold, skillDaySilver, skillDaySilver1, skillDayBronze}, false, false));

			skillDaySilver1.SkillStaffPeriodCollection.First().AbsoluteDifference.Should().Be.EqualTo(0);
		}

		[Test]
		public void IsolatedSkillSetsShouldNotAffectEachOther()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skillA1 = new Skill("A1").DefaultResolution(60).For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var skillDayA1 = skillA1.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var skillB1 = new Skill("B1").DefaultResolution(60).For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillDayB1 = skillB1.CreateSkillDayWithDemand(scenario, dateOnly, 100);
			var skillA2 = new Skill("A2").DefaultResolution(60).For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var skillDayA2 = skillA2.CreateSkillDayWithDemand(scenario, dateOnly, 0.5);
			var skillB2 = new Skill("B2").DefaultResolution(60).For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillDayB2 = skillB2.CreateSkillDayWithDemand(scenario, dateOnly, 0.5);
			var assignments = new List<IPersonAssignment>();
			for (var i = 0; i < 101; i++)
			{
				var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA1, skillB1);
				assignments.Add(new PersonAssignment(agent1, scenario, dateOnly).WithLayer(activity, new TimePeriod(8, 9)));
			}
			var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA2, skillB2);
			assignments.Add(new PersonAssignment(agent2, scenario, dateOnly).WithLayer(activity, new TimePeriod(8, 9)));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, assignments, new[] { skillDayA1, skillDayA2, skillDayB1, skillDayB2 }, false, false));

			skillDayA1.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			skillDayA2.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			skillDayB1.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			skillDayB2.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
		}
	}
}