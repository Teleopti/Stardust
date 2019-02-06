using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Islands;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTest]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	[LoggedOnAppDomain]
	public class IntradayOptimizationIncreaseIslandsTest : IntradayOptimizationScenarioTest
	{
		public IntradayOptimizationFromWeb Target;
		public FakeSkillRepository SkillRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonRepository PersonRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public ReduceIslandsLimits ReduceIslandsLimits;


		[Test]
		public void ShouldNotUseSkillsThatWereRemovedDuringIslandCreation()
		{
			ReduceIslandsLimits.SetValues_UseOnlyFromTest(0, 1);
			var date = new DateOnly(2015, 10, 12);
			var scenario = ScenarioRepository.LoadDefaultScenario();
			var activityAB = new Activity("AB").WithId();
			var activityC = new Activity("C").WithId();
			var skillA = SkillRepository.Has("skillA", activityAB);
			var skillB = SkillRepository.Has("skillB", activityAB);
			var skillC = SkillRepository.Has("skillC", activityC);
			var schedulePeriod = new SchedulePeriod(date.AddWeeks(-1), SchedulePeriodType.Day, 1);
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
			var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSetAB = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activityAB, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var ruleSetC = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activityC, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var ruleSetBag = new RuleSetBag(ruleSetAB);
			ruleSetBag.AddRuleSet(ruleSetC);
			var agentA = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, ruleSetBag, skillA);
			var agentABC = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, ruleSetBag, skillA, skillB, skillC);
			var planningPeriod = PlanningPeriodRepository.Has(date, 1);
			PersonAssignmentRepository.Has(agentA, scenario, activityAB, shiftCategory, date, new TimePeriod(8, 15, 16, 15));
			PersonAssignmentRepository.Has(agentABC, scenario, activityAB, shiftCategory, date, new TimePeriod(8, 15, 16, 15));
			SkillDayRepository.Has(
				skillA.CreateSkillDayWithDemand(scenario, date, 100),
				skillB.CreateSkillDayWithDemand(scenario, date, 1),
				skillC.CreateSkillDayWithDemand(scenario, date, 10));

			Target.Execute(planningPeriod.Id.Value);

			var shiftLayer = PersonAssignmentRepository.GetSingle(date, agentABC).ShiftLayers.Single();
			shiftLayer.Period.StartDateTime.TimeOfDay.Should().Be.EqualTo(TimeSpan.FromHours(8));
			shiftLayer.Payload.Should().Be.EqualTo(activityC);
		}
	}
}