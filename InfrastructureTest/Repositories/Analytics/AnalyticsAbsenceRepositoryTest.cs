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
	public class AnalyticsAbsenceRepositoryTest
	{
		public IAnalyticsAbsenceRepository Target;
		public WithAnalyticsUnitOfWork WithAnalyticsUnitOfWork;
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
		public void ShouldLoadAbsences()
		{
			var abs = new Absence(22, Guid.NewGuid(), "Freee", Color.LightGreen, _datasource, businessUnitId);
			analyticsDataFactory.Setup(abs);
			analyticsDataFactory.Persist();

			var scenarios = WithAnalyticsUnitOfWork.Get(() => Target.Absences());
			scenarios.Count.Should().Be.EqualTo(1);
		}
	}
}