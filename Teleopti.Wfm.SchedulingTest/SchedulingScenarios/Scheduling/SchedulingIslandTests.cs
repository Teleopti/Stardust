using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Islands;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	public class SchedulingIslandTests : SchedulingScenario
	{
		public ReduceIslandsLimits ReduceIslandsLimits;
		public FullScheduling Target;
		public FakePersonRepository PersonRepository;
		public FakeSkillRepository SkillRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeDayOffTemplateRepository DayOffTemplateRepository;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public MergeIslandsSizeLimit MergeIslandsSizeLimit;

		[Test]
		public void ShouldNotUseSkillsThatWereRemovedDuringIslandCreation()
		{
			MergeIslandsSizeLimit.TurnOff_UseOnlyFromTest();
			ReduceIslandsLimits.SetValues_UseOnlyFromTest(0, 1);
			DayOffTemplateRepository.Has(DayOffFactory.CreateDayOff());
			var date = new DateOnly(2015, 10, 12);
			var scenario = ScenarioRepository.Has("some name");
			var activityAB = new Activity("AB").WithId();
			var activityC = new Activity("C").WithId();
			var skillA = SkillRepository.Has("skillA", activityAB);
			var skillB = SkillRepository.Has("skillB", activityAB);
			var skillC = SkillRepository.Has("skillC", activityC);
			var shiftCategory = new ShiftCategory("_").WithId();
			var rulesetA = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activityAB, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var rulesetB = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activityC, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var bag = new RuleSetBag(rulesetA, rulesetB);
			PersonRepository.Has(new SchedulePeriod(date, SchedulePeriodType.Day, 1), bag, skillA);
			var agentABC = PersonRepository.Has(new SchedulePeriod(date, SchedulePeriodType.Day, 1), bag, skillA, skillB, skillC); //will in island be "skilled" on B and C
			SkillDayRepository.Has(
				skillA.CreateSkillDayWithDemand(scenario, date, 100),
				skillB.CreateSkillDayWithDemand(scenario, date, 1),
				skillC.CreateSkillDayWithDemand(scenario, date, 10));
			var planningPeriod = PlanningPeriodRepository.Has(date,SchedulePeriodType.Day, 1);
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			PersonAssignmentRepository.GetSingle(date, agentABC).ShiftLayers.Single().Payload
				.Should().Be.EqualTo(activityC);
		}

		public SchedulingIslandTests(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}