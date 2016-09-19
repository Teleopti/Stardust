﻿using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.WebReports;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.InfrastructureTest.WebReports.DailyMetricsForDay
{
	[TestFixture, Category("LongRunning")]
	public class NoDataAvailableTest : WebReportTest
	{
		protected override void InsertTestSpecificData(AnalyticsDataFactory analyticsDataFactory)
		{
		}

		[Test]
		public void ShouldHaveDataAvailableSetToFalse()
		{

			Target(
				(loggedOnUser, currentDataSource, currentBusinessUnit, globalSettingDataRepository) =>
					new DailyMetricsForDayQuery(loggedOnUser, currentDataSource, currentBusinessUnit, globalSettingDataRepository))
				.Execute(TheDate.Date)
				.Should().Be.Null();
		}
	}
}