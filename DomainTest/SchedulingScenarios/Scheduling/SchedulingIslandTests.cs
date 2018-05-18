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
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	public class SchedulingIslandTests : SchedulingScenario
	{
		public ReduceIslandsLimits ReduceIslandsLimits;
		public FullScheduling Target;
		public FakePersonRepository PersonRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillRepository SkillRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeDayOffTemplateRepository DayOffTemplateRepository;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public MergeIslandsSizeLimit MergeIslandsSizeLimit;

		
		[Test]
		[Ignore("Delete me - just a temp test for 74915")]
		public void ForKittensToUseWhenLabbing()
		{
			MergeIslandsSizeLimit.TurnOff_UseOnlyFromTest();
			DayOffTemplateRepository.Has(DayOffFactory.CreateDayOff());
			var firstDay = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 4);
			var activity = ActivityRepository.Has("_");
			var scenario = ScenarioRepository.Has("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSetWithLoadsOfShifts = 
				new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(0, 0, 24, 0, 1), new TimePeriodWithSegment(0, 0, 24, 0, 1), shiftCategory));
			
			const int numberOfIslands = 50;
			const int numberOfAgentsPerIsland = 1;
			for (var i = 0; i < numberOfIslands; i++)
			{
				var skill = SkillRepository.Has("skill", activity);
				SkillDayRepository.Has(skill.CreateSkillDayWithDemandOnInterval(scenario, firstDay, 111));
				for (int j = 0; j < numberOfAgentsPerIsland; j++)
				{
					PersonRepository.Has(new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSetWithLoadsOfShifts, skill);	
				}
			}
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate, period.EndDate, SchedulePeriodType.Week, 4);
			
			Target.DoScheduling(planningPeriod.Id.Value);
		}
		
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
			var planningPeriod = PlanningPeriodRepository.Has(date,date,SchedulePeriodType.Day, 1);
			
			Target.DoScheduling(planningPeriod.Id.Value);

			PersonAssignmentRepository.GetSingle(date, agentABC).ShiftLayers.Single().Payload
				.Should().Be.EqualTo(activityC);
		}

		public SchedulingIslandTests(SeperateWebRequest seperateWebRequest, bool resourcePlannerLessResourcesXXL74915) : base(seperateWebRequest, resourcePlannerLessResourcesXXL74915)
		{
		}
	}
}