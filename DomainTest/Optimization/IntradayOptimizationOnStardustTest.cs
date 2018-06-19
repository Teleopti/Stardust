using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[DomainTest]
	public class IntradayOptimizationOnStardustTest
	{
		public FakeEventPublisher EventPublisher;
		public IntradayOptimizationOnStardust Target;
		
		[Test]
		public void ShouldPublishEvent()
		{
			var planningPeriodId = Guid.NewGuid();

			Target.Execute(planningPeriodId);

			EventPublisher.PublishedEvents.OfType<IntradayOptimizationOnStardustWasOrdered>().Single().PlanningPeriodId
				.Should().Be.EqualTo(planningPeriodId);
		}
	}
}