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
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios.OvertimeScheduling
{
	[DomainTest]
	public class OvertimeOnNonScheduleDaysCascadingTest : OvertimeSchedulingScenario
	{
		public ScheduleOvertime Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;

		[Test]
		public void ShouldNotPlaceOvertimeShiftDueToNoUnderstaffingAfterShoveling()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var date = DateOnly.Today;
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), TimeSpan.FromHours(1), TimeSpan.FromHours(16)) };
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var skillA = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 16);
			var skillADay = skillA.CreateSkillDayWithDemand(scenario, date, 2);
			var skillB = new Skill("B").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 16);
			var skillBDay = skillB.CreateSkillDayWithDemand(scenario, date, 2);
			var agentKnowingSkillAandB1 = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skillA, skillB).WithSchedulePeriodOneDay(date);
			var agentKnowingSkillAandB2 = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skillA, skillB).WithSchedulePeriodOneDay(date);
			var agentThatShouldNotGetOverTime = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skillA).WithSchedulePeriodOneDay(date);
			var assAandB1 = new PersonAssignment(agentKnowingSkillAandB1, scenario, date).WithLayer(activity, new TimePeriod(8, 16));
			var assAandB2 = new PersonAssignment(agentKnowingSkillAandB2, scenario, date).WithLayer(activity, new TimePeriod(8, 16));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(date, date), new[] { agentKnowingSkillAandB1, agentKnowingSkillAandB2, agentThatShouldNotGetOverTime }, new[] { assAandB1, assAandB2 }, new[] { skillADay, skillBDay });
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var overtimePreference = new OvertimePreferences { OvertimeType = definitionSet, ShiftBagToUse = new RuleSetBag(ruleSet), ScheduleTag = new ScheduleTag() };

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agentThatShouldNotGetOverTime].ScheduledDay(date) });

			stateHolder.Schedules[agentThatShouldNotGetOverTime].ScheduledDay(date).PersonAssignment(true).OvertimeActivities()
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
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 1);	
			var skillDayB = skillB.CreateSkillDayWithDemand(scenario, dateOnly, 10);
			var agentAandB = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skillA, skillB).WithSchedulePeriodOneDay(dateOnly);
			var agentA = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skillA, skillB).WithSchedulePeriodOneDay(dateOnly);
			var assAandB = new PersonAssignment(agentAandB, scenario, dateOnly);
			var assA = new PersonAssignment(agentA, scenario, dateOnly).WithLayer(activity, new TimePeriod(8, 16));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agentAandB, agentA}, new[] { assAandB, assA}, new[] { skillDayA, skillDayB });
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = definitionSet,
				ShiftBagToUse = new RuleSetBag(ruleSet),
				ScheduleTag = new ScheduleTag(),
				UseSkills = UseSkills.Primary
			};

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agentAandB].ScheduledDay(dateOnly) });

			stateHolder.Schedules[agentAandB].ScheduledDay(dateOnly).PersonAssignment().OvertimeActivities().Any()
					.Should().Be.False();
		}

		[Test]
		public void ShouldPlaceOverTimeShiftEvenIfNoUnderstaffingOnPrimarySkillIfUseSkillAll()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = new DateOnly(2016, 12, 13);
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), TimeSpan.FromHours(1), TimeSpan.FromHours(16)) };
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var skillA = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 16);
			var skillB = new Skill("B").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 16);
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var skillDayB = skillB.CreateSkillDayWithDemand(scenario, dateOnly, 10);
			var agentAandB = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skillA, skillB).WithSchedulePeriodOneDay(dateOnly);
			var agentA = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA, skillB).WithSchedulePeriodOneDay(dateOnly);
			var assAandB = new PersonAssignment(agentAandB, scenario, dateOnly);
			var assA = new PersonAssignment(agentA, scenario, dateOnly).WithLayer(activity, new TimePeriod(8, 16));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agentAandB, agentA }, new[] { assAandB, assA }, new[] { skillDayA, skillDayB });
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = definitionSet,
				ShiftBagToUse = new RuleSetBag(ruleSet),
				ScheduleTag = new ScheduleTag(),
				UseSkills = UseSkills.All
			};

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agentAandB].ScheduledDay(dateOnly) });

			var overtimeWasPlaced = stateHolder.Schedules[agentAandB].ScheduledDay(dateOnly).PersonAssignment().OvertimeActivities().Any();
			overtimeWasPlaced.Should().Be.True();
		}
	}
}