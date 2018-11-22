using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Exceptions;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.DomainTest.Security.ImplementationDetails.LicenseOptions;
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
		public FakePreferenceDayRepository PreferenceDayRepository;

		
		[Test]
		[Ignore("76289 to be fixed")]
		public void ShouldDayOffOptimizeAgentWitPreferencesEvenIfOtherAgentFailsToBeScheduledWithPreferences()
		{
			var date = new DateOnly(2015, 10, 12); 
			var activity = ActivityRepository.Has();
			var skill = SkillRepository.Has(activity);
			var planningPeriod = PlanningPeriodRepository.Has(date, 1);
			var scenario = ScenarioRepository.Has();
			var schedulePeriod = new SchedulePeriod(date, SchedulePeriodType.Week, 1);
			var shiftCategoryInShiftBag = new ShiftCategory().WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15),shiftCategoryInShiftBag));
			
			var agentWithIncorrectPreferences = PersonRepository.Has(schedulePeriod, ruleSet, skill);
			var agentWithCorrectPreferences = PersonRepository.Has(schedulePeriod, ruleSet, skill);
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date,1, 1, 1, 1, 2, 2, 2));
			PersonAssignmentRepository.Has(agentWithIncorrectPreferences, scenario, new DayOffTemplate(), date.AddDays(5));
			PersonAssignmentRepository.Has(agentWithIncorrectPreferences, scenario, new DayOffTemplate(), date.AddDays(6));
			PersonAssignmentRepository.Has(agentWithCorrectPreferences, scenario, new DayOffTemplate(), date.AddDays(5));
			PersonAssignmentRepository.Has(agentWithCorrectPreferences, scenario, new DayOffTemplate(), date.AddDays(6));
			var inCorrectPreferenceRestriction = new PreferenceRestriction {ShiftCategory = new ShiftCategory()};
			PreferenceDayRepository.Has(agentWithIncorrectPreferences, date, inCorrectPreferenceRestriction);
			var correctPreferenceRestriction = new PreferenceRestriction {ShiftCategory = shiftCategoryInShiftBag};
			PreferenceDayRepository.Has(agentWithCorrectPreferences, new DateOnlyPeriod(date, date.AddDays(4)), correctPreferenceRestriction);
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);
			
			var saturdayAndSunday = PersonAssignmentRepository.LoadAll().Where(x => x.Date == date.AddDays(5) || x.Date == date.AddDays(6));
			var saturdaysAndSundaysForAgentWithCorrectPreferences = saturdayAndSunday.Where(x => x.Person.Equals(agentWithCorrectPreferences));
			saturdaysAndSundaysForAgentWithCorrectPreferences.Count(x => x.DayOffTemplate == null).Should().Be.EqualTo(0); //number of DOs moved
		}
		
		[Test]
		public void ShouldDayOffOptimizeWithoutPreferenceIfNotManageToScheduleWithPreferences()
		{
			var date = new DateOnly(2015, 10, 12); 
			var activity = ActivityRepository.Has();
			var skill = SkillRepository.Has(activity);
			var planningPeriod = PlanningPeriodRepository.Has(date, 1);
			var scenario = ScenarioRepository.Has();
			var schedulePeriod = new SchedulePeriod(date, SchedulePeriodType.Week, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory().WithId()));
			var agent = PersonRepository.Has(schedulePeriod, ruleSet, skill);
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date,
				1, 1, 2, 2, 2, 2, 2));
			PersonAssignmentRepository.Has(agent, scenario, new DayOffTemplate(), date.AddDays(5));
			PersonAssignmentRepository.Has(agent, scenario, new DayOffTemplate(), date.AddDays(6));
			var preferenceRestriction = new PreferenceRestriction {ShiftCategory = new ShiftCategory()};
			PreferenceDayRepository.Has(agent, date, preferenceRestriction);
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);
			
			var saturdayAndSunday = PersonAssignmentRepository.LoadAll().Where(x => x.Date == date.AddDays(5) || x.Date == date.AddDays(6));
			saturdayAndSunday.Count(x => x.DayOffTemplate == null).Should().Be.EqualTo(2); //number of DOs moved
		}
		
		[TestCase(0.60, ExpectedResult = 2)]
		[TestCase(0.80, ExpectedResult = 1)]
		[TestCase(1, ExpectedResult = 0)]
		public int ShouldConsiderPreference(double preferencePercentage)
		{
			var date = new DateOnly(2015, 10, 12); 
			var activity = ActivityRepository.Has();
			var skill = SkillRepository.Has(activity);
			var planningPeriod = PlanningPeriodRepository.Has(date, 1);
			planningPeriod.PlanningGroup.SetGlobalValues(new Percent(preferencePercentage));
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
		
		[Test]
		public void ShouldUse100PercentAsDefault()
		{
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
			
			PersonAssignmentRepository.LoadAll().Where(x => x.Date == date.AddDays(5) || x.Date == date.AddDays(6)).All(x => x.DayOffTemplate != null)
				.Should().Be.True();
		}
		
		public DayOffOptimizationPreferencesTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}