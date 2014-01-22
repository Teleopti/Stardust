using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.WebReports.DailyMetricsForDay
{
	[TestFixture]
	public class AverageAfterCallWorkTest : WebReportTest
	{
		private const int afterCallWorkQueueOne = 100;
		private const int afterCallWorkQueueTwo = 200;
		private const int answeredCallsQueueOne = 4;
		private const int answeredCallsQueueTwo = 6;
		
		protected override void InsertTestSpecificData(AnalyticsDataFactory analyticsDataFactory)
		{
			analyticsDataFactory.Setup(new FactAgentQueue(0, 1, 1, 1,afterCallWorkQueueOne, 1, answeredCallsQueueOne, 1, Datasource.RaptorDefaultDatasourceId));
			analyticsDataFactory.Setup(new FactAgentQueue(0, 2, 1, 1,afterCallWorkQueueTwo, 1, answeredCallsQueueTwo, 1, Datasource.RaptorDefaultDatasourceId));
		}

		[Test]
		public void ShouldReturnAverageAfterCallWork()
		{
			const int expectedAverage = 30;
			Target().Execute(new DateOnlyPeriod(2000, 1, 1, 2020, 1, 1), 1, 1, SetupFixtureForAssembly.loggedOnPerson, BusinessUnitFactory.BusinessUnitUsedInTest)
				.AfterCallWorkTime.Should().Be.EqualTo(expectedAverage);
		}
	}
}