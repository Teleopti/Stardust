using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;

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
			Target().Execute(SetupFixtureForAssembly.loggedOnPerson, new DateOnlyPeriod(2000, 1, 1, 2020, 1, 1), 1)
				.DataAvailable.Should().Be.False();
		}
	}
}