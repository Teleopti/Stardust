using System;
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
	public class CascadingResourceCalculationOverstaffedTest : ISetup
	{
		private readonly Type _implTypeToTest;
		public IResourceOptimizationHelper Target;
		public FakeTimeZoneGuard TimeZoneGuard;

		public CascadingResourceCalculationOverstaffedTest(Type implTypeToTest)
		{
			_implTypeToTest = implTypeToTest;
		}

		[Test]
		public void ShouldMoveResourceToSecondarySkill()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var prioritizedSkill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			prioritizedSkill.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(prioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var prioritizedSkillDay = prioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var nonPrioritizedSkill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			nonPrioritizedSkill.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(nonPrioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var nonPrioritizedSkillDay = nonPrioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent1.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { prioritizedSkill, nonPrioritizedSkill });
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly);
			ass1.AddActivity(activity, new TimePeriod(5, 0, 10, 0));
			var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent2.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { prioritizedSkill, nonPrioritizedSkill });
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly);
			ass2.AddActivity(activity, new TimePeriod(5, 0, 10, 0));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass1, ass2 }, new[] { prioritizedSkillDay, nonPrioritizedSkillDay }, false, false));

			prioritizedSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			nonPrioritizedSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldMoveResourceToSecondarySkillWhenThirdSkillExists()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var prioritizedSkill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			prioritizedSkill.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(prioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var prioritizedSkillDay = prioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var secondarySkill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			secondarySkill.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(secondarySkill, new TimePeriod(8, 0, 9, 0));
			var secondarySkillDay = secondarySkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var lastSkill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			lastSkill.SetCascadingIndex(3);
			WorkloadFactory.CreateWorkloadWithOpenHours(lastSkill, new TimePeriod(8, 0, 9, 0));
			var lastSkillDay = lastSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent1.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { prioritizedSkill, secondarySkill });
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly);
			ass1.AddActivity(activity, new TimePeriod(5, 0, 10, 0));
			var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent2.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { prioritizedSkill, secondarySkill });
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly);
			ass2.AddActivity(activity, new TimePeriod(5, 0, 10, 0));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass1, ass2 }, new[] { prioritizedSkillDay, secondarySkillDay, lastSkillDay }, false, false));

			prioritizedSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			secondarySkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			lastSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldNotMoveResourceWhenDifferentActivities()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("activity").WithId();
			var differentActivity = new Activity("differentActivity").WithId();
			var dateOnly = DateOnly.Today;
			var prioritizedSkill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			prioritizedSkill.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(prioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var prioritizedSkillDay = prioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var nonPrioritizedSkill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = differentActivity, TimeZone = TimeZoneInfo.Utc }.WithId();
			nonPrioritizedSkill.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(nonPrioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var nonPrioritizedSkillDay = nonPrioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent1.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { prioritizedSkill, nonPrioritizedSkill });
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly);
			ass1.AddActivity(activity, new TimePeriod(5, 0, 10, 0));
			var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent2.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { prioritizedSkill, nonPrioritizedSkill });
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly);
			ass2.AddActivity(activity, new TimePeriod(5, 0, 10, 0));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass1, ass2 }, new[] { prioritizedSkillDay, nonPrioritizedSkillDay }, false, false));

			prioritizedSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(1);
			nonPrioritizedSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldNotMoveResourceWhenSkillIsClosedPartOfDay()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var prioritizedSkill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			prioritizedSkill.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(prioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var prioritizedSkillDay = prioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var nonPrioritizedSkill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			nonPrioritizedSkill.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(nonPrioritizedSkill, new TimePeriod(12, 0, 13, 0));
			var nonPrioritizedSkillDay = nonPrioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent1.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { prioritizedSkill, nonPrioritizedSkill });
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly);
			ass1.AddActivity(activity, new TimePeriod(5, 0, 10, 0));
			var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent2.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { prioritizedSkill, nonPrioritizedSkill });
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly);
			ass2.AddActivity(activity, new TimePeriod(5, 0, 10, 0));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass1, ass2 }, new[] { prioritizedSkillDay, nonPrioritizedSkillDay }, false, false));

			prioritizedSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(1);
			nonPrioritizedSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldNotMoveResourceWhenSkillIsClosed()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var prioritizedSkill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			prioritizedSkill.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(prioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var prioritizedSkillDay = prioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var nonPrioritizedSkill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			nonPrioritizedSkill.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadThatIsClosed(nonPrioritizedSkill);
			var nonPrioritizedSkillDay = nonPrioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent1.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { prioritizedSkill, nonPrioritizedSkill });
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly);
			ass1.AddActivity(activity, new TimePeriod(5, 0, 10, 0));
			var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent2.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { prioritizedSkill, nonPrioritizedSkill });
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly);
			ass2.AddActivity(activity, new TimePeriod(5, 0, 10, 0));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass1, ass2 }, new[] { prioritizedSkillDay, nonPrioritizedSkillDay }, false, false));

			prioritizedSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotMoveResourceWhenNonPrioritizedSkillIsMaxSeat()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var prioritizedSkill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			prioritizedSkill.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(prioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var prioritizedSkillDay = prioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var nonPrioritizedSkill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.MaxSeatSkill)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			nonPrioritizedSkill.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(nonPrioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var nonPrioritizedSkillDay = nonPrioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent1.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { prioritizedSkill, nonPrioritizedSkill });
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly);
			ass1.AddActivity(activity, new TimePeriod(5, 0, 10, 0));
			var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent2.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { prioritizedSkill, nonPrioritizedSkill });
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly);
			ass2.AddActivity(activity, new TimePeriod(5, 0, 10, 0));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass1, ass2 }, new[] { prioritizedSkillDay, nonPrioritizedSkillDay }, false, false));

			prioritizedSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(1);
			nonPrioritizedSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldNotMoveResourcesIfLastSkillInChain()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var prioritizedSkill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			prioritizedSkill.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(prioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var prioritizedSkillDay = prioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent1.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { prioritizedSkill });
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly);
			ass1.AddActivity(activity, new TimePeriod(5, 0, 10, 0));
			var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent2.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { prioritizedSkill });
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly);
			ass2.AddActivity(activity, new TimePeriod(5, 0, 10, 0));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass1, ass2 }, new[] { prioritizedSkillDay }, false, false));

			prioritizedSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldMoveResourcesOnlyToUnderstaffedSkill()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var prioritizedSkill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			prioritizedSkill.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(prioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var prioritizedSkillDay = prioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var secondarySkill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			secondarySkill.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(secondarySkill, new TimePeriod(8, 0, 9, 0));
			var secondarySkillDay = secondarySkill.CreateSkillDayWithDemand(scenario, dateOnly, 0);
			var lastSkill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			lastSkill.SetCascadingIndex(3);
			WorkloadFactory.CreateWorkloadWithOpenHours(lastSkill, new TimePeriod(8, 0, 9, 0));
			var lastSkillDay = lastSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent1.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { prioritizedSkill, secondarySkill, lastSkill });
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly);
			ass1.AddActivity(activity, new TimePeriod(5, 0, 10, 0));
			var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent2.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { prioritizedSkill, secondarySkill, lastSkill });
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly);
			ass2.AddActivity(activity, new TimePeriod(5, 0, 10, 0));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass1, ass2 }, new[] { prioritizedSkillDay, secondarySkillDay, lastSkillDay }, false, false));

			prioritizedSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			secondarySkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			lastSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldMoveResourcesToMoreThanOneSkill()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var prioritizedSkill = new Skill("A", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			prioritizedSkill.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(prioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var prioritizedSkillDay = prioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var secondarySkill = new Skill("B", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			secondarySkill.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(secondarySkill, new TimePeriod(8, 0, 9, 0));
			var secondarySkillDay = secondarySkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var lastSkill = new Skill("C", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			lastSkill.SetCascadingIndex(3);
			WorkloadFactory.CreateWorkloadWithOpenHours(lastSkill, new TimePeriod(8, 0, 9, 0));
			var lastSkillDay = lastSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent1.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { prioritizedSkill, secondarySkill, lastSkill });
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly);
			ass1.AddActivity(activity, new TimePeriod(5, 0, 10, 0));
			var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent2.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { prioritizedSkill, secondarySkill, lastSkill });
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly);
			ass2.AddActivity(activity, new TimePeriod(5, 0, 10, 0));
			var agent3 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent3.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { prioritizedSkill, secondarySkill, lastSkill });
			var ass3 = new PersonAssignment(agent3, scenario, dateOnly);
			ass3.AddActivity(activity, new TimePeriod(5, 0, 10, 0));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass1, ass2, ass3 }, new[] { prioritizedSkillDay, secondarySkillDay, lastSkillDay }, false, false));

			prioritizedSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			secondarySkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			lastSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
		}


		[Test]
		public void ShouldMoveResourcesToMoreThanOneSkill_DifferentSkillOrder()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var prioritizedSkill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			prioritizedSkill.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(prioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var prioritizedSkillDay = prioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var secondarySkill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			secondarySkill.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(secondarySkill, new TimePeriod(8, 0, 9, 0));
			var secondarySkillDay = secondarySkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var lastSkill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			lastSkill.SetCascadingIndex(3);
			WorkloadFactory.CreateWorkloadWithOpenHours(lastSkill, new TimePeriod(8, 0, 9, 0));
			var lastSkillDay = lastSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent1.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { prioritizedSkill, secondarySkill, lastSkill });
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly);
			ass1.AddActivity(activity, new TimePeriod(5, 0, 10, 0));
			var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent2.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { prioritizedSkill, secondarySkill, lastSkill });
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly);
			ass2.AddActivity(activity, new TimePeriod(5, 0, 10, 0));
			var agent3 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent3.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { prioritizedSkill, secondarySkill, lastSkill });
			var ass3 = new PersonAssignment(agent3, scenario, dateOnly);
			ass3.AddActivity(activity, new TimePeriod(5, 0, 10, 0));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass1, ass2, ass3 }, new[] { secondarySkillDay, lastSkillDay, prioritizedSkillDay }, false, false));

			prioritizedSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			secondarySkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			lastSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
		}


		[Test]
		public void ShouldNotMakeLowPrioSkillOverstaffed()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skillA = new Skill("A", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillA.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillA, new TimePeriod(8, 0, 9, 0));
			var skillADay = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var skillB = new Skill("B", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillB.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB, new TimePeriod(8, 0, 9, 0));
			var skillBDay = skillB.CreateSkillDayWithDemand(scenario, dateOnly, 0.5);
			var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent1.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }),
				new[] { skillA, skillB });
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly);
			ass1.AddActivity(activity, new TimePeriod(5, 0, 10, 0));
			var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent2.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }),
				new[] { skillA, skillB });
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly);
			ass2.AddActivity(activity, new TimePeriod(5, 0, 10, 0));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass1, ass2 }, new[] { skillADay, skillBDay }, false, false));

			skillADay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0.5);
			skillBDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldMoveResourcesForPeriodConsiderEndUserTimeZone()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var scheduledAndSkillTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			var endUserTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time"); 
			var prioritizedSkill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = scheduledAndSkillTimeZone }.WithId();
			prioritizedSkill.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithFullOpenHours(prioritizedSkill);
			var prioritizedSkillDay = prioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 0.5);
			var nonPrioritizedSkill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = scheduledAndSkillTimeZone }.WithId();
			nonPrioritizedSkill.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithFullOpenHours(nonPrioritizedSkill);
			var nonPrioritizedSkillDay = nonPrioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent1 = new Person().InTimeZone(scheduledAndSkillTimeZone);
			agent1.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { prioritizedSkill, nonPrioritizedSkill });
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly);
			ass1.AddActivity(activity, new TimePeriod(0, 0, 24, 0));
			var agent2 = new Person().InTimeZone(scheduledAndSkillTimeZone);
			agent2.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { prioritizedSkill, nonPrioritizedSkill });
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly);
			ass2.AddActivity(activity, new TimePeriod(0, 0, 24, 0));
			TimeZoneGuard.SetTimeZone(endUserTimeZone);

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass1, ass2 }, new[] { prioritizedSkillDay, nonPrioritizedSkillDay }, false, false));

			var endUserPeriod = dateOnly.ToDateTimePeriod(endUserTimeZone);
			foreach (var skillStaffPeriod in prioritizedSkillDay.SkillStaffPeriodCollection)
			{
				skillStaffPeriod.AbsoluteDifference
					.Should().Be.EqualTo(endUserPeriod.Contains(skillStaffPeriod.Period) ? 0.5 : -0.5); //1.5 -> primärskillresursberäkning har körts men inte shovling
			}
		}

		[Test]
		public void ShouldOnlyMoveResourcesWithinSkillgroupWhereScheduledResourcesBelongToCorrectActivity()
		{
			var scenario = new Scenario("_");
			var activity1 = new Activity("1").WithId();
			var activity2 = new Activity("2").WithId();
			var dateOnly = DateOnly.Today;
			var skillA = new Skill("A", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity1, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillA.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillA, new TimePeriod(8, 0, 9, 0));
			var skillADay = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 0.5);
			var skillB = new Skill("B", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity1, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillB.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB, new TimePeriod(8, 0, 9, 0));
			var skillBDay = skillB.CreateSkillDayWithDemand(scenario, dateOnly, 1.5);
			var skillC = new Skill("C", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity2, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillC.SetCascadingIndex(3);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillC, new TimePeriod(8, 0, 9, 0));
			var skillCDay = skillC.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent1.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }),
				new[] { skillA, skillB, skillC });
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly);
			ass1.AddActivity(activity1, new TimePeriod(5, 0, 10, 0));
			var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent2.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }),
				new[] { skillA, skillB, skillC });
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly);
			ass2.AddActivity(activity2, new TimePeriod(5, 0, 10, 0));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass1, ass2 }, new[] { skillADay, skillBDay, skillCDay }, false, false));

			skillADay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			skillBDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(-1);
			skillCDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDoubleForType(_implTypeToTest).For<IShovelResourcesPerActivityIntervalSkillGroup>();
		}
	}
}