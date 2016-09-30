﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NHibernate.Criterion;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.ResourceCalculation
{
	[DomainTest]
	public class GuessResourceCalculationHasBeenMadeTest
	{
		public Func<ISchedulerStateHolder> SchedulerStateHolder;
		public Func<IResourceOptimizationHelperExtended> ResourceOptimizationHelperExtended;

		[Test]
		public void NoResourceCalculationHasBeenMade()
		{
			var scenario = new Scenario("_");
			var date = new DateOnly(2000, 1, 2);
			var activity = new Activity("_");
			var skill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony))
			{
				Activity = activity,
				TimeZone = TimeZoneInfo.Utc
			};
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			var skillDay = skill.CreateSkillDayWithDemand(scenario, date, TimeSpan.FromMinutes(60));
			var agent = new Person().WithId();
			agent.AddPeriodWithSkill(new PersonPeriod(date, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), skill);
			agent.AddSchedulePeriod(new SchedulePeriod(date, SchedulePeriodType.Day, 1));
			var ass = new PersonAssignment(agent, scenario, date);
			ass.AddActivity(activity, new TimePeriod(9, 0, 17, 0));
			var stateHolder = SchedulerStateHolder.Fill(scenario, new DateOnlyPeriod(date, date), new[] { agent }, new[] { ass }, skillDay);

			stateHolder.SchedulingResultState.GuessResourceCalculationHasBeenMade()
				.Should().Be.False();
		}

		[Test]
		public void ResourceCalculationHasBeenMade()
		{
			var scenario = new Scenario("_");
			var date = new DateOnly(2000, 1, 2);
			var activity = new Activity("_");
			var skill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony))
			{
				Activity = activity,
				TimeZone = TimeZoneInfo.Utc
			};
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			var skillDay = skill.CreateSkillDayWithDemand(scenario, date, TimeSpan.FromMinutes(60));
			var agent = new Person().WithId();
			agent.AddPeriodWithSkill(new PersonPeriod(date, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), skill);
			agent.AddSchedulePeriod(new SchedulePeriod(date, SchedulePeriodType.Day, 1));
			var ass = new PersonAssignment(agent, scenario, date);
			ass.AddActivity(activity, new TimePeriod(9, 0, 17, 0));
			var stateHolder = SchedulerStateHolder.Fill(scenario, new DateOnlyPeriod(date, date), new[] { agent}, new[] {ass}, skillDay);

			ResourceOptimizationHelperExtended().ResourceCalculateAllDays(new NoSchedulingProgress(), false);

			stateHolder.SchedulingResultState.GuessResourceCalculationHasBeenMade()
				.Should().Be.True();
		}
	}

	[DomainTest]
	public class ResourceCalculationTest
	{
		public Func<ISchedulerStateHolder> SchedulerStateHolder;
		public Func<IResourceOptimizationHelperExtended> ResourceOptimizationHelperExtended;

		[Test, Ignore(" #40338")]
		public void ShouldCalculateEslWithShrinkageCorrect()
		{
			var scenario = new Scenario("_");
			var date = new DateOnly(2000, 1, 2);
			var activity = new Activity("_");
			var skill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony))
			{
				Activity = activity,
				TimeZone = TimeZoneInfo.Utc
			};
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, new TimePeriod(9, 0, 17, 0));
			var skillDay = skill.CreateSkillDayWithDemand(scenario, date, 2000);
			foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
			{
				skillStaffPeriod.Payload.UseShrinkage = true;
				skillStaffPeriod.Payload.Shrinkage = new Percent(0.38);
			}

			var agents = new List<IPerson>();
			var asses = new List<IPersonAssignment>();

			for (var i = 0; i < 3225; i++)
			{
				var agent = new Person().WithId();
				agent.AddPeriodWithSkill(new PersonPeriod(date, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")),  new Team { Site = new Site("_") }), skill);
				agent.AddSchedulePeriod(new SchedulePeriod(date, SchedulePeriodType.Day, 1));
				var ass = new PersonAssignment(agent, scenario, date);
				ass.AddActivity(activity, new TimePeriod(9, 0, 17, 0));

				agents.Add(agent);
				asses.Add(ass);
			}
			SchedulerStateHolder.Fill(scenario, new DateOnlyPeriod(date, date), agents, asses, skillDay);

			ResourceOptimizationHelperExtended().ResourceCalculateAllDays(new NoSchedulingProgress(), false);

			skillDay.SkillStaffPeriodCollection.First().EstimatedServiceLevelShrinkage.Value.Should().Be.IncludedIn(0.79, 0.81);
		}
	}
}