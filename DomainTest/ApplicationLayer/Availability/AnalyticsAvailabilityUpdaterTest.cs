using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Availability;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Exceptions;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Availability
{
	[DomainTest]
	[NoDefaultData]
	public class AnalyticsAvailabilityUpdaterTest : IExtendSystem
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
		public FakePersonAssignmentRepository PersonAssignmentRepository;

		private const int businessUnitId = 123;
		private const int personId = 321;

		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<AnalyticsAvailabilityUpdater>();
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
			person.PersonPeriodCollection.First().WithId();
			PersonRepository.Add(person);
			AvailabilityDayRepository.Add(new StudentAvailabilityDay(person, today, new IStudentAvailabilityRestriction[]
			{
				new StudentAvailabilityRestriction()
			}));

			Assert.Throws<PersonPeriodMissingInAnalyticsException>(() => Target.Handle(new AvailabilityChangedEvent
			{
				PersonId = personCode,
				Dates = new List<DateOnly>{today},
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
			AvailabilityDayRepository.Add(new StudentAvailabilityDay(person, today, new IStudentAvailabilityRestriction[]
			{
				new StudentAvailabilityRestriction()
			}));
			AnalyticsPersonPeriodRepository.AddOrUpdatePersonPeriod(new AnalyticsPersonPeriod
			{
				PersonId = personId,
				PersonCode = personCode,
				PersonPeriodCode = personPeroidId
			});
			AnalyticsScenarioRepository.AddScenario(AnalyticsScenarioFactory.CreateAnalyticsScenario(scenario, businessUnitId));
			ScenarioRepository.Add(scenario);

			Target.Handle(new AvailabilityChangedEvent
			{
				PersonId = personCode,
				Dates = new List<DateOnly> { today },
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
		public void ShouldHandleDuplicateAvailability()
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

			var availabilityDay = new StudentAvailabilityDay(person, today, new IStudentAvailabilityRestriction[]
			{
				new StudentAvailabilityRestriction()
			});

			AvailabilityDayRepository.Add(availabilityDay);
			AvailabilityDayRepository.Add(availabilityDay);
			AnalyticsPersonPeriodRepository.AddOrUpdatePersonPeriod(new AnalyticsPersonPeriod
			{
				PersonId = personId,
				PersonCode = personCode,
				PersonPeriodCode = personPeroidId
			});
			AnalyticsScenarioRepository.AddScenario(AnalyticsScenarioFactory.CreateAnalyticsScenario(scenario, businessUnitId));
			ScenarioRepository.Add(scenario);

			Target.Handle(new AvailabilityChangedEvent
			{
				PersonId = personCode,
				Dates = new List<DateOnly> { today },
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
			AvailabilityDayRepository.Add(new StudentAvailabilityDay(person, today, new IStudentAvailabilityRestriction[]
			{
				new StudentAvailabilityRestriction()
			}));
			AnalyticsPersonPeriodRepository.AddOrUpdatePersonPeriod(new AnalyticsPersonPeriod
			{
				PersonId = personId,
				PersonCode = personCode,
				PersonPeriodCode = personPeroidId
			});
			AnalyticsScenarioRepository.AddScenario(AnalyticsScenarioFactory.CreateAnalyticsScenario(scenario, businessUnitId));
			ScenarioRepository.Add(scenario);

			Target.Handle(new AvailabilityChangedEvent
			{
				PersonId = personCode,
				Dates = new List<DateOnly> { today },
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
			
			var scenarioCode = Guid.NewGuid();
			var personPeroidId = Guid.NewGuid();
			var today = DateOnly.Today;
			
			var scenario = new Scenario("Asd");
			scenario.SetId(scenarioCode);
			
			AnalyticsScenarioRepository.AddScenario(AnalyticsScenarioFactory.CreateAnalyticsScenario(scenario, businessUnitId));
			ScenarioRepository.Add(scenario);
			var analyticsHourlyAvailablity = new AnalyticsHourlyAvailability
			{
				DateId = AnalyticsDateRepository.Date(today.Date).DateId,
				PersonId = personId,
				ScenarioId = AnalyticsScenarioRepository.Scenarios().First().ScenarioId
			};

			var personCode = Guid.NewGuid();
			var person = PersonFactory.CreatePersonWithPersonPeriod(today.AddDays(-1)).WithId(personCode);
			var personPeriod = person.PersonPeriodCollection.First();
			personPeriod.SetId(personPeroidId);
			PersonRepository.Add(person);
			AnalyticsPersonPeriodRepository.AddOrUpdatePersonPeriod(new AnalyticsPersonPeriod
			{
				PersonId = personId,
				PersonCode = personCode,
				PersonPeriodCode = personPeroidId
			});

			AnalyticsHourlyAvailabilityRepository.AddDimPerson(personId, personCode);
			AnalyticsHourlyAvailabilityRepository.AddOrUpdate(analyticsHourlyAvailablity);

			Target.Handle(new AvailabilityChangedEvent
			{
				PersonId = personCode,
				Dates = new List<DateOnly> { today },
				LogOnBusinessUnitId = businessUnitCode
			});

			AnalyticsHourlyAvailabilityRepository.AnalyticsHourlyAvailabilities.Should().Be.Empty();
		}

		[Test]
		public void ShouldRemoveHourlyAvailabilityWithoutFactPersonPeriod()
		{
			AnalyticsDateRepository.HasDatesBetween(DateTime.Today - TimeSpan.FromDays(100), DateTime.Today + TimeSpan.FromDays(100));
			var businessUnitCode = Guid.NewGuid();
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(businessUnitCode));

			var scenarioCode = Guid.NewGuid();
			var today = DateOnly.Today;
			var scenario = new Scenario("Asd");
			scenario.SetId(scenarioCode);
			ScenarioRepository.Add(scenario);
			AnalyticsScenarioRepository.AddScenario(AnalyticsScenarioFactory.CreateAnalyticsScenario(scenario, businessUnitId));

			var analyticsHourlyAvailablity = new AnalyticsHourlyAvailability
			{
				DateId = AnalyticsDateRepository.Date(today.Date).DateId,
				PersonId = personId,
				ScenarioId = AnalyticsScenarioRepository.Scenarios().First().ScenarioId
			};

			var personCode = Guid.NewGuid();
			var person = PersonFactory.CreatePersonWithPersonPeriod(today.AddDays(-1)).WithId(personCode);
			var personPeriod = person.PersonPeriodCollection.First();
			personPeriod.SetId(personPeriod.Id);
			PersonRepository.Add(person);

			AnalyticsHourlyAvailabilityRepository.AddDimPerson(personId, personCode);
			AnalyticsHourlyAvailabilityRepository.AddOrUpdate(analyticsHourlyAvailablity);

			Target.Handle(new AvailabilityChangedEvent
			{
				PersonId = personCode,
				Dates = new List<DateOnly> { today },
				LogOnBusinessUnitId = businessUnitCode
			});

			AnalyticsHourlyAvailabilityRepository.AnalyticsHourlyAvailabilities.Should().Be.Empty();
		}

		[Test]
		public void ShouldAddAvailabilityWithScheduleTimeWhenScheduleAdded()
		{
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
			AvailabilityDayRepository.Add(new StudentAvailabilityDay(person, today, new IStudentAvailabilityRestriction[]
			{
				new StudentAvailabilityRestriction()
			}));
			AnalyticsPersonPeriodRepository.AddOrUpdatePersonPeriod(new AnalyticsPersonPeriod
			{
				PersonId = personId,
				PersonCode = personCode,
				PersonPeriodCode = personPeroidId
			});
			AnalyticsScenarioRepository.AddScenario(AnalyticsScenarioFactory.CreateAnalyticsScenario(scenario, businessUnitId));
			ScenarioRepository.Add(scenario);

			IActivity phoneActivity = new Activity("act");
			phoneActivity.InWorkTime = true;
			PersonAssignmentRepository.Has(person, scenario, phoneActivity, new DateOnlyPeriod(today, today), new TimePeriod(0, 1));

			Target.Handle(new ScheduleChangedEvent
			{
				PersonId = personCode,
				ScenarioId = scenarioCode,
				StartDateTime = today.Date,
				EndDateTime = today.Date,
				LogOnBusinessUnitId = businessUnitCode
			});

			var result = AnalyticsHourlyAvailabilityRepository.AnalyticsHourlyAvailabilities;
			result.Should().Not.Be.Empty();
			result.Count.Should().Be(1);
			result.First().PersonId.Should().Be.EqualTo(personId);
			result.First().BusinessUnitId.Should().Be.EqualTo(businessUnitId);
			result.First().ScenarioId.Should().Be.GreaterThan(0);
			result.First().DateId.Should().Be.GreaterThan(0);
			result.First().ScheduledTimeMinutes.Should().Be(60);
		}

		[Test]
		public void ShouldAddAvailabilityForAllScheduledDates()
		{
			var businessUnitCode = Guid.NewGuid();
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(businessUnitCode));

			var personCode = Guid.NewGuid();
			var scenarioCode = Guid.NewGuid();
			var personPeroidId = Guid.NewGuid();
			var today = DateOnly.Today;
			var tomorrow = DateOnly.Today.AddDays(1);
			var person = PersonFactory.CreatePersonWithPersonPeriod(today.AddDays(-1)).WithId(personCode);
			person.PersonPeriodCollection.First().WithId(personPeroidId);
			var scenario = new Scenario("Asd").WithId(scenarioCode);
			scenario.EnableReporting = true;

			PersonRepository.Add(person);
			AvailabilityDayRepository.Add(new StudentAvailabilityDay(person, today, new IStudentAvailabilityRestriction[]
			{
				new StudentAvailabilityRestriction()
			}));
			AvailabilityDayRepository.Add(new StudentAvailabilityDay(person, tomorrow, new IStudentAvailabilityRestriction[]
			{
				new StudentAvailabilityRestriction()
			}));
			AnalyticsPersonPeriodRepository.AddOrUpdatePersonPeriod(new AnalyticsPersonPeriod
			{
				PersonId = personId,
				PersonCode = personCode,
				PersonPeriodCode = personPeroidId
			});
			AnalyticsScenarioRepository.AddScenario(AnalyticsScenarioFactory.CreateAnalyticsScenario(scenario, businessUnitId));
			ScenarioRepository.Add(scenario);

			IActivity phoneActivity = new Activity("act");
			phoneActivity.InWorkTime = true;
			PersonAssignmentRepository.Has(person, scenario, phoneActivity, new DateOnlyPeriod(today, today), new TimePeriod(4, 5));
			PersonAssignmentRepository.Has(person, scenario, phoneActivity, new DateOnlyPeriod(tomorrow, tomorrow), new TimePeriod(4, 6));
			
			Target.Handle(new ScheduleChangedEvent
			{
				PersonId = personCode,
				ScenarioId = scenarioCode,
				StartDateTime = today.Date,
				EndDateTime = tomorrow.Date,
				LogOnBusinessUnitId = businessUnitCode
			});

			var result = AnalyticsHourlyAvailabilityRepository.AnalyticsHourlyAvailabilities;
			result.Should().Not.Be.Empty();
			result.Count.Should().Be(2);
			result.First().ScheduledTimeMinutes.Should().Be(60);
			result.Last().ScheduledTimeMinutes.Should().Be(120);
		}

		[Test]
		public void ShouldNotAddAvailabilityWhenNoPersonPeriod()
		{
			var businessUnitCode = Guid.NewGuid();
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(businessUnitCode));

			var personCode = Guid.NewGuid();
			var scenarioCode = Guid.NewGuid();
			var today = DateOnly.Today;
			var person = PersonFactory.CreatePerson().WithId(personCode);
			var scenario = new Scenario("Asd");
			scenario.SetId(scenarioCode);
			scenario.EnableReporting = true;

			PersonRepository.Add(person);
			AvailabilityDayRepository.Add(new StudentAvailabilityDay(person, today, new IStudentAvailabilityRestriction[]
			{
				new StudentAvailabilityRestriction()
			}));
			ScenarioRepository.Add(scenario);

			var phoneActivity = new Activity("act")
			{
				InWorkTime = true
			};
			PersonAssignmentRepository.Has(person, scenario, phoneActivity, new DateOnlyPeriod(today, today), new TimePeriod(0, 1));

			Target.Handle(new ScheduleChangedEvent
			{
				PersonId = personCode,
				ScenarioId = scenarioCode,
				StartDateTime = today.Date,
				EndDateTime = today.Date,
				LogOnBusinessUnitId = businessUnitCode
			});

			AnalyticsHourlyAvailabilityRepository.AnalyticsHourlyAvailabilities.Should().Be.Empty();
		}
	}
}