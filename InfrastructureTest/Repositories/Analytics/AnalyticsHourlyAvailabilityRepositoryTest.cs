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

		[Test]
		public void ShouldUpdateFactHourlyAvailabilityWithUnlinkedPersonidsWhenAddNewPersonPeriod()
		{
			var specificDate1 = new SpecificDate
			{
				Date = new DateOnly(new DateTime(2015, 1, 20)),
				DateId = 20
			};
			var specificDate2 = new SpecificDate
			{
				Date = new DateOnly(new DateTime(2015, 1, 26)),
				DateId = 26
			};
			_analyticsDataFactory.Setup(specificDate1);
			_analyticsDataFactory.Setup(specificDate2);

			const int personPeriod1 = 10;
			const int personPeriod2 = 11;
			insertPerson(personPeriod1, new DateTime(2015, 1, 10), new DateTime(2015, 1, 24), false, 10, 24);
			insertPerson(personPeriod2, new DateTime(2015, 1, 25), AnalyticsDate.Eternity.DateDate, false, 25, -2);
			_analyticsDataFactory.Setup(Scenario.DefaultScenarioFor(scenarioId, Guid.NewGuid()));
			_analyticsDataFactory.Setup(new CurrentWeekDates());
			_analyticsDataFactory.Setup(new QuarterOfAnHourInterval());
			_analyticsDataFactory.Persist();

			insertFactHourlyAvailability(personPeriod1, specificDate1.DateId);
			insertFactHourlyAvailability(personPeriod1, specificDate2.DateId);

			var rowCount = WithAnalyticsUnitOfWork.Get(() => Target.GetFactHourlyAvailabilityRowCount(personPeriod1));
			rowCount.Should().Be.EqualTo(2);
			rowCount = WithAnalyticsUnitOfWork.Get(() => Target.GetFactHourlyAvailabilityRowCount(personPeriod2));
			rowCount.Should().Be.EqualTo(0);
			WithAnalyticsUnitOfWork.Do(() => Target.UpdateUnlinkedPersonids(new[] { 10, 11 }));
			rowCount = WithAnalyticsUnitOfWork.Get(() => Target.GetFactHourlyAvailabilityRowCount(personPeriod1));
			rowCount.Should().Be.EqualTo(1);
			rowCount = WithAnalyticsUnitOfWork.Get(() => Target.GetFactHourlyAvailabilityRowCount(personPeriod2));
			rowCount.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldUpdateFactHourlyAvailabilityWithUnlinkedPersonidsWhenDeleteExistingPersonPeriod()
		{
			var specificDate1 = new SpecificDate
			{
				Date = new DateOnly(new DateTime(2015, 1, 7)),
				DateId = 7
			};
			var specificDate2 = new SpecificDate
			{
				Date = new DateOnly(new DateTime(2015, 1, 15)),
				DateId = 15
			};
			_analyticsDataFactory.Setup(specificDate1);
			_analyticsDataFactory.Setup(specificDate2);

			const int personPeriod1 = 10;
			const int personPeriod2 = 11;
			insertPerson(personPeriod2, new DateTime(2015, 1, 5), AnalyticsDate.Eternity.DateDate, false, 5, -2);
			insertPerson(personPeriod1, new DateTime(2015, 1, 10), AnalyticsDate.Eternity.DateDate, true, 10, -2);
			_analyticsDataFactory.Setup(Scenario.DefaultScenarioFor(scenarioId, Guid.NewGuid()));
			_analyticsDataFactory.Setup(new CurrentWeekDates());
			_analyticsDataFactory.Setup(new QuarterOfAnHourInterval());
			_analyticsDataFactory.Persist();

			insertFactHourlyAvailability(personPeriod1, specificDate1.DateId);
			insertFactHourlyAvailability(personPeriod2, specificDate2.DateId);

			var rowCount = WithAnalyticsUnitOfWork.Get(() => Target.GetFactHourlyAvailabilityRowCount(personPeriod1));
			rowCount.Should().Be.EqualTo(1);
			rowCount = WithAnalyticsUnitOfWork.Get(() => Target.GetFactHourlyAvailabilityRowCount(personPeriod2));
			rowCount.Should().Be.EqualTo(1);
			WithAnalyticsUnitOfWork.Do(() => Target.UpdateUnlinkedPersonids(new[] { personPeriod1, personPeriod2 }));
			rowCount = WithAnalyticsUnitOfWork.Get(() => Target.GetFactHourlyAvailabilityRowCount(personPeriod1));
			rowCount.Should().Be.EqualTo(0);
			rowCount = WithAnalyticsUnitOfWork.Get(() => Target.GetFactHourlyAvailabilityRowCount(personPeriod2));
			rowCount.Should().Be.EqualTo(2);
		}


		private void insertPerson(int personPeriodId, DateTime validFrom, DateTime validTo, bool toBeDeleted = false, int validateFromDateId = 0, int validateToDateId = -2)
		{
			_analyticsDataFactory.Setup(new Person(personPeriodId, Guid.NewGuid(), Guid.NewGuid(), "Ashley", "Andeen", validFrom,
				validTo, validateFromDateId, validateToDateId, businessUnitId, Guid.NewGuid(), _datasource, toBeDeleted,
				_timeZones.UtcTimeZoneId));
		}

		private void insertFactHourlyAvailability(int personPeriodId, int dateId)
		{
			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.AddOrUpdate(new AnalyticsHourlyAvailability
				{
					AvailableDays = 1,
					ScenarioId = scenarioId,
					PersonId = personPeriodId,
					DateId = dateId,
					AvailableTimeMinutes = 345,
					BusinessUnitId = businessUnitId,
					ScheduledDays = 0,
					ScheduledTimeMinutes = 0
				});
			});
		}
	}
}