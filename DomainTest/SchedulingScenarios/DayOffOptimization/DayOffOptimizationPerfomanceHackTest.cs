using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.DayOffOptimization
{
	[DomainTest]
	[Toggle(Toggles.ResourcePlanner_RunPerfTestAsTeam_43537)]
	public class DayOffOptimizationPerfomanceHackTest
	{
		public IScheduleOptimization Target;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeSkillRepository SkillRepository;
		public FakePersonRepository PersonRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeActivityRepository ActivityRepository;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public GroupScheduleGroupPageDataProvider GroupScheduleGroupPageDataProvider;
		public FakeDayOffTemplateRepository DayOffTemplateRepository;

		[Test]
		public void ShouldUseTeamAndSameShiftCategory()
		{
			var team = new Team { Site = new Site("_") }.WithDescription(new Description("_"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			DayOffTemplateRepository.Add(new DayOffTemplate());
			var firstDay = new DateOnly(2015, 10, 12);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(168), TimeSpan.FromHours(1), TimeSpan.FromHours(1)) };
			var shiftCategory1 = new ShiftCategory("1").WithId();
			var shiftCategory2 = new ShiftCategory("2").WithId();
			var ruleset1 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory1));
			var ruleset2 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 15), new TimePeriodWithSegment(17, 0, 17, 0, 15), shiftCategory2));
			var bag = new RuleSetBag(ruleset1, ruleset2);
			var agent1 = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), team, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), bag, skill);
			agent1.SchedulePeriod(firstDay).SetDaysOff(1);
			var agent2 = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), team, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), bag, skill);
			agent2.SchedulePeriod(firstDay).SetDaysOff(1);
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1, 1, 10));
			var planningPeriod = PlanningPeriodRepository.Has(firstDay, 1);
			PersonAssignmentRepository.Has(agent1, scenario, activity, shiftCategory2, DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1), new TimePeriod(9, 0, 17, 0));
			PersonAssignmentRepository.GetSingle(firstDay.AddDays(6), agent1).SetDayOff(new DayOffTemplate());
			PersonAssignmentRepository.Has(agent2, scenario, activity, shiftCategory2, DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1), new TimePeriod(9, 0, 17, 0));
			PersonAssignmentRepository.GetSingle(firstDay.AddDays(6), agent2).SetDayOff(new DayOffTemplate());

			Target.Execute(planningPeriod.Id.Value);

			foreach (var schedulesOnDate in PersonAssignmentRepository.Find(new[] { agent1, agent2 }, DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1), scenario).GroupBy(x => x.Date))
			{
				schedulesOnDate.Select(x => x.ShiftCategory).Distinct().Count(x => x != null)
					.Should().Be.EqualTo(1);
			}
		}
	}
}