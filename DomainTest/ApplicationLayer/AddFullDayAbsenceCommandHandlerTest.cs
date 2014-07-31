using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	public class AddFullDayAbsenceCommandHandlerTest
	{
		[Test]
		public void ShouldRaiseFullDayAbsenceAddedEvent()
		{
			var personRepository = new FakeWriteSideRepository<IPerson> { PersonFactory.CreatePersonWithId() };
			var absenceRepository = new FakeWriteSideRepository<IAbsence> { AbsenceFactory.CreateAbsenceWithId() };
			var personAbsenceRepository = new FakeWriteSideRepository<IPersonAbsence>();
			var currentScenario = new FakeCurrentScenario();
			var target = new AddFullDayAbsenceCommandHandler(new FakePersonAssignmentReadScheduleRepository(), personRepository,
			                                                 absenceRepository, personAbsenceRepository, currentScenario);

			var operatedPersonId = Guid.NewGuid();
			var trackId = Guid.NewGuid();
			var command = new AddFullDayAbsenceCommand
				{
					AbsenceId = absenceRepository.Single().Id.Value,
					PersonId = personRepository.Single().Id.Value,
					StartDate = new DateTime(2013, 3, 25),
					EndDate = new DateTime(2013, 3, 25),
					TrackedCommandInfo = new TrackedCommandInfo()
					{
						OperatedPersonId = operatedPersonId,
						TrackId = trackId
					}
				};
			target.Handle(command);

			var @event = personAbsenceRepository.Single().PopAllEvents().Single() as FullDayAbsenceAddedEvent;
			@event.AbsenceId.Should().Be(absenceRepository.Single().Id.Value);
			@event.PersonId.Should().Be(personRepository.Single().Id.Value);
			@event.ScenarioId.Should().Be(currentScenario.Current().Id.Value);
			@event.StartDateTime.Should().Be(command.StartDate);
			@event.EndDateTime.Should().Be(command.EndDate.AddHours(24).AddMinutes(-1));
			@event.InitiatorId.Should().Be(operatedPersonId);
			@event.TrackId.Should().Be(trackId);
		}

		[Test]
		public void ShouldSetupEntityState()
		{ 
			var personRepository = new FakeWriteSideRepository<IPerson> { PersonFactory.CreatePersonWithId() };
			var absenceRepository = new FakeWriteSideRepository<IAbsence> { AbsenceFactory.CreateAbsenceWithId() };
			var personAbsenceRepository = new FakeWriteSideRepository<IPersonAbsence>();
			var currentScenario = new FakeCurrentScenario();
			var target = new AddFullDayAbsenceCommandHandler(new FakePersonAssignmentReadScheduleRepository(), personRepository, absenceRepository, personAbsenceRepository, currentScenario);

			var command = new AddFullDayAbsenceCommand
				{
					AbsenceId = absenceRepository.Single().Id.Value,
					PersonId = personRepository.Single().Id.Value,
					StartDate = new DateTime(2013, 3, 25),
					EndDate = new DateTime(2013, 3, 25),
				};
			target.Handle(command);

			var personAbsence = personAbsenceRepository.Single();
			var absenceLayer = personAbsence.Layer as AbsenceLayer;
			personAbsence.Person.Should().Be(personRepository.Single());
			absenceLayer.Payload.Should().Be(absenceRepository.Single());
			absenceLayer.Period.StartDateTime.Should().Be(command.StartDate);
			absenceLayer.Period.EndDateTime.Should().Be(command.EndDate.AddHours(24).AddMinutes(-1));
		}

		[Test]
		public void ShouldConvertFromAgentsTimeZone()
		{
			var person = PersonFactory.CreatePersonWithId();
			var agentsTimeZone = TimeZoneInfoFactory.HawaiiTimeZoneInfo();
			person.PermissionInformation.SetDefaultTimeZone(agentsTimeZone);
			var personRepository = new FakeWriteSideRepository<IPerson> { person };
			var absenceRepository = new FakeWriteSideRepository<IAbsence> { AbsenceFactory.CreateAbsenceWithId() };
			var personAbsenceRepository = new FakeWriteSideRepository<IPersonAbsence>();
			var currentScenario = new FakeCurrentScenario();
			var target = new AddFullDayAbsenceCommandHandler(new FakePersonAssignmentReadScheduleRepository(), personRepository, absenceRepository, personAbsenceRepository, currentScenario);

			var command = new AddFullDayAbsenceCommand
				{
					AbsenceId = absenceRepository.Single().Id.Value,
					PersonId = personRepository.Single().Id.Value,
					StartDate = new DateTime(2013, 3, 25),
					EndDate = new DateTime(2013, 3, 25),
				};
			target.Handle(command);

			var personAbsence = personAbsenceRepository.Single();
			var absenceLayer = personAbsence.Layer as AbsenceLayer;
			absenceLayer.Period.StartDateTime.Should().Be(TimeZoneInfo.ConvertTimeToUtc(command.StartDate, agentsTimeZone));
			absenceLayer.Period.EndDateTime.Should().Be(TimeZoneInfo.ConvertTimeToUtc(command.EndDate.AddHours(24).AddMinutes(-1), agentsTimeZone));
			var @event = personAbsenceRepository.Single().PopAllEvents().Single() as FullDayAbsenceAddedEvent;
			@event.StartDateTime.Should().Be(TimeZoneInfo.ConvertTimeToUtc(command.StartDate, agentsTimeZone));
			@event.EndDateTime.Should().Be(TimeZoneInfo.ConvertTimeToUtc(command.EndDate.AddHours(24).AddMinutes(-1), agentsTimeZone));
		}

		[Test]
		public void ShouldOverlapShift()
		{
			var person = PersonFactory.CreatePersonWithId();
			var personRepository = new FakeWriteSideRepository<IPerson> { person };
			var absenceRepository = new FakeWriteSideRepository<IAbsence> { AbsenceFactory.CreateAbsenceWithId() };
			var personAbsenceRepository = new FakeWriteSideRepository<IPersonAbsence>();
			var currentScenario = new FakeCurrentScenario();
			var personAssignmentPeriod = new DateTimePeriod(2013, 3, 25, 10, 2013, 3, 25, 15);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(currentScenario.Current(), person, personAssignmentPeriod);
			var target = new AddFullDayAbsenceCommandHandler(new FakePersonAssignmentReadScheduleRepository(personAssignment), personRepository, absenceRepository, personAbsenceRepository, currentScenario);

			target.Handle(new AddFullDayAbsenceCommand
				{
					AbsenceId = absenceRepository.Single().Id.Value,
					PersonId = personRepository.Single().Id.Value,
					StartDate = new DateTime(2013, 3, 25),
					EndDate = new DateTime(2013, 3, 25),
				});

			var personAbsence = personAbsenceRepository.Single();
			var absenceLayer = personAbsence.Layer as AbsenceLayer;
			absenceLayer.Period.StartDateTime.Should().Be(personAssignmentPeriod.StartDateTime);
			absenceLayer.Period.EndDateTime.Should().Be(personAssignmentPeriod.EndDateTime);
			var @event = personAbsenceRepository.Single().PopAllEvents().Single() as FullDayAbsenceAddedEvent;
			@event.StartDateTime.Should().Be(personAssignmentPeriod.StartDateTime);
			@event.EndDateTime.Should().Be(personAssignmentPeriod.EndDateTime);
		}

		[Test]
		public void ShouldNotOverlapNightShiftFromDayBeforeStartDate()
		{
			var person = PersonFactory.CreatePersonWithId();
			var personRepository = new FakeWriteSideRepository<IPerson> { person };
			var absenceRepository = new FakeWriteSideRepository<IAbsence> { AbsenceFactory.CreateAbsenceWithId() };
			var personAbsenceRepository = new FakeWriteSideRepository<IPersonAbsence>();
			var currentScenario = new FakeCurrentScenario();
			var personAssignmentPeriod = new DateTimePeriod(2013, 3, 24, 18, 2013, 3, 25, 5);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(currentScenario.Current(), person, personAssignmentPeriod);
			var target = new AddFullDayAbsenceCommandHandler(new FakePersonAssignmentReadScheduleRepository(personAssignment), personRepository, absenceRepository, personAbsenceRepository, currentScenario);

			target.Handle(new AddFullDayAbsenceCommand
				{
					AbsenceId = absenceRepository.Single().Id.Value,
					PersonId = personRepository.Single().Id.Value,
					StartDate = new DateTime(2013, 3, 25),
					EndDate = new DateTime(2013, 3, 25),
				});

			var personAbsence = personAbsenceRepository.Single();
			var absenceLayer = personAbsence.Layer as AbsenceLayer;
			absenceLayer.Period.StartDateTime.Should().Be(personAssignmentPeriod.EndDateTime);
			var @event = personAbsenceRepository.Single().PopAllEvents().Single() as FullDayAbsenceAddedEvent;
			@event.StartDateTime.Should().Be(personAssignmentPeriod.EndDateTime);
		}

		[Test]
		public void ShouldFullyOverlapNightShiftOnEndDate()
		{
			var person = PersonFactory.CreatePersonWithId();
			var personRepository = new FakeWriteSideRepository<IPerson> { person };
			var absenceRepository = new FakeWriteSideRepository<IAbsence> { AbsenceFactory.CreateAbsenceWithId() };
			var personAbsenceRepository = new FakeWriteSideRepository<IPersonAbsence>();
			var currentScenario = new FakeCurrentScenario();
			var personAssignmentPeriod = new DateTimePeriod(2013, 3, 25, 18, 2013, 3, 26, 5);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(currentScenario.Current(), person, personAssignmentPeriod);
			var target = new AddFullDayAbsenceCommandHandler(new FakePersonAssignmentReadScheduleRepository(personAssignment), personRepository, absenceRepository, personAbsenceRepository, currentScenario);

			target.Handle(new AddFullDayAbsenceCommand
				{
					AbsenceId = absenceRepository.Single().Id.Value,
					PersonId = personRepository.Single().Id.Value,
					StartDate = new DateTime(2013, 3, 25),
					EndDate = new DateTime(2013, 3, 25),
				});

			var personAbsence = personAbsenceRepository.Single();
			var absenceLayer = personAbsence.Layer as AbsenceLayer;
			absenceLayer.Period.EndDateTime.Should().Be(personAssignmentPeriod.EndDateTime);
			var @event = personAbsenceRepository.Single().PopAllEvents().Single() as FullDayAbsenceAddedEvent; 
			@event.EndDateTime.Should().Be(personAssignmentPeriod.EndDateTime);
		}

	}
}