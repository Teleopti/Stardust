using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Preference;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Preference.Analytics
{
	[TestFixture]
	public class PreferenceChangedHandlerTests
	{
		private PreferenceChangedHandler _target;
		private IAnalyticsPersonPeriodRepository _personPeriodRepository;
		private IAnalyticsBusinessUnitRepository _analyticsBusinessUnitRepository;
		private IPreferenceDayRepository _preferenceDayRepository;
		private IScenarioRepository _scenarioRepository;
		private IScheduleStorage _scheduleStorage;
		private IAnalyticsPreferenceRepository _analyticsPreferenceRepository;
		private IAnalyticsDateRepository _analyticsDateRepository;
		private FakeAnalyticsScheduleRepository _analyticsScheduleRepository;
		private FakePersonRepository _personRepository;
		private FakeAnalyticsDayOffRepository _analyticsDayOffRepository;
		private FakeAnalyticsScenarioRepository _analyticsScenarioRepository;

		[SetUp]
		public void Setup()
		{
			_personPeriodRepository = new FakeAnalyticsPersonPeriodRepository(
				new DateTime(2001, 01, 01),
				new DateTime(2002, 12, 31));

			_analyticsBusinessUnitRepository = new FakeAnalyticsBusinessUnitRepository();
			_preferenceDayRepository = new FakePreferenceDayRepository();
			_scenarioRepository = new FakeScenarioRepository();
			_scheduleStorage = new FakeScheduleDataReadScheduleStorage();
			_analyticsPreferenceRepository = new FakeAnalyticsPreferenceRepository();
			_analyticsDateRepository = new FakeAnalyticsDateRepository();
			_analyticsScheduleRepository = new FakeAnalyticsScheduleRepository();
			_personRepository = new FakePersonRepository();
			_analyticsDayOffRepository = new FakeAnalyticsDayOffRepository();
			_analyticsScenarioRepository = new FakeAnalyticsScenarioRepository();

			_target = new PreferenceChangedHandler(
				_scenarioRepository,
				_preferenceDayRepository,
				_personPeriodRepository,
				_analyticsBusinessUnitRepository,
				_analyticsScheduleRepository,
				_analyticsDateRepository,
				_scheduleStorage,
				_analyticsPreferenceRepository,
				_personRepository,
				_analyticsDayOffRepository,
				_analyticsScenarioRepository);
		}

		[Test]
		public void ShouldHandleEventForNewScenario()
		{
			var date1 = new DateTime(2001, 1, 1);
			IPerson person;
			IScenario scenario;
			var preferenceDay = setupValidPreferenceDay(date1, out person, out scenario);

			_analyticsPreferenceRepository.AddPreference(new AnalyticsFactSchedulePreference
			{
				PersonId = 0,
				DateId = _analyticsDateRepository.Date(date1).Value,
				ScenarioId = 123 // Not matched
			});
			_analyticsPreferenceRepository.PreferencesForPerson(0).Count.Should().Be.EqualTo(1);

			_target.Handle(new PreferenceChangedEvent
			{
				PreferenceDayId = preferenceDay.Id.GetValueOrDefault(),
				PersonId = person.Id.GetValueOrDefault(),
				RestrictionDate = date1,
				ScenarioId = scenario.Id.GetValueOrDefault()
			});
			_analyticsPreferenceRepository.PreferencesForPerson(0).Count.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldHandleEventForNotNewScenario()
		{
			var date1 = new DateTime(2001, 1, 1);
			IPerson person;
			IScenario scenario;
			var preferenceDay = setupValidPreferenceDay(date1, out person, out scenario);

			_analyticsPreferenceRepository.AddPreference(new AnalyticsFactSchedulePreference
			{
				PersonId = 0,
				DateId = _analyticsDateRepository.Date(date1).Value,
				ScenarioId = 123 // Not matched
			});
			_analyticsPreferenceRepository.AddPreference(new AnalyticsFactSchedulePreference
			{
				PersonId = 0,
				DateId = _analyticsDateRepository.Date(date1).Value,
				ScenarioId = 1 // Matched
			});
			_analyticsPreferenceRepository.PreferencesForPerson(0).Count.Should().Be.EqualTo(2);

			_target.Handle(new PreferenceChangedEvent
			{
				PreferenceDayId = preferenceDay.Id.GetValueOrDefault(),
				PersonId = person.Id.GetValueOrDefault(),
				RestrictionDate = date1,
				ScenarioId = scenario.Id.GetValueOrDefault()
			});
			_analyticsPreferenceRepository.PreferencesForPerson(0).Count.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldHandleDeletedPreferenceDay()
		{
			var date = new DateTime(2001, 1, 1);
			var person = PersonFactory.CreatePersonWithGuid("firstName", "lastName");
			var personPeriodCode = Guid.NewGuid();
			person.AddPersonPeriod(newTestPersonPeriod(date, personPeriodCode));
			_personPeriodRepository.AddPersonPeriod(newTestAnalyticsPersonPeriod(person, personPeriodCode));
			_personRepository.Add(person);
			_analyticsPreferenceRepository.AddPreference(new AnalyticsFactSchedulePreference
			{
				PersonId = 0,
				DateId = _analyticsDateRepository.Date(date).Value
			});

			_analyticsPreferenceRepository.PreferencesForPerson(0).Count.Should().Be.EqualTo(1);

			_target.Handle(new PreferenceDeletedEvent
			{
				PreferenceDayId = Guid.NewGuid(),
				PersonId = person.Id.GetValueOrDefault(),
				RestrictionDate = date
			});
			_analyticsPreferenceRepository.PreferencesForPerson(0).Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldHandleAndHaveFulfillment()
		{
			var date = new DateTime(2001, 1, 1);
			var preferenceRestriction = new PreferenceRestriction
			{
				ShiftCategory = ShiftCategoryFactory.CreateShiftCategory("hej")
			};
			IPerson person;
			IScenario scenario;
			var preferenceDay = setupPreferenceDay(date, preferenceRestriction, out person, out scenario);

			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				ActivityFactory.CreateActivity("sdfsdf"),
				person,
				new DateTimePeriod(2001, 1, 1, 2002, 1, 1),
				preferenceRestriction.ShiftCategory,
				scenario);

			_scheduleStorage.Add(assignment);

			_target.Handle(new PreferenceChangedEvent
			{
				PreferenceDayId = preferenceDay.Id.GetValueOrDefault(),
				PersonId = person.Id.GetValueOrDefault(),
				RestrictionDate = date
			});
			_analyticsPreferenceRepository.PreferencesForPerson(0).Count.Should().Be.EqualTo(1);
			_analyticsPreferenceRepository.PreferencesForPerson(0).First().PreferencesFulfilled.Should().Be.EqualTo(1);
			_analyticsPreferenceRepository.PreferencesForPerson(0).First().PreferencesUnfulfilled.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldHandleDeleteAndAddPreference()
		{
			IPerson person;
			IScenario scenario;
			var preferenceDay = setupValidPreferenceDay(new DateTime(2001, 1, 1), out person, out scenario);

			_analyticsPreferenceRepository.AddPreference(new AnalyticsFactSchedulePreference
			{
				PersonId = 0,
				DateId = _analyticsDateRepository.Date(new DateTime(2001, 1, 1)).Value
			});
			_analyticsPreferenceRepository.AddPreference(new AnalyticsFactSchedulePreference
			{
				PersonId = 0,
				DateId = _analyticsDateRepository.Date(new DateTime(2001, 1, 2)).Value
			});
			_analyticsPreferenceRepository.PreferencesForPerson(0).Count.Should().Be.EqualTo(2);

			_target.Handle(new PreferenceChangedEvent
			{
				PreferenceDayId = preferenceDay.Id.GetValueOrDefault(),
				PersonId = person.Id.GetValueOrDefault(),
				RestrictionDate = new DateTime(2001, 1, 1)
			});
			_analyticsPreferenceRepository.PreferencesForPerson(0).Count.Should().Be.EqualTo(2);
			_analyticsPreferenceRepository.PreferencesForPerson(0).First(a => a.DateId == _analyticsDateRepository.Date(new DateTime(2001, 1, 1)).Value).PreferencesFulfilled.Should().Be.EqualTo(0);
			_analyticsPreferenceRepository.PreferencesForPerson(0).First(a => a.DateId == _analyticsDateRepository.Date(new DateTime(2001, 1, 1)).Value).PreferencesUnfulfilled.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldHandleAndAddPreference()
		{
			IPerson person;
			IScenario scenario;
			var preferenceDay = setupValidPreferenceDay(new DateTime(2001, 1, 1), out person, out scenario);

			_target.Handle(new PreferenceChangedEvent
			{
				PreferenceDayId = preferenceDay.Id.GetValueOrDefault(),
				PersonId = person.Id.GetValueOrDefault(),
				RestrictionDate = new DateTime(2001, 1, 1)
			});
			_analyticsPreferenceRepository.PreferencesForPerson(0).Count.Should().Be.EqualTo(1);
			_analyticsPreferenceRepository.PreferencesForPerson(0).First().PreferencesFulfilled.Should().Be.EqualTo(0);
			_analyticsPreferenceRepository.PreferencesForPerson(0).First().PreferencesUnfulfilled.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldHandleSameEventTwicePreference()
		{
			IPerson person;
			IScenario scenario;
			var preferenceDay = setupValidPreferenceDay(new DateTime(2001, 1, 1), out person, out scenario);

			_target.Handle(new PreferenceChangedEvent
			{
				PreferenceDayId = preferenceDay.Id.GetValueOrDefault(),
				PersonId = person.Id.GetValueOrDefault(),
				RestrictionDate = new DateTime(2001, 1, 1)
			});
			_analyticsPreferenceRepository.PreferencesForPerson(0).Count.Should().Be.EqualTo(1);
			_target.Handle(new PreferenceChangedEvent
			{
				PreferenceDayId = preferenceDay.Id.GetValueOrDefault(),
				PersonId = person.Id.GetValueOrDefault(),
				RestrictionDate = new DateTime(2001, 1, 1)
			});
			_analyticsPreferenceRepository.PreferencesForPerson(0).Count.Should().Be.EqualTo(1);
			_analyticsPreferenceRepository.PreferencesForPerson(0).First().PreferencesFulfilled.Should().Be.EqualTo(0);
			_analyticsPreferenceRepository.PreferencesForPerson(0).First().PreferencesUnfulfilled.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldHandleAndMapEverything()
		{
			var date = new DateTime(2001, 1, 1);
			var preferenceRestriction = new PreferenceRestriction
			{
				ShiftCategory = ShiftCategoryFactory.CreateShiftCategory("hej").WithId(_analyticsScheduleRepository.ShiftCategories().First(a => a.Id == 1).Code),
				Absence = AbsenceFactory.CreateAbsence("Absence").WithId(_analyticsScheduleRepository.Absences().First(a => a.AbsenceId == 1).AbsenceCode),
				DayOffTemplate = DayOffFactory.CreateDayOff().WithId(),
				MustHave = true
			};

			_analyticsDayOffRepository.AddOrUpdate(new AnalyticsDayOff { DayOffName = preferenceRestriction.DayOffTemplate.Description.Name,
				DayOffCode = preferenceRestriction.DayOffTemplate.Id.GetValueOrDefault(), BusinessUnitId = 1});

			var person = PersonFactory.CreatePersonWithGuid("firstName", "lastName");
			_personRepository.Add(person);
			var personPeriodCode = Guid.NewGuid();
			person.AddPersonPeriod(newTestPersonPeriod(date, personPeriodCode));
			_personPeriodRepository.AddPersonPeriod(newTestAnalyticsPersonPeriod(person, personPeriodCode));

			var scenario = new FakeCurrentScenario().Current();
			scenario.DefaultScenario = true;
			scenario.EnableReporting = true;
			_scenarioRepository.Add(scenario);
			_analyticsScenarioRepository.AddScenario(new AnalyticsScenario { ScenarioCode = scenario.Id.GetValueOrDefault()});

			var preferenceDay = new PreferenceDay(person, new DateOnly(date), preferenceRestriction);
			preferenceDay.WithId();
			_scheduleStorage.Add(preferenceDay);
			_preferenceDayRepository.Add(preferenceDay);

			_target.Handle(new PreferenceChangedEvent
			{
				PreferenceDayId = preferenceDay.Id.GetValueOrDefault(),
				PersonId = person.Id.GetValueOrDefault(),
				RestrictionDate = new DateTime(2001, 1, 1)
			});

			_analyticsPreferenceRepository.PreferencesForPerson(0).Count.Should().Be.EqualTo(1);
			var item = _analyticsPreferenceRepository.PreferencesForPerson(0).First();
			item.AbsenceId.Should().Be.EqualTo(1);
			item.DayOffId.Should().Be.EqualTo(1);
			item.ShiftCategoryId.Should().Be.EqualTo(1);
			item.DatasourceId.Should().Be.EqualTo(1);
			item.DateId.Should().Be.EqualTo(_analyticsDateRepository.Date(new DateTime(2001, 1, 1)).Value);
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
			IPerson person;
			var preferenceDay = setupInvalidPreferenceDay(new DateTime(2001, 1, 1), out person);

			_target.Handle(new PreferenceChangedEvent
			{
				PreferenceDayId = preferenceDay.Id.GetValueOrDefault(),
				PersonId = person.Id.GetValueOrDefault(),
				RestrictionDate = new DateTime(2001, 1, 1)
			});
			_analyticsPreferenceRepository.PreferencesForPerson(0).Count.Should().Be.EqualTo(0);
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
			_personRepository.Add(person);
			var personPeriodCode = Guid.NewGuid();
			person.AddPersonPeriod(newTestPersonPeriod(date, personPeriodCode));
			_personPeriodRepository.AddPersonPeriod(newTestAnalyticsPersonPeriod(person, personPeriodCode));

			scenario = new FakeCurrentScenario().Current();
			scenario.DefaultScenario = true;
			scenario.EnableReporting = true;
			_scenarioRepository.Add(scenario);
			_analyticsScenarioRepository.AddScenario(new AnalyticsScenario { ScenarioCode = scenario.Id.GetValueOrDefault() });

			var preferenceDay = new PreferenceDay(person, new DateOnly(date), preferenceRestriction);
			preferenceDay.WithId();
			_scheduleStorage.Add(preferenceDay);
			_preferenceDayRepository.Add(preferenceDay);
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
				EmploymentEndDate = new DateTime(2059, 12, 31),
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
