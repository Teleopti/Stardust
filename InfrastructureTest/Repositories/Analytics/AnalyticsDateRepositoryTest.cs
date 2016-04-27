using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[TestFixture]
	[Category("LongRunning")]
	[AnalyticsDatabaseTest]
	public class AnalyticsDateRepositoryTest
	{
		private IAnalyticsDateRepository _target;
		private AnalyticsDataFactory analyticsDataFactory;
		ICurrentDataSource currentDataSource;

		[SetUp]
		public void Setup()
		{
			currentDataSource = CurrentDataSource.Make();
			_target = new AnalyticsDateRepository(currentDataSource);

			analyticsDataFactory = new AnalyticsDataFactory();
		}

		[Test]
		public void ShouldLoadDates()
		{
			var weekDates = new CurrentWeekDates();
			analyticsDataFactory.Setup(weekDates);
			analyticsDataFactory.Persist();
			var dates = _target.Dates();
			dates.Count.Should().Be.EqualTo(7);
		}

		[Test]
		public void ShouldLoadADate()
		{
			var weekDates = new CurrentWeekDates();
			analyticsDataFactory.Setup(weekDates);
			analyticsDataFactory.Persist();
			var date = _target.Date(DateTime.Now);
			date.Key.Date.Should().Be.EqualTo(DateTime.Now.Date);
			date.Value.Should().Be.GreaterThanOrEqualTo(0).And.Be.LessThanOrEqualTo(6);
		}
	}
}