using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;


namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.FairnessOptimization
{
	[DomainTest]
	[UseIocForFatClient]
	public class FairnessOptimizationDesktopTest
	{
		public OptimizationDesktopExecuter Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolder;

		[Test]
		public void ShouldHandleSchedulePeriodOutsideSelectedPeriod()
		{
			var date = new DateOnly(2018, 06, 1);
			var scenario  = new Scenario();
			var activity = new Activity();
			var shiftCategory = new ShiftCategory().WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), new ShiftCategory("_").WithId()));
			var ruleSetBag = new RuleSetBag(ruleSet).WithId();
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1);
			var agent1 = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(date.AddDays(-7), ruleSetBag).WithSchedulePeriodOneWeek(date);
			var agent2 = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSetBag).WithSchedulePeriodOneWeek(date.AddDays(-1));
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.SetFairnessType(FairnessType.Seniority);
			agent1.WorkflowControlSet = workflowControlSet;
			agent2.WorkflowControlSet = workflowControlSet;
			var assDayOff1 = new PersonAssignment(agent1, scenario, date.AddDays(6)).WithDayOff();
			var ass1 = new PersonAssignment(agent1, scenario, date.AddDays(5)).WithLayer(activity, new TimePeriod(8, 17)).ShiftCategory(shiftCategory);
			var assDayOff2 = new PersonAssignment(agent2, scenario, date.AddDays(3)).WithDayOff();
			var ass2 = new PersonAssignment(agent2, scenario, date.AddDays(5)).WithLayer(activity, new TimePeriod(8, 17)).ShiftCategory(shiftCategory);
			var stateHolder = SchedulerStateHolder.Fill(scenario, period, new[] { agent1, agent2 }, new List<IScheduleData>{ ass1, ass2, assDayOff1, assDayOff2}, new List<ISkillDay>());
			stateHolder.SchedulingResultState.SeniorityWorkDayRanks = new SeniorityWorkDayRanks();
			stateHolder.CommonStateHolder.SetShiftCategories(new List<IShiftCategory> {shiftCategory});
			var optPreferences = new OptimizationPreferences{General = { ScheduleTag = new ScheduleTag(), OptimizationStepFairness = true }};

			Assert.DoesNotThrow(() =>
			{
				Target.Execute(new NoSchedulingProgress(), stateHolder, new[] { agent1, agent2 }, period, optPreferences, new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()));
			});
		}

		[Test]
		public void ShouldHandleWeekBeforeAndDiffrentSchedulePeriods()
		{
			var date = new DateOnly(2018, 06, 1);
			var scenario = new Scenario();
			var activity = new Activity();
			var shiftCategory = new ShiftCategory().WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), new ShiftCategory("_").WithId()));
			var ruleSetBag = new RuleSetBag(ruleSet).WithId();
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1);
			var agent1 = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(date.AddDays(-7), ruleSetBag).WithSchedulePeriodOneWeek(date.AddDays(-4));
			var agent2 = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSetBag).WithSchedulePeriodOneWeek(date);
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.SetFairnessType(FairnessType.Seniority);
			agent1.WorkflowControlSet = workflowControlSet;
			agent2.WorkflowControlSet = workflowControlSet;
			var assDayOff1 = new PersonAssignment(agent1, scenario, date.AddDays(1)).WithDayOff();
			var ass1 = new PersonAssignment(agent1, scenario, date.AddDays(2)).WithLayer(activity, new TimePeriod(8, 17)).ShiftCategory(shiftCategory);
			var assDayOff2 = new PersonAssignment(agent2, scenario, date.AddDays(5)).WithDayOff();
			var ass2 = new PersonAssignment(agent2, scenario, date.AddDays(2)).WithLayer(activity, new TimePeriod(8, 17)).ShiftCategory(shiftCategory);
			var ass3 = new PersonAssignment(agent2, scenario, date.AddDays(1)).WithLayer(activity, new TimePeriod(8, 17)).ShiftCategory(shiftCategory);
			var stateHolder = SchedulerStateHolder.Fill(scenario, period, new[] { agent1, agent2 }, new List<IScheduleData> { ass1, ass2, ass3, assDayOff1, assDayOff2 }, new List<ISkillDay>());
			stateHolder.SchedulingResultState.SeniorityWorkDayRanks = new SeniorityWorkDayRanks();
			stateHolder.CommonStateHolder.SetShiftCategories(new List<IShiftCategory> { shiftCategory });
			var optPreferences = new OptimizationPreferences { General = { ScheduleTag = new ScheduleTag(), OptimizationStepFairness = true } };
			var dayOffPreferences = new DaysOffPreferences {ConsiderWeekBefore = true};

			Assert.DoesNotThrow(() =>
			{
				Target.Execute(new NoSchedulingProgress(), stateHolder, new[] { agent1, agent2 }, period, optPreferences, new FixedDayOffOptimizationPreferenceProvider(dayOffPreferences));
			});
		}
	}
}
