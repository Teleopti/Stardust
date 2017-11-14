using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTest]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	[LoggedOnAppDomain]
	[UseIocForFatClient]
	public class IntradayCombinationDesktopTest : IntradayOptimizationScenarioTest
	{
		public OptimizationDesktopExecuter Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderOrg;
	
		[Test]
		//[Ignore("46680 - to be fixed")]
		public void ShouldWorkInCombinationWithFlexibleWorkTime()
		{
			var date = new DateOnly(2014, 4, 1);
			var scenario = new Scenario().WithId();
			var shiftCategory = new ShiftCategory().WithId();
			var activity = new Activity().WithId();
			var skill = new Skill().WithId().For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen();
			var workShiftRuleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = new Person().WithPersonPeriod(workShiftRuleSet, skill).WithId().InTimeZone(TimeZoneInfo.Utc).WithSchedulePeriodOneDay(date);
			var ass = new PersonAssignment(agent, scenario, date).WithLayer(activity, new TimePeriod(7, 15)).ShiftCategory(shiftCategory);
			var skillDay = skill.CreateSkillDayWithDemandOnInterval(scenario, date, 1, new Tuple<TimePeriod, double>(new TimePeriod(15, 16), 2));
			var stateHolder = SchedulerStateHolderOrg.Fill(scenario, date, new[] { agent }, new[] { ass }, skillDay);
			var optimizationPreferences = new OptimizationPreferences
			{
				General = new GeneralPreferences { ScheduleTag = NullScheduleTag.Instance, OptimizationStepShiftsWithinDay = true, OptimizationStepDaysOffForFlexibleWorkTime = true}
			};

			Target.Execute(new NoSchedulingProgress(), stateHolder, new[] { agent }, date.ToDateOnlyPeriod(), optimizationPreferences, new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()));

			stateHolder.Schedules[agent].ScheduledDay(date).PersonAssignment().Period.StartDateTime.Hour
				.Should().Be.EqualTo(8);
		}

		public IntradayCombinationDesktopTest(OptimizationCodeBranch resourcePlannerMergeTeamblockClassicIntraday45508, BreakPreferenceStartTimeByMax resourcePlannerBreakPreferenceStartTimeByMax46002, RemoveImplicitResCalcContext resourcePlannerRemoveImplicitResCalcContext46680) : base(resourcePlannerMergeTeamblockClassicIntraday45508, resourcePlannerBreakPreferenceStartTimeByMax46002, resourcePlannerRemoveImplicitResCalcContext46680)
		{
		}
	}
}
