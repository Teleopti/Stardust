using NUnit.Framework;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.WebReports.DailyMetricsForDay
{
	[TestFixture]
	public class AverageTalkTime : WebReportTest
	{
		private const int talkTimeQueueOne = 200;
		private const int talkTimeQueueTwo = 400;
		private const int answeredCallsQueueOne = 8;
		private const int answeredCallsQueueTwo = 12;
		
		protected override void InsertTestSpecificData(AnalyticsDataFactory analyticsDataFactory)
		{
			analyticsDataFactory.Setup(new FactAgentQueue(0, 1, 1, talkTimeQueueOne, 1, 1, answeredCallsQueueOne, 1, Datasource.RaptorDefaultDatasourceId));
			analyticsDataFactory.Setup(new FactAgentQueue(0, 2, 1, talkTimeQueueTwo, 1, 1, answeredCallsQueueTwo, 1, Datasource.RaptorDefaultDatasourceId));
		}

		[Test]
		public void ShouldReturnAverageTalkTime()
		{
			const int expectedAverage = 30;
			var result = Target().Execute(new DateOnlyPeriod(2000, 1, 1, 2020, 1, 1), 1, 1, SetupFixtureForAssembly.loggedOnPerson, BusinessUnitFactory.BusinessUnitUsedInTest);
			Assert.That(result.TalkTime, Is.EqualTo(expectedAverage));
		}
	}
}