using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.WebReports;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;


namespace Teleopti.Ccc.InfrastructureTest.WebReports.DailyMetricsForDay
{
	[TestFixture, Category("BucketB")]
	public class ReadyTimePerScheduledReadyTimeTest : WebReportTest
	{
		private const int readyTimeIntervalOne = 50;
		private const int readyTimeIntervalTwo = 70;
		private const int scheduledReadyTimeOneMinutes = 3;
		private const int scheduledReadyTimeTwoMinutes = 5;

		protected override void InsertTestSpecificData(AnalyticsDataFactory analyticsDataFactory)
		{
			analyticsDataFactory.Setup(new FactAgent(TheDate.DateId, 1, AcdLoginId, readyTimeIntervalOne, 1, 1, 1, 1, 1, 1, 1, 1));
			analyticsDataFactory.Setup(new FactAgent(TheDate.DateId, 2, AcdLoginId, readyTimeIntervalTwo, 1, 1, 1, 1, 1, 1, 1, 1));
			analyticsDataFactory.Setup(new FactSchedule(PersonId, TheDate.DateId, TheDate.DateId, 0, scheduledReadyTimeOneMinutes, 1, ScenarioId));
			analyticsDataFactory.Setup(new FactSchedule(PersonId, TheDate.DateId, TheDate.DateId, 0, scheduledReadyTimeTwoMinutes, 2, ScenarioId));
		}

		[Test]
		public void ShouldReturnReadyTimePerScheduledReadyTime()
		{
			var expectedPercentage = new Percent(0.25);
			Target(
				(loggedOnUser, currentDataSource, currentBusinessUnit, globalSettingDataRepository) =>
					new DailyMetricsForDayQuery(loggedOnUser, currentDataSource, currentBusinessUnit, globalSettingDataRepository))
				.Execute(TheDate.Date)
				.ReadyTimePerScheduledReadyTime.Should().Be.EqualTo(expectedPercentage);
		}
	}
}