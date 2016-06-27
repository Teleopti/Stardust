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
	public class CascadingResourceCalculationOverstaffedSkillGroupsTest
	{
		private readonly Type _implTypeToTest;
		public CascadingResourceCalculation Target;

		public CascadingResourceCalculationOverstaffedSkillGroupsTest(Type implTypeToTest)
		{
			_implTypeToTest = implTypeToTest;
		}

		[Test]
		public void ShouldMoveResourceOnlyWithinSkillGroup()
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
			var skillBDay = skillB.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var skillC = new Skill("C", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillC.SetCascadingIndex(3);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillC, new TimePeriod(8, 0, 9, 0));
			var skillCDay = skillC.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent1.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), 
				new[] { skillA, skillC });
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly);
			ass1.AddActivity(activity, new TimePeriod(5, 0, 10, 0));
			var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent2.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), 
				new[] { skillA, skillC });
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly);
			ass2.AddActivity(activity, new TimePeriod(5, 0, 10, 0));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass1, ass2 }, new[] { skillADay, skillBDay, skillCDay }, false, false));

			skillADay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			skillBDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(-1);
			skillCDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldMoveResourceFromLowestSkillGroupFirst()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skillA = new Skill("A", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillA.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillA, new TimePeriod(8, 0, 9, 0));
			var skillADay = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 0.5);
			var skillB = new Skill("B", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillB.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB, new TimePeriod(8, 0, 9, 0));
			var skillBDay = skillB.CreateSkillDayWithDemand(scenario, dateOnly, 0.5);
			var skillC = new Skill("C", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillC.SetCascadingIndex(3);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillC, new TimePeriod(8, 0, 9, 0));
			var skillCDay = skillC.CreateSkillDayWithDemand(scenario, dateOnly, 0.5);
			var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent1.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }),
				new[] { skillA, skillC });
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly);
			ass1.AddActivity(activity, new TimePeriod(5, 0, 10, 0));
			var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent2.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }),
				new[] { skillB, skillC });
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly);
			ass2.AddActivity(activity, new TimePeriod(5, 0, 10, 0));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass1, ass2 }, new[] { skillADay, skillBDay, skillCDay }, false, false));

			skillADay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0.5);
			skillBDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			skillCDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldMoveResourcesFromLowestSkillGroup_DifferentLowestSkill()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skillA = new Skill("A", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillA.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillA, new TimePeriod(8, 0, 9, 0));
			var skillADay = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 0.5);
			var skillB = new Skill("B", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillB.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB, new TimePeriod(8, 0, 9, 0));
			var skillBDay = skillB.CreateSkillDayWithDemand(scenario, dateOnly, 0.5);
			var skillC = new Skill("C", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillC.SetCascadingIndex(3);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillC, new TimePeriod(8, 0, 9, 0));
			var skillCDay = skillC.CreateSkillDayWithDemand(scenario, dateOnly, 0.5);
			var skillD = new Skill("D", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillD.SetCascadingIndex(4);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillD, new TimePeriod(8, 0, 9, 0));
			var skillDDay = skillD.CreateSkillDayWithDemand(scenario, dateOnly, 0);
			var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent1.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }),
				new[] { skillA, skillC, skillD });
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly);
			ass1.AddActivity(activity, new TimePeriod(5, 0, 10, 0));
			var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent2.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }),
				new[] { skillB, skillC });
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly);
			ass2.AddActivity(activity, new TimePeriod(5, 0, 10, 0));

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
		public void ShouldNotMoveMoreResourcesThanAvailableInSameSkillGroup()
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
			var skillBDay = skillB.CreateSkillDayWithDemand(scenario, dateOnly, 2);
			var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent1.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }),
				new[] { skillA, skillB });
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly);
			ass1.AddActivity(activity, new TimePeriod(5, 0, 10, 0));
			var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent2.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }),
				new[] { skillA });
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly);
			ass2.AddActivity(activity, new TimePeriod(5, 0, 10, 0));
			var agent3 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent3.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }),
				new[] { skillA });
			var ass3 = new PersonAssignment(agent3, scenario, dateOnly);
			ass3.AddActivity(activity, new TimePeriod(5, 0, 10, 0));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass1, ass2, ass3 }, new[] { skillADay, skillBDay }, false, false));

			skillADay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(1);
			skillBDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldNotMoveMoreResourcesThanAvailableInSameSkillGroup_PartOfResourceNeedsToBeMoved()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skillA = new Skill("A", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillA.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillA, new TimePeriod(8, 0, 9, 0));
			var skillADay = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 0.5);
			var skillB = new Skill("B", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillB.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB, new TimePeriod(8, 0, 9, 0));
			var skillBDay = skillB.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var skillC = new Skill("C", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillC.SetCascadingIndex(3);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillC, new TimePeriod(8, 0, 9, 0));
			var skillCDay = skillC.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent1.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }),
				new[] { skillA, skillB, skillC });
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly);
			ass1.AddActivity(activity, new TimePeriod(5, 0, 10, 0));
			var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent2.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }),
				new[] { skillA });
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly);
			ass2.AddActivity(activity, new TimePeriod(5, 0, 10, 0));

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
			var skillA = new Skill("A", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillA.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillA, new TimePeriod(8, 0, 9, 0));
			var skillADay = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var skillB = new Skill("B", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillB.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB, new TimePeriod(8, 0, 9, 0));
			var skillBDay = skillB.CreateSkillDayWithDemand(scenario, dateOnly, 0.5);
			var skillC = new Skill("C", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillC.SetCascadingIndex(3);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillC, new TimePeriod(8, 0, 9, 0));
			var skillCDay = skillC.CreateSkillDayWithDemand(scenario, dateOnly, 0.5);
			var skillD = new Skill("D", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillD.SetCascadingIndex(4);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillD, new TimePeriod(8, 0, 9, 0));
			var skillDDay = skillD.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent1.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }),
				new[] { skillA, skillB, skillC, skillD });
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly);
			ass1.AddActivity(activity, new TimePeriod(5, 0, 10, 0));
			var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent2.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }),
				new[] { skillA, skillB, skillC, skillD });
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly);
			ass2.AddActivity(activity, new TimePeriod(5, 0, 10, 0));
			var agent3 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent3.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }),
				new[] { skillC });
			var ass3 = new PersonAssignment(agent3, scenario, dateOnly);
			ass3.AddActivity(activity, new TimePeriod(5, 0, 10, 0));

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
		public void ShouldMoveResourcesToHighestFollowingSkillWhenSkillGroupsHavingSamePrimarySkill()
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
			var skillBDay = skillB.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var skillC = new Skill("C", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillC.SetCascadingIndex(3);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillC, new TimePeriod(8, 0, 9, 0));
			var skillCDay = skillC.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent1.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }),
				new[]{skillA, skillB});
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly);
			ass1.AddActivity(activity, new TimePeriod(5, 0, 10, 0));
			var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent2.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }),
				new[] { skillA, skillC});
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly);
			ass2.AddActivity(activity, new TimePeriod(5, 0, 10, 0));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass1, ass2 }, new[] { skillADay, skillBDay, skillCDay }, false, false));

			skillADay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			skillBDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			skillCDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(-1);
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDoubleForType(_implTypeToTest).For<IShovelResourcesPerActivityIntervalSkillGroup>();
		}
	}
}