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
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;


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

		[TestCase(TeamBlockType.Individual)]
		[TestCase(TeamBlockType.Team)]
		public void ShouldNotOverwriteLocalResourceCalculationContextInIsland(TeamBlockType teamBlockType)
		{
			var scenario = new Scenario();
			var activity = new Activity();
			var date = new DateOnly(2010, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(17, 0, 17, 0, 15), new ShiftCategory("_").WithId()));
			var skillA = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 17);
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, date, 0);
			var skillB = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 17);
			var skillDayB = skillB.CreateSkillDayWithDemand(scenario, date, 1);
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, skillA, skillB).WithSchedulePeriodOneWeek(date);
			var ass = new PersonAssignment(agent, scenario, date).WithLayer(activity, new TimePeriod(8, 17)).ShiftCategory(new ShiftCategory("_").WithId());
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, date, agent, ass, new[] { skillDayA, skillDayB });

			Target.Execute(new NoSchedulingProgress(), stateHolder, new[] {agent}, date.ToDateOnlyPeriod(),
				new OptimizationPreferences
				{
					General = {ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = true},
					Extra = teamBlockType.CreateExtraPreferences()
				}, null);

			stateHolder.SchedulingResultState.SkillDays[skillA].Single().SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			stateHolder.SchedulingResultState.SkillDays[skillB].Single().SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
		}
	}
}
