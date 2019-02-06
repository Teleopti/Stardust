using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.Filters;
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
	public class IntradayOptimizationPlanningGroupTest : IntradayOptimizationScenarioTest
	{
		public FakeSkillRepository SkillRepository;
		public FakePersonRepository PersonRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public IntradayOptimizationFromWeb Target;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public FakePlanningGroupRepository PlanningGroupRepository;
		
		[Test]
		public void ShouldIntradayOptimizeForPlanningGroup()
		{
			var team = new Team { Site = new Site("site") };
			var planningGroup = new PlanningGroup().AddFilter(new TeamFilter(team));
			PlanningGroupRepository.Add(planningGroup);
			var phoneActivity = ActivityFactory.CreateActivity("phone");
			var skill = SkillRepository.Has("skill", phoneActivity);
			var dateOnly = new DateOnly(2015, 10, 12);
			var scenario = ScenarioRepository.LoadDefaultScenario();
			var schedulePeriod = new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1);
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
			var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), shiftCategory));
			var agent = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), team, schedulePeriod, ruleSet, skill);
			var planningPeriod = PlanningPeriodRepository.Has(dateOnly, 1, planningGroup);
			SkillDayRepository.Has(new List<ISkillDay>
			{
				skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)))
			});

			PersonAssignmentRepository.Has(agent, scenario, phoneActivity, shiftCategory, dateOnly, new TimePeriod(8, 0, 17, 0));

			Target.Execute(planningPeriod.Id.GetValueOrDefault());

			var dateTime = TimeZoneHelper.ConvertToUtc(dateOnly.Date, agent.PermissionInformation.DefaultTimeZone());
			PersonAssignmentRepository.GetSingle(dateOnly).Period
				.Should().Be.EqualTo(new DateTimePeriod(dateTime.AddHours(8).AddMinutes(15), dateTime.AddHours(17).AddMinutes(15)));
		}

		[Test]
		public void ShouldNotIntradayOptimizeForAgentNotInPlanningGroup()
		{
			var planningGroup = new PlanningGroup().AddFilter(new TeamFilter(new Team()));
			PlanningGroupRepository.Add(planningGroup);
			var phoneActivity = ActivityFactory.CreateActivity("phone");
			var skill = SkillRepository.Has("skill", phoneActivity);
			var dateOnly = new DateOnly(2015, 10, 12);
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1);
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
			var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), shiftCategory));
			var agent = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, ruleSet, skill);
			var planningPeriod = PlanningPeriodRepository.Has(dateOnly, 1, planningGroup);
			SkillDayRepository.Has(new List<ISkillDay>
			{
				skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)))
			});

			PersonAssignmentRepository.Has(agent, scenario, phoneActivity, shiftCategory, dateOnly, new TimePeriod(8, 0, 17, 0));

			Target.Execute(planningPeriod.Id.GetValueOrDefault());

			var dateTime = TimeZoneHelper.ConvertToUtc(dateOnly.Date, agent.PermissionInformation.DefaultTimeZone());
			PersonAssignmentRepository.GetSingle(dateOnly).Period
				.Should().Be.EqualTo(new DateTimePeriod(dateTime.AddHours(8), dateTime.AddHours(17)));
		}
	}
}