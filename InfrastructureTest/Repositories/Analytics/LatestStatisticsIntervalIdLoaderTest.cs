using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[TestFixture]
	[Category("LongRunning")]
	[AnalyticsDatabaseTest]
	public class LatestStatisticsIntervalIdLoaderTest
	{
		[Test]
		public void ShouldCheckThatStoredProcedureExists()
		{
			var target = new LatestStatisticsIntervalIdLoader();
			var intervalId = target.Load(new Guid[] {Guid.NewGuid()}, DateOnly.Today, TimeZoneInfo.Utc);

			intervalId.HasValue.Should().Be.False();
		}
	}
}