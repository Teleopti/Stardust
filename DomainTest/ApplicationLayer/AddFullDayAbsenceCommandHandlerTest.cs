using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	public class AddFullDayAbsenceCommandHandlerTest
	{
		public void ShouldRaiseFullDayAbsenceAddedEvent()
		{
			var personRepository = new TestWriteSideRepository<IPerson> { PersonFactory.CreatePersonWithId() };
			var absenceRepository = new TestWriteSideRepository<IAbsence> { AbsenceFactory.CreateAbsenceWithId() };
			var personAbsenceRepository = new TestWriteSideRepository<IPersonAbsence>();
			var scheduleRepository = MockRepository.GenerateMock<IScheduleRepository>();
			var currentScenario = new FakeCurrentScenario();
			scheduleRepository.Stub(x => x.FindSchedulesOnlyInGivenPeriod(null, null, new DateOnlyPeriod(), null))
							  .IgnoreArguments()
							  .Return(new ScheduleDictionaryForTest(currentScenario.Current(), new DateTime(2013, 3, 24), new DateTime(2013, 3, 25)));
			var command = new AddFullDayAbsenceCommand
			{
				AbsenceId = absenceRepository.Single().Id.Value,
				PersonId = personRepository.Single().Id.Value,
				StartDate = new DateTime(2013, 3, 25),
				EndDate = new DateTime(2013, 3, 25),
			};
			var target = new AddFullDayAbsenceCommandHandler(currentScenario, personRepository, absenceRepository, personAbsenceRepository, scheduleRepository);
			
			target.Handle(command);

			var @event = personAbsenceRepository.Single().PopAllEvents().Single() as FullDayAbsenceAddedEvent;
			@event.AbsenceId.Should().Be(absenceRepository.Single().Id.Value);
			@event.PersonId.Should().Be(personRepository.Single().Id.Value);
			@event.ScenarioId.Should().Be(currentScenario.Current().Id.Value);
			@event.StartDateTime.Should().Be(command.StartDate);
			@event.EndDateTime.Should().Be(command.EndDate.AddHours(24).AddMinutes(-1));
		}

		public void ShouldSetupEntityState()
		{ 
			var _dateTime = new DateTime(2013, 3, 25);
			var _dateOnly = new DateOnly(2013, 3, 25);
			var _previousDate = new DateOnly(2013, 3, 24);
			var _person = PersonFactory.CreatePersonWithId();
			var _personRepository = new TestWriteSideRepository<IPerson> { _person };
			var _absenceRepository = new TestWriteSideRepository<IAbsence> { AbsenceFactory.CreateAbsenceWithId() };
			var _personAbsenceRepository = new TestWriteSideRepository<IPersonAbsence>();
			var _scheduleRepository = MockRepository.GenerateMock<IScheduleRepository>();
			var scheduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();
			var _scheduleRange = MockRepository.GenerateMock<IScheduleRange>();
			var _previousDay = MockRepository.GenerateMock<IScheduleDay>();
			var _firstDay = MockRepository.GenerateMock<IScheduleDay>();

			scheduleDictionary.Stub(x => x[_person]).Return(_scheduleRange);
			_scheduleRepository.Stub(x => x.FindSchedulesOnlyInGivenPeriod(null, null, new DateOnlyPeriod(), null))
							  .IgnoreArguments()
							  .Return(scheduleDictionary);

			_previousDay.Stub(x => x.Period)
					   .Return(new DateOnlyPeriod(_previousDate, _previousDate).ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone()));
			_firstDay.Stub(x => x.Period)
					   .Return(new DateOnlyPeriod(_dateOnly, _dateOnly).ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone()));
			var _scheduleDays = new[]
				{
					_previousDay,
					_firstDay
				};
			_scheduleRange.Stub(
				x => x.ScheduledDayCollection(new DateOnlyPeriod(new DateOnly(_dateTime).AddDays(-1), new DateOnly(_dateTime)))).Return(_scheduleDays);
			_previousDay.Stub(x => x.PersonAssignment()).Return(null);
			_firstDay.Stub(x => x.PersonAssignment()).Return(null);

			var command = new AddFullDayAbsenceCommand
			{
				AbsenceId = _absenceRepository.Single().Id.Value,
				PersonId = _personRepository.Single().Id.Value,
				StartDate = new DateTime(2013, 3, 25),
				EndDate = new DateTime(2013, 3, 25),
			};

			var target = new AddFullDayAbsenceCommandHandler(new FakeCurrentScenario(), _personRepository, _absenceRepository,
			                                                 _personAbsenceRepository, _scheduleRepository);
			target.Handle(command);

			var personAbsence = _personAbsenceRepository.Single();
			var absenceLayer = personAbsence.Layer as AbsenceLayer;
			personAbsence.Person.Should().Be(_personRepository.Single());
			absenceLayer.Payload.Should().Be(_absenceRepository.Single());
			absenceLayer.Period.StartDateTime.Should().Be(command.StartDate);
			absenceLayer.Period.EndDateTime.Should().Be(command.EndDate.AddHours(24).AddMinutes(-1));
		}

		public void ShouldConvertFromAgentsTimeZone()
		{
			var _dateTime = new DateTime(2013, 3, 25);
			var _dateOnly = new DateOnly(2013, 3, 25);
			var _previousDate = new DateOnly(2013, 3, 24);
			var _person = PersonFactory.CreatePersonWithId();
			var _personRepository = new TestWriteSideRepository<IPerson> { _person };
			var _absenceRepository = new TestWriteSideRepository<IAbsence> { AbsenceFactory.CreateAbsenceWithId() };
			var _personAbsenceRepository = new TestWriteSideRepository<IPersonAbsence>();
			var _scheduleRepository = MockRepository.GenerateMock<IScheduleRepository>();
			var scheduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();
			var _scheduleRange = MockRepository.GenerateMock<IScheduleRange>();
			var _previousDay = MockRepository.GenerateMock<IScheduleDay>();
			var _firstDay = MockRepository.GenerateMock<IScheduleDay>();

			scheduleDictionary.Stub(x => x[_person]).Return(_scheduleRange);
			_scheduleRepository.Stub(x => x.FindSchedulesOnlyInGivenPeriod(null, null, new DateOnlyPeriod(), null))
							  .IgnoreArguments()
							  .Return(scheduleDictionary);

			var agentsTimeZone = TimeZoneInfoFactory.HawaiiTimeZoneInfo();
			_person.PermissionInformation.SetDefaultTimeZone(agentsTimeZone);
			_previousDay.Stub(x => x.Period)
					   .Return(new DateOnlyPeriod(_previousDate, _previousDate).ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone()));
			_firstDay.Stub(x => x.Period)
					   .Return(new DateOnlyPeriod(_dateOnly, _dateOnly).ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone()));
			var _scheduleDays = new[]
				{
					_previousDay,
					_firstDay
				};
			_scheduleRange.Stub(
				x => x.ScheduledDayCollection(new DateOnlyPeriod(new DateOnly(_dateTime).AddDays(-1), new DateOnly(_dateTime)))).Return(_scheduleDays);
			_previousDay.Stub(x => x.PersonAssignment()).Return(null);
			_firstDay.Stub(x => x.PersonAssignment()).Return(null);

			var command = new AddFullDayAbsenceCommand
			{
				AbsenceId = _absenceRepository.Single().Id.Value,
				PersonId = _personRepository.Single().Id.Value,
				StartDate = new DateTime(2013, 3, 25),
				EndDate = new DateTime(2013, 3, 25),
			};

			var target = new AddFullDayAbsenceCommandHandler(new FakeCurrentScenario(), _personRepository, _absenceRepository, _personAbsenceRepository, _scheduleRepository);
			target.Handle(command);

			var personAbsence = _personAbsenceRepository.Single();
			var absenceLayer = personAbsence.Layer as AbsenceLayer;
			personAbsence.Person.Should().Be(_personRepository.Single());
			absenceLayer.Payload.Should().Be(_absenceRepository.Single());
			absenceLayer.Period.StartDateTime.Should().Be(TimeZoneInfo.ConvertTimeToUtc(command.StartDate, agentsTimeZone));
			absenceLayer.Period.EndDateTime.Should().Be(TimeZoneInfo.ConvertTimeToUtc(command.EndDate.AddHours(24).AddMinutes(-1), agentsTimeZone));
			var @event = _personAbsenceRepository.Single().PopAllEvents().Single() as FullDayAbsenceAddedEvent;
			@event.StartDateTime.Should().Be(TimeZoneInfo.ConvertTimeToUtc(command.StartDate, agentsTimeZone));
			@event.EndDateTime.Should().Be(TimeZoneInfo.ConvertTimeToUtc(command.EndDate.AddHours(24).AddMinutes(-1), agentsTimeZone));
		}

		[Test]
		public void ShouldOverlapShift()
		{
			var _dateTime = new DateTime(2013, 3, 25);
			var _dateOnly = new DateOnly(2013, 3, 25);
			var _previousDate = new DateOnly(2013, 3, 24);
			var _person = PersonFactory.CreatePersonWithId();
			var _personRepository = new TestWriteSideRepository<IPerson> { _person };
			var _absenceRepository = new TestWriteSideRepository<IAbsence> { AbsenceFactory.CreateAbsenceWithId() };
			var _personAbsenceRepository = new TestWriteSideRepository<IPersonAbsence>();
			var _scheduleRepository = MockRepository.GenerateMock<IScheduleRepository>();
			var scheduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();
			var _scheduleRange = MockRepository.GenerateMock<IScheduleRange>();
			var _previousDay = MockRepository.GenerateMock<IScheduleDay>();
			var _firstDay = MockRepository.GenerateMock<IScheduleDay>();

			scheduleDictionary.Stub(x => x[_person]).Return(_scheduleRange);
			_scheduleRepository.Stub(x => x.FindSchedulesOnlyInGivenPeriod(null, null, new DateOnlyPeriod(), null))
							  .IgnoreArguments()
							  .Return(scheduleDictionary);

			_previousDay.Stub(x => x.Period)
					   .Return(new DateOnlyPeriod(_previousDate, _previousDate).ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone()));
			_firstDay.Stub(x => x.Period)
					   .Return(new DateOnlyPeriod(_dateOnly, _dateOnly).ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone()));
			var _scheduleDays = new[]
				{
					_previousDay,
					_firstDay
				};
			_scheduleRange.Stub(
				x => x.ScheduledDayCollection(new DateOnlyPeriod(new DateOnly(_dateTime).AddDays(-1), new DateOnly(_dateTime)))).Return(_scheduleDays);

			var assignmentStart = new DateTime(2013, 3, 25, 10, 0, 0, 0, DateTimeKind.Utc);
			var assignmentEnd = new DateTime(2013, 3, 25, 15, 0, 0, 0, DateTimeKind.Utc);
			var assignmentPeriod = new DateTimePeriod(assignmentStart, assignmentEnd);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(_person, assignmentPeriod);

			_previousDay.Stub(x => x.PersonAssignment()).Return(null);
			_firstDay.Stub(x => x.PersonAssignment()).Return(personAssignment);

			var command = new AddFullDayAbsenceCommand
			{
				AbsenceId = _absenceRepository.Single().Id.Value,
				PersonId = _personRepository.Single().Id.Value,
				StartDate = new DateTime(2013, 3, 25),
				EndDate = new DateTime(2013, 3, 25),
			};

			var target = new AddFullDayAbsenceCommandHandler(new FakeCurrentScenario(), _personRepository, _absenceRepository,
															 _personAbsenceRepository, _scheduleRepository);
			target.Handle(command);

			var personAbsence = _personAbsenceRepository.Single();
			var absenceLayer = personAbsence.Layer as AbsenceLayer;
			personAbsence.Person.Should().Be(_personRepository.Single());
			absenceLayer.Payload.Should().Be(_absenceRepository.Single());
			absenceLayer.Period.StartDateTime.Should().Be(command.StartDate.AddHours(10));
			absenceLayer.Period.EndDateTime.Should().Be(command.EndDate.AddHours(15));

			var @event = _personAbsenceRepository.Single().PopAllEvents().Single() as FullDayAbsenceAddedEvent;
			@event.StartDateTime.Should().Be(command.StartDate.AddHours(10));
			@event.EndDateTime.Should().Be(command.EndDate.AddHours(15));
		}

		[Test]
		public void ShouldNotOverlapNightShiftFromDayBeforeStartDate()
		{
			var _dateTime = new DateTime(2013, 3, 25);
			var _dateOnly = new DateOnly(2013, 3, 25);
			var _previousDate = new DateOnly(2013, 3, 24);
			var _person = PersonFactory.CreatePersonWithId();
			var _personRepository = new TestWriteSideRepository<IPerson> { _person };
			var _absenceRepository = new TestWriteSideRepository<IAbsence> { AbsenceFactory.CreateAbsenceWithId() };
			var _personAbsenceRepository = new TestWriteSideRepository<IPersonAbsence>();
			var _scheduleRepository = MockRepository.GenerateMock<IScheduleRepository>();
			var scheduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();
			var _scheduleRange = MockRepository.GenerateMock<IScheduleRange>();
			var _previousDay = MockRepository.GenerateMock<IScheduleDay>();
			var _firstDay = MockRepository.GenerateMock<IScheduleDay>();

			scheduleDictionary.Stub(x => x[_person]).Return(_scheduleRange);
			_scheduleRepository.Stub(x => x.FindSchedulesOnlyInGivenPeriod(null, null, new DateOnlyPeriod(), null))
							  .IgnoreArguments()
							  .Return(scheduleDictionary);

			_previousDay.Stub(x => x.Period)
					   .Return(new DateOnlyPeriod(_previousDate, _previousDate).ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone()));
			_firstDay.Stub(x => x.Period)
					   .Return(new DateOnlyPeriod(_dateOnly, _dateOnly).ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone()));
			var _scheduleDays = new[]
				{
					_previousDay,
					_firstDay
				};
			_scheduleRange.Stub(
				x => x.ScheduledDayCollection(new DateOnlyPeriod(new DateOnly(_dateTime).AddDays(-1), new DateOnly(_dateTime)))).Return(_scheduleDays);

			var assignmentStart = new DateTime(2013, 3, 24, 18, 0, 0, 0, DateTimeKind.Utc);
			var assignmentEnd = new DateTime(2013, 3, 25, 5, 0, 0, 0, DateTimeKind.Utc);
			var assignmentPeriod = new DateTimePeriod(assignmentStart, assignmentEnd);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(_person, assignmentPeriod);

			_previousDay.Stub(x => x.PersonAssignment()).Return(personAssignment);
			_firstDay.Stub(x => x.PersonAssignment()).Return(null);

			var command = new AddFullDayAbsenceCommand
			{
				AbsenceId = _absenceRepository.Single().Id.Value,
				PersonId = _personRepository.Single().Id.Value,
				StartDate = new DateTime(2013, 3, 25),
				EndDate = new DateTime(2013, 3, 25),
			};

			var target = new AddFullDayAbsenceCommandHandler(new FakeCurrentScenario(), _personRepository, _absenceRepository,
			                                                 _personAbsenceRepository, _scheduleRepository);
			target.Handle(command);

			var personAbsence = _personAbsenceRepository.Single();
			var absenceLayer = personAbsence.Layer as AbsenceLayer;
			personAbsence.Person.Should().Be(_personRepository.Single());
			absenceLayer.Payload.Should().Be(_absenceRepository.Single());
			absenceLayer.Period.StartDateTime.Should().Be(command.StartDate.AddHours(5));

			var @event = _personAbsenceRepository.Single().PopAllEvents().Single() as FullDayAbsenceAddedEvent;
			@event.StartDateTime.Should().Be(command.StartDate.AddHours(5));
			@event.EndDateTime.Should().Be(command.EndDate.AddHours(24).AddMinutes(-1));
		}

		[Test]
		public void ShouldFullyOverlapNightShiftOnEndDate()
		{
			var _dateTime = new DateTime(2013, 3, 25);
			var _dateOnly = new DateOnly(2013, 3, 25);
			var _previousDate = new DateOnly(2013, 3, 24);
			var _person = PersonFactory.CreatePersonWithId();
			var _personRepository = new TestWriteSideRepository<IPerson> { _person };
			var _absenceRepository = new TestWriteSideRepository<IAbsence> { AbsenceFactory.CreateAbsenceWithId() };
			var _personAbsenceRepository = new TestWriteSideRepository<IPersonAbsence>();
			var _scheduleRepository = MockRepository.GenerateMock<IScheduleRepository>();
			var scheduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();
			var _scheduleRange = MockRepository.GenerateMock<IScheduleRange>();
			var _previousDay = MockRepository.GenerateMock<IScheduleDay>();
			var _firstDay = MockRepository.GenerateMock<IScheduleDay>();

			scheduleDictionary.Stub(x => x[_person]).Return(_scheduleRange);
			_scheduleRepository.Stub(x => x.FindSchedulesOnlyInGivenPeriod(null, null, new DateOnlyPeriod(), null))
							  .IgnoreArguments()
							  .Return(scheduleDictionary);

			_previousDay.Stub(x => x.Period)
					   .Return(new DateOnlyPeriod(_previousDate, _previousDate).ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone()));
			_firstDay.Stub(x => x.Period)
					   .Return(new DateOnlyPeriod(_dateOnly, _dateOnly).ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone()));
			var _scheduleDays = new[]
				{
					_previousDay,
					_firstDay
				};
			_scheduleRange.Stub(
				x => x.ScheduledDayCollection(new DateOnlyPeriod(new DateOnly(_dateTime).AddDays(-1), new DateOnly(_dateTime)))).Return(_scheduleDays);

			var assignmentStart = new DateTime(2013, 3, 25, 18, 0, 0, 0, DateTimeKind.Utc);
			var assignmentEnd = new DateTime(2013, 3, 26, 5, 0, 0, 0, DateTimeKind.Utc);
			var assignmentPeriod = new DateTimePeriod(assignmentStart, assignmentEnd);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(_person, assignmentPeriod);

			_previousDay.Stub(x => x.PersonAssignment()).Return(null);
			_firstDay.Stub(x => x.PersonAssignment()).Return(personAssignment);

			var command = new AddFullDayAbsenceCommand
			{
				AbsenceId = _absenceRepository.Single().Id.Value,
				PersonId = _personRepository.Single().Id.Value,
				StartDate = new DateTime(2013, 3, 25),
				EndDate = new DateTime(2013, 3, 25),
			};

			var target = new AddFullDayAbsenceCommandHandler(new FakeCurrentScenario(), _personRepository, _absenceRepository,
			                                                 _personAbsenceRepository, _scheduleRepository);
			target.Handle(command);

			var personAbsence = _personAbsenceRepository.Single();
			var absenceLayer = personAbsence.Layer as AbsenceLayer;
			personAbsence.Person.Should().Be(_personRepository.Single());
			absenceLayer.Payload.Should().Be(_absenceRepository.Single());
			absenceLayer.Period.EndDateTime.Should().Be(command.EndDate.AddDays(1).AddHours(5));

			var @event = _personAbsenceRepository.Single().PopAllEvents().Single() as FullDayAbsenceAddedEvent;
			@event.EndDateTime.Should().Be(command.EndDate.AddDays(1).AddHours(5));
		}

		[Test]
		public void ShouldHandleShiftsOnSequenceDays()
		{
			var _dateTime = new DateTime(2013, 3, 25);
			var _dateOnly = new DateOnly(2013, 3, 25);
			var _previousDate = new DateOnly(2013, 3, 24);
			var _person = PersonFactory.CreatePersonWithId();
			var _personRepository = new TestWriteSideRepository<IPerson> { _person };
			var _absenceRepository = new TestWriteSideRepository<IAbsence> { AbsenceFactory.CreateAbsenceWithId() };
			var _personAbsenceRepository = new TestWriteSideRepository<IPersonAbsence>();
			var _scheduleRepository = MockRepository.GenerateMock<IScheduleRepository>();
			var scheduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();
			var _scheduleRange = MockRepository.GenerateMock<IScheduleRange>();
			var _previousDay = MockRepository.GenerateMock<IScheduleDay>();
			var _firstDay = MockRepository.GenerateMock<IScheduleDay>();

			scheduleDictionary.Stub(x => x[_person]).Return(_scheduleRange);
			_scheduleRepository.Stub(x => x.FindSchedulesOnlyInGivenPeriod(null, null, new DateOnlyPeriod(), null))
							  .IgnoreArguments()
							  .Return(scheduleDictionary);

			_previousDay.Stub(x => x.Period)
					   .Return(new DateOnlyPeriod(_previousDate, _previousDate).ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone()));
			_firstDay.Stub(x => x.Period)
					   .Return(new DateOnlyPeriod(_dateOnly, _dateOnly).ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone()));
			var secondDay = MockRepository.GenerateMock<IScheduleDay>();
			secondDay.Stub(x => x.Period)
					   .Return(new DateOnlyPeriod(_dateOnly.AddDays(1), _dateOnly.AddDays(1)).ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone()));
			var _scheduleDays = new[]
				{
					_previousDay,
					_firstDay,
					secondDay
				};
			_scheduleRange.Stub(
				x => x.ScheduledDayCollection(new DateOnlyPeriod(new DateOnly(_dateTime).AddDays(-1), new DateOnly(_dateTime).AddDays(1)))).Return(_scheduleDays);

			var assignmentStart = new DateTime(2013, 3, 25, 10, 0, 0, 0, DateTimeKind.Utc);
			var assignmentEnd = new DateTime(2013, 3, 25, 15, 0, 0, 0, DateTimeKind.Utc);
			var assignmentPeriod = new DateTimePeriod(assignmentStart, assignmentEnd);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(_person, assignmentPeriod);
			var personAssignmentOnSecondDay = PersonAssignmentFactory.CreateAssignmentWithMainShift(_person, new DateTimePeriod(assignmentStart.AddDays(1), assignmentEnd.AddDays(1).AddHours(2)));

			_previousDay.Stub(x => x.PersonAssignment()).Return(null);
			_firstDay.Stub(x => x.PersonAssignment()).Return(personAssignment);
			secondDay.Stub(x => x.PersonAssignment()).Return(personAssignmentOnSecondDay);

			var command = new AddFullDayAbsenceCommand
			{
				AbsenceId = _absenceRepository.Single().Id.Value,
				PersonId = _personRepository.Single().Id.Value,
				StartDate = new DateTime(2013, 3, 25),
				EndDate = new DateTime(2013, 3, 26),
			};

			var target = new AddFullDayAbsenceCommandHandler(new FakeCurrentScenario(), _personRepository, _absenceRepository,
			                                                 _personAbsenceRepository, _scheduleRepository);
			target.Handle(command);

			var personAbsence = _personAbsenceRepository.Single();
			var absenceLayer = personAbsence.Layer as AbsenceLayer;
			personAbsence.Person.Should().Be(_personRepository.Single());
			absenceLayer.Payload.Should().Be(_absenceRepository.Single());
			absenceLayer.Period.StartDateTime.Should().Be(command.StartDate.AddHours(10));
			absenceLayer.Period.EndDateTime.Should().Be(command.EndDate.AddHours(17));

			var @event = _personAbsenceRepository.Single().PopAllEvents().Single() as FullDayAbsenceAddedEvent;
			@event.StartDateTime.Should().Be(command.StartDate.AddHours(10));
			@event.EndDateTime.Should().Be(command.EndDate.AddHours(17));
		}

	}
}