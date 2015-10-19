using System;
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
		public IScheduleRepository ScheduleRepository;
		public IPersonWeekViolatingWeeklyRestSpecification CheckWeeklyRestRule;

		[Test]
		public void ShouldMoveDayOffToDayWithLessDemand()
		{
			var firstDay = new DateOnly(2015,10,12); //mon
			var planningPeriod = PlanningPeriodRepository.HasOneWeek(firstDay);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");

			var contract = new Contract("_")
			{
				WorkTime = new WorkTime(TimeSpan.FromHours(8)),
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(1), TimeSpan.FromHours(48), TimeSpan.FromHours(1), TimeSpan.FromHours(1))
			};
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1);
			schedulePeriod.SetDaysOff(1);
			var agent = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site")}, schedulePeriod);
			agent.AddSkill(new PersonSkill(skill, new Percent(1)), agent.Period(firstDay));
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var shiftBag = new RuleSetBag();
			shiftBag.AddRuleSet(ruleSet);
			agent.Period(firstDay).RuleSetBag = shiftBag;

			var skillDays = SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				TimeSpan.FromHours(5),
				TimeSpan.FromHours(1),
				TimeSpan.FromHours(5),
				TimeSpan.FromHours(5),
				TimeSpan.FromHours(5),
				TimeSpan.FromHours(25),
				TimeSpan.FromHours(5))
				);
			for (var dayNumber = 0; dayNumber < 7; dayNumber++)
			{
				var ass = new PersonAssignment(agent, scenario, firstDay.AddDays(dayNumber));
				ass.AddActivity(activity, new TimePeriod(8, 0, 16, 0));
				ass.SetShiftCategory(shiftCategory);
				PersonAssignmentRepository.Add(ass);
			}
			PersonAssignmentRepository.LoadAll().Single(pa => pa.Date == skillDays[5].CurrentDate) //saturday
				.SetDayOff(new DayOffTemplate()); 

			Target.Execute(planningPeriod.Id.Value);

			PersonAssignmentRepository.LoadAll().Single(pa => pa.Date == skillDays[5].CurrentDate) //saturday
				.DayOff().Should().Be.Null();
			PersonAssignmentRepository.LoadAll().Single(pa => pa.Date == skillDays[1].CurrentDate) //tuesday
				.DayOff().Should().Not.Be.Null();
		}

		[Test]
		public void ShouldFixWeeklyRest()
		{
			var weeklyRest = TimeSpan.FromHours(38);
      var firstDay = new DateOnly(2015, 10, 12); //mon
			var weekPeriod = new DateOnlyPeriod(firstDay, firstDay.AddDays(7));
			var planningPeriod = PlanningPeriodRepository.HasOneWeek(firstDay);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");

			var contract = new Contract("_")
			{
				WorkTime = new WorkTime(TimeSpan.FromHours(8)),
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(1), TimeSpan.FromHours(48), TimeSpan.FromHours(1), weeklyRest)
			};
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1);
			schedulePeriod.SetDaysOff(1);
			var agent = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod);
			agent.AddSkill(new PersonSkill(skill, new Percent(1)), agent.Period(firstDay));
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var shiftBag = new RuleSetBag();
			shiftBag.AddRuleSet(ruleSet);
			agent.Period(firstDay).RuleSetBag = shiftBag;

			var skillDays = SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				TimeSpan.FromHours(5),
				TimeSpan.FromHours(1),
				TimeSpan.FromHours(5),
				TimeSpan.FromHours(5),
				TimeSpan.FromHours(5),
				TimeSpan.FromHours(25),
				TimeSpan.FromHours(5))
				);
			for (var dayNumber = -1; dayNumber < 8; dayNumber++)
			{
				var ass = new PersonAssignment(agent, scenario, firstDay.AddDays(dayNumber));
				ass.AddActivity(activity, new TimePeriod(8, 0, 16, 0));
				ass.SetShiftCategory(shiftCategory);
				PersonAssignmentRepository.Add(ass);
			}
			var mondayAss = PersonAssignmentRepository.LoadAll().Single(pa => pa.Date == skillDays[0].CurrentDate); //monday
			mondayAss.Clear();
			mondayAss.AddActivity(activity, new TimePeriod(12, 0, 20, 0));
			PersonAssignmentRepository.LoadAll().Single(pa => pa.Date == skillDays[5].CurrentDate) //saturday
				.SetDayOff(new DayOffTemplate());

			Target.Execute(planningPeriod.Id.Value);

			var agentRange = ScheduleRepository.FindSchedulesForPersonOnlyInGivenPeriod(agent, new ScheduleDictionaryLoadOptions(false, false, false), weekPeriod, scenario)[agent];

			CheckWeeklyRestRule.IsSatisfyBy(agentRange, weekPeriod, weeklyRest)
				.Should().Be.True();
		}
	}
}