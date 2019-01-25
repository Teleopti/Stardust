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
		
		[Test]
		public void ShouldNotMoveDayOffWhenUsingSameDayOffAndNotInPlanningGroup()
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var team = new Team { Site = new Site("site") }.WithDescription("_").WithId();
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateBusinessUnitAndAppend(team).WithId(ServiceLocator_DONTUSE.CurrentBusinessUnit.Current().Id.Value));
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