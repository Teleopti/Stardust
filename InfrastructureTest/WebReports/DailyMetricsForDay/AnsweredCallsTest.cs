using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.WebReports;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.InfrastructureTest.WebReports.DailyMetricsForDay
{
	[TestFixture]
	public class AnsweredCallsTest : WebReportTest
	{
		private int answeredCalls;

		protected override void InsertTestSpecificData(AnalyticsDataFactory analyticsDataFactory)
		{
			const int answeredOne = 1;
			const int answeredTwo = 2;
			answeredCalls = answeredOne + answeredTwo;
			analyticsDataFactory.Setup(new FactAgentQueue(Today.DateId,1, 1, AcdLoginId, 1, 1, answeredOne, 1));
			analyticsDataFactory.Setup(new FactAgentQueue(Today.DateId,1, 2, AcdLoginId, 1, 1, answeredTwo, 1));
		}

		[Test]
		public void ShouldReturnAnsweredCallsFromAllQueues()
		{
			Target(
				(loggedOnUser, currentDataSource, currentBusinessUnit, globalSettingDataRepository) =>
					new DailyMetricsForDayQuery(loggedOnUser, currentDataSource, currentBusinessUnit, globalSettingDataRepository))
				.Execute(Today.Date)
				.AnsweredCalls.Should().Be.EqualTo(answeredCalls);
		}
	}
}