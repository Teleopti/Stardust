using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Person = Teleopti.Ccc.TestCommon.TestData.Analytics.Person;
using Scenario = Teleopti.Ccc.TestCommon.TestData.Analytics.Scenario;
using Teleopti.Ccc.Domain.UnitOfWork;


namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[TestFixture]
	[Category("BucketB")]
	[AnalyticsDatabaseTest]
	public class AnalyticsPreferenceRepositoryTest
	{
		public IAnalyticsPreferenceRepository Target;
		public WithAnalyticsUnitOfWork WithAnalyticsUnitOfWork;
		private UtcAndCetTimeZones _timeZones;
		private ExistingDatasources _datasource;
		private const int businessUnitId = 12;
		private AnalyticsDataFactory analyticsDataFactory;
		private Guid personCode1, personCode2;


		[SetUp]
		public void Setup()
		{
			analyticsDataFactory = new AnalyticsDataFactory();
			_timeZones = new UtcAndCetTimeZones();
			_datasource = new ExistingDatasources(_timeZones);
		}

		private void commonSetup()
		{
			personCode1 = Guid.NewGuid();
			personCode2 = Guid.NewGuid();

			analyticsDataFactory.Setup(new Person(10, personCode1, Guid.NewGuid(), "Ashley", "Andeen", new DateTime(2010, 1, 1),
							AnalyticsDate.Eternity.DateDate, 0, -2, businessUnitId, Guid.NewGuid(), _datasource, false, _timeZones.UtcTimeZoneId));

			analyticsDataFactory.Setup(new Person(20, personCode2, Guid.NewGuid(), "Teleopti", "Demo", new DateTime(2010, 1, 1),
							AnalyticsDate.Eternity.DateDate, 0, -2, businessUnitId, Guid.NewGuid(), _datasource, false, _timeZones.UtcTimeZoneId));

			setupForFactSchedulePreference();

			analyticsDataFactory.Persist();
		}

		private void setupForFactSchedulePreference()
		{
			analyticsDataFactory.Setup(Scenario.DefaultScenarioFor(10, Guid.NewGuid()));

			var actEmpty = new Activity(-1, Guid.NewGuid(), "Empty", Color.Black, _datasource, businessUnitId);
			var act = new Activity(22, Guid.NewGuid(), "Phone", Color.LightGreen, _datasource, businessUnitId);

			var absEmpty = new Absence(-1, Guid.NewGuid(), "Empty", Color.Black, _datasource, businessUnitId);
			var abs = new Absence(22, Guid.NewGuid(), "Freee", Color.LightGreen, _datasource, businessUnitId);

			var shiftLength = new ShiftLength(4, 480, _datasource);

			var emptyShiftCategory = new ShiftCategory(-1, Guid.Empty, "Not defined", Color.Aqua, _datasource, businessUnitId);
			var shiftCategory = new ShiftCategory(12, Guid.NewGuid(), "Shift 1", Color.Aqua, _datasource, businessUnitId);

			analyticsDataFactory.Setup(act);
			analyticsDataFactory.Setup(actEmpty);
			analyticsDataFactory.Setup(abs);
			analyticsDataFactory.Setup(absEmpty);
			analyticsDataFactory.Setup(new CurrentWeekDates());
			analyticsDataFactory.Setup(new QuarterOfAnHourInterval());
			analyticsDataFactory.Setup(shiftLength);
			analyticsDataFactory.Setup(emptyShiftCategory);
			analyticsDataFactory.Setup(shiftCategory);

			analyticsDataFactory.Setup(new DimDayOff(-1, Guid.NewGuid(), "DayOff", _datasource, businessUnitId));
		}

		[Test]
		public void ShouldAddPreference()
		{
			commonSetup();
			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.AddPreference(new AnalyticsFactSchedulePreference
				{
					DateId = 1,
					IntervalId = 0,
					PersonId = 10,
					ScenarioId = 10,
					PreferenceTypeId = 1,
					ShiftCategoryId = 12,
					DayOffId = -1,
					PreferencesRequested = 1,
					PreferencesFulfilled = 1,
					PreferencesUnfulfilled = 0,
					BusinessUnitId = 1,
					DatasourceId = 1,
					InsertDate = DateTime.Now,
					UpdateDate = DateTime.Now,
					DatasourceUpdateDate = DateTime.Now,
					MustHaves = 0,
					AbsenceId = -1
				});
			});
		}

		[Test]
		public void ShouldGetPreferencesCorrectData()
		{
			commonSetup();
			var expectedPreference = new AnalyticsFactSchedulePreference
			{
				DateId = 1,
				IntervalId = 0,
				PersonId = 10,
				ScenarioId = 10,
				PreferenceTypeId = 1,
				ShiftCategoryId = 12,
				DayOffId = -1,
				PreferencesRequested = 1,
				PreferencesFulfilled = 1,
				PreferencesUnfulfilled = 0,
				BusinessUnitId = 1,
				DatasourceId = 1,
				DatasourceUpdateDate = new DateTime(2001, 1, 1, 14, 0, 0),
				MustHaves = 0,
				AbsenceId = -1
			};

			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.AddPreference(expectedPreference);
			});

			var preferences = WithAnalyticsUnitOfWork.Get(() => Target.PreferencesForPerson(10));
			preferences.Count.Should().Be.EqualTo(1);

			var preference = preferences.First();
			preference.DateId.Should().Be.EqualTo(expectedPreference.DateId);
			preference.IntervalId.Should().Be.EqualTo(expectedPreference.IntervalId);
			preference.PersonId.Should().Be.EqualTo(expectedPreference.PersonId);
			preference.ScenarioId.Should().Be.EqualTo(expectedPreference.ScenarioId);
			preference.PreferenceTypeId.Should().Be.EqualTo(expectedPreference.PreferenceTypeId);
			preference.ShiftCategoryId.Should().Be.EqualTo(expectedPreference.ShiftCategoryId);
			preference.DayOffId.Should().Be.EqualTo(expectedPreference.DayOffId);
			preference.PreferencesRequested.Should().Be.EqualTo(expectedPreference.PreferencesRequested);
			preference.PreferencesFulfilled.Should().Be.EqualTo(expectedPreference.PreferencesFulfilled);
			preference.PreferencesUnfulfilled.Should().Be.EqualTo(expectedPreference.PreferencesUnfulfilled);
			preference.BusinessUnitId.Should().Be.EqualTo(expectedPreference.BusinessUnitId);
			preference.DatasourceId.Should().Be.EqualTo(expectedPreference.DatasourceId);
			preference.DatasourceUpdateDate.Should().Be.EqualTo(expectedPreference.DatasourceUpdateDate);
			preference.MustHaves.Should().Be.EqualTo(expectedPreference.MustHaves);
			preference.AbsenceId.Should().Be.EqualTo(expectedPreference.AbsenceId);
		}

		[Test]
		public void ShouldAddAndDeletePreferences()
		{
			commonSetup();
			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.AddPreference(getTestPreference(1, 10));
				Target.AddPreference(getTestPreference(2, 10));
				Target.AddPreference(getTestPreference(3, 10));

				Target.AddPreference(getTestPreference(3, 20));
			});

			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.DeletePreferences(1, personCode1);
			});

			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.PreferencesForPerson(10).Count.Should().Be.EqualTo(2);
				Target.PreferencesForPerson(20).Count.Should().Be.EqualTo(1);
			});
		}

		private AnalyticsFactSchedulePreference getTestPreference(int dateId, int personId, int scenarioId = 10)
		{
			return new AnalyticsFactSchedulePreference
			{
				DateId = dateId,
				IntervalId = 0,
				PersonId = personId,
				ScenarioId = scenarioId,
				PreferenceTypeId = 1,
				ShiftCategoryId = 12,
				DayOffId = -1,
				PreferencesRequested = 1,
				PreferencesFulfilled = 1,
				PreferencesUnfulfilled = 0,
				BusinessUnitId = 1,
				DatasourceId = 1,
				DatasourceUpdateDate = new DateTime(2001, 1, 1, 14, 0, 0),
				MustHaves = 0,
				AbsenceId = -1
			};
		}

		private void insertPerson(int personPeriodId, DateTime validFrom, DateTime validTo, bool toBeDeleted = false, int validateFromDateId = 0, int validateToDateId = -2)
		{
			analyticsDataFactory.Setup(new Person(personPeriodId, Guid.NewGuid(), Guid.NewGuid(), "Ashley", "Andeen", validFrom,
				validTo, validateFromDateId, validateToDateId, businessUnitId, Guid.NewGuid(), _datasource, toBeDeleted,
				_timeZones.UtcTimeZoneId));
		}

		[Test]
		public void ShouldUpdateFactSchedulePreferenceWithUnlinkedPersonidsWhenAddNewPersonPeriod()
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
			analyticsDataFactory.Setup(specificDate1);
			analyticsDataFactory.Setup(specificDate2);

			const int personPeriod1 = 10;
			const int personPeriod2 = 11;
			insertPerson(personPeriod1, new DateTime(2015, 1, 10), new DateTime(2015, 1, 24), false, 10, 24);
			insertPerson(personPeriod2, new DateTime(2015, 1, 25), AnalyticsDate.Eternity.DateDate, false, 25, -2);
			setupForFactSchedulePreference();
			analyticsDataFactory.Persist();

			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.AddPreference(getTestPreference(specificDate1.DateId, personPeriod1));
				Target.AddPreference(getTestPreference(specificDate2.DateId, personPeriod1));
			});

			var rowCount = WithAnalyticsUnitOfWork.Get(() => Target.PreferencesForPerson(personPeriod1).Count);
			rowCount.Should().Be.EqualTo(2);
			rowCount = WithAnalyticsUnitOfWork.Get(() => Target.PreferencesForPerson(personPeriod2).Count);
			rowCount.Should().Be.EqualTo(0);
			WithAnalyticsUnitOfWork.Do(() => Target.UpdateUnlinkedPersonids(new[] { 10, 11 }));
			rowCount = WithAnalyticsUnitOfWork.Get(() => Target.PreferencesForPerson(personPeriod1).Count);
			rowCount.Should().Be.EqualTo(1);
			rowCount = WithAnalyticsUnitOfWork.Get(() => Target.PreferencesForPerson(personPeriod2).Count);
			rowCount.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldUpdateFactSchedulePreferenceWithUnlinkedPersonidsWhenDeleteExistingPersonPeriod()
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
			analyticsDataFactory.Setup(specificDate1);
			analyticsDataFactory.Setup(specificDate2);

			const int personPeriod1 = 10;
			const int personPeriod2 = 11;
			insertPerson(personPeriod2, new DateTime(2015, 1, 5), AnalyticsDate.Eternity.DateDate, false, 5, -2);
			insertPerson(personPeriod1, new DateTime(2015, 1, 10), AnalyticsDate.Eternity.DateDate, true, 10, -2);
			setupForFactSchedulePreference();
			analyticsDataFactory.Persist();

			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.AddPreference(getTestPreference(specificDate1.DateId, personPeriod1));
				Target.AddPreference(getTestPreference(specificDate2.DateId, personPeriod2));
			});

			var rowCount = WithAnalyticsUnitOfWork.Get(() => Target.PreferencesForPerson(personPeriod1).Count);
			rowCount.Should().Be.EqualTo(1);
			rowCount = WithAnalyticsUnitOfWork.Get(() => Target.PreferencesForPerson(personPeriod2).Count);
			rowCount.Should().Be.EqualTo(1);
			WithAnalyticsUnitOfWork.Do(() => Target.UpdateUnlinkedPersonids(new[] { personPeriod1, personPeriod2 }));
			rowCount = WithAnalyticsUnitOfWork.Get(() => Target.PreferencesForPerson(personPeriod1).Count);
			rowCount.Should().Be.EqualTo(0);
			rowCount = WithAnalyticsUnitOfWork.Get(() => Target.PreferencesForPerson(personPeriod2).Count);
			rowCount.Should().Be.EqualTo(2);
		}
	}
}