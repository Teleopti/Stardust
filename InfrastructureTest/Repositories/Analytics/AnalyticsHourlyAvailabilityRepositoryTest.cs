using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;


namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[TestFixture]
	[Category("BucketB")]
	[AnalyticsDatabaseTest]
	public class AnalyticsHourlyAvailabilityRepositoryTest
	{
		public IAnalyticsHourlyAvailabilityRepository Target;
		public WithAnalyticsUnitOfWork WithAnalyticsUnitOfWork;

		private AnalyticsDataFactory _analyticsDataFactory;
		private UtcAndCetTimeZones _timeZones;
		private ExistingDatasources _datasource;
		private const int businessUnitId = 12;
		private const int scenarioId = 10;
		private const int personId = 10;

		[SetUp]
		public void Setup()
		{
			_analyticsDataFactory = new AnalyticsDataFactory();
			_timeZones = new UtcAndCetTimeZones();
			_datasource = new ExistingDatasources(_timeZones);
		}

		[Test]
		public void ShouldAddAndUpdateAvailability()
		{
			_analyticsDataFactory.Setup(new Person(personId, Guid.NewGuid(), Guid.NewGuid(), "Ashley", "Andeen", new DateTime(2010, 1, 1),
				AnalyticsDate.Eternity.DateDate, 0, -2, businessUnitId, Guid.NewGuid(), _datasource, false, _timeZones.UtcTimeZoneId));

			_analyticsDataFactory.Setup(Scenario.DefaultScenarioFor(scenarioId, Guid.NewGuid()));
			_analyticsDataFactory.Setup(new CurrentWeekDates());
			_analyticsDataFactory.Setup(new QuarterOfAnHourInterval());
			_analyticsDataFactory.Persist();

			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.AddOrUpdate(new AnalyticsHourlyAvailability
				{
					AvailableDays = 1,
					ScenarioId = scenarioId,
					PersonId = personId,
					DateId = 2,
					AvailableTimeMinutes = 345,
					BusinessUnitId = businessUnitId,
					ScheduledDays = 0,
					ScheduledTimeMinutes = 0
				});
			});

			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.AddOrUpdate(new AnalyticsHourlyAvailability
				{
					AvailableDays = 1,
					ScenarioId = scenarioId,
					PersonId = personId,
					DateId = 2,
					AvailableTimeMinutes = 567,
					BusinessUnitId = businessUnitId,
					ScheduledDays = 1,
					ScheduledTimeMinutes = 234
				});
			});
		}

		[Test]
		public void ShouldDeleteAvailability()
		{
			var personCode = Guid.NewGuid();
			_analyticsDataFactory.Setup(new Person(personId, personCode, Guid.NewGuid(), "Ashley", "Andeen", new DateTime(2010, 1, 1),
				AnalyticsDate.Eternity.DateDate, 0, -2, businessUnitId, Guid.NewGuid(), _datasource, false, _timeZones.UtcTimeZoneId));

			_analyticsDataFactory.Setup(Scenario.DefaultScenarioFor(scenarioId, Guid.NewGuid()));
			_analyticsDataFactory.Setup(new CurrentWeekDates());
			_analyticsDataFactory.Setup(new QuarterOfAnHourInterval());
			_analyticsDataFactory.Persist();

			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.AddOrUpdate(new AnalyticsHourlyAvailability
				{
					AvailableDays = 1,
					ScenarioId = scenarioId,
					PersonId = personId,
					DateId = 2,
					AvailableTimeMinutes = 345,
					BusinessUnitId = businessUnitId,
					ScheduledDays = 0,
					ScheduledTimeMinutes = 0
				});
			});

			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.Delete(personCode, 2, scenarioId);
			});
		}
	}
}