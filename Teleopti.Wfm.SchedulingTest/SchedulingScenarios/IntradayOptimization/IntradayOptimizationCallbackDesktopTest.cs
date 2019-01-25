using System;
using System.Collections.Generic;
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


namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTest]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	[LoggedOnAppDomain]
	[UseIocForFatClient]
	public class IntradayOptimizationCallbackDesktopTest : IntradayOptimizationScenarioTest
	{
		public OptimizeIntradayDesktop Target; //should be OptimizationDesktopExecuter but there is no way to inject TrackIntradayOptimizationCallback from there currently. fix if problematic in the future!
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		public CascadingResourceCalculationContextFactory ResourceCalculationContextFactory; //should not be needed if using OptimizationDesktopExecuter instead

		[Test]
		public void ShouldDoSuccesfulCallbacks()
		{
			const int numberOfAgents = 10;
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var dateOnly = new DateOnly(2010, 1, 1);
			var skill = new Skill("_").For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(9, 0, 9, 0, 60), new TimePeriodWithSegment(17, 0, 17, 0, 60), new ShiftCategory("new").WithId()));
			var asses = new List<IPersonAssignment>();
			for (var i = 0; i < numberOfAgents; i++)
			{
				var agent = new Person().WithId().WithPersonPeriod(ruleSet, new ContractWithMaximumTolerance(), skill).WithSchedulePeriodOneWeek(dateOnly);
				asses.Add(new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(phoneActivity, new TimePeriod(8, 17)));
			}
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly, asses.Select(x => x.Person), asses, skillDay);

			using (ResourceCalculationContextFactory.Create(stateHolder.SchedulingResultState, false, dateOnly.ToDateOnlyPeriod()))
			{
				var callbackTracker = new TrackOptimizationCallback();
				Target.Optimize(asses.Select(x => x.Person), new DateOnlyPeriod(dateOnly, dateOnly), new OptimizationPreferences{General = {ScheduleTag = NullScheduleTag.Instance }}, callbackTracker);
				callbackTracker.SuccessfulOptimizations().Should().Be.EqualTo(10);
			}
		}

		[Test]
		public void ShouldDoUnsuccesfulCallbacks()
		{
			const int numberOfAgents = 10;
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var dateOnly = new DateOnly(2010, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(9, 0, 9, 0, 60), new TimePeriodWithSegment(17, 0, 17, 0, 60), new ShiftCategory("new").WithId()));
			var asses = new List<IPersonAssignment>();
			var skill = new Skill("_").For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId();
			for (var i = 0; i < numberOfAgents; i++)
			{
				var agent = new Person().WithId().WithPersonPeriod(ruleSet, new ContractWithMaximumTolerance(), skill).WithSchedulePeriodOneWeek(dateOnly);
				asses.Add(new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(phoneActivity, new TimePeriod(8, 17)));
			}
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), asses.Select(x => x.Person), asses, new List<ISkillDay>());

			using (ResourceCalculationContextFactory.Create(stateHolder.SchedulingResultState, false, dateOnly.ToDateOnlyPeriod()))
			{
				var callbackTracker = new TrackOptimizationCallback();
				Target.Optimize(asses.Select(x => x.Person), new DateOnlyPeriod(dateOnly, dateOnly), new OptimizationPreferences{General = {ScheduleTag = NullScheduleTag.Instance }}, callbackTracker);
				callbackTracker.UnSuccessfulOptimizations().Should().Be.EqualTo(10);
			}
		}
	}
}