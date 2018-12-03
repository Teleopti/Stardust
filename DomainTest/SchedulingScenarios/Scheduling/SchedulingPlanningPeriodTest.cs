using System;
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


namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	public class SchedulingPlanningPeriodTest : SchedulingScenario
	{
		public FullScheduling Target;
		public FakePersonRepository PersonRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillRepository SkillRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAssignmentRepository AssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeDayOffTemplateRepository DayOffTemplateRepository;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public FakePlanningGroupRepository PlanningGroupRepository;
		
		[Test]
		public void ShouldOnlySchedulePeopleInPlanningGroup()
		{
			DayOffTemplateRepository.Has(DayOffFactory.CreateDayOff());
			var firstDay = new DateOnly(2015, 10, 12);
			var period = new DateOnlyPeriod(firstDay, firstDay.AddDays(7));
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(168), TimeSpan.FromHours(1), TimeSpan.FromHours(1))
			};
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var contractSchedule = new ContractSchedule("_");
			var partTimePercentage = new PartTimePercentage("_");
			var site = new Site("site");
			var team = new Team { Site = site };
			var team2 = new Team { Site = site };
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1);
			var agent = PersonRepository.Has(contract, contractSchedule, partTimePercentage, team, schedulePeriod, ruleSet, skill);
			var agent2 = PersonRepository.Has(contract, contractSchedule, partTimePercentage, team2, schedulePeriod, ruleSet, skill);

			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				1,
				1,
				1,
				1,
				1,
				1,
				1)
			);
			var planningGroup = new PlanningGroup().AddFilter(new TeamFilter(team));
			PlanningGroupRepository.Has(planningGroup);
			var planningPeriod = new PlanningPeriod(period.StartDate,SchedulePeriodType.Day, 8, planningGroup);
			PlanningPeriodRepository.Add(planningPeriod);

			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			AssignmentRepository.Find(new[] { agent }, period, scenario).Should().Not.Be.Empty();
			AssignmentRepository.Find(new[] { agent2 }, period, scenario).Should().Be.Empty();
		}
		
		[Test]
		public void ShouldBePossibleToScheduleOneMonthWithoutManuallyExtendingThePeriod()
		{
			DayOffTemplateRepository.Has(DayOffFactory.CreateDayOff());
			var period = new DateOnlyPeriod(new DateOnly(2017, 08, 01), new DateOnly(2017, 08, 31));
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory().WithId()));
			var schedulePeriod = new SchedulePeriod(period.StartDate, SchedulePeriodType.Month, 1);
			var agent = PersonRepository.Has(schedulePeriod, ruleSet, skill);
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, period,1));
			var planningPeriod = new PlanningPeriod(period.StartDate,SchedulePeriodType.Month,1,PlanningGroupRepository.Has());
			PlanningPeriodRepository.Add(planningPeriod);
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			AssignmentRepository.Find(new[] { agent }, period, scenario)
				.Count.Should().Be.EqualTo(31);
		}
		
		
		public SchedulingPlanningPeriodTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}