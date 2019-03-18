using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Status;

namespace Teleopti.Ccc.InfrastructureTest.Status
{
	[DatabaseTest]
	public class FetchCustomStatusStepsTest
	{
		public IFetchCustomStatusSteps FetchCustomStatusSteps;
		
		[Test]
		public void ShouldIncludeEtlTest()
		{
			FetchCustomStatusSteps.Execute()
				.SingleOrDefault(x => x.Name.Equals("ETL"))
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldLoadDefaultEtlData()
		{
			var result = FetchCustomStatusSteps.Execute("ETL");
			result.Name.Should().Be.EqualTo("ETL");
			result.Description.Should().Not.Be.Null();
			result.Limit.Should().Be.EqualTo(TimeSpan.FromSeconds(40));
			result.TimeSinceLastPing.Ticks.Should().Be.GreaterThan(TimeSpan.FromDays(10).Ticks);
			result.CanBeDeleted.Should().Be.False();
		}

		[Test]
		public void ShouldReturnNullIfNotFound()
		{
			FetchCustomStatusSteps.Execute(Guid.NewGuid().ToString())
				.Should().Be.Null();
		}
	}
}