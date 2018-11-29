using System;
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
	public class AdherenceForNoDeviationTest: WebReportTest
	{
		protected override AdherenceReportSettingCalculationMethod? AdherenceSetting
		{
			get { return AdherenceReportSettingCalculationMethod.ReadyTimeVSScheduledTime; }
		}

		protected override void InsertTestSpecificData(AnalyticsDataFactory analyticsDataFactory)
		{
			analyticsDataFactory.Setup(new FactSchedule(PersonId, TheDate.DateId, TheDate.DateId, 1, 0, 1, ScenarioId));
		}

		[Test]
		public void ShouldReturnAdherenceWith100Percent()
		{
			var expectedPercentage = new Percent(1);
			Target(
					(loggedOnUser, currentDataSource, currentBusinessUnit, globalSettingDataRepository) =>
						new DailyMetricsForDayQuery(loggedOnUser, currentDataSource, currentBusinessUnit, globalSettingDataRepository))
				.Execute(TheDate.Date)
				.Adherence.Should().Be.EqualTo(expectedPercentage);
		}
	}
}
