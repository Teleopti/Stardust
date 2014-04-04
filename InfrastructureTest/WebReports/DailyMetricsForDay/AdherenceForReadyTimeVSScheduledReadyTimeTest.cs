using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.WebReports.DailyMetricsForDay
{
	[TestFixture]
	public class AdherenceForReadyTimeVSScheduledReadyTimeTest : WebReportTest
	{
		private const int scheduledReadyTimeOneMinutes = 1;
		private const int scheduledReadyTimeTwoMinutes = 3;
		private const int deviationScheduleReadyOneSeconds = 60;
		private const int deviationScheduleReadyTwoSeconds = 120;

		protected override AdherenceReportSettingCalculationMethod? AdherenceSetting
		{
			get { return AdherenceReportSettingCalculationMethod.ReadyTimeVSScheduledReadyTime; }
		}

		protected override void InsertTestSpecificData(AnalyticsDataFactory analyticsDataFactory)
		{
			analyticsDataFactory.Setup(new FactSchedule(PersonId, Today.DateId, Today.DateId, 0, scheduledReadyTimeOneMinutes, 1, ScenarioId));
			analyticsDataFactory.Setup(new FactSchedule(PersonId, Today.DateId, Today.DateId, 0, scheduledReadyTimeTwoMinutes, 2, ScenarioId));
			analyticsDataFactory.Setup(new FactScheduleDeviation(Today.DateId, Today.DateId, 1, PersonId, 0, 0, deviationScheduleReadyOneSeconds, 0, true));
			analyticsDataFactory.Setup(new FactScheduleDeviation(Today.DateId, Today.DateId, 2, PersonId, 0, 0, deviationScheduleReadyTwoSeconds, 0, true));
		}

		[Test]
		public void ShouldReturnAdherenceForAdherenceType1()
		{
			var expectedPercentage = new Percent(0.25);
			Target().Execute(Today.Date)
				.Adherence.Should().Be.EqualTo(expectedPercentage);
		}
	}
}