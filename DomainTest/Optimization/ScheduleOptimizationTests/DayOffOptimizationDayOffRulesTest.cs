using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.Filters;
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
	public class DayOffOptimizationDayOffRulesTest
	{
		public ScheduleOptimization Target;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeSkillRepository SkillRepository;
		public FakePersonRepository PersonRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeActivityRepository ActivityRepository;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public FakeDayOffRulesRepository DayOffRulesRepository;

		[Test]
		public void ShouldUseSettingForConsecutiveDayOffs()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var planningPeriod = PlanningPeriodRepository.Has(firstDay, 1);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			DayOffRulesRepository.HasDefault(x => 
			{
				x.DayOffsPerWeek = new MinMax<int>(1, 4);
				x.ConsecutiveDayOffs = new MinMax<int>(1, 4);
			});
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1);
			schedulePeriod.SetDaysOff(4);
			var agent = PersonRepository.Has(new Contract("_"), new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, skill);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			agent.Period(firstDay).RuleSetBag = new RuleSetBag(ruleSet);

			var skillDays = SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				TimeSpan.FromHours(1), //DO
				TimeSpan.FromHours(1), //DO
				TimeSpan.FromHours(5), //DO
				TimeSpan.FromHours(10),
				TimeSpan.FromHours(20),
				TimeSpan.FromHours(20),
				TimeSpan.FromHours(25)) //DO
				);
			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory, new DateOnlyPeriod(firstDay.AddDays(-1), firstDay.AddDays(8)), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.GetSingle(skillDays[0].CurrentDate).SetDayOff(new DayOffTemplate());
			PersonAssignmentRepository.GetSingle(skillDays[1].CurrentDate).SetDayOff(new DayOffTemplate());
			PersonAssignmentRepository.GetSingle(skillDays[2].CurrentDate).SetDayOff(new DayOffTemplate());
			PersonAssignmentRepository.GetSingle(skillDays[6].CurrentDate).SetDayOff(new DayOffTemplate());

			Target.Execute(planningPeriod.Id.Value);

			PersonAssignmentRepository.GetSingle(skillDays[0].CurrentDate).DayOff().Should().Not.Be.Null();
			PersonAssignmentRepository.GetSingle(skillDays[1].CurrentDate).DayOff().Should().Not.Be.Null();
			PersonAssignmentRepository.GetSingle(skillDays[2].CurrentDate).DayOff().Should().Not.Be.Null();
			PersonAssignmentRepository.GetSingle(skillDays[3].CurrentDate).DayOff().Should().Not.Be.Null();
		}

		[Test]
		public void ShouldUseSettingForConsecutiveWorkDays()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var planningPeriod = PlanningPeriodRepository.Has(firstDay, 1);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			DayOffRulesRepository.HasDefault(x => x.ConsecutiveWorkdays = new MinMax<int>(1, 6));
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1);
			schedulePeriod.SetDaysOff(1);
			var agent = PersonRepository.Has(new Contract("_"), new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, skill);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			agent.Period(firstDay).RuleSetBag = new RuleSetBag(ruleSet);

			var skillDays = SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				TimeSpan.FromHours(5),
				TimeSpan.FromHours(1),
				TimeSpan.FromHours(5),
				TimeSpan.FromHours(25),
				TimeSpan.FromHours(5),
				TimeSpan.FromHours(5),
				TimeSpan.FromHours(5))
				);
			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory, new DateOnlyPeriod(firstDay.AddDays(-1), firstDay.AddDays(8)), new TimePeriod(8, 0, 16, 0));

			PersonAssignmentRepository.GetSingle(skillDays[3].CurrentDate).SetDayOff(new DayOffTemplate());
			PersonAssignmentRepository.GetSingle(skillDays[0].CurrentDate.AddDays(-1)) //sunday in week before
				.SetDayOff(new DayOffTemplate());

			Target.Execute(planningPeriod.Id.Value);
			
			PersonAssignmentRepository.GetSingle(skillDays[1].CurrentDate) //tuesday
				.DayOff().Should().Not.Be.Null();	
		}

		[Test]
		public void ShouldUseSettingForDayOffPerWeek_NotValid()
		{
			var firstDay = new DateOnly(2015, 10, 26); //mon
			var planningPeriod = PlanningPeriodRepository.Has(firstDay, 2);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			DayOffRulesRepository.HasDefault(x => x.DayOffsPerWeek = new MinMax<int>(3, 4));
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 2);
			schedulePeriod.SetDaysOff(2);
			var agent = PersonRepository.Has(new Contract("_"), new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, skill);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			agent.Period(firstDay).RuleSetBag = new RuleSetBag(ruleSet);

			var skillDays = SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				TimeSpan.FromHours(20),
				TimeSpan.FromHours(20),
				TimeSpan.FromHours(3),
				TimeSpan.FromHours(20),
				TimeSpan.FromHours(20),
				TimeSpan.FromHours(20),
				TimeSpan.FromHours(20),
				TimeSpan.FromHours(20),
				TimeSpan.FromHours(20),
				TimeSpan.FromHours(1), 
				TimeSpan.FromHours(2),
				TimeSpan.FromHours(20),
				TimeSpan.FromHours(20),
				TimeSpan.FromHours(20)
				));
			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory, new DateOnlyPeriod(firstDay, firstDay.AddDays(14)), new TimePeriod(8, 0, 16, 0));

			PersonAssignmentRepository.GetSingle(skillDays[0].CurrentDate).SetDayOff(new DayOffTemplate());
			PersonAssignmentRepository.GetSingle(skillDays[1].CurrentDate).SetDayOff(new DayOffTemplate());

			Target.Execute(planningPeriod.Id.Value);

			PersonAssignmentRepository.GetSingle(skillDays[0].CurrentDate).DayOff().Should().Not.Be.Null();
			PersonAssignmentRepository.GetSingle(skillDays[1].CurrentDate).DayOff().Should().Not.Be.Null();
		}

		[Test]
		public void ShouldUseSettingForDayOffPerWeek_Valid()
		{
			var firstDay = new DateOnly(2015, 10, 26); //mon
			var planningPeriod = PlanningPeriodRepository.Has(firstDay, 2);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			DayOffRulesRepository.HasDefault(x =>
			{
				x.DayOffsPerWeek = new MinMax<int>(1, 3);
				x.ConsecutiveWorkdays = new MinMax<int>(2, 20);
			});
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 2);
			schedulePeriod.SetDaysOff(2);
			var agent = PersonRepository.Has(new Contract("_"), new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, skill);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			agent.Period(firstDay).RuleSetBag = new RuleSetBag(ruleSet);

			var skillDays = SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				TimeSpan.FromHours(20),
				TimeSpan.FromHours(20),
				TimeSpan.FromHours(3), //expected
				TimeSpan.FromHours(20),
				TimeSpan.FromHours(20),
				TimeSpan.FromHours(20),
				TimeSpan.FromHours(20),
				TimeSpan.FromHours(20),
				TimeSpan.FromHours(20),
				TimeSpan.FromHours(1), //expected
				TimeSpan.FromHours(2),
				TimeSpan.FromHours(20),
				TimeSpan.FromHours(20),
				TimeSpan.FromHours(20)
        ));
			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory, new DateOnlyPeriod(firstDay, firstDay.AddDays(14)), new TimePeriod(8, 0, 16, 0));

			PersonAssignmentRepository.GetSingle(skillDays[0].CurrentDate).SetDayOff(new DayOffTemplate());
			PersonAssignmentRepository.GetSingle(skillDays[1].CurrentDate).SetDayOff(new DayOffTemplate());

			Target.Execute(planningPeriod.Id.Value);

			PersonAssignmentRepository.GetSingle(skillDays[2].CurrentDate).DayOff().Should().Not.Be.Null();
			PersonAssignmentRepository.GetSingle(skillDays[9].CurrentDate).DayOff().Should().Not.Be.Null();
		}

		[Test]
		public void ShouldUseDifferentDayOffRulesForDifferentFiltersWhenOneIsIncorrect()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var planningPeriod = PlanningPeriodRepository.Has(firstDay, 1);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");

			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1);
			schedulePeriod.SetDaysOff(1);
			var contractInFilter = new Contract("_");
			var agentWithValidDefaultFilter = PersonRepository.Has(new Contract("_"), new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, skill);
			var agentWithExplicitFilter = PersonRepository.Has(contractInFilter, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, skill);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			agentWithValidDefaultFilter.Period(firstDay).RuleSetBag = new RuleSetBag(ruleSet);
			agentWithExplicitFilter.Period(firstDay).RuleSetBag = new RuleSetBag(ruleSet);

			var dayOffRules = new DayOffRules { DayOffsPerWeek = new MinMax<int>(5,6)};
			dayOffRules.AddFilter(new ContractFilter(contractInFilter));
			DayOffRulesRepository.Add(dayOffRules);

			var skillDays = SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				TimeSpan.FromHours(20), 
				TimeSpan.FromHours(1), //expected
				TimeSpan.FromHours(5), 
				TimeSpan.FromHours(10),
				TimeSpan.FromHours(20),
				TimeSpan.FromHours(20),
				TimeSpan.FromHours(25)) 
				);
			PersonAssignmentRepository.Has(agentWithValidDefaultFilter, scenario, activity, shiftCategory, new DateOnlyPeriod(firstDay.AddDays(-1), firstDay.AddDays(8)), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.Has(agentWithExplicitFilter, scenario, activity, shiftCategory, new DateOnlyPeriod(firstDay.AddDays(-1), firstDay.AddDays(8)), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.GetSingle(skillDays[0].CurrentDate, agentWithValidDefaultFilter).SetDayOff(new DayOffTemplate());
			PersonAssignmentRepository.GetSingle(skillDays[6].CurrentDate, agentWithExplicitFilter).SetDayOff(new DayOffTemplate());

			Target.Execute(planningPeriod.Id.Value);

			PersonAssignmentRepository.GetSingle(skillDays[0].CurrentDate, agentWithValidDefaultFilter).DayOff().Should().Be.Null();
			PersonAssignmentRepository.GetSingle(skillDays[6].CurrentDate, agentWithExplicitFilter).DayOff().Should().Not.Be.Null();
		}
	}
}