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
		//[Test]
		//public void ShouldCallLoadWithNoRowsReturned()
		//{
		//	var target = new IntradayMonitorDataLoader();
		//	var data = target.Load();

		//	data.Count.Should().Be.EqualTo(0);
		//}
	}
}