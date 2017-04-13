using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Availability;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
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

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Availability
{
	[TestFixture]
	[DomainTestWithStaticDependenciesAvoidUse]
	public class AnalyticsAvailabilityUpdaterTest : ISetup
	{
		public AnalyticsAvailabilityUpdater Target;
		public FakeStudentAvailabilityDayRepository AvailabilityDayRepository;
		public FakeAnalyticsPersonPeriodRepository AnalyticsPersonPeriodRepository;
		public FakeAnalyticsDateRepository AnalyticsDateRepository;
		public FakeAnalyticsScenarioRepository AnalyticsScenarioRepository;
		public IPersonRepository PersonRepository;
		public IScheduleStorage ScheduleStorage;
		public FakeAnalyticsHourlyAvailabilityRepository AnalyticsHourlyAvailabilityRepository;
		public IScenarioRepository ScenarioRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;

		private const int businessUnitId = 123;
		private const int personId = 321;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<AnalyticsAvailabilityUpdater>();
		}

		[Test]
		public void PersonPeriodMissingFromAnalyticsShouldThrow()
		{
			AnalyticsDateRepository.HasDatesBetween(DateTime.Today - TimeSpan.FromDays(100), DateTime.Today + TimeSpan.FromDays(100));
			var businessUnitCode = Guid.NewGuid();
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(businessUnitCode));

			var personCode = Guid.NewGuid();
			var today = DateOnly.Today;
			var person = PersonFactory.CreatePersonWithPersonPeriod(today.AddDays(-1)).WithId(personCode);
			PersonRepository.Add(person);
			AvailabilityDayRepository.Add(new StudentAvailabilityDay(person, today, new IStudentAvailabilityRestriction[] { new StudentAvailabilityRestriction() }));

			Assert.Throws<ApplicationException>(() => Target.Handle(new AvailabilityChangedEvent
			{
				AvailabilityId = Guid.NewGuid(),
				PersonId = personCode,
				Date = today,
				LogOnBusinessUnitId = businessUnitCode
			}));
		}

		[Test]
		public void ShouldAddHourlyAvailabilityForScenario()
		{
			AnalyticsDateRepository.HasDatesBetween(DateTime.Today - TimeSpan.FromDays(100), DateTime.Today + TimeSpan.FromDays(100));
			var businessUnitCode = Guid.NewGuid();
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(businessUnitCode));

			var personCode = Guid.NewGuid();
			var scenarioCode = Guid.NewGuid();
			var personPeroidId = Guid.NewGuid();
			var today = DateOnly.Today;
			var person = PersonFactory.CreatePersonWithPersonPeriod(today.AddDays(-1)).WithId(personCode);
			var personPeriod = person.PersonPeriodCollection.First();
			personPeriod.SetId(personPeroidId);
			var scenario = new Scenario("Asd");
			scenario.SetId(scenarioCode);
			scenario.EnableReporting = true;

			PersonRepository.Add(person);
			AvailabilityDayRepository.Add(new StudentAvailabilityDay(person, today, new IStudentAvailabilityRestriction[] { new StudentAvailabilityRestriction() }));
			AnalyticsPersonPeriodRepository.AddPersonPeriod(new AnalyticsPersonPeriod {PersonId = personId, PersonCode = personCode, PersonPeriodCode = personPeroidId });
			AnalyticsScenarioRepository.AddScenario(AnalyticsScenarioFactory.CreateAnalyticsScenario(scenario, businessUnitId));
			ScenarioRepository.Add(scenario);

			Target.Handle(new AvailabilityChangedEvent
			{
				AvailabilityId = Guid.NewGuid(),
				PersonId = personCode,
				Date = today,
				LogOnBusinessUnitId = businessUnitCode
			});

			AnalyticsHourlyAvailabilityRepository.AnalyticsHourlyAvailabilities.Should().Not.Be.Empty();
			var result = AnalyticsHourlyAvailabilityRepository.AnalyticsHourlyAvailabilities.Single();
			result.PersonId.Should().Be.EqualTo(personId);
			result.BusinessUnitId.Should().Be.EqualTo(businessUnitId);
			result.ScenarioId.Should().Be.GreaterThan(0);
			result.DateId.Should().Be.GreaterThan(0);
		}

		[Test]
		public void ShouldNotAddHourlyAvailabilityForNonReportableScenario()
		{
			AnalyticsDateRepository.HasDatesBetween(DateTime.Today - TimeSpan.FromDays(100), DateTime.Today + TimeSpan.FromDays(100));
			var businessUnitCode = Guid.NewGuid();
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(businessUnitCode));

			var personCode = Guid.NewGuid();
			var scenarioCode = Guid.NewGuid();
			var personPeroidId = Guid.NewGuid();
			var today = DateOnly.Today;
			var person = PersonFactory.CreatePersonWithPersonPeriod(today.AddDays(-1)).WithId(personCode);
			var personPeriod = person.PersonPeriodCollection.First();
			personPeriod.SetId(personPeroidId);
			var scenario = new Scenario("Asd");
			scenario.SetId(scenarioCode);
			scenario.EnableReporting = false;
			
			PersonRepository.Add(person);
			AvailabilityDayRepository.Add(new StudentAvailabilityDay(person, today, new IStudentAvailabilityRestriction[] { new StudentAvailabilityRestriction() }));
			AnalyticsPersonPeriodRepository.AddPersonPeriod(new AnalyticsPersonPeriod { PersonId = personId, PersonCode = personCode, PersonPeriodCode = personPeroidId });
			AnalyticsScenarioRepository.AddScenario(AnalyticsScenarioFactory.CreateAnalyticsScenario(scenario, businessUnitId));
			ScenarioRepository.Add(scenario);

			Target.Handle(new AvailabilityChangedEvent
			{
				AvailabilityId = Guid.NewGuid(),
				PersonId = personCode,
				Date = today,
				LogOnBusinessUnitId = businessUnitCode
			});

			AnalyticsHourlyAvailabilityRepository.AnalyticsHourlyAvailabilities.Should().Be.Empty();
		}

		[Test]
		public void ShouldRemoveHourlyAvailabilityForScenario()
		{
			AnalyticsDateRepository.HasDatesBetween(DateTime.Today - TimeSpan.FromDays(100), DateTime.Today + TimeSpan.FromDays(100));
			var businessUnitCode = Guid.NewGuid();
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(businessUnitCode));

			var personCode = Guid.NewGuid();
			var scenarioCode = Guid.NewGuid();
			var personPeroidId = Guid.NewGuid();
			var today = DateOnly.Today;
			var person = PersonFactory.CreatePersonWithPersonPeriod(today.AddDays(-1)).WithId(personCode);
			var personPeriod = person.PersonPeriodCollection.First();
			personPeriod.SetId(personPeroidId);
			var scenario = new Scenario("Asd");
			scenario.SetId(scenarioCode);

			PersonRepository.Add(person);
			AnalyticsPersonPeriodRepository.AddPersonPeriod(new AnalyticsPersonPeriod { PersonId = personId, PersonCode = personCode, PersonPeriodCode = personPeroidId });
			AnalyticsScenarioRepository.AddScenario(AnalyticsScenarioFactory.CreateAnalyticsScenario(scenario, businessUnitId));
			ScenarioRepository.Add(scenario);
			AnalyticsHourlyAvailabilityRepository.AnalyticsHourlyAvailabilities.Add(new AnalyticsHourlyAvailability {DateId = AnalyticsDateRepository.Date(today.Date).DateId, PersonId = personId, ScenarioId = AnalyticsScenarioRepository.Scenarios().First().ScenarioId});

			Target.Handle(new AvailabilityChangedEvent
			{
				AvailabilityId = Guid.NewGuid(),
				PersonId = personCode,
				Date = today,
				LogOnBusinessUnitId = businessUnitCode
			});

			AnalyticsHourlyAvailabilityRepository.AnalyticsHourlyAvailabilities.Should().Be.Empty();
		}
	}
}