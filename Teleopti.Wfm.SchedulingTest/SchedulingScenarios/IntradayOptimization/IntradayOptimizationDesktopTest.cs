using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
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
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;


namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTest]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	[LoggedOnAppDomain]
	[UseIocForFatClient]
	public class IntradayOptimizationDesktopTest: IntradayOptimizationScenarioTest
	{
		public OptimizationDesktopExecuter Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		public ResourceCalculateWithNewContext ResourceCalculateWithNewContext;

		[Test]
		public void ShouldMoveShiftToTheIntervalWithHighestDemand()
		{
			var scenario = new Scenario("_");
			var activity = ActivityFactory.CreateActivity("_");
			var date = new DateOnly(2015, 10, 12);
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpenBetween(8, 19);
			skill.DefaultResolution = 30;
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, date, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(18, TimeSpan.FromMinutes(90)));
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(10, 0, 10, 0, 15), new TimePeriodWithSegment(19, 0, 19, 0, 15), shiftCategory));
			var ruleSet2 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), shiftCategory));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(date, new RuleSetBag(ruleSet, ruleSet2), new ContractWithMaximumTolerance(), ContractScheduleFactory.Create7DaysWorkingContractSchedule(), new PartTimePercentage("_"), new Team { Site = new Site("site") }, skill).WithSchedulePeriodOneDay(date);
			var agent2 = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(new ContractWithMaximumTolerance(), skill).WithSchedulePeriodOneWeek(date);
			var assignment = new PersonAssignment(agent, scenario, date).WithLayer(activity, new TimePeriod(8, 15, 17, 15));
			assignment.SetShiftCategory(shiftCategory);
			var assignment2 = new PersonAssignment(agent2, scenario, date).WithLayer(activity, new TimePeriod(8, 19));
			assignment2.SetShiftCategory(shiftCategory);
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(date, date), new[] { agent, agent2 }, new[] { assignment, assignment2 }, new[] { skillDay });
			var stateHolder = SchedulerStateHolderFrom();
			ResourceCalculateWithNewContext.ResourceCalculate(stateHolder.RequestedPeriod.DateOnlyPeriod, stateHolder.SchedulingResultState.ToResourceOptimizationData(stateHolder.ConsiderShortBreaks, false));

			Target.Execute(new NoSchedulingProgress(), stateHolder, new[] { agent }, date.ToDateOnlyPeriod(),
				new OptimizationPreferences { General = { ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = true } }, null);

			schedulerStateHolderFrom.Schedules.Values.Single(x => x.Person.Id.Equals(agent.Id))
				.ScheduledDayCollection(date.ToDateOnlyPeriod()).First().PersonAssignment().ShiftLayers.First().Period.StartDateTime
				.Hour.Should().Be.EqualTo(10);
		}
	}
}
