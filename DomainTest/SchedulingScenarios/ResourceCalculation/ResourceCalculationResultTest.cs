﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.ResourceCalculation
{
	[DomainTest]
	[TestFixture(true)]
	[TestFixture(false)]
	public class ResourceCalculationResultTest : IConfigureToggleManager
	{
		private readonly bool _resourcePlannerMaxSeatsNew40939;
		public Func<ISchedulerStateHolder> SchedulerStateHolder;
		public Func<IResourceOptimizationHelperExtended> ResourceOptimizationHelperExtended;
		public IInitMaxSeatForStateHolder InitMaxSeatForStateHolder;

		public ResourceCalculationResultTest(bool resourcePlannerMaxSeatsNew40939)
		{
			_resourcePlannerMaxSeatsNew40939 = resourcePlannerMaxSeatsNew40939;
		}

		[TestCase(2000, 0, 2000)]
		[TestCase(2000, 0.38, 3225)] //some "magic numbers" here to expose bug #40338
		public void ShouldCalculateEslCorrect(int demandedAgents, double shrinkage, int scheduledAgents)
		{
			const double serviceLevel = 0.8;
			var scenario = new Scenario("_");
			var date = DateOnly.Today;
			var activity = new Activity("_");
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpenBetween(9, 17);
			var skillDay = skill.CreateSkillDayWithDemand(scenario, date, demandedAgents);
			foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
			{
				skillStaffPeriod.Payload.UseShrinkage = true;
				skillStaffPeriod.Payload.Shrinkage = new Percent(shrinkage);
				skillStaffPeriod.Payload.ServiceAgreementData = skillStaffPeriod.Payload.ServiceAgreementData.WithServiceLevel(new ServiceLevel(new Percent(serviceLevel), 15 * 60));
			}
			var agents = Enumerable.Repeat(1,scheduledAgents).Select(i => new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skill).WithSchedulePeriodOneDay(date)).ToArray();
			var asses =
				agents.Select(agent => new PersonAssignment(agent, scenario, date).WithLayer(activity, new TimePeriod(9, 17)))
					.ToArray();

			SchedulerStateHolder.Fill(scenario, new DateOnlyPeriod(date, date), agents, asses, skillDay);

			ResourceOptimizationHelperExtended().ResourceCalculateAllDays(new NoSchedulingProgress(), false);

			skillDay.SkillStaffPeriodCollection.First().EstimatedServiceLevelShrinkage.Value
				.Should().Be.IncludedIn(serviceLevel-0.01, serviceLevel+0.01);
		}

		[Test]
		public void ShouldCalculateEslCorrectOnLowVolumes()
		{
			var scenario = new Scenario("_");
			var date = DateOnly.Today;
			var activity = new Activity("_");
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpenBetween(9, 17);
			var skillDay = skill.CreateSkillDayWithDemand(scenario, date, 2.4555555);
			foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
			{
				skillStaffPeriod.Payload.UseShrinkage = true;
				skillStaffPeriod.Payload.Shrinkage = new Percent(0.05);
				skillStaffPeriod.Payload.ServiceAgreementData = skillStaffPeriod.Payload.ServiceAgreementData.WithServiceLevel(new ServiceLevel(new Percent(0.8), 15*60));
				skillStaffPeriod.Payload.TaskData = new Task(7, TimeSpan.Zero, TimeSpan.FromSeconds(1));
			}
			var agent = new Person().WithId().WithPersonPeriod(skill).WithSchedulePeriodOneDay(date);
			var ass = new PersonAssignment(agent, scenario, date).WithLayer(activity, new TimePeriod(9, 17));
			SchedulerStateHolder.Fill(scenario, date.ToDateOnlyPeriod(), new[] {agent}, new[] {ass}, skillDay);

			ResourceOptimizationHelperExtended().ResourceCalculateAllDays(new NoSchedulingProgress(), false);

			skillDay.SkillStaffPeriodCollection.First().EstimatedServiceLevelShrinkage.Value.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldCalculateEslCorrectOnLowVolumesWithShrinkageBug42665()
		{
			var scenario = new Scenario("_");
			var date = DateOnly.Today;
			var activity = new Activity("_");
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpenBetween(9, 17);
			var skillDay = skill.CreateSkillDayWithDemand(scenario, date, 7.0643);
			foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
			{
				skillStaffPeriod.Payload.UseShrinkage = true;
				skillStaffPeriod.Payload.Shrinkage = new Percent(0.3);
				skillStaffPeriod.Payload.ServiceAgreementData = skillStaffPeriod.Payload.ServiceAgreementData.WithServiceLevel(new ServiceLevel(new Percent(0.8), 40));
				skillStaffPeriod.Payload.TaskData = new Task(50, TimeSpan.FromSeconds(180), TimeSpan.FromSeconds(0));
			}
			var agents = new List<IPerson>();
			var asses = new List<IPersonAssignment>();
			for (int i = 0; i < 10; i++) //this will give me a slight understaffing and ESL should be below 80%
			{
				agents.Add(new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skill).WithSchedulePeriodOneDay(date));
				asses.Add(new PersonAssignment(agents[i], scenario, date).WithLayer(activity, new TimePeriod(9, 17)));
			}
			SchedulerStateHolder.Fill(scenario, date.ToDateOnlyPeriod(), agents, asses, skillDay);

			ResourceOptimizationHelperExtended().ResourceCalculateAllDays(new NoSchedulingProgress(), false);

			skillDay.SkillStaffPeriodCollection.First().EstimatedServiceLevelShrinkage.Value.Should().Be.IncludedIn(0.75, 0.8);
		}

		[Test]
		public void ShouldSetCalculatedMaxUsedSeats()
		{
			var scenario = new Scenario("_");
			var date = DateOnly.Today;
			var activity = new Activity("_") {RequiresSeat = true};
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(new Team { Site = new Site("_") { MaxSeats = 10 }.WithId() });
			var ass = new PersonAssignment(agent, scenario, date).WithLayer(activity, new TimePeriod(9, 17));
			SchedulerStateHolder.Fill(scenario, date.ToDateOnlyPeriod(), new[] { agent }, new[] { ass }, Enumerable.Empty<ISkillDay>());
			InitMaxSeatForStateHolder.Execute(15);

			ResourceOptimizationHelperExtended().ResourceCalculateAllDays(new NoSchedulingProgress(), false);

			SchedulerStateHolder()
				.SchedulingResultState.SkillDays.Single()
				.Value.Single(x => x.CurrentDate == date)
				.SkillStaffPeriodCollection.Single(x => x.Period.StartDateTime.TimeOfDay == TimeSpan.FromHours(9))
				.Payload.CalculatedUsedSeats
				.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldBeAbleToSetIntervalLengthOnMaxSeatSkill()
		{
			const int intervalLength = 10;
			var scenario = new Scenario("_");
			var date = DateOnly.Today;
			var activity = new Activity("_") { RequiresSeat = true };
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(new Team { Site = new Site("_") { MaxSeats = 10 }.WithId() });
			var ass = new PersonAssignment(agent, scenario, date).WithLayer(activity, new TimePeriod(9, 17));
			SchedulerStateHolder.Fill(scenario, date.ToDateOnlyPeriod(), new[] { agent }, new[] { ass }, Enumerable.Empty<ISkillDay>());
			InitMaxSeatForStateHolder.Execute(intervalLength);

			ResourceOptimizationHelperExtended().ResourceCalculateAllDays(new NoSchedulingProgress(), false);

			SchedulerStateHolder()
				.SchedulingResultState.SkillDays.Single()
				.Value.Single(x => x.CurrentDate == date)
				.SkillStaffPeriodCollection.Single(x => x.Period.StartDateTime.TimeOfDay == TimeSpan.FromHours(9))
				.Period.EndDateTime.TimeOfDay
				.Minutes.Should().Be.EqualTo(intervalLength);
		}

		[Test]
		public void ShouldBeAbleToSetMaxSeatsForGrid()
		{
			const int numberOfSeats = 11;
			var scenario = new Scenario("_");
			var date = DateOnly.Today;
			var activity = new Activity("_") { RequiresSeat = true };
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(new Team { Site = new Site("_") { MaxSeats = numberOfSeats }.WithId() });
			var ass = new PersonAssignment(agent, scenario, date).WithLayer(activity, new TimePeriod(9, 17));
			SchedulerStateHolder.Fill(scenario, date.ToDateOnlyPeriod(), new[] { agent }, new[] { ass }, Enumerable.Empty<ISkillDay>());
			InitMaxSeatForStateHolder.Execute(15);

			ResourceOptimizationHelperExtended().ResourceCalculateAllDays(new NoSchedulingProgress(), false);

			SchedulerStateHolder()
				.SchedulingResultState.SkillDays.Single()
				.Value.Single(x => x.CurrentDate == date)
				.SkillStaffPeriodCollection.Single(x => x.Period.StartDateTime.TimeOfDay == TimeSpan.FromHours(9))
				.Payload.MaxSeats
				.Should().Be.EqualTo(numberOfSeats);
		}

		[Ignore("#42060")]
		[Test]
		public void ShouldCalculateResourcesOnSkillWithTimeZoneKathmandu([Values(15, 30)] int intervalLength)
		{
			var scenario = new Scenario("_");
			var date = DateOnly.Today;
			var activity = new Activity("_");
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpenBetween(9, 17);
			var skillDay = skill.CreateSkillDayWithDemand(scenario, date, 10);
			var agent = new Person().WithId().InTimeZone(TimeZoneInfoFactory.Kathmandu()).WithPersonPeriod(skill);
			var ass = new PersonAssignment(agent, scenario, date).WithLayer(activity, new TimePeriod(9, 17));
			SchedulerStateHolder.Fill(scenario, date.ToDateOnlyPeriod(), new[] { agent }, new[] { ass }, skillDay);

			ResourceOptimizationHelperExtended().ResourceCalculateAllDays(new NoSchedulingProgress(), false);

			skillDay.SkillStaffPeriodCollection.First().CalculatedResource
				.Should().Be.EqualTo(1);
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			if (_resourcePlannerMaxSeatsNew40939)
			{
				toggleManager.Enable(Toggles.ResourcePlanner_MaxSeatsNew_40939);
			}
			else
			{
				toggleManager.Disable(Toggles.ResourcePlanner_MaxSeatsNew_40939);
			}
		}
	}
}