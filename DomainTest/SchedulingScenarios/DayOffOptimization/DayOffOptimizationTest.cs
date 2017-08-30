using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.DayOffOptimization
{
	[DomainTest]
	public class DayOffOptimizationTest : DayOffOptimizationScenario
	{
		public ScheduleOptimizationTeamBlock Target;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeSkillRepository SkillRepository;
		public FakePersonRepository PersonRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeActivityRepository ActivityRepository;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public FakeDayOffRulesRepository DayOffRulesRepository;
		public IScheduleStorage ScheduleStorage;
		public IPersonWeekViolatingWeeklyRestSpecification CheckWeeklyRestRule;
		public FakeDayOffTemplateRepository DayOffTemplateRepository;
		public OptimizationPreferencesDefaultValueProvider OptimizationPreferencesProvider;

		[Test]
		public void ShouldMoveDayOffToDayWithLessDemand()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var planningPeriod = PlanningPeriodRepository.Has(firstDay, 1);
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1);
			schedulePeriod.SetDaysOff(1);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(new Contract("_"), new ContractSchedule("_"), new PartTimePercentage("_"), new Team {Site = new Site("site")}, schedulePeriod, ruleSet, skill);
			var skillDays = SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				5,
				1,
				5,
				5,
				5,
				25,
				5)
			);
			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory,
				new DateOnlyPeriod(firstDay, firstDay.AddDays(7)), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.GetSingle(skillDays[5].CurrentDate) //saturday
				.SetDayOff(new DayOffTemplate());

			Target.Execute(planningPeriod.Id.Value);

			PersonAssignmentRepository.GetSingle(skillDays[5].CurrentDate) //saturday
				.DayOff().Should().Be.Null();
			PersonAssignmentRepository.GetSingle(skillDays[1].CurrentDate) //tuesday
				.DayOff().Should().Not.Be.Null();
		}

		[Test]
		public void ShouldCheckMinWeekWorkTimeAndMoveIfCriteriaIsMet()
		{
			var firstDay = new DateOnly(2016, 5, 23);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var planningPeriod = PlanningPeriodRepository.Has(firstDay, 2);
			var scenario = ScenarioRepository.Has("some name");
			var team = new Team {Site = new Site("_")}.WithDescription(new Description("_"));
			var shiftCategory = new ShiftCategory("_").WithId();
			var normalRuleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity,
				new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent1 = PersonRepository.Has(new Contract("_"), ContractScheduleFactory.CreateWorkingWeekContractSchedule(),
				new PartTimePercentage("_"), team, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 2), normalRuleSet, skill);
			var agent2 = PersonRepository.Has(new Contract("_"), ContractScheduleFactory.CreateWorkingWeekContractSchedule(),
				new PartTimePercentage("_"), team, null, skill);
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				16d,
				16d,
				16d,
				16d,
				8d,
				1d,
				1d, // I want 3 DO this week
				16d,
				16d,
				16d,
				16d,
				16d,
				1d,
				8d)
			);
			var dayOffTemplate = new DayOffTemplate(new Description("_"));
			DayOffTemplateRepository.Add(dayOffTemplate);
			for (var day = 0; day < 14; day++)
			{
				if (day == 5 || day == 6 || day == 12 || day == 13)
				{
					PersonAssignmentRepository.Has(agent1, scenario, dayOffTemplate, firstDay.AddDays(day));
					PersonAssignmentRepository.Has(agent2, scenario, dayOffTemplate, firstDay.AddDays(day));
				}
				else
				{
					PersonAssignmentRepository.Has(agent1, scenario, activity, shiftCategory, firstDay.AddDays(day),
						new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(16)));
					PersonAssignmentRepository.Has(agent2, scenario, activity, shiftCategory, firstDay.AddDays(day),
						new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(16)));
				}
			}
			Target.Execute(planningPeriod.Id.Value);

			var assignments =
				PersonAssignmentRepository.Find(new[] {agent1}, DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 2), scenario)
					.OrderBy(ass => ass.Date)
					.ToList();

			assignments[4].DayOff().Should().Not.Be.Null();
			assignments[5].DayOff().Should().Not.Be.Null();
			assignments[6].DayOff().Should().Not.Be.Null();
			assignments[12].DayOff().Should().Not.Be.Null();
		}

		[Test]
		public void ShouldCheckMinWeekWorkTimeAnNotdMoveIfCriteriaIsNotMet()
		{
			var firstDay = new DateOnly(2016, 5, 23);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var planningPeriod = PlanningPeriodRepository.Has(firstDay, 2);
			var scenario = ScenarioRepository.Has("some name");
			var team = new Team {Site = new Site("_")}.WithDescription(new Description("_"));
			var shiftCategory = new ShiftCategory("_").WithId();
			var normalRuleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity,
				new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent1 = PersonRepository.Has(new Contract("_"), ContractScheduleFactory.CreateWorkingWeekContractSchedule(),
				new PartTimePercentage("_"), team, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 2), normalRuleSet, skill);
			var agent2 = PersonRepository.Has(new Contract("_"), ContractScheduleFactory.CreateWorkingWeekContractSchedule(),
				new PartTimePercentage("_"), team, null, skill);
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				16d,
				16d,
				16d,
				16d,
				8d,
				1d,
				1d, // I want 3 DO this week but then min week work time will be broken
				16d,
				16d,
				16d,
				16d,
				16d,
				1d,
				8d)
			);
			var dayOffTemplate = new DayOffTemplate(new Description("_"));
			DayOffTemplateRepository.Add(dayOffTemplate);
			for (var day = 0; day < 14; day++)
			{
				if (day == 5 || day == 6 || day == 12 || day == 13)
				{
					PersonAssignmentRepository.Has(agent1, scenario, dayOffTemplate, firstDay.AddDays(day));
					PersonAssignmentRepository.Has(agent2, scenario, dayOffTemplate, firstDay.AddDays(day));
				}
				else
				{
					PersonAssignmentRepository.Has(agent1, scenario, activity, shiftCategory, firstDay.AddDays(day),
						new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(16)));
					PersonAssignmentRepository.Has(agent2, scenario, activity, shiftCategory, firstDay.AddDays(day),
						new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(16)));
				}
			}
			agent1.Period(firstDay).PersonContract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(40),
				TimeSpan.FromHours(48), TimeSpan.Zero, TimeSpan.Zero); //Min 40 hours per week
			Target.Execute(planningPeriod.Id.Value);

			var assignments =
				PersonAssignmentRepository.Find(new[] {agent1}, DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 2), scenario)
					.OrderBy(ass => ass.Date)
					.ToList();

			assignments[5].DayOff().Should().Not.Be.Null();
			assignments[6].DayOff().Should().Not.Be.Null();
			assignments[7].DayOff().Should().Not.Be.Null();
			assignments[12].DayOff().Should().Not.Be.Null();
		}

		[Test]
		public void ShouldFixWeeklyRest()
		{
			var weeklyRest = TimeSpan.FromHours(38);
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var weekPeriod = new DateOnlyPeriod(firstDay, firstDay.AddDays(7));
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var planningPeriod = PlanningPeriodRepository.Has(firstDay, 1);

			var scenario = ScenarioRepository.Has("some name");
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(1), TimeSpan.FromHours(48), TimeSpan.FromHours(1),
					weeklyRest)
			};
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1);
			schedulePeriod.SetDaysOff(1);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity,
				new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"),
				new Team {Site = new Site("site")}, schedulePeriod, ruleSet, skill);

			var skillDays = SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				5,
				1,
				5,
				5,
				5,
				25,
				5)
			);

			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory,
				new DateOnlyPeriod(firstDay, firstDay.AddDays(7)), new TimePeriod(8, 0, 16, 0));
			var mondayAss = PersonAssignmentRepository.GetSingle(skillDays[0].CurrentDate); //monday
			mondayAss.Clear();
			mondayAss.AddActivity(activity, new TimePeriod(12, 0, 20, 0));
			PersonAssignmentRepository.GetSingle(skillDays[5].CurrentDate) //saturday
				.SetDayOff(new DayOffTemplate());

			Target.Execute(planningPeriod.Id.Value);

			var agentRange = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(agent,
				new ScheduleDictionaryLoadOptions(false, false, false), weekPeriod, scenario)[agent];

			CheckWeeklyRestRule.IsSatisfyBy(agentRange, weekPeriod, weeklyRest)
				.Should()
				.Be.True();
		}

		[Test, Timeout(5000)]
		public void ShouldFixWeeklyRestWithShiftsEndingAtMidnigth()
		{
			var weeklyRest = TimeSpan.FromHours(40).Add(TimeSpan.FromMinutes(15));
			var firstSunday = new DateOnly(2015, 10, 11);
			var monday = new DateOnly(2015, 10, 12);
			var tuesday = new DateOnly(2015, 10, 13);
			var lastSunday = new DateOnly(2015, 10, 18);
			var activity = ActivityRepository.Has("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var skill = SkillRepository.Has("skill", activity);
			var planningPeriod = PlanningPeriodRepository.Has(monday, 1);
			var weekPeriod = new DateOnlyPeriod(monday, monday.AddDays(7));
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(monday, SchedulePeriodType.Week, 1);
			schedulePeriod.SetDaysOff(2);
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(1), TimeSpan.FromHours(48), TimeSpan.FromHours(16),
					weeklyRest)
			};
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity,
				new TimePeriodWithSegment(16, 0, 16, 0, 15), new TimePeriodWithSegment(24, 0, 24, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"),
				new Team {Site = new Site("site")}, schedulePeriod, ruleSet, skill);

			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, monday,
				5,
				1,
				5,
				5,
				5,
				5,
				1)
			);

			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory,
				new DateOnlyPeriod(firstSunday, firstSunday.AddDays(8)), new TimePeriod(16, 0, 24, 0));
			PersonAssignmentRepository.GetSingle(firstSunday).SetDayOff(new DayOffTemplate());
			PersonAssignmentRepository.GetSingle(tuesday).SetDayOff(new DayOffTemplate());
			PersonAssignmentRepository.GetSingle(lastSunday).SetDayOff(new DayOffTemplate());

			Target.Execute(planningPeriod.Id.Value);
			var agentRange = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(agent,
				new ScheduleDictionaryLoadOptions(false, false, false), weekPeriod, scenario)[agent];
			CheckWeeklyRestRule.IsSatisfyBy(agentRange, weekPeriod, weeklyRest).Should().Be.True();
			Assert.IsTrue(PersonAssignmentRepository.GetSingle(monday).ShiftLayers.IsEmpty());
		}

		[Test]
		public void ShouldFixWeeklyRestWithShiftsOverMidnight()
		{
			var weeklyRest = TimeSpan.FromHours(46);
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var weekPeriod = new DateOnlyPeriod(firstDay, firstDay.AddDays(7));
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var planningPeriod = PlanningPeriodRepository.Has(firstDay, 1);

			var scenario = ScenarioRepository.Has("some name");
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(1), TimeSpan.FromHours(48), TimeSpan.FromHours(1),
					weeklyRest)
			};
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1);
			schedulePeriod.SetDaysOff(1);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity,
				new TimePeriodWithSegment(13, 0, 22, 0, 15), new TimePeriodWithSegment(23, 0, 30, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"),
				new Team {Site = new Site("site")}, schedulePeriod, ruleSet, skill);


			var skillDays = SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				5,
				5,
				5,
				1,
				5,
				25,
				5)
			);

			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory,
				new DateOnlyPeriod(firstDay, firstDay.AddDays(7)), new TimePeriod(22, 0, 30, 0));
			var monday = PersonAssignmentRepository.GetSingle(skillDays[0].CurrentDate);
			monday.Clear();
			monday.AddActivity(activity, new TimePeriod(0, 0, 21, 0));
			PersonAssignmentRepository.GetSingle(skillDays[5].CurrentDate).SetDayOff(new DayOffTemplate());

			Target.Execute(planningPeriod.Id.Value);
			var agentRange = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(agent,
				new ScheduleDictionaryLoadOptions(false, false, false), weekPeriod, scenario)[agent];
			CheckWeeklyRestRule.IsSatisfyBy(agentRange, weekPeriod, weeklyRest).Should().Be.True();
		}

		[Test]
		public void ShouldNotCalculateIntraIntervalIssuses()
		{
			var phoneActivity = ActivityFactory.CreateActivity("phone");
			var skill = SkillRepository.Has("skill", phoneActivity);
			var dateOnly = new DateOnly(2015, 10, 12);
			var planningPeriod = PlanningPeriodRepository.Has(dateOnly.AddDays(-6), 1);
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1);
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11),
				TimeSpan.FromHours(36));
			var contract = new Contract("contract")
			{
				WorkTimeDirective = worktimeDirective,
				PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9)
			};
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity,
				new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), shiftCategory));
			var agent = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"),
				new Team {Site = new Site("site")}, schedulePeriod, ruleSet, skill);
			SkillDayRepository.Has(new List<ISkillDay>
			{
				skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60),
					new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)))
			});

			var dateTime = TimeZoneHelper.ConvertToUtc(dateOnly.Date, agent.PermissionInformation.DefaultTimeZone());
			var assignment = new PersonAssignment(agent, scenario, dateOnly);
			assignment.SetShiftCategory(shiftCategory);
			assignment.AddActivity(phoneActivity, new DateTimePeriod(dateTime.AddHours(8), dateTime.AddHours(8).AddMinutes(10)));
			PersonAssignmentRepository.Add(assignment);

			Target.Execute(planningPeriod.Id.Value);

			var skillDays = SkillDayRepository.FindReadOnlyRange(new DateOnlyPeriod(dateOnly, dateOnly),
				new List<ISkill> {skill}, scenario);
			var skillStaffPeriods = skillDays.First().SkillStaffPeriodCollection;
			foreach (var skillStaffPeriod in skillStaffPeriods)
			{
				skillStaffPeriod.HasIntraIntervalIssue.Should().Be.False();
			}
		}

		[TestCase(true)]
		[TestCase(false)]
		public void ShouldTryAgainAfterShiftCategoryLimitationHasBlockedFirstMove(bool order)
		{
			//Won't be red all runs if fix is gone. Not optimal, fix if we touch these areas again...
			var firstDay = new DateOnly(2015, 10, 26); //mon
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var planningPeriod = PlanningPeriodRepository.Has(firstDay, 1);
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1).NumberOfDaysOf(1);
			var shiftCategory = new ShiftCategory("allowed").WithId();
			var shiftCategoryNotAllowed = new ShiftCategory("not allowed").WithId();
			var allowedRuleset = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var notAllowedRuleset = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategoryNotAllowed));
			var bag = order ? new RuleSetBag(allowedRuleset, notAllowedRuleset) : new RuleSetBag(notAllowedRuleset, allowedRuleset);
			var agent = PersonRepository.Has(new Contract("_"), new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, bag, skill);
			agent.SchedulePeriod(firstDay).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCategoryNotAllowed) { MaxNumberOf = 0 });
			var skillDays = SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				10,
				1, 
				10,
				10,
				10,
				10,
				10
			));
			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory, DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1), new TimePeriod(6, 0, 14, 0));
			PersonAssignmentRepository.GetSingle(skillDays[0].CurrentDate).SetDayOff(new DayOffTemplate());
			var optPrefs = OptimizationPreferencesProvider.Fetch();
			optPrefs.General.UseShiftCategoryLimitations = true;
			OptimizationPreferencesProvider.SetFromTestsOnly(optPrefs);

			Target.Execute(planningPeriod.Id.Value);

			PersonAssignmentRepository.GetSingle(skillDays[1].CurrentDate).DayOff()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldMoveDayOffToDayWithLessDemand_NightlyRestGetsBrokenDuringOptimization()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var planningPeriod = PlanningPeriodRepository.Has(firstDay, 1);
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1);
			schedulePeriod.SetDaysOff(1);
			var shiftCategory = new ShiftCategory("_").WithId();
			var contract = new Contract("8 hours nightly rest")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(8), TimeSpan.FromHours(100), TimeSpan.FromHours(8), TimeSpan.FromHours(4))
			};
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 7, 0, 15), new TimePeriodWithSegment(15, 0, 15, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, ruleSet, skill);
			var skillDays = SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				5,
				1,
				5,
				5,
				5,
				25,
				5)
			);
			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory,
				new DateOnlyPeriod(firstDay, firstDay.AddDays(7)), new TimePeriod(16, 0, 24, 0));
			PersonAssignmentRepository.GetSingle(skillDays[5].CurrentDate) //saturday
				.SetDayOff(new DayOffTemplate());

			Target.Execute(planningPeriod.Id.Value);

			PersonAssignmentRepository.GetSingle(skillDays[5].CurrentDate) //saturday
				.DayOff().Should().Be.Null();
			PersonAssignmentRepository.GetSingle(skillDays[1].CurrentDate) //tuesday
				.DayOff().Should().Not.Be.Null();
		}

	}
}