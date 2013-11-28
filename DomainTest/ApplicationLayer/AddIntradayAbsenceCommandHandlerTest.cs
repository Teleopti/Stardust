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
using Teleopti.Ccc.TestCommon.FakeRepositories;
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
			var target = new AddIntradayAbsenceCommandHandler(new FakePersonAssignmentReadScheduleRepository(), personRepository,
			                                                  absenceRepository, personAbsenceRepository, currentScenario, new UtcTimeZone());

			var command = new AddIntradayAbsenceCommand
				{
					AbsenceId = absenceRepository.Single().Id.Value,
					PersonId = personRepository.Single().Id.Value,
					Date = new DateOnly(2013, 11, 27),
					StartTime = new TimeOfDay(new TimeSpan(14, 00, 00)),
					EndTime = new TimeOfDay(new TimeSpan(15, 00, 00))
				};
			target.Handle(command);

			var @event = personAbsenceRepository.Single().PopAllEvents().Single() as PersonAbsenceAddedEvent;
			@event.AbsenceId.Should().Be(absenceRepository.Single().Id.Value);
			@event.PersonId.Should().Be(personRepository.Single().Id.Value);
			@event.ScenarioId.Should().Be(currentScenario.Current().Id.Value);
			@event.StartDateTime.Should().Be(command.Date.Date.Add(command.StartTime.Time));
			@event.EndDateTime.Should().Be(command.Date.Date.Add(command.EndTime.Time));
		}

		[Test]
		public void ShouldSetupEntityState()
		{
			var personRepository = new FakeWriteSideRepository<IPerson> { PersonFactory.CreatePersonWithId() };
			var absenceRepository = new FakeWriteSideRepository<IAbsence> { AbsenceFactory.CreateAbsenceWithId() };
			var personAbsenceRepository = new FakeWriteSideRepository<IPersonAbsence>();
			var currentScenario = new FakeCurrentScenario();
			var target = new AddIntradayAbsenceCommandHandler(new FakePersonAssignmentReadScheduleRepository(), personRepository, absenceRepository, personAbsenceRepository, currentScenario, new UtcTimeZone());

			var command = new AddIntradayAbsenceCommand
				{
					AbsenceId = absenceRepository.Single().Id.Value,
					PersonId = personRepository.Single().Id.Value,
					Date = new DateOnly(2013, 11, 27),
					StartTime = new TimeOfDay(new TimeSpan(14, 00, 00)),
					EndTime = new TimeOfDay(new TimeSpan(15, 00, 00))
				};
			target.Handle(command);

			var personAbsence = personAbsenceRepository.Single();
			var absenceLayer = personAbsence.Layer as AbsenceLayer;
			personAbsence.Person.Should().Be(personRepository.Single());
			absenceLayer.Payload.Should().Be(absenceRepository.Single());
			absenceLayer.Period.StartDateTime.Should().Be(command.Date.Date.Add(command.StartTime.Time));
			absenceLayer.Period.EndDateTime.Should().Be(command.Date.Date.Add(command.EndTime.Time));
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
			var target = new AddIntradayAbsenceCommandHandler(new FakePersonAssignmentReadScheduleRepository(), personRepository, absenceRepository, personAbsenceRepository, currentScenario, new SpecificTimeZone(userTimeZone));

			var command = new AddIntradayAbsenceCommand
				{
					AbsenceId = absenceRepository.Single().Id.Value,
					PersonId = personRepository.Single().Id.Value,
					Date = new DateOnly(2013, 11, 27),
					StartTime = new TimeOfDay(new TimeSpan(14, 00, 00)),
					EndTime = new TimeOfDay(new TimeSpan(15, 00, 00))
				};
			target.Handle(command);

			var personAbsence = personAbsenceRepository.Single();
			var absenceLayer = personAbsence.Layer as AbsenceLayer;
			absenceLayer.Period.StartDateTime.Should().Be(TimeZoneInfo.ConvertTimeToUtc(command.Date.Date.Add(command.StartTime.Time), userTimeZone));
			absenceLayer.Period.EndDateTime.Should().Be(TimeZoneInfo.ConvertTimeToUtc(command.Date.Date.Add(command.EndTime.Time), userTimeZone));
			var @event = personAbsenceRepository.Single().PopAllEvents().Single() as PersonAbsenceAddedEvent;
			@event.StartDateTime.Should().Be(TimeZoneInfo.ConvertTimeToUtc(command.Date.Date.Add(command.StartTime.Time), userTimeZone));
			@event.EndDateTime.Should().Be(TimeZoneInfo.ConvertTimeToUtc(command.Date.Date.Add(command.EndTime.Time), userTimeZone));
		}
	}
}