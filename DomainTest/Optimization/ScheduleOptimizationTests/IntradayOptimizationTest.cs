﻿using System;
using System.Collections.Generic;
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
	public class IntradayOptimizationTest
	{
		public FakeSkillRepository SkillRepository;
		public FakePersonRepository PersonRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public IntradayOptimization Target;
		public IPersonWeekViolatingWeeklyRestSpecification CheckWeeklyRestRule;
		public IScheduleRepository ScheduleRepository;

		[Test]
		public void ShouldUseShiftThatCoverHigherDemand()
		{
			//before:	shift with phone 8 - 17
			//demand:	higher 17 - 18
			//after:	shift 8:15 - 17:15
			var phoneActivity = ActivityFactory.CreateActivity("phone");
			phoneActivity.RequiresSkill = true;
			
			var skill = SkillRepository.Has("skill", phoneActivity);
			var dateOnly = new DateOnly(2015, 10, 12);
			var planningPeriod = PlanningPeriodRepository.Has(dateOnly, 1);
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1);
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
			var contract = new Contract("contract"){WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9)};
			var agent = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, skill);	
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), shiftCategory));
			agent.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);

			SkillDayRepository.Has(new List<ISkillDay>
				{
					skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)))
				});

			var dateTime = TimeZoneHelper.ConvertToUtc(dateOnly.Date, agent.PermissionInformation.DefaultTimeZone());
			var assignment = new PersonAssignment(agent, scenario, dateOnly);
			assignment.SetShiftCategory(shiftCategory);
			assignment.AddActivity(phoneActivity, new DateTimePeriod(dateTime.AddHours(8), dateTime.AddHours(17)));
			PersonAssignmentRepository.Add(assignment);

			Target.Optimize(planningPeriod.Id.Value);

			PersonAssignmentRepository.GetSingle(dateOnly).Period
				.Should().Be.EqualTo(new DateTimePeriod(dateTime.AddHours(8).AddMinutes(15), dateTime.AddHours(17).AddMinutes(15)));
		}

		[Test]
		public void ShouldNotOptimizeDaysWithOvertime()
		{
			//before:	shift with phone 8 - 17, overtime 8 - 9
			//demand:	higher 17 - 18
			//after:	shift 8 - 17
			var phoneActivity = ActivityFactory.CreateActivity("phone");
			phoneActivity.RequiresSkill = true;
			var skill = SkillRepository.Has("skill", phoneActivity);
			var dateOnly = new DateOnly(2015, 10, 12);
			var planningPeriod = PlanningPeriodRepository.Has(dateOnly, 1);
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1);
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
			var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			var agent = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, skill);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), shiftCategory));
			agent.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);

			SkillDayRepository.Has(new List<ISkillDay>
				{
					skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)))
				});

			var dateTime = TimeZoneHelper.ConvertToUtc(dateOnly.Date, agent.PermissionInformation.DefaultTimeZone());
			var assignment = new PersonAssignment(agent, scenario, dateOnly);
			assignment.SetShiftCategory(shiftCategory);
			assignment.AddActivity(phoneActivity, new DateTimePeriod(dateTime.AddHours(9), dateTime.AddHours(17)));
			assignment.AddOvertimeActivity(phoneActivity, new DateTimePeriod(dateTime.AddHours(8), dateTime.AddHours(9)),
											MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("multiplicator", MultiplicatorType.Overtime));
			PersonAssignmentRepository.Add(assignment);
			
			Target.Optimize(planningPeriod.Id.Value);

			PersonAssignmentRepository.GetSingle(dateOnly).Period
				.Should().Be.EqualTo(new DateTimePeriod(dateTime.AddHours(8), dateTime.AddHours(17)));
		}

		[Test]
		public void ShouldResolveWeeklyRest()
		{
			var weeklyRest = TimeSpan.FromHours(38);
			var phoneActivity = ActivityFactory.CreateActivity("phone");
			phoneActivity.RequiresSkill = true;
			phoneActivity.InWorkTime = true;
			var skill = SkillRepository.Has("skill", phoneActivity);
			var dateOnly = new DateOnly(2015, 10, 12);
			var weekPeriod = new DateOnlyPeriod(dateOnly, dateOnly.AddDays(7));
			var planningPeriod = PlanningPeriodRepository.Has(dateOnly, 1);
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1);
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), weeklyRest);
			var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			var agent = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, skill);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), shiftCategory));
			agent.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);

			PersonAssignmentRepository.Has(agent, scenario, phoneActivity, shiftCategory, new DateOnlyPeriod(dateOnly, dateOnly.AddDays(7)), new TimePeriod(8, 0, 16, 0));
			Target.Optimize(planningPeriod.Id.Value);
			var agentRange = ScheduleRepository.FindSchedulesForPersonOnlyInGivenPeriod(agent, new ScheduleDictionaryLoadOptions(false, false, false), weekPeriod, scenario)[agent];
			CheckWeeklyRestRule.IsSatisfyBy(agentRange, weekPeriod, weeklyRest).Should().Be.True();	
		}

		[Test]
		public void ShouldNotCalculateDayAfterIfTodayIsNotANightShift()
		{
			var phoneActivity = ActivityFactory.CreateActivity("phone");
			phoneActivity.RequiresSkill = true;
			var skill = SkillRepository.Has("skill", phoneActivity);
			var dateOnly = new DateOnly(2015, 10, 12);
			var planningPeriod = PlanningPeriodRepository.Has(dateOnly.AddDays(-6), 1);
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1);
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
			var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			var agent = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, skill);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), shiftCategory));
			agent.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);

			SkillDayRepository.Has(new List<ISkillDay>
				{
					skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)))
				});

			SkillDayRepository.Has(new List<ISkillDay>
				{
					skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly.AddDays(1), TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)))
				});

			var dateTime = TimeZoneHelper.ConvertToUtc(dateOnly.Date, agent.PermissionInformation.DefaultTimeZone());
			var assignment = new PersonAssignment(agent, scenario, dateOnly);
			assignment.SetShiftCategory(shiftCategory);
			assignment.AddActivity(phoneActivity, new DateTimePeriod(dateTime.AddHours(8), dateTime.AddHours(17)));
			PersonAssignmentRepository.Add(assignment);

			var assignmentDayAfter = new PersonAssignment(agent, scenario, dateOnly.AddDays(1));
			assignmentDayAfter.SetShiftCategory(shiftCategory);
			assignmentDayAfter.AddActivity(phoneActivity, new DateTimePeriod(dateTime.AddDays(1).AddHours(8), dateTime.AddDays(1).AddHours(17)));
			PersonAssignmentRepository.Add(assignmentDayAfter);

			Target.Optimize(planningPeriod.Id.Value);

			var skillDays = SkillDayRepository.FindReadOnlyRange(new DateOnlyPeriod(dateOnly.AddDays(1), dateOnly.AddDays(1)), new List<ISkill> {skill}, scenario);
			var skillStaffPeriods = skillDays.First().SkillStaffPeriodCollection;
			foreach (var skillStaffPeriod in skillStaffPeriods)
			{
				skillStaffPeriod.CalculatedResource.Should().Be.EqualTo(0d);
			}
		}

		[Test]
		public void ShouldCalculateDayAfterIfTodayIsANightShift()
		{
			var phoneActivity = ActivityFactory.CreateActivity("phone");
			phoneActivity.RequiresSkill = true;
			var skill = SkillRepository.Has("skill", phoneActivity);
			var dateOnly = new DateOnly(2015, 10, 12);
			var planningPeriod = PlanningPeriodRepository.Has(dateOnly.AddDays(-6), 1);
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1);
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
			var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			var agent = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, skill);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), shiftCategory));
			agent.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);

			SkillDayRepository.Has(new List<ISkillDay>
				{
					skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)))
				});

			SkillDayRepository.Has(new List<ISkillDay>
				{
					skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly.AddDays(1), TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)))
				});

			var dateTime = TimeZoneHelper.ConvertToUtc(dateOnly.Date, agent.PermissionInformation.DefaultTimeZone());
			var assignment = new PersonAssignment(agent, scenario, dateOnly);
			assignment.SetShiftCategory(shiftCategory);
			assignment.AddActivity(phoneActivity, new DateTimePeriod(dateTime.AddHours(17), dateTime.AddHours(26)));
			PersonAssignmentRepository.Add(assignment);

			var assignmentDayAfter = new PersonAssignment(agent, scenario, dateOnly.AddDays(1));
			assignmentDayAfter.SetShiftCategory(shiftCategory);
			assignmentDayAfter.AddActivity(phoneActivity, new DateTimePeriod(dateTime.AddDays(1).AddHours(8), dateTime.AddDays(1).AddHours(17)));
			PersonAssignmentRepository.Add(assignmentDayAfter);

			Target.Optimize(planningPeriod.Id.Value);

			var skillDays = SkillDayRepository.FindReadOnlyRange(new DateOnlyPeriod(dateOnly.AddDays(1), dateOnly.AddDays(1)), new List<ISkill> { skill }, scenario);
			var skillStaffPeriods = skillDays.First().SkillStaffPeriodCollection;
			var hasResource = false;
			foreach (var skillStaffPeriod in skillStaffPeriods)
			{
				if (skillStaffPeriod.CalculatedResource > 0d)
				{
					hasResource = true;
					break;
				}
			}

			hasResource.Should().Be.True();
		}
	}
}
