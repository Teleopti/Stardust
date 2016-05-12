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
	public class CascadingResourceCalculationTest
	{
		public CascadingResourceCalculation Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolder;
			
		[Test]
		public void ShouldCalculateNonCascadingSkills()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var nonCascadingSkill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc };
			WorkloadFactory.CreateWorkloadWithOpenHours(nonCascadingSkill, new TimePeriod(8, 0, 9, 0));
			var skillDay = nonCascadingSkill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(30)); //2 agents needed per interval
			var agent = new Person();
			agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			agent.AddPeriodWithSkill(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), nonCascadingSkill);
			var assignment = new PersonAssignment(agent, scenario, dateOnly);
			assignment.AddActivity(activity, new TimePeriod(5, 0, 10, 0)); //1 agent per interval
			SchedulerStateHolder.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent }, new[] {assignment}, skillDay);

			Target.ForDay(dateOnly);

			skillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(-1); 
		}

		[Test]
		public void ShouldMoveAllResourcesToPrioritySkill_OneAgent()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;

			var prioritizedSkill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc}.WithId();
			prioritizedSkill.SetCascadingIndex_UseFromTestOnly(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(prioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var prioritizedSkillDay = prioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(15)); //2 agents needed per interval
			var nonPrioritizedSkill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			nonPrioritizedSkill.SetCascadingIndex_UseFromTestOnly(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(nonPrioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var nonPrioritizedSkillDay = nonPrioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(15)); //2 agents needed per interval

			var agent1 = new Person();
			agent1.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			agent1.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { prioritizedSkill, nonPrioritizedSkill});
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly);
			ass1.AddActivity(activity, new TimePeriod(5, 0, 10, 0)); 

			SchedulerStateHolder.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent1 }, new[] { ass1 }, new[] { prioritizedSkillDay, nonPrioritizedSkillDay });

			Target.ForDay(dateOnly);

			prioritizedSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			nonPrioritizedSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldSplitResourceBetweenPrioritizedSkillAndNonCascadingSkill()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;

			var prioritizedSkill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			prioritizedSkill.SetCascadingIndex_UseFromTestOnly(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(prioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var prioritizedSkillDay = prioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(15)); //1 agent needed per interval

			var nonPrioritizedSkill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			nonPrioritizedSkill.SetCascadingIndex_UseFromTestOnly(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(nonPrioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var nonPrioritizedSkillDay = nonPrioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(15)); //1 agent needed per interval

			var nonCascadingSkill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			WorkloadFactory.CreateWorkloadWithOpenHours(nonCascadingSkill, new TimePeriod(8, 0, 9, 0));
			var nonCascadingSkillDay = nonCascadingSkill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(15)); //1 agent needed per interval

			var agent1 = new Person();
			agent1.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			agent1.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { prioritizedSkill, nonCascadingSkill, nonPrioritizedSkill });
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly);
			ass1.AddActivity(activity, new TimePeriod(5, 0, 10, 0));

			SchedulerStateHolder.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent1 }, new[] { ass1 }, new[] { prioritizedSkillDay,  nonCascadingSkillDay, nonPrioritizedSkillDay });

			Target.ForDay(dateOnly);

			prioritizedSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(-0.5);
			nonCascadingSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(-0.5);
		}

		[Test]
		public void ShouldNotConsiderNonActiveSkills()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;

			var prioritizedSkill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			prioritizedSkill.SetCascadingIndex_UseFromTestOnly(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(prioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var prioritizedSkillDay = prioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(15)); //2 agents needed per interval
			var nonPrioritizedSkill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			nonPrioritizedSkill.SetCascadingIndex_UseFromTestOnly(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(nonPrioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var nonPrioritizedSkillDay = nonPrioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(15)); //2 agents needed per interval

			var agent1 = new Person();
			agent1.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			agent1.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { prioritizedSkill, nonPrioritizedSkill });
			((PersonSkill) agent1.Period(dateOnly).PersonSkillCollection.Single(x => x.Skill.Equals(prioritizedSkill))).Active = false;
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly);
			ass1.AddActivity(activity, new TimePeriod(5, 0, 10, 0));

			SchedulerStateHolder.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent1 }, new[] { ass1 }, new[] { prioritizedSkillDay, nonPrioritizedSkillDay });

			Target.ForDay(dateOnly);

			prioritizedSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(-1);
			nonPrioritizedSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
		}
	}
}