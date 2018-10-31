using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	public class SchedulingFilterAgentsByHintsTest : SchedulingScenario
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
		
		[Test]
		public void ShouldNotScheduleAgentWithoutShiftBag()
		{	
			var expected = !ResourcePlannerTestParameters.IsEnabled(Toggles.ResourcePlanner_FasterSeamlessPlanningForPreferences_78286);
			
			DayOffTemplateRepository.Has(DayOffFactory.CreateDayOff());
			var date = new DateOnly(2015, 10, 12);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			PersonRepository.Has(new ContractScheduleWorkingMondayToFriday(), new SchedulePeriod(date, SchedulePeriodType.Week, 1), skill);
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), 1));
			var planningPeriod = PlanningPeriodRepository.Has(date, 1);
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			AssignmentRepository.LoadAll().Any(x => x.HasDayOffOrMainShiftLayer())
				.Should().Be.EqualTo(expected);
		}

		public SchedulingFilterAgentsByHintsTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}