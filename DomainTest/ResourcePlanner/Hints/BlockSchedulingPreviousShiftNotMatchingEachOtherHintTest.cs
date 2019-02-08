using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Optimization;
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
	public class BlockSchedulingPreviousShiftNotMatchingEachOtherHintTest
	{
		public CheckScheduleHints Target;
		public FakePersonRepository PersonRepository;
		public FakeSkillRepository SkillRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeScenarioRepository ScenarioRepository;

		[Test]
		public void PreviousShiftNotMatchingStartTimeForTheFirstDay()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var period = new DateOnlyPeriod(startDate.AddDays(-7), endDate);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);
			var scenario = new Scenario {DefaultScenario = true};
			ScenarioRepository.Has(scenario);
			var shiftCategory = new ShiftCategory("_").WithId();
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var contract = new Contract("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(contract, new ContractScheduleWorkingMondayToFriday(), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(startDate, SchedulePeriodType.Week, 1), ruleSet, skill);

			var currentSchedule = new ScheduleDictionaryForTest(scenario, period.ToDateTimePeriod(TimeZoneInfo.Utc));

			var personAssignment = new PersonAssignment(agent, scenario, startDate).WithLayer(activity, new TimePeriod(9, 16)).ShiftCategory(shiftCategory);
			var personAssignment2 = new PersonAssignment(agent, scenario, startDate.AddDays(-1)).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(shiftCategory);
			currentSchedule.AddPersonAssignment(personAssignment);
			currentSchedule.AddPersonAssignment(personAssignment2);

			var result =
				Target.Execute(new SchedulePostHintInput(currentSchedule, new[] {agent}, planningPeriod,
					new FixedBlockPreferenceProvider(new ExtraPreferences
					{
						UseTeamBlockOption = true,
						BlockTypeValue = BlockFinderType.BetweenDayOff,
						UseBlockSameStartTime = true
					}), 0)).InvalidResources;

			result.First().ValidationErrors.Count(x => x.ErrorResource == nameof(Resources.ExistingShiftNotMatchStartTime)).Should().Be.EqualTo(1);
			HintsHelper.BuildErrorMessage(result.First().ValidationErrors.First(), UserTimeZone.Make())
				.Should()
				.Be.EqualTo(string.Format(Resources.ExistingShiftNotMatchStartTime,
					personAssignment.Period.StartDateTime, startDate.Date,
					personAssignment2.Period.StartDateTime, startDate.AddDays(-1).Date));
		}

		[Test]
		public void PreviousShiftNotMatchingShiftForTheFirstDay()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var period = new DateOnlyPeriod(startDate.AddDays(-7), endDate);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);
			var scenario = new Scenario { DefaultScenario = true };
			ScenarioRepository.Has(scenario);
			var shiftCategory = new ShiftCategory("_").WithId();
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var contract = new Contract("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(contract, new ContractScheduleWorkingMondayToFriday(), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(startDate, SchedulePeriodType.Week, 1), ruleSet, skill);

			var currentSchedule = new ScheduleDictionaryForTest(scenario, period.ToDateTimePeriod(TimeZoneInfo.Utc));

			var personAssignment = new PersonAssignment(agent, scenario, startDate).WithLayer(activity, new TimePeriod(8, 16)).WithLayer(activity, new TimePeriod(11, 12)).ShiftCategory(shiftCategory);
			var personAssignment2 = new PersonAssignment(agent, scenario, startDate.AddDays(-1)).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(shiftCategory);
			currentSchedule.AddPersonAssignment(personAssignment);
			currentSchedule.AddPersonAssignment(personAssignment2);

			var result =
				Target.Execute(new SchedulePostHintInput(currentSchedule, new[] {agent}, planningPeriod,
					new FixedBlockPreferenceProvider(new ExtraPreferences
					{
						UseTeamBlockOption = true,
						BlockTypeValue = BlockFinderType.BetweenDayOff,
						UseBlockSameShift = true
					}), 0)).InvalidResources;

			result.First().ValidationErrors.Count(x => x.ErrorResource == nameof(Resources.ExistingShiftNotMatchShift)).Should().Be.EqualTo(1);
			HintsHelper.BuildErrorMessage(result.First().ValidationErrors.First(), UserTimeZone.Make())
				.Should().Be.EqualTo(string.Format(Resources.ExistingShiftNotMatchShift, personAssignment.Date.ToShortDateString(), startDate.AddDays(-1).ToShortDateString()));
		}

		[Test]
		public void PreviousShiftNotMatchingShiftCategoryForTheFirstDay()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var period = new DateOnlyPeriod(startDate.AddDays(-7), endDate);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);
			var scenario = new Scenario {DefaultScenario = true};
			ScenarioRepository.Has(scenario);
			var shiftCategory = new ShiftCategory("_").WithId();
			var anotherSshiftCategory = new ShiftCategory("_2").WithId();
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var contract = new Contract("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(contract, new ContractScheduleWorkingMondayToFriday(), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(startDate, SchedulePeriodType.Week, 1), ruleSet, skill);

			var currentSchedule = new ScheduleDictionaryForTest(scenario, period.ToDateTimePeriod(TimeZoneInfo.Utc));

			var personAssignment = new PersonAssignment(agent, scenario, startDate).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(shiftCategory);
			var personAssignment2 = new PersonAssignment(agent, scenario, startDate.AddDays(-1)).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(anotherSshiftCategory);
			currentSchedule.AddPersonAssignment(personAssignment);
			currentSchedule.AddPersonAssignment(personAssignment2);

			var result =
				Target.Execute(new SchedulePostHintInput(currentSchedule, new[] {agent}, planningPeriod,
					new FixedBlockPreferenceProvider(new ExtraPreferences
					{
						UseTeamBlockOption = true,
						BlockTypeValue = BlockFinderType.BetweenDayOff,
						UseBlockSameShiftCategory = true
					}), 0)).InvalidResources;

			result.First().ValidationErrors.Count.Should().Be.EqualTo(1);
			result.First().ValidationTypes.First().Name.Should().Be.EqualTo(nameof(BlockSchedulingPreviousShiftNotMatchingEachOtherHint));
			HintsHelper.BuildErrorMessage(result.First().ValidationErrors.First(), UserTimeZone.Make())
				.Should()
				.Be.EqualTo(string.Format(Resources.ExistingShiftNotMatchShiftCategory, shiftCategory.Description.ShortName,
					startDate.ToShortDateString(), anotherSshiftCategory.Description.ShortName, startDate.AddDays(-1).ToShortDateString()));
		}
		
		[Test]
		public void PreviousShiftsNotMatchingEachOthersShiftCategory()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var period = new DateOnlyPeriod(startDate.AddDays(-7), endDate);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);
			var scenario = new Scenario {DefaultScenario = true};
			ScenarioRepository.Has(scenario);
			var shiftCategory = new ShiftCategory("_").WithId();
			var anotherSshiftCategory = new ShiftCategory("_2").WithId();
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var contract = new Contract("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(contract, new ContractScheduleWorkingMondayToFriday(), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(startDate, SchedulePeriodType.Week, 1), ruleSet, skill);

			var currentSchedule = new ScheduleDictionaryForTest(scenario, period.ToDateTimePeriod(TimeZoneInfo.Utc));

			var personAssignment = new PersonAssignment(agent, scenario, startDate.AddDays(-1)).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(shiftCategory);
			var personAssignment2 = new PersonAssignment(agent, scenario, startDate.AddDays(-2)).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(anotherSshiftCategory);
			currentSchedule.AddPersonAssignment(personAssignment);
			currentSchedule.AddPersonAssignment(personAssignment2);

			var result =
				Target.Execute(new SchedulePostHintInput(currentSchedule, new[] {agent}, planningPeriod,
					new FixedBlockPreferenceProvider(new ExtraPreferences
					{
						UseTeamBlockOption = true,
						BlockTypeValue = BlockFinderType.BetweenDayOff,
						UseBlockSameShiftCategory = true
					}), 0)).InvalidResources;

			result.First().ValidationErrors.Count.Should().Be.EqualTo(1);
			result.First().ValidationTypes.First().Name.Should().Be.EqualTo(nameof(BlockSchedulingPreviousShiftNotMatchingEachOtherHint));
			HintsHelper.BuildErrorMessage(result.First().ValidationErrors.First(), UserTimeZone.Make())
				.Should()
				.Be.EqualTo(string.Format(Resources.ExistingShiftNotMatchShiftCategory, shiftCategory.Description.ShortName,
					startDate.AddDays(-1).ToShortDateString(), anotherSshiftCategory.Description.ShortName,
					startDate.AddDays(-2).ToShortDateString()));
		}

		[Test]
		public void PreviousShiftsNotMatchingEachOthersShift()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var period = new DateOnlyPeriod(startDate.AddDays(-7), endDate);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);
			var scenario = new Scenario { DefaultScenario = true };
			ScenarioRepository.Has(scenario);
			var shiftCategory = new ShiftCategory("_").WithId();
			var anotherSshiftCategory = new ShiftCategory("_2").WithId();
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var contract = new Contract("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(contract, new ContractScheduleWorkingMondayToFriday(), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(startDate, SchedulePeriodType.Week, 1), ruleSet, skill);

			var currentSchedule = new ScheduleDictionaryForTest(scenario, period.ToDateTimePeriod(TimeZoneInfo.Utc));

			var personAssignment = new PersonAssignment(agent, scenario, startDate.AddDays(-1)).WithLayer(activity, new TimePeriod(8, 16)).WithLayer(activity, new TimePeriod(11, 12)).ShiftCategory(shiftCategory);
			var personAssignment2 = new PersonAssignment(agent, scenario, startDate.AddDays(-2)).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(anotherSshiftCategory);
			currentSchedule.AddPersonAssignment(personAssignment);
			currentSchedule.AddPersonAssignment(personAssignment2);

			var result =
				Target.Execute(new SchedulePostHintInput(currentSchedule, new[] {agent}, planningPeriod,
					new FixedBlockPreferenceProvider(new ExtraPreferences
					{
						UseTeamBlockOption = true,
						BlockTypeValue = BlockFinderType.BetweenDayOff,
						UseBlockSameShift = true
					}), 0)).InvalidResources;

			result.First().ValidationErrors.Count.Should().Be.EqualTo(1);
			result.First().ValidationTypes.First().Name.Should().Be.EqualTo(nameof(BlockSchedulingPreviousShiftNotMatchingEachOtherHint));
			HintsHelper.BuildErrorMessage(result.First().ValidationErrors.First(), UserTimeZone.Make())
				.Should().Be.EqualTo(string.Format(Resources.ExistingShiftNotMatchShift, personAssignment.Date.ToShortDateString(), startDate.AddDays(-2).ToShortDateString()));
		}

		[Test]
		public void PreviousShiftsNotMatchingEachOthersStartTime()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var period = new DateOnlyPeriod(startDate.AddDays(-7), endDate);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);
			var scenario = new Scenario {DefaultScenario = true};
			ScenarioRepository.Has(scenario);
			var shiftCategory = new ShiftCategory("_").WithId();
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var contract = new Contract("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(contract, new ContractScheduleWorkingMondayToFriday(), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(startDate, SchedulePeriodType.Week, 1), ruleSet, skill);

			var currentSchedule = new ScheduleDictionaryForTest(scenario, period.ToDateTimePeriod(TimeZoneInfo.Utc));

			var personAssignment = new PersonAssignment(agent, scenario, startDate.AddDays(-1)).WithLayer(activity, new TimePeriod(9, 16));
			var personAssignment2 = new PersonAssignment(agent, scenario, startDate.AddDays(-2)).WithLayer(activity, new TimePeriod(8, 16));
			currentSchedule.AddPersonAssignment(personAssignment);
			currentSchedule.AddPersonAssignment(personAssignment2);

			var result =
				Target.Execute(new SchedulePostHintInput(currentSchedule, new[] {agent}, planningPeriod,
					new FixedBlockPreferenceProvider(new ExtraPreferences
					{
						UseTeamBlockOption = true,
						BlockTypeValue = BlockFinderType.BetweenDayOff,
						UseBlockSameStartTime = true
					}), 0)).InvalidResources;

			result.First().ValidationErrors.Count.Should().Be.EqualTo(1);
			result.First().ValidationTypes.First().Name.Should().Be.EqualTo(nameof(BlockSchedulingPreviousShiftNotMatchingEachOtherHint));
			HintsHelper.BuildErrorMessage(result.First().ValidationErrors.First(), UserTimeZone.Make())
				.Should()
				.Be.EqualTo(string.Format(Resources.ExistingShiftNotMatchStartTime,
					personAssignment.Period.StartDateTime, startDate.AddDays(-1).Date,
					personAssignment2.Period.StartDateTime, startDate.AddDays(-2).Date));
		}
		
		[Test]
		public void ShouldNotReturnValidationWhenTheFirstDayIsDayOff()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var period = new DateOnlyPeriod(startDate.AddDays(-7), endDate);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);
			var scenario = new Scenario {DefaultScenario = true};
			ScenarioRepository.Has(scenario);
			var shiftCategory = new ShiftCategory("_").WithId();
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var contract = new Contract("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(contract, new ContractScheduleWorkingMondayToFriday(), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(startDate, SchedulePeriodType.Week, 1), ruleSet, skill);

			var currentSchedule = new ScheduleDictionaryForTest(scenario, period.ToDateTimePeriod(TimeZoneInfo.Utc));

			var personAssignment = new PersonAssignment(agent, scenario, startDate).WithDayOff();
			currentSchedule.AddPersonAssignment(personAssignment);

			var result =
				Target.Execute(new SchedulePostHintInput(currentSchedule, new[] {agent}, planningPeriod,
					new FixedBlockPreferenceProvider(new ExtraPreferences
					{
						UseTeamBlockOption = true,
						BlockTypeValue = BlockFinderType.BetweenDayOff,
						UseBlockSameShiftCategory = true
					}), 0)).InvalidResources;

			result.First().ValidationTypes.Count(x=>x.Name== nameof(BlockSchedulingPreviousShiftNotMatchingEachOtherHint)).Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotCrashWhenHavingPersonAssignmentWithNoShiftCategory()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var period = new DateOnlyPeriod(startDate.AddDays(-7), endDate);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);
			var scenario = new Scenario { DefaultScenario = true };
			ScenarioRepository.Has(scenario);
			var shiftCategory = new ShiftCategory("_2").WithId();
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var contract = new Contract("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(contract, new ContractScheduleWorkingMondayToFriday(), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(startDate, SchedulePeriodType.Week, 1), ruleSet, skill);

			var currentSchedule = new ScheduleDictionaryForTest(scenario, period.ToDateTimePeriod(TimeZoneInfo.Utc));

			var personAssignmentWithoutShiftCategory = new PersonAssignment(agent, scenario, startDate).WithLayer(activity, new TimePeriod(8, 16));
			var personAssignment2 = new PersonAssignment(agent, scenario, startDate.AddDays(-1)).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(shiftCategory);
			currentSchedule.AddPersonAssignment(personAssignmentWithoutShiftCategory);
			currentSchedule.AddPersonAssignment(personAssignment2);


			Target.Execute(new SchedulePostHintInput(currentSchedule, new[] { agent }, planningPeriod,
				new FixedBlockPreferenceProvider(new ExtraPreferences
				{
					UseTeamBlockOption = true,
					BlockTypeValue = BlockFinderType.BetweenDayOff,
					UseBlockSameShiftCategory = true
				}), 0));
		}

		[Test]
		public void ShouldNotCrashWhenHavingPreviousPersonAssignmentWithNoShiftCategory()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var period = new DateOnlyPeriod(startDate.AddDays(-7), endDate);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);
			var scenario = new Scenario { DefaultScenario = true };
			ScenarioRepository.Has(scenario);
			var shiftCategory = new ShiftCategory("_2").WithId();
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var contract = new Contract("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(contract, new ContractScheduleWorkingMondayToFriday(), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(startDate, SchedulePeriodType.Week, 1), ruleSet, skill);

			var currentSchedule = new ScheduleDictionaryForTest(scenario, period.ToDateTimePeriod(TimeZoneInfo.Utc));

			var personAssignmentWithoutShiftCategory = new PersonAssignment(agent, scenario, startDate.AddDays(-1)).WithLayer(activity, new TimePeriod(8, 16));
			var personAssignment2 = new PersonAssignment(agent, scenario, startDate).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(shiftCategory);
			currentSchedule.AddPersonAssignment(personAssignmentWithoutShiftCategory);
			currentSchedule.AddPersonAssignment(personAssignment2);


			Target.Execute(new SchedulePostHintInput(currentSchedule, new[] { agent }, planningPeriod,
				new FixedBlockPreferenceProvider(new ExtraPreferences
				{
					UseTeamBlockOption = true,
					BlockTypeValue = BlockFinderType.BetweenDayOff,
					UseBlockSameShiftCategory = true
				}), 0));
		}

		[Test]
		public void ShouldNotCrashWhenPersonPeriodStartsInTheMiddleOfPlanningPeriod()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var period = new DateOnlyPeriod(startDate.AddDays(-7), endDate);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);
			var scenario = new Scenario { DefaultScenario = true };
			ScenarioRepository.Has(scenario);
			var shiftCategory = new ShiftCategory("_2").WithId();
			var activity = ActivityRepository.Has("_");
			var agent = PersonRepository.Has(new Person().WithId(), new Team { Site = new Site("site") }, startDate.AddDays(1));

			agent.PersonPeriodCollection.First().PersonContract = new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractScheduleWorkingMondayToFriday());
			var currentSchedule = new ScheduleDictionaryForTest(scenario, period.ToDateTimePeriod(TimeZoneInfo.Utc));

			var personAssignment1 = new PersonAssignment(agent, scenario, startDate.AddDays(-1)).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(shiftCategory);
			var personAssignment2 = new PersonAssignment(agent, scenario, startDate.AddDays(1)).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(shiftCategory);
			currentSchedule.AddPersonAssignment(personAssignment1);
			currentSchedule.AddPersonAssignment(personAssignment2);


			Target.Execute(new SchedulePostHintInput(currentSchedule, new[] { agent }, planningPeriod,
				new FixedBlockPreferenceProvider(new ExtraPreferences
				{
					UseTeamBlockOption = true,
					BlockTypeValue = BlockFinderType.BetweenDayOff,
					UseBlockSameShiftCategory = true
				}), 0));
		}

		[Test]
		public void ShouldNotReturnHintsForHourlyEmployees()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var period = new DateOnlyPeriod(startDate.AddDays(-7), endDate);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);
			var scenario = new Scenario { DefaultScenario = true };
			ScenarioRepository.Has(scenario);
			var shiftCategory = new ShiftCategory("_").WithId();
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var contract = new Contract("_"){EmploymentType = EmploymentType.HourlyStaff};
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(contract, new ContractScheduleWorkingMondayToFriday(), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(startDate, SchedulePeriodType.Week, 1), ruleSet, skill);

			var currentSchedule = new ScheduleDictionaryForTest(scenario, period.ToDateTimePeriod(TimeZoneInfo.Utc));

			var personAssignment = new PersonAssignment(agent, scenario, startDate).WithLayer(activity, new TimePeriod(9, 16)).ShiftCategory(shiftCategory);
			var personAssignment2 = new PersonAssignment(agent, scenario, startDate.AddDays(-1)).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(shiftCategory);
			currentSchedule.AddPersonAssignment(personAssignment);
			currentSchedule.AddPersonAssignment(personAssignment2);

			var result =
				Target.Execute(new SchedulePostHintInput(currentSchedule, new[] { agent }, planningPeriod,
					new FixedBlockPreferenceProvider(new ExtraPreferences
					{
						UseTeamBlockOption = true,
						BlockTypeValue = BlockFinderType.BetweenDayOff,
						UseBlockSameStartTime = true
					}), 0)).InvalidResources;

			result.Count().Should().Be.EqualTo(0);
		}
	}
}