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
	public class IntradayMonitorDataLoaderTest
	{
		[Test]
		public void ShouldCallLoadWithNoRowsReturned()
		{
			var intradayMonitorDataLoader = new IntradayMonitorDataLoader();
			var data = intradayMonitorDataLoader.Load(new[] {Guid.NewGuid()}, TimeZoneInfo.Utc, DateOnly.Today);

			data.Count.Should().Be.EqualTo(0);
		}
		

	}
}