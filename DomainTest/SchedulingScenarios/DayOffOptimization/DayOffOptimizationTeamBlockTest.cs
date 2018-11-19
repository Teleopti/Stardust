using System.Linq;
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
		public OptimizationPreferencesDefaultValueProvider OptimizationPreferencesProvider;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public FakePlanningGroupRepository PlanningGroupRepository;

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
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateBusinessUnitAndAppend(team).WithId(ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value));
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				1, 2, 2, 2, 2, 2, 2,
				1, 2, 2, 2, 2, 2, 2));
			planningPeriod.PlanningGroup.ModifyDefault(x =>
			{
				x.ConsecutiveWorkdays = new MinMax<int>(1, 20); //just to make sure anything goes
			}); 
			var optPrefs = OptimizationPreferencesProvider.Fetch();
			optPrefs.Extra.UseTeams= true;
			OptimizationPreferencesProvider.SetFromTestsOnly(optPrefs);

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

		[Test]
		public void ShouldNotMoveDayOffWhenUsingSameDayOffAndNotInPlanningGroup()
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var team = new Team { Site = new Site("site") }.WithDescription("_").WithId();
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateBusinessUnitAndAppend(team).WithId(ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value));
			var contractToSchedule = new Contract("_").WithId();
			var contractNotToSchedule = new Contract("_").WithId();
			var planningGroup = new PlanningGroup().AddFilter(new ContractFilter(contractToSchedule));
			PlanningGroupRepository.Has(planningGroup);
			var planningPeriod = PlanningPeriodRepository.Has(firstDay, 1, planningGroup);
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1).NumberOfDaysOff(1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory().WithId()));
			var agentToSchedule = PersonRepository.Has(contractToSchedule, new ContractSchedule("_"), new PartTimePercentage("_"), team, schedulePeriod, ruleSet, skill);
			var agentNotToSchedule = PersonRepository.Has(contractNotToSchedule, new ContractSchedule("_"), new PartTimePercentage("_"), team, schedulePeriod, ruleSet, skill);
			var skillDays = SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 5, 5, 5, 25, 5));
			var dayOffTemplate = new DayOffTemplate();
			PersonAssignmentRepository.Has(agentToSchedule, scenario, activity, DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.GetSingle(skillDays[5].CurrentDate, agentToSchedule).SetDayOff(dayOffTemplate);
			PersonAssignmentRepository.Has(agentNotToSchedule, scenario, activity, DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.GetSingle(skillDays[5].CurrentDate, agentNotToSchedule).SetDayOff(dayOffTemplate);
			var optPrefs = OptimizationPreferencesProvider.Fetch();
			optPrefs.Extra.UseTeamSameDaysOff = true;
			optPrefs.Extra.UseTeams = true;
			optPrefs.Extra.TeamGroupPage = new GroupPageLight("_", GroupPageType.Hierarchy);
			optPrefs.Extra.UseTeamBlockOption = true;
			OptimizationPreferencesProvider.SetFromTestsOnly(optPrefs);

			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			PersonAssignmentRepository.GetSingle(skillDays[5].CurrentDate, agentToSchedule).DayOff()
				.Should().Not.Be.Null();
		}

		public DayOffOptimizationTeamBlockTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}