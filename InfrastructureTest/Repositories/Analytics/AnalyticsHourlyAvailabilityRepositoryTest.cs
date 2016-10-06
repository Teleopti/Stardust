using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Analytics;
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

		private UtcAndCetTimeZones _timeZones;
		private ExistingDatasources _datasource;
		private const int businessUnitId = 12;
		private const int scenarioId = 10;
		private const int personId = 10;

		[SetUp]
		public void Setup()
		{
			var analyticsDataFactory = new AnalyticsDataFactory();
			_timeZones = new UtcAndCetTimeZones();
			_datasource = new ExistingDatasources(_timeZones);

			analyticsDataFactory.Setup(new Person(personId, Guid.NewGuid(), Guid.NewGuid(), "Ashley", "Andeen", new DateTime(2010, 1, 1),
				AnalyticsDate.Eternity.DateDate, 0, -2, businessUnitId, Guid.NewGuid(), _datasource, false, _timeZones.UtcTimeZoneId));

			analyticsDataFactory.Setup(Scenario.DefaultScenarioFor(scenarioId, Guid.NewGuid()));

			analyticsDataFactory.Setup(new CurrentWeekDates());
			analyticsDataFactory.Setup(new QuarterOfAnHourInterval());

			analyticsDataFactory.Persist();
		}

		[Test]
		public void ShouldAddAndUpdateAvailability()
		{
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
				Target.Delete(personId, 2, scenarioId);
			});
		}

	}
}