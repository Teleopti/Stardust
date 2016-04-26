using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Person = Teleopti.Ccc.TestCommon.TestData.Analytics.Person;
using Scenario = Teleopti.Ccc.TestCommon.TestData.Analytics.Scenario;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[TestFixture]
	[Category("LongRunning")]
	[AnalyticsDatabaseTest]
	public class AnalyticsPreferenceRepositoryTest
	{
		public IAnalyticsPreferenceRepository Target;
		public WithAnalyticsUnitOfWork WithAnalyticsUnitOfWork;
		private UtcAndCetTimeZones _timeZones;
		private ExistingDatasources _datasource;
		private const int businessUnitId = 12;

		[SetUp]
		public void Setup()
		{
			var analyticsDataFactory = new AnalyticsDataFactory();
			_timeZones = new UtcAndCetTimeZones();
			_datasource = new ExistingDatasources(_timeZones);

			analyticsDataFactory.Setup(new Person(10, Guid.NewGuid(), Guid.NewGuid(), "Ashley", "Andeen", new DateTime(2010, 1, 1),
							new DateTime(2059, 12, 31), 0, -2, businessUnitId, Guid.NewGuid(), _datasource, false, _timeZones.UtcTimeZoneId));

			analyticsDataFactory.Setup(new Person(20, Guid.NewGuid(), Guid.NewGuid(), "Teleopti", "Demo", new DateTime(2010, 1, 1),
							new DateTime(2059, 12, 31), 0, -2, businessUnitId, Guid.NewGuid(), _datasource, false, _timeZones.UtcTimeZoneId));

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

			analyticsDataFactory.Persist();
		}

		[Test]
		public void ShouldAddPreference()
		{
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
			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.AddPreference(getTestPreference(1, 10));
				Target.AddPreference(getTestPreference(2, 10));
				Target.AddPreference(getTestPreference(3, 10));

				Target.AddPreference(getTestPreference(3, 20));
			});

			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.DeletePreferences(1, 10);
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
	}
}