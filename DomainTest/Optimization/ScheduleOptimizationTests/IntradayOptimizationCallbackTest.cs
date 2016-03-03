using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.ScheduleOptimizationTests
{
	[Toggle(Toggles.ResourcePlanner_JumpOutWhenLargeGroupIsHalfOptimized_37049)]
	[DomainTest(false)]
	public class IntradayOptimizationCallbackTest
	{
		public FakeSkillRepository SkillRepository;
		public FakePersonRepository PersonRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public IntradayOptimizationCommandHandler Target;
		public IntradayOptimizationCallbackContext CallbackContext;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public IntradayOptimization TargetOptimization;
		public WebSchedulingSetup WebSchedulingSetup;

		[Test]
		public void ShouldDoSuccesfulCallbacks()
		{
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

			var callbackTracker = new TrackIntradayOptimizationCallback();
			using (CallbackContext.Create(callbackTracker))
			{
				Target.Execute(planningPeriod.Id.Value);
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
			var planningPeriod = PlanningPeriodRepository.Has(dateOnly, 1);
			for (var i = 0; i < numberOfAgents; i++)
			{
				var agent = PersonRepository.Has(new Contract("_"), new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1), skill);
				PersonAssignmentRepository.Has(agent, scenario, phoneActivity, new ShiftCategory("_"), dateOnly, new TimePeriod(8, 0, 17, 0));
			}
		
			var callbackTracker = new TrackIntradayOptimizationCallback();
			using (CallbackContext.Create(callbackTracker))
			{
				Target.Execute(planningPeriod.Id.Value);
			}
			callbackTracker.UnSuccessfulOptimizations().Should().Be.GreaterThanOrEqualTo(10);
		}

		[Test]
		public void ShouldOnlyDoSuccesfulCallbackOnAgentsInEvent()
		{
			var selectedAgents = new List<Guid>();
			const int numberOfAgents = 10;
			var phoneActivity = ActivityFactory.CreateActivity("phone");
			var skill = SkillRepository.Has("skill", phoneActivity);
			var scenario = ScenarioRepository.Has("some name");
			var dateOnly = new DateOnly(2015, 10, 12);
			var dateOnlyPeriod = new DateOnlyPeriod(dateOnly, dateOnly.AddDays(6));
			
			for (var i = 0; i < numberOfAgents; i++)
			{
				var agent = PersonRepository.Has(new Contract("_"), new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1), skill);
				PersonAssignmentRepository.Has(agent, scenario, phoneActivity, new ShiftCategory("_"), dateOnly, new TimePeriod(8, 0, 17, 0));
				selectedAgents.Add(agent.Id.Value);
			}

			selectedAgents.RemoveAt(0);
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnlyPeriod, TimeSpan.FromMinutes(60)));
			WebSchedulingSetup.Setup(dateOnlyPeriod);

			var callbackTracker = new TrackIntradayOptimizationCallback();
			var @event = new OptimizationWasOrdered{Period = dateOnlyPeriod, AgentIds = selectedAgents};

			using (CallbackContext.Create(callbackTracker))
			{
				TargetOptimization.Handle(@event);
			}

			callbackTracker.SuccessfulOptimizations().Should().Be.EqualTo(9);
		}
	}
}