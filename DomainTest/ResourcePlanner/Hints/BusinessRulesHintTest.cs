using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourcePlanner.Hints;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourcePlanner.Hints
{
	[DomainTest]
	[UseIocForFatClient]
	public class BusinessRulesHintDesktopTest
	{
		public CheckScheduleHints Target;
		public FakePersonRepository PersonRepository;
		public FakeSkillRepository SkillRepository;
		public FakeActivityRepository ActivityRepository;

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

			var result = Target.Execute(new HintInput(scheduleDictionary, new[] { agent }, new DateOnlyPeriod(startDate.AddDays(2), endDate.AddDays(-2)), null, false)).InvalidResources;

			result.First().ValidationErrors.Count.Should().Be.EqualTo(1);
			result.First().ValidationTypes.First().Name.Should().Be.EqualTo(nameof(BusinessRulesHint));
			HintsHelper.BuildErrorMessage(result.First().ValidationErrors.First()).Should()
				.Be.EqualTo(string.Format(Resources.TargetDayOffNotFulfilledMessage, 4));
		}

		[Test]
		public void ShouldNotHaveErrorsIfSuccessScheduledForSelectedDays()
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

			scheduleDictionary.AddPersonAssignment(new PersonAssignment(agent, scenario, startDate).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(shiftCategory));
			scheduleDictionary.AddPersonAssignment(new PersonAssignment(agent, scenario, endDate.AddDays(-1)).WithDayOff());
			scheduleDictionary.AddPersonAssignment(new PersonAssignment(agent, scenario, endDate).WithDayOff());

			var result = Target.Execute(new HintInput(scheduleDictionary, new[] { agent }, new DateOnlyPeriod(startDate, startDate), null, false)).InvalidResources;

			result.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnNumberOfDaysWithoutSchedule()
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

			scheduleDictionary.AddPersonAssignment(new PersonAssignment(agent, scenario, startDate).WithLayer(activity, new TimePeriod(8, 18)).ShiftCategory(shiftCategory));
			scheduleDictionary.AddPersonAssignment(new PersonAssignment(agent, scenario, startDate.AddDays(1)).WithLayer(activity, new TimePeriod(8, 18)).ShiftCategory(shiftCategory));
			scheduleDictionary.AddPersonAssignment(new PersonAssignment(agent, scenario, startDate.AddDays(2)).WithLayer(activity, new TimePeriod(8, 18)).ShiftCategory(shiftCategory));
			scheduleDictionary.AddPersonAssignment(new PersonAssignment(agent, scenario, startDate.AddDays(3)).WithLayer(activity, new TimePeriod(8, 18)).ShiftCategory(shiftCategory));
			scheduleDictionary.AddPersonAssignment(new PersonAssignment(agent, scenario, endDate.AddDays(-1)).WithDayOff());
			scheduleDictionary.AddPersonAssignment(new PersonAssignment(agent, scenario, endDate).WithDayOff());

			var result = Target.Execute(new HintInput(scheduleDictionary, new[] { agent }, new DateOnlyPeriod(startDate.AddDays(4), startDate.AddDays(4)), null, false)).InvalidResources;

			result.First().ValidationErrors.Count.Should().Be.EqualTo(1);
			result.First().ValidationTypes.First().Name.Should().Be.EqualTo(nameof(BusinessRulesHint));
			HintsHelper.BuildErrorMessage(result.First().ValidationErrors.First()).Should().Be.EqualTo(string.Format(Resources.AgentHasDaysWithoutAnySchedule, 1));
		}
	}

	[DomainTest]
	public class BusinessRulesHintTest : ISetup
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

			var result = Target.Execute(new HintInput(scheduleDictionary, new[] { agent }, planningPeriod, null, false)).InvalidResources;

			result.First().ValidationErrors.Count.Should().Be.EqualTo(1);
			result.First().ValidationTypes.First().Name.Should().Be.EqualTo(nameof(BusinessRulesHint));
			HintsHelper.BuildErrorMessage(result.First().ValidationErrors.First()).Should()
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

			var result = Target.Execute(new HintInput(scheduleDictionary, new[] { agent }, planningPeriod, null, false)).InvalidResources;

			result.First().ValidationErrors.Count.Should().Be.EqualTo(1);
			result.First().ValidationTypes.First().Name.Should().Be.EqualTo(nameof(BusinessRulesHint));
			HintsHelper.BuildErrorMessage(result.First().ValidationErrors.First()).Should()
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

			var result = Target.Execute(new HintInput(scheduleDictionary, new[] { agent }, planningPeriod, null, false)).InvalidResources;

			result.First().ValidationErrors.Count.Should().Be.EqualTo(1);
			result.First().ValidationTypes.First().Name.Should().Be.EqualTo(nameof(BusinessRulesHint));
			HintsHelper.BuildErrorMessage(result.First().ValidationErrors.First()).Should()
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

			var result = Target.Execute(new HintInput(scheduleDictionary, new[] { agent }, planningPeriod, null, false)).InvalidResources;

			result.First().ValidationErrors.Count.Should().Be.EqualTo(1);
			result.First().ValidationTypes.Any(x => x.Name == nameof(BusinessRulesHint)).Should().Be.True();
			HintsHelper.BuildErrorMessage(result.First().ValidationErrors.First()).Should().Be.EqualTo(string.Format(Resources.AgentHasDaysWithoutAnySchedule, 6));
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

			var result = Target.Execute(new HintInput(scheduleDictionary, new[] { agent }, planningPeriod, null, false)).InvalidResources;

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

			var result = Target.Execute(new HintInput(scheduleDictionary, new[] { agent }, planningPeriod, null, false)).InvalidResources;

			result.Count.Should().Be.EqualTo(1);
			result.First().ResourceId.Should().Be.EqualTo(id);
		}


		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble(new FakeScenarioRepository(ScenarioFactory.CreateScenario("Default", true, true))).For<IScenarioRepository>();
		}
	}
}
