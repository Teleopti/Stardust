using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[TestFixture]
	[Category("LongRunning")]
	[AnalyticsDatabaseTest]
	public class AnalyticsTimeZoneRepositoryTest
	{
		[Test]
		public void ShouldGet()
		{
			var timeZones = new UtcAndCetTimeZones();
			var analyticsDataFactory = new AnalyticsDataFactory();
			analyticsDataFactory.Setup(timeZones);
			analyticsDataFactory.Persist();
			var target = new AnalyticsTimeZoneRepository(CurrentDataSource.Make());

			var result = target.Get("W. Europe Standard Time");
			result.TimeZoneId.Should().Be.EqualTo(1);
		}
	}
}