using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	public class AddIntradayAbsenceCommandHandlerTest
	{
		[Test]
		public void ShouldRaiseIntradayAbsenceAddedEvent()
		{
			var personRepository = new FakeWriteSideRepository<IPerson> { PersonFactory.CreatePersonWithId() };
			var absenceRepository = new FakeWriteSideRepository<IAbsence> { AbsenceFactory.CreateAbsenceWithId() };
			var personAbsenceRepository = new FakeWriteSideRepository<IPersonAbsence>();
			var currentScenario = new FakeCurrentScenario();
			var target = new AddIntradayAbsenceCommandHandler(personRepository,
			                                                  absenceRepository, personAbsenceRepository, currentScenario, new UtcTimeZone());

			var operatedPersonId = Guid.NewGuid();
			var command = new AddIntradayAbsenceCommand
				{
					AbsenceId = absenceRepository.Single().Id.Value,
					PersonId = personRepository.Single().Id.Value,
					StartTime = new DateTime(2013, 11, 27, 14, 00, 00, DateTimeKind.Utc),
					EndTime = new DateTime(2013, 11, 27, 15, 00, 00, DateTimeKind.Utc),
					TrackedCommandInfo = new TrackedCommandInfo
					{
						OperatedPersonId = operatedPersonId
					}
				};
			target.Handle(command);

			var @event = personAbsenceRepository.Single().PopAllEvents().Single() as PersonAbsenceAddedEvent;
			@event.AbsenceId.Should().Be(absenceRepository.Single().Id.Value);
			@event.PersonId.Should().Be(personRepository.Single().Id.Value);
			@event.ScenarioId.Should().Be(currentScenario.Current().Id.Value);
			@event.StartDateTime.Should().Be(command.StartTime);
			@event.EndDateTime.Should().Be(command.EndTime);
			@event.InitiatorId.Should().Be(operatedPersonId);
		}

		[Test]
		public void ShouldSetupEntityState()
		{
			var personRepository = new FakeWriteSideRepository<IPerson> { PersonFactory.CreatePersonWithId() };
			var absenceRepository = new FakeWriteSideRepository<IAbsence> { AbsenceFactory.CreateAbsenceWithId() };
			var personAbsenceRepository = new FakeWriteSideRepository<IPersonAbsence>();
			var currentScenario = new FakeCurrentScenario();
			var target = new AddIntradayAbsenceCommandHandler(personRepository, absenceRepository, personAbsenceRepository, currentScenario, new UtcTimeZone());

			var command = new AddIntradayAbsenceCommand
				{
					AbsenceId = absenceRepository.Single().Id.Value,
					PersonId = personRepository.Single().Id.Value,
					StartTime = new DateTime(2013, 11, 27, 14, 00, 00, DateTimeKind.Utc),
					EndTime = new DateTime(2013, 11, 27, 15, 00, 00, DateTimeKind.Utc),
					TrackedCommandInfo = new TrackedCommandInfo()
				};
			target.Handle(command);

			var personAbsence = personAbsenceRepository.Single();
			var absenceLayer = personAbsence.Layer as AbsenceLayer;
			personAbsence.Person.Should().Be(personRepository.Single());
			absenceLayer.Payload.Should().Be(absenceRepository.Single());
			absenceLayer.Period.StartDateTime.Should().Be(command.StartTime);
			absenceLayer.Period.EndDateTime.Should().Be(command.EndTime);
		}

		[Test]
		public void ShouldConvertTimesFromUsersTimeZone()
		{
			var person = PersonFactory.CreatePersonWithId();
			var personRepository = new FakeWriteSideRepository<IPerson> { person };
			var absenceRepository = new FakeWriteSideRepository<IAbsence> { AbsenceFactory.CreateAbsenceWithId() };
			var personAbsenceRepository = new FakeWriteSideRepository<IPersonAbsence>();
			var currentScenario = new FakeCurrentScenario();
			var userTimeZone = TimeZoneInfoFactory.HawaiiTimeZoneInfo();
			var target = new AddIntradayAbsenceCommandHandler(personRepository, absenceRepository, personAbsenceRepository, currentScenario, new SpecificTimeZone(userTimeZone));

			var command = new AddIntradayAbsenceCommand
				{
					AbsenceId = absenceRepository.Single().Id.Value,
					PersonId = personRepository.Single().Id.Value,
					StartTime = new DateTime(2013, 11, 27, 14, 00, 00),
					EndTime = new DateTime(2013, 11, 27, 15, 00, 00),
					TrackedCommandInfo = new TrackedCommandInfo()
				};
			target.Handle(command);

			var personAbsence = personAbsenceRepository.Single();
			var absenceLayer = personAbsence.Layer as AbsenceLayer;
			absenceLayer.Period.StartDateTime.Should().Be(TimeZoneInfo.ConvertTimeToUtc(command.StartTime, userTimeZone));
			absenceLayer.Period.EndDateTime.Should().Be(TimeZoneInfo.ConvertTimeToUtc(command.EndTime, userTimeZone));
			var @event = personAbsenceRepository.Single().PopAllEvents().Single() as PersonAbsenceAddedEvent;
			@event.StartDateTime.Should().Be(TimeZoneInfo.ConvertTimeToUtc(command.StartTime, userTimeZone));
			@event.EndDateTime.Should().Be(TimeZoneInfo.ConvertTimeToUtc(command.EndTime, userTimeZone));
		}
	}
}