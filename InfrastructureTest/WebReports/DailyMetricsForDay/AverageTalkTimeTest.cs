using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.WebReports;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.InfrastructureTest.WebReports.DailyMetricsForDay
{
	[TestFixture]
	public class AverageTalkTimeTest : WebReportTest
	{
		private const int talkTimeQueueOne = 200;
		private const int talkTimeQueueTwo = 400;
		private const int answeredCallsQueueOne = 8;
		private const int answeredCallsQueueTwo = 12;
		
		protected override void InsertTestSpecificData(AnalyticsDataFactory analyticsDataFactory)
		{
			analyticsDataFactory.Setup(new FactAgentQueue(Today.DateId,1, 1, AcdLoginId, talkTimeQueueOne, 1, answeredCallsQueueOne, 1));
			analyticsDataFactory.Setup(new FactAgentQueue(Today.DateId, 1, 2, AcdLoginId, talkTimeQueueTwo, 1, answeredCallsQueueTwo, 1));
		}

		[Test]
		public void ShouldReturnAverageTalkTime()
		{
			var expectedAverage = TimeSpan.FromSeconds(30);
			Target(
				(loggedOnUser, currentDataSource, currentBusinessUnit, globalSettingDataRepository) =>
					new DailyMetricsForDayQuery(loggedOnUser, currentDataSource, currentBusinessUnit, globalSettingDataRepository))
				.Execute(Today.Date)
				.TalkTimeAverage.Should().Be.EqualTo(expectedAverage);
		}
	}
}