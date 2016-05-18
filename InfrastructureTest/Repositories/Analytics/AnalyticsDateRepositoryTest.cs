using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.Infrastructure.Analytics;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[TestFixture]
	[Category("LongRunning")]
	[AnalyticsUnitOfWorkTest]
	public class AnalyticsDateRepositoryTest
	{
		public ICurrentAnalyticsUnitOfWork UnitOfWork;
		public IAnalyticsDateRepository Target;
		private AnalyticsDataFactory analyticsDataFactory;

		[SetUp]
		public void Setup()
		{
			analyticsDataFactory = new AnalyticsDataFactory();
			var weekDates = new CurrentWeekDates();
			analyticsDataFactory.Setup(weekDates);
			analyticsDataFactory.Persist();
		}

		[Test]
		public void ShouldLoadDates()
		{
			var dates = Target.Dates();
			dates.Count.Should().Be.EqualTo(7);
		}

		[Test]
		public void ShouldLoadADate()
		{
			var date = Target.Date(DateTime.Now);
			date.Key.Date.Should().Be.EqualTo(DateTime.Now.Date);
			date.Value.Should().Be.GreaterThanOrEqualTo(0).And.Be.LessThanOrEqualTo(6);
		}
	}
}