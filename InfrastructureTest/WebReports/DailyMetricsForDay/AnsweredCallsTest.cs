using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.WebReports;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.WebReports.DailyMetricsForDay
{
	[TestFixture]
	public class AnsweredCallsTest : WebReportTest
	{
		private const int answeredCalls = 3;

		protected override void InsertTestSpecificData(AnalyticsDataFactory analyticsDataFactory)
		{
			analyticsDataFactory.Setup(new FactAgentQueue(0, 1, 1, 1, 1, 1, answeredCalls, 1, Datasource.RaptorDefaultDatasourceId));
		}

		[Test]
		public void ShouldReturnAnsweredCalls()
		{
			var result = new DailyMetricsForDayQuery().Execute(new DateOnlyPeriod(2000, 1, 1, 2020, 1, 1), 1, 1, SetupFixtureForAssembly.loggedOnPerson, BusinessUnitFactory.BusinessUnitUsedInTest);
			Assert.That(result.AnsweredCalls,Is.EqualTo(answeredCalls));
		}
	}
}