using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
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
			var contract = new Contract("contract")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36)),
				PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9)
			};
			var agent = PersonRepository.Has(contract, schedulePeriod, skill);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), shiftCategory));
			agent.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);
			SkillDayRepository.Has(new List<ISkillDay>
			{
				skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)))
			});
			PersonAssignmentRepository.Has(agent, scenario, phoneActivity, shiftCategory, dateOnly, new TimePeriod(8, 0, 17, 0));

			Target.Optimize(planningPeriod.Id.Value);

			var dateTime = TimeZoneHelper.ConvertToUtc(dateOnly.Date, agent.PermissionInformation.DefaultTimeZone());
			var oldAssPeriod = new DateTimePeriod(dateTime.AddHours(8), dateTime.AddHours(17));
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
			var contract = new Contract("contract")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36)),
				PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9)
			};
			var agent = PersonRepository.Has(contract, schedulePeriod, skill);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), shiftCategory));
			agent.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);
			SkillDayRepository.Has(new List<ISkillDay>
			{
				skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)))
			});
			PersonAssignmentRepository.Has(agent, scenario, phoneActivity, shiftCategory, dateOnly, new TimePeriod(8, 0, 17, 0));

			Target.Optimize(planningPeriod.Id.Value);

			var dateTime = TimeZoneHelper.ConvertToUtc(dateOnly.Date, agent.PermissionInformation.DefaultTimeZone());
			var oldAssPeriod = new DateTimePeriod(dateTime.AddHours(8), dateTime.AddHours(17));
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
			var contract = new Contract("contract")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36)),
				PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9)
			};
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), shiftCategory));
			var agent1 = PersonRepository.Has(contract, schedulePeriod, skill);
			agent1.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);
			var agent2 = PersonRepository.Has(contract, schedulePeriod, skill);
			agent2.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);
			SkillDayRepository.Has(new List<ISkillDay>
			{
				skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60),
					new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)))
			});
			PersonAssignmentRepository.Has(agent1, scenario, phoneActivity, shiftCategory, dateOnly, new TimePeriod(8, 0, 17, 0));
			PersonAssignmentRepository.Has(agent2, scenario, phoneActivity, shiftCategory, dateOnly, new TimePeriod(8, 0, 17, 0));

			Target.Optimize(planningPeriod.Id.Value);

			var dateTime = TimeZoneHelper.ConvertToUtc(dateOnly.Date, agent1.PermissionInformation.DefaultTimeZone());
			var oldAssPeriod = new DateTimePeriod(dateTime.AddHours(8), dateTime.AddHours(17));
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
			var contract = new Contract("contract")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36)),
				PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9)
			};
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), shiftCategory));
			var agent1 = PersonRepository.Has(contract, schedulePeriod, skill);
			agent1.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);
			var agent2 = PersonRepository.Has(contract, schedulePeriod, skill);
			agent2.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);
			SkillDayRepository.Has(new List<ISkillDay>
			{
				skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360))),
				skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly.AddDays(1), TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)))
			});
			PersonAssignmentRepository.Has(agent1, scenario, phoneActivity, shiftCategory, new DateOnlyPeriod(dateOnly, dateOnly.AddDays(1)), new TimePeriod(8, 0, 17, 0));
			PersonAssignmentRepository.Has(agent2, scenario, phoneActivity, shiftCategory, new DateOnlyPeriod(dateOnly, dateOnly.AddDays(1)), new TimePeriod(8, 0, 17, 0));

			Target.Optimize(planningPeriod.Id.Value);

			var dateTime = TimeZoneHelper.ConvertToUtc(dateOnly.Date, agent1.PermissionInformation.DefaultTimeZone());
			var oldAssPeriod = new DateTimePeriod(dateTime.AddHours(8), dateTime.AddHours(17));
			var assStartTimes = PersonAssignmentRepository.LoadAll().Select(ass => ass.Period.StartDateTime);
			assStartTimes.Count(start => start == oldAssPeriod.StartDateTime)
				.Should().Be.EqualTo(1);
			assStartTimes.Count(start => start == oldAssPeriod.StartDateTime)
				.Should().Be.EqualTo(1);
		}
	}
}
