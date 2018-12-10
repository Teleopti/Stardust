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
	public class AdherenceForReadyTimeVsContractScheduleTimeTest : WebReportTest
	{
		private const int contractTimeOneSeconds = 60;
		private const int contractTimeOTwoSeconds = 180;
		private const int deviationContractOneSeconds = 60;
		private const int deviationContractTwoSeconds = 120;

		protected override AdherenceReportSettingCalculationMethod? AdherenceSetting
		{
			get { return AdherenceReportSettingCalculationMethod.ReadyTimeVSContractScheduleTime; }
		}

		protected override void InsertTestSpecificData(AnalyticsDataFactory analyticsDataFactory)
		{
			analyticsDataFactory.Setup(new FactScheduleDeviation(TheDate.DateId, TheDate.DateId, 1, PersonId, contractTimeOneSeconds, 0, 0, deviationContractOneSeconds, true));
			analyticsDataFactory.Setup(new FactScheduleDeviation(TheDate.DateId, TheDate.DateId, 2, PersonId, contractTimeOTwoSeconds, 0, 0, deviationContractTwoSeconds, true));
		}

		[Test]
		public void ShouldReturnAdherenceForAdherenceType3()
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