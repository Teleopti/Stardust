using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
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
	public class IntradayOptimizationStopWhenLargeGroupTest : ISetup
	{
		public IntradayOptimization Target;
		public TrackOptimizeDaysForAgents TrackOptimizeDaysForAgents;
		public IntradayOptmizerLimiter IntradayOptmizerLimiter;
		public FakeSkillRepository SkillRepository;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonRepository PersonRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;

		[Test]
		public void ShouldOptimizeNoneIf0PercentShouldBeOptimized()
		{
			IntradayOptmizerLimiter.SetFromTest(new Percent(0), 0);

			var phoneActivity = ActivityFactory.CreateActivity("phone");
			var skill = SkillRepository.Has("skill", phoneActivity);
			var scenario = ScenarioRepository.Has("some name");
			var dateOnly = new DateOnly(2015, 10, 12);
			var planningPeriod = PlanningPeriodRepository.Has(dateOnly, 1);
			var agent = PersonRepository.Has(new Contract("_"), new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1), skill);
			PersonAssignmentRepository.Has(agent, scenario, phoneActivity, new ShiftCategory("_"), new DateOnlyPeriod(dateOnly, dateOnly.AddDays(7)), new TimePeriod(8, 0, 16, 0));
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnlyPeriod(dateOnly, dateOnly.AddDays(6)), TimeSpan.FromMinutes(60)));

			Target.Optimize(planningPeriod.Id.Value);

			TrackOptimizeDaysForAgents.NumberOfOptimizations()
				.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldOptimizeAllIf100PercentShouldBeOptimized()
		{
			IntradayOptmizerLimiter.SetFromTest(new Percent(1), 0);
			var phoneActivity = ActivityFactory.CreateActivity("phone");
			var skill = SkillRepository.Has("skill", phoneActivity);
			var scenario = ScenarioRepository.Has("some name");
			var dateOnly = new DateOnly(2015, 10, 12);
			var planningPeriod = PlanningPeriodRepository.Has(dateOnly, 1);
			var agent = PersonRepository.Has(new Contract("_"), new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1), skill);
			PersonAssignmentRepository.Has(agent, scenario, phoneActivity, new ShiftCategory("_"), new DateOnlyPeriod(dateOnly, dateOnly.AddDays(7)), new TimePeriod(8, 0, 16, 0));
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnlyPeriod(dateOnly, dateOnly.AddDays(6)), TimeSpan.FromMinutes(60)));

			Target.Optimize(planningPeriod.Id.Value);

			TrackOptimizeDaysForAgents.NumberOfOptimizations()
				.Should().Be.EqualTo(planningPeriod.Range.DayCount());
		}

		[Test]
		public void ShouldOptimizeHalfGroupIf50PercentShouldBeOptimized()
		{
			IntradayOptmizerLimiter.SetFromTest(new Percent(0.5), 0);
			var phoneActivity = ActivityFactory.CreateActivity("phone");
			var skill = SkillRepository.Has("skill", phoneActivity);
			var scenario = ScenarioRepository.Has("some name");
			var dateOnly = new DateOnly(2015, 10, 12);
			var planningPeriod = PlanningPeriodRepository.Has(dateOnly, 1);
			var agent1 = PersonRepository.Has(new Contract("_"), new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1), skill);
			var agent2 = PersonRepository.Has(new Contract("_"), new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1), skill);
			PersonAssignmentRepository.Has(agent1, scenario, phoneActivity, new ShiftCategory("_"), dateOnly, new TimePeriod(8, 0, 17, 0));
			PersonAssignmentRepository.Has(agent2, scenario, phoneActivity, new ShiftCategory("_"), dateOnly, new TimePeriod(8, 0, 17, 0));
			SkillDayRepository.Has(new []{skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60))});

			Target.Optimize(planningPeriod.Id.Value);

			TrackOptimizeDaysForAgents.NumberOfOptimizationsFor(dateOnly)
				.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldOptimizeAllIfGroupIsTooSmallNoMatterPercentSetting()
		{
			IntradayOptmizerLimiter.SetFromTest(new Percent(0.01), 3);
			var phoneActivity = ActivityFactory.CreateActivity("phone");
			var skill = SkillRepository.Has("skill", phoneActivity);
			var scenario = ScenarioRepository.Has("some name");
			var dateOnly = new DateOnly(2015, 10, 12);
			var planningPeriod = PlanningPeriodRepository.Has(dateOnly, 1);
			var agent1 = PersonRepository.Has(new Contract("_"), new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1), skill);
			var agent2 = PersonRepository.Has(new Contract("_"), new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1), skill);
			PersonAssignmentRepository.Has(agent1, scenario, phoneActivity, new ShiftCategory("_"), dateOnly, new TimePeriod(8, 0, 17, 0));
			PersonAssignmentRepository.Has(agent2, scenario, phoneActivity, new ShiftCategory("_"), dateOnly, new TimePeriod(8, 0, 17, 0));
			SkillDayRepository.Has(new[] { skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60)) });

			Target.Optimize(planningPeriod.Id.Value);

			TrackOptimizeDaysForAgents.NumberOfOptimizationsFor(dateOnly)
				.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldSkipRestOfPeriodWhenOnlyOneDayCouldBeOptimized()
		{
			IntradayOptmizerLimiter.SetFromTest(new Percent(1), 0);
			var phoneActivity = ActivityFactory.CreateActivity("phone");
			var skill = SkillRepository.Has("skill", phoneActivity);
			var scenario = ScenarioRepository.Has("some name");
			var dateOnly = new DateOnly(2015, 10, 12);
			var planningPeriod = PlanningPeriodRepository.Has(dateOnly, 1);
			var agent = PersonRepository.Has(new Contract("_"), new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1), skill);
			PersonAssignmentRepository.Has(agent, scenario, phoneActivity, new ShiftCategory("_"), dateOnly, new TimePeriod(8, 0, 17, 0));
			SkillDayRepository.Has(new[] { skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60)) });

			Target.Optimize(planningPeriod.Id.Value);

			TrackOptimizeDaysForAgents.NumberOfOptimizations()
				.Should().Be.EqualTo(2);	
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<TrackOptimizeDaysForAgents>().For<IIntradayOptimizeOneDayCallback>();
		}
	}
}