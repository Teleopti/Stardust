using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.Filters;
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
	public class DayOffOptimizationPlanningGroupTest : DayOffOptimizationScenario
	{
		public FullScheduling Target;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeSkillRepository SkillRepository;
		public FakePersonRepository PersonRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeActivityRepository ActivityRepository;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public FakePlanningGroupRepository PlanningGroupRepository;

		[Test]
		public void ShouldNotMoveDayOffForAgentNotPartOfPlanningGroup()
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var validTeam = new Team();
			var planningGroup = new PlanningGroup();
			planningGroup.AddFilter(new TeamFilter(validTeam));
			PlanningGroupRepository.Has(planningGroup);
			var planningPeriod = PlanningPeriodRepository.Has(firstDay, 1, planningGroup);
			var scenario = ScenarioRepository.LoadDefaultScenario();
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1).NumberOfDaysOff(1);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var validAgent = PersonRepository.Has(new Contract("_"), new ContractSchedule("_"), new PartTimePercentage("_"), validTeam, schedulePeriod, ruleSet, skill);
			var nonValidAgent = PersonRepository.Has(new Contract("_"), new ContractSchedule("_"), new PartTimePercentage("_"), new Team(), schedulePeriod, ruleSet, skill);
			var skillDays = SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 5, 5, 5, 25, 25));
			PersonAssignmentRepository.Has(validAgent, scenario, activity, shiftCategory, new DateOnlyPeriod(firstDay, firstDay.AddDays(7)), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.GetSingle(skillDays[5].CurrentDate, validAgent).SetDayOff(new DayOffTemplate());
			PersonAssignmentRepository.Has(nonValidAgent, scenario, activity, shiftCategory, new DateOnlyPeriod(firstDay, firstDay.AddDays(7)), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.GetSingle(skillDays[6].CurrentDate, nonValidAgent).SetDayOff(new DayOffTemplate());

			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			PersonAssignmentRepository.GetSingle(skillDays[6].CurrentDate, nonValidAgent)
				.DayOff().Should().Not.Be.Null();
		}

		public DayOffOptimizationPlanningGroupTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}