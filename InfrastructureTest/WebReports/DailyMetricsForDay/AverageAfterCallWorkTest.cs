using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.WebReports;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;

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
			analyticsDataFactory.Setup(new FactAgentQueue(Today.DateId,1, 1, AcdLoginId, 1, afterCallWorkQueueOne, answeredCallsQueueOne, 1));
			analyticsDataFactory.Setup(new FactAgentQueue(Today.DateId, 1, 2, AcdLoginId, 1, afterCallWorkQueueTwo, answeredCallsQueueTwo, 1));
		}

		[Test]
		public void ShouldReturnAverageAfterCallWork()
		{
			var expectedAverage = TimeSpan.FromSeconds(30);
			Target(
				(loggedOnUser, currentDataSource, currentBusinessUnit, globalSettingDataRepository) =>
					new DailyMetricsForDayQuery(loggedOnUser, currentDataSource, currentBusinessUnit, globalSettingDataRepository))
				.Execute(Today.Date)
				.AfterCallWorkTimeAverage.Should().Be.EqualTo(expectedAverage);
		}
	}
}