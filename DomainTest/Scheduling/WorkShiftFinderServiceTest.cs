using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;
using Teleopti.Wfm.SchedulingTest.SchedulingScenarios;
using Teleopti.Wfm.SchedulingTest.SchedulingScenarios.Scheduling;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
	[DomainTest]
	public class WorkShiftFinderServiceTest : SchedulingScenario
	{
		public Func<ISchedulerStateHolder> SchedulerStateHolder;
		public WorkShiftFinderService FinderService;
		public MatrixListFactory MatrixFactory;

		[Test]
		public void ShouldFilterOnOpenHours()
		{
			if (!ResourcePlannerTestParameters.IsEnabled(Toggles.ResourcePlanner_ConsiderOpenHoursWhenDecidingPossibleWorkTimes_76118))
				Assert.Ignore("only works with toggle on");

			var date = new DateOnly(2018, 10, 1);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1);
			var activity = new Activity { RequiresSkill = true }.WithId();
			var scenario = new Scenario();
			var skill = new Skill().For(activity).WithId().InTimeZone(TimeZoneInfo.Utc).IsOpenBetween(8, 13);
			var contract = new ContractWithMaximumTolerance();
			var ruleSet1 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(6, 0, 6, 0, 60), new TimePeriodWithSegment(14, 0, 14, 0, 60), new ShiftCategory().WithId()));
			var ruleSet2 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(13, 0, 13, 0, 60), new ShiftCategory().WithId()));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(new RuleSetBag(ruleSet1, ruleSet2), contract, skill).WithSchedulePeriodOneWeek(date);
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date, 10, 10, 10, 10, 10, 10, 10);
			var ass = new PersonAssignment(agent, scenario, date);
			var schedulerStateHolder = SchedulerStateHolder.Fill(scenario, period, new[] { agent }, new[] { ass }, skillDays);
			var scheduleDay = schedulerStateHolder.Schedules[agent].ScheduledDay(date);
			var matrixList = MatrixFactory.CreateMatrixListForSelection(schedulerStateHolder.Schedules, new List<IScheduleDay> { scheduleDay }).ToList();

			var result = FinderService.FindBestShift(scheduleDay, new SchedulingOptions(), matrixList[0], new EffectiveRestriction());

			result.ShiftProjection.MainShiftProjection().Period()?.ElapsedTime().TotalHours.Should().Be.EqualTo(5);
		}

		public WorkShiftFinderServiceTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}
