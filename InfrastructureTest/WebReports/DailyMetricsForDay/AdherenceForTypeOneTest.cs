using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.WebReports.DailyMetricsForDay
{
	[TestFixture]
	public class AdherenceForTypeOneTest : WebReportTest
	{
		private const int scheduledReadyTimeOneMinutes = 1;
		private const int scheduledReadyTimeTwoMinutes = 3;
		private const int deviationScheduleReadyOneSeconds = 60;
		private const int deviationScheduleReadyTwoSeconds = 120;
		protected override int AdherenceId
		{
			get { return 1; }
		}

		protected override void InsertTestSpecificData(AnalyticsDataFactory analyticsDataFactory)
		{
			analyticsDataFactory.Setup(new FactSchedule(PersonId, Today.DateId, 0, scheduledReadyTimeOneMinutes, 1, ScenarioId));
			analyticsDataFactory.Setup(new FactSchedule(PersonId, Today.DateId, 0, scheduledReadyTimeTwoMinutes, 2, ScenarioId));
			analyticsDataFactory.Setup(new FactScheduleDeviation(Today.DateId, 1, PersonId, 0, deviationScheduleReadyOneSeconds, true));
			analyticsDataFactory.Setup(new FactScheduleDeviation(Today.DateId, 2, PersonId, 0, deviationScheduleReadyTwoSeconds, true));
		}

		[Test]
		public void ShouldReturnAdherenceForAdherenceType1()
		{
			const int expectedPercentage = 25;
			Target().Execute(Today.Date)
				.Adherence.Should().Be.EqualTo(expectedPercentage);
		}
	}
}