using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[DomainTest]
	public class TerminatePersonHandlerTest : ISetup
	{
		public MutableNow Now;
		public IPersonAssignmentRepository PersonAssignmentRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		public IScenarioRepository ScenarioRepository;
		public IPersonRepository PersonRepository;
		public TerminatePersonHandler Target;
		public FakeEventPublisher Publisher;
		public IBusinessUnitRepository BusinessUnitRepository;

		private IBusinessUnit businessUnit;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<TerminatePersonHandler>();
			businessUnit = BusinessUnitFactory.CreateWithId("something");
		}

		[Test]
		public void ShouldRemovePersonAssignmentAfterLeavingDate()
		{
			BusinessUnitRepository.Add(businessUnit);
			var scenario = ScenarioFactory.CreateScenario("Default", true, false).WithId();
			ScenarioRepository.Add(scenario);

			var person = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("A", "B").WithId(),
				new DateOnly(2001, 1, 1));
			PersonRepository.Add(person);

			PersonAssignmentRepository.Add(PersonAssignmentFactory
				.CreateAssignmentWithMainShift(person, scenario, new DateTimePeriod(2018, 1, 3, 8, 2018, 1, 3, 17)).WithId());
			PersonAssignmentRepository.Add(PersonAssignmentFactory
				.CreateAssignmentWithMainShift(person, scenario, new DateTimePeriod(2018, 1, 4, 8, 2018, 1, 4, 17)).WithId());

			Target.Handle(new PersonTerminalDateChangedEvent
			{
				LogOnBusinessUnitId = businessUnit.Id.Value,
				PersonId = person.Id.Value,
				TerminationDate = new DateTime(2018, 1, 3)
			});

			PersonAssignmentRepository.Find(Enumerable.Repeat(person, 1), new DateOnlyPeriod(2018, 1, 3, 2018, 1, 5), scenario)
				.Single().Period
				.Should().Be(new DateTimePeriod(2018, 1, 3, 8, 2018, 1, 3, 17));
		}

		[Test]
		public void ShouldNotRemoveScheduleWhenClearingLeavingDate()
		{
			BusinessUnitRepository.Add(businessUnit);
			var scenario = ScenarioFactory.CreateScenario("Default", true, false).WithId();
			ScenarioRepository.Add(scenario);

			var person = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("A", "B").WithId(),
				new DateOnly(2001, 1, 1));
			PersonRepository.Add(person);

			var personAssignment = PersonAssignmentFactory
				.CreateAssignmentWithMainShift(person, scenario, new DateTimePeriod(2018, 1, 1, 8, 2018, 1, 1, 17)).WithId();
			PersonAssignmentRepository.Add(personAssignment);

			Target.Handle(new PersonTerminalDateChangedEvent
			{
				LogOnBusinessUnitId = businessUnit.Id.Value,
				PersonId = person.Id.Value
			});

			PersonAssignmentRepository.Find(Enumerable.Repeat(person, 1), new DateOnlyPeriod(2018, 1, 1, 2018, 1, 2), scenario)
				.Count
				.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldRemoveScheduleAfterLeavingDateInAllScenarios()
		{
			BusinessUnitRepository.Add(businessUnit);
			var defaultScenario = ScenarioFactory.CreateScenario("Default", true, false).WithId();
			var scenario = ScenarioFactory.CreateScenario("High", false, false).WithId();
			ScenarioRepository.Add(defaultScenario);
			ScenarioRepository.Add(scenario);

			var person = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("A", "B").WithId(),
				new DateOnly(2001, 1, 1));
			PersonRepository.Add(person);

			PersonAssignmentRepository.Add(PersonAssignmentFactory
				.CreateAssignmentWithMainShift(person, scenario, new DateTimePeriod(2001, 1, 1, 8, 2001, 1, 1, 17)).WithId());
			PersonAssignmentRepository.Add(PersonAssignmentFactory
				.CreateAssignmentWithMainShift(person, scenario, new DateTimePeriod(2001, 2, 1, 8, 2001, 2, 1, 17)).WithId());
			PersonAssignmentRepository.Add(PersonAssignmentFactory
				.CreateAssignmentWithMainShift(person, defaultScenario, new DateTimePeriod(2001, 1, 1, 8, 2001, 1, 1, 17))
				.WithId());
			PersonAssignmentRepository.Add(PersonAssignmentFactory
				.CreateAssignmentWithMainShift(person, defaultScenario, new DateTimePeriod(2001, 2, 1, 8, 2001, 2, 1, 17))
				.WithId());

			Target.Handle(new PersonTerminalDateChangedEvent
			{
				LogOnBusinessUnitId = businessUnit.Id.Value,
				PersonId = person.Id.Value,
				TerminationDate = new DateTime(2001, 1, 31)
			});

			PersonAssignmentRepository.Find(Enumerable.Repeat(person, 1), new DateOnlyPeriod(2001, 1, 1, 2001, 2, 28), scenario)
				.Single().Period
				.Should().Be(new DateTimePeriod(2001, 1, 1, 8, 2001, 1, 1, 17));

			PersonAssignmentRepository.Find(Enumerable.Repeat(person, 1), new DateOnlyPeriod(2001, 1, 1, 2001, 2, 28),
					defaultScenario)
				.Single().Period
				.Should().Be(new DateTimePeriod(2001, 1, 1, 8, 2001, 1, 1, 17));
		}


		[Test]
		public void ShouldRemoveAbsenceAfterLeavingDate()
		{
			BusinessUnitRepository.Add(businessUnit);
			var scenario = ScenarioFactory.CreateScenario("Default", true, false).WithId();
			ScenarioRepository.Add(scenario);

			var person = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("A", "B").WithId(),
				new DateOnly(2001, 1, 1));
			PersonRepository.Add(person);

			PersonAbsenceRepository.Add(PersonAbsenceFactory
				.CreatePersonAbsence(person, scenario, new DateTimePeriod(2001, 1, 1, 8, 2001, 1, 1, 17)).WithId());
			PersonAbsenceRepository.Add(PersonAbsenceFactory
				.CreatePersonAbsence(person, scenario, new DateTimePeriod(2001, 2, 1, 8, 2001, 2, 1, 17)).WithId());

			Target.Handle(new PersonTerminalDateChangedEvent
			{
				LogOnBusinessUnitId = businessUnit.Id.Value,
				PersonId = person.Id.Value,
				TerminationDate = new DateTime(2001, 1, 31)
			});

			PersonAbsenceRepository.Find(Enumerable.Repeat(person, 1), new DateTimePeriod(2001, 1, 1, 2001, 2, 28), scenario)
				.Single().Period
				.Should().Be(new DateTimePeriod(2001, 1, 1, 8, 2001, 1, 1, 17));
		}

		[Test]
		public void ShouldNotRemoveAbsenceSpanningOverLeavingDate()
		{
			BusinessUnitRepository.Add(businessUnit);
			var scenario = ScenarioFactory.CreateScenario("Default", true, false).WithId();
			scenario.SetBusinessUnit(BusinessUnitFactory.BusinessUnitUsedInTest);
			ScenarioRepository.Add(scenario);

			var person = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("A", "B").WithId(),
				new DateOnly(2001, 1, 1));
			PersonRepository.Add(person);

			PersonAbsenceRepository.Add(PersonAbsenceFactory
				.CreatePersonAbsence(person, scenario, new DateTimePeriod(2001, 1, 31, 8, 2001, 2, 1, 17)).WithId());

			Target.Handle(new PersonTerminalDateChangedEvent
			{
				LogOnBusinessUnitId = businessUnit.Id.Value,
				PersonId = person.Id.Value,
				TerminationDate = new DateTime(2001, 1, 31)
			});

			PersonAbsenceRepository.Find(Enumerable.Repeat(person, 1), new DateTimePeriod(2001, 1, 1, 2001, 2, 28), scenario)
				.Single().Period
				.Should().Be.EqualTo(new DateTimePeriod(2001, 1, 31, 8, 2001, 2, 1, 17));
		}

		[Test]
		public void ShouldRemoveAbsenceAfterLeavingDateInAllScenarios()
		{
			BusinessUnitRepository.Add(businessUnit);
			var defaultScenario = ScenarioFactory.CreateScenario("Default", true, false).WithId();
			var otherScenario = ScenarioFactory.CreateScenario("High", false, false).WithId();
			ScenarioRepository.Add(defaultScenario);
			ScenarioRepository.Add(otherScenario);

			var person = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("A", "B").WithId(),
				new DateOnly(2001, 1, 1));
			PersonRepository.Add(person);

			PersonAbsenceRepository.Add(PersonAbsenceFactory
				.CreatePersonAbsence(person, defaultScenario, new DateTimePeriod(2001, 1, 1, 8, 2001, 1, 1, 17)).WithId());
			PersonAbsenceRepository.Add(PersonAbsenceFactory
				.CreatePersonAbsence(person, defaultScenario, new DateTimePeriod(2001, 2, 1, 8, 2001, 2, 1, 17)).WithId());
			PersonAbsenceRepository.Add(PersonAbsenceFactory
				.CreatePersonAbsence(person, otherScenario, new DateTimePeriod(2001, 1, 1, 8, 2001, 1, 1, 17)).WithId());
			PersonAbsenceRepository.Add(PersonAbsenceFactory
				.CreatePersonAbsence(person, otherScenario, new DateTimePeriod(2001, 2, 1, 8, 2001, 2, 1, 17)).WithId());

			Target.Handle(new PersonTerminalDateChangedEvent
			{
				LogOnBusinessUnitId = businessUnit.Id.Value,
				PersonId = person.Id.Value,
				TerminationDate = new DateTime(2001, 1, 31)
			});

			PersonAbsenceRepository
				.Find(Enumerable.Repeat(person, 1), new DateTimePeriod(2001, 1, 1, 2001, 2, 28), defaultScenario)
				.Count
				.Should().Be.EqualTo(1);
			PersonAbsenceRepository
				.Find(Enumerable.Repeat(person, 1), new DateTimePeriod(2001, 1, 1, 2001, 2, 28), otherScenario)
				.Count
				.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldPublishScheduleChangeEventOnEveryScenario()
		{
			BusinessUnitRepository.Add(businessUnit);
			var defaultScenario = ScenarioFactory.CreateScenario("Default", true, false).WithId();
			var otherScenario = ScenarioFactory.CreateScenario("High", false, false).WithId();
			ScenarioRepository.Add(defaultScenario);
			ScenarioRepository.Add(otherScenario);

			var person = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("A", "B").WithId(),
				new DateOnly(2001, 1, 1));
			PersonRepository.Add(person);

			PersonAssignmentRepository.Add(PersonAssignmentFactory
				.CreateAssignmentWithMainShift(person, defaultScenario, new DateTimePeriod(2018, 1, 2, 7, 2018, 1, 2, 16))
				.WithId());
			PersonAbsenceRepository.Add(PersonAbsenceFactory
				.CreatePersonAbsence(person, otherScenario, new DateTimePeriod(2018, 1, 3, 8, 2018, 1, 3, 17)).WithId());

			var @event = new PersonTerminalDateChangedEvent
			{
				LogOnBusinessUnitId = businessUnit.Id.Value,
				PersonId = person.Id.Value,
				TerminationDate = new DateTime(2018, 01, 01)
			};
			Target.Handle(@event);

			var published = Publisher.PublishedEvents.OfType<ScheduleChangedEvent>().ToList();
			published.Count.Should().Be(2);
			published.Any(x => x.ScenarioId == defaultScenario.Id.Value).Should().Be.True();
			published.Any(x => x.ScenarioId == otherScenario.Id.Value).Should().Be.True();
			published.All(x => x.PersonId == @event.PersonId).Should().Be.True();
		}

		[Test]
		public void ShouldPublishScheduleChangeEventWithValidPeriod()
		{
			BusinessUnitRepository.Add(businessUnit);
			var scenario = ScenarioFactory.CreateScenario("Default", true, false).WithId();
			ScenarioRepository.Add(scenario);

			var person = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("A", "B").WithId(),
				new DateOnly(2001, 1, 1));
			PersonRepository.Add(person);

			PersonAssignmentRepository.Add(PersonAssignmentFactory
				.CreateAssignmentWithMainShift(person, scenario, new DateTimePeriod(2018, 1, 2, 7, 2018, 1, 2, 16)).WithId());
			PersonAbsenceRepository.Add(PersonAbsenceFactory
				.CreatePersonAbsence(person, scenario, new DateTimePeriod(2018, 1, 3, 8, 2018, 1, 3, 17)).WithId());
			PersonAbsenceRepository.Add(PersonAbsenceFactory
				.CreatePersonAbsence(person, scenario, new DateTimePeriod(2018, 1, 01, 8, 2018, 1, 02, 17)).WithId());

			var @event = new PersonTerminalDateChangedEvent
			{
				LogOnBusinessUnitId = businessUnit.Id.Value,
				PersonId = person.Id.Value,
				TerminationDate = new DateTime(2018, 01, 01),
			};
			Target.Handle(@event);

			var published = Publisher.PublishedEvents.OfType<ScheduleChangedEvent>().Single();
			published.StartDateTime.Should().Be(new DateTime(2018, 1, 2, 7, 0, 0));
			published.EndDateTime.Should().Be(new DateTime(2018, 1, 3, 17, 0, 0));
		}

		[Test]
		public void ShouldNotPublishScheduleChangeEventWhenNoSchedulesDeleted()
		{
			BusinessUnitRepository.Add(businessUnit);
			var scenario = ScenarioFactory.CreateScenario("Default", true, false).WithId();
			ScenarioRepository.Add(scenario);

			var person = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("A", "B").WithId(),
				new DateOnly(2001, 1, 1));
			PersonRepository.Add(person);

			var @event = new PersonTerminalDateChangedEvent
			{
				LogOnBusinessUnitId = businessUnit.Id.Value,
				PersonId = person.Id.Value,
				TerminationDate = new DateTime(2018, 01, 01)
			};
			Target.Handle(@event);

			Publisher.PublishedEvents.OfType<ScheduleChangedEvent>().Count().Should().Be(0);
		}

		[Test]
		public void ShouldOnlyPublishScheduleChangeEventForScenarioWithSchedulesDeleted()
		{
			BusinessUnitRepository.Add(businessUnit);
			var scenario = ScenarioFactory.CreateScenario("Default", true, false).WithId();
			ScenarioRepository.Add(scenario);

			var anotherScenario = ScenarioFactory.CreateScenario("High", true, false).WithId();
			ScenarioRepository.Add(anotherScenario);

			var person = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("A", "B").WithId(),
				new DateOnly(2001, 1, 1));
			PersonRepository.Add(person);

			PersonAssignmentRepository.Add(PersonAssignmentFactory
				.CreateAssignmentWithMainShift(person, scenario, new DateTimePeriod(2018, 1, 2, 7, 2018, 1, 2, 16)).WithId());
			PersonAssignmentRepository.Add(PersonAssignmentFactory
				.CreateAssignmentWithMainShift(person, anotherScenario, new DateTimePeriod(2018, 1, 1, 7, 2018, 1, 1, 16))
				.WithId());

			var @event = new PersonTerminalDateChangedEvent
			{
				LogOnBusinessUnitId = businessUnit.Id.Value,
				PersonId = person.Id.Value,
				TerminationDate = new DateTime(2018, 01, 01)
			};
			Target.Handle(@event);

			var published = Publisher.PublishedEvents.OfType<ScheduleChangedEvent>().Single();
			published.ScenarioId.Should().Be(scenario.Id.Value);
		}

	}
}
