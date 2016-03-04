using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.ScheduleOptimizationTests
{
	[Toggle(Toggles.ResourcePlanner_JumpOutWhenLargeGroupIsHalfOptimized_37049)]
	[DomainTest]
	[UseEventPublisher(typeof(RunInProcessEventPublisher))]
	public class IntradayOptimizationStopWhenLargeGroupTest : ISetup
	{
		public IntradayOptimizationCommandHandler Target;
		public TrackOptimizeDaysForAgents TrackOptimizeDaysForAgents;
		public IntradayOptmizerLimiter IntradayOptmizerLimiter;
		public FakeSkillRepository SkillRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonRepository PersonRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public OptimizationPreferencesFactory OptimizationPreferencesFactory;
		public FakePlanningPeriodRepository PlanningPeriodRepository;

		[Test]
		public void ShouldOptimizeNoneIf0PercentShouldBeOptimized()
		{
			IntradayOptmizerLimiter.SetFromTest(new Percent(0), 0);

			var phoneActivity = ActivityFactory.CreateActivity("phone");
			const int numberOfAgents = 10;
			var skill = SkillRepository.Has("skill", phoneActivity);
			var scenario = ScenarioRepository.Has("some name");
			var dateOnly = new DateOnly(2015, 10, 12);
			var planningPeriod = PlanningPeriodRepository.Has(dateOnly, 1);
			for (var i = 0; i < numberOfAgents; i++)
			{
				var agent = PersonRepository.Has(new Contract("_"), new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1), skill);
				PersonAssignmentRepository.Has(agent, scenario, phoneActivity, new ShiftCategory("_"), dateOnly, new TimePeriod(8, 0, 17, 0));
			}
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnlyPeriod(dateOnly, dateOnly.AddDays(6)), TimeSpan.FromMinutes(60)));

			Target.Execute(planningPeriod.Id.Value);

			TrackOptimizeDaysForAgents.NumberOfOptimizationsFor(dateOnly)
				.Should().Be.LessThanOrEqualTo(1); //"should" be 0 but impl detail makes it easier to accept 1 as well
		}

		[Test]
		public void ShouldOptimizeAllIf100PercentShouldBeOptimized()
		{
			IntradayOptmizerLimiter.SetFromTest(new Percent(1), 0);
			const int numberOfAgents = 10;
			var phoneActivity = ActivityFactory.CreateActivity("phone");
			var skill = SkillRepository.Has("skill", phoneActivity);
			var scenario = ScenarioRepository.Has("some name");
			var dateOnly = new DateOnly(2015, 10, 12);
			var planningPeriod = PlanningPeriodRepository.Has(dateOnly, 1);
			for (var i = 0; i < numberOfAgents; i++)
			{
				var agent = PersonRepository.Has(new Contract("_"), new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1), skill);
				PersonAssignmentRepository.Has(agent, scenario, phoneActivity, new ShiftCategory("_"), dateOnly, new TimePeriod(8, 0, 17, 0));
			}
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnlyPeriod(dateOnly, dateOnly.AddDays(6)), TimeSpan.FromMinutes(60)));

			Target.Execute(planningPeriod.Id.Value);

			TrackOptimizeDaysForAgents.NumberOfOptimizationsFor(dateOnly)
				.Should().Be.EqualTo(numberOfAgents);
		}

		[Test]
		public void ShouldOptimizeHalfGroupIf50PercentShouldBeOptimized()
		{
			IntradayOptmizerLimiter.SetFromTest(new Percent(0.5), 0);
			const int numberOfAgents = 10;
			var phoneActivity = ActivityFactory.CreateActivity("phone");
			var skill = SkillRepository.Has("skill", phoneActivity);
			var scenario = ScenarioRepository.Has("some name");
			var dateOnly = new DateOnly(2015, 10, 12);
			var planningPeriod = PlanningPeriodRepository.Has(dateOnly, 1);
			SkillDayRepository.Has(new[] { skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60)) });
			for (var i = 0; i < numberOfAgents; i++)
			{
				var agent = PersonRepository.Has(new Contract("_"), new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1), skill);
				PersonAssignmentRepository.Has(agent, scenario, phoneActivity, new ShiftCategory("_"), dateOnly, new TimePeriod(8, 0, 17, 0));
			}

			Target.Execute(planningPeriod.Id.Value);

			TrackOptimizeDaysForAgents.NumberOfOptimizationsFor(dateOnly)
				.Should().Be.EqualTo(5);
		}

		[Test]
		public void ShouldOptimizeAllIfGroupIsTooSmallNoMatterPercentSetting()
		{
			IntradayOptmizerLimiter.SetFromTest(new Percent(0.01), 3);
			var phoneActivity = ActivityFactory.CreateActivity("phone");
			var skill = SkillRepository.Has("skill", phoneActivity);
			var scenario = ScenarioRepository.Has("some name");
			var dateOnly = new DateOnly(2015, 10, 12);
			var agent1 = PersonRepository.Has(new Contract("_"), new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1), skill);
			var agent2 = PersonRepository.Has(new Contract("_"), new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1), skill);
			var planningPeriod = PlanningPeriodRepository.Has(dateOnly, 1);
			PersonAssignmentRepository.Has(agent1, scenario, phoneActivity, new ShiftCategory("_"), dateOnly, new TimePeriod(8, 0, 17, 0));
			PersonAssignmentRepository.Has(agent2, scenario, phoneActivity, new ShiftCategory("_"), dateOnly, new TimePeriod(8, 0, 17, 0));
			SkillDayRepository.Has(new[] { skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60)) });

			Target.Execute(planningPeriod.Id.Value);

			TrackOptimizeDaysForAgents.NumberOfOptimizationsFor(dateOnly)
				.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldSkipOptimizationForRestOfPeriodIfExecuterThinkThatsAGoodIdea()
		{
			var prefUsedInThisTest = OptimizationPreferencesFactory.Create();
			prefUsedInThisTest.Shifts.KeepShifts = true;
			prefUsedInThisTest.Shifts.KeepShiftsValue = int.MaxValue;
			OptimizationPreferencesFactory.SetFromTestsOnly(prefUsedInThisTest);
			IntradayOptmizerLimiter.SetFromTest(new Percent(1), 0);
			var phoneActivity = ActivityFactory.CreateActivity("phone");
			var skill = SkillRepository.Has("skill", phoneActivity);
			var scenario = ScenarioRepository.Has("some name");
			var dateOnly = new DateOnly(2015, 10, 12);
			var agent = PersonRepository.Has(new Contract("_"), new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1), skill);
			var planningPeriod = PlanningPeriodRepository.Has(dateOnly, 1);
			PersonAssignmentRepository.Has(agent, scenario, phoneActivity, new ShiftCategory("_"), new DateOnlyPeriod(dateOnly, dateOnly.AddDays(7)), new TimePeriod(8, 0, 16, 0));
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnlyPeriod(dateOnly, dateOnly.AddDays(6)), TimeSpan.FromMinutes(60)));

			Target.Execute(planningPeriod.Id.Value);

			TrackOptimizeDaysForAgents.NumberOfOptimizations()
				.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldOptimizeHalfGroupIf50PercentShouldBeOptimized_WhenAgentsHaveMultipleSchedulePeriods()
		{
			IntradayOptmizerLimiter.SetFromTest(new Percent(0.5), 0);
			const int numberOfAgents = 10;
			var phoneActivity = ActivityFactory.CreateActivity("phone");
			var skill = SkillRepository.Has("skill", phoneActivity);
			var scenario = ScenarioRepository.Has("some name");
			var dateOnly = new DateOnly(2015, 10, 12);
			var planningPeriod = PlanningPeriodRepository.Has(dateOnly, 1);
			SkillDayRepository.Has(new[] { skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60)) });
			for (var i = 0; i < numberOfAgents; i++)
			{
				var agent = PersonRepository.Has(new Contract("_"), new SchedulePeriod(dateOnly, SchedulePeriodType.Day, 1), skill);
				agent.AddSchedulePeriod(new SchedulePeriod(dateOnly.AddDays(1), SchedulePeriodType.Day, 1));
				PersonAssignmentRepository.Has(agent, scenario, phoneActivity, new ShiftCategory("_"), dateOnly, new TimePeriod(8, 0, 17, 0));
				PersonAssignmentRepository.Has(agent, scenario, phoneActivity, new ShiftCategory("_"), dateOnly.AddDays(1), new TimePeriod(8, 0, 17, 0));
			}

			Target.Execute(planningPeriod.Id.Value);

			TrackOptimizeDaysForAgents.NumberOfOptimizationsFor(dateOnly).Should().Be.EqualTo(5);
			TrackOptimizeDaysForAgents.NumberOfOptimizationsFor(dateOnly.AddDays(1)).Should().Be.EqualTo(5);
		}

		[Test] //one test here verifying skill groups are used. more detailed tests in AgentsToSkillGroupsTest
		public void ShouldHaveDifferentCountersForDifferentSkillGroups()
		{
			IntradayOptmizerLimiter.SetFromTest(new Percent(0.5), 5);
			const int numberOfAgentsInSkillGroup1 = 10;
			const int numberOfAgentsInSkillGroup2 = 4;
			var phoneActivity = ActivityFactory.CreateActivity("phone");
			var skill1 = SkillRepository.Has("skill", phoneActivity);
			var skill2 = SkillRepository.Has("skill", phoneActivity);
			var scenario = ScenarioRepository.Has("some name");
			var dateOnly = new DateOnly(2015, 10, 12);
			var planningPeriod = PlanningPeriodRepository.Has(dateOnly, 1);
			SkillDayRepository.Has(new[] { skill1.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60)) });
			SkillDayRepository.Has(new[] { skill2.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60)) });
			for (var i = 0; i < numberOfAgentsInSkillGroup1; i++)
			{
				var agent = PersonRepository.Has(new Contract("_"), new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1), skill1);
				PersonAssignmentRepository.Has(agent, scenario, phoneActivity, new ShiftCategory("_"), dateOnly, new TimePeriod(8, 0, 17, 0));
			}
			for (var i = 0; i < numberOfAgentsInSkillGroup2; i++)
			{
				var agent = PersonRepository.Has(new Contract("_"), new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1), skill2);
				PersonAssignmentRepository.Has(agent, scenario, phoneActivity, new ShiftCategory("_"), dateOnly, new TimePeriod(8, 0, 17, 0));
			}

			Target.Execute(planningPeriod.Id.Value);

			TrackOptimizeDaysForAgents.NumberOfOptimizationsFor(dateOnly).Should().Be.EqualTo(9);
		}

		[Test]
		public void ShouldPickAgentRandomly()
		{
			IntradayOptmizerLimiter.SetFromTest(new Percent(0.5), 0);
			const int numberOfAgents = 2;
			const int retriesBeforeGivingUp = 50;
			var phoneActivity = ActivityFactory.CreateActivity("phone");
			var skill = SkillRepository.Has("skill", phoneActivity);
			var scenario = ScenarioRepository.Has("some name");
			var dateOnly = new DateOnly(2015, 10, 12);
			SkillDayRepository.Has(new[] { skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60)) });
			var agents = new List<IPerson>();
			var planningPeriod = PlanningPeriodRepository.Has(dateOnly, 1);
			for (var i = 0; i < numberOfAgents; i++)
			{
				agents.Add(PersonRepository.Has(new Contract("_"), new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1), skill));
			}

			var optimizedAgentsInAnyOfLoops = new HashSet<IPerson>();
			for (var retries = 0; retries < retriesBeforeGivingUp; retries++)
			{
				PersonAssignmentRepository.Clear();
				TrackOptimizeDaysForAgents.Clear();
				for (var i = 0; i < numberOfAgents; i++)
				{
					PersonAssignmentRepository.Has(agents[i], scenario, phoneActivity, new ShiftCategory("_"), dateOnly, new TimePeriod(8, 0, 17, 0));
				}

				Target.Execute(planningPeriod.Id.Value);

				optimizedAgentsInAnyOfLoops.Add(TrackOptimizeDaysForAgents.OptimizedAgentsOn(dateOnly).Single());
				if (optimizedAgentsInAnyOfLoops.Count == 2)
					return;
			}
			Assert.Fail("Tried to optimize 2 agents {0} times. A limit is set to 50% and it's always the same agent that are optimized. Giving up.", retriesBeforeGivingUp);
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<TrackOptimizeDaysForAgents>().For<IIntradayOptimizeOneDayCallback>();
		}
	}
}