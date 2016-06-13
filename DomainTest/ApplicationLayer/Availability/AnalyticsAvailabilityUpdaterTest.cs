using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Availability;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Availability
{
	[TestFixture]
	public class AnalyticsAvailabilityUpdaterTest
	{
		private AnalyticsAvailabilityUpdater _target;
		private IStudentAvailabilityDayRepository _availabilityDayRepository;
		private IAnalyticsPersonPeriodRepository _analyticsPersonPeriodRepository;
		private IAnalyticsDateRepository _analyticsDateRepository;
		private IAnalyticsScenarioRepository _analyticsScenarioRepository;
		private IPersonRepository _personRepository;
		private IScheduleStorage _scheduleStorage;
		private FakeAnalyticsHourlyAvailabilityRepository _analyticsHourlyAvailabilityRepository;
		private IScenarioRepository _scenarioRepository;

		private const int businessUnitId = 123;
		private const int personId = 321;

		[SetUp]
		public void Setup()
		{
			_availabilityDayRepository = new FakeStudentAvailabilityDayRepository();
			_analyticsPersonPeriodRepository = new FakeAnalyticsPersonPeriodRepository();
			_analyticsDateRepository = new FakeAnalyticsDateRepository(DateTime.Today-TimeSpan.FromDays(100), DateTime.Today + TimeSpan.FromDays(100));
			_analyticsScenarioRepository = new FakeAnalyticsScenarioRepository();
			_personRepository = new FakePersonRepository();
			_scheduleStorage = new FakeScheduleStorage();
			_analyticsHourlyAvailabilityRepository = new FakeAnalyticsHourlyAvailabilityRepository();
			_scenarioRepository = new FakeScenarioRepository();

			_target = new AnalyticsAvailabilityUpdater(_availabilityDayRepository, _analyticsPersonPeriodRepository,
				_analyticsDateRepository, _analyticsScenarioRepository, _personRepository, _scheduleStorage,
				_analyticsHourlyAvailabilityRepository, _scenarioRepository);
		}

		[Test, ExpectedException]
		public void PersonPeriodMissingFromAnalyticsShouldThrow()
		{
			var personCode = Guid.NewGuid();
			var today = DateOnly.Today;
			var person = PersonFactory.CreatePersonWithPersonPeriod(today.AddDays(-1));
			person.SetId(personCode);
			_personRepository.Add(person);
			_availabilityDayRepository.Add(new StudentAvailabilityDay(person, today, new IStudentAvailabilityRestriction[] { new StudentAvailabilityRestriction() }));

			_target.Handle(new AvailabilityChangedEvent
			{
				AvailabilityId = Guid.NewGuid(),
				PersonId = personCode,
				Date = today
			});
		}

		[Test]
		public void ShouldAddHourlyAvailabilityForScenario()
		{
			var personCode = Guid.NewGuid();
			var scenarioCode = Guid.NewGuid();
			var personPeroidId = Guid.NewGuid();
			var today = DateOnly.Today;
			var person = PersonFactory.CreatePersonWithPersonPeriod(today.AddDays(-1));
			person.SetId(personCode);
			var personPeriod = person.PersonPeriodCollection.First();
			personPeriod.SetId(personPeroidId);
			var scenario = new Scenario("Asd");
			scenario.SetId(scenarioCode);
			scenario.EnableReporting = true;

			_personRepository.Add(person);
			_availabilityDayRepository.Add(new StudentAvailabilityDay(person, today, new IStudentAvailabilityRestriction[] { new StudentAvailabilityRestriction() }));
			_analyticsPersonPeriodRepository.AddPersonPeriod(new AnalyticsPersonPeriod {PersonId = personId, PersonCode = personCode, PersonPeriodCode = personPeroidId });
			_analyticsScenarioRepository.AddScenario(new AnalyticsScenario {ScenarioCode = scenarioCode, BusinessUnitId = businessUnitId});
			_scenarioRepository.Add(scenario);

			_target.Handle(new AvailabilityChangedEvent
			{
				AvailabilityId = Guid.NewGuid(),
				PersonId = personCode,
				Date = today
			});

			_analyticsHourlyAvailabilityRepository.AnalyticsHourlyAvailabilities.Should().Not.Be.Empty();
			var result = _analyticsHourlyAvailabilityRepository.AnalyticsHourlyAvailabilities.Single();
			result.PersonId.Should().Be.EqualTo(personId);
			result.BusinessUnitId.Should().Be.EqualTo(businessUnitId);
			result.ScenarioId.Should().Be.GreaterThan(0);
			result.DateId.Should().Be.GreaterThan(0);
		}

		[Test]
		public void ShouldNotAddHourlyAvailabilityForNonReportableScenario()
		{
			var personCode = Guid.NewGuid();
			var scenarioCode = Guid.NewGuid();
			var personPeroidId = Guid.NewGuid();
			var today = DateOnly.Today;
			var person = PersonFactory.CreatePersonWithPersonPeriod(today.AddDays(-1));
			person.SetId(personCode);
			var personPeriod = person.PersonPeriodCollection.First();
			personPeriod.SetId(personPeroidId);
			var scenario = new Scenario("Asd");
			scenario.SetId(scenarioCode);
			scenario.EnableReporting = false;

			_personRepository.Add(person);
			_availabilityDayRepository.Add(new StudentAvailabilityDay(person, today, new IStudentAvailabilityRestriction[] { new StudentAvailabilityRestriction() }));
			_analyticsPersonPeriodRepository.AddPersonPeriod(new AnalyticsPersonPeriod { PersonId = personId, PersonCode = personCode, PersonPeriodCode = personPeroidId });
			_analyticsScenarioRepository.AddScenario(new AnalyticsScenario { ScenarioCode = scenarioCode, BusinessUnitId = businessUnitId });
			_scenarioRepository.Add(scenario);

			_target.Handle(new AvailabilityChangedEvent
			{
				AvailabilityId = Guid.NewGuid(),
				PersonId = personCode,
				Date = today
			});

			_analyticsHourlyAvailabilityRepository.AnalyticsHourlyAvailabilities.Should().Be.Empty();
		}

		[Test]
		public void ShouldRemoveHourlyAvailabilityForScenario()
		{
			var personCode = Guid.NewGuid();
			var scenarioCode = Guid.NewGuid();
			var personPeroidId = Guid.NewGuid();
			var today = DateOnly.Today;
			var person = PersonFactory.CreatePersonWithPersonPeriod(today.AddDays(-1));
			person.SetId(personCode);
			var personPeriod = person.PersonPeriodCollection.First();
			personPeriod.SetId(personPeroidId);
			var scenario = new Scenario("Asd");
			scenario.SetId(scenarioCode);

			_personRepository.Add(person);
			_analyticsPersonPeriodRepository.AddPersonPeriod(new AnalyticsPersonPeriod { PersonId = personId, PersonCode = personCode, PersonPeriodCode = personPeroidId });
			_analyticsScenarioRepository.AddScenario(new AnalyticsScenario { ScenarioCode = scenarioCode, BusinessUnitId = businessUnitId });
			_scenarioRepository.Add(scenario);
			_analyticsHourlyAvailabilityRepository.AnalyticsHourlyAvailabilities.Add(new AnalyticsHourlyAvailability {DateId = _analyticsDateRepository.Date(today.Date).DateId, PersonId = personId, ScenarioId = _analyticsScenarioRepository.Scenarios().First().ScenarioId});

			_target.Handle(new AvailabilityChangedEvent
			{
				AvailabilityId = Guid.NewGuid(),
				PersonId = personCode,
				Date = today
			});

			_analyticsHourlyAvailabilityRepository.AnalyticsHourlyAvailabilities.Should().Be.Empty();
		}
	}
}