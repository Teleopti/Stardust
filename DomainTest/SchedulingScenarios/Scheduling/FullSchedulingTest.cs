using System;
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

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	public class FullSchedulingTest
	{
		public FullScheduling Target;
		public FakePersonRepository PersonRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillRepository SkillRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public FakePersonAssignmentRepository AssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeAgentDayScheduleTagRepository AgentDayScheduleTagRepository;

		[Test]
		public void ShouldNotCreateTags()
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var period = new DateOnlyPeriod(firstDay, firstDay.AddDays(7));
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(168), TimeSpan.FromHours(1), TimeSpan.FromHours(1))
			};
			var agent = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), skill);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			agent.Period(firstDay).RuleSetBag = new RuleSetBag(ruleSet);
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				TimeSpan.FromHours(10),
				TimeSpan.FromHours(10),
				TimeSpan.FromHours(10),
				TimeSpan.FromHours(10),
				TimeSpan.FromHours(10),
				TimeSpan.FromHours(10),
				TimeSpan.FromHours(10))
				);

			Target.DoScheduling(period);

			AssignmentRepository.Find(new[] {agent}, period, scenario).Should().Not.Be.Empty();
			AgentDayScheduleTagRepository.LoadAll().Should().Be.Empty();
		}

		[Test]
		public void ShouldNotScheduleDaysOffOutsideSelectedDays()
		{
			var firstDay = new DateOnly(2015, 10, 12); 
			var period = new DateOnlyPeriod(firstDay, firstDay.AddDays(7)); //12 to 18
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(168), TimeSpan.FromHours(1), TimeSpan.FromHours(1))
			};
			var contractSchedule = ContractScheduleFactory.CreateWorkingWeekContractSchedule();
			var agent = PersonRepository.Has(contract, contractSchedule, new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), skill);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			agent.Period(firstDay).RuleSetBag = new RuleSetBag(ruleSet);
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				TimeSpan.FromHours(10),
				TimeSpan.FromHours(10),
				TimeSpan.FromHours(10),
				TimeSpan.FromHours(10),
				TimeSpan.FromHours(10),
				TimeSpan.FromHours(10),
				TimeSpan.FromHours(10))
				);

			Target.DoScheduling(new DateOnlyPeriod(new DateOnly(2015,10,16), new DateOnly(2015, 10, 17))); //friday saturday

			var assignments = AssignmentRepository.Find(new[] {agent}, period, scenario);
			assignments.Count.Should().Be.EqualTo(1);

			var assignment = assignments.First();
			assignment.Date.Should().Be.EqualTo(new DateOnly(2015, 10, 17));
			assignment.DayOff().Should().Not.Be.Null();

		}
	}
}