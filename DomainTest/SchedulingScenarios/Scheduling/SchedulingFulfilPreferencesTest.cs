﻿using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourcePlanner.Hints;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	public class SchedulingFulfilPreferencesTest : SchedulingScenario
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
		public FakePreferenceDayRepository PreferenceDayRepository;
		
		[Test]
		public void ShouldScheduleWithoutPreferencesIfPreferencesCannotBeFulfilled()
		{
			DayOffTemplateRepository.Has(DayOffFactory.CreateDayOff());
			var date = new DateOnly(2015, 10, 12); //mon;
			var planningPeriod = PlanningPeriodRepository.Has(date, 1);
			var activity = ActivityRepository.Has();
			var skill = SkillRepository.Has("_", activity);
			var scenario = ScenarioRepository.Has("_");
			var shiftCategory = new ShiftCategory().WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agentToSchedule = PersonRepository.Has(new SchedulePeriod(date, SchedulePeriodType.Week, 1), ruleSet, skill);
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnlyPeriod(date, date.AddDays(6)), 1)); 
			var preferenceRestriction = new PreferenceRestriction {ShiftCategory = new ShiftCategory()};
			PreferenceDayRepository.Add(new PreferenceDay(agentToSchedule, date, preferenceRestriction));

			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			AssignmentRepository.LoadAll().Count().Should().Be.EqualTo(7);
		}
		
		[Test]
		public void ShouldScheduleWithoutPreferencesIfSomePreferencesCannotBeFulfilled()
		{
			DayOffTemplateRepository.Has(DayOffFactory.CreateDayOff());
			var date = new DateOnly(2015, 10, 12); //mon;
			var planningPeriod = PlanningPeriodRepository.Has(date, 1);
			var activity = ActivityRepository.Has();
			var skill = SkillRepository.Has("_", activity);
			var scenario = ScenarioRepository.Has("_");
			var shiftCategory10H = new ShiftCategory().WithId();
			var shiftCategory8H = new ShiftCategory().WithId();
			var ruleSet8H = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory8H));
			var ruleSet10H = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(18, 0, 18, 0, 15), shiftCategory10H));
			var shiftBag = new RuleSetBag(ruleSet8H, ruleSet10H);
			var agentToSchedule = PersonRepository.Has(new Contract("_"),new ContractScheduleWorkingMondayToFriday(), new PartTimePercentage("_"),new Team(), new SchedulePeriod(date, SchedulePeriodType.Week, 1), shiftBag, skill);
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnlyPeriod(date, date.AddDays(6)), 1)); 
			var preferenceRestriction = new PreferenceRestriction {ShiftCategory = shiftCategory10H};
			PreferenceDayRepository.Add(new PreferenceDay(agentToSchedule, date, preferenceRestriction));

			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			AssignmentRepository.LoadAll().Count().Should().Be.EqualTo(7);
		}
		
		[TestCase(true, ExpectedResult = true)]
		[TestCase(false, ExpectedResult = false)]
		public bool ShouldGiveHintForAgentsScheduledWithoutPreference(bool blockedByPreference)
		{
			DayOffTemplateRepository.Has(DayOffFactory.CreateDayOff());
			var date = new DateOnly(2015, 10, 12); //mon;
			var planningPeriod = PlanningPeriodRepository.Has(date, 1);
			var activity = ActivityRepository.Has();
			var skill = SkillRepository.Has("_", activity);
			var scenario = ScenarioRepository.Has();
			var shiftCategoryInRuleSet = new ShiftCategory().WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategoryInRuleSet));
			var agentToSchedule = PersonRepository.Has(new SchedulePeriod(date, SchedulePeriodType.Week, 1), ruleSet, skill);
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), 1));
			var shiftCategoryInPref = blockedByPreference ? new ShiftCategory() : shiftCategoryInRuleSet;
			var preferenceRestriction = new PreferenceRestriction {ShiftCategory = shiftCategoryInPref};
			PreferenceDayRepository.Add(new PreferenceDay(agentToSchedule, date, preferenceRestriction));

			var scheduleResult = Target.DoSchedulingAndDO(planningPeriod.Id.Value, false);

			return scheduleResult.BusinessRulesValidationResults.SelectMany(x => x.ValidationErrors)
				.Select(x => x.ResourceType).Contains(ValidationResourceType.Preferences);
		}
		
		[Test]
		public void ShouldDeleteBeforeRescheduleWhenPreferencesCantBeFulfilled()
		{
			DayOffTemplateRepository.Has(DayOffFactory.CreateDayOff());
			var date = new DateOnly(2015, 10, 12); //mon;
			var planningPeriod = PlanningPeriodRepository.Has(date, 1);
			var activity = ActivityRepository.Has();
			var skill = SkillRepository.Has("_", activity);
			var scenario = ScenarioRepository.Has("_");
			var shiftCategory = new ShiftCategory().WithId();
			var ruleSet8H = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var ruleSet10H = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(18, 0, 18, 0, 15), shiftCategory));
			var shiftBag = new RuleSetBag(ruleSet8H, ruleSet10H);
			var contract = new Contract("_")
			{
				NegativeDayOffTolerance = 0,
				PositiveDayOffTolerance = 0,
				NegativePeriodWorkTimeTolerance = new TimeSpan(0, 0, 0),
				PositivePeriodWorkTimeTolerance = new TimeSpan(0, 0, 0)
			};

			var agentToSchedule = PersonRepository.Has(contract,new ContractScheduleWorkingMondayToFriday(), new PartTimePercentage("_"),new Team(), new SchedulePeriod(date, SchedulePeriodType.Week, 1), shiftBag, skill);
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnlyPeriod(date, date.AddDays(6)), 1)); 
			var preferenceRestriction = new PreferenceRestriction {ShiftCategory = new ShiftCategory().WithId()};
			PreferenceDayRepository.Add(new PreferenceDay(agentToSchedule, date, preferenceRestriction));

			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			AssignmentRepository.LoadAll().Count().Should().Be.EqualTo(7);
		}
		
		[Test]
		public void ShouldNotDeleteAlreadyExistingScheduleWhenPreferencesCantBeFulfilled()
		{
			DayOffTemplateRepository.Has(DayOffFactory.CreateDayOff());
			var date = new DateOnly(2015, 10, 12); //mon;
			var planningPeriod = PlanningPeriodRepository.Has(date, 1);
			var activity = ActivityRepository.Has();
			var skill = SkillRepository.Has("_", activity);
			var scenario = ScenarioRepository.Has("_");
			var shiftCategory = new ShiftCategory().WithId();
			var ruleSet8H = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var shiftBag = new RuleSetBag(ruleSet8H);
			var agentToSchedule = PersonRepository.Has(new SchedulePeriod(date, SchedulePeriodType.Week, 1), shiftBag, skill);
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnlyPeriod(date, date.AddDays(6)), 1)); 
			var preferenceRestriction = new PreferenceRestriction {ShiftCategory = new ShiftCategory().WithId()};
			PreferenceDayRepository.Add(new PreferenceDay(agentToSchedule, date, preferenceRestriction));
			AssignmentRepository.Has(agentToSchedule, scenario, activity, new ShiftCategory(), date.AddDays(1), new TimePeriod(10, 18));
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			AssignmentRepository.GetSingle(date.AddDays(1), agentToSchedule).Period.StartDateTime.Hour
				.Should().Be.EqualTo(10);
		}


		
		public SchedulingFulfilPreferencesTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
			if (!ResourcePlannerTestParameters.IsEnabled(Toggles.ResourcePlanner_SeamlessPlanningForPreferences_76288))
			{
				Assert.Ignore("only works with toggle on");
			}
		}
	}
}