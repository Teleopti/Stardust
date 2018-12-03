using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.WebReports;
using Teleopti.Ccc.InfrastructureTest.WebReports.DailyMetricsForDay;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;


namespace Teleopti.Ccc.InfrastructureTest.WebReports.DetailedAdherenceForDay
{
	[TestFixture, Category("BucketB")]
	public class TotalAdherenceForDateTest : WebReportTest
	{
		private const int scheduledReadyTimeOneMinutes = 1;
		private const int scheduledReadyTimeTwoMinutes = 3;
		private const int deviationScheduleReadyOneSeconds = 60;
		private const int deviationScheduleReadyTwoSeconds = 120;

		protected override void InsertTestSpecificData(AnalyticsDataFactory analyticsDataFactory)
		{
			analyticsDataFactory.Setup(new FactSchedule(PersonId, TheDate.DateId, TheDate.DateId, 0, scheduledReadyTimeOneMinutes, 1, ScenarioId));
			analyticsDataFactory.Setup(new FactSchedule(PersonId, TheDate.DateId, TheDate.DateId, 0, scheduledReadyTimeTwoMinutes, 2, ScenarioId));
			analyticsDataFactory.Setup(new FactScheduleDeviation(TheDate.DateId, TheDate.DateId, 1, PersonId, 0, 0, deviationScheduleReadyOneSeconds, 0, true));
			analyticsDataFactory.Setup(new FactScheduleDeviation(TheDate.DateId, TheDate.DateId, 2, PersonId, 0, 0, deviationScheduleReadyTwoSeconds, 0, true));
		}

		[Test]
		public void ShouldReturnTotalAdherenceForDate()
		{
			var expectedPercentage = new Percent(0.25);
			Target(
				(loggedOnUser, currentDataSource, currentBusinessUnit, globalSettingDataRepository) =>
					new DetailedAdherenceForDayQuery(loggedOnUser, currentDataSource, currentBusinessUnit, globalSettingDataRepository))
				.Execute(TheDate.Date).Last()
				.TotalAdherence.Should().Be.EqualTo(expectedPercentage);
		}
	}
}