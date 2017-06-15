using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Preference;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Preference.Analytics
{
	[DomainTest]
	public class AnalyticsPreferenceUpdaterTests : ISetup
	{
		public AnalyticsPreferenceUpdater Target;
		public IAnalyticsPersonPeriodRepository PersonPeriodRepository;
		public IPreferenceDayRepository PreferenceDayRepository;
		public IScenarioRepository ScenarioRepository;
		public IScheduleStorage ScheduleStorage;
		public IAnalyticsPreferenceRepository AnalyticsPreferenceRepository;
		public FakeAnalyticsDateRepository AnalyticsDateRepository;
		public FakePersonRepository PersonRepository;
		public FakeAnalyticsDayOffRepository AnalyticsDayOffRepository;
		public FakeAnalyticsScenarioRepository AnalyticsScenarioRepository;
		public FakeAnalyticsAbsenceRepository AnalyticsAbsenceRepository;
		public FakeAnalyticsShiftCategoryRepository AnalyticsShiftCategoryRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;

		private readonly Guid _businessUnitId = Guid.NewGuid();

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<AnalyticsPreferenceUpdater>();
		}

		private void setup()
		{
			AnalyticsDateRepository.HasDatesBetween(new DateTime(2001, 01, 01), new DateTime(2002, 12, 31));
			AnalyticsAbsenceRepository.AddAbsence(new AnalyticsAbsence { AbsenceCode = Guid.Empty, AbsenceId = -1 });
			AnalyticsAbsenceRepository.AddAbsence(new AnalyticsAbsence { AbsenceCode = Guid.NewGuid(), AbsenceId = 1 });
			AnalyticsShiftCategoryRepository.AddShiftCategory(new AnalyticsShiftCategory { ShiftCategoryCode = Guid.Empty, ShiftCategoryId = -1 });
			AnalyticsShiftCategoryRepository.AddShiftCategory(new AnalyticsShiftCategory { ShiftCategoryCode = Guid.NewGuid(), ShiftCategoryId = 1 });
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(_businessUnitId));
		}

		[Test]
		public void ShouldHandleEventForNewScenario()
		{
			setup();

			var date1 = new DateTime(2001, 1, 1);
			IPerson person;
			IScenario scenario;
			var preferenceDay = setupValidPreferenceDay(date1, out person, out scenario);

			AnalyticsPreferenceRepository.AddPreference(new AnalyticsFactSchedulePreference
			{
				PersonId = 0,
				DateId = AnalyticsDateRepository.Date(date1).DateId,
				ScenarioId = 123 // Not matched
			});
			AnalyticsPreferenceRepository.PreferencesForPerson(0).Count.Should().Be.EqualTo(1);

			Target.Handle(new PreferenceChangedEvent
			{
				PreferenceDayId = preferenceDay.Id.GetValueOrDefault(),
				PersonId = person.Id.GetValueOrDefault(),
				RestrictionDate = date1,
				ScenarioId = scenario.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = _businessUnitId
			});
			AnalyticsPreferenceRepository.PreferencesForPerson(0).Count.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldHandleEventForNotNewScenario()
		{
			setup();

			var date1 = new DateTime(2001, 1, 1);
			IPerson person;
			IScenario scenario;
			var preferenceDay = setupValidPreferenceDay(date1, out person, out scenario);

			AnalyticsPreferenceRepository.AddPreference(new AnalyticsFactSchedulePreference
			{
				PersonId = 0,
				DateId = AnalyticsDateRepository.Date(date1).DateId,
				ScenarioId = 123 // Not matched
			});
			AnalyticsPreferenceRepository.AddPreference(new AnalyticsFactSchedulePreference
			{
				PersonId = 0,
				DateId = AnalyticsDateRepository.Date(date1).DateId,
				ScenarioId = 0 // Matched
			});
			AnalyticsPreferenceRepository.PreferencesForPerson(0).Count.Should().Be.EqualTo(2);

			Target.Handle(new PreferenceChangedEvent
			{
				PreferenceDayId = preferenceDay.Id.GetValueOrDefault(),
				PersonId = person.Id.GetValueOrDefault(),
				RestrictionDate = date1,
				ScenarioId = scenario.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = _businessUnitId
			});
			AnalyticsPreferenceRepository.PreferencesForPerson(0).Count.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldHandleDeletedPreferenceDay()
		{
			setup();

			var date = new DateTime(2001, 1, 1);
			var person = PersonFactory.CreatePersonWithGuid("firstName", "lastName");
			var personPeriodCode = Guid.NewGuid();
			person.AddPersonPeriod(newTestPersonPeriod(date, personPeriodCode));
			PersonPeriodRepository.AddPersonPeriod(newTestAnalyticsPersonPeriod(person, personPeriodCode));
			PersonRepository.Add(person);
			AnalyticsPreferenceRepository.AddPreference(new AnalyticsFactSchedulePreference
			{
				PersonId = 0,
				DateId = AnalyticsDateRepository.Date(date).DateId
			});

			AnalyticsPreferenceRepository.PreferencesForPerson(0).Count.Should().Be.EqualTo(1);

			Target.Handle(new PreferenceDeletedEvent
			{
				PreferenceDayId = Guid.NewGuid(),
				PersonId = person.Id.GetValueOrDefault(),
				RestrictionDate = date,
				LogOnBusinessUnitId = _businessUnitId
			});
			AnalyticsPreferenceRepository.PreferencesForPerson(0).Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldHandleAndHaveFulfillment()
		{
			setup();

			var date = new DateTime(2001, 1, 1);
			var preferenceRestriction = new PreferenceRestriction
			{
				ShiftCategory = ShiftCategoryFactory.CreateShiftCategory("hej")
			};
			IPerson person;
			IScenario scenario;
			var preferenceDay = setupPreferenceDay(date, preferenceRestriction, out person, out scenario);

			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				scenario, ActivityFactory.CreateActivity("sdfsdf"), new DateTimePeriod(2001, 1, 1, 2002, 1, 1), preferenceRestriction.ShiftCategory);

			ScheduleStorage.Add(assignment);

			Target.Handle(new PreferenceChangedEvent
			{
				PreferenceDayId = preferenceDay.Id.GetValueOrDefault(),
				PersonId = person.Id.GetValueOrDefault(),
				RestrictionDate = date,
				LogOnBusinessUnitId = _businessUnitId
			});
			AnalyticsPreferenceRepository.PreferencesForPerson(0).Count.Should().Be.EqualTo(1);
			AnalyticsPreferenceRepository.PreferencesForPerson(0).First().PreferencesFulfilled.Should().Be.EqualTo(1);
			AnalyticsPreferenceRepository.PreferencesForPerson(0).First().PreferencesUnfulfilled.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldHandleDeleteAndAddPreference()
		{
			setup();

			IPerson person;
			IScenario scenario;
			var preferenceDay = setupValidPreferenceDay(new DateTime(2001, 1, 1), out person, out scenario);

			AnalyticsPreferenceRepository.AddPreference(new AnalyticsFactSchedulePreference
			{
				PersonId = 0,
				DateId = AnalyticsDateRepository.Date(new DateTime(2001, 1, 1)).DateId
			});
			AnalyticsPreferenceRepository.AddPreference(new AnalyticsFactSchedulePreference
			{
				PersonId = 0,
				DateId = AnalyticsDateRepository.Date(new DateTime(2001, 1, 2)).DateId
			});
			AnalyticsPreferenceRepository.PreferencesForPerson(0).Count.Should().Be.EqualTo(2);

			Target.Handle(new PreferenceChangedEvent
			{
				PreferenceDayId = preferenceDay.Id.GetValueOrDefault(),
				PersonId = person.Id.GetValueOrDefault(),
				RestrictionDate = new DateTime(2001, 1, 1),
				LogOnBusinessUnitId = _businessUnitId
			});
			AnalyticsPreferenceRepository.PreferencesForPerson(0).Count.Should().Be.EqualTo(2);
			AnalyticsPreferenceRepository.PreferencesForPerson(0).First(a => a.DateId == AnalyticsDateRepository.Date(new DateTime(2001, 1, 1)).DateId).PreferencesFulfilled.Should().Be.EqualTo(0);
			AnalyticsPreferenceRepository.PreferencesForPerson(0).First(a => a.DateId == AnalyticsDateRepository.Date(new DateTime(2001, 1, 1)).DateId).PreferencesUnfulfilled.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldHandleAndAddPreference()
		{
			setup();

			IPerson person;
			IScenario scenario;
			var preferenceDay = setupValidPreferenceDay(new DateTime(2001, 1, 1), out person, out scenario);

			Target.Handle(new PreferenceChangedEvent
			{
				PreferenceDayId = preferenceDay.Id.GetValueOrDefault(),
				PersonId = person.Id.GetValueOrDefault(),
				RestrictionDate = new DateTime(2001, 1, 1),
				LogOnBusinessUnitId = _businessUnitId
			});
			AnalyticsPreferenceRepository.PreferencesForPerson(0).Count.Should().Be.EqualTo(1);
			AnalyticsPreferenceRepository.PreferencesForPerson(0).First().PreferencesFulfilled.Should().Be.EqualTo(0);
			AnalyticsPreferenceRepository.PreferencesForPerson(0).First().PreferencesUnfulfilled.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldHandleSameEventTwicePreference()
		{
			setup();

			IPerson person;
			IScenario scenario;
			var preferenceDay = setupValidPreferenceDay(new DateTime(2001, 1, 1), out person, out scenario);

			Target.Handle(new PreferenceChangedEvent
			{
				PreferenceDayId = preferenceDay.Id.GetValueOrDefault(),
				PersonId = person.Id.GetValueOrDefault(),
				RestrictionDate = new DateTime(2001, 1, 1),
				LogOnBusinessUnitId = _businessUnitId
			});
			AnalyticsPreferenceRepository.PreferencesForPerson(0).Count.Should().Be.EqualTo(1);
			Target.Handle(new PreferenceChangedEvent
			{
				PreferenceDayId = preferenceDay.Id.GetValueOrDefault(),
				PersonId = person.Id.GetValueOrDefault(),
				RestrictionDate = new DateTime(2001, 1, 1),
				LogOnBusinessUnitId = _businessUnitId
			});
			AnalyticsPreferenceRepository.PreferencesForPerson(0).Count.Should().Be.EqualTo(1);
			AnalyticsPreferenceRepository.PreferencesForPerson(0).First().PreferencesFulfilled.Should().Be.EqualTo(0);
			AnalyticsPreferenceRepository.PreferencesForPerson(0).First().PreferencesUnfulfilled.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldHandleAndMapEverything()
		{
			setup();

			var date = new DateTime(2001, 1, 1);
			var preferenceRestriction = new PreferenceRestriction
			{
				ShiftCategory = ShiftCategoryFactory.CreateShiftCategory("hej").WithId(AnalyticsShiftCategoryRepository.ShiftCategories().First(a => a.ShiftCategoryId == 1).ShiftCategoryCode),
				Absence = AbsenceFactory.CreateAbsence("Absence").WithId(AnalyticsAbsenceRepository.Absences().First(a => a.AbsenceId == 1).AbsenceCode),
				DayOffTemplate = DayOffFactory.CreateDayOff().WithId(),
				MustHave = true
			};

			AnalyticsDayOffRepository.AddOrUpdate(new AnalyticsDayOff { DayOffName = preferenceRestriction.DayOffTemplate.Description.Name,
				DayOffCode = preferenceRestriction.DayOffTemplate.Id.GetValueOrDefault(), BusinessUnitId = 1});

			var person = PersonFactory.CreatePersonWithGuid("firstName", "lastName");
			PersonRepository.Add(person);
			var personPeriodCode = Guid.NewGuid();
			person.AddPersonPeriod(newTestPersonPeriod(date, personPeriodCode));
			PersonPeriodRepository.AddPersonPeriod(newTestAnalyticsPersonPeriod(person, personPeriodCode));

			var scenario = new FakeCurrentScenario().Current();
			scenario.DefaultScenario = true;
			scenario.EnableReporting = true;
			ScenarioRepository.Add(scenario);
			AnalyticsScenarioRepository.AddScenario(new AnalyticsScenario { ScenarioCode = scenario.Id.GetValueOrDefault(), ScenarioId = 1});

			var preferenceDay = new PreferenceDay(person, new DateOnly(date), preferenceRestriction);
			preferenceDay.WithId();
			ScheduleStorage.Add(preferenceDay);
			PreferenceDayRepository.Add(preferenceDay);

			Target.Handle(new PreferenceChangedEvent
			{
				PreferenceDayId = preferenceDay.Id.GetValueOrDefault(),
				PersonId = person.Id.GetValueOrDefault(),
				RestrictionDate = new DateTime(2001, 1, 1),
				LogOnBusinessUnitId = _businessUnitId
			});

			AnalyticsPreferenceRepository.PreferencesForPerson(0).Count.Should().Be.EqualTo(1);
			var item = AnalyticsPreferenceRepository.PreferencesForPerson(0).First();
			item.AbsenceId.Should().Be.EqualTo(1);
			item.DayOffId.Should().Be.EqualTo(1);
			item.ShiftCategoryId.Should().Be.EqualTo(1);
			item.DatasourceId.Should().Be.EqualTo(1);
			item.DateId.Should().Be.EqualTo(AnalyticsDateRepository.Date(new DateTime(2001, 1, 1)).DateId);
			item.ScenarioId.Should().Be.EqualTo(1);
			item.BusinessUnitId.Should().Be.EqualTo(1);
			item.IntervalId.Should().Be.EqualTo(0);
			item.PersonId.Should().Be.EqualTo(0);
			item.PreferenceTypeId.Should().Be.EqualTo(4);
			item.MustHaves.Should().Be.EqualTo(preferenceRestriction.MustHave);
		}

		[Test]
		public void ShouldHandleEventWithInvalidRestriction()
		{
			setup();

			IPerson person;
			var preferenceDay = setupInvalidPreferenceDay(new DateTime(2001, 1, 1), out person);

			Target.Handle(new PreferenceChangedEvent
			{
				PreferenceDayId = preferenceDay.Id.GetValueOrDefault(),
				PersonId = person.Id.GetValueOrDefault(),
				RestrictionDate = new DateTime(2001, 1, 1),
				LogOnBusinessUnitId = _businessUnitId
			});
			AnalyticsPreferenceRepository.PreferencesForPerson(0).Count.Should().Be.EqualTo(0);
		}

		private static PersonPeriod newTestPersonPeriod(DateTime startDate, Guid? id = null)
		{
			var personPeriod = new PersonPeriod(new DateOnly(startDate),
				new PersonContract(new Contract("ContractNameTest"),
					new PartTimePercentage("100%"),
					new ContractSchedule("ScheduleName")),
				new Team
				{
					Site = new Site("SiteName")
				});
			personPeriod.SetId(id ?? Guid.NewGuid());
			return personPeriod;
		}

		private PreferenceDay setupPreferenceDay(DateTime date, IPreferenceRestriction preferenceRestriction, out IPerson person, out IScenario scenario)
		{
			person = PersonFactory.CreatePersonWithGuid("firstName", "lastName");
			PersonRepository.Add(person);
			var personPeriodCode = Guid.NewGuid();
			person.AddPersonPeriod(newTestPersonPeriod(date, personPeriodCode));
			PersonPeriodRepository.AddPersonPeriod(newTestAnalyticsPersonPeriod(person, personPeriodCode));

			scenario = new FakeCurrentScenario().Current();
			scenario.DefaultScenario = true;
			scenario.EnableReporting = true;
			ScenarioRepository.Add(scenario);
			AnalyticsScenarioRepository.AddScenario(new AnalyticsScenario { ScenarioCode = scenario.Id.GetValueOrDefault() });

			var preferenceDay = new PreferenceDay(person, new DateOnly(date), preferenceRestriction);
			preferenceDay.WithId();
			ScheduleStorage.Add(preferenceDay);
			PreferenceDayRepository.Add(preferenceDay);
			return preferenceDay;
		}

		private PreferenceDay setupValidPreferenceDay(DateTime date, out IPerson person, out IScenario scenario)
		{
			var preferenceRestrictionNew = new PreferenceRestriction
			{
				ShiftCategory = ShiftCategoryFactory.CreateShiftCategory("hej")
			};
			return setupPreferenceDay(date, preferenceRestrictionNew, out person, out scenario);
		}

		private PreferenceDay setupInvalidPreferenceDay(DateTime date, out IPerson person)
		{
			IScenario scenario;
			return setupPreferenceDay(date, new PreferenceRestriction(), out person, out scenario);
		}

		private static AnalyticsPersonPeriod newTestAnalyticsPersonPeriod(IPerson person, Guid personPeriodCode)
		{
			return new AnalyticsPersonPeriod
			{
				PersonPeriodCode = personPeriodCode,
				ValidFromDate = new DateTime(2000, 1, 1),
				ValidToDate = new DateTime(2001, 1, 1),
				BusinessUnitCode = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault(),
				BusinessUnitName = BusinessUnitFactory.BusinessUnitUsedInTest.Name,
				BusinessUnitId = 1,
				ContractCode = Guid.NewGuid(),
				ContractName = "Test contract",
				DatasourceId = 1,
				DatasourceUpdateDate = DateTime.Now,
				Email = person.Email,
				EmploymentStartDate = new DateTime(2000, 1, 1),
				EmploymentEndDate = AnalyticsDate.Eternity.DateDate,
				EmploymentNumber = "",
				EmploymentTypeCode = 0,
				EmploymentTypeName = "",
				FirstName = person.Name.FirstName,
				LastName = person.Name.LastName,
				IsAgent = true,
				IsUser = false,
				Note = person.Note,
				PersonCode = person.Id.GetValueOrDefault(),
				PersonName = person.Name.ToString(),
				ToBeDeleted = false,
				TeamId = 1,
				TeamCode = Guid.NewGuid(),
				TeamName = "team",
				SiteId = 1,
				SiteCode = Guid.NewGuid(),
				SiteName = "site",
				SkillsetId = null,
				WindowsDomain = "domain\\user",
				WindowsUsername = "user",
				ValidToDateIdMaxDate = 1,
				ValidToIntervalIdMaxDate = 1,
				ValidFromDateIdLocal = 1,
				ValidToDateIdLocal = 1,
				ValidFromDateLocal = DateTime.Now,
				ValidToDateLocal = DateTime.Now.AddYears(1),
				ValidToIntervalId = 1,
				ParttimeCode = Guid.NewGuid(),
				ParttimePercentage = "100%",
				TimeZoneId = 1,
				ValidFromDateId = 1,
				ValidFromIntervalId = 1,
				ValidToDateId = 1
			};
		}
	}
}
