using System;
using System.Linq;
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
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTest]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher_SimulateNoNewThreads))]
	[LoggedOnAppDomain]
	[UseIocForFatClient]
	public class IntradayOptimizationCascadingSameThreadDesktopTest : IntradayOptimizationScenarioTest
	{
		public OptimizationDesktopExecuter Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;

		[Test]
		public void ShouldNotOverwriteLocalResourceCalculationContextInIsland()
		{
			if(_resourcePlannerRemoveImplicitResCalcContext46680==RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue)
				Assert.Ignore("#46898 #46923");
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var dateOnly = new DateOnly(2010, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(17, 0, 17, 0, 15), new ShiftCategory("_").WithId()));
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36)),
				PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9)
			};
			var skillA = new Skill("_").For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 17);
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 0);
			var skillB = new Skill("_").For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 17);
			var skillDayB = skillB.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skillA, skillB).WithSchedulePeriodOneWeek(dateOnly);
			var ass = new PersonAssignment(agent, scenario, dateOnly).WithLayer(phoneActivity, new TimePeriod(8, 17)).ShiftCategory(new ShiftCategory("_").WithId());
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent }, new[] { ass }, new[] { skillDayA, skillDayB });

			Target.Execute(new NoSchedulingProgress(), schedulerStateHolderFrom, new[] { agent }, dateOnly.ToDateOnlyPeriod(), new OptimizationPreferences { General = { ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = true } }, null);

			schedulerStateHolderFrom.SchedulingResultState.SkillDays[skillA].Single().SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			schedulerStateHolderFrom.SchedulingResultState.SkillDays[skillB].Single().SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
		}
		public IntradayOptimizationCascadingSameThreadDesktopTest(BreakPreferenceStartTimeByMax resourcePlannerBreakPreferenceStartTimeByMax46002, RemoveImplicitResCalcContext resourcePlannerRemoveImplicitResCalcContext46680) : base(resourcePlannerBreakPreferenceStartTimeByMax46002, resourcePlannerRemoveImplicitResCalcContext46680)
		{
		}
	}
}
