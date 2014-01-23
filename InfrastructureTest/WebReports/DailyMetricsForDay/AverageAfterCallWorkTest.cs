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
			analyticsDataFactory.Setup(new FactAgentQueue(Today.DateId, 1, AcdLoginId, 1, afterCallWorkQueueOne, answeredCallsQueueOne, 1));
			analyticsDataFactory.Setup(new FactAgentQueue(Today.DateId, 2, AcdLoginId, 1, afterCallWorkQueueTwo, answeredCallsQueueTwo, 1));
		}

		[Test]
		public void ShouldReturnAverageAfterCallWork()
		{
			const int expectedAverage = 30;
			Target().Execute(SetupFixtureForAssembly.loggedOnPerson, new DateOnlyPeriod(2000, 1, 1, 2020, 1, 1), 1)
				.AfterCallWorkTime.Should().Be.EqualTo(expectedAverage);
		}
	}
}