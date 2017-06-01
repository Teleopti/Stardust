using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntraIntervalOptimization
{
	[DomainTest]
	public class IntraIntervalOptimizationTest : ISetup
	{
		public OptimizationExecuter Target;
		public Func<ISchedulerStateHolder> StateHolder;
		
		[Test]
		public void ShouldSolveIntraIntervalIssue()
		{
			var date = DateOnly.Today;
			var scenario = new Scenario("_");
			var activity = new Activity().WithId();
			var skill = new Skill().DefaultResolution(30).For(activity).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, date, 10);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), shiftCategory));
			var agent1 = new Person().WithPersonPeriod(ruleSet, skill).WithSchedulePeriodOneDay(date).WithId().InTimeZone(TimeZoneInfo.Utc);
			var agent2 = new Person().WithPersonPeriod(skill).WithId().InTimeZone(TimeZoneInfo.Utc);
			var agents = new[] {agent1, agent2};
			var ass1 = new PersonAssignment(agent1, scenario, date).WithLayer(activity, new TimePeriod(8, 15, 16, 15)).ShiftCategory(shiftCategory);
			var ass2 = new PersonAssignment(agent2, scenario, date).WithLayer(activity, new TimePeriod(8, 15, 16, 30)).ShiftCategory(shiftCategory);
			var stateHolder = StateHolder.Fill(scenario, date.ToDateOnlyPeriod(), agents, new[] { ass1, ass2 }, skillDay);
			var optimizationPreferences = new OptimizationPreferencesDefaultValueProvider().Fetch();
			optimizationPreferences.General.OptimizationStepIntraInterval = true;
			optimizationPreferences.Extra.TeamGroupPage = new GroupPageLight("_", GroupPageType.SingleAgent); //change default value instead
		
			Target.Execute(new NoSchedulingProgress(), stateHolder, new []{agent1}, date.ToDateOnlyPeriod(), optimizationPreferences, new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()));

			stateHolder.Schedules[agent1].ScheduledDay(date).PersonAssignment().Period.StartDateTime.TimeOfDay
				.Should().Be.EqualTo(TimeSpan.FromHours(8));
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<DesktopOptimizationContext>().For<IFillSchedulerStateHolder, ISynchronizeIntradayOptimizationResult, IOptimizationPreferencesProvider, IPeopleInOrganization>();
		}
	}
}