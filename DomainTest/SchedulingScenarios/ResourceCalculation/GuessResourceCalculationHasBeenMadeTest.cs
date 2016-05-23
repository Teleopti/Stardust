using System;
using System.Drawing;
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
}