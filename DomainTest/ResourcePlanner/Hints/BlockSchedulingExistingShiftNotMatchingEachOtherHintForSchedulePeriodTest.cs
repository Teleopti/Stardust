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
	public class BlockSchedulingExistingShiftNotMatchingEachOtherHintForSchedulePeriodTest
	{
		public CheckScheduleHints Target;
		public FakePersonRepository PersonRepository;
		public FakeSkillRepository SkillRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeScenarioRepository ScenarioRepository;

		[Test]
		public void ShouldNotAddErrorIfExistingShiftNotMatchingShiftCategoryInDifferentSchedulePeriod()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 02, 5);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);
			var scenario = new Scenario { DefaultScenario = true };
			ScenarioRepository.Has(scenario);
			var shiftCategory = new ShiftCategory("_").WithId();
			var anotherShiftCategory = new ShiftCategory("_2").WithId();
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var contract = new Contract("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var schedulePeriod = new SchedulePeriod(startDate, SchedulePeriodType.Week, 1);
			var schedulePeriod2 = new SchedulePeriod(startDate.AddDays(7), SchedulePeriodType.Week, 1);
			var agent = PersonRepository.Has(contract, new ContractScheduleWorkingMondayToFriday(), new PartTimePercentage("_"), new Team { Site = new Site("site") }, 
				schedulePeriod, ruleSet, skill);
			agent.AddSchedulePeriod(schedulePeriod2);

			var currentSchedule = new ScheduleDictionaryForTest(scenario, planningPeriod.ToDateTimePeriod(TimeZoneInfo.Utc));

			var personAssignment = new PersonAssignment(agent, scenario, startDate).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(shiftCategory);
			var personAssignment2 = new PersonAssignment(agent, scenario, startDate.AddDays(7)).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(anotherShiftCategory);
			currentSchedule.AddPersonAssignment(personAssignment);
			currentSchedule.AddPersonAssignment(personAssignment2);

			var result =
				Target.Execute(new SchedulePostHintInput(currentSchedule, new[] {agent}, planningPeriod,
					new FixedBlockPreferenceProvider(new ExtraPreferences
					{
						UseTeamBlockOption = true,
						BlockTypeValue = BlockFinderType.SchedulePeriod,
						UseBlockSameShiftCategory = true
					}), 0)).InvalidResources;


			result.First().ValidationErrors.Count(x => x.ErrorResource == nameof(Resources.ExistingShiftNotMatchShiftCategory)).Should().Be.EqualTo(0);
		}

		[Test]
		public void ExistingShiftNotMatchingShiftCategory()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);
			var scenario = new Scenario { DefaultScenario = true };
			ScenarioRepository.Has(scenario);
			var shiftCategory = new ShiftCategory("_").WithId();
			var anotherShiftCategory = new ShiftCategory("_2").WithId();
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var contract = new Contract("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(contract, new ContractScheduleWorkingMondayToFriday(), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(startDate, SchedulePeriodType.Week, 1), ruleSet, skill);

			var currentSchedule = new ScheduleDictionaryForTest(scenario, planningPeriod.ToDateTimePeriod(TimeZoneInfo.Utc));

			var personAssignment = new PersonAssignment(agent, scenario, startDate).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(shiftCategory);
			var personAssignment2 = new PersonAssignment(agent, scenario, startDate.AddDays(1)).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(anotherShiftCategory);
			currentSchedule.AddPersonAssignment(personAssignment);
			currentSchedule.AddPersonAssignment(personAssignment2);

			var result =
				Target.Execute(new SchedulePostHintInput(currentSchedule, new[] {agent}, planningPeriod,
					new FixedBlockPreferenceProvider(new ExtraPreferences
					{
						UseTeamBlockOption = true,
						BlockTypeValue = BlockFinderType.SchedulePeriod,
						UseBlockSameShiftCategory = true
					}), 0)).InvalidResources;

			result.First().ValidationErrors.Count(x => x.ErrorResource == nameof(Resources.ExistingShiftNotMatchShiftCategory)).Should().Be.EqualTo(1);
			HintsHelper.BuildErrorMessage(result.First().ValidationErrors.First(), UserTimeZone.Make())
				.Should().Be.EqualTo(string.Format(Resources.ExistingShiftNotMatchShiftCategory, shiftCategory.Description.ShortName, startDate.ToShortDateString(), anotherShiftCategory.Description.ShortName, startDate.AddDays(1).ToShortDateString()));
		}

		[Test]
		public void ShouldReturnBothValidationsTypes()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);
			var scenario = new Scenario { DefaultScenario = true };
			ScenarioRepository.Has(scenario);
			var shiftCategory = new ShiftCategory("_").WithId();
			var anotherShiftCategory = new ShiftCategory("_2").WithId();
			var activity = ActivityRepository.Has("_");
			var contract = new Contract("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(contract, new ContractScheduleWorkingMondayToFriday(), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(startDate, SchedulePeriodType.Week, 1), ruleSet);

			var currentSchedule = new ScheduleDictionaryForTest(scenario, planningPeriod.ToDateTimePeriod(TimeZoneInfo.Utc));

			var personAssignment = new PersonAssignment(agent, scenario, startDate).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(shiftCategory);
			var personAssignment2 = new PersonAssignment(agent, scenario, startDate.AddDays(1)).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(anotherShiftCategory);
			currentSchedule.AddPersonAssignment(personAssignment);
			currentSchedule.AddPersonAssignment(personAssignment2);

			var result =
				Target.Execute(new SchedulePostHintInput(currentSchedule, new[] { agent }, planningPeriod,
					new FixedBlockPreferenceProvider(new ExtraPreferences
					{
						UseTeamBlockOption = true,
						BlockTypeValue = BlockFinderType.SchedulePeriod,
						UseBlockSameShiftCategory = true
					}), 0)).InvalidResources;


			result.First().ValidationErrors.Count(x => x.ErrorResource == nameof(Resources.ExistingShiftNotMatchShiftCategory)).Should().Be.EqualTo(1);
			result.First().ValidationTypes.Count(x => x.Name == nameof(PersonSkillHint)).Should().Be.EqualTo(1);
		}

		[Test]
		public void ExistingShiftNotMatchingStartTime()
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

			var personAssignment = new PersonAssignment(agent, scenario, startDate).WithLayer(activity, new TimePeriod(9, 16)).ShiftCategory(shiftCategory);
			var personAssignment2 = new PersonAssignment(agent, scenario, startDate.AddDays(1)).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(shiftCategory);
			currentSchedule.AddPersonAssignment(personAssignment);
			currentSchedule.AddPersonAssignment(personAssignment2);

			var result =
				Target.Execute(new SchedulePostHintInput(currentSchedule, new[] {agent}, planningPeriod,
					new FixedBlockPreferenceProvider(new ExtraPreferences
					{
						UseTeamBlockOption = true,
						BlockTypeValue = BlockFinderType.SchedulePeriod,
						UseBlockSameStartTime = true
					}), 0)).InvalidResources;

			result.First().ValidationErrors.Count(x => x.ErrorResource == nameof(Resources.ExistingShiftNotMatchStartTime)).Should().Be.EqualTo(1);
			HintsHelper.BuildErrorMessage(result.First().ValidationErrors.First(), UserTimeZone.Make())
				.Should().Be.EqualTo(string.Format(Resources.ExistingShiftNotMatchStartTime, personAssignment.Period.StartDateTime, startDate.Date, personAssignment2.Period.StartDateTime, startDate.AddDays(1).Date));
		}
		
		
		[Test]
		public void ExistingShiftNotMatchingStartTimForDayLightSaving()
		{
			var startDate = new DateOnly(2018, 10, 22);
			var endDate = new DateOnly(2018, 10, 28);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);
			var scenario = new Scenario { DefaultScenario = true };
			ScenarioRepository.Has(scenario);
			var shiftCategory = new ShiftCategory("_").WithId();
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var contract = new Contract("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(contract, new ContractScheduleWorkingMondayToFriday(), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(startDate, SchedulePeriodType.Week, 1), ruleSet, skill);
			agent.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			var currentSchedule = new ScheduleDictionaryForTest(scenario, planningPeriod.ToDateTimePeriod(TimeZoneInfo.Utc));

			var personAssignment = new PersonAssignment(agent, scenario, startDate).WithLayer(activity, new TimePeriod(9, 16)).ShiftCategory(shiftCategory);
			var personAssignment2 = new PersonAssignment(agent, scenario, endDate).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(shiftCategory);
			currentSchedule.AddPersonAssignment(personAssignment);
			currentSchedule.AddPersonAssignment(personAssignment2);

			var result =
				Target.Execute(new SchedulePostHintInput(currentSchedule, new[] {agent}, planningPeriod,
					new FixedBlockPreferenceProvider(new ExtraPreferences
					{
						UseTeamBlockOption = true,
						BlockTypeValue = BlockFinderType.SchedulePeriod,
						UseBlockSameStartTime = true
					}), 0)).InvalidResources;

			result.First().ValidationErrors.Count(x => x.ErrorResource == nameof(Resources.ExistingShiftNotMatchStartTime)).Should().Be.EqualTo(1);
			HintsHelper.BuildErrorMessage(result.First().ValidationErrors.First(), UserTimeZone.Make())
				.Should().Be.EqualTo(string.Format(Resources.ExistingShiftNotMatchStartTime, personAssignment.Period.StartDateTime, startDate.Date, personAssignment2.Period.StartDateTime, endDate.Date));
		}

		[Test]
		public void ExistingShiftNotMatchingShift()
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

			var personAssignment =
				new PersonAssignment(agent, scenario, startDate).WithLayer(activity, new TimePeriod(8, 16)).WithLayer(activity, new TimePeriod(11, 12)).ShiftCategory(shiftCategory);
			var personAssignment2 = new PersonAssignment(agent, scenario, startDate.AddDays(1)).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(shiftCategory);
			currentSchedule.AddPersonAssignment(personAssignment);
			currentSchedule.AddPersonAssignment(personAssignment2);

			var result =
				Target.Execute(new SchedulePostHintInput(currentSchedule, new[] {agent}, planningPeriod,
					new FixedBlockPreferenceProvider(new ExtraPreferences
					{
						UseTeamBlockOption = true,
						BlockTypeValue = BlockFinderType.SchedulePeriod,
						UseBlockSameShift = true
					}), 0)).InvalidResources;

			result.First().ValidationErrors.Count(x => x.ErrorResource == nameof(Resources.ExistingShiftNotMatchShift)).Should().Be.EqualTo(1);
			HintsHelper.BuildErrorMessage(result.First().ValidationErrors.First(), UserTimeZone.Make())
				.Should().Be.EqualTo(string.Format(Resources.ExistingShiftNotMatchShift, startDate.ToShortDateString(), startDate.AddDays(1).ToShortDateString()));
		}

		[Test]
		public void ExistingShiftsMatchingEachotherShouldNotReturnHint()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 24);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);
			var scenario = new Scenario { DefaultScenario = true };
			ScenarioRepository.Has(scenario);
			var shiftCategory = new ShiftCategory("_").WithId();
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var contract = new Contract("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(contract, new ContractScheduleWorkingMondayToFriday(), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(startDate, SchedulePeriodType.Day, 2), ruleSet, skill);

			var currentSchedule = new ScheduleDictionaryForTest(scenario, planningPeriod.ToDateTimePeriod(TimeZoneInfo.Utc));

			var personAssignment =
				new PersonAssignment(agent, scenario, startDate).WithLayer(activity, new TimePeriod(8, 16)).WithLayer(activity, new TimePeriod(11, 12)).ShiftCategory(shiftCategory);
			var personAssignment2 =
				new PersonAssignment(agent, scenario, startDate.AddDays(1)).WithLayer(activity, new TimePeriod(8, 16)).WithLayer(activity, new TimePeriod(11, 12)).ShiftCategory(shiftCategory);
			currentSchedule.AddPersonAssignment(personAssignment);
			currentSchedule.AddPersonAssignment(personAssignment2);

			var result =
				Target.Execute(new SchedulePostHintInput(currentSchedule, new[] { agent }, planningPeriod,
					new FixedBlockPreferenceProvider(new ExtraPreferences
					{
						UseTeamBlockOption = true,
						BlockTypeValue = BlockFinderType.SchedulePeriod,
						UseBlockSameShift = true
					}), 0)).InvalidResources;

			result.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ExistingShiftAndEmptyShiftShouldNotReturnHint()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 24);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);
			var scenario = new Scenario { DefaultScenario = true };
			ScenarioRepository.Has(scenario);
			var shiftCategory = new ShiftCategory("_").WithId();
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var contract = new Contract("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(contract, new ContractScheduleWorkingMondayToFriday(), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(startDate, SchedulePeriodType.Day, 2), ruleSet, skill);

			var currentSchedule = new ScheduleDictionaryForTest(scenario, planningPeriod.ToDateTimePeriod(TimeZoneInfo.Utc));

			var personAssignment =
				new PersonAssignment(agent, scenario, startDate).WithLayer(activity, new TimePeriod(8, 16)).WithLayer(activity, new TimePeriod(11, 12)).ShiftCategory(shiftCategory);
			currentSchedule.AddPersonAssignment(personAssignment);

			var result =
				Target.Execute(new SchedulePostHintInput(currentSchedule, new[] { agent }, planningPeriod,
					new FixedBlockPreferenceProvider(new ExtraPreferences
					{
						UseTeamBlockOption = true,
						BlockTypeValue = BlockFinderType.SchedulePeriod,
						UseBlockSameShift = true
					}), 0)).InvalidResources;

			result.First().ValidationErrors.Count(x => x.ErrorResource == nameof(Resources.ExistingShiftNotMatchShift)).Should().Be.EqualTo(0);
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

			var personAssignment = new PersonAssignment(agent, scenario, startDate).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(shiftCategory);
			var personAssignmentWithoutShiftCategory = new PersonAssignment(agent, scenario, startDate.AddDays(1)).WithLayer(activity, new TimePeriod(8, 16));
			currentSchedule.AddPersonAssignment(personAssignment);
			currentSchedule.AddPersonAssignment(personAssignmentWithoutShiftCategory);

			Target.Execute(new SchedulePostHintInput(currentSchedule, new[] { agent }, planningPeriod,
				new FixedBlockPreferenceProvider(new ExtraPreferences
				{
					UseTeamBlockOption = true,
					BlockTypeValue = BlockFinderType.SchedulePeriod,
					UseBlockSameShiftCategory = true
				}), 0));
		}

		[Test]
		[Timeout(8000)]
		public void ShouldNotHangWhenHaveLeavingDate()
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
			agent.TerminatePerson(startDate, new PersonAccountUpdaterDummy());

			var currentSchedule = new ScheduleDictionaryForTest(scenario, planningPeriod.ToDateTimePeriod(TimeZoneInfo.Utc));
			var personAssignment = new PersonAssignment(agent, scenario, startDate).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(shiftCategory);
			currentSchedule.AddPersonAssignment(personAssignment);

			Target.Execute(new SchedulePostHintInput(currentSchedule, new[] { agent }, planningPeriod,
				new FixedBlockPreferenceProvider(new ExtraPreferences
				{
					UseTeamBlockOption = true,
					BlockTypeValue = BlockFinderType.SchedulePeriod,
					UseBlockSameShiftCategory = true
				}), 0));
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
			var anotherShiftCategory = new ShiftCategory("_2").WithId();
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var contract = new Contract("_"){EmploymentType = EmploymentType.HourlyStaff};
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(contract, new ContractScheduleWorkingMondayToFriday(), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(startDate, SchedulePeriodType.Week, 1), ruleSet, skill);

			var currentSchedule = new ScheduleDictionaryForTest(scenario, planningPeriod.ToDateTimePeriod(TimeZoneInfo.Utc));

			var personAssignment = new PersonAssignment(agent, scenario, startDate).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(shiftCategory);
			var personAssignment2 = new PersonAssignment(agent, scenario, startDate.AddDays(1)).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(anotherShiftCategory);
			currentSchedule.AddPersonAssignment(personAssignment);
			currentSchedule.AddPersonAssignment(personAssignment2);

			var result =
				Target.Execute(new SchedulePostHintInput(currentSchedule, new[] { agent }, planningPeriod,
					new FixedBlockPreferenceProvider(new ExtraPreferences
					{
						UseTeamBlockOption = true,
						BlockTypeValue = BlockFinderType.SchedulePeriod,
						UseBlockSameShiftCategory = true
					}), 0)).InvalidResources;

			result.Should().Be.Empty();
		}
	}
}