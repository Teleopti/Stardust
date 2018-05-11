using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.DayOffOptimization
{
	[DomainTest]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	public class DayOffOptimizationMinimumAgentsTest : DayOffOptimizationScenario
	{
		public DayOffOptimizationWeb Target;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeSkillRepository SkillRepository;
		public FakePersonRepository PersonRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeActivityRepository ActivityRepository;
		public FakePlanningPeriodRepository PlanningPeriodRepository;

		[Test]
		public void ShouldConsiderMinimumAgentsWhenMovingDayOffTo()
		{
			var date = new DateOnly(2015, 10, 12); //mon
			var activity = ActivityRepository.Has();
			var skill = SkillRepository.Has("skill", activity);
			var planningPeriod = PlanningPeriodRepository.Has(date, 1);
			var scenario = ScenarioRepository.Has();
			var schedulePeriod = new SchedulePeriod(date, SchedulePeriodType.Week, 1).NumberOfDaysOf(1);
			var shiftCategory = new ShiftCategory().WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(schedulePeriod, ruleSet, skill);
			var skillDays = SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date,
				0.8, //minimum staffing = 1, prevent putting DO here
				0.9, //DO should end up here
				1,
				1,
				1,
				1, //DO from beginning
				1)
			);
			skillDays.First().SetMinimumAgents(1);
			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.GetSingle(skillDays[5].CurrentDate).WithDayOff();

			Target.Execute(planningPeriod.Id.Value);

			PersonAssignmentRepository.LoadAll().Single(x => x.DayOff() != null).Date
				.Should().Be.EqualTo(skillDays[1].CurrentDate); //only DO is on tuesday
		}
		
		[Test]
		[Ignore("Test issue for #75339")]
		public void ShouldMoveMoreThanOneDoPerAgent()
		{
			var date = new DateOnly(2015, 10, 12); //mon
			var activity = ActivityRepository.Has();
			var skill = SkillRepository.Has("skill", activity);
			var planningPeriod = PlanningPeriodRepository.Has(date, 1);
			var scenario = ScenarioRepository.Has();
			var schedulePeriod = new SchedulePeriod(date, SchedulePeriodType.Week, 1).NumberOfDaysOf(2);
			var shiftCategory = new ShiftCategory().WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(schedulePeriod, ruleSet, skill);
			var skillDays = SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date,
				0.1, //minimum staffing = 1, prevent putting DO here
				0.1, //minimum staffing = 1, prevent putting DO here
				0.2,
				0.2,
				1,
				1, //DO from beginning
				1) //DO from beginning
			);
			skillDays[0].SetMinimumAgents(1);
			skillDays[1].SetMinimumAgents(1);
			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.GetSingle(skillDays[5].CurrentDate).WithDayOff();
			PersonAssignmentRepository.GetSingle(skillDays[6].CurrentDate).WithDayOff();

			Target.Execute(planningPeriod.Id.Value);

			PersonAssignmentRepository.LoadAll().Where(x => x.DayOff() != null).Select(x => x.Date)
				.Should().Have.SameValuesAs(skillDays[2].CurrentDate, skillDays[3].CurrentDate);
		}
		
		[Test]
		public void ShouldConsiderMinimumAgentsCorrectlyWhenAgentHasMultipleSkills()
		{
			var date = new DateOnly(2015, 10, 12); //mon
			var activity = ActivityRepository.Has();
			var skill = SkillRepository.Has("skill", activity);
			var extraSkill = SkillRepository.Has("extraSkill", activity);
			var planningPeriod = PlanningPeriodRepository.Has(date, 1);
			var scenario = ScenarioRepository.Has();
			var schedulePeriod = new SchedulePeriod(date, SchedulePeriodType.Week, 1).NumberOfDaysOf(1);
			var shiftCategory = new ShiftCategory().WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var skillDays = SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date,
				0.5, //DO should end up here
				1, 
				1,
				1,
				1,
				1, //DO from beginning
				1)
			);
			skillDays.First().SetMinimumAgents(1);
			SkillDayRepository.Has(extraSkill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), 1));
			var alreadyScheduledAgent = PersonRepository.Has(skill, extraSkill);
			PersonAssignmentRepository.Has(alreadyScheduledAgent, scenario, activity, shiftCategory, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new TimePeriod(0, 0, 24, 0));
			var agentToOptimize = PersonRepository.Has(schedulePeriod, ruleSet, skill);
			PersonAssignmentRepository.Has(agentToOptimize, scenario, activity, shiftCategory, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.GetSingle(skillDays[5].CurrentDate, agentToOptimize).WithDayOff();

			Target.Execute(planningPeriod.Id.Value);

			PersonAssignmentRepository.LoadAll().Single(x => x.DayOff() != null).Date
				.Should().Be.EqualTo(skillDays[0].CurrentDate); //only DO is on Monday
		}
		
		[Test]
		public void ShouldConsiderMultipleMinimumAgentsWhenMovingDayOffTo()
		{
			var date = new DateOnly(2015, 10, 12); //mon
			var activity = ActivityRepository.Has();
			var skill = SkillRepository.Has("skill", activity);
			var planningPeriod = PlanningPeriodRepository.Has(date, 1);
			var scenario = ScenarioRepository.Has();
			var schedulePeriod = new SchedulePeriod(date, SchedulePeriodType.Week, 1).NumberOfDaysOf(1);
			var shiftCategory = new ShiftCategory().WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(schedulePeriod, ruleSet, skill);
			var skillDays = SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date,
					1, //minimum staffing = 3,
					1, //minimum staffing = 3,
					1, //minimum staffing = 3
					1, //minimum staffing = 3 //DO in the beginning
					1, //minimum staffing = 1 //DO should end up here
					1, //minimum staffing = 3 
					1 //minimum staffing = 3 
					).SetMinimumAgents(3)
			);
			skillDays[4].SetMinimumAgents(1);
			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.GetSingle(skillDays[3].CurrentDate).WithDayOff();

			Target.Execute(planningPeriod.Id.Value);

			PersonAssignmentRepository.LoadAll().Single(x => x.DayOff() != null).Date
				.Should().Be.EqualTo(skillDays[4].CurrentDate);
		}


		[Test]
		public void ShouldNotConsiderMinimumAgentsIfFulfilled()
		{
			var date = new DateOnly(2015, 10, 12); //mon
			var activity = ActivityRepository.Has();
			var skill = SkillRepository.Has("skill", activity);
			var planningPeriod = PlanningPeriodRepository.Has(date, 1);
			var scenario = ScenarioRepository.Has();
			var schedulePeriod = new SchedulePeriod(date, SchedulePeriodType.Week, 1).NumberOfDaysOf(1);
			var shiftCategory = new ShiftCategory().WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var skillDays = SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date,
				1.8, //DO should end up here (demand left to fulfil = 1.8 - 1 = 0.8)
				0.9, 
				1,
				1,
				1,
				1, //DO from beginning
				1)
			);
			skillDays.First().SetMinimumAgents(1);
			var alreadyScheduledAgent = PersonRepository.Has(skill);
			PersonAssignmentRepository.Has(alreadyScheduledAgent, scenario, activity, shiftCategory, date, new TimePeriod(0, 0, 24, 0));
			var agentToOptimize = PersonRepository.Has(schedulePeriod, ruleSet, skill);
			PersonAssignmentRepository.Has(agentToOptimize, scenario, activity, shiftCategory, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.GetSingle(skillDays[5].CurrentDate).WithDayOff();

			Target.Execute(planningPeriod.Id.Value);

			PersonAssignmentRepository.LoadAll().Single(x => x.DayOff() != null).Date
				.Should().Be.EqualTo(skillDays[0].CurrentDate); //only DO is on Monday
		}
		
		[Test] 
		public void ShouldConsiderMinimumAgentsWhenMovingDayOffFrom()
		{
			var date = new DateOnly(2015, 10, 12); //mon
			var activity = ActivityRepository.Has();
			var skill = SkillRepository.Has("skill", activity);
			var planningPeriod = PlanningPeriodRepository.Has(date, 1);
			var scenario = ScenarioRepository.Has();
			var schedulePeriod = new SchedulePeriod(date, SchedulePeriodType.Week, 1).NumberOfDaysOf(1);
			var shiftCategory = new ShiftCategory().WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(schedulePeriod, ruleSet, skill);
			var skillDays = SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date,
				1,
				1,
				1,
				1,
				1,
				1,
				0.1) //DO from beginning, minimum staffing 1 so we should move DO from this spot
			);
			skillDays.Last().SetMinimumAgents(1);
			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.GetSingle(skillDays[6].CurrentDate).WithDayOff();

			Target.Execute(planningPeriod.Id.Value);
			
			PersonAssignmentRepository.LoadAll().Single(x => x.DayOff() != null).Date
				.Should().Not.Be.EqualTo(skillDays[6].CurrentDate); //should have been moved
		}
		
		
		public DayOffOptimizationMinimumAgentsTest(SeperateWebRequest seperateWebRequest, bool resourcePlannerDayOffOptimizationIslands47208, bool resourcePlannerMinimumAgents75339, bool resourcePlannerLessResourcesXXL74915) : base(seperateWebRequest, resourcePlannerDayOffOptimizationIslands47208, resourcePlannerMinimumAgents75339, resourcePlannerLessResourcesXXL74915)
		{
			if(!ResourcePlannerMinimumAgents75339)
				Assert.Ignore("Only available when ResourcePlanner_MinimumAgents_75339 is true");
		}
	}
}