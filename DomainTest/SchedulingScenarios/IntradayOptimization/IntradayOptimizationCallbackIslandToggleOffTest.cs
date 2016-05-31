﻿using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	[TestFixture(false)]
	[TestFixture(true)]
	[DomainTest]
	[UseEventPublisher(typeof(RunInProcessEventPublisher))]
	public class IntradayOptimizationCallbackIslandToggleOffTest : IntradayOptimizationScenario
	{
		public FakeSkillRepository SkillRepository;
		public FakePersonRepository PersonRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public IntradayOptimizationEventHandler Target;
		public IntradayOptimizationCallbackContext CallbackContext;
		public FakePlanningPeriodRepository PlanningPeriodRepository;

		public IntradayOptimizationCallbackIslandToggleOffTest(bool jumpOutWhenLargeGroupIsHalfOptimized)
			: base(false, jumpOutWhenLargeGroupIsHalfOptimized)
		{
		}

		[Test]
		public void ShouldDoSuccesfulCallbacks()
		{
			const int numberOfAgents = 10;
			var phoneActivity = ActivityFactory.CreateActivity("phone");
			var skill = SkillRepository.Has("skill", phoneActivity);
			var scenario = ScenarioRepository.Has("some name");
			var dateOnly = new DateOnly(2015, 10, 12);
			for (var i = 0; i < numberOfAgents; i++)
			{
				var agent = PersonRepository.Has(new Contract("_"), new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1), skill);
				PersonAssignmentRepository.Has(agent, scenario, phoneActivity, new ShiftCategory("_"), dateOnly, new TimePeriod(8, 0, 17, 0));
			}
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnlyPeriod(dateOnly, dateOnly.AddDays(6)), TimeSpan.FromMinutes(60)));

			var callbackTracker = new TrackIntradayOptimizationCallback();
			using (CallbackContext.Create(callbackTracker))
			{
				Target.Handle(new OptimizationWasOrdered {AgentsInIsland = PersonRepository.LoadAll().Select(x => x.Id.Value), Period = new DateOnlyPeriod(dateOnly, dateOnly.AddDays(6))});
			}
			callbackTracker.SuccessfulOptimizations().Should().Be.EqualTo(10);
		}

		[Test]
		public void ShouldDoUnsuccesfulCallbacksWhenDemandDoesntExists()
		{
			const int numberOfAgents = 10;
			var phoneActivity = ActivityFactory.CreateActivity("phone");
			var skill = SkillRepository.Has("skill", phoneActivity);
			var scenario = ScenarioRepository.Has("some name");
			var dateOnly = new DateOnly(2015, 10, 12);
			for (var i = 0; i < numberOfAgents; i++)
			{
				var agent = PersonRepository.Has(new Contract("_"), new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1), skill);
				PersonAssignmentRepository.Has(agent, scenario, phoneActivity, new ShiftCategory("_"), dateOnly, new TimePeriod(8, 0, 17, 0));
			}
		
			var callbackTracker = new TrackIntradayOptimizationCallback();
			using (CallbackContext.Create(callbackTracker))
			{
				Target.Handle(new OptimizationWasOrdered { AgentsInIsland = PersonRepository.LoadAll().Select(x => x.Id.Value), Period = new DateOnlyPeriod(dateOnly, dateOnly.AddDays(6)) });
			}
			callbackTracker.UnSuccessfulOptimizations().Should().Be.GreaterThanOrEqualTo(10);
		}
	}
}