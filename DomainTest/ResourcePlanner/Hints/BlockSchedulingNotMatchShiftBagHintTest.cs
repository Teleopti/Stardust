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
	public class BlockSchedulingNotMatchShiftBagHintTest
	{
		public CheckScheduleHints Target;
		public FakePersonRepository PersonRepository;
		public FakeSkillRepository SkillRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeScenarioRepository ScenarioRepository;

		[Test]
		public void ShouldReturnShiftStartTimeNotMatchingShiftBag()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);
			var scenario = new Scenario { DefaultScenario = true };
			ScenarioRepository.Has(scenario);
			var shiftCategory = new ShiftCategory("_").WithId();
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var contract = new Contract("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(contract, new ContractScheduleWorkingMondayToFriday(), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(startDate, SchedulePeriodType.Week, 1), ruleSet, skill);

			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, planningPeriod.ToDateTimePeriod(TimeZoneInfo.Utc));

			var personAssignment = new PersonAssignment(agent, scenario, startDate.AddDays(1)).WithLayer(activity, new TimePeriod(8, 16));
			scheduleDictionary.AddPersonAssignment(personAssignment);

			var result =
				Target.Execute(new SchedulePostHintInput(scheduleDictionary, new[] { agent }, planningPeriod,
					new FixedBlockPreferenceProvider(new ExtraPreferences
					{
						UseTeamBlockOption = true,
						BlockTypeValue = BlockFinderType.BetweenDayOff,
						UseBlockSameStartTime = true
					}), false)).InvalidResources;

			result.First().ValidationErrors.Count.Should().Be.EqualTo(1);
			result.First().ValidationTypes.First().Name.Should().Be.EqualTo(nameof(BlockSchedulingNotMatchShiftBagHint));
			HintsHelper.BuildErrorMessage(result.First().ValidationErrors.First(), UserTimeZone.Make()).Should().Be.EqualTo(string.Format(Resources.StartTimeNotMatchingShiftBag, personAssignment.Period.StartDateTime, personAssignment.Date.Date,
				agent.PersonPeriodCollection.First().RuleSetBag.Description.Name));
		}

		[Test]
		public void ShouldReturnShiftCategoryNotMatchingShiftBag()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);
			var scenario = new Scenario { DefaultScenario = true };
			ScenarioRepository.Has(scenario);
			var shiftCategory = new ShiftCategory("_").WithId();
			var anotherShiftCategory = new ShiftCategory("_").WithId();
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var contract = new Contract("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(contract, new ContractScheduleWorkingMondayToFriday(), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(startDate, SchedulePeriodType.Week, 1), ruleSet, skill);

			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, planningPeriod.ToDateTimePeriod(TimeZoneInfo.Utc));

			var personAssignment = new PersonAssignment(agent, scenario, startDate.AddDays(1)).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(anotherShiftCategory);
			scheduleDictionary.AddPersonAssignment(personAssignment);

			var result =
				Target.Execute(new SchedulePostHintInput(scheduleDictionary, new[] { agent }, planningPeriod,
					new FixedBlockPreferenceProvider(new ExtraPreferences
					{
						UseTeamBlockOption = true,
						BlockTypeValue = BlockFinderType.BetweenDayOff,
						UseBlockSameShiftCategory = true
					}), false)).InvalidResources;

			result.First().ValidationErrors.Count.Should().Be.EqualTo(1);
			result.First().ValidationTypes.First().Name.Should().Be.EqualTo(nameof(BlockSchedulingNotMatchShiftBagHint));
			HintsHelper.BuildErrorMessage(result.First().ValidationErrors.First(), UserTimeZone.Make()).Should().Be.EqualTo(string.Format(Resources.ShiftCategoryNotMatchingShiftBag, personAssignment.ShiftCategory.Description.ShortName, personAssignment.Date.ToShortDateString(),
				agent.PersonPeriodCollection.First().RuleSetBag.Description.Name));
		}

		[Test]
		public void ShouldNotReturnShiftNotMatchingShiftBagWhenSameShiftIsUsed()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);
			var scenario = new Scenario { DefaultScenario = true };
			ScenarioRepository.Has(scenario);
			var shiftCategory = new ShiftCategory("_").WithId();
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var contract = new Contract("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(contract, new ContractScheduleWorkingMondayToFriday(), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(startDate, SchedulePeriodType.Week, 1), ruleSet, skill);

			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, planningPeriod.ToDateTimePeriod(TimeZoneInfo.Utc));

			var personAssignment = new PersonAssignment(agent, scenario, startDate.AddDays(1)).WithLayer(activity, new TimePeriod(8, 17)).ShiftCategory(shiftCategory);
			scheduleDictionary.AddPersonAssignment(personAssignment);

			var result =
				Target.Execute(new SchedulePostHintInput(scheduleDictionary, new[] { agent }, planningPeriod,
					new FixedBlockPreferenceProvider(new ExtraPreferences
					{
						UseTeamBlockOption = true,
						BlockTypeValue = BlockFinderType.BetweenDayOff,
						UseBlockSameShift = true
					}), false)).InvalidResources;

			result.First().ValidationErrors.Count(x => x.ErrorResource == nameof(Resources.ShiftNotMatchingShiftBag)).Should().Be.EqualTo(0);
		}
		[Test]
		public void ShouldNotThrowExceptionWhenNoShiftBagIsAssigned()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);
			var scenario = new Scenario { DefaultScenario = true };
			ScenarioRepository.Has(scenario);
			var shiftCategory = new ShiftCategory("_").WithId();
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var contract = new Contract("_");
			var agent = PersonRepository.Has(contract, new ContractScheduleWorkingMondayToFriday(), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(startDate, SchedulePeriodType.Week, 1), skill);

			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, planningPeriod.ToDateTimePeriod(TimeZoneInfo.Utc));

			var personAssignment = new PersonAssignment(agent, scenario, startDate.AddDays(1)).WithLayer(activity, new TimePeriod(8, 17)).ShiftCategory(shiftCategory);
			scheduleDictionary.AddPersonAssignment(personAssignment);

			var result =
				Target.Execute(new SchedulePostHintInput(scheduleDictionary, new[] { agent }, planningPeriod,
					new FixedBlockPreferenceProvider(new ExtraPreferences
					{
						UseTeamBlockOption = true,
						BlockTypeValue = BlockFinderType.BetweenDayOff,
						UseBlockSameShiftCategory = true
					}), false)).InvalidResources;

			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldNotCrashWhenHavingPersonAssignmentWithNoShiftCategory()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);
			var scenario = new Scenario { DefaultScenario = true };
			ScenarioRepository.Has(scenario);
			var shiftCategory = new ShiftCategory("_").WithId();
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var contract = new Contract("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(contract, new ContractScheduleWorkingMondayToFriday(), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(startDate, SchedulePeriodType.Week, 1), ruleSet, skill);

			var currentSchedule = new ScheduleDictionaryForTest(scenario, planningPeriod.ToDateTimePeriod(TimeZoneInfo.Utc));

			var personAssignmentWithoutShiftCategory = new PersonAssignment(agent, scenario, startDate.AddDays(1)).WithLayer(activity, new TimePeriod(8, 16));
			currentSchedule.AddPersonAssignment(personAssignmentWithoutShiftCategory);

			Target.Execute(new SchedulePostHintInput(currentSchedule, new[] { agent }, planningPeriod,
				new FixedBlockPreferenceProvider(new ExtraPreferences
				{
					UseTeamBlockOption = true,
					BlockTypeValue = BlockFinderType.BetweenDayOff,
					UseBlockSameShiftCategory = true
				}), false));
		}

		[Test]
		public void ShouldNotReturnHintsForHourlyEmployees()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);
			var scenario = new Scenario { DefaultScenario = true };
			ScenarioRepository.Has(scenario);
			var shiftCategory = new ShiftCategory("_").WithId();
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var contract = new Contract("_"){EmploymentType = EmploymentType.HourlyStaff};
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(contract, new ContractScheduleWorkingMondayToFriday(), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(startDate, SchedulePeriodType.Week, 1), ruleSet, skill);

			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, planningPeriod.ToDateTimePeriod(TimeZoneInfo.Utc));

			var personAssignment = new PersonAssignment(agent, scenario, startDate.AddDays(1)).WithLayer(activity, new TimePeriod(8, 16));
			scheduleDictionary.AddPersonAssignment(personAssignment);

			var result =
				Target.Execute(new SchedulePostHintInput(scheduleDictionary, new[] { agent }, planningPeriod,
					new FixedBlockPreferenceProvider(new ExtraPreferences
					{
						UseTeamBlockOption = true,
						BlockTypeValue = BlockFinderType.BetweenDayOff,
						UseBlockSameStartTime = true
					}), false)).InvalidResources;

			result.Count.Should().Be.EqualTo(0);
		}
	}
}