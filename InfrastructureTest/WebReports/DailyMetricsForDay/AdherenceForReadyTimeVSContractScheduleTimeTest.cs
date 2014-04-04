using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.WebReports.DailyMetricsForDay
{
	[TestFixture]
	public class AdherenceForReadyTimeVSContractScheduleTimeTest : WebReportTest
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
			analyticsDataFactory.Setup(new FactScheduleDeviation(Today.DateId, Today.DateId, 1, PersonId, contractTimeOneSeconds, 0, 0, deviationContractOneSeconds, true));
			analyticsDataFactory.Setup(new FactScheduleDeviation(Today.DateId, Today.DateId, 2, PersonId, contractTimeOTwoSeconds, 0, 0, deviationContractTwoSeconds, true));
		}

		[Test]
		public void ShouldReturnAdherenceForAdherenceType3()
		{
			var expectedPercentage = new Percent(0.25);
			Target().Execute(Today.Date)
				.Adherence.Should().Be.EqualTo(expectedPercentage);
		}
	}
}