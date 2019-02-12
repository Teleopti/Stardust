using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourcePlanner.Hints;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.DomainTest.ResourcePlanner.Hints
{
	[DomainTest]
	[NoDefaultData]
	public class BusinessRulesHintTest : IIsolateSystem
	{
		public CheckScheduleHints Target;
		public FakePersonRepository PersonRepository;
		public FakeSkillRepository SkillRepository;
		public FakeActivityRepository ActivityRepository;

		[Test]
		public void ShouldReturnDaysOffNotFulFilled()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);
			var scenario = new Scenario();
			var shiftCategory = new ShiftCategory("_").WithId();
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var contract = new Contract("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(contract, new ContractScheduleWorkingMondayToFriday(), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(startDate, SchedulePeriodType.Week, 1), ruleSet, skill);

			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, planningPeriod.ToDateTimePeriod(TimeZoneInfo.Utc));

			scheduleDictionary.AddPersonAssignment(new PersonAssignment(agent, scenario, endDate).WithDayOff());

			var result = Target.Execute(new SchedulePostHintInput(scheduleDictionary, new[] { agent }, planningPeriod, null, 0)).InvalidResources;

			result.First().ValidationErrors.Count.Should().Be.EqualTo(1);
			result.First().ValidationTypes.First().Name.Should().Be.EqualTo(nameof(BusinessRulesHint));
			HintsHelper.BuildErrorMessage(result.First().ValidationErrors.First(), UserTimeZone.Make()).Should()
				.Be.EqualTo(string.Format(Resources.TargetDayOffNotFulfilledMessage,2));
		}

		[Test]
		public void ShouldReturnDaysOffNotFulFilledForMultipleSchedulePeriods()
		{
			var startDate = new DateOnly(2017, 01, 16);
			var endDate = new DateOnly(2017, 01, 29);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);
			var scenario = new Scenario();
			var shiftCategory = new ShiftCategory("_").WithId();
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var contract = new Contract("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(contract, new ContractScheduleWorkingMondayToFriday(), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(startDate, SchedulePeriodType.Week, 1), ruleSet, skill);

			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, planningPeriod.ToDateTimePeriod(TimeZoneInfo.Utc));

			scheduleDictionary.AddPersonAssignment(new PersonAssignment(agent, scenario, endDate).WithDayOff());

			var result = Target.Execute(new SchedulePostHintInput(scheduleDictionary, new[] { agent }, planningPeriod, null, 0)).InvalidResources;

			result.First().ValidationErrors.Count.Should().Be.EqualTo(1);
			result.First().ValidationTypes.First().Name.Should().Be.EqualTo(nameof(BusinessRulesHint));
			HintsHelper.BuildErrorMessage(result.First().ValidationErrors.First(), UserTimeZone.Make()).Should()
				.Be.EqualTo(string.Format(Resources.TargetDayOffNotFulfilledMessage, 4));
		}

		[Test]
		public void ShouldReturnContractTimeNotFulFilled()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);
			var scenario = new Scenario();
			var shiftCategory = new ShiftCategory("_").WithId();
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var contract = new Contract("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(contract, new ContractScheduleWorkingMondayToFriday(), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(startDate, SchedulePeriodType.Week, 1), ruleSet, skill);

			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, planningPeriod.ToDateTimePeriod(TimeZoneInfo.Utc));

			scheduleDictionary.AddPersonAssignment(new PersonAssignment(agent, scenario, endDate).WithDayOff());
			scheduleDictionary.AddPersonAssignment(new PersonAssignment(agent, scenario, endDate.AddDays(-1)).WithDayOff());

			var result = Target.Execute(new SchedulePostHintInput(scheduleDictionary, new[] { agent }, planningPeriod, null, 0)).InvalidResources;

			result.First().ValidationErrors.Count.Should().Be.EqualTo(1);
			result.First().ValidationTypes.First().Name.Should().Be.EqualTo(nameof(BusinessRulesHint));
			HintsHelper.BuildErrorMessage(result.First().ValidationErrors.First(), UserTimeZone.Make()).Should()
				.Be.EqualTo(string.Format(Resources.TargetScheduleTimeNotFullfilled, DateHelper.HourMinutesString(40*60)));
		}

		[Test]
		public void ShouldReturnAgentHasDayWithoutSchedule()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);
			var scenario = new Scenario();
			var shiftCategory = new ShiftCategory("_").WithId();
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var contract = new ContractWithMaximumTolerance();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(contract, new ContractScheduleWorkingMondayToFriday(), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(startDate, SchedulePeriodType.Week, 1), ruleSet, skill);

			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, planningPeriod.ToDateTimePeriod(TimeZoneInfo.Utc));

			scheduleDictionary.AddPersonAssignment(new PersonAssignment(agent, scenario, endDate).WithDayOff());

			var result = Target.Execute(new SchedulePostHintInput(scheduleDictionary, new[] { agent }, planningPeriod, null, 0)).InvalidResources;

			result.First().ValidationErrors.Count.Should().Be.EqualTo(1);
			result.First().ValidationTypes.Any(x => x.Name == nameof(BusinessRulesHint)).Should().Be.True();
			HintsHelper.BuildErrorMessage(result.First().ValidationErrors.First(), UserTimeZone.Make()).Should().Be.EqualTo(string.Format(Resources.AgentHasDaysWithoutAnySchedule, 6));
		}

		[Test]
		public void ShouldOnlyReturnMissingShiftBag()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);
			var scenario = new Scenario();
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var contract = new ContractWithMaximumTolerance();
			var agent = PersonRepository.Has(contract, new ContractScheduleWorkingMondayToFriday(), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(startDate, SchedulePeriodType.Week, 1), skill);
			
			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, planningPeriod.ToDateTimePeriod(TimeZoneInfo.Utc));

			scheduleDictionary.AddPersonAssignment(new PersonAssignment(agent, scenario, endDate).WithDayOff());

			var result = Target.Execute(new SchedulePostHintInput(scheduleDictionary, new[] { agent }, planningPeriod, null, 0)).InvalidResources;

			result.First().ValidationErrors.Count(x=>x.ErrorResource == nameof(Resources.MissingShiftBagForPeriod)).Should().Be.EqualTo(1);
			result.First().ValidationTypes.First().Name.Should().Be.EqualTo(nameof(PersonShiftBagHint));
		}

		[Test]
		public void ShouldOnlyReturnValidationForSelectedAgents()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);
			var scenario = new Scenario();
			var shiftCategory = new ShiftCategory("_").WithId();
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var contract = new Contract("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var id = Guid.NewGuid();
			var agent = PersonRepository.Has(contract, new ContractScheduleWorkingMondayToFriday(), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(startDate, SchedulePeriodType.Week, 1), ruleSet, skill).WithId(id);
			var agent2 = PersonRepository.Has(contract, new ContractScheduleWorkingMondayToFriday(), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(startDate, SchedulePeriodType.Week, 1), ruleSet, skill);

			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, planningPeriod.ToDateTimePeriod(TimeZoneInfo.Utc));

			scheduleDictionary.AddPersonAssignment(new PersonAssignment(agent, scenario, endDate).WithDayOff());
			scheduleDictionary.AddPersonAssignment(new PersonAssignment(agent2, scenario, endDate).WithDayOff());

			var result = Target.Execute(new SchedulePostHintInput(scheduleDictionary, new[] { agent }, planningPeriod, null, 0)).InvalidResources;

			result.Count.Should().Be.EqualTo(1);
			result.First().ResourceId.Should().Be.EqualTo(id);
		}

		[Test]
		public void ShouldNotReturnHintsForHourlyEmployees()
		{
			var scenario = new Scenario();
			var activity = new Activity();
			var shiftCategory = new ShiftCategory();
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);

			var person = PersonFactory.CreatePerson().WithId();
			var personPeriod = PersonPeriodFactory.CreatePersonPeriodWithSkills(startDate, new Skill("_"));
			personPeriod.PersonContract = new PersonContract(
				new Contract("_") { EmploymentType = EmploymentType.HourlyStaff }, new PartTimePercentage("_"),
				new ContractSchedule("_"));
			personPeriod.RuleSetBag = new RuleSetBag();
			person.AddPersonPeriod(personPeriod);
			person.AddSchedulePeriod(new SchedulePeriod(startDate, SchedulePeriodType.Week, 1));

			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, planningPeriod.ToDateTimePeriod(TimeZoneInfo.Utc));
			scheduleDictionary.AddPersonAssignment(new PersonAssignment(person, scenario, startDate).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(shiftCategory));

			var result = Target.Execute(new SchedulePostHintInput(scheduleDictionary, new[] { person }, planningPeriod, null, 0)).InvalidResources;

			result.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotReturnDaysOffNotFulfilledWhenFirstSchedulePeriodStartsEarlierThanPlanningPeriod()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var schedulePeriodStartDate = startDate.AddDays(-7);
			var endDate = new DateOnly(2017, 01, 29);
			var scenario = new Scenario();
			var activity = ActivityRepository.Has("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory("_").WithId()));
			var agent = PersonRepository.Has(new Contract("_"), new ContractScheduleWorkingMondayToFriday(), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(schedulePeriodStartDate, SchedulePeriodType.Week, 1), ruleSet, SkillRepository.Has("skill", activity)).WithId(Guid.NewGuid());
			
			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, new DateOnlyPeriod(schedulePeriodStartDate, endDate).ToDateTimePeriod(TimeZoneInfo.Utc));

			scheduleDictionary.AddPersonAssignment(new PersonAssignment(agent, scenario, endDate).WithDayOff());
			scheduleDictionary.AddPersonAssignment(new PersonAssignment(agent, scenario, endDate.AddDays(-1)).WithDayOff());

			var result = Target.Execute(new SchedulePostHintInput(scheduleDictionary, new[] { agent }, new DateOnlyPeriod(startDate, endDate), null, 0)).InvalidResources;
			
			result.First().ValidationErrors.Count(x =>x.ErrorResource.Equals("TargetDayOffNotFulfilledMessage")).Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotUpdateTargetTimeWhenValidatingBusinessRule()
		{
			var date = new DateOnly(2018, 10, 1);
			var planningPeriod = DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1);
			var totalPeriod = DateOnlyPeriod.CreateWithNumberOfWeeks(date, 2);
			var scenario = new Scenario();
			var shiftCategory = new ShiftCategory().WithId();
			var activity = ActivityRepository.Has();
			var skill = SkillRepository.Has(activity);
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(40), TimeSpan.FromHours(40), TimeSpan.FromHours(11), TimeSpan.FromHours(36)) };
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var schedulePeriod = new SchedulePeriod(date, SchedulePeriodType.Week, 1);
			var agent = PersonRepository.Has(contract, new ContractScheduleWorkingMondayToFriday(), new PartTimePercentage("_"), new Team { Site = new Site("_") }, schedulePeriod, ruleSet, skill);
			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, totalPeriod.ToDateTimePeriod(TimeZoneInfo.Utc));
			for (var i = 1; i < 14; i++)
			{
				scheduleDictionary.AddPersonAssignment(new[] { 5, 6, 12, 13 }.Contains(i)
					? new PersonAssignment(agent, scenario, date.AddDays(i)).WithDayOff()
					: new PersonAssignment(agent, scenario, date.AddDays(i)).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(shiftCategory));
			}

			var result = Target.Execute(new SchedulePostHintInput(scheduleDictionary, new[] { agent }, planningPeriod, null, 0)).InvalidResources;

			result.First().ValidationErrors.Count(x => x.ErrorResource.Equals("TargetScheduleTimeNotFullfilled")).Should().Be.EqualTo(1);
			scheduleDictionary[agent].CalculatedTargetTimeHolder(totalPeriod).Should().Be.EqualTo(TimeSpan.FromHours(80));
		}


		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble(new FakeScenarioRepository(ScenarioFactory.CreateScenario("Default", true, true))).For<IScenarioRepository>();
		}
	}
}
