using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[TestFixture]
	[Category("BucketB")]
	[AnalyticsDatabaseTest]
	public class IntradayQueueStatisticsLoaderTest
	{
		[Test]
		public void ShouldCheckThatStoredProcedureExists()
		{
			var target = new IntradayQueueStatisticsLoader();
			var actualWorkloadInSecondsPerSkillInterval = target.LoadActualCallPerSkillInterval(new Guid[] { Guid.NewGuid() }, TimeZoneInfo.Utc, DateOnly.Today);

			actualWorkloadInSecondsPerSkillInterval.Count.Should().Be.EqualTo(0);
		}
	}
}