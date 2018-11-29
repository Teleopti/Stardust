using System;
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
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{ 
	[DomainTest]
	[Toggle(Toggles.ResourcePlanner_RunPerfTestAsTeam_43537)]
	public class PerformanceTestHackTest : SchedulingScenario
	{
		public FullScheduling Target;
		public FakePersonRepository PersonRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillRepository SkillRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public FakePersonAssignmentRepository AssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeDayOffTemplateRepository DayOffTemplateRepository;
		public FakePlanningPeriodRepository PlanningPeriodRepository;

		[Test]
		public void ShouldUseTeamAndSameShiftCategory()
		{
			var team = new Team { Site = new Site("_") }.WithDescription(new Description("_"));
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateBusinessUnitAndAppend(team).WithId(ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value));
			DayOffTemplateRepository.Has(DayOffFactory.CreateDayOff());
			var firstDay = new DateOnly(2015, 10, 12);
			var period = new DateOnlyPeriod(firstDay, firstDay.AddDays(7));
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(168), TimeSpan.FromHours(1), TimeSpan.FromHours(1))};
			var shiftCategory1 = new ShiftCategory("_").WithId();
			var shiftCategory2 = new ShiftCategory("_").WithId();
			var ruleset1 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory1));
			var ruleset2 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 15), new TimePeriodWithSegment(17, 0, 17, 0, 15), shiftCategory2));
			var bag = new RuleSetBag(ruleset1, ruleset2);
			var agent1 = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), team, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), bag, skill);
			var agent2 = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), team, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), bag, skill);
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1, 1, 1));
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate,SchedulePeriodType.Day, 8);
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			foreach (var schedulesOnDate in AssignmentRepository.Find(new[] { agent1, agent2 }, period, scenario).GroupBy(x => x.Date))
			{
				schedulesOnDate.Select(x => x.ShiftCategory).Distinct().Count()
					.Should().Be.EqualTo(1);
			}
		}

		public PerformanceTestHackTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}