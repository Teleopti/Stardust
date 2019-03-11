using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Status;
using Teleopti.Ccc.Infrastructure.Status;

namespace Teleopti.Ccc.InfrastructureTest.Status
{
	[DatabaseTest]
	public class PingCustomStepTest
	{
		public PingCustomStep Target;
		public IFetchCustomStatusSteps FetchCustomStatusSteps;
		public StoreNewCustomStatusStep StoreNewCustomStatusStep;
		
		[Test]
		public void ShouldSetLastPing()
		{
			var stepName = Guid.NewGuid().ToString();
			StoreNewCustomStatusStep.Execute(stepName, string.Empty, TimeSpan.Zero);

			Target.Execute(stepName);
			
			FetchCustomStatusSteps.Execute(stepName).TimeSinceLastPing.Ticks
				.Should().Be.LessThan(TimeSpan.FromSeconds(1).Ticks)
				.And.Be.GreaterThanOrEqualTo(0);
		}

		[TestCase("found", ExpectedResult = true)]
		[TestCase("notFound", ExpectedResult = false)]
		public bool ShouldReturnValueBasedOnStepNameIsFound(string stepName)
		{
			StoreNewCustomStatusStep.Execute("found", string.Empty, TimeSpan.Zero);

			return Target.Execute(stepName);
		}
	}
}