using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.InfrastructureTest.WebReports.DailyMetricsForDay
{
	[TestFixture]
	public class NoDataAvailableTest : WebReportTest
	{
		protected override void InsertTestSpecificData(AnalyticsDataFactory analyticsDataFactory)
		{
		}

		[Test]
		public void ShouldHaveDataAvailableSetToFalse()
		{
			Target().Execute(Today.Date)
				.DataAvailable.Should().Be.False();
		}
	}
}