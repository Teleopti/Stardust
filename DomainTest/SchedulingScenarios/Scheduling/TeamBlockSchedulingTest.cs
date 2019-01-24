using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.Filters;
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
		public FakePreferenceDayRepository PreferenceDayRepository;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public FakePlanningGroupRepository PlanningGroupRepository;

		[Test]
		public void ShouldTeamSchedulingUseSameShiftCategoryForHierarchy()
		{
			if (!ResourcePlannerTestParameters.IsEnabled(Toggles.ResourcePlanner_TeamSchedulingInPlans_79283))
				Assert.Ignore("only works with toggle on");
			DayOffTemplateRepository.Add(new DayOffTemplate());
			var date = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1);
			var activity = ActivityRepository.Has();
			var skill = SkillRepository.Has(activity);
			var scenario = ScenarioRepository.Has();
			var team = new Team().WithDescription(new Description("team1")).WithId();
			
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateBusinessUnitAndAppend(team).WithId(ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value));
			var shiftCategory1 = new ShiftCategory().WithId();
			var shiftCategory2 = new ShiftCategory().WithId();
			var ruleSet1 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory1));
			var ruleSet2 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(12, 0, 12, 0, 15), new TimePeriodWithSegment(20, 0, 20, 0, 15), shiftCategory2));
			var bag = new RuleSetBag(ruleSet1, ruleSet2);
			PersonRepository.Has(team, new SchedulePeriod(date, SchedulePeriodType.Week, 1), bag, skill);
			PersonRepository.Has(team, new SchedulePeriod(date, SchedulePeriodType.Week, 1), bag, skill);
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), 2));
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate, SchedulePeriodType.Week, 1);
			planningPeriod.PlanningGroup.SetTeamSettings(new TeamSettings
			{
				GroupPageType = GroupPageType.Hierarchy,
				TeamSameType = TeamSameType.ShiftCategory
			});

			Target.DoSchedulingAndDO(planningPeriod.Id.Value, false);

			Assert.Multiple(() =>
			{
				var allAssesWithLayers = AssignmentRepository.LoadAll().Where(x => x.ShiftLayers.Any());
				allAssesWithLayers.Select(x => x.ShiftCategory).ToHashSet().Count
					.Should().Be.EqualTo(2);
				foreach (var day in DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1).DayCollection())
				{
					var assesOnDay = AssignmentRepository.LoadAll().Where(x => x.Date == day);
					assesOnDay.First().ShiftCategory
						.Should().Be.EqualTo(assesOnDay.Last().ShiftCategory);
				}
			});
		}
		
		[Test]
        public void ShouldTeamSchedulingUseSameStartTimeForHierarchy()
        {
        	if (!ResourcePlannerTestParameters.IsEnabled(Toggles.ResourcePlanner_TeamSchedulingInPlans_79283))
        		Assert.Ignore("only works with toggle on");
        	DayOffTemplateRepository.Add(new DayOffTemplate());
        	var date = new DateOnly(2015, 10, 12);
        	var period = DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1);
        	var activity = ActivityRepository.Has();
        	var skill = SkillRepository.Has(activity);
        	var scenario = ScenarioRepository.Has();
        	var team = new Team().WithDescription(new Description("team1")).WithId();
        	
        	BusinessUnitRepository.Has(BusinessUnitFactory.CreateBusinessUnitAndAppend(team).WithId(ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value));
        	var shiftCategory = new ShiftCategory().WithId();
        	var ruleSet1 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
        	var ruleSet2 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(12, 0, 12, 0, 15), new TimePeriodWithSegment(20, 0, 20, 0, 15), shiftCategory));
        	var bag = new RuleSetBag(ruleSet1, ruleSet2);
        	PersonRepository.Has(team, new SchedulePeriod(date, SchedulePeriodType.Week, 1), bag, skill);
        	PersonRepository.Has(team, new SchedulePeriod(date, SchedulePeriodType.Week, 1), bag, skill);
        	SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), 2));
        	var planningPeriod = PlanningPeriodRepository.Has(period.StartDate, SchedulePeriodType.Week, 1);
        	planningPeriod.PlanningGroup.SetTeamSettings(new TeamSettings
        	{
        		GroupPageType = GroupPageType.Hierarchy,
        		TeamSameType = TeamSameType.StartTime
        	});

        	Target.DoSchedulingAndDO(planningPeriod.Id.Value, false);

        	Assert.Multiple(() =>
        	{
        		foreach (var day in DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1).DayCollection())
        		{
        			var assesOnDay = AssignmentRepository.LoadAll().Where(x => x.Date == day);
        			assesOnDay.First().ShiftLayers.First().Period.StartDateTime
        				.Should().Be.EqualTo(assesOnDay.Last().ShiftLayers.First().Period.StartDateTime);
        		}
        	});
        }
		
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
			planningPeriod.PlanningGroup.SetTeamSettings(new TeamSettings
			{
				GroupPageType = GroupPageType.Hierarchy,
				TeamSameType = TeamSameType.ShiftCategory
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
			planningPeriod.PlanningGroup.SetTeamSettings(new TeamSettings
			{
				GroupPageType = GroupPageType.Hierarchy,
				TeamSameType = TeamSameType.ShiftCategory
			});

			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			AssignmentRepository.Find(new[] { agent }, period, scenario).Count(personAssignment => personAssignment.MainActivities().Any()).Should().Be.EqualTo(5);
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
			planningPeriod.PlanningGroup.SetTeamSettings(new TeamSettings
			{
				GroupPageType = GroupPageType.Hierarchy,
				TeamSameType = TeamSameType.ShiftCategory
			});
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			var assignments = AssignmentRepository.Find(new[] { agent1, agent2 }, period, scenario);
			assignments.Count().Should().Be.EqualTo(14);

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
			planningPeriod.PlanningGroup.SetTeamSettings(new TeamSettings
			{
				GroupPageType = GroupPageType.Hierarchy,
				TeamSameType = TeamSameType.ShiftCategory
			});
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			var assignments = AssignmentRepository.Find(new[] { agent1, agent2 }, period, scenario);
			assignments.Count().Should().Be.EqualTo(14);

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
			planningPeriod.PlanningGroup.SetTeamSettings(new TeamSettings
			{
				GroupPageType = GroupPageType.Hierarchy,
				TeamSameType = TeamSameType.ShiftCategory
			});
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value, false);

			var assignments = AssignmentRepository.Find(new[] { agent1, agent2 }, period, scenario);
			assignments.Count().Should().Be.EqualTo(14);

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
			planningPeriod.PlanningGroup.SetTeamSettings(new TeamSettings
			{
				GroupPageType = GroupPageType.Hierarchy,
				TeamSameType = TeamSameType.ShiftCategory
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
			planningPeriod.PlanningGroup.SetTeamSettings(new TeamSettings
			{
				GroupPageType = GroupPageType.Hierarchy,
				TeamSameType = TeamSameType.ShiftCategory
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
			planningPeriod.PlanningGroup.SetTeamSettings(new TeamSettings
			{
				GroupPageType = GroupPageType.Hierarchy,
				TeamSameType = TeamSameType.ShiftCategory
			});

			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			AssignmentRepository.GetSingle(date, agentBC).ShiftLayers.Single().Period.StartDateTime.Hour
				.Should().Be.EqualTo(9);
		}
		
		public TeamBlockSchedulingTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}
