using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Cascading;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Cascading
{
	[DomainTest]
	public class CascadingResourceCalculationOverstaffedTest
	{
		public CascadingResourceCalculation Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolder;

		[Test]
		public void ShouldMoveResourceToSecondarySkill()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var prioritizedSkill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			prioritizedSkill.SetCascadingIndex_UseFromTestOnly(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(prioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var prioritizedSkillDay = prioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var nonPrioritizedSkill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			nonPrioritizedSkill.SetCascadingIndex_UseFromTestOnly(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(nonPrioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var nonPrioritizedSkillDay = nonPrioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent1 = new Person();
			agent1.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			agent1.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { prioritizedSkill, nonPrioritizedSkill });
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly);
			ass1.AddActivity(activity, new TimePeriod(5, 0, 10, 0));
			var agent2 = new Person();
			agent2.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			agent2.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { prioritizedSkill, nonPrioritizedSkill });
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly);
			ass2.AddActivity(activity, new TimePeriod(5, 0, 10, 0));
			SchedulerStateHolder.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent1, agent2 }, new[] {  ass1, ass2 }, new[] { prioritizedSkillDay, nonPrioritizedSkillDay });

			Target.ForDay(dateOnly);

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
			prioritizedSkill.SetCascadingIndex_UseFromTestOnly(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(prioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var prioritizedSkillDay = prioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var secondarySkill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			secondarySkill.SetCascadingIndex_UseFromTestOnly(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(secondarySkill, new TimePeriod(8, 0, 9, 0));
			var secondarySkillDay = secondarySkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var lastSkill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			lastSkill.SetCascadingIndex_UseFromTestOnly(3);
			WorkloadFactory.CreateWorkloadWithOpenHours(lastSkill, new TimePeriod(8, 0, 9, 0));
			var lastSkillDay = lastSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent1 = new Person();
			agent1.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			agent1.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { prioritizedSkill, secondarySkill });
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly);
			ass1.AddActivity(activity, new TimePeriod(5, 0, 10, 0));
			var agent2 = new Person();
			agent2.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			agent2.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { prioritizedSkill, secondarySkill });
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly);
			ass2.AddActivity(activity, new TimePeriod(5, 0, 10, 0));
			SchedulerStateHolder.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent1, agent2 }, new[] { ass1, ass2 }, new[] { prioritizedSkillDay, secondarySkillDay, lastSkillDay });

			Target.ForDay(dateOnly);

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
			prioritizedSkill.SetCascadingIndex_UseFromTestOnly(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(prioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var prioritizedSkillDay = prioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var nonPrioritizedSkill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = differentActivity, TimeZone = TimeZoneInfo.Utc }.WithId();
			nonPrioritizedSkill.SetCascadingIndex_UseFromTestOnly(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(nonPrioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var nonPrioritizedSkillDay = nonPrioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent1 = new Person();
			agent1.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			agent1.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { prioritizedSkill, nonPrioritizedSkill });
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly);
			ass1.AddActivity(activity, new TimePeriod(5, 0, 10, 0));
			var agent2 = new Person();
			agent2.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			agent2.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { prioritizedSkill, nonPrioritizedSkill });
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly);
			ass2.AddActivity(activity, new TimePeriod(5, 0, 10, 0));
			SchedulerStateHolder.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent1, agent2 }, new[] { ass1, ass2 }, new[] { prioritizedSkillDay, nonPrioritizedSkillDay });

			Target.ForDay(dateOnly);

			prioritizedSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(1);

			nonPrioritizedSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldNotMoveResourceWhenSkillNotOpen()
		{
			var scenario = new Scenario("_");	
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var prioritizedSkill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			prioritizedSkill.SetCascadingIndex_UseFromTestOnly(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(prioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var prioritizedSkillDay = prioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var nonPrioritizedSkill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			nonPrioritizedSkill.SetCascadingIndex_UseFromTestOnly(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(nonPrioritizedSkill, new TimePeriod(12, 0, 13, 0));
			var nonPrioritizedSkillDay = nonPrioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent1 = new Person();
			agent1.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			agent1.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { prioritizedSkill, nonPrioritizedSkill });
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly);
			ass1.AddActivity(activity, new TimePeriod(5, 0, 10, 0));
			var agent2 = new Person();
			agent2.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			agent2.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { prioritizedSkill, nonPrioritizedSkill });
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly);
			ass2.AddActivity(activity, new TimePeriod(5, 0, 10, 0));
			SchedulerStateHolder.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent1, agent2 }, new[] { ass1, ass2 }, new[] { prioritizedSkillDay, nonPrioritizedSkillDay });

			Target.ForDay(dateOnly);

			prioritizedSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(1);
			nonPrioritizedSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldNotMoveResourceWhenMaxSeatSkillInvolved()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var prioritizedSkill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			prioritizedSkill.SetCascadingIndex_UseFromTestOnly(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(prioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var prioritizedSkillDay = prioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var nonPrioritizedSkill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.MaxSeatSkill)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			nonPrioritizedSkill.SetCascadingIndex_UseFromTestOnly(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(nonPrioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var nonPrioritizedSkillDay = nonPrioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent1 = new Person();
			agent1.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			agent1.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { prioritizedSkill, nonPrioritizedSkill });
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly);
			ass1.AddActivity(activity, new TimePeriod(5, 0, 10, 0));
			var agent2 = new Person();
			agent2.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			agent2.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { prioritizedSkill, nonPrioritizedSkill });
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly);
			ass2.AddActivity(activity, new TimePeriod(5, 0, 10, 0));
			SchedulerStateHolder.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent1, agent2 }, new[] { ass1, ass2 }, new[] { prioritizedSkillDay, nonPrioritizedSkillDay });

			Target.ForDay(dateOnly);

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
			prioritizedSkill.SetCascadingIndex_UseFromTestOnly(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(prioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var prioritizedSkillDay = prioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent1 = new Person();
			agent1.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			agent1.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { prioritizedSkill });
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly);
			ass1.AddActivity(activity, new TimePeriod(5, 0, 10, 0));
			var agent2 = new Person();
			agent2.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			agent2.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { prioritizedSkill });
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly);
			ass2.AddActivity(activity, new TimePeriod(5, 0, 10, 0));
			SchedulerStateHolder.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent1, agent2 }, new[] { ass1, ass2 }, new[] { prioritizedSkillDay });

			Target.ForDay(dateOnly);

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
			prioritizedSkill.SetCascadingIndex_UseFromTestOnly(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(prioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var prioritizedSkillDay = prioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var secondarySkill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			secondarySkill.SetCascadingIndex_UseFromTestOnly(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(secondarySkill, new TimePeriod(8, 0, 9, 0));
			var secondarySkillDay = secondarySkill.CreateSkillDayWithDemand(scenario, dateOnly, 0);
			var lastSkill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			lastSkill.SetCascadingIndex_UseFromTestOnly(3);
			WorkloadFactory.CreateWorkloadWithOpenHours(lastSkill, new TimePeriod(8, 0, 9, 0));
			var lastSkillDay = lastSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent1 = new Person();
			agent1.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			agent1.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { prioritizedSkill, secondarySkill });
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly);
			ass1.AddActivity(activity, new TimePeriod(5, 0, 10, 0));
			var agent2 = new Person();
			agent2.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			agent2.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { prioritizedSkill, secondarySkill });
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly);
			ass2.AddActivity(activity, new TimePeriod(5, 0, 10, 0));
			SchedulerStateHolder.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent1, agent2 }, new[] { ass1, ass2 }, new[] { prioritizedSkillDay, secondarySkillDay, lastSkillDay });

			Target.ForDay(dateOnly);

			prioritizedSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			secondarySkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			lastSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
		}
	}
}