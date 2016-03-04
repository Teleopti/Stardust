using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ResourcePlanner
{
	[DomainTest]
	public class IntradayOptimizationCommandHandlerTest
	{
		public IntradayOptimizationCommandHandler Target;
		public FakeEventPublisher EventPublisher;
		public FakePlanningPeriodRepository PlanningPeriodRepository;

		[Test]
		public void ShouldSetPeriodBasedOnPlanningPeriod()
		{
			var dateOnly = new DateOnly(2015, 10, 12);
			var planningPeriod = PlanningPeriodRepository.Has(dateOnly, 1);

			Target.Execute(planningPeriod.Id.Value, null);

			EventPublisher.PublishedEvents.OfType<OptimizationWasOrdered>().Single()
				.Period.Should().Be.EqualTo(planningPeriod.Range);
		}

		[Test]
		public void ShouldSetAgentIds()
		{
			var listOfAgentIds = new List<Guid>();

			Target.Execute(Guid.NewGuid(), listOfAgentIds);

			EventPublisher.PublishedEvents.OfType<OptimizationWasOrdered>().Single()
				.AgentIds.Should().Be.SameInstanceAs(listOfAgentIds);
		}
	}
}