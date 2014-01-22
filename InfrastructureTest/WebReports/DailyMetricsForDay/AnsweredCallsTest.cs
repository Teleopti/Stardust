using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.FakeData;
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
			analyticsDataFactory.Setup(new FactAgentQueue(0, 1, 1, 1, 1, 1, answeredOne, 1, Datasource.RaptorDefaultDatasourceId));
			analyticsDataFactory.Setup(new FactAgentQueue(0, 2, 1, 1, 1, 1, answeredTwo, 1, Datasource.RaptorDefaultDatasourceId));
		}

		[Test]
		public void ShouldReturnAnsweredCallsFromAllQueues()
		{
			Target().Execute(new DateOnlyPeriod(2000, 1, 1, 2020, 1, 1), 1, 1, SetupFixtureForAssembly.loggedOnPerson, BusinessUnitFactory.BusinessUnitUsedInTest)
				.AnsweredCalls.Should().Be.EqualTo(answeredCalls);
		}
	}
}