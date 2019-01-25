using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTest]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	[LoggedOnAppDomain]
	public class IntradayOptimizationIslandTests : IntradayOptimizationScenarioTest
	{
		public IntradayOptimizationFromWeb Target;
		public FakeSkillRepository SkillRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonRepository PersonRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakePlanningPeriodRepository PlanningPeriodRepository;

		[Test]
		public void ShouldWorkCompleteWayWithMultipleIslands()
		{
			var activity = ActivityFactory.CreateActivity("phone");
			var skill1 = SkillRepository.Has("skill", activity);
			var skill2 = SkillRepository.Has("skill", activity);
			var dateOnly = new DateOnly(2015, 10, 12);
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1);
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
			var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), shiftCategory));
			var agent1 = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, ruleSet, skill1);
			var agent2 = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, ruleSet, skill2);
			var planningPeriod = PlanningPeriodRepository.Has(dateOnly, 1);
			SkillDayRepository.Has(
				skill1.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(10), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360))),
				skill2.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(10), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360))));
			PersonAssignmentRepository.Has(agent1, scenario, activity, shiftCategory, dateOnly, new TimePeriod(8, 0, 17, 0));
			PersonAssignmentRepository.Has(agent2, scenario, activity, shiftCategory, dateOnly, new TimePeriod(8, 0, 17, 0));

			Target.Execute(planningPeriod.Id.Value);

			var dateTime1 = TimeZoneHelper.ConvertToUtc(dateOnly.Date, agent1.PermissionInformation.DefaultTimeZone());
			PersonAssignmentRepository.GetSingle(dateOnly, agent1).Period
				.Should().Be.EqualTo(new DateTimePeriod(dateTime1.AddHours(8).AddMinutes(15), dateTime1.AddHours(17).AddMinutes(15)));

			var dateTime2 = TimeZoneHelper.ConvertToUtc(dateOnly.Date, agent2.PermissionInformation.DefaultTimeZone());
			PersonAssignmentRepository.GetSingle(dateOnly, agent2).Period
				.Should().Be.EqualTo(new DateTimePeriod(dateTime2.AddHours(8).AddMinutes(15), dateTime2.AddHours(17).AddMinutes(15)));
		}
	}
}