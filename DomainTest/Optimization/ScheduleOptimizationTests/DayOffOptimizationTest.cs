using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
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

		[Test, Explicit("2 be continued")]
		public void ShouldMoveDayOff()
		{
			var firstDay = new DateOnly(2015,10,12); //mon
			var planningGroupId = Guid.NewGuid();

			var contract = new Contract("_");
			var partTimePercentage = new PartTimePercentage("_");
			var contractSchedule = new ContractSchedule("_");
			var team = new Team {Site = new Site("site")};

			var skill = SkillRepository.Has("skill");
			var scenario = ScenarioRepository.Has("some name");
			var activity = ActivityRepository.Has("_");
			var agent = PersonRepository.Has(contract, contractSchedule, partTimePercentage, team);
			agent.AddSkill(new PersonSkill(skill, new Percent(100)), agent.Period(firstDay));

			SkillDayRepository.HasSkillDayWithDemand(skill, firstDay, TimeSpan.FromHours(5), scenario);
			var skillDay1 = SkillDayRepository.HasSkillDayWithDemand(skill, firstDay.AddDays(1), TimeSpan.FromHours(1), scenario);
			SkillDayRepository.HasSkillDayWithDemand(skill, firstDay.AddDays(2), TimeSpan.FromHours(5), scenario);
			SkillDayRepository.HasSkillDayWithDemand(skill, firstDay.AddDays(3), TimeSpan.FromHours(5), scenario);
			SkillDayRepository.HasSkillDayWithDemand(skill, firstDay.AddDays(4), TimeSpan.FromHours(5), scenario);
			var skillDay5 = SkillDayRepository.HasSkillDayWithDemand(skill, firstDay.AddDays(5), TimeSpan.FromHours(25), scenario);
			SkillDayRepository.HasSkillDayWithDemand(skill, firstDay.AddDays(6), TimeSpan.FromHours(5), scenario);

			for (var dayNumber = 0; dayNumber < 7; dayNumber++)
			{
				var currDate = firstDay.AddDays(dayNumber);
				var someTimeDuringTheDay = new DateTime(currDate.Year, currDate.Month, currDate.Day, 8, 0, 0, DateTimeKind.Utc);
				var ass = new PersonAssignment(agent, scenario, currDate);
				ass.AddActivity(activity, new DateTimePeriod(someTimeDuringTheDay, someTimeDuringTheDay.AddHours(8)));
				PersonAssignmentRepository.Add(ass);
			}
			PersonAssignmentRepository.LoadAll().Single(pa => pa.Date == skillDay5.CurrentDate) //saturday
				.SetDayOff(new DayOffTemplate()); //TODO: behöver förmodligen använda en template som finns i repo (?)

			Target.Execute(planningGroupId);

			PersonAssignmentRepository.LoadAll().Single(pa => pa.Date == skillDay1.CurrentDate)
				.DayOff().Should().Not.Be.Null();
		}
	}
}