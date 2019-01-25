using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
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
	public class DayOffOptimizationTeamBlockTest : DayOffOptimizationScenario
	{
		public FullScheduling Target;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeSkillRepository SkillRepository;
		public FakePersonRepository PersonRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeActivityRepository ActivityRepository;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public FakeDayOffTemplateRepository DayOffTemplateRepository;
		
		public DayOffOptimizationTeamBlockTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
		
		[Test]
        public void ShouldDayOffOptimizeTeamSchedulingUseSameStartTimeForHierarchy()
        {
        	if (!ResourcePlannerTestParameters.IsEnabled(Toggles.ResourcePlanner_TeamSchedulingInPlans_79283))
        		Assert.Ignore("only works with toggle on");
        	DayOffTemplateRepository.Add(new DayOffTemplate());
        	var date = new DateOnly(2015, 10, 12);
        	var period = DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1);
        	var activity = ActivityRepository.Has();
        	var skill = SkillRepository.Has(activity);
        	var scenario = ScenarioRepository.Has();
        	var team = new Team().WithDescription(new Description("team1")).WithId();
        	
        	BusinessUnitRepository.Has(BusinessUnitFactory.CreateBusinessUnitAndAppend(team).WithId(ServiceLocator_DONTUSE.CurrentBusinessUnit.Current().Id.Value));
        	var shiftCategory = new ShiftCategory().WithId();
        	var ruleSet1 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
        	var ruleSet2 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(12, 0, 12, 0, 15), new TimePeriodWithSegment(20, 0, 20, 0, 15), shiftCategory));
        	var bag = new RuleSetBag(ruleSet1, ruleSet2);
        	
			PersonRepository.Has(new Contract("_"), new ContractScheduleWorkingMondayToFriday(),new PartTimePercentage("_"), team, new SchedulePeriod(date, SchedulePeriodType.Week, 1), bag, skill);
        	PersonRepository.Has(new Contract("_"), new ContractScheduleWorkingMondayToFriday(),new PartTimePercentage("_"), team, new SchedulePeriod(date, SchedulePeriodType.Week, 1), bag, skill);
			SkillDayRepository.Has(
				skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date, 1, 1, 1, 1, 1, 10, 1));
        	var planningPeriod = PlanningPeriodRepository.Has(period.StartDate, SchedulePeriodType.Week, 1);
        	planningPeriod.PlanningGroup.SetTeamSettings(new TeamSettings
        	{
        		GroupPageType = GroupPageType.Hierarchy,
        		TeamSameType = TeamSameType.StartTime
        	});

        	Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			var shiftsOnSaturday = PersonAssignmentRepository.LoadAll().Where(x => x.Date == date.AddDays(5));
			shiftsOnSaturday.First().ShiftLayers.First().Period.StartDateTime.Should().Be
				.EqualTo(shiftsOnSaturday.Last().ShiftLayers.First().Period.StartDateTime);
        }
		
		[Test]
        public void ShouldDayOffOptimizeTeamSchedulingUseSameEndTimeForHierarchy()
        {
        	if (!ResourcePlannerTestParameters.IsEnabled(Toggles.ResourcePlanner_TeamSchedulingInPlans_79283))
        		Assert.Ignore("only works with toggle on");
        	DayOffTemplateRepository.Add(new DayOffTemplate());
        	var date = new DateOnly(2015, 10, 12);
        	var period = DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1);
        	var activity = ActivityRepository.Has();
        	var skill = SkillRepository.Has(activity);
        	var scenario = ScenarioRepository.Has();
        	var team = new Team().WithDescription(new Description("team1")).WithId();
        	
        	BusinessUnitRepository.Has(BusinessUnitFactory.CreateBusinessUnitAndAppend(team).WithId(ServiceLocator_DONTUSE.CurrentBusinessUnit.Current().Id.Value));
        	var shiftCategory = new ShiftCategory().WithId();
        	var ruleSet1 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
        	var ruleSet2 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(12, 0, 12, 0, 15), new TimePeriodWithSegment(20, 0, 20, 0, 15), shiftCategory));
        	var bag = new RuleSetBag(ruleSet1, ruleSet2);
        	
			PersonRepository.Has(new Contract("_"), new ContractScheduleWorkingMondayToFriday(),new PartTimePercentage("_"), team, new SchedulePeriod(date, SchedulePeriodType.Week, 1), bag, skill);
        	PersonRepository.Has(new Contract("_"), new ContractScheduleWorkingMondayToFriday(),new PartTimePercentage("_"), team, new SchedulePeriod(date, SchedulePeriodType.Week, 1), bag, skill);
			SkillDayRepository.Has(
				skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date, 1, 1, 1, 1, 1, 10, 1));
        	var planningPeriod = PlanningPeriodRepository.Has(period.StartDate, SchedulePeriodType.Week, 1);
        	planningPeriod.PlanningGroup.SetTeamSettings(new TeamSettings
        	{
        		GroupPageType = GroupPageType.Hierarchy,
        		TeamSameType = TeamSameType.EndTime
        	});

        	Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			var shiftsOnSaturday = PersonAssignmentRepository.LoadAll().Where(x => x.Date == date.AddDays(5));
			shiftsOnSaturday.First().ShiftLayers.First().Period.EndDateTime.Should().Be
				.EqualTo(shiftsOnSaturday.Last().ShiftLayers.First().Period.EndDateTime);
        }
		
		[Test]
        public void ShouldDayOffOptimizeTeamSchedulingUseSameShiftCategoryForHierarchy()
        {
        	if (!ResourcePlannerTestParameters.IsEnabled(Toggles.ResourcePlanner_TeamSchedulingInPlans_79283))
        		Assert.Ignore("only works with toggle on");
        	DayOffTemplateRepository.Add(new DayOffTemplate());
        	var date = new DateOnly(2015, 10, 12);
        	var period = DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1);
        	var activity = ActivityRepository.Has();
        	var skill = SkillRepository.Has(activity);
        	var scenario = ScenarioRepository.Has();
        	var team = new Team().WithDescription(new Description("team1")).WithId();
        	
        	BusinessUnitRepository.Has(BusinessUnitFactory.CreateBusinessUnitAndAppend(team).WithId(ServiceLocator_DONTUSE.CurrentBusinessUnit.Current().Id.Value));
        	var shiftCategory1 = new ShiftCategory().WithId();
        	var shiftCategory2 = new ShiftCategory().WithId();
        	var ruleSet1 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory1));
        	var ruleSet2 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(12, 0, 12, 0, 15), new TimePeriodWithSegment(20, 0, 20, 0, 15), shiftCategory2));
        	var bag = new RuleSetBag(ruleSet1, ruleSet2);
        	
			PersonRepository.Has(new Contract("_"), new ContractScheduleWorkingMondayToFriday(),new PartTimePercentage("_"), team, new SchedulePeriod(date, SchedulePeriodType.Week, 1), bag, skill);
        	PersonRepository.Has(new Contract("_"), new ContractScheduleWorkingMondayToFriday(),new PartTimePercentage("_"), team, new SchedulePeriod(date, SchedulePeriodType.Week, 1), bag, skill);
			SkillDayRepository.Has(
				skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date, 1, 1, 1, 1, 1, 10, 1));
        	var planningPeriod = PlanningPeriodRepository.Has(period.StartDate, SchedulePeriodType.Week, 1);
        	planningPeriod.PlanningGroup.SetTeamSettings(new TeamSettings
        	{
        		GroupPageType = GroupPageType.Hierarchy,
        		TeamSameType = TeamSameType.ShiftCategory
        	});

        	Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			var shiftsOnSaturday = PersonAssignmentRepository.LoadAll().Where(x => x.Date == date.AddDays(5));
			shiftsOnSaturday.First().ShiftCategory.Should().Be
				.EqualTo(shiftsOnSaturday.Last().ShiftCategory);
        }
		
		[Test]
		public void ShouldNotMoveDOsForOneAgentOnlyButChangeAfterEachPeriod([Values(true, false)] bool useTeams)
		{
			const int numberOfAttempts = 20;
			var firstDay = new DateOnly(2015, 10, 12);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var planningPeriod = PlanningPeriodRepository.Has(firstDay, 2);
			var scenario = ScenarioRepository.Has("_");
			var team = new Team {Site = new Site("site")};
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory("_").WithId()));
			var agent1 = PersonRepository.Has(new Contract("_"), new ContractSchedule("_"), new PartTimePercentage("_"), team, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1).NumberOfDaysOff(1), ruleSet, skill);
			var agent2 = PersonRepository.Has(new Contract("_"), new ContractSchedule("_"), new PartTimePercentage("_"), team, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1).NumberOfDaysOff(1), ruleSet, skill);
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateBusinessUnitAndAppend(team).WithId(ServiceLocator_DONTUSE.CurrentBusinessUnit.Current().Id.Value));
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				1, 2, 2, 2, 2, 2, 2,
				1, 2, 2, 2, 2, 2, 2));
			planningPeriod.PlanningGroup.ModifyDefault(x =>
			{
				x.ConsecutiveWorkdays = new MinMax<int>(1, 20); //just to make sure anything goes
			}); 
			planningPeriod.PlanningGroup.SetTeamSettings(new TeamSettings()
			{
				GroupPageType = GroupPageType.Hierarchy,
				TeamSameType = TeamSameType.ShiftCategory
			});

			for (var i = 0; i < numberOfAttempts; i++)
			{
				PersonAssignmentRepository.Clear();
				PersonAssignmentRepository.Has(agent1, scenario, activity, new ShiftCategory("_").WithId(), DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 2), new TimePeriod(8, 0, 16, 0));
				var dayOffTemplate = new DayOffTemplate();
				PersonAssignmentRepository.GetSingle(firstDay.AddWeeks(0).AddDays(6), agent1).SetDayOff(dayOffTemplate);
				PersonAssignmentRepository.GetSingle(firstDay.AddWeeks(1).AddDays(6), agent1).SetDayOff(dayOffTemplate);
				PersonAssignmentRepository.Has(agent2, scenario, activity, new ShiftCategory("_").WithId(), DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 2), new TimePeriod(8, 0, 16, 0));
				PersonAssignmentRepository.GetSingle(firstDay.AddWeeks(0).AddDays(6), agent2).SetDayOff(dayOffTemplate);
				PersonAssignmentRepository.GetSingle(firstDay.AddWeeks(1).AddDays(6), agent2).SetDayOff(dayOffTemplate);

				Target.DoSchedulingAndDO(planningPeriod.Id.Value);

				var allDOs = PersonAssignmentRepository.LoadAll().Where(x => x.DayOff() != null);
				var movedD01 = allDOs.Single(x => x.Date == firstDay);
				var movedD02 = allDOs.Single(x => x.Date == firstDay.AddWeeks(1));
				if (!movedD01.Person.Equals(movedD02.Person))
					return;
			}

			Assert.Fail(
				$"Tried optimize {numberOfAttempts} number of times but always moving DOs from same agent. Giving up...");
		}
	}
}