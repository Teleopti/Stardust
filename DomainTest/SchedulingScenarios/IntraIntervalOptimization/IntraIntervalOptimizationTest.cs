using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
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
		public IResourceCalculation ResourceCalculation; //Should (probably) not be here...

		[Test]
		[Ignore("To be continued... 44551")]
		public void Poo()
		{
			// köra både classic/teamblock-toggle?

			var date = DateOnly.Today;
			var scenario = new Scenario("_");
			var activity = new Activity().WithId();
			var skill = new Skill().DefaultResolution(30).For(activity).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, date, 2);
			var shiftCategory = new ShiftCategory("_").WithId();
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), TimeSpan.FromHours(1), TimeSpan.FromHours(16)) };

			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), shiftCategory));
			var agent1 = new Person().WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneDay(date).WithId();
			var agent2 = new Person().WithPersonPeriod(skill).WithSchedulePeriodOneDay(date).WithId();
			var agents = new[] {agent1, agent2};
//			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(new Activity("base"), new TimePeriodWithSegment(8, 0, 8, 0, 0), new TimePeriodWithSegment(16, 0, 16, 0, 0), new ShiftCategory("_")));

			var ass1 = new PersonAssignment(agent1, scenario, date).WithLayer(activity, new TimePeriod(8, 15, 16, 15)).ShiftCategory(shiftCategory);
			var ass2 = new PersonAssignment(agent2, scenario, date).WithLayer(activity, new TimePeriod(8, 15, 16, 30)).ShiftCategory(shiftCategory);
			
			var stateHolder = StateHolder.Fill(scenario, date.ToDateOnlyPeriod(),
				agents,
				new[] { ass1, ass2 },
				skillDay);
			var optimizationPreferences = new OptimizationPreferencesDefaultValueProvider().Fetch();
			optimizationPreferences.General.OptimizationStepIntraInterval = true;
			optimizationPreferences.Extra.TeamGroupPage = new GroupPageLight("_", GroupPageType.SingleAgent); //change default value instead

			ResourceCalculation.ResourceCalculate(date, new ResourceCalculationData(stateHolder.SchedulingResultState, true, true));
			Target.Execute(new NoSchedulingProgress(), stateHolder, new []{agent1}, date.ToDateOnlyPeriod(), optimizationPreferences, new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()));

			var foo = stateHolder.Schedules;
			Console.WriteLine(foo[agent1].ScheduledDay(date).PersonAssignment().Period);
			Console.WriteLine(foo[agent2].ScheduledDay(date).PersonAssignment().Period);

		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<DesktopOptimizationContext>().For<IFillSchedulerStateHolder, ISynchronizeIntradayOptimizationResult, IOptimizationPreferencesProvider, IPeopleInOrganization>();
		}
	}
}