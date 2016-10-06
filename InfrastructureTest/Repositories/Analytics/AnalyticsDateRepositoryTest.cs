using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[TestFixture]
	[Category("BucketB")]
	[AnalyticsDatabaseTest]
	[ToggleOff(Toggles.ETL_EventbasedDate_39562)]
	public class AnalyticsDateRepositoryTest
	{
		public IAnalyticsDateRepository Target;
		public WithAnalyticsUnitOfWork WithAnalyticsUnitOfWork;
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
		public void ShouldLoadADate()
		{
			var date = WithAnalyticsUnitOfWork.Get(() => Target.Date(DateTime.Now));
			date.DateDate.Date.Should().Be.EqualTo(DateTime.Now.Date);
			date.DateId.Should().Be.GreaterThanOrEqualTo(0).And.Be.LessThanOrEqualTo(6);
		}

		[Test]
		public void ShouldLoadMaxDate()
		{
			var date = WithAnalyticsUnitOfWork.Get(() => Target.MaxDate());
			date.DateId.Should().Be.EqualTo(6);
		}

		[Test]
		public void ShouldLoadMinDate()
		{
			var date = WithAnalyticsUnitOfWork.Get(() => Target.MinDate());
			date.DateId.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldLoadAllDates()
		{
			var dates = WithAnalyticsUnitOfWork.Get(() => Target.GetAllPartial());
			dates.Count.Should().Be.EqualTo(7);
			dates.Any(x => x.DateDate.Date == DateTime.Today).Should().Be.True();
		}
	}
}