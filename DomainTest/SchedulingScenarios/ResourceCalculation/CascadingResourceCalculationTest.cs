﻿using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Cascading;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.ResourceCalculation
{
	[DomainTest]
	public class CascadingResourceCalculationTest
	{
		public CascadingResourceCalculation Target;
		public IPersonSkillProvider PersonSkillProvider;
		public CascadingResourceCalculationContextFactory ResourceCalculationContextFactory;

		[Test]
		public void ShouldCalculateNonCascadingSkills()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var nonCascadingSkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc);
			WorkloadFactory.CreateWorkloadWithOpenHours(nonCascadingSkill, new TimePeriod(8, 0, 9, 0));
			var skillDay = nonCascadingSkill.CreateSkillDayWithDemand(scenario, dateOnly, 2);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent.AddPeriodWithSkill(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), nonCascadingSkill);
			var assignment = new PersonAssignment(agent, scenario, dateOnly);
			assignment.AddActivity(activity, new TimePeriod(5, 0, 10, 0)); //1 agent per interval

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { assignment }, new[] { skillDay }, false, false));

			skillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(-1); 
		}

		[Test]
		public void ShouldMoveAllResourcesToPrioritySkill_OneAgent()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var prioritizedSkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().WithCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(prioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var prioritizedSkillDay = prioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var nonPrioritizedSkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().WithCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(nonPrioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var nonPrioritizedSkillDay = nonPrioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).KnowsSkill(prioritizedSkill, nonPrioritizedSkill);
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(activity, new TimePeriod(5, 0, 10, 0));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass }, new[] { prioritizedSkillDay, nonPrioritizedSkillDay }, false, false));

			prioritizedSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			nonPrioritizedSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldWorkWithOuterContext()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var prioritizedSkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().WithCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(prioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var prioritizedSkillDay = prioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var nonPrioritizedSkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().WithCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(nonPrioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var nonPrioritizedSkillDay = nonPrioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).KnowsSkill(prioritizedSkill, nonPrioritizedSkill);
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(activity, new TimePeriod(5, 0, 10, 0));
			var resCalcData = ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] {ass}, new[] {prioritizedSkillDay, nonPrioritizedSkillDay}, false, false);

			using (ResourceCalculationContextFactory.Create(resCalcData.Schedules, resCalcData.Skills, false, dateOnly.ToDateOnlyPeriod()))
			{
				Target.ResourceCalculate(dateOnly, resCalcData);

				prioritizedSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
					.Should().Be.EqualTo(0);
				nonPrioritizedSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
					.Should().Be.EqualTo(-1);
			}
		}

		[Test]
		public void ShouldSplitResourceBetweenPrioritizedSkillAndNonCascadingSkill()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var prioritizedSkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().WithCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(prioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var prioritizedSkillDay = prioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var nonPrioritizedSkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().WithCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(nonPrioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var nonPrioritizedSkillDay = nonPrioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var nonCascadingSkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId();
			WorkloadFactory.CreateWorkloadWithOpenHours(nonCascadingSkill, new TimePeriod(8, 0, 9, 0));
			var nonCascadingSkillDay = nonCascadingSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).KnowsSkill(prioritizedSkill, nonCascadingSkill, nonPrioritizedSkill);
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(activity, new TimePeriod(5, 0, 10, 0));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass }, new[] { prioritizedSkillDay, nonCascadingSkillDay, nonPrioritizedSkillDay }, false, false));

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
			var prioritizedSkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().WithCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(prioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var prioritizedSkillDay = prioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var nonPrioritizedSkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().WithCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(nonPrioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var nonPrioritizedSkillDay = nonPrioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).KnowsSkill(prioritizedSkill, nonPrioritizedSkill);
			((PersonSkill) agent.Period(dateOnly).PersonSkillCollection.Single(x => x.Skill.Equals(prioritizedSkill))).Active = false;
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(activity, new TimePeriod(5, 0, 10, 0));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass }, new[] { prioritizedSkillDay, nonPrioritizedSkillDay }, false, false));

			prioritizedSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(-1);
			nonPrioritizedSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotConsiderDeletedSkills()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var prioritizedSkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().WithCascadingIndex(1);
			((Skill)prioritizedSkill).SetDeleted();
			WorkloadFactory.CreateWorkloadWithOpenHours(prioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var prioritizedSkillDay = prioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var nonPrioritizedSkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().WithCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(nonPrioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var nonPrioritizedSkillDay = nonPrioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).KnowsSkill(prioritizedSkill, nonPrioritizedSkill);
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(activity, new TimePeriod(5, 0, 10, 0));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass }, new[] { prioritizedSkillDay, nonPrioritizedSkillDay }, false, false));

			prioritizedSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(-1);
			nonPrioritizedSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldUseNotHighestPrioritizedSkillIfActivityPointsToThatOne()
		{
			var scenario = new Scenario("_");
			var scheduledActivity = new Activity("activity that will be scheduled").WithId();
			var dateOnly = DateOnly.Today;
			var highestPrioSkill = new Skill("_").For(new Activity("_").WithId()).InTimeZone(TimeZoneInfo.Utc).WithId().WithCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(highestPrioSkill, new TimePeriod(8, 0, 9, 0));
			var highestPrioSkillDay = highestPrioSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var nonHighestPrioSkill = new Skill("_").For(scheduledActivity).InTimeZone(TimeZoneInfo.Utc).WithId().WithCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(nonHighestPrioSkill, new TimePeriod(8, 0, 9, 0));
			var nonHighestPrioSkillDay = nonHighestPrioSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).KnowsSkill(highestPrioSkill, nonHighestPrioSkill);
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(scheduledActivity, new TimePeriod(5, 0, 10, 0));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass }, new[] { highestPrioSkillDay, nonHighestPrioSkillDay }, false, false));

			highestPrioSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(-1);
			nonHighestPrioSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotRemoveAnyPersonSkillFromPersonEntity()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var prioritizedSkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().WithCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(prioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var prioritizedSkillDay = prioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var nonPrioritizedSkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().WithCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(nonPrioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var nonPrioritizedSkillDay = nonPrioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).KnowsSkill(prioritizedSkill, nonPrioritizedSkill);
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(activity, new TimePeriod(5, 0, 10, 0));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass }, new[] { prioritizedSkillDay, nonPrioritizedSkillDay }, false, false));

			agent.Period(dateOnly).PersonSkillCollection.Count(x => x.Active)
				.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldMoveAllResourcesToPrioritySkillWhenSkillCombinationsAlreadyBeenSet_OneAgent()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var prioritizedSkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().WithCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(prioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var prioritizedSkillDay = prioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var nonPrioritizedSkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().WithCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(nonPrioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var nonPrioritizedSkillDay = nonPrioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).KnowsSkill(prioritizedSkill, nonPrioritizedSkill);
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(activity, new TimePeriod(5, 0, 10, 0));
			PersonSkillProvider.SkillsOnPersonDate(agent, dateOnly);

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass }, new[] { prioritizedSkillDay, nonPrioritizedSkillDay }, false, false));

			prioritizedSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			nonPrioritizedSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldCalculateAllDays()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var nonCascadingSkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc);
			WorkloadFactory.CreateWorkloadWithOpenHours(nonCascadingSkill, new TimePeriod(8, 0, 9, 0));
			var skillDay = nonCascadingSkill.CreateSkillDayWithDemand(scenario, dateOnly, 2);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent.AddPeriodWithSkill(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), nonCascadingSkill);
			var assignment = new PersonAssignment(agent, scenario, dateOnly);
			assignment.AddActivity(activity, new TimePeriod(5, 0, 10, 0)); //1 agent per interval

			Target.ResourceCalculate(new DateOnlyPeriod(dateOnly.AddDays(-1), dateOnly.AddDays(1)), ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { assignment }, new[] { skillDay }, false, false));

			skillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldNotConsiderSkillsWithoutOpenHours()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var prioritizedSkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().WithCascadingIndex(1);
			WorkloadFactory.CreateWorkloadThatIsClosed(prioritizedSkill);
			var prioritizedSkillDay = prioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var nonPrioritizedSkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().WithCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(nonPrioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var nonPrioritizedSkillDay = nonPrioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).KnowsSkill(prioritizedSkill, nonPrioritizedSkill);
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(activity, new TimePeriod(5, 0, 10, 0));

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
			var prioritizedSkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().WithCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(prioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var prioritizedSkillDay = prioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var nonPrioritizedSkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().WithCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(nonPrioritizedSkill, new TimePeriod(8, 0, 10, 0));
			var nonPrioritizedSkillDay = nonPrioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).KnowsSkill(prioritizedSkill, nonPrioritizedSkill);
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(activity, new TimePeriod(8, 0, 10, 0));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass }, new[] { prioritizedSkillDay, nonPrioritizedSkillDay }, false, false));

			prioritizedSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			nonPrioritizedSkillDay.SkillStaffPeriodCollection.Last().AbsoluteDifference
				.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldHandleSkillGroupOnlyContainingNoneCascadingSkills()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var prioritizedSkill = new Skill("_").DefaultResolution(60).For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().WithCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(prioritizedSkill, new TimePeriod(8, 0, 9, 0));
			var prioritizedSkillDay = prioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var nonCascadingSkill = new Skill("_").DefaultResolution(60).For(activity).InTimeZone(TimeZoneInfo.Utc).WithId();
			WorkloadFactory.CreateWorkloadWithOpenHours(nonCascadingSkill, new TimePeriod(8, 0, 9, 0));
			var nonCascadingSkillDay = nonCascadingSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).KnowsSkill(nonCascadingSkill);
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(activity, new TimePeriod(8, 0, 9, 0)); //will produce one skillgroup with only the nonCascadingSkill
			var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc).KnowsSkill(prioritizedSkill);
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly);
			ass1.AddActivity(activity, new TimePeriod(8, 0, 9, 0));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass, ass1 }, new[] { prioritizedSkillDay, nonCascadingSkillDay }, false, false));

			prioritizedSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			nonCascadingSkillDay.SkillStaffPeriodCollection.Last().AbsoluteDifference
				.Should().Be.EqualTo(0);
		}
	}
}