using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
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
	[Toggle(Toggles.ResourcePlanner_JumpOutWhenLargeGroupIsHalfOptimized_37049)]
	[DomainTest]
	public class IntradayOptimizationStopIfLargeGroupTest
	{
		public FakeSkillRepository SkillRepository;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonRepository PersonRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public IntradayOptimization Target;
		public IntradayOptmizerLimiter IntradayOptmizerLimiter;

		[Test]
		public void ShouldOptimizeNoneIf0PercentShouldBeOptimized()
		{
			IntradayOptmizerLimiter.SetFromTest(new Percent(0), 0);

			var phoneActivity = ActivityFactory.CreateActivity("phone");

			var skill = SkillRepository.Has("skill", phoneActivity);
			var dateOnly = new DateOnly(2015, 10, 12);
			var planningPeriod = PlanningPeriodRepository.Has(dateOnly, 1);
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1);
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11),
				TimeSpan.FromHours(36));
			var contract = new Contract("contract")
			{
				WorkTimeDirective = worktimeDirective,
				PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9)
			};
			var agent = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"),
				new Team {Site = new Site("site")}, schedulePeriod, skill);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet =
				new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15),
					new TimePeriodWithSegment(17, 15, 17, 15, 15), shiftCategory));
			agent.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);

			SkillDayRepository.Has(new List<ISkillDay>
			{
				skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60),
					new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)))
			});

			var dateTime = TimeZoneHelper.ConvertToUtc(dateOnly.Date, agent.PermissionInformation.DefaultTimeZone());
			var oldAssPeriod = new DateTimePeriod(dateTime.AddHours(8), dateTime.AddHours(17));
			var assignment = new PersonAssignment(agent, scenario, dateOnly);
			assignment.SetShiftCategory(shiftCategory);
			assignment.AddActivity(phoneActivity, oldAssPeriod);
			PersonAssignmentRepository.Add(assignment);

			Target.Optimize(planningPeriod.Id.Value);

			PersonAssignmentRepository.GetSingle(dateOnly).Period
				.Should().Be.EqualTo(oldAssPeriod);
		}

		[Test]
		public void ShouldOptimizeAllIf100PercentShouldBeOptimized()
		{
			IntradayOptmizerLimiter.SetFromTest(new Percent(1), 0);

			var phoneActivity = ActivityFactory.CreateActivity("phone");

			var skill = SkillRepository.Has("skill", phoneActivity);
			var dateOnly = new DateOnly(2015, 10, 12);
			var planningPeriod = PlanningPeriodRepository.Has(dateOnly, 1);
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1);
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11),
				TimeSpan.FromHours(36));
			var contract = new Contract("contract")
			{
				WorkTimeDirective = worktimeDirective,
				PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9)
			};
			var agent = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"),
				new Team {Site = new Site("site")}, schedulePeriod, skill);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet =
				new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15),
					new TimePeriodWithSegment(17, 15, 17, 15, 15), shiftCategory));
			agent.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);

			SkillDayRepository.Has(new List<ISkillDay>
			{
				skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60),
					new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)))
			});

			var dateTime = TimeZoneHelper.ConvertToUtc(dateOnly.Date, agent.PermissionInformation.DefaultTimeZone());
			var oldAssPeriod = new DateTimePeriod(dateTime.AddHours(8), dateTime.AddHours(17));
			var assignment = new PersonAssignment(agent, scenario, dateOnly);
			assignment.SetShiftCategory(shiftCategory);
			assignment.AddActivity(phoneActivity, oldAssPeriod);
			PersonAssignmentRepository.Add(assignment);

			Target.Optimize(planningPeriod.Id.Value);

			PersonAssignmentRepository.GetSingle(dateOnly).Period
				.Should().Not.Be.EqualTo(oldAssPeriod);
		}

		[Test]
		public void ShouldOptimizeHalfGroupIf50PercentShouldBeOptimized()
		{
			IntradayOptmizerLimiter.SetFromTest(new Percent(0.5), 0);

			var phoneActivity = ActivityFactory.CreateActivity("phone");

			var skill = SkillRepository.Has("skill", phoneActivity);
			var dateOnly = new DateOnly(2015, 10, 12);
			var planningPeriod = PlanningPeriodRepository.Has(dateOnly, 1);
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
			var ruleSet =
				new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15),
					new TimePeriodWithSegment(17, 15, 17, 15, 15), shiftCategory));
			var agent1 = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"),
				new Team {Site = new Site("site")}, schedulePeriod, skill);
			agent1.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);
			var agent2 = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"),
				new Team {Site = new Site("site")}, schedulePeriod, skill);
			agent2.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);

			SkillDayRepository.Has(new List<ISkillDay>
			{
				skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60),
					new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)))
			});

			var dateTime = TimeZoneHelper.ConvertToUtc(dateOnly.Date, agent1.PermissionInformation.DefaultTimeZone());
			var oldAssPeriod = new DateTimePeriod(dateTime.AddHours(8), dateTime.AddHours(17));
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly);
			ass1.SetShiftCategory(shiftCategory);
			ass1.AddActivity(phoneActivity, oldAssPeriod);
			PersonAssignmentRepository.Add(ass1);
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly);
			ass2.SetShiftCategory(shiftCategory);
			ass2.AddActivity(phoneActivity, oldAssPeriod);
			PersonAssignmentRepository.Add(ass2);

			Target.Optimize(planningPeriod.Id.Value);

			PersonAssignmentRepository
				.LoadAll()
				.Select(ass => ass.Period.StartDateTime)
				.Count(start => start == oldAssPeriod.StartDateTime)
				.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldOptimizeHalfGroupWhenTwoAgentsAndTwoDaysIf50PercentShouldBeOptimized()
		{
			IntradayOptmizerLimiter.SetFromTest(new Percent(0.5), 0);

			var phoneActivity = ActivityFactory.CreateActivity("phone");

			var skill = SkillRepository.Has("skill", phoneActivity);
			var dateOnly = new DateOnly(2015, 10, 12);
			var planningPeriod = PlanningPeriodRepository.Has(dateOnly, 1);
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
			var ruleSet =
				new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15),
					new TimePeriodWithSegment(17, 15, 17, 15, 15), shiftCategory));
			var agent1 = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, skill);
			agent1.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);
			var agent2 = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, skill);
			agent2.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);

			SkillDayRepository.Has(new List<ISkillDay>
			{
				skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360))),
				skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly.AddDays(1), TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)))
			});

			var dateTime = TimeZoneHelper.ConvertToUtc(dateOnly.Date, agent1.PermissionInformation.DefaultTimeZone());
			var oldAssPeriod = new DateTimePeriod(dateTime.AddHours(8), dateTime.AddHours(17));
			var ass1_1 = new PersonAssignment(agent1, scenario, dateOnly);
			ass1_1.SetShiftCategory(shiftCategory);
			ass1_1.AddActivity(phoneActivity, oldAssPeriod);
			var ass2_1 = new PersonAssignment(agent2, scenario, dateOnly);
			ass2_1.SetShiftCategory(shiftCategory);
			ass2_1.AddActivity(phoneActivity, oldAssPeriod);
			var ass1_2 = new PersonAssignment(agent1, scenario, dateOnly.AddDays(1));
			ass1_2.SetShiftCategory(shiftCategory);
			ass1_2.AddActivity(phoneActivity, oldAssPeriod.MovePeriod(TimeSpan.FromDays(1)));
			var ass2_2 = new PersonAssignment(agent2, scenario, dateOnly.AddDays(1));
			ass2_2.SetShiftCategory(shiftCategory);
			ass2_2.AddActivity(phoneActivity, oldAssPeriod.MovePeriod(TimeSpan.FromDays(1)));
			PersonAssignmentRepository.Add(ass1_1);
			PersonAssignmentRepository.Add(ass2_1);
			PersonAssignmentRepository.Add(ass1_2);
			PersonAssignmentRepository.Add(ass2_2);

			Target.Optimize(planningPeriod.Id.Value);

			var assStartTimes = PersonAssignmentRepository.LoadAll().Select(ass => ass.Period.StartDateTime);
			assStartTimes.Count(start => start == oldAssPeriod.StartDateTime)
				.Should().Be.EqualTo(1);
			assStartTimes.Count(start => start == oldAssPeriod.StartDateTime)
				.Should().Be.EqualTo(1);
		}
	}
}
