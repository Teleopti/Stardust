using System;
using System.Drawing;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[TestFixture]
	[Category("LongRunning")]
	[AnalyticsDatabaseTest]
	public class AnalyticsShiftCategoryRepositoryTest
	{
		public WithAnalyticsUnitOfWork WithAnalyticsUnitOfWork;
		public IAnalyticsShiftCategoryRepository Target;
		private AnalyticsDataFactory analyticsDataFactory;
		private UtcAndCetTimeZones _timeZones;
		private ExistingDatasources _datasource;
		private const int businessUnitId = 12;

		[SetUp]
		public void Setup()
		{
			analyticsDataFactory = new AnalyticsDataFactory();
			_timeZones = new UtcAndCetTimeZones();
			_datasource = new ExistingDatasources(_timeZones);
		}

		[Test]
		public void ShouldLoadCategories()
		{
			analyticsDataFactory.Setup(Scenario.DefaultScenarioFor(1, Guid.NewGuid()));
			analyticsDataFactory.Setup(new ShiftCategory(1, Guid.NewGuid(), "Kattegat", Color.Green, _datasource, businessUnitId));
			analyticsDataFactory.Persist();

			var cats = WithAnalyticsUnitOfWork.Get(() => Target.ShiftCategories());
			cats.Count.Should().Be.EqualTo(1);
		}
	}
}