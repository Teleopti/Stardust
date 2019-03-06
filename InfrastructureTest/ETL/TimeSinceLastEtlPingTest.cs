using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ETL;

namespace Teleopti.Ccc.InfrastructureTest.ETL
{
	[AnalyticsDatabaseTest]
	public class TimeSinceLastEtlPingTest
	{
		public ITimeSinceLastEtlPing TimeSinceLastEtlPing;
		public IMarkEtlPing MarkEtlPing;
		
		[Test]
		public void ShouldHaveOldValueBeforeAnythingHasRun()
		{
			TimeSinceLastEtlPing.Fetch().TotalDays
				.Should().Be.GreaterThan(1);				
		}
		
		[Test]
		public void ShouldBeSetWhenGetSchedulesAreCalled()
		{
			MarkEtlPing.Store();

			TimeSinceLastEtlPing.Fetch().TotalSeconds
				.Should().Be.LessThan(3);
		}
	}
}