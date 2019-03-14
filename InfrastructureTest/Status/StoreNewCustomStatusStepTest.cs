using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Status;
using Teleopti.Ccc.Infrastructure.Status;

namespace Teleopti.Ccc.InfrastructureTest.Status
{
	[DatabaseTest]
	public class StoreNewCustomStatusStepTest
	{
		public StoreNewCustomStatusStep Target;
		public IFetchCustomStatusSteps FetchCustomStatusSteps;

		[Test]
		public void ShouldStoreNewStep()
		{
			var name = Guid.NewGuid().ToString();
			var desc = Guid.NewGuid().ToString();
			var limit = TimeSpan.FromDays(0.44);
			
			Target.Execute(name, desc, limit);

			var result = FetchCustomStatusSteps.Execute(name);
			result.Description.Should().Be.EqualTo(desc);
			result.Limit.Should().Be.EqualTo(limit);
			result.TimeSinceLastPing.Ticks.Should().Be.GreaterThan(TimeSpan.FromDays(10).Ticks);
		}
		
		
		[Test]
		public void ShouldIncludeUniqueId()
		{
			Target.Execute("1", "1", TimeSpan.FromDays(1));
			Target.Execute("2", "2", TimeSpan.FromDays(1));

			var result1 = FetchCustomStatusSteps.Execute("1");
			var result2 = FetchCustomStatusSteps.Execute("2");
			
			result1.Id.Should().Not.Be.EqualTo(result2.Id);
		}
	}
}