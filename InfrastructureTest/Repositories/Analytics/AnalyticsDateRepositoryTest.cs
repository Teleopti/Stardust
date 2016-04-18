using System;
using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Infrastructure.Analytics;
using Person = Teleopti.Ccc.TestCommon.TestData.Analytics.Person;
using Scenario = Teleopti.Ccc.TestCommon.TestData.Analytics.Scenario;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[TestFixture]
	[Category("LongRunning")]
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
	}
}