using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	public class AddFullDayAbsenceCommandHandlerTest
	{

		private static IScheduleRepository stubScheduleRepository(ICurrentScenario currentScenario, DateTime date)
		{
			return stubScheduleRepository(currentScenario, date, null);
		}

		private static IScheduleRepository stubScheduleRepository(ICurrentScenario currentScenario, DateTime date, IPersonAssignment personAssignment)
		{
			var scheduleDictionary = new ScheduleDictionaryForTest(
				currentScenario.Current(),
				date.AddDays(-1),
				date
				);

			if (personAssignment != null)
				scheduleDictionary.AddPersonAssignment(personAssignment);

			var scheduleRepository = MockRepository.GenerateMock<IScheduleRepository>();
			scheduleRepository.Stub(x => x.FindSchedulesOnlyInGivenPeriod(null, null, new DateOnlyPeriod(), null))
							  .IgnoreArguments()
							  .Return(scheduleDictionary);
			return scheduleRepository;
		}

		[Test]
		public void ShouldRaiseFullDayAbsenceAddedEvent()
		{
			var personRepository = new TestWriteSideRepository<IPerson> { PersonFactory.CreatePersonWithId() };
			var absenceRepository = new TestWriteSideRepository<IAbsence> { AbsenceFactory.CreateAbsenceWithId() };
			var personAbsenceRepository = new TestWriteSideRepository<IPersonAbsence>();
			var currentScenario = new FakeCurrentScenario();
			var target = new AddFullDayAbsenceCommandHandler(stubScheduleRepository(currentScenario, new DateTime(2013, 3, 25)), personRepository, absenceRepository, personAbsenceRepository, currentScenario);

			var command = new AddFullDayAbsenceCommand
				{
					AbsenceId = absenceRepository.Single().Id.Value,
					PersonId = personRepository.Single().Id.Value,
					StartDate = new DateTime(2013, 3, 25),
					EndDate = new DateTime(2013, 3, 25),
				};
			target.Handle(command);

			var @event = personAbsenceRepository.Single().PopAllEvents().Single() as FullDayAbsenceAddedEvent;
			@event.AbsenceId.Should().Be(absenceRepository.Single().Id.Value);
			@event.PersonId.Should().Be(personRepository.Single().Id.Value);
			@event.ScenarioId.Should().Be(currentScenario.Current().Id.Value);
			@event.StartDateTime.Should().Be(command.StartDate);
			@event.EndDateTime.Should().Be(command.EndDate.AddHours(24).AddMinutes(-1));
		}

		[Test]
		public void ShouldSetupEntityState()
		{ 
			var personRepository = new TestWriteSideRepository<IPerson> { PersonFactory.CreatePersonWithId() };
			var absenceRepository = new TestWriteSideRepository<IAbsence> { AbsenceFactory.CreateAbsenceWithId() };
			var personAbsenceRepository = new TestWriteSideRepository<IPersonAbsence>();
			var currentScenario = new FakeCurrentScenario();
			var target = new AddFullDayAbsenceCommandHandler(stubScheduleRepository(currentScenario, new DateTime(2013, 3, 25)), personRepository, absenceRepository, personAbsenceRepository, currentScenario);

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
			var personRepository = new TestWriteSideRepository<IPerson> { person };
			var absenceRepository = new TestWriteSideRepository<IAbsence> { AbsenceFactory.CreateAbsenceWithId() };
			var personAbsenceRepository = new TestWriteSideRepository<IPersonAbsence>();
			var currentScenario = new FakeCurrentScenario();
			var target = new AddFullDayAbsenceCommandHandler(stubScheduleRepository(currentScenario, new DateTime(2013, 3, 25)), personRepository, absenceRepository, personAbsenceRepository, currentScenario);

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
			var personRepository = new TestWriteSideRepository<IPerson> { person };
			var absenceRepository = new TestWriteSideRepository<IAbsence> { AbsenceFactory.CreateAbsenceWithId() };
			var personAbsenceRepository = new TestWriteSideRepository<IPersonAbsence>();
			var currentScenario = new FakeCurrentScenario();
			var personAssignmentPeriod = new DateTimePeriod(2013, 3, 25, 10, 2013, 3, 25, 15);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(currentScenario.Current(), person, personAssignmentPeriod);
			var scheduleRepository = stubScheduleRepository(currentScenario, new DateTime(2013, 3, 25), personAssignment);
			var target = new AddFullDayAbsenceCommandHandler(scheduleRepository, personRepository, absenceRepository, personAbsenceRepository, currentScenario);

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
			var personRepository = new TestWriteSideRepository<IPerson> { person };
			var absenceRepository = new TestWriteSideRepository<IAbsence> { AbsenceFactory.CreateAbsenceWithId() };
			var personAbsenceRepository = new TestWriteSideRepository<IPersonAbsence>();
			var currentScenario = new FakeCurrentScenario();
			var personAssignmentPeriod = new DateTimePeriod(2013, 3, 24, 18, 2013, 3, 25, 5);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(currentScenario.Current(), person, personAssignmentPeriod);
			var scheduleRepository = stubScheduleRepository(currentScenario, new DateTime(2013, 3, 25), personAssignment);
			var target = new AddFullDayAbsenceCommandHandler(scheduleRepository, personRepository, absenceRepository, personAbsenceRepository, currentScenario);

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
			var personRepository = new TestWriteSideRepository<IPerson> { person };
			var absenceRepository = new TestWriteSideRepository<IAbsence> { AbsenceFactory.CreateAbsenceWithId() };
			var personAbsenceRepository = new TestWriteSideRepository<IPersonAbsence>();
			var currentScenario = new FakeCurrentScenario();
			var personAssignmentPeriod = new DateTimePeriod(2013, 3, 25, 18, 2013, 3, 26, 5);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(currentScenario.Current(), person, personAssignmentPeriod);
			var scheduleRepository = stubScheduleRepository(currentScenario, new DateTime(2013, 3, 25), personAssignment);
			var target = new AddFullDayAbsenceCommandHandler(scheduleRepository, personRepository, absenceRepository, personAbsenceRepository, currentScenario);

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

		[Test]
		public void ShouldHandleShiftsOnSequenceDays()
		{
			var dateTime = new DateTime(2013, 3, 25);
			var dateOnly = new DateOnly(2013, 3, 25);
			var previousDate = new DateOnly(2013, 3, 24);
			var person = PersonFactory.CreatePersonWithId();
			var personRepository = new TestWriteSideRepository<IPerson> { person };
			var absenceRepository = new TestWriteSideRepository<IAbsence> { AbsenceFactory.CreateAbsenceWithId() };
			var personAbsenceRepository = new TestWriteSideRepository<IPersonAbsence>();
			var scheduleRepository = MockRepository.GenerateMock<IScheduleRepository>();
			var scheduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();
			var scheduleRange = MockRepository.GenerateMock<IScheduleRange>();
			var previousDay = MockRepository.GenerateMock<IScheduleDay>();
			var firstDay = MockRepository.GenerateMock<IScheduleDay>();

			scheduleDictionary.Stub(x => x[person]).Return(scheduleRange);
			scheduleRepository.Stub(x => x.FindSchedulesOnlyInGivenPeriod(null, null, new DateOnlyPeriod(), null))
							  .IgnoreArguments()
							  .Return(scheduleDictionary);

			previousDay.Stub(x => x.Period)
					   .Return(new DateOnlyPeriod(previousDate, previousDate).ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone()));
			firstDay.Stub(x => x.Period)
					   .Return(new DateOnlyPeriod(dateOnly, dateOnly).ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone()));
			var secondDay = MockRepository.GenerateMock<IScheduleDay>();
			secondDay.Stub(x => x.Period)
					   .Return(new DateOnlyPeriod(dateOnly.AddDays(1), dateOnly.AddDays(1)).ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone()));
			var _scheduleDays = new[]
				{
					previousDay,
					firstDay,
					secondDay
				};
			scheduleRange.Stub(
				x => x.ScheduledDayCollection(new DateOnlyPeriod(new DateOnly(dateTime).AddDays(-1), new DateOnly(dateTime).AddDays(1)))).Return(_scheduleDays);

			var assignmentStart = new DateTime(2013, 3, 25, 10, 0, 0, 0, DateTimeKind.Utc);
			var assignmentEnd = new DateTime(2013, 3, 25, 15, 0, 0, 0, DateTimeKind.Utc);
			var assignmentPeriod = new DateTimePeriod(assignmentStart, assignmentEnd);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, assignmentPeriod);
			var personAssignmentOnSecondDay = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, new DateTimePeriod(assignmentStart.AddDays(1), assignmentEnd.AddDays(1).AddHours(2)));

			previousDay.Stub(x => x.PersonAssignment()).Return(null);
			firstDay.Stub(x => x.PersonAssignment()).Return(personAssignment);
			secondDay.Stub(x => x.PersonAssignment()).Return(personAssignmentOnSecondDay);

			var command = new AddFullDayAbsenceCommand
			{
				AbsenceId = absenceRepository.Single().Id.Value,
				PersonId = personRepository.Single().Id.Value,
				StartDate = new DateTime(2013, 3, 25),
				EndDate = new DateTime(2013, 3, 26),
			};

			var target = new AddFullDayAbsenceCommandHandler(scheduleRepository, personRepository, absenceRepository, personAbsenceRepository, new FakeCurrentScenario());
			target.Handle(command);

			var personAbsence = personAbsenceRepository.Single();
			var absenceLayer = personAbsence.Layer as AbsenceLayer;
			personAbsence.Person.Should().Be(personRepository.Single());
			absenceLayer.Payload.Should().Be(absenceRepository.Single());
			absenceLayer.Period.StartDateTime.Should().Be(command.StartDate.AddHours(10));
			absenceLayer.Period.EndDateTime.Should().Be(command.EndDate.AddHours(17));

			var @event = personAbsenceRepository.Single().PopAllEvents().Single() as FullDayAbsenceAddedEvent;
			@event.StartDateTime.Should().Be(command.StartDate.AddHours(10));
			@event.EndDateTime.Should().Be(command.EndDate.AddHours(17));
		}

	}
}