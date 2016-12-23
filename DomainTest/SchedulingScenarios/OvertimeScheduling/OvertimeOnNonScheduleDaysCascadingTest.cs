﻿using System;
using System.Drawing;
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
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.OvertimeScheduling
{
	[DomainTest]
	[TestFixture(true)]
	[TestFixture(false)]
	public class OvertimeOnNonScheduleDaysCascadingTest : IConfigureToggleManager
	{
		private readonly bool _resourcePlannerCascadingScheduleOvertimeOnPrimary41318;
		public ScheduleOvertime Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;

		public OvertimeOnNonScheduleDaysCascadingTest(bool resourcePlannerCascadingScheduleOvertimeOnPrimary41318)
		{
			_resourcePlannerCascadingScheduleOvertimeOnPrimary41318 = resourcePlannerCascadingScheduleOvertimeOnPrimary41318;
		}

		[Test]
		public void ShouldNotPlaceOvertimeShiftDueToNoUnderstaffingAfterShoveling()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_").WithId();
			var date = DateOnly.Today;
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), TimeSpan.FromHours(1), TimeSpan.FromHours(16)) };
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var skillA = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().WithCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillA, new TimePeriod(8, 0, 16, 0));
			var skillADay = skillA.CreateSkillDayWithDemand(scenario, date, 2);
			var skillB = new Skill("B").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().WithCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB, new TimePeriod(8, 0, 16, 0));
			var skillBDay = skillB.CreateSkillDayWithDemand(scenario, date, 2);

			var agentKnowingSkillAandB1 = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			agentKnowingSkillAandB1.AddPeriodWithSkills(new PersonPeriod(date, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }),
				new[] { skillA, skillB });
			agentKnowingSkillAandB1.AddSchedulePeriod(new SchedulePeriod(date, SchedulePeriodType.Day, 1));

			var agentKnowingSkillAandB2 = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			agentKnowingSkillAandB2.AddPeriodWithSkills(new PersonPeriod(date, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }),
				new[] { skillA, skillB });
			agentKnowingSkillAandB2.AddSchedulePeriod(new SchedulePeriod(date, SchedulePeriodType.Day, 1));

			var agentThatShouldNotGetOverTime = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			agentThatShouldNotGetOverTime.AddPeriodWithSkills(new PersonPeriod(date, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }),
				new[] { skillA });
			agentThatShouldNotGetOverTime.AddSchedulePeriod(new SchedulePeriod(date, SchedulePeriodType.Day, 1));

			var assAandB1 = new PersonAssignment(agentKnowingSkillAandB1, scenario, date);
			assAandB1.AddActivity(activity, new TimePeriod(8, 0, 16, 0));
			var assAandB2 = new PersonAssignment(agentKnowingSkillAandB2, scenario, date);
			assAandB2.AddActivity(activity, new TimePeriod(8, 0, 16, 0));


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
			var activity = new Activity("_").WithId();
			var dateOnly = new DateOnly(2016, 12, 13);
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), TimeSpan.FromHours(1), TimeSpan.FromHours(16)) };
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var skillA = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().WithCascadingIndex(1);
			var skillB = new Skill("B").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().WithCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillA, new TimePeriod(8, 0, 16, 0));
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB, new TimePeriod(8, 0, 16, 0));
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 1);	
			var skillDayB = skillB.CreateSkillDayWithDemand(scenario, dateOnly, 10);
			var agentAandB = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var agentA = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			agentAandB.AddPeriodWithSkills(new PersonPeriod(dateOnly, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { skillA, skillB });
			agentAandB.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Day, 1));
			agentA.AddPeriodWithSkills(new PersonPeriod(dateOnly, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { skillA, skillB });
			agentA.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Day, 1));
			var assAandB = new PersonAssignment(agentAandB, scenario, dateOnly);
			var assA = new PersonAssignment(agentA, scenario, dateOnly);
			assA.AddActivity(activity, new TimePeriod(8, 0, 16, 0));
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

			var overtimeWasPlaced = stateHolder.Schedules[agentAandB].ScheduledDay(dateOnly).PersonAssignment().OvertimeActivities().Any();
			if (_resourcePlannerCascadingScheduleOvertimeOnPrimary41318)
			{
				overtimeWasPlaced.Should().Be.False();
			}
			else
			{
				overtimeWasPlaced.Should().Be.True();
			}
		}

		[Test]
		public void ShouldPlaceOverTimeShiftEvenIfNoUnderstaffingOnPrimarySkillIfUseSkillAll()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_").WithId();
			var dateOnly = new DateOnly(2016, 12, 13);
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), TimeSpan.FromHours(1), TimeSpan.FromHours(16)) };
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var skillA = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().WithCascadingIndex(1);
			var skillB = new Skill("B").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().WithCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillA, new TimePeriod(8, 0, 16, 0));
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB, new TimePeriod(8, 0, 16, 0));
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var skillDayB = skillB.CreateSkillDayWithDemand(scenario, dateOnly, 10);
			var agentAandB = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var agentA = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			agentAandB.AddPeriodWithSkills(new PersonPeriod(dateOnly, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { skillA, skillB });
			agentAandB.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Day, 1));
			agentA.AddPeriodWithSkills(new PersonPeriod(dateOnly, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { skillA, skillB });
			agentA.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Day, 1));
			var assAandB = new PersonAssignment(agentAandB, scenario, dateOnly);
			var assA = new PersonAssignment(agentA, scenario, dateOnly);
			assA.AddActivity(activity, new TimePeriod(8, 0, 16, 0));
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

		public void Configure(FakeToggleManager toggleManager)
		{
			if (_resourcePlannerCascadingScheduleOvertimeOnPrimary41318)
			{
				toggleManager.Enable(Toggles.ResourcePlanner_CascadingScheduleOvertimeOnPrimary_41318);
			}
		}
	}
}