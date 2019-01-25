using System;
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
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	public class SchedulingAndDayOffTest : SchedulingScenario
	{
		public FullScheduling Target;
		public FakePersonRepository PersonRepository;
		public FakeSkillRepository SkillRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeDayOffTemplateRepository DayOffTemplateRepository;
		public FakePlanningGroupRepository PlanningGroupRepository;

		[Test]
		public void ShouldScheduleAndDayOffOptimizeAllDays()
		{
			DayOffTemplateRepository.Has(DayOffFactory.CreateDayOff());
			var date = new DateOnly(2018, 8, 13);
			var activity = ActivityRepository.Has();
			var skill = SkillRepository.Has(activity);
			var scenario = ScenarioRepository.Has();
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date, 1, 2, 2, 2, 2, 2, 2));
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var schedulePeriod = new SchedulePeriod(date, SchedulePeriodType.Week, 1);
			var team = new Team();
			var planningGroup = new PlanningGroup();
			planningGroup.AddFilter(new TeamFilter(team));
			PlanningGroupRepository.Has(planningGroup);
			var planningPeriod = PlanningPeriodRepository.Has(date, 1, planningGroup);
			var agentToSchedule = PersonRepository.Has(team,schedulePeriod, ruleSet, skill);
			var contractSchedule = new ContractSchedule("_");
			var contractScheduleWeek = new ContractScheduleWeek();
			contractScheduleWeek.SetWorkdaysExcept(DayOfWeek.Sunday);
			contractSchedule.AddContractScheduleWeek(contractScheduleWeek);
			agentToSchedule.Period(date).PersonContract.ContractSchedule = contractSchedule;
			var agentNotToSchedule = PersonRepository.Has(schedulePeriod, ruleSet, skill);
			PersonAssignmentRepository.Has(agentNotToSchedule, scenario, activity, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new TimePeriod(8, 16));

			Target.DoSchedulingAndDO(planningPeriod.Id.Value);
			
			var scheduledAgentsAsses = PersonAssignmentRepository.LoadAll().Where(x => x.Person.Equals(agentToSchedule));
			scheduledAgentsAsses.Count().Should().Be.EqualTo(7);
			scheduledAgentsAsses.Single(x => x.Date.DayOfWeek == DayOfWeek.Monday).DayOffTemplate.Should().Not.Be.Null();

		}

		public SchedulingAndDayOffTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}