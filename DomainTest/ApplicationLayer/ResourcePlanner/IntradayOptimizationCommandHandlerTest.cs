using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ResourcePlanner
{
	[DomainTest]
	public class IntradayOptimizationCommandHandlerTest
	{
		public IntradayOptimizationCommandHandler Target;
		public FakeEventPublisher EventPublisher;

		[Test]
		public void ShouldCreateEventForPlanningPeriod()
		{
			var planningPeriodId = Guid.NewGuid();

			Target.Execute(planningPeriodId);

			EventPublisher.PublishedEvents.OfType<OptimizationWasOrdered>().Single()
				.PlanningPeriodId.Should().Be.EqualTo(planningPeriodId);
		}
	}
}