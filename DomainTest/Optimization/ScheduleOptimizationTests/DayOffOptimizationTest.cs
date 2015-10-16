using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
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

		[Test]
		public void ShouldMoveDayOff()
		{
			var firstDay = new DateOnly(2015,10,12); //mon
			var planningPeriod = PlanningPeriodRepository.HasOneWeek(firstDay);
			var contract = new Contract("_")
			{
				WorkTime = new WorkTime(TimeSpan.FromHours(8)),
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(1), TimeSpan.FromHours(48), TimeSpan.FromHours(1), TimeSpan.FromHours(1))
			};
			var partTimePercentage = new PartTimePercentage("_");
			var contractSchedule = new ContractSchedule("_");
			var team = new Team {Site = new Site("site")};
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");

			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1);
			schedulePeriod.SetDaysOff(1);
			var agent = PersonRepository.Has(contract, contractSchedule, partTimePercentage, team, schedulePeriod);
			agent.AddSkill(new PersonSkill(skill, new Percent(1)) {Active=true}, agent.Period(firstDay));
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
				var currDate = firstDay.AddDays(dayNumber);
				var someTimeDuringTheDay = new DateTime(currDate.Year, currDate.Month, currDate.Day, 8, 0, 0, DateTimeKind.Utc);
				var ass = new PersonAssignment(agent, scenario, currDate);
				ass.AddActivity(activity, new DateTimePeriod(someTimeDuringTheDay, someTimeDuringTheDay.AddHours(8)));
				ass.SetShiftCategory(shiftCategory);
				PersonAssignmentRepository.Add(ass);
			}

			PersonAssignmentRepository.LoadAll().Single(pa => pa.Date == skillDays[5].CurrentDate) //saturday
				.SetDayOff(new DayOffTemplate()); 

			Target.Execute(planningPeriod.Id.Value);

			PersonAssignmentRepository.LoadAll().Single(pa => pa.Date == skillDays[1].CurrentDate)
				.DayOff().Should().Not.Be.Null();
		}
	}
}