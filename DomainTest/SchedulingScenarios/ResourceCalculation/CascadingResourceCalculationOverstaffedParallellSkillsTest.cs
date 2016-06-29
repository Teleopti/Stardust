using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Cascading;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.ResourceCalculation
{
	[DomainTest]
	[Toggle(Toggles.ResourcePlanner_CascadingSkills_38524)]
	[TestFixture(typeof(ShovelResourcesPercentageDistribution))]
	[TestFixture(typeof(ShovelResourcesFocusHighUnderstaffingPercentage))]
	public class CascadingResourceCalculationOverstaffedParallellSkillsTest : ISetup
	{
		private readonly Type _implTypeToTest;
		public IResourceOptimizationHelper Target;

		public CascadingResourceCalculationOverstaffedParallellSkillsTest(Type implTypeToTest)
		{
			_implTypeToTest = implTypeToTest;
		}

		[Test]
		public void ShouldMoveResourceToTwoSkillsWithSameDemand()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skillA = new Skill("A", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillA.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillA, new TimePeriod(8, 0, 9, 0));
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 0.5);
			var skillB1 = new Skill("B1", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillB1.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB1, new TimePeriod(8, 0, 9, 0));
			var skillDayB1 = skillB1.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var skillB2 = new Skill("B2", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillB2.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB2, new TimePeriod(8, 0, 9, 0));
			var skillDayB2 = skillB2.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { skillA, skillB1, skillB2 });
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(activity, new TimePeriod(5, 0, 10, 0));

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
			var skillA = new Skill("A", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillA.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillA, new TimePeriod(8, 0, 9, 0));
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 0.5);
			var skillB1 = new Skill("B1", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillB1.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB1, new TimePeriod(8, 0, 9, 0));
			var skillDayB1 = skillB1.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var skillB2 = new Skill("B2", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillB2.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB2, new TimePeriod(8, 0, 9, 0));
			var skillDayB2 = skillB2.CreateSkillDayWithDemand(scenario, dateOnly, 0);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { skillA, skillB1, skillB2 });
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(activity, new TimePeriod(5, 0, 10, 0));

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
			var skillA = new Skill("A", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillA.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillA, new TimePeriod(8, 0, 9, 0));
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 0);
			var skillB1 = new Skill("B1", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillB1.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB1, new TimePeriod(8, 0, 9, 0));
			var skillDayB1 = skillB1.CreateSkillDayWithDemand(scenario, dateOnly, 10);
			var skillB2 = new Skill("B2Overstaffed", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillB2.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB2, new TimePeriod(8, 0, 9, 0));
			var skillDayB2 = skillB2.CreateSkillDayWithDemand(scenario, dateOnly, 0.5);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { skillA, skillB1, skillB2 });
			var agentB2 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agentB2.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { skillB2 });
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(activity, new TimePeriod(5, 0, 10, 0));
			var assB2 = new PersonAssignment(agentB2, scenario, dateOnly);
			assB2.AddActivity(activity, new TimePeriod(5, 0, 10, 0));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass, assB2 }, new[] { skillDayA, skillDayB1, skillDayB2 }, false, false));

			skillDayA.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			skillDayB1.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(-9);
			skillDayB2.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0.5);
		}

		[Test, Ignore("To be fixed")]
		public void ShouldMoveResourcesIfOnePrimarySkillIsOverstaffed()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skillA1 = new Skill("skillA1", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillA1.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillA1, new TimePeriod(8, 0, 9, 0));
			var skillDayA1 = skillA1.CreateSkillDayWithDemand(scenario, dateOnly, 0); //-> overstaffed with one fixed agent and a small rest from multi skill guy
			var skillA2 = new Skill("skillA2", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillA2.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillA2, new TimePeriod(8, 0, 9, 0));
			var skillDayA2 = skillA2.CreateSkillDayWithDemand(scenario, dateOnly, 3); //makes total primary skills understaffed
			var skillB = new Skill("skillB", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillB.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB, new TimePeriod(8, 0, 9, 0));
			var skillDayB = skillB.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var multiSkilledAgent = new Person().InTimeZone(TimeZoneInfo.Utc);
			multiSkilledAgent.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { skillA1, skillA2, skillB });
			var multiSkilledAss = new PersonAssignment(multiSkilledAgent, scenario, dateOnly);
			multiSkilledAss.AddActivity(activity, new TimePeriod(8, 0, 9, 0));
			var agentA1 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agentA1.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { skillA1 });
			var a1Ass = new PersonAssignment(agentA1, scenario, dateOnly);
			a1Ass.AddActivity(activity, new TimePeriod(8, 0, 9, 0));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { multiSkilledAss, a1Ass }, new[] { skillDayA1, skillDayA2, skillDayB }, false, false));

			skillDayA1.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(1);
			skillDayB.SkillStaffPeriodCollection.First().AbsoluteDifference
					.Should().Be.GreaterThan(-1); 
		}

		[Test]
		public void ShouldNotMoveResourcesFromUnderstaffedPrimarySkill()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skillA1 = new Skill("skillA1", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillA1.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillA1, new TimePeriod(8, 0, 9, 0));
			var skillDayA1 = skillA1.CreateSkillDayWithDemand(scenario, dateOnly, 0); //-> lite mer än 1 överbemannat
			var skillA2 = new Skill("skillA2", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillA2.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillA2, new TimePeriod(8, 0, 9, 0));
			var skillDayA2 = skillA2.CreateSkillDayWithDemand(scenario, dateOnly, 2); //-> lite mer än 1 underbemannat
			var skillB = new Skill("skillB", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillB.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB, new TimePeriod(8, 0, 9, 0));
			var skillDayB = skillB.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var multiSkilledAgent = new Person().InTimeZone(TimeZoneInfo.Utc);
			multiSkilledAgent.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { skillA1, skillA2, skillB });
			var multiSkilledAss = new PersonAssignment(multiSkilledAgent, scenario, dateOnly);
			multiSkilledAss.AddActivity(activity, new TimePeriod(8, 0, 9, 0));
			var agentA1 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agentA1.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { skillA1 });
			var a1Ass = new PersonAssignment(agentA1, scenario, dateOnly);
			a1Ass.AddActivity(activity, new TimePeriod(8, 0, 9, 0));

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
			var skillA1 = new Skill("skillA1", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillA1.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillA1, new TimePeriod(8, 0, 9, 0));
			var skillDayA1 = skillA1.CreateSkillDayWithDemand(scenario, dateOnly, 0.2);
			var skillA2 = new Skill("skillA2", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillA2.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillA2, new TimePeriod(8, 0, 9, 0));
			var skillDayA2 = skillA2.CreateSkillDayWithDemand(scenario, dateOnly, 0.3);
			var skillB = new Skill("skillB", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillB.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB, new TimePeriod(8, 0, 9, 0));
			var skillDayB = skillB.CreateSkillDayWithDemand(scenario, dateOnly, 0.3);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { skillA1, skillA2, skillB });
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(activity, new TimePeriod(8, 0, 9, 0));

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
			var skillA1 = new Skill("skillA1", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillA1.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillA1, new TimePeriod(7, 0, 9, 0));
			var skillDayA1 = skillA1.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var skillA2 = new Skill("skillA2", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillA2.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillA2, new TimePeriod(8, 0, 9, 0));
			var skillDayA2 = skillA2.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var skillB = new Skill("skillB", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillB.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB, new TimePeriod(7, 0, 9, 0));
			var skillDayB = skillB.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { skillA1, skillA2, skillB });
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(activity, new TimePeriod(7, 0, 8, 0));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass }, new[] { skillDayA1, skillDayA2, skillDayB }, false, false));

			skillDayA1.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			skillDayB.SkillStaffPeriodCollection.First().AbsoluteDifference
				  .Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldGetResourcesFromSkillGroupWhenAllPrimarySkillsAreClosed()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skillA1 = new Skill("skillA1", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillA1.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillA1, new TimePeriod(8, 0, 9, 0));
			var skillDayA1 = skillA1.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var skillA2 = new Skill("skillA2", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillA2.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillA2, new TimePeriod(8, 0, 9, 0));
			var skillDayA2 = skillA2.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var skillB = new Skill("skillB", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillB.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB, new TimePeriod(7, 0, 9, 0));
			var skillDayB = skillB.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { skillA1, skillA2, skillB });
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(activity, new TimePeriod(7, 0, 8, 0));

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
			var skillA = new Skill("A", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillA.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillA, new TimePeriod(8, 0, 9, 0));
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 0);
			var skillB1 = new Skill("B1", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillB1.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB1, new TimePeriod(8, 0, 9, 0));
			var skillDayB1 = skillB1.CreateSkillDayWithDemand(scenario, dateOnly, 10);
			var skillB2 = new Skill("B2", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillB2.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB2, new TimePeriod(8, 0, 9, 0));
			var skillDayB2 = skillB2.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var asses = new List<IPersonAssignment>();
			for (var i = 0; i < 5; i++)
			{
				var agent = new Person().InTimeZone(TimeZoneInfo.Utc);
				agent.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), 
					new[] { skillA, skillB1, skillB2 });
				var ass = new PersonAssignment(agent, scenario, dateOnly);
				ass.AddActivity(activity, new TimePeriod(8, 0, 9, 0));
				asses.Add(ass);

				var agentB1 = new Person().InTimeZone(TimeZoneInfo.Utc);
				agentB1.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }),
					new[] { skillB1 });
				var assB1 = new PersonAssignment(agentB1, scenario, dateOnly);
				assB1.AddActivity(activity, new TimePeriod(8, 0, 9, 0));
				asses.Add(assB1);
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
			var skillA = new Skill("A", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillA.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillA, new TimePeriod(8, 0, 9, 0));
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 0);
			var skillB1 = new Skill("B1", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillB1.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB1, new TimePeriod(8, 0, 9, 0));
			var skillDayB1 = skillB1.CreateSkillDayWithDemand(scenario, dateOnly, 10);
			var skillB2 = new Skill("B2", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillB2.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB2, new TimePeriod(8, 0, 9, 0));
			var skillDayB2 = skillB2.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var asses = new List<IPersonAssignment>();
			for (var i = 0; i < 20; i++)
			{
				var agent = new Person().InTimeZone(TimeZoneInfo.Utc);
				agent.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }),
					new[] { skillA, skillB1, skillB2 });
				var ass = new PersonAssignment(agent, scenario, dateOnly);
				ass.AddActivity(activity, new TimePeriod(8, 0, 9, 0));
				asses.Add(ass);	
			}

			for (var i = 0; i < 5; i++)
			{
				var agentB1 = new Person().InTimeZone(TimeZoneInfo.Utc);
				agentB1.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }),
					new[] { skillB1 });
				var assB1 = new PersonAssignment(agentB1, scenario, dateOnly);
				assB1.AddActivity(activity, new TimePeriod(8, 0, 9, 0));
				asses.Add(assB1);
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

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDoubleForType(_implTypeToTest).For<IShovelResourcesPerActivityIntervalSkillGroup>();
		}
	}
}