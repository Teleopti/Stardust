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


namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.DayOffOptimization
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
		public FakePlanningGroupRepository PlanningGroupRepository;
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
        	
        	BusinessUnitRepository.Has(BusinessUnitFactory.CreateBusinessUnitAndAppend(team).WithId(ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value));
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
        	
        	BusinessUnitRepository.Has(BusinessUnitFactory.CreateBusinessUnitAndAppend(team).WithId(ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value));
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
        	
        	BusinessUnitRepository.Has(BusinessUnitFactory.CreateBusinessUnitAndAppend(team).WithId(ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value));
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
	}
}