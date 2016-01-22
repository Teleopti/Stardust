using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
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
	public class IntradayOptimizationTest
	{
		public FakeSkillRepository SkillRepository;
		public FakePersonRepository PersonRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakePlanningPeriodRepository PlanningPeriodRepository;

		[Test, Ignore("To be fixed... #36617")]
		public void ShouldMoveLunchToLowerDemandInterval()
		{
			//skifts med phone 8 - 17, lunch varje timme
			//utgångsläge lunch 11 - 12
			//efter lunch 10 -11
			var phoneActivity = ActivityFactory.CreateActivity("phone");
			var lunchActivity = ActivityFactory.CreateActivity("lunch");	
			var skill = SkillRepository.Has("skill", phoneActivity);
			var dateOnly = new DateOnly(2015, 10, 12);
			var dateTime = TimeZoneHelper.ConvertToUtc(dateOnly.Date, skill.TimeZone);
			var planningPeriod = PlanningPeriodRepository.Has(dateOnly, 1);
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1);
			var agent = PersonRepository.Has(new Contract("_"), new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, skill);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(17, 0, 17, 0, 15), shiftCategory));
			ruleSet.AddExtender(new ActivityAbsoluteStartExtender(lunchActivity, new TimePeriodWithSegment(1, 0, 1, 0, 60), new TimePeriodWithSegment(8, 0, 16, 0, 60)));
			
			agent.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);
	
			SkillDayRepository.Has(new List<ISkillDay>
				{
					skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(10), new Tuple<int, TimeSpan>(10, TimeSpan.FromHours(5)))
				});

	
			var assignment = new PersonAssignment(agent, scenario, dateOnly);
			assignment.AddActivity(phoneActivity, new DateTimePeriod(dateTime.AddHours(8), dateTime.AddHours(17)));
			assignment.AddActivity(lunchActivity, new DateTimePeriod(dateTime.AddHours(11), dateTime.AddHours(12)));
			PersonAssignmentRepository.Add(assignment);

			//Target.Doit(planning);

			PersonAssignmentRepository.GetSingle(dateOnly).MainActivities().Single(x => x.Payload.Equals(lunchActivity)).Period
				.Should().Be.EqualTo(new DateTimePeriod(dateTime.AddHours(10), dateTime.AddHours(11)));

		}
	}
}
