using System;
using System.Collections.Generic;
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
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Preference.Analytics
{
	[DomainTest]
	[NoDefaultData]
	public class AnalyticsPreferenceUpdaterTests : IExtendSystem
	{
		public AnalyticsPreferenceUpdater Target;
		public IAnalyticsPersonPeriodRepository PersonPeriodRepository;
		public IPreferenceDayRepository PreferenceDayRepository;
		public FakeScenarioRepository ScenarioRepository;
		public IScheduleStorage ScheduleStorage;
		public IShiftCategoryRepository ShiftCategoryRepository;
		public IAnalyticsPreferenceRepository AnalyticsPreferenceRepository;
		public FakeAnalyticsDateRepository AnalyticsDateRepository;
		public FakePersonRepository PersonRepository;
		public FakeAnalyticsDayOffRepository AnalyticsDayOffRepository;
		public FakeAnalyticsScenarioRepository AnalyticsScenarioRepository;
		public FakeAnalyticsAbsenceRepository AnalyticsAbsenceRepository;
		public FakeAnalyticsShiftCategoryRepository AnalyticsShiftCategoryRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public IPersonAssignmentRepository PersonAssignmentRepository;
		public IPersonAbsenceRepository PersonAbsenceRepository;

		private readonly Guid _businessUnitId = Guid.NewGuid();

		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<AnalyticsPreferenceUpdater>();
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
			setupValidPreferenceDay(date1, out var person, out var scenario);

			AnalyticsPreferenceRepository.AddPreference(new AnalyticsFactSchedulePreference
			{
				PersonId = 0,
				DateId = AnalyticsDateRepository.Date(date1).DateId,
				ScenarioId = 123 // Not matched
			});
			AnalyticsPreferenceRepository.PreferencesForPerson(0).Count.Should().Be.EqualTo(1);

			Target.Handle(new PreferenceChangedEvent
			{
				PersonId = person.Id.GetValueOrDefault(),
				RestrictionDates = new List<DateTime> {date1},
				ScenarioId = scenario.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = _businessUnitId
			});
			AnalyticsPreferenceRepository.PreferencesForPerson(0).Count.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldHandlePreferenceCreatedEventEvent()
		{
			setup();

			var date1 = new DateTime(2001, 1, 1);
			setupValidPreferenceDay(date1, out var person, out var scenario);

			var shiftCategory = new ShiftCategory("Late");
			ShiftCategoryRepository.Add(shiftCategory);

			PreferenceDayRepository.Add(new PreferenceDay(person, new DateOnly(date1), new PreferenceRestriction
			{
				ShiftCategory = shiftCategory
			}));

			Target.Handle(new PreferenceCreatedEvent
			{
				PersonId = person.Id.GetValueOrDefault(),
				RestrictionDates = new List<DateTime> { date1 },
				ScenarioId = scenario.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = _businessUnitId
			});
			AnalyticsPreferenceRepository.PreferencesForPerson(0).Count.Should().Be.EqualTo(1);
		}

		// Refer to bug #75976 (duplicates of preferenceday in app DB)
		[Test]
		public void ShouldHandleDuplicateExistingPreferenceDaysOnHandlingPreferenceCreatedEventEvent()
		{
			setup();

			var date1 = new DateTime(2001, 1, 1);
			setupValidPreferenceDay(date1, out var person, out var scenario);

			var shiftCategory = new ShiftCategory("Late");
			ShiftCategoryRepository.Add(shiftCategory);

			PreferenceDayRepository.Add(new PreferenceDay(person, new DateOnly(date1), new PreferenceRestriction
			{
				ShiftCategory = shiftCategory
			}));
			PreferenceDayRepository.Add(new PreferenceDay(person, new DateOnly(date1), new PreferenceRestriction
			{
				ShiftCategory = shiftCategory
			}));

			Target.Handle(new PreferenceCreatedEvent
			{
				PersonId = person.Id.GetValueOrDefault(),
				RestrictionDates = new List<DateTime> { date1 },
				ScenarioId = scenario.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = _businessUnitId
			});
			AnalyticsPreferenceRepository.PreferencesForPerson(0).Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldHandleEventForNotNewScenario()
		{
			const int dimPersonId = 0;
			setup();

			var date1 = new DateTime(2001, 1, 1);
			setupValidPreferenceDay(date1, out var person, out var scenario);
			((FakeAnalyticsPreferenceRepository)AnalyticsPreferenceRepository)
				.AddDimPerson(dimPersonId, person.Id.GetValueOrDefault());

			AnalyticsPreferenceRepository.AddPreference(new AnalyticsFactSchedulePreference
			{
				PersonId = dimPersonId,
				DateId = AnalyticsDateRepository.Date(date1).DateId,
				ScenarioId = 123 // Not matched
			});
			AnalyticsPreferenceRepository.AddPreference(new AnalyticsFactSchedulePreference
			{
				PersonId = dimPersonId,
				DateId = AnalyticsDateRepository.Date(date1).DateId,
				ScenarioId = 0 // Matched
			});
			AnalyticsPreferenceRepository.PreferencesForPerson(dimPersonId).Count.Should().Be.EqualTo(2);

			Target.Handle(new PreferenceChangedEvent
			{
				PersonId = person.Id.GetValueOrDefault(),
				RestrictionDates = new List<DateTime> {date1},
				ScenarioId = scenario.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = _businessUnitId
			});
			AnalyticsPreferenceRepository.PreferencesForPerson(0).Count.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldHandleDeletedPreferenceDay()
		{
			const int dimPersonId = 0;
			setup();
			var date = new DateTime(2001, 1, 1);
			var person = PersonFactory.CreatePersonWithGuid("firstName", "lastName");
			((FakeAnalyticsPreferenceRepository)AnalyticsPreferenceRepository)
				.AddDimPerson(dimPersonId, person.Id.GetValueOrDefault());

			var personPeriodCode = Guid.NewGuid();
			person.AddPersonPeriod(newTestPersonPeriod(date, personPeriodCode));
			PersonPeriodRepository.AddOrUpdatePersonPeriod(newTestAnalyticsPersonPeriod(person, personPeriodCode));
			PersonRepository.Add(person);
			AnalyticsPreferenceRepository.AddPreference(new AnalyticsFactSchedulePreference
			{
				PersonId = dimPersonId,
				DateId = AnalyticsDateRepository.Date(date).DateId
			});

			AnalyticsPreferenceRepository.PreferencesForPerson(dimPersonId).Count.Should().Be.EqualTo(1);

			Target.Handle(new PreferenceDeletedEvent
			{
				PersonId = person.Id.GetValueOrDefault(),
				RestrictionDates = new List<DateTime> {date},
				LogOnBusinessUnitId = _businessUnitId
			});
			AnalyticsPreferenceRepository.PreferencesForPerson(0).Should().Be.Empty();
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

			setupPreferenceDay(date, preferenceRestriction, out var person, out var scenario);

			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				scenario, ActivityFactory.CreateActivity("sdfsdf"), new DateTimePeriod(2001, 1, 1, 2002, 1, 1),
				preferenceRestriction.ShiftCategory);

			ScheduleStorage.Add(assignment);

			Target.Handle(new PreferenceChangedEvent
			{
				PersonId = person.Id.GetValueOrDefault(),
				RestrictionDates = new List<DateTime> {date},
				LogOnBusinessUnitId = _businessUnitId
			});
			var preference = AnalyticsPreferenceRepository.PreferencesForPerson(0).Single();
			preference.PreferencesFulfilled.Should().Be.EqualTo(1);
			preference.PreferencesUnfulfilled.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldHandleDeleteAndAddPreference()
		{
			setup();
			setupValidPreferenceDay(new DateTime(2001, 1, 1), out var person, out _);
			((FakeAnalyticsPreferenceRepository)AnalyticsPreferenceRepository)
				.AddDimPerson(0, person.Id.GetValueOrDefault());

			var analyticsDate = AnalyticsDateRepository.Date(new DateTime(2001, 1, 1));
			AnalyticsPreferenceRepository.AddPreference(new AnalyticsFactSchedulePreference
			{
				PersonId = 0,
				DateId = analyticsDate.DateId
			});
			analyticsDate = AnalyticsDateRepository.Date(new DateTime(2001, 1, 2));
			AnalyticsPreferenceRepository.AddPreference(new AnalyticsFactSchedulePreference
			{
				PersonId = 0,
				DateId = analyticsDate.DateId
			});
			AnalyticsPreferenceRepository.PreferencesForPerson(0).Count.Should().Be.EqualTo(2);

			Target.Handle(new PreferenceChangedEvent
			{
				PersonId = person.Id.GetValueOrDefault(),
				RestrictionDates = new List<DateTime> {new DateTime(2001, 1, 1)},
				LogOnBusinessUnitId = _businessUnitId
			});

			AnalyticsPreferenceRepository.PreferencesForPerson(0).Count.Should().Be.EqualTo(2);

			var preference = AnalyticsPreferenceRepository.PreferencesForPerson(0)
				.First(a => a.DateId == AnalyticsDateRepository.Date(new DateTime(2001, 1, 1)).DateId);
			preference.PreferencesFulfilled.Should().Be.EqualTo(0);
			preference.PreferencesUnfulfilled.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldHandleAndAddPreference()
		{
			setup();
			setupValidPreferenceDay(new DateTime(2001, 1, 1), out var person, out _);

			Target.Handle(new PreferenceChangedEvent
			{
				PersonId = person.Id.GetValueOrDefault(),
				RestrictionDates = new List<DateTime> {new DateTime(2001, 1, 1)},
				LogOnBusinessUnitId = _businessUnitId
			});
			var preference = AnalyticsPreferenceRepository.PreferencesForPerson(0).Single();
			preference.PreferencesFulfilled.Should().Be.EqualTo(0);
			preference.PreferencesUnfulfilled.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldSkipPreferenceDayWhenMissing()
		{
			setup();
			setupValidPreferenceDay(new DateTime(2001, 1, 2), out var person, out _);

			Target.Handle(new PreferenceChangedEvent
			{
				PersonId = person.Id.GetValueOrDefault(),
				RestrictionDates = new List<DateTime> { new DateTime(2001, 1, 1), new DateTime(2001, 1, 2) },
				LogOnBusinessUnitId = _businessUnitId
			});
			var preference = AnalyticsPreferenceRepository.PreferencesForPerson(0).Single();
			preference.PreferencesFulfilled.Should().Be.EqualTo(0);
			preference.PreferencesUnfulfilled.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldHandleSameEventTwicePreference()
		{
			const int dimPersonId = 0;

			setup();

			setupValidPreferenceDay(new DateTime(2001, 1, 1), out var person, out _);
			((FakeAnalyticsPreferenceRepository)AnalyticsPreferenceRepository)
				.AddDimPerson(dimPersonId, person.Id.GetValueOrDefault());

			Target.Handle(new PreferenceChangedEvent
			{
				PersonId = person.Id.GetValueOrDefault(),
				RestrictionDates = new List<DateTime> {new DateTime(2001, 1, 1)},
				LogOnBusinessUnitId = _businessUnitId
			});
			AnalyticsPreferenceRepository.PreferencesForPerson(0).Count.Should().Be.EqualTo(1);
			Target.Handle(new PreferenceChangedEvent
			{
				PersonId = person.Id.GetValueOrDefault(),
				RestrictionDates = new List<DateTime> {new DateTime(2001, 1, 1)},
				LogOnBusinessUnitId = _businessUnitId
			});

			var preference = AnalyticsPreferenceRepository.PreferencesForPerson(dimPersonId).Single();
			preference.PreferencesFulfilled.Should().Be.EqualTo(0);
			preference.PreferencesUnfulfilled.Should().Be.EqualTo(1);
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
			PersonPeriodRepository.AddOrUpdatePersonPeriod(newTestAnalyticsPersonPeriod(person, personPeriodCode));
			
			var scenario = ScenarioRepository.Has("Default");
			scenario.EnableReporting = true;
			AnalyticsScenarioRepository.AddScenario(new AnalyticsScenario
			{
				ScenarioCode = scenario.Id.GetValueOrDefault(),
				ScenarioId = 1
			});

			var preferenceDay = new PreferenceDay(person, new DateOnly(date), preferenceRestriction);
			preferenceDay.WithId();
			PreferenceDayRepository.Add(preferenceDay);

			Target.Handle(new PreferenceChangedEvent
			{
				PersonId = person.Id.GetValueOrDefault(),
				RestrictionDates = new List<DateTime> {new DateTime(2001, 1, 1)},
				LogOnBusinessUnitId = _businessUnitId
			});

			var preference = AnalyticsPreferenceRepository.PreferencesForPerson(0).Single();
			preference.AbsenceId.Should().Be.EqualTo(1);
			preference.DayOffId.Should().Be.EqualTo(1);
			preference.ShiftCategoryId.Should().Be.EqualTo(1);
			preference.DatasourceId.Should().Be.EqualTo(1);
			preference.DateId.Should().Be.EqualTo(AnalyticsDateRepository.Date(new DateTime(2001, 1, 1)).DateId);
			preference.ScenarioId.Should().Be.EqualTo(1);
			preference.BusinessUnitId.Should().Be.EqualTo(1);
			preference.IntervalId.Should().Be.EqualTo(0);
			preference.PersonId.Should().Be.EqualTo(0);
			preference.PreferenceTypeId.Should().Be.EqualTo(4);
			preference.MustHaves.Should().Be.EqualTo(preferenceRestriction.MustHave);
		}

		[Test]
		public void ShouldHandleEventWithInvalidRestriction()
		{
			setup();
			setupInvalidPreferenceDay(new DateTime(2001, 1, 1), out var person);

			Target.Handle(new PreferenceChangedEvent
			{
				PersonId = person.Id.GetValueOrDefault(),
				RestrictionDates = new List<DateTime> {new DateTime(2001, 1, 1)},
				LogOnBusinessUnitId = _businessUnitId
			});
			AnalyticsPreferenceRepository.PreferencesForPerson(0).Should().Be.Empty();
		}

		[Test]
		public void ShouldHandleMissingPersonPeriod()
		{
			setup();
			setupInvalidPreferenceDay(new DateTime(2001, 1, 2), out var person);
			person.TerminatePerson(new DateOnly(2001, 1, 1), new PersonAccountUpdaterDummy(),
				new ClearPersonRelatedInformation(PersonAssignmentRepository, ScenarioRepository, PersonAbsenceRepository));
			Target.Handle(new PreferenceChangedEvent
			{
				PersonId = person.Id.GetValueOrDefault(),
				RestrictionDates = new List<DateTime> { new DateTime(2001, 1, 2) },
				LogOnBusinessUnitId = _businessUnitId
			});
			AnalyticsPreferenceRepository.PreferencesForPerson(0).Should().Be.Empty();
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

		private void setupPreferenceDay(DateTime date, IPreferenceRestriction preferenceRestriction, out IPerson person, out IScenario scenario)
		{
			person = PersonFactory.CreatePersonWithGuid("firstName", "lastName");
			PersonRepository.Add(person);
			var personPeriodCode = Guid.NewGuid();
			person.AddPersonPeriod(newTestPersonPeriod(date, personPeriodCode));
			PersonPeriodRepository.AddOrUpdatePersonPeriod(newTestAnalyticsPersonPeriod(person, personPeriodCode));

			scenario = ScenarioRepository.Has("Default");
			scenario.EnableReporting = true;
			AnalyticsScenarioRepository.AddScenario(new AnalyticsScenario { ScenarioCode = scenario.Id.GetValueOrDefault() });

			var preferenceDay = new PreferenceDay(person, new DateOnly(date), preferenceRestriction);
			preferenceDay.WithId();
			PreferenceDayRepository.Add(preferenceDay);
		}

		private void setupValidPreferenceDay(DateTime date, out IPerson person, out IScenario scenario)
		{
			var preferenceRestrictionNew = new PreferenceRestriction
			{
				ShiftCategory = ShiftCategoryFactory.CreateShiftCategory("hej")
			};
			setupPreferenceDay(date, preferenceRestrictionNew, out person, out scenario);
		}

		private void setupInvalidPreferenceDay(DateTime date, out IPerson person)
		{
			setupPreferenceDay(date, new PreferenceRestriction(), out person, out _);
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
