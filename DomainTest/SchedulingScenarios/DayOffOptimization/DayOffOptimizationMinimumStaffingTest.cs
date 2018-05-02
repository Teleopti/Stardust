using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
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
	public class DayOffOptimizationMinimumStaffingTest : DayOffOptimizationScenario
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
		public void ShouldConsiderMinimumStaffingWhenMovingDayOffTo()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var planningPeriod = PlanningPeriodRepository.Has(firstDay, 1);
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1);
			schedulePeriod.SetDaysOff(1);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(new Team {Site = new Site("site")}, schedulePeriod, ruleSet, skill);
			var skillDays = SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				0.8, //minimum staffing = 1, prevent putting DO here
				0.9, //DO should end up here
				1,
				1,
				1,
				1, //DO from beginning
				1)
			);
			skillDays.First().SetMinimumAgents(1);
			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory, new DateOnlyPeriod(firstDay, firstDay.AddDays(7)), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.GetSingle(skillDays[5].CurrentDate).SetDayOff(new DayOffTemplate());

			Target.Execute(planningPeriod.Id.Value);

			PersonAssignmentRepository.LoadAll().Single(x => x.DayOff() != null).Date
				.Should().Be.EqualTo(skillDays[1].CurrentDate); //only DO is on tuesday
		}
		
		[Test]
		[Ignore("2 be fixed")]
		public void ShouldConsiderMultipleMinimumStaffingsWhenMovingDayOffTo()
		{
			var date = new DateOnly(2015, 10, 12); //mon
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var planningPeriod = PlanningPeriodRepository.Has(date, 1);
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(date, SchedulePeriodType.Week, 1);
			schedulePeriod.SetDaysOff(1);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(new Team {Site = new Site("site")}, schedulePeriod, ruleSet, skill);
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
			PersonAssignmentRepository.GetSingle(skillDays[3].CurrentDate).SetDayOff(new DayOffTemplate());

			Target.Execute(planningPeriod.Id.Value);

			PersonAssignmentRepository.LoadAll().Single(x => x.DayOff() != null).Date
				.Should().Be.EqualTo(skillDays[4].CurrentDate);
		}


		[Test]
		public void ShouldNotConsiderMinimumStaffingIfFulfilled()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var planningPeriod = PlanningPeriodRepository.Has(firstDay, 1);
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1);
			schedulePeriod.SetDaysOff(1);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var skillDays = SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				1.8, //DO should end up here
				0.9, 
				1,
				1,
				1,
				1, //DO from beginning
				1)
			);
			skillDays.First().SetMinimumAgents(1);
			var alreadyScheduledAgent = PersonRepository.Has(skill);
			PersonAssignmentRepository.Has(alreadyScheduledAgent, scenario, activity, shiftCategory, firstDay, new TimePeriod(0, 0, 24, 0));
			var agentToOptimize = PersonRepository.Has(new Team {Site = new Site("site")}, schedulePeriod, ruleSet, skill);
			PersonAssignmentRepository.Has(agentToOptimize, scenario, activity, shiftCategory, new DateOnlyPeriod(firstDay, firstDay.AddDays(7)), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.GetSingle(skillDays[5].CurrentDate).SetDayOff(new DayOffTemplate());

			Target.Execute(planningPeriod.Id.Value);

			PersonAssignmentRepository.LoadAll().Single(x => x.DayOff() != null).Date
				.Should().Be.EqualTo(skillDays[0].CurrentDate); //only DO is on tuesday
		}
		
		[Test] 
		public void ShouldConsiderMinimumStaffingWhenMovingDayOffFrom()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var planningPeriod = PlanningPeriodRepository.Has(firstDay, 1);
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1);
			schedulePeriod.SetDaysOff(1);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(new Team {Site = new Site("site")}, schedulePeriod, ruleSet, skill);
			var skillDays = SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				1,
				1,
				1,
				1,
				1,
				1,
				0.1) //DO from beginning, minimum staffing 1 so we should move DO from this spot
			);
			skillDays.Last().SetMinimumAgents(1);
			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory, new DateOnlyPeriod(firstDay, firstDay.AddDays(7)), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.GetSingle(skillDays[6].CurrentDate).SetDayOff(new DayOffTemplate());

			Target.Execute(planningPeriod.Id.Value);
			
			PersonAssignmentRepository.LoadAll().Single(x => x.DayOff() != null).Date
				.Should().Not.Be.EqualTo(skillDays[6].CurrentDate); //should have been moved
		}
		
		
		public DayOffOptimizationMinimumStaffingTest(SeperateWebRequest seperateWebRequest, bool resourcePlannerDayOffOptimizationIslands47208, bool resourcePlannerMinimumStaffing75339) : base(seperateWebRequest, resourcePlannerDayOffOptimizationIslands47208, resourcePlannerMinimumStaffing75339)
		{
			if(!_resourcePlannerMinimumStaffing75339)
				Assert.Ignore("Only available when ResourcePlanner_MinimumStaffing_75339 is true");
		}
	}
}