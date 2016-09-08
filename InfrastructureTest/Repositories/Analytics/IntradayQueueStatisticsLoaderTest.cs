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
	public class IntradayQueueStatisticsLoaderTest
	{
		[Test]
		public void ShouldCheckThatStoredProcedureExists()
		{
			var target = new IntradayQueueStatisticsLoader();
			var latestStatisticsTimeAndWorkload = target.LoadActualWorkloadInSeconds(new Guid[] { Guid.NewGuid() }, TimeZoneInfo.Utc, DateOnly.Today);

			latestStatisticsTimeAndWorkload.ActualworkloadInSeconds.HasValue.Should().Be.False();
			latestStatisticsTimeAndWorkload.LatestStatisticsIntervalId.HasValue.Should().Be.False();
		}
	}
}