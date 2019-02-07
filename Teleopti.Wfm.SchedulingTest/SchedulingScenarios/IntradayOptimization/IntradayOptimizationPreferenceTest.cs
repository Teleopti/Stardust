using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Common;
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

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTest]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	[LoggedOnAppDomain]
	public class IntradayOptimizationPreferenceTest : IntradayOptimizationScenarioTest
	{
		public FakeSkillRepository SkillRepository;
		public FakePersonRepository PersonRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public IntradayOptimizationFromWeb Target;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public FakePreferenceDayRepository PreferenceDayRepository;

		[TestCase("BlockScheduling", ExpectedResult = 1)]
		[TestCase("Preferences", ExpectedResult = 2)]
		public int ShouldIntradayOptimizeWithPreferencesIfLastSchedulingFailedToScheduleWithOtherReason(string brokenHint)
		{
			var activity = ActivityFactory.CreateActivity("_");
			var skill = SkillRepository.Has("_", activity);
			var dateWithPreference = new DateOnly(2015, 10, 12);
			var dateWithoutPreference = dateWithPreference.AddDays(1);
			var scenario = ScenarioRepository.LoadDefaultScenario();
			var schedulePeriod = new SchedulePeriod(dateWithPreference, SchedulePeriodType.Week, 1);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), new ShiftCategory().WithId()));
			var agent = PersonRepository.Has(schedulePeriod, ruleSet, skill);
			var planningPeriod = PlanningPeriodRepository.Has(dateWithPreference, 1);
			var jobResult = new JobResult(JobCategory.WebSchedule, planningPeriod.Range, null, DateTime.UtcNow){FinishedOk = true}				
				.WithBroken(brokenHint, agent)
				.WithId();
			planningPeriod.JobResults.Add(jobResult);
			var preferenceRestriction = new PreferenceRestriction {ShiftCategory = shiftCategory};
			PreferenceDayRepository.Has(agent, dateWithPreference, preferenceRestriction);
			SkillDayRepository.Has(new List<ISkillDay>
			{
				skill.CreateSkillDayWithDemandPerHour(scenario, dateWithPreference, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360))),
				skill.CreateSkillDayWithDemandPerHour(scenario, dateWithoutPreference, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)))
			});
			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory, dateWithPreference, new TimePeriod(8, 0, 17, 0));
			PersonAssignmentRepository.Has(agent, scenario, activity, new ShiftCategory().WithId(), dateWithoutPreference, new TimePeriod(8, 0, 17, 0));

			Target.Execute(planningPeriod.Id.GetValueOrDefault());

			return PersonAssignmentRepository.Find(new []{agent}, planningPeriod.Range, scenario).Count(x => x.Period.StartDateTime.Minute == 15);
		}

		[Test]
		public void ShouldIntradayOptimizeWithPreferences()
		{
			var activity = ActivityFactory.CreateActivity("_");
			var skill = SkillRepository.Has("_", activity);
			var dateWithPreference = new DateOnly(2015, 10, 12);
			var dateWithoutPreference = dateWithPreference.AddDays(1);
			var scenario = ScenarioRepository.LoadDefaultScenario();
			var schedulePeriod = new SchedulePeriod(dateWithPreference, SchedulePeriodType.Week, 1);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), new ShiftCategory().WithId()));
			var agent = PersonRepository.Has(schedulePeriod, ruleSet, skill);
			var planningPeriod = PlanningPeriodRepository.Has(dateWithPreference, 1);
			var preferenceRestriction = new PreferenceRestriction {ShiftCategory = shiftCategory};
			PreferenceDayRepository.Has(agent, dateWithPreference, preferenceRestriction);
			SkillDayRepository.Has(new List<ISkillDay>
			{
				skill.CreateSkillDayWithDemandPerHour(scenario, dateWithPreference, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360))),
				skill.CreateSkillDayWithDemandPerHour(scenario, dateWithoutPreference, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)))
			});
			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory, dateWithPreference, new TimePeriod(8, 0, 17, 0));
			PersonAssignmentRepository.Has(agent, scenario, activity, new ShiftCategory().WithId(), dateWithoutPreference, new TimePeriod(8, 0, 17, 0));

			Target.Execute(planningPeriod.Id.GetValueOrDefault());

			var numberOfMovedShifts = PersonAssignmentRepository.Find(new []{agent}, planningPeriod.Range, scenario).Count(x => x.Period.StartDateTime.Minute == 15);
			numberOfMovedShifts.Should().Be.EqualTo(1);
		}
		
		[TestCase(1)]
		[TestCase(0.99)]
		public void ShouldBehaveSameWhenIntradayOptimizeWithDifferentPreferenceValues(double preferenceValue)
		{
			var activity = ActivityFactory.CreateActivity("_");
			var skill = SkillRepository.Has("_", activity);
			var date = new DateOnly(2015, 10, 12);
			var scenario = ScenarioRepository.LoadDefaultScenario();
			var schedulePeriod = new SchedulePeriod(date, SchedulePeriodType.Week, 1);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), new ShiftCategory().WithId()));
			var agent = PersonRepository.Has(schedulePeriod, ruleSet, skill);
			var planningPeriod = PlanningPeriodRepository.Has(date, 1);
			planningPeriod.PlanningGroup.SetGlobalValues(new Percent(preferenceValue));
			var preferenceRestriction = new PreferenceRestriction {ShiftCategory = new ShiftCategory()};
			PreferenceDayRepository.Has(agent, date, preferenceRestriction);
			SkillDayRepository.Has(new List<ISkillDay>
			{
				skill.CreateSkillDayWithDemandPerHour(scenario, date, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360))),
			});
			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory, date, new TimePeriod(8, 0, 17, 0));

			Target.Execute(planningPeriod.Id.GetValueOrDefault());

			PersonAssignmentRepository.GetSingle(date, agent).Period.StartDateTime.Minute 
				.Should().Be.EqualTo(15);
		}
	}
}