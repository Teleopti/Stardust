using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.DayOffOptimization
{	
	[DomainTest]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	public class DayOffOptimizationPreferencesTest : DayOffOptimizationScenario
	{
		public FullScheduling Target;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeSkillRepository SkillRepository;
		public FakePersonRepository PersonRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeActivityRepository ActivityRepository;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public FakePlanningGroupSettingsRepository PlanningGroupSettingsRepository;
		public FakePreferenceDayRepository PreferenceDayRepository;

		[TestCase(0.60, ExpectedResult = 2)]
		[TestCase(0.80, ExpectedResult = 1)]
		[TestCase(1, ExpectedResult = 0)]
		[Ignore("#76289 To be fixed")]
		public int ShouldConsiderPreference(double preferencePercentage)
		{
			var planningGroupSettings = PlanningGroupSettings.CreateDefault();
			planningGroupSettings.PreferenceValue = new Percent(preferencePercentage);
			PlanningGroupSettingsRepository.Add(planningGroupSettings);
			var date = new DateOnly(2015, 10, 12); 
			var activity = ActivityRepository.Has();
			var skill = SkillRepository.Has(activity);
			var planningPeriod = PlanningPeriodRepository.Has(date, 1);
			var scenario = ScenarioRepository.Has();
			var schedulePeriod = new SchedulePeriod(date, SchedulePeriodType.Week, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory().WithId()));
			var agent = PersonRepository.Has(schedulePeriod, ruleSet, skill);
			var presentShiftCategory = new ShiftCategory().WithId();
			var skillDays = SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date,
				1, 1, 2, 2, 2, 2, 2));
			PersonAssignmentRepository.Has(agent, scenario, activity, presentShiftCategory, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.GetSingle(skillDays[5].CurrentDate).SetDayOff(new DayOffTemplate()); //saturday
			PersonAssignmentRepository.GetSingle(skillDays[6].CurrentDate).SetDayOff(new DayOffTemplate()); //sunday
			var preferenceRestriction = new PreferenceRestriction {ShiftCategory = presentShiftCategory};
			PreferenceDayRepository.Has(agent, new DateOnlyPeriod(date, date.AddDays(4)), preferenceRestriction);
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);
			
			var saturdayAndSunday = PersonAssignmentRepository.LoadAll().Where(x => x.Date == date.AddDays(5) || x.Date == date.AddDays(6));
			return saturdayAndSunday.Count(x => x.DayOffTemplate == null); //number of DOs moved
		}
		
		public DayOffOptimizationPreferencesTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}