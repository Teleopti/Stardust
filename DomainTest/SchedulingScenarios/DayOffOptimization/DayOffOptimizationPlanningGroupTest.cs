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
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.DayOffOptimization
{
	[DomainTest]
	public class DayOffOptimizationPlanningGroupTest : DayOffOptimizationScenario
	{
		public DayOffOptimizationWeb Target;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeSkillRepository SkillRepository;
		public FakePersonRepository PersonRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeActivityRepository ActivityRepository;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public OptimizationPreferencesDefaultValueProvider OptimizationPreferencesProvider;

		[Test]
		public void ShouldNotMoveDayOffForAgentNotPartOfPlanningGroup()
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var validTeam = new Team();
			var planningGroup = new PlanningGroup("_");
			planningGroup.AddFilter(new TeamFilter(validTeam));
			var planningPeriod = PlanningPeriodRepository.Has(firstDay, 1, planningGroup);
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1).NumberOfDaysOf(1);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var validAgent = PersonRepository.Has(new Contract("_"), new ContractSchedule("_"), new PartTimePercentage("_"), validTeam, schedulePeriod, ruleSet, skill);
			var nonValidAgent = PersonRepository.Has(new Contract("_"), new ContractSchedule("_"), new PartTimePercentage("_"), new Team(), schedulePeriod, ruleSet, skill);
			var skillDays = SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 5, 5, 5, 25, 25));
			PersonAssignmentRepository.Has(validAgent, scenario, activity, shiftCategory, new DateOnlyPeriod(firstDay, firstDay.AddDays(7)), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.GetSingle(skillDays[5].CurrentDate, validAgent).SetDayOff(new DayOffTemplate());
			PersonAssignmentRepository.Has(nonValidAgent, scenario, activity, shiftCategory, new DateOnlyPeriod(firstDay, firstDay.AddDays(7)), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.GetSingle(skillDays[6].CurrentDate, nonValidAgent).SetDayOff(new DayOffTemplate());

			Target.Execute(planningPeriod.Id.Value);

			PersonAssignmentRepository.GetSingle(skillDays[6].CurrentDate, nonValidAgent)
				.DayOff().Should().Not.Be.Null();
		}

		[Test]
		[Ignore("47139 to be fixed")]
		public void ShouldNotMoveDayOffWhenUsingSameDayOffAndNotInPlanningGroup()
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var team = new Team().WithId();
			var planningGroup = new PlanningGroup("_");
			var contractToSchedule = new Contract("_").WithId();
			var contractNotToSchedule = new Contract("_").WithId();
			planningGroup.AddFilter(new ContractFilter(contractToSchedule));
			var planningPeriod = PlanningPeriodRepository.Has(firstDay, 1, planningGroup);
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1).NumberOfDaysOf(1);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agentToSchedule = PersonRepository.Has(contractToSchedule, new ContractSchedule("_"), new PartTimePercentage("_"), team, schedulePeriod, ruleSet, skill);
			var agentNotToSchedule = PersonRepository.Has(contractNotToSchedule, new ContractSchedule("_"), new PartTimePercentage("_"), team, schedulePeriod, ruleSet, skill);
			var skillDays = SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 5, 5, 5, 25, 5));
			PersonAssignmentRepository.Has(agentToSchedule, scenario, activity, shiftCategory, new DateOnlyPeriod(firstDay, firstDay.AddDays(7)), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.GetSingle(skillDays[5].CurrentDate, agentToSchedule).SetDayOff(new DayOffTemplate());
			PersonAssignmentRepository.Has(agentNotToSchedule, scenario, activity, shiftCategory, new DateOnlyPeriod(firstDay, firstDay.AddDays(7)), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.GetSingle(skillDays[5].CurrentDate, agentNotToSchedule).SetDayOff(new DayOffTemplate());
			var optPrefs = OptimizationPreferencesProvider.Fetch();
			optPrefs.Extra.UseTeamBlockOption = true;
			optPrefs.Extra.UseTeamSameDaysOff = true;
			OptimizationPreferencesProvider.SetFromTestsOnly(optPrefs);

			Target.Execute(planningPeriod.Id.Value);

			PersonAssignmentRepository.GetSingle(skillDays[5].CurrentDate, agentToSchedule)
				.DayOff().Should().Not.Be.Null();
		}

		public DayOffOptimizationPlanningGroupTest(RemoveImplicitResCalcContext removeImplicitResCalcContext) : base(removeImplicitResCalcContext)
		{
		}
	}
}