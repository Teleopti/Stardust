using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.ResourceCalculation
{
	[DomainTest]
	[UseIocForFatClient]
	public class MaxSeatResourceCalculationDesktopTest : ResourceCalculationScenario
	{
		public ResourceCalculateWithNewContext Target;
		public InitMaxSeatForStateHolder InitMaxSeatForStateHolder;
		public Func<ISchedulerStateHolder> StateHolder;
		
		[Test, Ignore("#77273 To be fixed")]
		public void ShouldNotMoveAnyResourcesToPrioritySkill3()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_"){RequiresSeat = true};
			var dateOnly = DateOnly.Today;
			var prioritizedSkill1 = new Skill("skill1").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var prioritizedSkillDay1 = prioritizedSkill1.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var prioritizedSkill2 = new Skill("skill2").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var prioritizedSkillDay2 = prioritizedSkill2.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var prioritizedSkill3 = new Skill("skill3").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(3).IsOpenBetween(8, 9);
			var prioritizedSkillDay3 = prioritizedSkill3.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var site1 = new Site("site1") {MaxSeats = 1}.WithId(Guid.NewGuid());
			var site2 = new Site("site2") {MaxSeats = 1}.WithId(Guid.NewGuid());
			var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(new Team {Site = site1}, prioritizedSkill1, prioritizedSkill2, prioritizedSkill3);
			var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(new Team {Site = site2}, prioritizedSkill1, prioritizedSkill2, prioritizedSkill3);
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));
			var stateHolder = StateHolder.Fill(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agent1, agent2}, new []{ass1, ass2}, new []{prioritizedSkillDay1, prioritizedSkillDay2, prioritizedSkillDay3});
			InitMaxSeatForStateHolder.Execute(stateHolder.SchedulingResultState.MinimumSkillIntervalLength());
			
			var resCalcData = stateHolder.SchedulingResultState.ToResourceOptimizationData(false, false);
			Target.ResourceCalculate(dateOnly, resCalcData);

			prioritizedSkillDay1.SkillStaffPeriodCollection.First().CalculatedResource.Should().Be.EqualTo(1);
			prioritizedSkillDay2.SkillStaffPeriodCollection.First().CalculatedResource.Should().Be.EqualTo(1);
			prioritizedSkillDay3.SkillStaffPeriodCollection.First().CalculatedResource.Should().Be.EqualTo(0);
		}
	}
}