using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.InfrastructureTest.WebReports.DailyMetricsForDay
{
	[TestFixture]
	public class AdherenceForTypeTwoTest : WebReportTest
	{
		private const int scheduledTimeOneMinutes = 1;
		private const int scheduledTimeTwoMinutes = 3;
		private const int deviationScheduleOneSeconds = 60;
		private const int deviationScheduleTwoSeconds = 120;
		protected override int AdherenceId
		{
			get { return 2; }
		}

		protected override void InsertTestSpecificData(AnalyticsDataFactory analyticsDataFactory)
		{
			analyticsDataFactory.Setup(new FactSchedule(PersonId, Today.DateId, scheduledTimeOneMinutes, 0, 1, ScenarioId));
			analyticsDataFactory.Setup(new FactSchedule(PersonId, Today.DateId, scheduledTimeTwoMinutes, 0, 2, ScenarioId));
			analyticsDataFactory.Setup(new FactScheduleDeviation(Today.DateId, 1, PersonId, deviationScheduleOneSeconds, 0, true));
			analyticsDataFactory.Setup(new FactScheduleDeviation(Today.DateId, 2, PersonId, deviationScheduleTwoSeconds, 0, true));
		}

		[Test]
		public void ShouldReturnAdherenceForAdherenceType2()
		{
			const int expectedPercentage = 25;
			Target().Execute(Today.Date)
				.Adherence.Should().Be.EqualTo(expectedPercentage);
		}
	}
}