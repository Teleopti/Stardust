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
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.DayOffOptimization
{
	[DomainTest]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	public class DayOffOptimizationDayOffRulesTest : DayOffOptimizationScenario
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
		public OptimizationPreferencesDefaultValueProvider OptimizationPreferencesProvider;

		[Test]
		public void ShouldUseSettingForConsecutiveDayOffs()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var planningPeriod = PlanningPeriodRepository.Has(firstDay, 1);
			var scenario = ScenarioRepository.Has("some name");
			planningPeriod.PlanningGroup.ModifyDefault(x => 
			{
				x.DayOffsPerWeek = new MinMax<int>(1, 4);
				x.ConsecutiveDayOffs = new MinMax<int>(1, 4);
			});
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1);
			schedulePeriod.SetDaysOff(4);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(new Contract("_"), new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, ruleSet, skill);

			var skillDays = SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				1, //DO
				1, //DO
				5, //DO
				10,
				20,
				20,
				25) //DO
				);

			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory, new DateOnlyPeriod(firstDay.AddDays(-1), firstDay.AddDays(8)), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.GetSingle(skillDays[0].CurrentDate).SetDayOff(new DayOffTemplate());
			PersonAssignmentRepository.GetSingle(skillDays[1].CurrentDate).SetDayOff(new DayOffTemplate());
			PersonAssignmentRepository.GetSingle(skillDays[2].CurrentDate).SetDayOff(new DayOffTemplate());
			PersonAssignmentRepository.GetSingle(skillDays[6].CurrentDate).SetDayOff(new DayOffTemplate());

			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			PersonAssignmentRepository.GetSingle(skillDays[0].CurrentDate).DayOff().Should().Not.Be.Null();
			PersonAssignmentRepository.GetSingle(skillDays[1].CurrentDate).DayOff().Should().Not.Be.Null();
			PersonAssignmentRepository.GetSingle(skillDays[2].CurrentDate).DayOff().Should().Not.Be.Null();
			PersonAssignmentRepository.GetSingle(skillDays[3].CurrentDate).DayOff().Should().Not.Be.Null();
		}

		[Test]
		public void ShouldUseSettingForConsecutiveWorkDays()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mo
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var planningPeriod = PlanningPeriodRepository.Has(firstDay, 1);
			var scenario = ScenarioRepository.Has("some name");
			planningPeriod.PlanningGroup.ModifyDefault(x => x.ConsecutiveWorkdays = new MinMax<int>(1, 6));
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1);
			schedulePeriod.SetDaysOff(1);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(new Contract("_"), new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, ruleSet, skill);

			var skillDays = SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				5,
				1,
				5,
				25,
				5,
				5,
				5)
				);

			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory, new DateOnlyPeriod(firstDay.AddDays(-1), firstDay.AddDays(8)), new TimePeriod(8, 0, 16, 0));

			PersonAssignmentRepository.GetSingle(skillDays[3].CurrentDate).SetDayOff(new DayOffTemplate());
			PersonAssignmentRepository.GetSingle(skillDays[0].CurrentDate.AddDays(-1)) //sunday in week before
				.SetDayOff(new DayOffTemplate());

			Target.DoSchedulingAndDO(planningPeriod.Id.Value);
			
			PersonAssignmentRepository.GetSingle(skillDays[1].CurrentDate) //tuesday
				.DayOff().Should().Not.Be.Null();	
		}

		[Test]
		public void ShouldUseSettingForDayOffPerWeek_NotValid()
		{
			var firstDay = new DateOnly(2015, 10, 26); //mon
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var planningPeriod = PlanningPeriodRepository.Has(firstDay, 2);
			var scenario = ScenarioRepository.Has("some name");
			planningPeriod.PlanningGroup.ModifyDefault(x => x.DayOffsPerWeek = new MinMax<int>(3, 4));
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 2);
			schedulePeriod.SetDaysOff(2);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(new Contract("_"), new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, ruleSet, skill);

			var skillDays = SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				20,
				20,
				3,
				20,
				20,
				20,
				20,
				20,
				20,
				1,
				2,
				20,
				20,
				20
				));

			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory, new DateOnlyPeriod(firstDay, firstDay.AddDays(14)), new TimePeriod(8, 0, 16, 0));

			PersonAssignmentRepository.GetSingle(skillDays[0].CurrentDate).SetDayOff(new DayOffTemplate());
			PersonAssignmentRepository.GetSingle(skillDays[1].CurrentDate).SetDayOff(new DayOffTemplate());

			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			PersonAssignmentRepository.GetSingle(skillDays[0].CurrentDate).DayOff().Should().Not.Be.Null();
			PersonAssignmentRepository.GetSingle(skillDays[1].CurrentDate).DayOff().Should().Not.Be.Null();
		}

		[Test]
		public void ShouldUseSettingForDayOffPerWeek_Valid()
		{
			var firstDay = new DateOnly(2015, 10, 26); //mon
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var planningPeriod = PlanningPeriodRepository.Has(firstDay, 2);
			var scenario = ScenarioRepository.Has("some name");
			planningPeriod.PlanningGroup.ModifyDefault(x =>
			{
				x.DayOffsPerWeek = new MinMax<int>(1, 3);
				x.ConsecutiveWorkdays = new MinMax<int>(2, 20);
			});
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 2);
			schedulePeriod.SetDaysOff(2);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(new ContractWithMaximumTolerance(), new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, ruleSet, skill);
			var skillDays = SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				20,
				20,
				3, //expected
				20,
				20,
				20,
				20,
				20,
				20,
				1, //expected
				2,
				20,
				20,
				20
			));
			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory, new DateOnlyPeriod(firstDay, firstDay.AddDays(14)), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.GetSingle(skillDays[0].CurrentDate).SetDayOff(new DayOffTemplate());
			PersonAssignmentRepository.GetSingle(skillDays[1].CurrentDate).SetDayOff(new DayOffTemplate());

			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			PersonAssignmentRepository.GetSingle(skillDays[2].CurrentDate).DayOff().Should().Not.Be.Null();
			PersonAssignmentRepository.GetSingle(skillDays[9].CurrentDate).DayOff().Should().Not.Be.Null();
		}

		[Test]
		public void ShouldUseSettingForDayOffPerWeek_Valid_EvenWhenUseTeamSameDaysOffIsTrueButUseTeamIsNot()
		{
			var optPrefs = OptimizationPreferencesProvider.Fetch();
			optPrefs.Extra.UseTeams = false;
			optPrefs.Extra.UseTeamSameDaysOff = true;
			ShouldUseSettingForDayOffPerWeek_Valid();
		}

		[Test]
		public void ShouldUseDifferentDayOffRulesForDifferentFiltersWhenOneIsIncorrect()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var planningPeriod = PlanningPeriodRepository.Has(firstDay, 1);
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1);
			schedulePeriod.SetDaysOff(1);
			var contractInFilter = new Contract("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agentWithValidDefaultFilter = PersonRepository.Has(new Contract("_"), new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, ruleSet, skill);
			var agentWithExplicitFilter = PersonRepository.Has(contractInFilter, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, ruleSet, skill);
			var dayOffRules = new PlanningGroupSettings() { DayOffsPerWeek = new MinMax<int>(5,6)};
			dayOffRules.AddFilter(new ContractFilter(contractInFilter));
			planningPeriod.PlanningGroup.AddSetting(dayOffRules);
			PlanningGroupRepository.Add(planningPeriod.PlanningGroup);
			var skillDays = SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				20, 
				1, //expected
				5, 
				10,
				20,
				20,
				25) 
				);
			PersonAssignmentRepository.Has(agentWithValidDefaultFilter, scenario, activity, shiftCategory, new DateOnlyPeriod(firstDay.AddDays(-1), firstDay.AddDays(8)), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.Has(agentWithExplicitFilter, scenario, activity, shiftCategory, new DateOnlyPeriod(firstDay.AddDays(-1), firstDay.AddDays(8)), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.GetSingle(skillDays[0].CurrentDate, agentWithValidDefaultFilter).SetDayOff(new DayOffTemplate());
			PersonAssignmentRepository.GetSingle(skillDays[6].CurrentDate, agentWithExplicitFilter).SetDayOff(new DayOffTemplate());

			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			PersonAssignmentRepository.GetSingle(skillDays[0].CurrentDate, agentWithValidDefaultFilter).DayOff().Should().Be.Null();
			PersonAssignmentRepository.GetSingle(skillDays[6].CurrentDate, agentWithExplicitFilter).DayOff().Should().Not.Be.Null();
		}

		public DayOffOptimizationDayOffRulesTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}