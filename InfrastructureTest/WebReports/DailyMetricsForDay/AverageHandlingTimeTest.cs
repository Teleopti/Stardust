using NUnit.Framework;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.WebReports.DailyMetricsForDay
{
	[TestFixture]
	public class AverageHandlingTimeTest : WebReportTest
	{
		private const int talkTimeQueueOne = 50;
		private const int talkTimeQueueTwo = 100;
		private const int afterCallWorkQueueOne = 50;
		private const int afterCallWorkQueueTwo = 100;
		private const int answeredCallsQueueOne = 4;
		private const int answeredCallsQueueTwo = 6;
		
		protected override void InsertTestSpecificData(AnalyticsDataFactory analyticsDataFactory)
		{
			analyticsDataFactory.Setup(new FactAgentQueue(0, 1, 1, talkTimeQueueOne, afterCallWorkQueueOne, 1, answeredCallsQueueOne, 1, Datasource.RaptorDefaultDatasourceId));
			analyticsDataFactory.Setup(new FactAgentQueue(0, 2, 1, talkTimeQueueTwo, afterCallWorkQueueTwo, 1, answeredCallsQueueTwo, 1, Datasource.RaptorDefaultDatasourceId));
		}

		[Test]
		public void ShouldReturnAverageHandlingTime()
		{
			const int expectedAverage = 30;
			var result = Target().Execute(new DateOnlyPeriod(2000, 1, 1, 2020, 1, 1), 1, 1, SetupFixtureForAssembly.loggedOnPerson, BusinessUnitFactory.BusinessUnitUsedInTest);
			Assert.That(result.HandlingTime, Is.EqualTo(expectedAverage));
		}
	}
}