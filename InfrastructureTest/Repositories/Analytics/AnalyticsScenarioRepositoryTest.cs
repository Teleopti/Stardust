using System;
using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Foundation;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Infrastructure.Analytics;
using Person = Teleopti.Ccc.TestCommon.TestData.Analytics.Person;
using Scenario = Teleopti.Ccc.TestCommon.TestData.Analytics.Scenario;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[TestFixture]
	[Category("LongRunning")]
	[AnalyticsDatabaseTest]
	public class AnalyticsScenarioRepositoryTest
	{
		public IAnalyticsScenarioRepository Target;
		public WithAnalyticsUnitOfWork WithAnalyticsUnitOfWork;
		private AnalyticsDataFactory analyticsDataFactory;
		private UtcAndCetTimeZones _timeZones;
		private ExistingDatasources _datasource;

		[SetUp]
		public void Setup()
		{
			analyticsDataFactory = new AnalyticsDataFactory();
			_timeZones = new UtcAndCetTimeZones();
			_datasource = new ExistingDatasources(_timeZones);
		}
		
		[Test]
		public void ShouldLoadScenarios()
		{
			analyticsDataFactory.Setup(new Scenario(10, Guid.NewGuid(), false));
			analyticsDataFactory.Setup(new Scenario(1, Guid.NewGuid(), true));
			analyticsDataFactory.Persist();

			var scenarios = WithAnalyticsUnitOfWork.Get(() => Target.Scenarios());
			scenarios.Count.Should().Be.EqualTo(2);
		}
	}
}