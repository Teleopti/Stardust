using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;


namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.ResourceCalculation
{
	[DomainTest]
	public class GuessResourceCalculationHasBeenMadeTest : ResourceCalculationScenario
	{
		public Func<ISchedulerStateHolder> SchedulerStateHolder;
		public ResourceCalculateWithNewContext Target;

		[Test]
		public void NoResourceCalculationHasBeenMade()
		{
			var scenario = new Scenario("_");
			var date = new DateOnly(2000, 1, 2);
			var activity = new Activity("_");
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, date, TimeSpan.FromMinutes(60));
			var agent = new Person().WithId().WithPersonPeriod(skill).WithSchedulePeriodOneDay(date);
			var ass = new PersonAssignment(agent, scenario, date).WithLayer(activity, new TimePeriod(9, 17));
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
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, date, TimeSpan.FromMinutes(60));
			var agent = new Person().WithId().WithPersonPeriod(skill).WithSchedulePeriodOneDay(date);
			var ass = new PersonAssignment(agent, scenario, date).WithLayer(activity, new TimePeriod(9, 17));
			var stateHolder = SchedulerStateHolder.Fill(scenario, new DateOnlyPeriod(date, date), new[] { agent}, new[] {ass}, skillDay);

			Target.ResourceCalculate(date, new ResourceCalculationData(stateHolder.SchedulingResultState, false, false));

			stateHolder.SchedulingResultState.GuessResourceCalculationHasBeenMade()
				.Should().Be.True();
		}
	}
}