using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTest]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	[LoggedOnAppDomain]
	[UseIocForFatClient]
	public class IntradayOptimizationCascadingDesktopTest : IntradayOptimizationScenarioTest
	{
		public OptimizationDesktopExecuter Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		public CascadingResourceCalculationContextFactory ResourceCalculationContextFactory;
		public IResourceCalculation ResourceCalculation;
		public ResourceCalculateWithNewContext ResourceCalculateWithNewContext;

		[Test]
		public void ShouldShovelResources()
		{
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

		[Test]
		public void ShouldKeepCalculatedLoggedOnValueWhenHavingOuterResContext()
		{
			var scenario = new Scenario("_");
			var activity = ActivityFactory.CreateActivity("_");
			var date = DateOnly.Today;
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36)), PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			var skillA = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 17);
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, date, 1);
			var skillB = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 17);
			var skillDayB = skillB.CreateSkillDayWithDemand(scenario, date, 1);
			var agentAB = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skillA, skillB).WithSchedulePeriodOneWeek(date);
			var agentB = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skillB).WithSchedulePeriodOneWeek(date);
			var assAB = new PersonAssignment(agentAB, scenario, date).WithLayer(activity, new TimePeriod(8, 17));
			var assB = new PersonAssignment(agentB, scenario, date).WithLayer(activity, new TimePeriod(8, 17));
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(date, date), new[] { agentAB, agentB }, new[] { assAB, assB }, new[] { skillDayA, skillDayB });

			using (ResourceCalculationContextFactory.Create(schedulerStateHolderFrom.SchedulingResultState, false, date.ToDateOnlyPeriod()))
			{
				var stateHolder = SchedulerStateHolderFrom();
				ResourceCalculation.ResourceCalculate(stateHolder.RequestedPeriod.DateOnlyPeriod, stateHolder.SchedulingResultState.ToResourceOptimizationData(stateHolder.ConsiderShortBreaks, false));

				Target.Execute(new NoSchedulingProgress(), stateHolder, new[] { agentAB, agentB }, date.ToDateOnlyPeriod(), new OptimizationPreferences { General = { ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = true } }, null);

				schedulerStateHolderFrom.SchedulingResultState.SkillDays[skillA].Single().SkillStaffPeriodCollection.First().CalculatedLoggedOn
					.Should().Be.EqualTo(1);
				schedulerStateHolderFrom.SchedulingResultState.SkillDays[skillB].Single().SkillStaffPeriodCollection.First().CalculatedLoggedOn
					.Should().Be.EqualTo(1);
			}
		}

		[Test]
		public void ShouldKeepCalculatedLoggedOnValueWhenHavingNoOuterResContext()
		{
			var scenario = new Scenario("_");
			var activity = ActivityFactory.CreateActivity("_");
			var date = DateOnly.Today;
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36)), PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			var skillA = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 17);
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, date, 1);
			var skillB = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 17);
			var skillDayB = skillB.CreateSkillDayWithDemand(scenario, date, 1);
			var agentAB = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skillA, skillB).WithSchedulePeriodOneWeek(date);
			var agentB = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skillB).WithSchedulePeriodOneWeek(date);
			var assAB = new PersonAssignment(agentAB, scenario, date).WithLayer(activity, new TimePeriod(8, 17));
			var assB = new PersonAssignment(agentB, scenario, date).WithLayer(activity, new TimePeriod(8, 17));
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(date, date), new[] { agentAB, agentB }, new[] { assAB, assB }, new[] { skillDayA, skillDayB });
			var stateHolder = SchedulerStateHolderFrom();
			ResourceCalculateWithNewContext.ResourceCalculate(stateHolder.RequestedPeriod.DateOnlyPeriod, stateHolder.SchedulingResultState.ToResourceOptimizationData(stateHolder.ConsiderShortBreaks, false));

			Target.Execute(new NoSchedulingProgress(), stateHolder, new[] { agentAB, agentB }, date.ToDateOnlyPeriod(), new OptimizationPreferences { General = { ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = true } }, null);

			schedulerStateHolderFrom.SchedulingResultState.SkillDays[skillA].Single().SkillStaffPeriodCollection.First().CalculatedLoggedOn
				.Should().Be.EqualTo(1);
			schedulerStateHolderFrom.SchedulingResultState.SkillDays[skillB].Single().SkillStaffPeriodCollection.First().CalculatedLoggedOn
				.Should().Be.EqualTo(1);
		}

		[Test]
		public void IncreasedResourcesShouldBeSameAsDecreasedInOtherSkills_WhenResourceCalcHasBeenMadeBeforeOptimization()
		{
			var scenario = new Scenario("_");
			var activity = ActivityFactory.CreateActivity("_");
			var date = DateOnly.Today;
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36)), PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			var skillA = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 17);
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, date, 1);
			var skillB = new Skill("B").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 17);
			var skillDayB = skillB.CreateSkillDayWithDemand(scenario, date, 1);
			var skillC = new Skill("C").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(3).IsOpenBetween(8, 17);
			var skillDayC = skillC.CreateSkillDayWithDemand(scenario, date, 1);
			var agentAB = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skillA,skillB).WithSchedulePeriodOneWeek(date);
			var agentAC = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skillA, skillC).WithSchedulePeriodOneWeek(date);
			var assAB = new PersonAssignment(agentAB, scenario, date).WithLayer(activity, new TimePeriod(8, 17));
			var assAC = new PersonAssignment(agentAC, scenario, date).WithLayer(activity, new TimePeriod(8, 17));
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(date, date), new[] { agentAB, agentAC }, new[] { assAB, assAC }, new[] { skillDayA, skillDayB, skillDayC });
			var stateHolder = SchedulerStateHolderFrom();
			ResourceCalculateWithNewContext.ResourceCalculate(stateHolder.RequestedPeriod.DateOnlyPeriod, stateHolder.SchedulingResultState.ToResourceOptimizationData(stateHolder.ConsiderShortBreaks, false));

			Target.Execute(new NoSchedulingProgress(), stateHolder, new[] { agentAB, agentAC }, date.ToDateOnlyPeriod(), new OptimizationPreferences { General = { ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = true } }, null);

			var skillASSkillStaffPeriod = schedulerStateHolderFrom.SchedulingResultState.SkillDays[skillA].Single().SkillStaffPeriodCollection.First();
			var skillBSSkillStaffPeriod = schedulerStateHolderFrom.SchedulingResultState.SkillDays[skillB].Single().SkillStaffPeriodCollection.First();
			var skillCSSkillStaffPeriod = schedulerStateHolderFrom.SchedulingResultState.SkillDays[skillC].Single().SkillStaffPeriodCollection.First();

			(skillASSkillStaffPeriod.CalculatedResource - skillASSkillStaffPeriod.CalculatedLoggedOn +
			(skillBSSkillStaffPeriod.CalculatedResource - skillBSSkillStaffPeriod.CalculatedLoggedOn) +
			(skillCSSkillStaffPeriod.CalculatedResource - skillCSSkillStaffPeriod.CalculatedLoggedOn))
				.IsZero().Should().Be.True();
		}

		[Test]
		public void ShouldCalculateIntraIntervalDeviationWhenStateHolderIsConsideringShortBreaks()
		{	
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var dateOnly = new DateOnly(2010, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 0, 8, 0, 5), new TimePeriodWithSegment(8, 5, 8, 5, 5), new ShiftCategory("_").WithId()));
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36)),
				PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9)
			};
			var skillA = new Skill().For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpen(new TimePeriod(8, 0, 8, 15)).DefaultResolution(15);
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skillA).WithSchedulePeriodOneWeek(dateOnly);
			var ass = new PersonAssignment(agent, scenario, dateOnly).WithLayer(phoneActivity, new TimePeriod(8, 0, 8, 5)).ShiftCategory(new ShiftCategory("_").WithId());
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent }, new[] { ass }, new[] { skillDayA });
			schedulerStateHolderFrom.ConsiderShortBreaks = true;

			Target.Execute(new NoSchedulingProgress(), schedulerStateHolderFrom, new[] { agent }, dateOnly.ToDateOnlyPeriod(), new OptimizationPreferences { General = { ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = true } }, null);

			schedulerStateHolderFrom.SchedulingResultState.SkillDays[skillA].Single().SkillStaffPeriodCollection.First().IntraIntervalDeviation
				.Should().Not.Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotCalculateIntraIntervalDeviationWhenStateHolderIsNotConsideringShortBreaks()
		{
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var dateOnly = new DateOnly(2010, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 0, 8, 0, 5), new TimePeriodWithSegment(8, 5, 8, 5, 5), new ShiftCategory("_").WithId()));
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36)),
				PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9)
			};
			var skillA = new Skill("_").For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpen(new TimePeriod(8, 0, 8, 15)).DefaultResolution(15);
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skillA).WithSchedulePeriodOneWeek(dateOnly);
			var ass = new PersonAssignment(agent, scenario, dateOnly).WithLayer(phoneActivity, new TimePeriod(8, 0, 8, 5)).ShiftCategory(new ShiftCategory("_").WithId());
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent }, new[] { ass }, new[] { skillDayA });
			schedulerStateHolderFrom.ConsiderShortBreaks = false;
			//TODO consider to check/set consider short breaks in one place only
			var optimizationPreferences = new OptimizationPreferences
			{
				General = {ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = true},
				Rescheduling = {ConsiderShortBreaks = false}
			};

			Target.Execute(new NoSchedulingProgress(), schedulerStateHolderFrom, new[] { agent }, dateOnly.ToDateOnlyPeriod(), optimizationPreferences, null);

			schedulerStateHolderFrom.SchedulingResultState.SkillDays[skillA].Single().SkillStaffPeriodCollection.First().IntraIntervalDeviation
				.Should().Be.EqualTo(0);
		}
	}
}