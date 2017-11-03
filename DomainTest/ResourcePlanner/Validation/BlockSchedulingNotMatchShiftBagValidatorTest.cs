using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourcePlanner.Validation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourcePlanner.Validation
{
	[DomainTest]
	[Toggle(Toggles.ResourcePlanner_BlockSchedulingValidation_46092)]
	public class BlockSchedulingNotMatchShiftBagValidatorTest
	{
		public SchedulingValidator Target;
		public FakePersonRepository PersonRepository;
		public FakeSkillRepository SkillRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeScenarioRepository ScenarioRepository;

		[Test, Ignore("will see if we need it")]
		public void ShouldReturnShiftStartTimeNotMatchingShiftBag()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);
			var scenario = new Scenario {DefaultScenario = true};
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
			
			var result = Target.Validate(new ValidationInput(null, new[] { agent }, planningPeriod)
			{
				BlockPreferenceProvider = new FixedBlockPreferenceProvider(new ExtraPreferences
				{
					UseTeamBlockOption = true,
					BlockTypeValue = BlockFinderType.BetweenDayOff,
					UseBlockSameStartTime = true
				}),
				Schedules = scheduleDictionary
			}).InvalidResources;

			result.First().ValidationErrors.Count.Should().Be.EqualTo(1);
			result.First().ValidationTypes.First().Name.Should().Be.EqualTo(nameof(BlockSchedulingNotMatchShiftBagValidator));
			result.First().ValidationErrors.First().Should().Be.EqualTo(string.Format(Resources.StartTimeNotMatchingShiftBag, personAssignment.Date,
				agent.PersonPeriodCollection.First().RuleSetBag.Description.Name));
		}

		[Test, Ignore("will see if we need it")]
		public void ShouldReturnShiftNotMatchingShiftBag()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);
			var scenario = new Scenario {DefaultScenario = true};
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
			
			var result = Target.Validate(new ValidationInput(null, new[] { agent }, planningPeriod)
			{
				BlockPreferenceProvider = new FixedBlockPreferenceProvider(new ExtraPreferences
				{
					UseTeamBlockOption = true,
					BlockTypeValue = BlockFinderType.BetweenDayOff,
					UseBlockSameShift = true
				}),
				Schedules = scheduleDictionary
			}).InvalidResources;

			result.First().ValidationErrors.Count.Should().Be.EqualTo(1);
			result.First().ValidationTypes.First().Name.Should().Be.EqualTo(nameof(BlockSchedulingNotMatchShiftBagValidator));
			result.First().ValidationErrors.First().Should().Be.EqualTo(string.Format(Resources.ShiftNotMatchingShiftBag, personAssignment.Date,
				agent.PersonPeriodCollection.First().RuleSetBag.Description.Name));
		}
	}
}