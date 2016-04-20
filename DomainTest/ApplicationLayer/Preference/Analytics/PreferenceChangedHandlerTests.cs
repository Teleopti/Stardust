using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.Preference;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;
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

		[SetUp]
		public void Setup()
		{
			_personPeriodRepository = new FakeAnalyticsPersonPeriodRepository(
				new DateTime(2001, 01, 01),
				new DateTime(2017, 12, 31));

			_analyticsBusinessUnitRepository = new FakeAnalyticsBusinessUnitRepository();
			_preferenceDayRepository = new FakePreferenceDayRepository();
			_scenarioRepository = new FakeScenarioRepository();
			_scheduleStorage = new FakeScheduleDataReadScheduleStorage();
			_analyticsPreferenceRepository = new FakeAnalyticsPreferenceRepository();
			_analyticsDateRepository = new FakeAnalyticsDateRepository();
			_analyticsScheduleRepository = new FakeAnalyticsScheduleRepository();

			_target = new PreferenceChangedHandler(
				_scenarioRepository,
				_preferenceDayRepository,
				_personPeriodRepository,
				_analyticsBusinessUnitRepository,
				_analyticsScheduleRepository,
				_analyticsDateRepository,
				_scheduleStorage,
				_analyticsPreferenceRepository);
		}

		[Test]
		public void ShouldHandleDeleteAndAddPreference()
		{
			var preferenceDay = setupPreferenceDay(new DateTime(2001, 1, 1));

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
				PreferenceDayId = preferenceDay.Id.GetValueOrDefault()
			});
			_analyticsPreferenceRepository.PreferencesForPerson(0).Count.Should().Be.EqualTo(2);
			_analyticsPreferenceRepository.PreferencesForPerson(0).First(a => a.DateId == _analyticsDateRepository.Date(new DateTime(2001, 1, 1)).Value).PreferencesFulfilled.Should().Be.EqualTo(0);
			_analyticsPreferenceRepository.PreferencesForPerson(0).First(a => a.DateId == _analyticsDateRepository.Date(new DateTime(2001, 1, 1)).Value).PreferencesUnfulfilled.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldHandleAndAddPreference()
		{
			var preferenceDay = setupPreferenceDay(new DateTime(2001, 1, 1));

			_target.Handle(new PreferenceChangedEvent
			{
				PreferenceDayId = preferenceDay.Id.GetValueOrDefault()
			});
			_analyticsPreferenceRepository.PreferencesForPerson(0).Count.Should().Be.EqualTo(1);
			_analyticsPreferenceRepository.PreferencesForPerson(0).First().PreferencesFulfilled.Should().Be.EqualTo(0);
			_analyticsPreferenceRepository.PreferencesForPerson(0).First().PreferencesUnfulfilled.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldHandleSameEventTwicePreference()
		{
			var preferenceDay = setupPreferenceDay(new DateTime(2001, 1, 1));

			_target.Handle(new PreferenceChangedEvent
			{
				PreferenceDayId = preferenceDay.Id.GetValueOrDefault()
			});
			_analyticsPreferenceRepository.PreferencesForPerson(0).Count.Should().Be.EqualTo(1);
			_target.Handle(new PreferenceChangedEvent
			{
				PreferenceDayId = preferenceDay.Id.GetValueOrDefault()
			});
			_analyticsPreferenceRepository.PreferencesForPerson(0).Count.Should().Be.EqualTo(1);
			_analyticsPreferenceRepository.PreferencesForPerson(0).First().PreferencesFulfilled.Should().Be.EqualTo(0);
			_analyticsPreferenceRepository.PreferencesForPerson(0).First().PreferencesUnfulfilled.Should().Be.EqualTo(1);
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

		private PreferenceDay setupPreferenceDay(DateTime date)
		{
			var person = PersonFactory.CreatePersonWithGuid("firstName", "lastName");
			var personPeriodCode = Guid.NewGuid();
			person.AddPersonPeriod(newTestPersonPeriod(date, personPeriodCode));
			_personPeriodRepository.AddPersonPeriod(newTestAnalyticsPersonPeriod(person, personPeriodCode));

			var preferenceRestrictionNew = new PreferenceRestriction();
			preferenceRestrictionNew.ShiftCategory = ShiftCategoryFactory.CreateShiftCategory("hej");
			preferenceRestrictionNew.MustHave = true;

			var defaultScenario = new FakeCurrentScenario().Current();
			defaultScenario.DefaultScenario = true;
			defaultScenario.EnableReporting = true;
			_scenarioRepository.Add(defaultScenario);
			_analyticsScheduleRepository.AddScenario(new AnalyticsGeneric { Code = defaultScenario.Id.GetValueOrDefault(), Id = 2 });

			var preferenceDay = new PreferenceDay(person, new DateOnly(date), preferenceRestrictionNew);
			preferenceDay.WithId();
			_scheduleStorage.Add(preferenceDay);
			_preferenceDayRepository.Add(preferenceDay);
			return preferenceDay;
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
