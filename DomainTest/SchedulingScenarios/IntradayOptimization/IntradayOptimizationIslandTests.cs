using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTestWithStaticDependenciesAvoidUse]
	[UseEventPublisher(typeof(RunInProcessEventPublisher))]
	[LoggedOnAppDomain]
	[TestFixture(true)]
	[TestFixture(false)]
	public class IntradayOptimizationIslandTests : ISetup, IConfigureToggleManager
	{
		private readonly bool _resourcePlannerSplitBigIslands42049;
		public IntradayOptimizationFromWeb Target;
		public FakeSkillRepository SkillRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonRepository PersonRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeSkillDayRepositorySimulateNewUnitOfWork SkillDayRepository;
		public FakePlanningPeriodRepository PlanningPeriodRepository;

		public IntradayOptimizationIslandTests(bool resourcePlannerSplitBigIslands42049)
		{
			_resourcePlannerSplitBigIslands42049 = resourcePlannerSplitBigIslands42049;
		}
		
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
			SkillDayRepository.Has(new List<Func<ISkillDay>>
			{
				() => skill1.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(10), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360))),
				() => skill2.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(10), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)))
			});
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

		[Test, Ignore("to be fixed")]
		public void ShouldNotUseSkillsThatWereRemovedDuringIslandCreation()
		{
			var date = new DateOnly(2015, 10, 12);
			var scenario = ScenarioRepository.Has("some name");
			var activityAB = ActivityFactory.CreateActivity("AB");
			var activityC = ActivityFactory.CreateActivity("C");
			var skillA = SkillRepository.Has("skillA", activityAB);
			var skillB = SkillRepository.Has("skillB", activityAB);
			var skillC = SkillRepository.Has("skillC", activityC);
			var schedulePeriod = new SchedulePeriod(date, SchedulePeriodType.Day, 1);
//			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
//			var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			var ruleSetAB = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activityAB, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory("_").WithId()));
			var ruleSetC = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activityC, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory("_").WithId()));
			var ruleSetBag = new RuleSetBag(ruleSetAB);
			ruleSetBag.AddRuleSet(ruleSetC);
			var agentA = PersonRepository.Has(new Contract("contract"), new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, ruleSetBag, skillA);
			var agentABC = PersonRepository.Has(new Contract("contract"), new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, ruleSetBag, skillA);
			var planningPeriod = PlanningPeriodRepository.HasOneDayPeriod(date);
			PersonAssignmentRepository.Has(agentA, scenario, new Activity("X"), new ShiftCategory("_").WithId(), date, new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.Has(agentABC, scenario, new Activity("X"), new ShiftCategory("_").WithId(), date, new TimePeriod(8, 0, 16, 0));
			SkillDayRepository.Has(new List<Func<ISkillDay>>
			{
				() => skillA.CreateSkillDayWithDemand(scenario, date, 100),
				() => skillB.CreateSkillDayWithDemand(scenario, date, 1),
				() => skillC.CreateSkillDayWithDemand(scenario, date, 10)
			});

			Target.Execute(planningPeriod.Id.Value);

			PersonAssignmentRepository.GetSingle(date, agentABC).ShiftLayers.Single().Payload
				.Should().Be.EqualTo(activityC);
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeSkillDayRepositorySimulateNewUnitOfWork>().For<ISkillDayRepository>();
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			if (_resourcePlannerSplitBigIslands42049)
				toggleManager.Enable(Toggles.ResourcePlanner_SplitBigIslands_42049);
		}
	}
}