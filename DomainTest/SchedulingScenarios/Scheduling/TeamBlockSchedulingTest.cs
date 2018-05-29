﻿using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
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
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	public class TeamBlockSchedulingTest : SchedulingScenario
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
			var agent1 = PersonRepository.Has(new ContractWithMaximumTolerance(), new ContractSchedule("_"), new PartTimePercentage("_"), new Team(), new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSetBag, skill);
			var agent2 = PersonRepository.Has(new ContractWithMaximumTolerance(), new ContractSchedule("_"), new PartTimePercentage("_"), new Team(), new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSetBag, skill);
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
			SchedulingOptionsProvider.SetFromTest(new SchedulingOptions
			{
				UseTeam = true,
				GroupOnGroupPageForTeamBlockPer = new GroupPageLight("_", GroupPageType.RuleSetBag),
				UseBlock = true,
				BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff,
				TeamSameShiftCategory = true,
				BlockSameShiftCategory = true
			});
			var planningPeriod = PlanningPeriodRepository.Has(firstDay, firstDay.AddDays(4),SchedulePeriodType.Day, 5);
			
			Target.DoScheduling(planningPeriod.Id.Value);

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
			SchedulingOptionsProvider.SetFromTest(new SchedulingOptions
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
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate,period.EndDate,SchedulePeriodType.Week, 1);
			
			Target.DoScheduling(planningPeriod.Id.Value);

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
			SchedulingOptionsProvider.SetFromTest(new SchedulingOptions
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
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate,period.EndDate,SchedulePeriodType.Week, 1);
			
			Target.DoScheduling(planningPeriod.Id.Value);

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
			SchedulingOptionsProvider.SetFromTest(new SchedulingOptions
			{
				DayOffTemplate = dayOffTemplate,
				ScheduleEmploymentType = ScheduleEmploymentType.FixedStaff,
				GroupOnGroupPageForTeamBlockPer = new GroupPageLight(UserTexts.Resources.Main, GroupPageType.RuleSetBag),
				UseTeam = true,
				TeamSameShiftCategory = true,
				TagToUseOnScheduling = NullScheduleTag.Instance
			});
			var planningPeriod = PlanningPeriodRepository.Has(date,date,SchedulePeriodType.Day, 1);
			
			Target.DoScheduling(planningPeriod.Id.Value);

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
			SchedulingOptionsProvider.SetFromTest(new SchedulingOptions
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
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate,period.EndDate,SchedulePeriodType.Week, 1);
			
			Target.DoScheduling(planningPeriod.Id.Value);

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
			SchedulingOptionsProvider.SetFromTest(new SchedulingOptions
			{
				GroupOnGroupPageForTeamBlockPer = new GroupPageLight(UserTexts.Resources.Main, GroupPageType.Hierarchy),
				UseTeam = true,
				TagToUseOnScheduling = NullScheduleTag.Instance
			});
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate, period.EndDate, SchedulePeriodType.Week, 1);
			
			Target.DoScheduling(planningPeriod.Id.Value);

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
			SchedulingOptionsProvider.SetFromTest(new SchedulingOptions
			{
				DayOffTemplate = dayOffTemplate,
				GroupOnGroupPageForTeamBlockPer = new GroupPageLight(UserTexts.Resources.Main, GroupPageType.Hierarchy),
				UseTeam = true,
				TagToUseOnScheduling = NullScheduleTag.Instance,
				UseShiftCategoryLimitations = true
			});
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate,period.EndDate,SchedulePeriodType.Week, 1);
			
			Target.DoScheduling(planningPeriod.Id.Value);

			var assignments = AssignmentRepository.Find(new[] { agent }, period, scenario);
			foreach (var personAssignment in assignments)
			{
				if (personAssignment.AssignedWithDayOff(dayOffTemplate))continue;
				personAssignment.ShiftCategory.Should().Be.EqualTo(shiftCategoryB);
			}
		}

		[Test]
		public void ShouldHandleMasterActivity()
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = ActivityRepository.Has("_");
			var masterActivity = new MasterActivity().WithId();
			masterActivity.ActivityCollection.Add(activity);
			var skill = SkillRepository.Has("_", activity);
			var scenario = ScenarioRepository.Has("_");
			var contractSchedule = ContractScheduleFactory.CreateWorkingWeekContractSchedule();
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(masterActivity, new TimePeriodWithSegment(12, 0, 12, 0, 15), new TimePeriodWithSegment(20, 0, 20, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(new Contract("_"), contractSchedule, new PartTimePercentage("_"), new Team(),  new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSet, skill);	
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1, 1, 1));
			var dayOffTemplate = new DayOffTemplate(new Description("_")).WithId();
			DayOffTemplateRepository.Add(dayOffTemplate);
			SchedulingOptionsProvider.SetFromTest(new SchedulingOptions
			{
				DayOffTemplate = dayOffTemplate,
				TagToUseOnScheduling = NullScheduleTag.Instance,
				UseBlock = true,
				BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff
			});
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate,period.EndDate,SchedulePeriodType.Week, 1);
			
			Target.DoScheduling(planningPeriod.Id.Value);

			AssignmentRepository.Find(new[] { agent }, period, scenario).Count(personAssignment => personAssignment.MainActivities().Any()).Should().Be.EqualTo(5);
		}

		[Test]
		public void ShouldHandleMasterActivityOnBaseActivityAgentKnowOneOfThePossibleUnderlyingActivities()
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = ActivityRepository.Has("_");
			var activityAgentDontKnow = ActivityRepository.Has("_");
			var masterActivity = new MasterActivity().WithId();
			masterActivity.ActivityCollection.Add(activity);
			masterActivity.ActivityCollection.Add(activityAgentDontKnow);
			var skill = SkillRepository.Has("_", activity);
			var skillAgentDontKnow = SkillRepository.Has("_", activityAgentDontKnow);
			var scenario = ScenarioRepository.Has("_");
			var contractSchedule = ContractScheduleFactory.CreateWorkingWeekContractSchedule();
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(masterActivity, new TimePeriodWithSegment(12, 0, 12, 0, 15), new TimePeriodWithSegment(20, 0, 20, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(new Contract("_"), contractSchedule, new PartTimePercentage("_"), new Team(), new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSet, skill);
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1, 1, 1));
			SkillDayRepository.Has(skillAgentDontKnow.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 10, 10, 10, 10, 10, 10, 10));
			var dayOffTemplate = new DayOffTemplate(new Description("_")).WithId();
			DayOffTemplateRepository.Add(dayOffTemplate);
			SchedulingOptionsProvider.SetFromTest(new SchedulingOptions
			{
				DayOffTemplate = dayOffTemplate,
				TagToUseOnScheduling = NullScheduleTag.Instance,
				UseBlock = true,
				BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff
			});
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate,period.EndDate,SchedulePeriodType.Week, 1);
			
			Target.DoScheduling(planningPeriod.Id.Value);

			AssignmentRepository.Find(new[] { agent }, period, scenario)
				.Where(personAssignment => !personAssignment.AssignedWithDayOff(dayOffTemplate))
				.Count(personAssignment => personAssignment.MainActivities().First().Payload.Equals(activity))
				.Should().Be.EqualTo(5);	
		}

		[Test]
		public void ShouldHandleMasterActivityOnExtendedActivityAgentKnowOneOfThePossibleUnderlyingActivities()
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = ActivityRepository.Has("_");
			var activityExtended = ActivityRepository.Has("A");
			var activityExtendedAgentDontKnow = ActivityRepository.Has("_");
			var masterActivity = new MasterActivity().WithId();
			masterActivity.ActivityCollection.Add(activityExtended);
			masterActivity.ActivityCollection.Add(activityExtendedAgentDontKnow);
			var skillMainActivity = SkillRepository.Has("_", activity);
			var skillExtendedActivity = SkillRepository.Has("_", activityExtended);
			var skillExtendedActivityAgentDontKnow = SkillRepository.Has("_", activityExtendedAgentDontKnow);
			var scenario = ScenarioRepository.Has("_");
			var contractSchedule = ContractScheduleFactory.CreateWorkingWeekContractSchedule();
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(12, 0, 12, 0, 15), new TimePeriodWithSegment(20, 0, 20, 0, 15), shiftCategory));
			ruleSet.AddExtender(new ActivityRelativeStartExtender(masterActivity, new TimePeriodWithSegment(1, 0, 1, 0, 15), new TimePeriodWithSegment(0, 0, 0, 0, 15)));
			var agent = PersonRepository.Has(new Contract("_"), contractSchedule, new PartTimePercentage("_"), new Team(), new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSet, skillMainActivity, skillExtendedActivity);
			SkillDayRepository.Has(skillMainActivity.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1, 1, 1));
			SkillDayRepository.Has(skillExtendedActivity.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1, 1, 1));
			SkillDayRepository.Has(skillExtendedActivityAgentDontKnow.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 10, 10, 10, 10, 10, 10, 10));
			var dayOffTemplate = new DayOffTemplate(new Description("_")).WithId();
			DayOffTemplateRepository.Add(dayOffTemplate);
			SchedulingOptionsProvider.SetFromTest(new SchedulingOptions
			{
				DayOffTemplate = dayOffTemplate,
				TagToUseOnScheduling = NullScheduleTag.Instance,
				UseBlock = true,
				BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff
			});
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate,period.EndDate,SchedulePeriodType.Week, 1);
			
			Target.DoScheduling(planningPeriod.Id.Value);

			AssignmentRepository.Find(new[] { agent }, period, scenario)
				.Where(personAssignment => !personAssignment.AssignedWithDayOff(dayOffTemplate))
				.Count(personAssignment => !personAssignment.ShiftLayers.All(x => x.Payload.Equals(activityExtendedAgentDontKnow)) 
											&& personAssignment.ShiftLayers.Any(y => y.Payload.Equals(activityExtended)))
				.Should().Be.EqualTo(5);
		}

		[TestCase(true)] //not included because agent doesn't know skill
		[TestCase(false)] //not included because filtered in ShiftFromMasterActivityService (not sure it's correct but that's current impl)
		public void ShouldHandleMasterActivityAsBaseActivity_RequiresSkill_AgentDoesntKnowActivity(bool masterActivityActivityRequiresSkill)
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var masterActivityActivity = ActivityRepository.Has("_");
			masterActivityActivity.RequiresSkill = masterActivityActivityRequiresSkill;
			var masterActivity = new MasterActivity().WithId();
			masterActivity.ActivityCollection.Add(masterActivityActivity);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("_", activity);
			var scenario = ScenarioRepository.Has("_");
			var contractSchedule = ContractScheduleFactory.CreateWorkingWeekContractSchedule();
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(masterActivity, new TimePeriodWithSegment(12, 0, 12, 0, 15), new TimePeriodWithSegment(20, 0, 20, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(new Contract("_"), contractSchedule, new PartTimePercentage("_"), new Team(), new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSet, skill);
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1, 1, 1));
			var dayOffTemplate = new DayOffTemplate(new Description("_")).WithId();
			DayOffTemplateRepository.Add(dayOffTemplate);
			SchedulingOptionsProvider.SetFromTest(new SchedulingOptions
			{
				DayOffTemplate = dayOffTemplate,
				TagToUseOnScheduling = NullScheduleTag.Instance,
				UseBlock = true,
				BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff
			});
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate,period.EndDate,SchedulePeriodType.Week, 1);
			
			Target.DoScheduling(planningPeriod.Id.Value);

			AssignmentRepository.Find(new[] { agent }, period, scenario).Any(personAssignment => personAssignment.MainActivities().Any())
				.Should().Be.False();
		}

		[Test]
		public void ShouldHandleMasterActivityOnExtendedActivity()
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = ActivityRepository.Has("_");
			var otherActivity = ActivityRepository.Has("_");
			var masterActivity = new MasterActivity().WithId();
			masterActivity.ActivityCollection.Add(otherActivity);
			var skillMainActivity = SkillRepository.Has("_", activity);
			var skillExtendedActivity = SkillRepository.Has("_", otherActivity);
			var scenario = ScenarioRepository.Has("_");
			var contractSchedule = ContractScheduleFactory.CreateWorkingWeekContractSchedule();
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(12, 0, 12, 0, 15), new TimePeriodWithSegment(20, 0, 20, 0, 15), shiftCategory));
			ruleSet.AddExtender(new ActivityRelativeStartExtender(masterActivity, new TimePeriodWithSegment(1, 0, 1, 0, 15), new TimePeriodWithSegment(0, 0, 0, 0, 15)));
			var agent = PersonRepository.Has(new Contract("_"), contractSchedule, new PartTimePercentage("_"), new Team(), new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSet, skillMainActivity, skillExtendedActivity);
			SkillDayRepository.Has(skillMainActivity.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1, 1, 1));
			SkillDayRepository.Has(skillExtendedActivity.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1, 1, 1));
			var dayOffTemplate = new DayOffTemplate(new Description("_")).WithId();
			DayOffTemplateRepository.Add(dayOffTemplate);
			SchedulingOptionsProvider.SetFromTest(new SchedulingOptions
			{
				DayOffTemplate = dayOffTemplate,
				TagToUseOnScheduling = NullScheduleTag.Instance,
				UseBlock = true,
				BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff
			});
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate,period.EndDate,SchedulePeriodType.Week, 1);
			
			Target.DoScheduling(planningPeriod.Id.Value);

			AssignmentRepository.Find(new[] { agent }, period, scenario).Count(personAssignment => personAssignment.MainActivities().Any()).Should().Be.EqualTo(5);
		}

		[TestCase(true)] //not included because agent doesn't know skill
		[TestCase(false)] //not included because filtered in ShiftFromMasterActivityService (not sure it's correct but that's current impl)
		public void ShouldHandleMasterActivityAsExtendedActivity_RequiresSkill_AgentDoesntKnowActivity(bool masterActivityActivityRequiresSkill)
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var masterActivityActivity = ActivityRepository.Has("_");
			masterActivityActivity.RequiresSkill = masterActivityActivityRequiresSkill;
			var masterActivity = new MasterActivity().WithId();
			masterActivity.ActivityCollection.Add(masterActivityActivity);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("_", activity);
			var scenario = ScenarioRepository.Has("_");
			var contractSchedule = ContractScheduleFactory.CreateWorkingWeekContractSchedule();
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(12, 0, 12, 0, 15), new TimePeriodWithSegment(20, 0, 20, 0, 15), shiftCategory));
			ruleSet.AddExtender(new ActivityRelativeStartExtender(masterActivity, new TimePeriodWithSegment(1, 0, 1, 0, 15), new TimePeriodWithSegment(0, 0, 0, 0, 15)));
			var agent = PersonRepository.Has(new Contract("_"), contractSchedule, new PartTimePercentage("_"), new Team(), new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSet, skill);
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1, 1, 1));
			var dayOffTemplate = new DayOffTemplate(new Description("_")).WithId();
			DayOffTemplateRepository.Add(dayOffTemplate);
			SchedulingOptionsProvider.SetFromTest(new SchedulingOptions
			{
				DayOffTemplate = dayOffTemplate,
				TagToUseOnScheduling = NullScheduleTag.Instance,
				UseBlock = true,
				BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff
			});
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate,period.EndDate,SchedulePeriodType.Week, 1);
			
			Target.DoScheduling(planningPeriod.Id.Value);

			AssignmentRepository.Find(new[] { agent }, period, scenario).Any(personAssignment => personAssignment.MainActivities().Any())
				.Should().Be.False();
		}
		
		[Test]
		public void ShouldGetCorrectLengthOnMasterActivityLayersAsNonBaseActivity()
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = ActivityRepository.Has("_");
			var otherActivity = ActivityRepository.Has("_");
			var masterActivity = new MasterActivity().WithId();
			masterActivity.ActivityCollection.Add(otherActivity);
			var skillMainActivity = SkillRepository.Has("_", activity);
			var skillExtendedActivity = SkillRepository.Has("_", otherActivity);
			var scenario = ScenarioRepository.Has("_");
			var contractSchedule = ContractScheduleFactory.CreateWorkingWeekContractSchedule();
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(12, 0, 12, 0, 15), new TimePeriodWithSegment(20, 0, 20, 0, 15), shiftCategory));
			ruleSet.AddExtender(new ActivityRelativeStartExtender(masterActivity, new TimePeriodWithSegment(1, 0, 1, 0, 15), new TimePeriodWithSegment(0, 0, 0, 0, 15)));
			var agent = PersonRepository.Has(new Contract("_"), contractSchedule, new PartTimePercentage("_"), new Team(), new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSet, skillMainActivity, skillExtendedActivity);
			SkillDayRepository.Has(skillMainActivity.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1, 1, 1));
			SkillDayRepository.Has(skillExtendedActivity.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1, 1, 1));
			var dayOffTemplate = new DayOffTemplate(new Description("_")).WithId();
			DayOffTemplateRepository.Add(dayOffTemplate);
			SchedulingOptionsProvider.SetFromTest(new SchedulingOptions
			{
				DayOffTemplate = dayOffTemplate,
				TagToUseOnScheduling = NullScheduleTag.Instance,
				UseBlock = true,
				BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff
			});
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate,period.EndDate,SchedulePeriodType.Week, 1);
			
			Target.DoScheduling(planningPeriod.Id.Value);
			
			foreach (var personAssignment in AssignmentRepository.Find(new[] {agent}, period, scenario))
			{
				if (personAssignment.DayOff() != null) continue;
				foreach (var shiftLayer in personAssignment.ShiftLayers)
				{
					if (shiftLayer.Payload.Equals(otherActivity))
					{
						shiftLayer.Period.ElapsedTime().Hours.Should().Be.EqualTo(1);
					}
				}
			}
		}

		[Test]
		public void ShouldBePossibleToHaveSkillAgentDoesntKnowIfNotRequiresSkillAndNotMasterSkill()
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = ActivityRepository.Has("_");
			activity.RequiresSkill = false;
			var skill = SkillRepository.Has("_", activity);
			var scenario = ScenarioRepository.Has("_");
			var contractSchedule = ContractScheduleFactory.CreateWorkingWeekContractSchedule();
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(12, 0, 12, 0, 15), new TimePeriodWithSegment(20, 0, 20, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(new Contract("_"), contractSchedule, new PartTimePercentage("_"), new Team(), new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSet, new Skill("_").For(new Activity()).WithId());
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1, 1, 1));
			var dayOffTemplate = new DayOffTemplate(new Description("_")).WithId();
			DayOffTemplateRepository.Add(dayOffTemplate);
			SchedulingOptionsProvider.SetFromTest(new SchedulingOptions
			{
				DayOffTemplate = dayOffTemplate,
				TagToUseOnScheduling = NullScheduleTag.Instance,
				UseBlock = true,
				BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff
			});
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate, period.EndDate, SchedulePeriodType.Week, 1);
			
			Target.DoScheduling(planningPeriod.Id.Value);

			AssignmentRepository.Find(new[] { agent }, period, scenario).Count(personAssignment => personAssignment.MainActivities().Any()).Should().Be.EqualTo(5);
		}

		[Test]
		public void ShouldBePossibleToHaveSkillInExtenderAgentDoesntKnowIfNotRequiresSkillAndNotMasterSkill()
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var baseActivity = ActivityRepository.Has("_");
			var extenderActivity = ActivityRepository.Has("_");
			extenderActivity.RequiresSkill = false;
			var skill = SkillRepository.Has("_", baseActivity);
			var scenario = ScenarioRepository.Has("_");
			var contractSchedule = ContractScheduleFactory.CreateWorkingWeekContractSchedule();
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(baseActivity, new TimePeriodWithSegment(12, 0, 12, 0, 15), new TimePeriodWithSegment(20, 0, 20, 0, 15), shiftCategory));
			ruleSet.AddExtender(new ActivityRelativeStartExtender(extenderActivity, new TimePeriodWithSegment(1, 0, 1, 0, 15), new TimePeriodWithSegment(0, 0, 0, 0, 15)));
			var agent = PersonRepository.Has(new Contract("_"), contractSchedule, new PartTimePercentage("_"), new Team(), new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSet, skill);
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1, 1, 1));
			var dayOffTemplate = new DayOffTemplate(new Description("_")).WithId();
			DayOffTemplateRepository.Add(dayOffTemplate);
			SchedulingOptionsProvider.SetFromTest(new SchedulingOptions
			{
				DayOffTemplate = dayOffTemplate,
				TagToUseOnScheduling = NullScheduleTag.Instance,
				UseBlock = true,
				BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff
			});
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate, period.EndDate, SchedulePeriodType.Week, 1);
			
			Target.DoScheduling(planningPeriod.Id.Value);

			AssignmentRepository.Find(new[] { agent }, period, scenario).Count(personAssignment => personAssignment.MainActivities().Any()).Should().Be.EqualTo(5);
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
			SchedulingOptionsProvider.SetFromTest(new SchedulingOptions
			{
				GroupOnGroupPageForTeamBlockPer = new GroupPageLight(UserTexts.Resources.Main, GroupPageType.Hierarchy),
				UseTeam = true,
				TeamSameShiftCategory = true
			});
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate, period.EndDate, SchedulePeriodType.Week, 1);
			
			Target.DoScheduling(planningPeriod.Id.Value);

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
			SchedulingOptionsProvider.SetFromTest(new SchedulingOptions
			{
				GroupOnGroupPageForTeamBlockPer = new GroupPageLight(UserTexts.Resources.Main, GroupPageType.Hierarchy),
				UseTeam = true,
				TeamSameShiftCategory = true
			});
			var planningGroup = new PlanningGroup();
			planningGroup.AddFilter(new TeamFilter(team));
			var planningPeriod = new PlanningPeriod(date.ToDateOnlyPeriod(),SchedulePeriodType.Day, 1, planningGroup);
			PlanningPeriodRepository.Add(planningPeriod);

			Target.DoScheduling(planningPeriod.Id.Value);

			AssignmentRepository.GetSingle(date, agentBC).ShiftLayers.Single().Period.StartDateTime.Hour
				.Should().Be.EqualTo(9);
		}

		[Test]
		public void ShouldHandlePartlyOpenedSkillCorrectlyForAgentsInStrangeTimezones()
		{
			DayOffTemplateRepository.Add(new DayOffTemplate(new Description("_")).WithId());
			var date = new DateOnly(2015, 10, 12);
			var activity = ActivityRepository.Has("_");
			//opens UTC 10:00
			var skill = SkillRepository.Has("Open", activity, new TimePeriod(11, 19)).InTimeZone(TimeZoneInfoFactory.StockholmTimeZoneInfo());
			var scenario = ScenarioRepository.Has("_");
			//starts UTC 10:00
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(3, 0, 3, 0, 15), new TimePeriodWithSegment(11, 0, 11, 0, 15), new ShiftCategory("_").WithId()));
			var agent = PersonRepository.Has(new ContractWithMaximumTolerance(), new SchedulePeriod(date, SchedulePeriodType.Day, 1), ruleSet, skill).InTimeZone(TimeZoneInfoFactory.DenverTimeZoneInfo());
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date.AddDays(-1), 1, 1, 1));
			SchedulingOptionsProvider.SetFromTest(new SchedulingOptions
			{
				UseBlock = true,
				BlockSameShift = true,
				BlockFinderTypeForAdvanceScheduling = BlockFinderType.SchedulePeriod
			});				
			var planningPeriod = PlanningPeriodRepository.Has(date,date,SchedulePeriodType.Day, 1);
			
			Target.DoScheduling(planningPeriod.Id.Value);

			AssignmentRepository.Find(new[] { agent}, date.ToDateOnlyPeriod(), scenario).Any(x => x.ShiftLayers.Any()).Should().Be.True();
		}
		
		
		[Test]
		public void ShouldHandleBlockSamerStartTimeInCombinationWithRotationWithSpecifyedShiftCategoryOnBlockStartingDateBug41378()
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
			var agent1 = PersonRepository.Has(new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1),ruleSetBag, skill);
			var schedulePeriod = agent1.SchedulePeriod(firstDay);
			schedulePeriod.SetDaysOff(2);
			var dayOffTemplate = new DayOffTemplate(new Description("DO", "DO"));
			DayOffTemplateRepository.Add(dayOffTemplate);
			AssignmentRepository.Has(agent1, scenario, dayOffTemplate, firstDay);
			AssignmentRepository.Has(agent1, scenario, dayOffTemplate, firstDay.AddDays(6));
			var rotation = new Rotation("_", 7);
			rotation.RotationDays[1].RestrictionCollection[0].ShiftCategory = shiftCategory7H;
			var personRotation = new PersonRotation(agent1, rotation, firstDay, 0).WithId();
			PersonRotationRepository.Add(personRotation);
			SchedulingOptionsProvider.SetFromTest(new SchedulingOptions
			{
				UseRotations = true,
				UseBlock = true,
				BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff,
				BlockSameStartTime = true,
				BlockSameShiftCategory = false,
				UseAverageShiftLengths = true
			});
			
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate, period.EndDate, SchedulePeriodType.Week, 1);
			
			Target.DoScheduling(planningPeriod.Id.Value);
			
			AssignmentRepository.Find(new[] { agent1}, firstDay.ToDateOnlyPeriod(), scenario).Single().AssignedWithDayOff(dayOffTemplate).Should().Be.True();
			AssignmentRepository.Find(new[] {agent1}, firstDay.AddDays(1).ToDateOnlyPeriod(), scenario).Single().ShiftCategory.Should().Be.EqualTo(shiftCategory7H);
			AssignmentRepository.Find(new[] { agent1}, firstDay.AddDays(2).ToDateOnlyPeriod(), scenario).Single().ShiftCategory.Should().Be.EqualTo(shiftCategory8H15M);
			AssignmentRepository.Find(new[] { agent1}, firstDay.AddDays(3).ToDateOnlyPeriod(), scenario).Single().ShiftCategory.Should().Be.EqualTo(shiftCategory8H15M);
			AssignmentRepository.Find(new[] { agent1}, firstDay.AddDays(4).ToDateOnlyPeriod(), scenario).Single().ShiftCategory.Should().Be.EqualTo(shiftCategory8H15M);
			AssignmentRepository.Find(new[] { agent1}, firstDay.AddDays(5).ToDateOnlyPeriod(), scenario).Single().ShiftCategory.Should().Be.EqualTo(shiftCategory8H15M);
			AssignmentRepository.Find(new[] { agent1}, firstDay.AddDays(6).ToDateOnlyPeriod(), scenario).Single().AssignedWithDayOff(dayOffTemplate).Should().Be.True();
		}

		[TestCase(true)]
		[TestCase(false)]
		public void ShouldHandleMasterActivityBetweenExtenderActivities(bool useMasterActivity)
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = ActivityRepository.Has("_");
			var extenderActivity = ActivityRepository.Has("_");
			extenderActivity.RequiresSkill = false;
			var masterActivity = new MasterActivity().WithId();
			masterActivity.ActivityCollection.Add(activity);
			var skill = SkillRepository.Has("_", activity);
			var scenario = ScenarioRepository.Has("_");
			var contractSchedule = ContractScheduleFactory.CreateWorkingWeekContractSchedule();
			var shiftCategory = new ShiftCategory("_").WithId();
			var activityToUse = useMasterActivity ? masterActivity : activity;
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activityToUse, new TimePeriodWithSegment(12, 0, 12, 0, 15), new TimePeriodWithSegment(20, 0, 20, 0, 15), shiftCategory));
			ruleSet.AddExtender(new ActivityRelativeEndExtender(extenderActivity, new TimePeriodWithSegment(1, 0, 1, 0, 15), new TimePeriodWithSegment(0, 0, 1, 0, 15)));
			ruleSet.AddExtender(new ActivityRelativeStartExtender(extenderActivity, new TimePeriodWithSegment(1, 0, 1, 0, 15), new TimePeriodWithSegment(0, 0, 0, 0, 15)));
			var agent = PersonRepository.Has(new Contract("_"), contractSchedule, new PartTimePercentage("_"), new Team(), new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSet, skill);
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1, 1, 1));
			var dayOffTemplate = new DayOffTemplate(new Description("_")).WithId();
			DayOffTemplateRepository.Add(dayOffTemplate);
			SchedulingOptionsProvider.SetFromTest(new SchedulingOptions
			{
				DayOffTemplate = dayOffTemplate,
				TagToUseOnScheduling = NullScheduleTag.Instance,
				UseBlock = true,
				BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff
			});
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate, period.EndDate, SchedulePeriodType.Week, 1);

			Target.DoScheduling(planningPeriod.Id.Value);

			AssignmentRepository.Find(new[] { agent }, period, scenario).Count(personAssignment => personAssignment.MainActivities().Any()).Should().Be.EqualTo(5);
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
			SchedulingOptionsProvider.SetFromTest(new SchedulingOptions
			{
				DayOffTemplate = dayOffTemplate,
				ScheduleEmploymentType = ScheduleEmploymentType.FixedStaff,
				UseBlock = true,
				BlockSameShift = true,
				BlockFinderTypeForAdvanceScheduling = BlockFinderType.SchedulePeriod,
				UseAverageShiftLengths = true
			});
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate, period.EndDate, SchedulePeriodType.Week, 1);

			Target.DoScheduling(planningPeriod.Id.Value);


			var assignments = AssignmentRepository.Find(new[] { agent }, period, scenario);
			assignments.Count.Should().Be.EqualTo(6);
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
			SchedulingOptionsProvider.SetFromTest(new SchedulingOptions
			{
				DayOffTemplate = dayOffTemplate,
				ScheduleEmploymentType = ScheduleEmploymentType.FixedStaff,
				UseBlock = true,
				BlockSameShift = true,
				BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff,
				UseAverageShiftLengths = true
			});
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate, period.EndDate, SchedulePeriodType.Week, 1);

			Target.DoScheduling(planningPeriod.Id.Value);

			var assignments = AssignmentRepository.Find(new[] { agent }, period, scenario);
			assignments.Count.Should().Be.EqualTo(6);
		}



		public TeamBlockSchedulingTest(SeperateWebRequest seperateWebRequest, bool resourcePlannerLessResourcesXXL74915) : base(seperateWebRequest, resourcePlannerLessResourcesXXL74915)
		{
		}
	}
}
