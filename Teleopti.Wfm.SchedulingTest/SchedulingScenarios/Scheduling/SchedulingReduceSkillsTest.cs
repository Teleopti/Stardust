using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
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
	public class SchedulingReduceSkillsTest : SchedulingScenario
	{
		public FullScheduling Target;
		public FakePersonRepository PersonRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillRepository SkillRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeDayOffTemplateRepository DayOffTemplateRepository;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public ReduceIslandsLimits ReduceIslandsLimits;
		public MergeIslandsSizeLimit MergeIslandsSizeLimit;
		public FakePlanningGroupRepository PlanningGroupRepository;

		[TestCase(0, 12, 12, 24, true)]
		[TestCase(8, 16, 8, 16, true)]
		[TestCase(0, 12, 13, 24, false)]
		[TestCase(0, 12, 0, 12, false)]
		public void ShouldHandleDifferentOpenHoursWhenReducingSkills(int skill1Start, int skill1End, int skill2Start, int skill2End, bool canBeScheduled)
		{
			MergeIslandsSizeLimit.TurnOff_UseOnlyFromTest();
			ReduceIslandsLimits.SetValues_UseOnlyFromTest(0, 1);
			DayOffTemplateRepository.Has(DayOffFactory.CreateDayOff());
			var date = new DateOnly(2015, 10, 12);
			var activity = ActivityRepository.Has();
			var skill1 = new Skill("skill1").For(activity).IsOpenBetween(skill1Start, skill1End).WithId();
			var skill2 = new Skill("skill2").For(activity).IsOpenBetween(skill2Start, skill2End).WithId();
			SkillRepository.Has(skill1, skill2); 
			var scenario = ScenarioRepository.Has();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory("_").WithId()));
			var agentToSchedule = PersonRepository.Has(new SchedulePeriod(date, SchedulePeriodType.Day, 1), ruleSet, skill1, skill2);
			PersonRepository.Has(skill1);
			SkillDayRepository.Has(
				skill1.CreateSkillDayWithDemand(scenario, date, 100), 
				skill2.CreateSkillDayWithDemand(scenario, date, 100)
				);
			PersonAssignmentRepository.Has(new PersonAssignment(agentToSchedule, scenario, date)); //just to make easier assert
			var planningPeriod = PlanningPeriodRepository.Has(date, 1, SchedulePeriodType.Day, PlanningGroupRepository.Has());
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);
			
			PersonAssignmentRepository.LoadAll().Single(x => x.Person.Equals(agentToSchedule) && x.Date == date).ShiftLayers.Any()
				.Should().Be.EqualTo(canBeScheduled);
		}

		[Test]
		public void ShouldNotCauseThreadingIssues()
		{
			const int numberOfReducedSkillsAndIslands = 50; 
			MergeIslandsSizeLimit.TurnOff_UseOnlyFromTest();
			ReduceIslandsLimits.SetValues_UseOnlyFromTest(0, 1);
			DayOffTemplateRepository.Has(DayOffFactory.CreateDayOff());
			var date = new DateOnly(2015, 10, 12);
			var activity = ActivityRepository.Has();
			var scenario = ScenarioRepository.Has();
			var superSkillThatEveryOneKnows = new Skill("super skill").For(activity).IsOpen().WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory().WithId()));
			SkillDayRepository.Has(superSkillThatEveryOneKnows.CreateSkillDayWithDemand(scenario, date, 1000));
			for (var i = 0; i < numberOfReducedSkillsAndIslands; i++)
			{
				var extraSkill = new Skill("extra skill" + i).For(activity).IsOpen().WithId();
				SkillDayRepository.Has(extraSkill.CreateSkillDayWithDemand(scenario, date, 1000));
				PersonRepository.Has(new SchedulePeriod(date, SchedulePeriodType.Day, 1), ruleSet, superSkillThatEveryOneKnows, extraSkill);
			}
			var planningPeriod = PlanningPeriodRepository.Has(date, 1, SchedulePeriodType.Day, PlanningGroupRepository.Has());
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);
			
			PersonAssignmentRepository.LoadAll().Where(x => x.Date == date).All(ass => ass.ShiftLayers.Any())
				.Should().Be.True();
		}
		
		public SchedulingReduceSkillsTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}