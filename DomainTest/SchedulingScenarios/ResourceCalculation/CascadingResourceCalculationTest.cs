﻿using System;
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
			var nonCascadingSkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpenBetween(8, 9);
			var skillDay = nonCascadingSkill.CreateSkillDayWithDemand(scenario, dateOnly, 2);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(nonCascadingSkill);
			var assignment = new PersonAssignment(agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));

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
			var prioritizedSkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var prioritizedSkillDay = prioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var nonPrioritizedSkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var nonPrioritizedSkillDay = nonPrioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(prioritizedSkill, nonPrioritizedSkill);
			var ass = new PersonAssignment(agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));

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
			var prioritizedSkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var prioritizedSkillDay = prioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var nonPrioritizedSkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var nonPrioritizedSkillDay = nonPrioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(prioritizedSkill, nonPrioritizedSkill);
			var ass = new PersonAssignment(agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));
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
			var prioritizedSkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var prioritizedSkillDay = prioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var nonPrioritizedSkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var nonPrioritizedSkillDay = nonPrioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var nonCascadingSkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpenBetween(8, 9);
			var nonCascadingSkillDay = nonCascadingSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(prioritizedSkill, nonCascadingSkill, nonPrioritizedSkill);
			var ass = new PersonAssignment(agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));

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
			var prioritizedSkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var prioritizedSkillDay = prioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var nonPrioritizedSkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var nonPrioritizedSkillDay = nonPrioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(prioritizedSkill, nonPrioritizedSkill);
			((PersonSkill) agent.Period(dateOnly).PersonSkillCollection.Single(x => x.Skill.Equals(prioritizedSkill))).Active = false;
			var ass = new PersonAssignment(agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));

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
			var prioritizedSkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			((Skill)prioritizedSkill).SetDeleted();
			var prioritizedSkillDay = prioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var nonPrioritizedSkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var nonPrioritizedSkillDay = nonPrioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(prioritizedSkill, nonPrioritizedSkill);
			var ass = new PersonAssignment(agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));

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
			var highestPrioSkill = new Skill("_").For(new Activity("_").WithId()).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var highestPrioSkillDay = highestPrioSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var nonHighestPrioSkill = new Skill("_").For(scheduledActivity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var nonHighestPrioSkillDay = nonHighestPrioSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(highestPrioSkill, nonHighestPrioSkill);
			var ass = new PersonAssignment(agent, scenario, dateOnly).WithLayer(scheduledActivity, new TimePeriod(5, 10));

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
			var prioritizedSkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var prioritizedSkillDay = prioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var nonPrioritizedSkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var nonPrioritizedSkillDay = nonPrioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(prioritizedSkill, nonPrioritizedSkill);
			var ass = new PersonAssignment(agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));

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
			var prioritizedSkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var prioritizedSkillDay = prioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var nonPrioritizedSkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var nonPrioritizedSkillDay = nonPrioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(prioritizedSkill, nonPrioritizedSkill);
			var ass = new PersonAssignment(agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));
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
			var nonCascadingSkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpenBetween(8, 9);
			var skillDay = nonCascadingSkill.CreateSkillDayWithDemand(scenario, dateOnly, 2);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(nonCascadingSkill);
			var assignment = new PersonAssignment(agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));

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
		[Ignore("#43299")]
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
		[Ignore("#43299")]
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
		[Ignore("#43299")]
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
		[Ignore("#43299")]
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
		[Ignore("#43299")]
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
			var subskill2Day = subskill2.CreateSkillDayWithDemand(scenario, dateOnly, 0.1);	//0.1 understaffed
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
		[Ignore("#43299")]
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
		public void ShouldHandleSkillGroupOnlyContainingNoneCascadingSkills()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var prioritizedSkill = new Skill("_").DefaultResolution(60).For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var prioritizedSkillDay = prioritizedSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var nonCascadingSkill = new Skill("_").DefaultResolution(60).For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpenBetween(8, 9);
			var nonCascadingSkillDay = nonCascadingSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(nonCascadingSkill);
			var ass = new PersonAssignment(agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(8, 9));
			var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(prioritizedSkill);
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly).WithLayer(activity, new TimePeriod(8, 9));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass, ass1 }, new[] { prioritizedSkillDay, nonCascadingSkillDay }, false, false));

			prioritizedSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			nonCascadingSkillDay.SkillStaffPeriodCollection.Last().AbsoluteDifference
				.Should().Be.EqualTo(0);
		}
	}
}