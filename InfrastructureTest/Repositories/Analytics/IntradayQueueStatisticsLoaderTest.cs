using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[TestFixture]
	[Category("BucketB")]
	[AnalyticsDatabaseTest]
	public class IntradayQueueStatisticsLoaderTest
	{
		[Test]
		public void ShouldCheckThatStoredsProcedureExists()
		{
			var target = new IntradayQueueStatisticsLoader();
			var emailBacklogWorkload = target.LoadActualEmailBacklogForWorkload(Guid.NewGuid(), new DateTimePeriod(DateTime.UtcNow.Date, DateTime.UtcNow.Date));

			emailBacklogWorkload.Should().Be.EqualTo(0);
		}
	}
}