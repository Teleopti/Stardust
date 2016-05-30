using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
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
			date.DateDate.Date.Should().Be.EqualTo(DateTime.Now.Date);
			date.DateId.Should().Be.GreaterThanOrEqualTo(0).And.Be.LessThanOrEqualTo(6);
		}

		[Test]
		public void ShouldLoadMaxDate()
		{
			var date = Target.MaxDate();
			date.DateId.Should().Be.EqualTo(6);
		}

		[Test]
		public void ShouldLoadMinDate()
		{
			var date = Target.MinDate();
			date.DateId.Should().Be.EqualTo(0);
		}
	}
}