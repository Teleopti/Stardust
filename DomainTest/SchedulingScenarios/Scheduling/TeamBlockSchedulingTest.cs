using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.Filters;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	/*  DONT ADD MORE TESTS HERE! - LEGACY TESTS HERE!
	 *  Web supports (limited) block (not team) scheduling only.
	 *  If you want to add simple (web) block scheduling tests that PlanningGroup supports, add it to SchedulingBlockTest
	 *  If you want to add a team scheduling tests, add it as a desktop test (until web supports it)
	 */
	[DomainTest]
	public class TeamBlockSchedulingTest : SchedulingScenario, IIsolateSystem
	{
		public FullScheduling Target;
		public FakePersonRepository PersonRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillRepository SkillRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public FakePersonAssignmentRepository AssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeDayOffTemplateRepository DayOffTemplateRepository;
		public SchedulingOptionsProvider SchedulingOptionsProvider;
		public FakePreferenceDayRepository PreferenceDayRepository;
		public FakeRuleSetBagRepository RuleSetBagRepository;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public FakePersonRotationRepository PersonRotationRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakePlanningGroupRepository PlanningGroupRepository;

		[Test]
		public void ShouldHandleTeamUsingShiftOverMidnight()
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = ActivityRepository.Has();
			var skill = SkillRepository.Has(activity);
			var scenario = ScenarioRepository.Has();
			var team = new Team().WithId();
			var dayOffTemplate = new DayOffTemplate(new Description()).WithId();
			dayOffTemplate.SetTargetAndFlexibility(TimeSpan.FromHours(24), TimeSpan.FromHours(8));
			dayOffTemplate.Anchor = TimeSpan.FromHours(12);
			DayOffTemplateRepository.Add(dayOffTemplate);
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateBusinessUnitAndAppend(team).WithId(ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value));
			var contractSchedule = ContractScheduleFactory.CreateWorkingWeekContractSchedule();
			var shiftCategory = new ShiftCategory().WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(19, 0, 19, 0, 15), new TimePeriodWithSegment(27, 0, 27, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(new ContractWithMaximumTolerance(), contractSchedule, new PartTimePercentage("_"), team, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSet, skill);
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1, 1, 1));
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate, SchedulePeriodType.Week, 1);
			SchedulingOptionsProvider.SetFromTest_LegacyDONOTUSE(planningPeriod, new SchedulingOptions
			{
				DayOffTemplate =  dayOffTemplate,
				GroupOnGroupPageForTeamBlockPer = new GroupPageLight(UserTexts.Resources.Main, GroupPageType.Hierarchy),
				UseTeam = true,
				TeamSameShiftCategory = true
			});

			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			AssignmentRepository.Find(new[] { agent }, period, scenario).Count(personAssignment => personAssignment.MainActivities().Any()).Should().Be.EqualTo(5);
		}

		[Test]
		public void ShouldHandleTeamUsingShiftOverMidnightMultipleSkills()
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = ActivityRepository.Has();
			var skill1 = SkillRepository.Has("A", activity, new TimePeriod(8, 24));
			var skill2 = SkillRepository.Has("B", activity);
			var scenario = ScenarioRepository.Has();
			var team = new Team().WithId();
			var dayOffTemplate = new DayOffTemplate(new Description()).WithId();
			dayOffTemplate.SetTargetAndFlexibility(TimeSpan.FromHours(24), TimeSpan.FromHours(8));
			dayOffTemplate.Anchor = TimeSpan.FromHours(12);
			DayOffTemplateRepository.Add(dayOffTemplate);
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateBusinessUnitAndAppend(team).WithId(ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value));
			var contractSchedule = ContractScheduleFactory.CreateWorkingWeekContractSchedule();
			var shiftCategory = new ShiftCategory().WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(19, 0, 19, 0, 15), new TimePeriodWithSegment(27, 0, 27, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(new ContractWithMaximumTolerance(), contractSchedule, new PartTimePercentage("_"), team, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSet, skill1, skill2);
			SkillDayRepository.Has(skill1.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1, 1, 1));
			SkillDayRepository.Has(skill2.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1, 1, 1));
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate, SchedulePeriodType.Week, 1);
			SchedulingOptionsProvider.SetFromTest_LegacyDONOTUSE(planningPeriod, new SchedulingOptions
			{
				DayOffTemplate = dayOffTemplate,
				GroupOnGroupPageForTeamBlockPer = new GroupPageLight(UserTexts.Resources.Main, GroupPageType.Hierarchy),
				UseTeam = true,
				TeamSameShiftCategory = true
			});

			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			AssignmentRepository.Find(new[] { agent }, period, scenario).Count(personAssignment => personAssignment.MainActivities().Any()).Should().Be.EqualTo(5);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void ShouldHandleMixOfTeamAndBlockAndNotClearToMuch_BetweenDayOffs(bool reversedAgentOrder)
		{
			var firstDay = new DateOnly(2016, 05, 30);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			var shiftCategory = new ShiftCategory("_").WithId();
			var otherShiftCategory = new ShiftCategory("other").WithId();
			var ruleSetBag = new RuleSetBag(new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory))) { Description = new Description("_") }.WithId();
			RuleSetBagRepository.Add(ruleSetBag);
			var contract = new ContractWithMaximumTolerance().WithNoDayOffTolerance();
			var agent1 = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team(), new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSetBag, skill);
			var agent2 = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team(), new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSetBag, skill);
			if (reversedAgentOrder)
				PersonRepository.ReversedOrder();
			DayOffTemplateRepository.Add(new DayOffTemplate());
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 2, 2, 2, 2, 2, 2, 2));
			AssignmentRepository.Has(agent1, scenario, new DayOffTemplate(), firstDay.AddDays(2));
			AssignmentRepository.Has(agent2, scenario, new DayOffTemplate(), firstDay.AddDays(2));
			AssignmentRepository.Has(agent1, scenario, new DayOffTemplate(), firstDay.AddDays(4));
			AssignmentRepository.Has(agent2, scenario, new DayOffTemplate(), firstDay.AddDays(5));
			AssignmentRepository.Has(agent2, scenario, activity, otherShiftCategory, firstDay.AddDays(4), new TimePeriod(8, 16));
			AssignmentRepository.Has(agent1, scenario, activity, otherShiftCategory, firstDay, new TimePeriod(8, 16));
			AssignmentRepository.Has(agent1, scenario, activity, otherShiftCategory, firstDay.AddDays(1), new TimePeriod(8, 16));
			AssignmentRepository.Has(agent2, scenario, activity, otherShiftCategory, firstDay, new TimePeriod(8, 16));
			AssignmentRepository.Has(agent2, scenario, activity, otherShiftCategory, firstDay.AddDays(1), new TimePeriod(8, 16));
			var planningPeriod = PlanningPeriodRepository.Has(firstDay,SchedulePeriodType.Day, 5);
			SchedulingOptionsProvider.SetFromTest_LegacyDONOTUSE(planningPeriod, new SchedulingOptions
			{
				UseTeam = true,
				GroupOnGroupPageForTeamBlockPer = new GroupPageLight("_", GroupPageType.RuleSetBag),
				UseBlock = true,
				BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff,
				TeamSameShiftCategory = true,
				BlockSameShiftCategory = true
			});
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			AssignmentRepository.LoadAll()
				.Count(x => otherShiftCategory.Equals(x.ShiftCategory))
				.Should()
				.Be.GreaterThanOrEqualTo(4); //<--
		}

		[Test]
		public void TeamBlockSchedulingShouldNotUseShiftsMarkedForRestrictionOnlyWhenThereIsNoRestriction()
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			var team = new Team().WithDescription(new Description("team1"));
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var contract = new Contract("_");
			var contractSchedule = ContractScheduleFactory.CreateWorkingWeekContractSchedule();
			var agent1 = PersonRepository.Has(contract, contractSchedule, new PartTimePercentage("_"), team, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), skill);
			var agent2 = PersonRepository.Has(contract, contractSchedule, new PartTimePercentage("_"), team, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), skill);
			var shiftCategory = new ShiftCategory("_").WithId();
			var restrictedRuleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory))
			{ OnlyForRestrictions = true };
			var normalRuleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(12, 0, 12, 0, 15), new TimePeriodWithSegment(20, 0, 20, 0, 15), shiftCategory));
			var ruleSetBag = new RuleSetBag(restrictedRuleSet);
			ruleSetBag.AddRuleSet(normalRuleSet);
			agent1.Period(firstDay).RuleSetBag = ruleSetBag;
			agent2.Period(firstDay).RuleSetBag = ruleSetBag;
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				1, 1, 1, 1, 1, 1, 1));
			var dayOffTemplate = new DayOffTemplate(new Description("_default")).WithId();
			DayOffTemplateRepository.Add(dayOffTemplate);
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate,SchedulePeriodType.Week, 1);
			SchedulingOptionsProvider.SetFromTest_LegacyDONOTUSE(planningPeriod, new SchedulingOptions
			{
				UseAvailability = true,
				UsePreferences = true,
				UseRotations = true,
				UseStudentAvailability = false,
				DayOffTemplate = dayOffTemplate,
				ScheduleEmploymentType = ScheduleEmploymentType.FixedStaff,
				GroupOnGroupPageForTeamBlockPer = new GroupPageLight(UserTexts.Resources.Main, GroupPageType.Hierarchy),
				UseTeam = true,
				TeamSameShiftCategory = true,
				TagToUseOnScheduling = NullScheduleTag.Instance
			});
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			var assignments = AssignmentRepository.Find(new[] { agent1, agent2 }, period, scenario);
			assignments.Count.Should().Be.EqualTo(14);

			foreach (var personAssignment in assignments)
			{
				if (personAssignment.DayOff() != null)
					continue;

				personAssignment.MainActivities()
					.First()
					.Period.StartDateTimeLocal(personAssignment.Person.PermissionInformation.DefaultTimeZone())
					.TimeOfDay
					.Should()
					.Be.EqualTo(TimeSpan.FromHours(12)); //early shifts are not allowed
			}
		}

		[Test]
		public void TeamBlockSchedulingShouldNotUseShiftsMarkedForRestrictionOnlyWhenThereIsNoRestrictionOnSingleAgentTeams()
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var period = new DateOnlyPeriod(firstDay, firstDay.AddDays(6)); //12 to 18
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var team = new Team().WithDescription(new Description("team1"));
			var contract = new Contract("_");
			var contractSchedule = ContractScheduleFactory.CreateWorkingWeekContractSchedule();
			var agent1 = PersonRepository.Has(contract, contractSchedule, new PartTimePercentage("_"), team, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), skill);
			var agent2 = PersonRepository.Has(contract, contractSchedule, new PartTimePercentage("_"), team, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), skill);
			var shiftCategory = new ShiftCategory("_").WithId();
			var restrictedRuleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var normalRuleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(12, 0, 12, 0, 15), new TimePeriodWithSegment(20, 0, 20, 0, 15), shiftCategory));
			restrictedRuleSet.OnlyForRestrictions = true;
			var ruleSetBag = new RuleSetBag(restrictedRuleSet);
			ruleSetBag.AddRuleSet(normalRuleSet);
			agent1.Period(firstDay).RuleSetBag = ruleSetBag;
			agent2.Period(firstDay).RuleSetBag = ruleSetBag;
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				1, 1, 1, 1, 1, 1, 1));
			var dayOffTemplate = new DayOffTemplate(new Description("_default")).WithId();
			DayOffTemplateRepository.Add(dayOffTemplate);
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate,SchedulePeriodType.Week, 1);
			SchedulingOptionsProvider.SetFromTest_LegacyDONOTUSE(planningPeriod, new SchedulingOptions
			{
				UseAvailability = true,
				UsePreferences = true,
				UseRotations = true,
				UseStudentAvailability = false,
				DayOffTemplate = dayOffTemplate,
				ScheduleEmploymentType = ScheduleEmploymentType.FixedStaff,
				GroupOnGroupPageForTeamBlockPer = new GroupPageLight(UserTexts.Resources.Main, GroupPageType.Hierarchy),
				UseTeam = true,
				TeamSameShiftCategory = true,
				TagToUseOnScheduling = NullScheduleTag.Instance
			});
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			var assignments = AssignmentRepository.Find(new[] { agent1, agent2 }, period, scenario);
			assignments.Count.Should().Be.EqualTo(14);

			foreach (var personAssignment in assignments)
			{
				if (personAssignment.AssignedWithDayOff(dayOffTemplate))
					continue;

				personAssignment.MainActivities()
					.First()
					.Period.StartDateTimeLocal(personAssignment.Person.PermissionInformation.DefaultTimeZone())
					.TimeOfDay
					.Should()
					.Be.EqualTo(TimeSpan.FromHours(12)); //early shifts are not allowed
			}
		}

		[Test, Ignore("#40904")]
		public void ShouldNotPlaceShiftThatDoesntMatchAllGroupsInvolvedSkillsOpenHour()
		{
			var date = DateOnly.Today;
			var activity = ActivityRepository.Has("_");
			var skill1 = SkillRepository.Has("skill open only during lunch", activity, new TimePeriod(12, 0, 13, 0));
			var skill2 = SkillRepository.Has("open skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			var agent1 = PersonRepository.Has(new Contract("_"), new SchedulePeriod(date, SchedulePeriodType.Day, 1), skill1);
			var agent2 = PersonRepository.Has(new Contract("_"), new SchedulePeriod(date, SchedulePeriodType.Day, 1), skill1, skill2);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSetBag = new RuleSetBag(new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory))) { Description = new Description("_") };
			RuleSetBagRepository.Has(ruleSetBag);
			agent1.Period(date).RuleSetBag = ruleSetBag;
			agent2.Period(date).RuleSetBag = ruleSetBag;
			SkillDayRepository.Has(skill1.CreateSkillDayWithDemand(scenario, date, 10), skill2.CreateSkillDayWithDemand(scenario, date, 10));
			var dayOffTemplate = new DayOffTemplate(new Description("_")).WithId();
			DayOffTemplateRepository.Add(dayOffTemplate);
			var planningPeriod = PlanningPeriodRepository.Has(date,SchedulePeriodType.Day, 1);
			SchedulingOptionsProvider.SetFromTest_LegacyDONOTUSE(planningPeriod, new SchedulingOptions
			{
				DayOffTemplate = dayOffTemplate,
				ScheduleEmploymentType = ScheduleEmploymentType.FixedStaff,
				GroupOnGroupPageForTeamBlockPer = new GroupPageLight(UserTexts.Resources.Main, GroupPageType.RuleSetBag),
				UseTeam = true,
				TeamSameShiftCategory = true,
				TagToUseOnScheduling = NullScheduleTag.Instance
			});
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			AssignmentRepository.Find(date.ToDateOnlyPeriod(), scenario)
				.Should().Be.Empty();
		}

		[Test]
		public void TeamBlockSchedulingShouldUseShiftsMarkedForRestrictionOnlyWhenThereIsRestriction()
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1); //12 to 18
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var team = new Team().WithDescription(new Description("team"));
			var contract = new Contract("_");
			var contractSchedule = ContractScheduleFactory.CreateWorkingWeekContractSchedule();
			var agent1 = PersonRepository.Has(contract, contractSchedule, new PartTimePercentage("_"), team, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), skill);
			var agent2 = PersonRepository.Has(contract, contractSchedule, new PartTimePercentage("_"), team, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), skill);
			var shiftCategory = new ShiftCategory("_").WithId();
			var restrictedRuleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var normalRuleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(12, 0, 12, 0, 15), new TimePeriodWithSegment(20, 0, 20, 0, 15), shiftCategory));
			restrictedRuleSet.OnlyForRestrictions = true;
			var ruleSetBag = new RuleSetBag(restrictedRuleSet);
			ruleSetBag.AddRuleSet(normalRuleSet);
			agent1.Period(firstDay).RuleSetBag = ruleSetBag;
			agent2.Period(firstDay).RuleSetBag = ruleSetBag;
			PreferenceDayRepository.Add(new PreferenceDay(agent2, firstDay,
				new PreferenceRestriction
				{
					StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(8))
				}).WithId());
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				1, 1, 1, 1, 1, 1, 1));
			var dayOffTemplate = new DayOffTemplate(new Description("_default")).WithId();
			DayOffTemplateRepository.Add(dayOffTemplate);
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate,SchedulePeriodType.Week, 1);
			SchedulingOptionsProvider.SetFromTest_LegacyDONOTUSE(planningPeriod, new SchedulingOptions
			{
				UseAvailability = true,
				UsePreferences = true,
				UseRotations = true,
				UseStudentAvailability = false,
				DayOffTemplate = dayOffTemplate,
				ScheduleEmploymentType = ScheduleEmploymentType.FixedStaff,
				GroupOnGroupPageForTeamBlockPer = new GroupPageLight(UserTexts.Resources.Main, GroupPageType.Hierarchy),
				UseTeam = true,
				TeamSameShiftCategory = true,
				TagToUseOnScheduling = NullScheduleTag.Instance
			});
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value, false);

			var assignments = AssignmentRepository.Find(new[] { agent1, agent2 }, period, scenario);
			assignments.Count.Should().Be.EqualTo(14);

			var foundRestrictedAssignments = 0;
			foreach (var personAssignment in assignments)
			{
				if (personAssignment.AssignedWithDayOff(dayOffTemplate))
					continue;

				if (
					personAssignment.MainActivities()
						.First()
						.Period.StartDateTimeLocal(personAssignment.Person.PermissionInformation.DefaultTimeZone())
						.TimeOfDay.Equals(TimeSpan.FromHours(8)))
					foundRestrictedAssignments++;
			}
			foundRestrictedAssignments.Should().Be.EqualTo(1);
		}

		[Test, Ignore("Bug in teamblock scheduling? Works on Mondays but not on Tuesdays.")]
		public void ShouldNotMatterIfSchedulingMondayOrTuesday([Values] bool isFirstDayOfWeek)
		{
			var currentDay = new DateOnly(2016, 10, 24);
			if (!isFirstDayOfWeek)
				currentDay = currentDay.AddDays(1);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(currentDay, 1);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 60), new TimePeriodWithSegment(17, 0, 17, 0, 60), new ShiftCategory("_").WithId()));
			var agent = PersonRepository.Has(new Contract("_"), new SchedulePeriod(currentDay, SchedulePeriodType.Day, 1), ruleSet, skill);
			SkillDayRepository.Add(skill.CreateSkillDayWithDemand(scenario, currentDay, 1));

			var dayOffTemplate = new DayOffTemplate(new Description("_default")).WithId();
			DayOffTemplateRepository.Add(dayOffTemplate);
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate, SchedulePeriodType.Week, 1);
			SchedulingOptionsProvider.SetFromTest_LegacyDONOTUSE(planningPeriod, new SchedulingOptions
			{
				GroupOnGroupPageForTeamBlockPer = new GroupPageLight(UserTexts.Resources.Main, GroupPageType.Hierarchy),
				UseTeam = true,
				TagToUseOnScheduling = NullScheduleTag.Instance
			});
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			AssignmentRepository.GetSingle(currentDay, agent)
				.Should().Not.Be.Null();
		}

		[TestCase("AFirst")]
		[TestCase("BFirst")]
		public void ShouldConsiderCorrectShiftCategoryLimitation(string shiftCategoryLimitationOrder)
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("_", activity);
			var scenario = ScenarioRepository.Has("_");
			var team = new Team().WithDescription(new Description("team"));
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var contract = new Contract("_");
			var contractSchedule = ContractScheduleFactory.CreateWorkingWeekContractSchedule();
			var agent = PersonRepository.Has(contract, contractSchedule, new PartTimePercentage("_"), team, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), skill);
			var shiftCategoryA = new ShiftCategory("A").WithId();
			var shiftCategoryB = new ShiftCategory("B").WithId();
			var ruleSetA = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(12, 0, 12, 0, 15), new TimePeriodWithSegment(20, 0, 20, 0, 15), shiftCategoryA));
			var ruleSetB = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(12, 0, 12, 0, 15), new TimePeriodWithSegment(20, 0, 20, 0, 15), shiftCategoryB));
			var ruleSetBag = new RuleSetBag(ruleSetA);
			ruleSetBag.AddRuleSet(ruleSetB);
			agent.Period(firstDay).RuleSetBag = ruleSetBag;
			var shiftCategoryLimitationA = new ShiftCategoryLimitation(shiftCategoryA) {MaxNumberOf = 0, Weekly = true};
			var shiftCategoryLimitationB = new ShiftCategoryLimitation(shiftCategoryB) {MaxNumberOf = 7, Weekly = true};
			if (shiftCategoryLimitationOrder.Equals("AFirst"))
			{
				agent.SchedulePeriod(firstDay).AddShiftCategoryLimitation(shiftCategoryLimitationA);
				agent.SchedulePeriod(firstDay).AddShiftCategoryLimitation(shiftCategoryLimitationB);
			}
			if (shiftCategoryLimitationOrder.Equals("BFirst"))
			{
				agent.SchedulePeriod(firstDay).AddShiftCategoryLimitation(shiftCategoryLimitationB);
				agent.SchedulePeriod(firstDay).AddShiftCategoryLimitation(shiftCategoryLimitationA);
			}
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1, 1, 1));
			var dayOffTemplate = new DayOffTemplate(new Description("_")).WithId();
			DayOffTemplateRepository.Add(dayOffTemplate);
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate,SchedulePeriodType.Week, 1);
			SchedulingOptionsProvider.SetFromTest_LegacyDONOTUSE(planningPeriod, new SchedulingOptions
			{
				DayOffTemplate = dayOffTemplate,
				GroupOnGroupPageForTeamBlockPer = new GroupPageLight(UserTexts.Resources.Main, GroupPageType.Hierarchy),
				UseTeam = true,
				TagToUseOnScheduling = NullScheduleTag.Instance,
				UseShiftCategoryLimitations = true
			});
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			var assignments = AssignmentRepository.Find(new[] { agent }, period, scenario);
			foreach (var personAssignment in assignments)
			{
				if (personAssignment.AssignedWithDayOff(dayOffTemplate))continue;
				personAssignment.ShiftCategory.Should().Be.EqualTo(shiftCategoryB);
			}
		}

		[Test]
		public void ShouldNotPlaceShiftOutsideOpenHoursWhenOtherTeamMemberKnowOpenSkill()
		{
			DayOffTemplateRepository.Add(new DayOffTemplate(new Description("_")).WithId());
			var firstDay = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = ActivityRepository.Has("_");
			var closedSkill = SkillRepository.Has("Closed", activity, new TimePeriod(18, 23));
			var openSkill = SkillRepository.Has("Open", activity);
			var scenario = ScenarioRepository.Has("_");
			var team = new Team().WithDescription(new Description("team")).WithId();
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateBusinessUnitAndAppend(team).WithId(ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value));
			var contractSchedule = ContractScheduleFactory.CreateWorkingWeekContractSchedule();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 15), new TimePeriodWithSegment(17, 0, 17, 0, 15), new ShiftCategory("_").WithId()));
			PersonRepository.Has(new ContractWithMaximumTolerance(), contractSchedule, new PartTimePercentage("_"), team, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSet, openSkill);
			var agentOnlyKnowingClosedSkill = PersonRepository.Has(new ContractWithMaximumTolerance(), contractSchedule, new PartTimePercentage("_"), team, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSet, closedSkill);
			agentOnlyKnowingClosedSkill.SetName(new Name("AgentClosed", "AgentClosed"));
			SkillDayRepository.Has(closedSkill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1, 1, 1));
			SkillDayRepository.Has(openSkill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1, 1, 1));
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate, SchedulePeriodType.Week, 1);
			SchedulingOptionsProvider.SetFromTest_LegacyDONOTUSE(planningPeriod, new SchedulingOptions
			{
				GroupOnGroupPageForTeamBlockPer = new GroupPageLight(UserTexts.Resources.Main, GroupPageType.Hierarchy),
				UseTeam = true,
				TeamSameShiftCategory = true
			});
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			AssignmentRepository.Find(new[] { agentOnlyKnowingClosedSkill }, period, scenario).Any(x => x.ShiftLayers.Any()).Should().Be.False();
		}
		
		[Test]
		public void ShouldConsiderCrossSkillAgents()
		{
			DayOffTemplateRepository.Add(new DayOffTemplate(new Description("_")).WithId());
			var team = new Team().WithDescription(new Description("team")).WithId();
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateBusinessUnitAndAppend(team).WithId(ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value));
			var scenario = ScenarioRepository.Has("_");
			var activity = ActivityRepository.Has("_");
			var date = new DateOnly(2010, 1, 1);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 9, 0, 15), new TimePeriodWithSegment(16, 0, 17, 0, 15), shiftCategory));
			ruleSet.AddLimiter(new ContractTimeLimiter(new TimePeriod(8, 8), TimeSpan.FromHours(1)));
			var skillA = SkillRepository.Has("A", activity);
			var skillB = SkillRepository.Has("B", activity, new TimePeriod(8, 16));
			var skillC = SkillRepository.Has("C", activity, new TimePeriod(9, 17));
			var agentAB = PersonRepository.Has(new Team(), new SchedulePeriod(date, SchedulePeriodType.Week, 1), ruleSet, skillA, skillB);
			var agentBC = PersonRepository.Has(team, new SchedulePeriod(date, SchedulePeriodType.Week, 1), ruleSet, skillC, skillB);
			AssignmentRepository.Has(agentAB, scenario, activity, shiftCategory, date, new TimePeriod(8, 17));//0.5 resources on skillB
			SkillDayRepository.Has(
				skillA.CreateSkillDayWithDemand(scenario, date, 1),
				skillB.CreateSkillDayWithDemand(scenario, date, 1.1), //make test red if skillA/agentAB isn't counted
				skillC.CreateSkillDayWithDemand(scenario, date, 1));
			var planningGroup = new PlanningGroup();
			planningGroup.AddFilter(new TeamFilter(team));
			PlanningGroupRepository.Has(planningGroup);
			var planningPeriod = new PlanningPeriod(date,SchedulePeriodType.Day, 1, planningGroup);
			PlanningPeriodRepository.Add(planningPeriod);
			SchedulingOptionsProvider.SetFromTest_LegacyDONOTUSE(planningPeriod, new SchedulingOptions
			{
				GroupOnGroupPageForTeamBlockPer = new GroupPageLight(UserTexts.Resources.Main, GroupPageType.Hierarchy),
				UseTeam = true,
				TeamSameShiftCategory = true
			});

			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			AssignmentRepository.GetSingle(date, agentBC).ShiftLayers.Single().Period.StartDateTime.Hour
				.Should().Be.EqualTo(9);
		}
		
		[Test]
		public void ShouldHandleBlockSameStartTimeInCombinationWithRotationWithSpecifiedShiftCategoryOnBlockStartingDateBug41378()
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var scenario = ScenarioRepository.Has("_");
			var phoneActivity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("Open", phoneActivity, new TimePeriod(12, 0, 21, 0)).InTimeZone(TimeZoneInfo.Utc);
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,10, 10, 10, 10, 10, 10, 10));
			var shiftCategory8H15M = new ShiftCategory("L").WithId();
			var ruleSet8H15M = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(12, 0, 12, 0, 15), new TimePeriodWithSegment(20, 15, 20, 15, 15), shiftCategory8H15M));
			var ruleSetBag = new RuleSetBag(ruleSet8H15M);
			var shiftCategory7H = new ShiftCategory("S").WithId();
			var ruleSet7H = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(12, 0, 12, 0, 15), new TimePeriodWithSegment(19, 0, 19, 0, 15), shiftCategory7H));
			ruleSetBag.AddRuleSet(ruleSet7H);
			var agent = PersonRepository.Has(new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1),ruleSetBag, skill);
			var schedulePeriod = agent.SchedulePeriod(firstDay);
			schedulePeriod.SetDaysOff(2);
			var dayOffTemplate = new DayOffTemplate(new Description("DO", "DO"));
			DayOffTemplateRepository.Add(dayOffTemplate);
			AssignmentRepository.Has(agent, scenario, dayOffTemplate, firstDay);
			AssignmentRepository.Has(agent, scenario, dayOffTemplate, firstDay.AddDays(6));
			var rotation = new Rotation("_", 7);
			rotation.RotationDays[1].RestrictionCollection[0].ShiftCategory = shiftCategory7H;
			var personRotation = new PersonRotation(agent, rotation, firstDay, 0).WithId();
			PersonRotationRepository.Add(personRotation);
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate, SchedulePeriodType.Week, 1);
			SchedulingOptionsProvider.SetFromTest_LegacyDONOTUSE(planningPeriod, new SchedulingOptions
			{
				UseRotations = true,
				UseBlock = true,
				BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff,
				BlockSameStartTime = true,
				BlockSameShiftCategory = false,
				UseAverageShiftLengths = true
			});
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value, false);
			
			AssignmentRepository.Find(new[] { agent}, firstDay.ToDateOnlyPeriod(), scenario).Single().AssignedWithDayOff(dayOffTemplate).Should().Be.True();
			AssignmentRepository.Find(new[] {agent}, firstDay.AddDays(1).ToDateOnlyPeriod(), scenario).Single().ShiftCategory.Should().Be.EqualTo(shiftCategory7H);
			AssignmentRepository.Find(new[] { agent}, firstDay.AddDays(2).ToDateOnlyPeriod(), scenario).Single().ShiftCategory.Should().Be.EqualTo(shiftCategory8H15M);
			AssignmentRepository.Find(new[] { agent}, firstDay.AddDays(3).ToDateOnlyPeriod(), scenario).Single().ShiftCategory.Should().Be.EqualTo(shiftCategory8H15M);
			AssignmentRepository.Find(new[] { agent}, firstDay.AddDays(4).ToDateOnlyPeriod(), scenario).Single().ShiftCategory.Should().Be.EqualTo(shiftCategory8H15M);
			AssignmentRepository.Find(new[] { agent}, firstDay.AddDays(5).ToDateOnlyPeriod(), scenario).Single().ShiftCategory.Should().Be.EqualTo(shiftCategory8H15M);
			AssignmentRepository.Find(new[] { agent}, firstDay.AddDays(6).ToDateOnlyPeriod(), scenario).Single().AssignedWithDayOff(dayOffTemplate).Should().Be.True();
		}



		[Test]
		public void ShouldBePossibleToScheduleBlockSchedulePeriodSameShiftWhenSkillIsClosedOneDay()
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var agent = PersonRepository.Has(new Contract("_"), ContractScheduleFactory.CreateWorkingWeekContractSchedule(),
				new PartTimePercentage("_"), new Team(), new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), skill);
			agent.Period(firstDay).RuleSetBag = new RuleSetBag(new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity,
				new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory("_").WithId())));

			foreach (var dayTemplate in skill.WorkloadCollection.First().TemplateWeekCollection.Values)
			{
				if (dayTemplate.DayOfWeek == DayOfWeek.Monday || dayTemplate.DayOfWeek == DayOfWeek.Sunday || dayTemplate.DayOfWeek == DayOfWeek.Saturday)
				{
					dayTemplate.Close();
				}
			}
			PersonAbsenceRepository.Has(new PersonAbsence(agent, scenario, new AbsenceLayer(new Absence { InContractTime = true },
					firstDay.ToDateTimePeriod(new TimePeriod(8, 16), TimeZoneInfo.Utc))));
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 0, 1, 1, 1, 1, 0, 0));
			var dayOffTemplate = new DayOffTemplate(new Description("_default")).WithId();
			DayOffTemplateRepository.Add(dayOffTemplate);
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate, SchedulePeriodType.Week, 1);
			SchedulingOptionsProvider.SetFromTest_LegacyDONOTUSE(planningPeriod, new SchedulingOptions
			{
				DayOffTemplate = dayOffTemplate,
				ScheduleEmploymentType = ScheduleEmploymentType.FixedStaff,
				UseBlock = true,
				BlockSameShift = true,
				BlockFinderTypeForAdvanceScheduling = BlockFinderType.SchedulePeriod,
				UseAverageShiftLengths = true
			});

			Target.DoSchedulingAndDO(planningPeriod.Id.Value);


			var assignments = AssignmentRepository.Find(new[] { agent }, period, scenario);
			assignments.Count(x => x.MainActivities().Any() || x.AssignedWithDayOff(dayOffTemplate)).Should().Be.EqualTo(6);
		}

		[Test]
		public void ShouldBePossibleToScheduleBlockBetweenDaysOffSameShiftWhenSkillIsClosedOneDay()
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var agent = PersonRepository.Has(new Contract("_"), ContractScheduleFactory.CreateWorkingWeekContractSchedule(),
				new PartTimePercentage("_"), new Team(), new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), skill);
			agent.Period(firstDay).RuleSetBag = new RuleSetBag(new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity,
				new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory("_").WithId())));
			foreach (var dayTemplate in skill.WorkloadCollection.First().TemplateWeekCollection.Values)
			{
				if (dayTemplate.DayOfWeek == DayOfWeek.Monday || dayTemplate.DayOfWeek == DayOfWeek.Sunday || dayTemplate.DayOfWeek == DayOfWeek.Saturday)
				{
					dayTemplate.Close();
				}
			}
			PersonAbsenceRepository.Has(new PersonAbsence(agent, scenario, new AbsenceLayer(new Absence { InContractTime = true },
					firstDay.ToDateTimePeriod(new TimePeriod(8, 16), TimeZoneInfo.Utc))));
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 0, 1, 1, 1, 1, 0, 0));
			var dayOffTemplate = new DayOffTemplate(new Description("_default")).WithId();
			DayOffTemplateRepository.Add(dayOffTemplate);
			PersonAssignmentRepository.Has(agent, scenario, dayOffTemplate,firstDay.AddDays(-1));			
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate, SchedulePeriodType.Week, 1);
			SchedulingOptionsProvider.SetFromTest_LegacyDONOTUSE(planningPeriod, new SchedulingOptions
			{
				DayOffTemplate = dayOffTemplate,
				ScheduleEmploymentType = ScheduleEmploymentType.FixedStaff,
				UseBlock = true,
				BlockSameShift = true,
				BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff,
				UseAverageShiftLengths = true
			});

			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			var assignments = AssignmentRepository.Find(new[] { agent }, period, scenario);
			assignments.Count(x => x.MainActivities().Any() || x.AssignedWithDayOff(dayOffTemplate)).Should().Be.EqualTo(6);
		}
		
		public void Isolate(IIsolate isolate)
		{
			//hack until web supports team scheduling
			isolate.UseTestDouble<SchedulingOptionsProvider>().For<ISchedulingOptionsProvider>();
		}

		public TeamBlockSchedulingTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}
