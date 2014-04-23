using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.WebReports;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.WebReports.DailyMetricsForDay
{
	[TestFixture]
	public class ReadyTimePerScheduledReadyTimeTest : WebReportTest
	{
		private const int readyTimeIntervalOne = 50;
		private const int readyTimeIntervalTwo = 70;
		private const int scheduledReadyTimeOneMinutes = 3;
		private const int scheduledReadyTimeTwoMinutes = 5;

		protected override void InsertTestSpecificData(AnalyticsDataFactory analyticsDataFactory)
		{
			analyticsDataFactory.Setup(new FactAgent(Today.DateId, 1, AcdLoginId, 0, 0, readyTimeIntervalOne, 1, 1, 1, 1, 1, 1, 1, 1));
			analyticsDataFactory.Setup(new FactAgent(Today.DateId, 2, AcdLoginId, 0, 0, readyTimeIntervalTwo, 1, 1, 1, 1, 1, 1, 1, 1));
			analyticsDataFactory.Setup(new FactSchedule(PersonId, Today.DateId, Today.DateId, 0, scheduledReadyTimeOneMinutes, 1, ScenarioId));
			analyticsDataFactory.Setup(new FactSchedule(PersonId, Today.DateId, Today.DateId, 0, scheduledReadyTimeTwoMinutes, 2, ScenarioId));
		}

		[Test]
		public void ShouldReturnReadyTimePerScheduledReadyTime()
		{
			var expectedPercentage = new Percent(0.25);
			Target(
				(loggedOnUser, currentDataSource, currentBusinessUnit, globalSettingDataRepository) =>
					new DailyMetricsForDayQuery(loggedOnUser, currentDataSource, currentBusinessUnit, globalSettingDataRepository))
				.Execute(Today.Date)
				.ReadyTimePerScheduledReadyTime.Should().Be.EqualTo(expectedPercentage);
		}
	}
}