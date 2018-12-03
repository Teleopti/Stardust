using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.WebReports;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;


namespace Teleopti.Ccc.InfrastructureTest.WebReports.DailyMetricsForDay
{
	[TestFixture, Category("BucketB")]
	public class AdherenceForTypeReadyTimeVsScheduledTimeTest : WebReportTest
	{
		private const int scheduledTimeOneMinutes = 1;
		private const int scheduledTimeTwoMinutes = 3;
		private const int deviationScheduleOneSeconds = 60;
		private const int deviationScheduleTwoSeconds = 120;

		protected override AdherenceReportSettingCalculationMethod? AdherenceSetting
		{
			get { return AdherenceReportSettingCalculationMethod.ReadyTimeVSScheduledTime; }
		}

		protected override void InsertTestSpecificData(AnalyticsDataFactory analyticsDataFactory)
		{
			analyticsDataFactory.Setup(new FactSchedule(PersonId, TheDate.DateId, TheDate.DateId, scheduledTimeOneMinutes, 0, 1, ScenarioId));
			analyticsDataFactory.Setup(new FactSchedule(PersonId, TheDate.DateId, TheDate.DateId, scheduledTimeTwoMinutes, 0, 2, ScenarioId));
			analyticsDataFactory.Setup(new FactScheduleDeviation(TheDate.DateId, TheDate.DateId, 1, PersonId, 0, deviationScheduleOneSeconds, 0, 0, true));
			analyticsDataFactory.Setup(new FactScheduleDeviation(TheDate.DateId, TheDate.DateId, 2, PersonId, 0, deviationScheduleTwoSeconds, 0, 0, true));
		}

		[Test]
		public void ShouldReturnAdherenceForAdherenceType2()
		{
			var expectedPercentage = new Percent(0.25);
			Target(
				(loggedOnUser, currentDataSource, currentBusinessUnit, globalSettingDataRepository) =>
					new DailyMetricsForDayQuery(loggedOnUser, currentDataSource, currentBusinessUnit, globalSettingDataRepository))
				.Execute(TheDate.Date)
				.Adherence.Should().Be.EqualTo(expectedPercentage);
		}
	}
}