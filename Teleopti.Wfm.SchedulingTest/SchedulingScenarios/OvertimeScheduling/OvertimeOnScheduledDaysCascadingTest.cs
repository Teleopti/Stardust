using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios.OvertimeScheduling
{
	[DomainTest]
	public class OvertimeOnScheduledDaysCascadingTest : OvertimeSchedulingScenario
	{
		public ScheduleOvertime Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;

		[Test]
		public void ShouldNotPlaceOvertimeShiftDueToNoUnderstaffingAfterShoveling()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), TimeSpan.FromHours(1), TimeSpan.FromHours(16)) };
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var skillA = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 16);
			var skillADay = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 2);
			var skillB = new Skill("B").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 16);
			var skillBDay = skillB.CreateSkillDayWithDemand(scenario, dateOnly, 2);
			var agentKnowingSkillAandB1 = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract,skillA, skillB).WithSchedulePeriodOneDay(dateOnly);
			var agentKnowingSkillAandB2 = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skillA, skillB).WithSchedulePeriodOneDay(dateOnly);
			var agentThatShouldNotGetOverTime = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skillA).WithSchedulePeriodOneDay(dateOnly);
			var assAandB1 = new PersonAssignment(agentKnowingSkillAandB1, scenario, dateOnly).WithLayer(activity, new TimePeriod(8, 16));
			var assAandB2 = new PersonAssignment(agentKnowingSkillAandB2, scenario, dateOnly).WithLayer(activity, new TimePeriod(8, 16));
			var scheduleNotToTouch = new PersonAssignment(agentThatShouldNotGetOverTime, scenario, dateOnly).WithLayer(activity, new TimePeriod(8, 15));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agentKnowingSkillAandB1, agentKnowingSkillAandB2, agentThatShouldNotGetOverTime }, new[] { assAandB1, assAandB2, scheduleNotToTouch }, new[] { skillADay, skillBDay });
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = definitionSet,
				ScheduleTag = new ScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(15, 0, 16, 0),
				SelectedTimePeriod = new TimePeriod(1, 0, 1, 0),
				SkillActivity = activity
			};

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agentThatShouldNotGetOverTime].ScheduledDay(dateOnly) });

			stateHolder.Schedules[agentThatShouldNotGetOverTime].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities()
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotPlaceOverTimeShiftDueToNoUnderstaffingOnPrimarySkill()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = new DateOnly(2016, 12, 13);
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), TimeSpan.FromHours(1), TimeSpan.FromHours(16)) };
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var skillA = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 16);
			var skillB = new Skill("B").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 16);
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 0);
			var skillDayB = skillB.CreateSkillDayWithDemand(scenario, dateOnly, 10);
			var agentAandB = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skillA, skillB).WithSchedulePeriodOneDay(dateOnly);
			var assAandB = new PersonAssignment(agentAandB, scenario, dateOnly).WithLayer(activity, new TimePeriod(8, 15));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agentAandB }, new[] { assAandB }, new[] { skillDayA, skillDayB });
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = definitionSet,
				ScheduleTag = new ScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(15, 0, 16, 0),
				SelectedTimePeriod = new TimePeriod(1, 0, 1, 0),
				SkillActivity = activity,
				UseSkills = UseSkills.Primary
			};

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agentAandB].ScheduledDay(dateOnly) });

			stateHolder.Schedules[agentAandB].ScheduledDay(dateOnly).PersonAssignment().OvertimeActivities().Any()
				.Should().Be.False();
		}

		[Test]
		public void ShouldPlaceOverTimeShiftEvenIfNoUnderstaffingOnPrimarySkillIfUseSkillsIsAll()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = new DateOnly(2016, 12, 13);
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), TimeSpan.FromHours(1), TimeSpan.FromHours(16)) };
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var skillA = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 16);
			var skillB = new Skill("B").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 16);
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 0);
			var skillDayB = skillB.CreateSkillDayWithDemand(scenario, dateOnly, 10);
			var agentAandB = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skillA, skillB).WithSchedulePeriodOneDay(dateOnly);
			var assAandB = new PersonAssignment(agentAandB, scenario, dateOnly).WithLayer(activity, new TimePeriod(8, 15));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agentAandB }, new[] { assAandB }, new[] { skillDayA, skillDayB });
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = definitionSet,
				ScheduleTag = new ScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(15, 0, 16, 0),
				SelectedTimePeriod = new TimePeriod(1, 0, 1, 0),
				SkillActivity = activity,
				UseSkills = UseSkills.All
			};

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agentAandB].ScheduledDay(dateOnly) });

			var overtimeWasPlaced = stateHolder.Schedules[agentAandB].ScheduledDay(dateOnly).PersonAssignment().OvertimeActivities().Any();
			overtimeWasPlaced.Should().Be.True();
		}
	}
}