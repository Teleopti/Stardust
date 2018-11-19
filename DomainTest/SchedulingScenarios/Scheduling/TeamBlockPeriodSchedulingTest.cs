using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
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
	public class TeamBlockPeriodSchedulingTest : SchedulingScenario
	{
		public FullScheduling Target;
		public FakePersonRepository PersonRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillRepository SkillRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAssignmentRepository AssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeDayOffTemplateRepository DayOffTemplateRepository;
		public SchedulingOptionsProvider SchedulingOptionsProvider;
		public FakeRuleSetBagRepository RuleSetBagRepository;
		public FakePlanningPeriodRepository PlanningPeriodRepository;

		[TestCase(true)]
		[TestCase(false)]
		public void ShouldHandleMixOfTeamAndBlock(bool reversedAgentOrder)
		{
			var firstDay = new DateOnly(2016, 05, 30);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			var ruleSetBag = new RuleSetBag(new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory("_").WithId()))) { Description = new Description("_") }.WithId();
			RuleSetBagRepository.Add(ruleSetBag);
			var agent1 = PersonRepository.Has(new ContractWithMaximumTolerance(), new ContractSchedule("_"), new PartTimePercentage("_"), new Team(), new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSetBag, skill);
			var agent2 = PersonRepository.Has(new ContractWithMaximumTolerance(), new ContractSchedule("_"), new PartTimePercentage("_"), new Team(), new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSetBag, skill);
			if (reversedAgentOrder)
				PersonRepository.ReversedOrder();
			DayOffTemplateRepository.Add(new DayOffTemplate());
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 2, 2, 2, 2, 2, 2, 2));
			AssignmentRepository.Has(agent1, scenario, new DayOffTemplate(), firstDay.AddDays(2));
			AssignmentRepository.Has(agent2, scenario, new DayOffTemplate(), firstDay.AddDays(2));
			AssignmentRepository.Has(agent1, scenario, new DayOffTemplate(), firstDay.AddDays(4));
			AssignmentRepository.Has(agent2, scenario, new DayOffTemplate(), firstDay.AddDays(5));
			var planningPeriod = PlanningPeriodRepository.Has(firstDay.AddDays(3),SchedulePeriodType.Day, 2);
			SchedulingOptionsProvider.SetFromTest(planningPeriod, new SchedulingOptions
			{
				UseTeam = true,
				GroupOnGroupPageForTeamBlockPer = new GroupPageLight("_", GroupPageType.RuleSetBag),
				UseBlock = true,
				BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff,
				TeamSameShiftCategory = true,
				BlockSameShiftCategory = true
			});
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			AssignmentRepository.LoadAll().Count(x => x.MainActivities().Any())
				.Should().Be.EqualTo(3); //schedule all selected days except one that has DO
		}

		[TestCase(true)]
		[TestCase(false)]
		public void ShouldHandleMixOfTeamAndBlockAndHaveToClearTeamBlock_BetweenDayOffs(bool reversedAgentOrder)
		{
			var firstDay = new DateOnly(2016, 05, 30);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			var shiftCategory = new ShiftCategory("_").WithId();
			var otherShiftCategory = new ShiftCategory("other").WithId();
			var ruleSetBag = new RuleSetBag(new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory))) { Description = new Description("_") }.WithId();
			RuleSetBagRepository.Add(ruleSetBag);
			var agent1 = PersonRepository.Has(new ContractWithMaximumTolerance(), new ContractSchedule("_"), new PartTimePercentage("_"), new Team(), new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSetBag, skill);
			var agent2 = PersonRepository.Has(new ContractWithMaximumTolerance(), new ContractSchedule("_"), new PartTimePercentage("_"), new Team(), new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSetBag, skill);
			if (reversedAgentOrder)
				PersonRepository.ReversedOrder();
			DayOffTemplateRepository.Add(new DayOffTemplate());
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 2, 2, 2, 2, 2, 2, 2));
			AssignmentRepository.Has(agent1, scenario, new DayOffTemplate(), firstDay.AddDays(2));
			AssignmentRepository.Has(agent2, scenario, new DayOffTemplate(), firstDay.AddDays(2));
			AssignmentRepository.Has(agent1, scenario, new DayOffTemplate(), firstDay.AddDays(4));
			AssignmentRepository.Has(agent2, scenario, new DayOffTemplate(), firstDay.AddDays(5));
			AssignmentRepository.Has(agent2, scenario, activity, otherShiftCategory, firstDay.AddDays(4), new TimePeriod(8, 16));
			var planningPeriod = PlanningPeriodRepository.Has(firstDay.AddDays(3),SchedulePeriodType.Day, 2);
			SchedulingOptionsProvider.SetFromTest(planningPeriod, new SchedulingOptions
			{
				UseTeam = true,
				GroupOnGroupPageForTeamBlockPer = new GroupPageLight("_", GroupPageType.RuleSetBag),
				UseBlock = true,
				BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff,
				TeamSameShiftCategory = true,
				BlockSameShiftCategory = true
			});
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			AssignmentRepository.LoadAll().Count(x => shiftCategory.Equals(x.ShiftCategory))
				.Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldHandleMixOfTeamAndBlockAndHaveToClearTeamBlock_BetweenDayOffsCase2()
		{
			var firstDay = new DateOnly(2016, 05, 30);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			var shiftCategory = new ShiftCategory("_").WithId();
			var otherShiftCategory = new ShiftCategory("other").WithId();
			var ruleSetBag = new RuleSetBag(new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory))) { Description = new Description("_") }.WithId();
			RuleSetBagRepository.Add(ruleSetBag);
			var agent1 = PersonRepository.Has(new ContractWithMaximumTolerance(), new ContractSchedule("_"), new PartTimePercentage("_"), new Team(), new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSetBag, skill);
			var agent2 = PersonRepository.Has(new ContractWithMaximumTolerance(), new ContractSchedule("_"), new PartTimePercentage("_"), new Team(), new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSetBag, skill);
			DayOffTemplateRepository.Add(new DayOffTemplate());
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 2, 2, 2, 2, 2, 2, 2));
			AssignmentRepository.Has(agent1, scenario, new DayOffTemplate(), firstDay.AddDays(2));
			AssignmentRepository.Has(agent2, scenario, new DayOffTemplate(), firstDay);
			AssignmentRepository.Has(agent1, scenario, new DayOffTemplate(), firstDay.AddDays(4));
			AssignmentRepository.Has(agent2, scenario, new DayOffTemplate(), firstDay.AddDays(4));
			AssignmentRepository.Has(agent1, scenario, activity, otherShiftCategory, firstDay, new TimePeriod(8, 16));
			AssignmentRepository.Has(agent1, scenario, activity, otherShiftCategory, firstDay.AddDays(1), new TimePeriod(8, 16));
			AssignmentRepository.Has(agent2, scenario, activity, otherShiftCategory, firstDay.AddDays(1), new TimePeriod(8, 16));
			AssignmentRepository.Has(agent2, scenario, activity, otherShiftCategory, firstDay.AddDays(2), new TimePeriod(8, 16));
			var planningPeriod = PlanningPeriodRepository.Has(firstDay,SchedulePeriodType.Day, 5);
			SchedulingOptionsProvider.SetFromTest(planningPeriod, new SchedulingOptions
			{
				UseTeam = true,
				GroupOnGroupPageForTeamBlockPer = new GroupPageLight("_", GroupPageType.RuleSetBag),
				UseBlock = true,
				BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff,
				TeamSameShiftCategory = true,
				BlockSameShiftCategory = true
			});
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			AssignmentRepository.LoadAll().Count(x => shiftCategory.Equals(x.ShiftCategory))
				.Should().Be.EqualTo(6);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void ShouldHandleMixOfTeamAndBlockAndHaveToClearTeamBlock_SchedulePeriod(bool reversedAgentOrder)
		{
			var firstDay = new DateOnly(2016, 05, 30);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			var shiftCategory = new ShiftCategory("_").WithId();
			var otherShiftCategory = new ShiftCategory("other").WithId();
			var ruleSetBag = new RuleSetBag(new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory))) { Description = new Description("_") }.WithId();
			RuleSetBagRepository.Add(ruleSetBag);
			var agent1 = PersonRepository.Has(new ContractWithMaximumTolerance(), new ContractSchedule("_"), new PartTimePercentage("_"), new Team(), new SchedulePeriod(firstDay.AddWeeks(-1).AddDays(3), SchedulePeriodType.Week, 1), ruleSetBag, skill);
			var agent2 = PersonRepository.Has(new ContractWithMaximumTolerance(), new ContractSchedule("_"), new PartTimePercentage("_"), new Team(), new SchedulePeriod(firstDay.AddWeeks(-1).AddDays(4), SchedulePeriodType.Week, 1), ruleSetBag, skill);
			if (reversedAgentOrder)
				PersonRepository.ReversedOrder();
			DayOffTemplateRepository.Add(new DayOffTemplate());
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 2, 2, 2, 2, 2, 2, 2));
			AssignmentRepository.Has(agent1, scenario, new DayOffTemplate(), firstDay.AddDays(2));
			AssignmentRepository.Has(agent2, scenario, new DayOffTemplate(), firstDay.AddDays(2));
			AssignmentRepository.Has(agent1, scenario, new DayOffTemplate(), firstDay.AddDays(4));
			AssignmentRepository.Has(agent2, scenario, new DayOffTemplate(), firstDay.AddDays(5));
			AssignmentRepository.Has(agent2, scenario, activity, otherShiftCategory, firstDay.AddDays(4), new TimePeriod(8, 16));
			var planningPeriod = PlanningPeriodRepository.Has(firstDay.AddDays(3),SchedulePeriodType.Day, 2);
			SchedulingOptionsProvider.SetFromTest(planningPeriod, new SchedulingOptions
			{
				UseTeam = true,
				GroupOnGroupPageForTeamBlockPer = new GroupPageLight("_", GroupPageType.RuleSetBag),
				UseBlock = true,
				BlockFinderTypeForAdvanceScheduling = BlockFinderType.SchedulePeriod,
				TeamSameShiftCategory = true,
				BlockSameShiftCategory = true
			});
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			AssignmentRepository.LoadAll().Count(x => shiftCategory.Equals(x.ShiftCategory))
				.Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldNotCrashWhenTeamMembersHaveDifferentSchedulePeriods()
		{
			var firstDay = new DateOnly(2016, 05, 30);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			var ruleSetBag = new RuleSetBag(new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory("_").WithId()))) { Description = new Description("_") }.WithId();
			RuleSetBagRepository.Add(ruleSetBag);
			PersonRepository.Has(new ContractWithMaximumTolerance(), new ContractSchedule("_"), new PartTimePercentage("_"), new Team(), new SchedulePeriod(firstDay, SchedulePeriodType.Week, 2), ruleSetBag, skill);
			PersonRepository.Has(new ContractWithMaximumTolerance(), new ContractSchedule("_"), new PartTimePercentage("_"), new Team(), new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSetBag, skill);
			DayOffTemplateRepository.Add(new DayOffTemplate());
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 2, 2, 2, 2, 2, 2, 2));
			var planningPeriod = PlanningPeriodRepository.Has(firstDay,SchedulePeriodType.Day, 1);
			SchedulingOptionsProvider.SetFromTest(planningPeriod, new SchedulingOptions
			{
				UseTeam = true,
				GroupOnGroupPageForTeamBlockPer = new GroupPageLight("_", GroupPageType.RuleSetBag),
				UseBlock = true,
				BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff,
				TeamSameShiftCategory = true,
				BlockSameShiftCategory = true
			});

			Assert.DoesNotThrow(() =>
			{
				Target.DoSchedulingAndDO(planningPeriod.Id.Value);
			});
		}

		public TeamBlockPeriodSchedulingTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}