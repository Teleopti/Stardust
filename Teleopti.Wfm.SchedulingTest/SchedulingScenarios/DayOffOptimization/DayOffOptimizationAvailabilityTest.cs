using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
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
	public class DayOffOptimizationAvailabilityTest : DayOffOptimizationScenario
	{
		public FullScheduling Target;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeSkillRepository SkillRepository;
		public FakePersonRepository PersonRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeActivityRepository ActivityRepository;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public FakePersonAvailabilityRepository PersonAvailabilityRepository;
		
		[Test]
		public void ShouldRespectAvailability()
		{
			var date = new DateOnly(2015, 10, 12); //mon
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var planningPeriod = PlanningPeriodRepository.Has(date, 1);
			var scenario = ScenarioRepository.LoadDefaultScenario();
			var schedulePeriod = new SchedulePeriod(date, SchedulePeriodType.Week, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory().WithId()));
			var agent = PersonRepository.Has(schedulePeriod, ruleSet, skill);
			var dayOffTemplate = new DayOffTemplate();
			var personAvailability = new PersonAvailability(agent, new AvailabilityRotation("_", 7), date);
			personAvailability.Availability.AvailabilityDays[6].Restriction.NotAvailable = true;
			var skillDays = SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date,
				1, 1, 2, 2, 2, 2, 2));
			PersonAssignmentRepository.Has(agent, scenario, activity, new ShiftCategory(), DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.GetSingle(skillDays[5].CurrentDate).SetDayOff(dayOffTemplate); //saturday
			PersonAssignmentRepository.GetSingle(skillDays[6].CurrentDate).SetDayOff(dayOffTemplate); //sunday
			PersonAvailabilityRepository.Has(personAvailability);
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			var saturdayAndSunday = PersonAssignmentRepository.LoadAll().Where(x => x.Date == date.AddDays(5) || x.Date == date.AddDays(6));
			saturdayAndSunday.Count(x => x.DayOffTemplate != null)
				.Should().Be.EqualTo(1);
		}
		
		
		public DayOffOptimizationAvailabilityTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}