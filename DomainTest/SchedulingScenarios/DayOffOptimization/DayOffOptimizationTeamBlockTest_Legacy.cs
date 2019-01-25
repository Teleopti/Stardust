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

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.DayOffOptimization
{
	/*  DONT ADD MORE TESTS HERE! - LEGACY TESTS HERE!
	 *  Web supports (limited) block (not team) DOopt only.
	 * Dont use OptimizationPreferencesProvider.SetFromTestsOnly_LegacyDONOTUSE...
	 * Either set it on planninggroup or make a desktop test instead.
	 */
	[DomainTest]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	public class DayOffOptimizationTeamBlockTest_Legacy : DayOffOptimizationScenario
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
		public FakeDayOffTemplateRepository DayOffTemplateRepository;
		
		
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
			OptimizationPreferencesProvider.SetFromTestsOnly_LegacyDONOTUSE(optPrefs);

			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			PersonAssignmentRepository.GetSingle(skillDays[5].CurrentDate, agentToSchedule).DayOff()
				.Should().Not.Be.Null();
		}
		
		[Test]
		public void ShouldNotContinueWhenWorsePeriodValueAndUsingTweakedValues()
		{
			var prefUsedInThisTest = OptimizationPreferencesProvider.Fetch();
			prefUsedInThisTest.Advanced.UseTweakedValues = true;
			OptimizationPreferencesProvider.SetFromTestsOnly_LegacyDONOTUSE(prefUsedInThisTest);
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
			var agent = PersonRepository.Has(new Contract("_"), new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, ruleSet, skill);
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

			var dayOff1 = PersonAssignmentRepository.GetSingle(skillDays[2].CurrentDate).DayOff();
			var dayOff2 = PersonAssignmentRepository.GetSingle(skillDays[9].CurrentDate).DayOff();

			//when using tweaked values we should bail out when period value gets worse when we optimize
			Assert.IsTrue(dayOff1 == null || dayOff2 == null);
		}

		public override void Isolate(IIsolate isolate)
		{
			base.Isolate(isolate);
			//hack until web supports team scheduling
			isolate.UseTestDouble<OptimizationPreferencesDefaultValueProvider>().For<IOptimizationPreferencesProvider>();
		}

		public DayOffOptimizationTeamBlockTest_Legacy(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}