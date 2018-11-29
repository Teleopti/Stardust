using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	public class FullSchedulingResultTest : SchedulingScenario
	{
		public FullScheduling Target;
		public FakePersonRepository PersonRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillRepository SkillRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeDayOffTemplateRepository DayOffTemplateRepository;
		public FakePersonAssignmentRepository AssignmentRepository;
		public FakePreferenceDayRepository PreferenceDayRepository;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;

		[Test]
		public void ShouldShowAgentWithoutSchedulePeriodAsNotScheduled()
		{
			DayOffTemplateRepository.Has(DayOffFactory.CreateDayOff());
			var firstDay = new DateOnly(2015, 10, 12);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory("_").WithId()));
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(0), TimeSpan.FromHours(90), TimeSpan.FromHours(8), TimeSpan.FromHours(0))
			};
			PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, null, ruleSet, skill);
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1));
			var planningPeriod = PlanningPeriodRepository.Has(firstDay, SchedulePeriodType.Day,1);
			
			var result = Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			result.ScheduledAgentsCount.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldShowAgentsWithNoScheduleAsNotScheduledEvenWithTolerance()
		{
			DayOffTemplateRepository.Has(DayOffFactory.CreateDayOff());
			var firstDay = new DateOnly(2015, 10, 12);
			var skillActivity = ActivityRepository.Has("_");
			var shiftBagActivity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", skillActivity);
			var scenario = ScenarioRepository.Has("some name");
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(shiftBagActivity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory("_").WithId()));
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(0), TimeSpan.FromHours(90), TimeSpan.FromHours(8), TimeSpan.FromHours(0)),
				WorkTime = new WorkTime(TimeSpan.FromHours(8)),
				NegativePeriodWorkTimeTolerance = TimeSpan.FromHours(40)
			};
			var agent = PersonRepository.Has(contract, ContractScheduleFactory.CreateWorkingWeekContractSchedule(), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSet, skill);
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1));
			var planningPeriod = PlanningPeriodRepository.Has(firstDay,SchedulePeriodType.Week, 1);
			
			var result = Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			var assignments = AssignmentRepository.Find(new[] { agent }, firstDay.ToDateOnlyPeriod(), scenario);
			assignments.Count(x => x.ShiftLayers.Any()).Should().Be.EqualTo(0);

			result.ScheduledAgentsCount.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldShowAgentWithinNegativeToleranceAsScheduled()
		{
			DayOffTemplateRepository.Has(DayOffFactory.CreateDayOff());
			var firstDay = new DateOnly(2015, 10, 12);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(12, 0, 12, 0, 15), new ShiftCategory("_").WithId()));
			var contract = new Contract("_")
			{
				NegativePeriodWorkTimeTolerance = TimeSpan.FromHours(4),
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(0), TimeSpan.FromHours(90), TimeSpan.FromHours(8), TimeSpan.FromHours(0))
			};
			var contractSchedule = ContractScheduleFactory.CreateWorkingWeekContractSchedule();
			var agent = PersonRepository.Has(contract, contractSchedule, new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(firstDay, SchedulePeriodType.Day, 1), ruleSet, skill);
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1));
			var period = firstDay.ToDateOnlyPeriod();
			var planningPeriod = PlanningPeriodRepository.Has(firstDay,SchedulePeriodType.Day, 1);
			
			var result = Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			var assignments = AssignmentRepository.Find(new[] { agent }, period, scenario);
			assignments.Count.Should().Be.EqualTo(1);

			result.ScheduledAgentsCount.Should().Be.EqualTo(1);
			result.BusinessRulesValidationResults.Should().Be.Empty();
		}

		[Test]
		public void ShouldShowAgentWithinPositiveToleranceAsScheduled()
		{
			DayOffTemplateRepository.Has(DayOffFactory.CreateDayOff());
			var firstDay = new DateOnly(2015, 10, 12);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(20, 0, 20, 0, 15), new ShiftCategory("_").WithId()));
			var contract = new Contract("_")
			{
				PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(4),
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(0), TimeSpan.FromHours(90), TimeSpan.FromHours(8), TimeSpan.FromHours(0))
			};
			var contractSchedule = ContractScheduleFactory.CreateWorkingWeekContractSchedule();
			var partTimePercentage = new PartTimePercentage("_");
			var team = new Team { Site = new Site("site") };
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Day, 1);
			var agent = PersonRepository.Has(contract, contractSchedule, partTimePercentage, team, schedulePeriod, ruleSet, skill);
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1));
			var period = firstDay.ToDateOnlyPeriod();
			var planningPeriod = PlanningPeriodRepository.Has(firstDay,SchedulePeriodType.Day, 1);
			
			var result = Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			var assignments = AssignmentRepository.Find(new[] { agent }, period, scenario);
			assignments.Count.Should().Be.EqualTo(1);

			result.ScheduledAgentsCount.Should().Be.EqualTo(1);
			result.BusinessRulesValidationResults.Should().Be.Empty();
		}

		[Test]
		public void ShouldShowAgentWithinNegativeDayOffToleranceAsScheduled()
		{
			DayOffTemplateRepository.Has(DayOffFactory.CreateDayOff());
			var firstDay = new DateOnly(2015, 10, 12);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory("_").WithId()));
			var contract = new Contract("_")
			{
				NegativeDayOffTolerance = 1,
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(0), TimeSpan.FromHours(90), TimeSpan.FromHours(8), TimeSpan.FromHours(0))
			};
			var contractSchedule = ContractScheduleFactory.CreateContractScheduleWithoutWorkDays("_");
			var agent = PersonRepository.Has(contract, contractSchedule, new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(firstDay, SchedulePeriodType.Day, 1), ruleSet, skill);
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1));
			var period = firstDay.ToDateOnlyPeriod();
			var planningPeriod = PlanningPeriodRepository.Has(firstDay, SchedulePeriodType.Day, 1);
			
			var result = Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			var assignments = AssignmentRepository.Find(new[] { agent }, period, scenario);
			assignments.Count.Should().Be.EqualTo(1);

			result.ScheduledAgentsCount.Should().Be.EqualTo(1);
			result.BusinessRulesValidationResults.Should().Be.Empty();
		}

		[Test]
		public void ShouldShowAgentWithinPositiveDayOffToleranceAsScheduled()
		{
			var dayOffTemplate = DayOffFactory.CreateDayOff().WithId();
			DayOffTemplateRepository.Has(dayOffTemplate);
			var firstDay = new DateOnly(2015, 10, 12);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var contract = new Contract("_")
			{
				PositiveDayOffTolerance = 1,
				NegativePeriodWorkTimeTolerance = TimeSpan.FromHours(8),
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(0), TimeSpan.FromHours(90), TimeSpan.FromHours(8), TimeSpan.FromHours(0))
			};
			
			var agent = PersonRepository.Has(contract, ContractScheduleFactory.CreateWorkingWeekContractSchedule(), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSet, skill);

			PreferenceDayRepository.Add(new PreferenceDay(agent, new DateOnly(2015, 10, 16), new PreferenceRestriction { DayOffTemplate = dayOffTemplate }).WithId());
			PreferenceDayRepository.Add(new PreferenceDay(agent, new DateOnly(2015, 10, 17), new PreferenceRestriction { DayOffTemplate = dayOffTemplate }).WithId());
			PreferenceDayRepository.Add(new PreferenceDay(agent, new DateOnly(2015, 10, 18), new PreferenceRestriction { DayOffTemplate = dayOffTemplate }).WithId());
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1));
			var endDate = new DateOnly(2015, 10, 18);
			var period = new DateOnlyPeriod(firstDay, endDate);
			var planningPeriod = PlanningPeriodRepository.Has(firstDay, SchedulePeriodType.Day, 7);
			
			var result = Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			var assignments = AssignmentRepository.Find(new[] { agent }, period, scenario);
			assignments.Count(a => a.DayOff() != null).Should().Be.EqualTo(3);
			assignments.Count(a => shiftCategory.Equals(a.ShiftCategory)).Should().Be.EqualTo(4);

			result.ScheduledAgentsCount.Should().Be.EqualTo(1);
			result.BusinessRulesValidationResults.Should().Be.Empty();
		}

		[TestCase(0, ExpectedResult = 1)]
		[TestCase(6, ExpectedResult = 0)]
		public int ShouldCountDayWithAbsenceOnTopOfContractScheduleAsScheduled(int absencePosition)
		{
			var monday = new DateOnly(2018, 10, 8);
			var planningPeriod = PlanningPeriodRepository.Has(monday, SchedulePeriodType.Day, 1);
			var scenario = ScenarioRepository.Has();
			var agent = PersonRepository.Has(new ContractScheduleWorkingMondayToFriday(), new SchedulePeriod(monday, SchedulePeriodType.Day, 1));
			var personAbsence = new PersonAbsence(agent, scenario, new AbsenceLayer(new Absence(), monday.AddDays(absencePosition).ToDateOnlyPeriod().ToDateTimePeriod(agent.PermissionInformation.DefaultTimeZone())));
			PersonAbsenceRepository.Has(personAbsence);
	
			var result = Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			return result.ScheduledAgentsCount;
		}

		public FullSchedulingResultTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}
