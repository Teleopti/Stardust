using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
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

namespace Teleopti.Ccc.DomainTest.Optimization.ScheduleOptimizationTests
{
	[DomainTest]
	public class DayOffOptimizationTest
	{
		public ScheduleOptimization Target;
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

		[Test]
		public void ShouldMoveDayOffToDayWithLessDemand()
		{
			var firstDay = new DateOnly(2015,10,12); //mon
			var planningPeriod = PlanningPeriodRepository.Has(firstDay, 1);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1);
			schedulePeriod.SetDaysOff(1);
			var agent = PersonRepository.Has(new Contract("_"), new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site")}, schedulePeriod, skill);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			agent.Period(firstDay).RuleSetBag = new RuleSetBag(ruleSet);

			var skillDays = SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				TimeSpan.FromHours(5),
				TimeSpan.FromHours(1),
				TimeSpan.FromHours(5),
				TimeSpan.FromHours(5),
				TimeSpan.FromHours(5),
				TimeSpan.FromHours(25),
				TimeSpan.FromHours(5))
				);
			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory, new DateOnlyPeriod(firstDay, firstDay.AddDays(7)), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.GetSingle(skillDays[5].CurrentDate) //saturday
				.SetDayOff(new DayOffTemplate()); 

			Target.Execute(planningPeriod.Id.Value);

			PersonAssignmentRepository.GetSingle(skillDays[5].CurrentDate) //saturday
				.DayOff().Should().Be.Null();
			PersonAssignmentRepository.GetSingle(skillDays[1].CurrentDate) //tuesday
				.DayOff().Should().Not.Be.Null();
		}

		[Test]
		public void ShouldFixWeeklyRest()
		{
			var weeklyRest = TimeSpan.FromHours(38);
      var firstDay = new DateOnly(2015, 10, 12); //mon
			var weekPeriod = new DateOnlyPeriod(firstDay, firstDay.AddDays(7));
			var planningPeriod = PlanningPeriodRepository.Has(firstDay, 1);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(1), TimeSpan.FromHours(48), TimeSpan.FromHours(1), weeklyRest)
			};
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1);
			schedulePeriod.SetDaysOff(1);
			var agent = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, skill);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			agent.Period(firstDay).RuleSetBag = new RuleSetBag(ruleSet);

			var skillDays = SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				TimeSpan.FromHours(5),
				TimeSpan.FromHours(1),
				TimeSpan.FromHours(5),
				TimeSpan.FromHours(5),
				TimeSpan.FromHours(5),
				TimeSpan.FromHours(25),
				TimeSpan.FromHours(5))
				);
			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory, new DateOnlyPeriod(firstDay, firstDay.AddDays(7)), new TimePeriod(8, 0, 16, 0));
			var mondayAss = PersonAssignmentRepository.GetSingle(skillDays[0].CurrentDate); //monday
			mondayAss.Clear();
			mondayAss.AddActivity(activity, new TimePeriod(12, 0, 20, 0));
			PersonAssignmentRepository.GetSingle(skillDays[5].CurrentDate) //saturday
				.SetDayOff(new DayOffTemplate());

			Target.Execute(planningPeriod.Id.Value);

			var agentRange = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(agent, new ScheduleDictionaryLoadOptions(false, false, false), weekPeriod, scenario)[agent];

			CheckWeeklyRestRule.IsSatisfyBy(agentRange, weekPeriod, weeklyRest)
				.Should().Be.True();
		}

		[Test, Explicit("To be fixed - #36191")]
		public void ShouldOptimizeEvenWhenDayoffsAreNotInLegalStateAtStart()
		{
			var firstDay = new DateOnly(2015, 12, 07); //mon
			var scenario = ScenarioRepository.Has("some name");
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var planningPeriod = PlanningPeriodRepository.Has(firstDay, 2);
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 2);
			schedulePeriod.SetDaysOff(4);
			DayOffRulesRepository.HasDefault(x =>
			{
				x.ConsecutiveDayOffs = new MinMax<int>(1, 1);   //actual setup tp test 
				x.ConsecutiveWorkdays = new MinMax<int>(1, 20); //just to make sure anything goes
			});

			var agent = PersonRepository.Has(new Contract("_"), new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, skill);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			agent.Period(firstDay).RuleSetBag = new RuleSetBag(ruleSet);

			var skillDays = SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				TimeSpan.FromHours(20), //DO at start
				TimeSpan.FromHours(10), //DO at start
				TimeSpan.FromHours(10),
				TimeSpan.FromHours(10),
				TimeSpan.FromHours(10),
				TimeSpan.FromHours(10),
				TimeSpan.FromHours(1),

				TimeSpan.FromHours(20), //DO at start
				TimeSpan.FromHours(10), //DO at start
				TimeSpan.FromHours(10),
				TimeSpan.FromHours(10),
				TimeSpan.FromHours(10),
				TimeSpan.FromHours(10),
				TimeSpan.FromHours(1)
				));
			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory, new DateOnlyPeriod(firstDay, firstDay.AddDays(14)), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.GetSingle(skillDays[0].CurrentDate).SetDayOff(new DayOffTemplate());
			PersonAssignmentRepository.GetSingle(skillDays[1].CurrentDate).SetDayOff(new DayOffTemplate());
			PersonAssignmentRepository.GetSingle(skillDays[7].CurrentDate).SetDayOff(new DayOffTemplate());
			PersonAssignmentRepository.GetSingle(skillDays[8].CurrentDate).SetDayOff(new DayOffTemplate());

			Target.Execute(planningPeriod.Id.Value);

			PersonAssignmentRepository.GetSingle(skillDays[0].CurrentDate).DayOff().Should().Be.Null();
			PersonAssignmentRepository.GetSingle(skillDays[7].CurrentDate).DayOff().Should().Be.Null();
		}

		[Test, Explicit("To be implemented, fix above and see what needs to be done/written")]
		public void ShouldNotLeaveBlankSpotsWhenWhenDayoffsAreNotInLegalStateAtStart()
		{
			//something similar like above but different asserts
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
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
			var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			var agent = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, skill);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), shiftCategory));
			agent.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);

			SkillDayRepository.Has(new List<ISkillDay>
				{
					skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)))
				});

			var dateTime = TimeZoneHelper.ConvertToUtc(dateOnly.Date, agent.PermissionInformation.DefaultTimeZone());
			var assignment = new PersonAssignment(agent, scenario, dateOnly);
			assignment.SetShiftCategory(shiftCategory);
			assignment.AddActivity(phoneActivity, new DateTimePeriod(dateTime.AddHours(8), dateTime.AddHours(8).AddMinutes(10)));
			PersonAssignmentRepository.Add(assignment);

			Target.Execute(planningPeriod.Id.Value);

			var skillDays = SkillDayRepository.FindReadOnlyRange(new DateOnlyPeriod(dateOnly, dateOnly), new List<ISkill> { skill }, scenario);
			var skillStaffPeriods = skillDays.First().SkillStaffPeriodCollection;
			foreach (var skillStaffPeriod in skillStaffPeriods)
			{
				skillStaffPeriod.HasIntraIntervalIssue.Should().Be.False();
			}
		}
	}
}