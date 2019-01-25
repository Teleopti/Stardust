using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios.DayOffOptimization
{
	[DomainTest]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	public class DayOffOptimizationMinimumAgentsTest : DayOffOptimizationScenario
	{
		public FullScheduling Target;
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
			var schedulePeriod = new SchedulePeriod(date, SchedulePeriodType.Week, 1).NumberOfDaysOff(1);
			var shiftCategory = new ShiftCategory().WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(schedulePeriod, ruleSet, skill);
			var alreadyScheduledAgent = PersonRepository.Has(skill);
			var skillDays = SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date,
				1.8, //minimum staffing = 1, prevent putting DO here
				1.9, //DO should end up here
				2,
				2,
				2,
				2, //DO from beginning
				2)
			);
			skillDays.First().SetMinimumAgents(new TimePeriod(8, 16), 2);
			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.GetSingle(skillDays[5].CurrentDate).WithDayOff();
			PersonAssignmentRepository.Has(alreadyScheduledAgent, scenario, activity, shiftCategory, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new TimePeriod(8, 0, 16, 0));

			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			PersonAssignmentRepository.LoadAll().Single(x => x.DayOff() != null).Date
				.Should().Be.EqualTo(skillDays[1].CurrentDate); //only DO is on tuesday
		}
		
		[Test]
		public void ShouldConsiderMinimumAgentsOnPartOfIntervals()
		{
			var date = new DateOnly(2015, 10, 12); //mon
			var activity = ActivityRepository.Has();
			var skill = SkillRepository.Has("skill", activity).DefaultResolution(60);
			var planningPeriod = PlanningPeriodRepository.Has(date, 1);
			var scenario = ScenarioRepository.Has();
			var schedulePeriod = new SchedulePeriod(date, SchedulePeriodType.Week, 1).NumberOfDaysOff(1);
			var shiftCategory = new ShiftCategory().WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(schedulePeriod, ruleSet, skill);
			var alreadyScheduledAgent = PersonRepository.Has(skill);
			var partlyScheduledAgent = PersonRepository.Has(skill);
			var skillDays = SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date,
				1,
				2.5,
				3,
				3,
				3,
				3,
				3)
			);
			skillDays[0].SetMinimumAgents(new TimePeriod(8, 9), 2);
			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.GetSingle(date.AddDays(6)).WithDayOff();
			PersonAssignmentRepository.Has(alreadyScheduledAgent, scenario, activity, shiftCategory, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.Has(partlyScheduledAgent, scenario, activity, shiftCategory, date.AddDays(0), new TimePeriod(8, 0, 8, 30)); //is not enough, only half interval

			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			PersonAssignmentRepository.LoadAll().Single(x => x.DayOff() != null).Date
				.Should().Be.EqualTo(date.AddDays(1)); 
		}
		
		[Test]
		public void ShouldMoveMoreThanOneDoPerAgent()
		{
			var date = new DateOnly(2015, 10, 12); //mon
			var activity = ActivityRepository.Has();
			var skill = SkillRepository.Has("skill", activity);
			var planningPeriod = PlanningPeriodRepository.Has(date, 1);
			var scenario = ScenarioRepository.Has();
			var schedulePeriod = new SchedulePeriod(date, SchedulePeriodType.Week, 1).NumberOfDaysOff(2);
			var shiftCategory = new ShiftCategory().WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(schedulePeriod, ruleSet, skill);
			var alreadyScheduledAgent = PersonRepository.Has(skill);
			var skillDays = SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date,
				1.1, //minimum staffing = 1, prevent putting DO here
				1.1, //minimum staffing = 1, prevent putting DO here
				1.2,
				1.2,
				2,
				2, //DO from beginning
				2) //DO from beginning
			);
			skillDays[0].SetMinimumAgents(new TimePeriod(8, 16), 2);
			skillDays[1].SetMinimumAgents(new TimePeriod(8, 16), 2);
			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.GetSingle(skillDays[5].CurrentDate).WithDayOff();
			PersonAssignmentRepository.GetSingle(skillDays[6].CurrentDate).WithDayOff();
			PersonAssignmentRepository.Has(alreadyScheduledAgent, scenario, activity, shiftCategory, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new TimePeriod(8, 0, 16, 0));

			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			PersonAssignmentRepository.LoadAll().Where(x => x.DayOff() != null).Select(x => x.Date)
				.Should().Have.SameValuesAs(skillDays[2].CurrentDate, skillDays[3].CurrentDate);
		}
		
		[Test]
		public void ShouldMoveMoreThanOneDoWhenMultipleAgents()
		{
			var date = new DateOnly(2015, 10, 12); //mon
			var activity = ActivityRepository.Has();
			var skill = SkillRepository.Has("skill", activity);
			var planningPeriod = PlanningPeriodRepository.Has(date, 1);
			var scenario = ScenarioRepository.Has();
			var schedulePeriod = new SchedulePeriod(date, SchedulePeriodType.Week, 1).NumberOfDaysOff(2);
			var shiftCategory = new ShiftCategory().WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent1 = PersonRepository.Has(schedulePeriod, ruleSet, skill);
			var agent2 = PersonRepository.Has(schedulePeriod, ruleSet, skill);
			var skillDays = SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), 1))
				.SetMinimumAgents(new TimePeriod(8, 16), 1);
			PersonAssignmentRepository.Has(agent1, scenario, activity, shiftCategory, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new TimePeriod(0, 0, 24, 0));
			PersonAssignmentRepository.GetSingle(skillDays[5].CurrentDate, agent1).WithDayOff();
			PersonAssignmentRepository.GetSingle(skillDays[6].CurrentDate, agent1).WithDayOff();
			PersonAssignmentRepository.Has(agent2, scenario, activity, shiftCategory, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new TimePeriod(0, 0, 24, 0));
			PersonAssignmentRepository.GetSingle(skillDays[5].CurrentDate, agent2).WithDayOff();
			PersonAssignmentRepository.GetSingle(skillDays[6].CurrentDate, agent2).WithDayOff();

			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			//max one DO per day
			var doDates = PersonAssignmentRepository.LoadAll().Where(x => x.DayOff() != null).Select(x => x.Date);
			doDates.Count()
				.Should().Be.EqualTo(doDates.Distinct().Count());
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
			var schedulePeriod = new SchedulePeriod(date, SchedulePeriodType.Week, 1).NumberOfDaysOff(1);
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
			skillDays.First().SetMinimumAgents(new TimePeriod(8, 16), 1);
			SkillDayRepository.Has(extraSkill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), 1));
			var alreadyScheduledAgent = PersonRepository.Has(skill, extraSkill);
			PersonAssignmentRepository.Has(alreadyScheduledAgent, scenario, activity, shiftCategory, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new TimePeriod(0, 0, 24, 0));
			var agentToOptimize = PersonRepository.Has(schedulePeriod, ruleSet, skill);
			PersonAssignmentRepository.Has(agentToOptimize, scenario, activity, shiftCategory, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.GetSingle(skillDays[5].CurrentDate, agentToOptimize).WithDayOff();

			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

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
			var schedulePeriod = new SchedulePeriod(date, SchedulePeriodType.Week, 1).NumberOfDaysOff(1);
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
					).SetMinimumAgents(new TimePeriod(8, 16), 3)
			);
			skillDays[4].SetMinimumAgents(new TimePeriod(8, 16), 1);
			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.GetSingle(skillDays[3].CurrentDate).WithDayOff();

			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

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
			var schedulePeriod = new SchedulePeriod(date, SchedulePeriodType.Week, 1).NumberOfDaysOff(1);
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
			skillDays.First().SetMinimumAgents(new TimePeriod(8, 16), 1);
			var alreadyScheduledAgent = PersonRepository.Has(skill);
			PersonAssignmentRepository.Has(alreadyScheduledAgent, scenario, activity, shiftCategory, date, new TimePeriod(0, 0, 24, 0));
			var agentToOptimize = PersonRepository.Has(schedulePeriod, ruleSet, skill);
			PersonAssignmentRepository.Has(agentToOptimize, scenario, activity, shiftCategory, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.GetSingle(skillDays[5].CurrentDate).WithDayOff();

			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

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
			var schedulePeriod = new SchedulePeriod(date, SchedulePeriodType.Week, 1).NumberOfDaysOff(1);
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
			skillDays.Last().SetMinimumAgents(new TimePeriod(8, 16), 1);
			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.GetSingle(skillDays[6].CurrentDate).WithDayOff();

			Target.DoSchedulingAndDO(planningPeriod.Id.Value);
			
			PersonAssignmentRepository.LoadAll().Single(x => x.DayOff() != null).Date
				.Should().Not.Be.EqualTo(skillDays[6].CurrentDate); //should have been moved
		}
		
		[Test]
		public void ShouldMoveDayOffFromDayLackingMinimumAgentsEvenThoughDemandIsLow()
		{
			var date = new DateOnly(2015, 10, 12);
			var activity = ActivityRepository.Has();
			var skill = SkillRepository.Has("skill", activity);
			var planningPeriod = PlanningPeriodRepository.Has(date, 1);
			var scenario = ScenarioRepository.Has();
			var schedulePeriod = new SchedulePeriod(date, SchedulePeriodType.Week, 1).NumberOfDaysOff(1);
			var shiftCategory = new ShiftCategory().WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity,
				new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), shiftCategory));
			var agent = PersonRepository.Has(schedulePeriod, ruleSet, skill);
			var alreadyScheduledAgent = PersonRepository.Has(schedulePeriod, ruleSet, skill);
			var skillDays = skill.CreateSkillDayWithDemandOnInterval(scenario, new DateOnlyPeriod(date, date.AddDays(5)), 10).ToList();
			skillDays.Add(skill.CreateSkillDayWithDemandOnInterval(scenario, date.AddDays(6), 1).SetMinimumAgents(new TimePeriod(8, 16), 2));
			SkillDayRepository.Has(skillDays);
			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.Has(alreadyScheduledAgent, scenario, activity, shiftCategory, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.GetSingle(skillDays[6].CurrentDate, agent).WithDayOff();

			Target.DoSchedulingAndDO(planningPeriod.Id.Value);
			
			PersonAssignmentRepository.LoadAll().Single(x => x.DayOff() != null).Date
				.Should().Not.Be.EqualTo(skillDays[6].CurrentDate); //should have been moved
		}
		
		[Test]
		[Timeout(8000)]
		public void ShouldNotHangDueToPingPongBetweenTwoDays()
		{
			var date = new DateOnly(2015, 10, 12);
			var activity = ActivityRepository.Has();
			var skill = SkillRepository.Has("skill", activity).IsOpenBetween(8, 16);
			var planningPeriod = PlanningPeriodRepository.Has(date, 1);
			var scenario = ScenarioRepository.Has();
			var schedulePeriod = new SchedulePeriod(date, SchedulePeriodType.Week, 1).NumberOfDaysOff(1);
			var shiftCategory = new ShiftCategory().WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), shiftCategory));
			var agent = PersonRepository.Has(schedulePeriod, ruleSet, skill);
			var skillDays = SkillDayRepository.Has(
				skill.CreateSkillDayWithDemandOnInterval(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), 0.5)
					.SetMinimumAgents(new TimePeriod(8, 16), 1));
			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.GetSingle(skillDays[6].CurrentDate).WithDayOff();

			Target.DoSchedulingAndDO(planningPeriod.Id.Value);
		}
		
		[Test]
		public void ShouldContinueIfFirstMoveDidntSuccessBecauseIntroducingNewBreakingOfMinimumStaffing()
		{
			var date = new DateOnly(2015, 10, 12);
			var activity = ActivityRepository.Has();
			var skill = SkillRepository.Has("skill", activity).IsOpenBetween(8, 16);
			var planningPeriod = PlanningPeriodRepository.Has(date, 1);
			var scenario = ScenarioRepository.Has();
			var schedulePeriod = new SchedulePeriod(date, SchedulePeriodType.Week, 1).NumberOfDaysOff(1);
			var shiftCategory = new ShiftCategory().WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), shiftCategory));
			var agent = PersonRepository.Has(schedulePeriod, ruleSet, skill);
			var alreadyScheduledAgent = PersonRepository.Has(skill);
			var skillDays = SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date,
					0.5, 
					0.6, //should end up here due to min agents on first day
					1,
					1,
					1,
					1,
					0.1) //DO from beginning, issue occur if low demand here
			);
			skillDays[0].SetMinimumAgents(new TimePeriod(8, 16), 2);
			skillDays[6].SetMinimumAgents(new TimePeriod(8, 16), 2);
			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.GetSingle(skillDays[6].CurrentDate).WithDayOff();
			PersonAssignmentRepository.Has(alreadyScheduledAgent, scenario, activity, shiftCategory, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new TimePeriod(8, 0, 16, 0));

			Target.DoSchedulingAndDO(planningPeriod.Id.Value);
			
			PersonAssignmentRepository.LoadAll().Single(x => x.DayOff() != null).Date
				.Should().Be.EqualTo(skillDays[1].CurrentDate);
		}
		
		[Test]
		public void ShouldContinueIfFirstMinimumAgentMoveMadeNightRestBroken()
		{
			var date = new DateOnly(2015, 10, 12);
			var activity = ActivityRepository.Has();
			var skill = SkillRepository.Has("skill", activity).IsOpenBetween(8, 16);
			var planningPeriod = PlanningPeriodRepository.Has(date, 1);
			var scenario = ScenarioRepository.Has();
			var schedulePeriod = new SchedulePeriod(date, SchedulePeriodType.Week, 1).NumberOfDaysOff(2);
			var shiftCategory = new ShiftCategory().WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(23, 0, 23, 0, 60), new TimePeriodWithSegment(31, 0, 31, 0, 60), shiftCategory));
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(1),TimeSpan.FromHours(48),TimeSpan.FromHours(11),TimeSpan.FromHours(1))};
			var agent = PersonRepository.Has(contract, schedulePeriod, ruleSet, skill);
			var alreadyScheduledAgent = PersonRepository.Has(skill);
			var skillDays = SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date,
					1, 
					1,
					1,
					1,
					1,
					1, //DO from beginning
					1, //DO from beginning
					1) 
			);
			skillDays[5].SetMinimumAgents(new TimePeriod(23, 24), 2);
			skillDays[6].SetMinimumAgents(new TimePeriod(23, 24), 3);
			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new TimePeriod(23, 0, 31, 0));
			var dayOff = new DayOffTemplate {Anchor = TimeSpan.FromHours(12)};
			dayOff.SetTargetAndFlexibility(TimeSpan.FromHours(24), TimeSpan.FromHours(9));
			PersonAssignmentRepository.GetSingle(skillDays[5].CurrentDate).WithDayOff(dayOff);
			PersonAssignmentRepository.GetSingle(skillDays[6].CurrentDate).WithDayOff(dayOff);
			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory, date.AddDays(7), new TimePeriod(17, 18)); //prevents replacing last DO
			PersonAssignmentRepository.Has(alreadyScheduledAgent, scenario, activity, shiftCategory, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new TimePeriod(23, 0, 31, 0));

			Target.DoSchedulingAndDO(planningPeriod.Id.Value);
			
			var doDates = PersonAssignmentRepository.LoadAll().Where(x => x.DayOff() != null).Select(x => x.Date);
			doDates.Count().Should().Be.EqualTo(2);
			doDates.Should().Not.Contain(date.AddDays(5));
			doDates.Should().Contain(date.AddDays(6));
		}
		
		[Test]
		public void ShouldTryToPutDoOnADayThatWasUnsuccessfulEarlierInOptimization()
		{
			var date = new DateOnly(2015, 10, 12);
			var activity = ActivityRepository.Has();
			var skill = SkillRepository.Has("skill", activity).IsOpenBetween(8, 16);
			var planningPeriod = PlanningPeriodRepository.Has(date, 1);
			var scenario = ScenarioRepository.Has();
			var schedulePeriod = new SchedulePeriod(date, SchedulePeriodType.Week, 1).NumberOfDaysOff(2);
			var shiftCategory = new ShiftCategory().WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(23, 0, 23, 0, 60), new TimePeriodWithSegment(31, 0, 31, 0, 60), shiftCategory));
			var contract = new Contract("_"){WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(1),TimeSpan.FromHours(48),TimeSpan.FromHours(11),TimeSpan.FromHours(1))};
			var agent = PersonRepository.Has(contract, schedulePeriod, ruleSet, skill);
			var alreadyScheduledAgent = PersonRepository.Has(skill);
			var skillDays = SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date,
					0.1, 
					1,
					1,
					1,
					1,
					1, //DO from beginning
					1, //DO from beginning
					1) 
			);
			skillDays[5].SetMinimumAgents(new TimePeriod(23, 24), 2);
			skillDays[6].SetMinimumAgents(new TimePeriod(23, 24), 3);
			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new TimePeriod(23, 0, 31, 0));
			var dayOff = new DayOffTemplate(){Anchor = TimeSpan.FromHours(12)};
			dayOff.SetTargetAndFlexibility(TimeSpan.FromHours(24), TimeSpan.FromHours(9));
			PersonAssignmentRepository.GetSingle(skillDays[5].CurrentDate).WithDayOff(dayOff);
			PersonAssignmentRepository.GetSingle(skillDays[6].CurrentDate).WithDayOff(dayOff);
			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory, date.AddDays(7), new TimePeriod(17, 18)); //prevents replacing last DO
			PersonAssignmentRepository.Has(alreadyScheduledAgent, scenario, activity, shiftCategory, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new TimePeriod(23, 0, 31, 0));

			Target.DoSchedulingAndDO(planningPeriod.Id.Value);
			
			PersonAssignmentRepository.LoadAll().Where(x => x.DayOff() != null).Select(x => x.Date)
				.Should().Have.SameValuesAs(date.AddDays(0), date.AddDays(6));
		}


		[Test]
		public void ShouldKeepRemovedDayOffIfMinimumAgentsTurnsOutBetter()
		{
			var date = new DateOnly(2015, 10, 12);
			var activity = ActivityRepository.Has();
			var skill = SkillRepository.Has("skill", activity);
			var planningPeriod = PlanningPeriodRepository.Has(date, 1);
			var scenario = ScenarioRepository.Has();
			var schedulePeriod = new SchedulePeriod(date, SchedulePeriodType.Week, 1).NumberOfDaysOff(1);
			var shiftCategory = new ShiftCategory().WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), shiftCategory));
			var agent = PersonRepository.Has(schedulePeriod, ruleSet, skill);
			var alreadyScheduledAgent = PersonRepository.Has(skill);
			var skillDays = skill.CreateSkillDayWithDemandOnInterval(scenario, new DateOnlyPeriod(date, date.AddDays(5)), 2).ToList();
			skillDays.Add(skill.CreateSkillDayWithDemandOnInterval(scenario, date.AddDays(6), 1).SetMinimumAgents(2));
			SkillDayRepository.Has(skillDays);
			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.GetSingle(skillDays[6].CurrentDate, agent).WithDayOff();
			PersonAssignmentRepository.Has(alreadyScheduledAgent, scenario, activity, shiftCategory, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new TimePeriod(8, 0, 16, 0));

			Target.DoSchedulingAndDO(planningPeriod.Id.Value);
			
			PersonAssignmentRepository.LoadAll().Single(x => x.DayOff() != null).Date
				.Should().Not.Be.EqualTo(skillDays[6].CurrentDate); //should have been moved
		}
		
		[Test]
		public void ShouldTryToMoveDayOffIfMoveFailedDueToNewBrokenMinimumAgents()
		{
			var date = new DateOnly(2015, 10, 12);
			var activity = ActivityRepository.Has();
			var skill = SkillRepository.Has("skill", activity);
			var planningPeriod = PlanningPeriodRepository.Has(date, 1);
			var scenario = ScenarioRepository.Has();
			var schedulePeriod = new SchedulePeriod(date, SchedulePeriodType.Week, 1).NumberOfDaysOff(1);
			var shiftCategory = new ShiftCategory().WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), shiftCategory));
			var agent = PersonRepository.Has(schedulePeriod, ruleSet, skill);
			var skillDays = skill.CreateSkillDayWithDemandOnInterval(scenario, new DateOnlyPeriod(date.AddDays(1), date.AddDays(6)), 2).ToList();
			skillDays.Add(skill.CreateSkillDayWithDemandOnInterval(scenario, date.AddDays(0), 0.5)); //will first try to make move here
			skillDays.Single(x => x.CurrentDate == date).SetMinimumAgents(new TimePeriod(8, 16), 1); //..but will be rolled back (should then move sucessfully move to another day)
			skillDays.Single(x => x.CurrentDate == date.AddDays(6)).SetMinimumAgents(new TimePeriod(13, 14), 1);
			SkillDayRepository.Has(skillDays);
			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.GetSingle(date.AddDays(6), agent).WithDayOff();

			Target.DoSchedulingAndDO(planningPeriod.Id.Value);
			
			PersonAssignmentRepository.LoadAll().Single(x => x.DayOff() != null).Date
				.Should().Not.Be.EqualTo(date.AddDays(0))
				.And.Not.Be.EqualTo(date.AddDays(6));
		}
		
		public DayOffOptimizationMinimumAgentsTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}