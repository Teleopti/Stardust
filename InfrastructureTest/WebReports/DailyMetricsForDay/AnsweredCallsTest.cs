using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;

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
			analyticsDataFactory.Setup(new FactAgentQueue(Today.DateId, 1, AcdLoginId, 1, 1, answeredOne, 1));
			analyticsDataFactory.Setup(new FactAgentQueue(Today.DateId, 2, AcdLoginId, 1, 1, answeredTwo, 1));
		}

		[Test]
		public void ShouldReturnAnsweredCallsFromAllQueues()
		{
			Target().Execute(new DateOnlyPeriod(2000, 1, 1, 2020, 1, 1))
				.AnsweredCalls.Should().Be.EqualTo(answeredCalls);
		}
	}
}