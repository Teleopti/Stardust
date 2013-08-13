using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
		private IPerson _person;
		private TestWriteSideRepository<IPerson> _personRepository;
		private TestWriteSideRepository<IAbsence> _absenceRepository;
		private TestWriteSideRepository<IPersonAbsence> _personAbsenceRepository;
		private IScheduleRepository _scheduleRepository;
		private IScheduleDay _previousDay;
		private IScheduleDay _firstDay;
		private DateOnly _dateOnly;
		private DateOnly _previousDate;
		private IScheduleDay[] _scheduleDays;
		private IScheduleRange _scheduleRange;
		private DateTime _dateTime;

		[SetUp]
		public void Setup()
		{
			_dateTime = new DateTime(2013, 3, 25);
			_dateOnly = new DateOnly(2013, 3, 25);
			_previousDate = new DateOnly(2013, 3, 24);
			_person = PersonFactory.CreatePersonWithId();
			_personRepository = new TestWriteSideRepository<IPerson> { _person };
			_absenceRepository = new TestWriteSideRepository<IAbsence> { AbsenceFactory.CreateAbsenceWithId() };
			_personAbsenceRepository = new TestWriteSideRepository<IPersonAbsence>();
			_scheduleRepository = MockRepository.GenerateMock<IScheduleRepository>();
			var scheduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();
			_scheduleRange = MockRepository.GenerateMock<IScheduleRange>();
			_previousDay = MockRepository.GenerateMock<IScheduleDay>();
			_firstDay = MockRepository.GenerateMock<IScheduleDay>();
			
			
			scheduleDictionary.Stub(x => x[_person]).Return(_scheduleRange);
			_scheduleRepository.Stub(x => x.FindSchedulesOnlyInGivenPeriod(null, null, new DateTimePeriod(), null))
							  .IgnoreArguments()
							  .Return(scheduleDictionary);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void ShouldRaiseFullDayAbsenceAddedEvent()
		{
			var currentScenario = new FakeCurrentScenario();
			_previousDay.Stub(x => x.Period)
					   .Return(new DateOnlyPeriod(_previousDate, _previousDate).ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone()));
			_firstDay.Stub(x => x.Period)
					   .Return(new DateOnlyPeriod(_dateOnly, _dateOnly).ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone()));
			_scheduleDays = new[]
				{
					_previousDay,
					_firstDay
				};
			_scheduleRange.Stub(
				x => x.ScheduledDayCollection(new DateOnlyPeriod(new DateOnly(_dateTime).AddDays(-1), new DateOnly(_dateTime)))).Return(_scheduleDays);
			_previousDay.Stub(x => x.PersonAssignmentCollection())
							.Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment>()));
			_firstDay.Stub(x => x.PersonAssignmentCollection())
							.Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment>()));

			var command = new AddFullDayAbsenceCommand
			{
				AbsenceId = _absenceRepository.Single().Id.Value,
				PersonId = _personRepository.Single().Id.Value,
				StartDate = new DateTime(2013, 3, 25),
				EndDate = new DateTime(2013, 3, 25),
			};

			var target = new AddFullDayAbsenceCommandHandler(currentScenario, _personRepository,
															 _absenceRepository, _personAbsenceRepository, _scheduleRepository);
			target.Handle(command);

			var @event = _personAbsenceRepository.Single().PopAllEvents().Single() as FullDayAbsenceAddedEvent;
			@event.AbsenceId.Should().Be(_absenceRepository.Single().Id.Value);
			@event.PersonId.Should().Be(_personRepository.Single().Id.Value);
			@event.ScenarioId.Should().Be(currentScenario.Current().Id.Value);
			@event.StartDateTime.Should().Be(command.StartDate);
			@event.EndDateTime.Should().Be(command.EndDate.AddHours(24).AddMinutes(-1));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void ShouldSetupEntityState()
		{
			_previousDay.Stub(x => x.Period)
					   .Return(new DateOnlyPeriod(_previousDate, _previousDate).ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone()));
			_firstDay.Stub(x => x.Period)
					   .Return(new DateOnlyPeriod(_dateOnly, _dateOnly).ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone()));
			_scheduleDays = new[]
				{
					_previousDay,
					_firstDay
				};
			_scheduleRange.Stub(
				x => x.ScheduledDayCollection(new DateOnlyPeriod(new DateOnly(_dateTime).AddDays(-1), new DateOnly(_dateTime)))).Return(_scheduleDays);
			_previousDay.Stub(x => x.PersonAssignmentCollection())
							.Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment>()));
			_firstDay.Stub(x => x.PersonAssignmentCollection())
							.Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment>()));

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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void ShouldConvertFromAgentsTimeZone()
		{
			var agentsTimeZone = TimeZoneInfoFactory.HawaiiTimeZoneInfo();
			_person.PermissionInformation.SetDefaultTimeZone(agentsTimeZone);
			_previousDay.Stub(x => x.Period)
					   .Return(new DateOnlyPeriod(_previousDate, _previousDate).ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone()));
			_firstDay.Stub(x => x.Period)
					   .Return(new DateOnlyPeriod(_dateOnly, _dateOnly).ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone()));
			_scheduleDays = new[]
				{
					_previousDay,
					_firstDay
				};
			_scheduleRange.Stub(
				x => x.ScheduledDayCollection(new DateOnlyPeriod(new DateOnly(_dateTime).AddDays(-1), new DateOnly(_dateTime)))).Return(_scheduleDays);
			_previousDay.Stub(x => x.PersonAssignmentCollection())
							.Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment>()));
			_firstDay.Stub(x => x.PersonAssignmentCollection())
							.Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment>()));

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
		public void ShouldConsiderNightShiftEndOnDayBeforeStartDate()
		{
			_previousDay.Stub(x => x.Period)
					   .Return(new DateOnlyPeriod(_previousDate, _previousDate).ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone()));
			_firstDay.Stub(x => x.Period)
					   .Return(new DateOnlyPeriod(_dateOnly, _dateOnly).ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone()));
			_scheduleDays = new[]
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

			var personAssignmentsList = new List<IPersonAssignment> { personAssignment };
			var readOnlyCollection = new ReadOnlyCollection<IPersonAssignment>(personAssignmentsList);
			_previousDay.Stub(x => x.PersonAssignmentCollection()).Return(readOnlyCollection);
			_firstDay.Stub(x => x.PersonAssignmentCollection())
							.Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment>()));

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
		public void ShouldConsiderNightShiftEndOnEndDate()
		{
			_previousDay.Stub(x => x.Period)
					   .Return(new DateOnlyPeriod(_previousDate, _previousDate).ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone()));
			_firstDay.Stub(x => x.Period)
					   .Return(new DateOnlyPeriod(_dateOnly, _dateOnly).ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone()));
			_scheduleDays = new[]
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

			var personAssignmentsList = new List<IPersonAssignment> { personAssignment };
			var readOnlyCollection = new ReadOnlyCollection<IPersonAssignment>(personAssignmentsList);
			_previousDay.Stub(x => x.PersonAssignmentCollection())
							.Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment>()));
			_firstDay.Stub(x => x.PersonAssignmentCollection()).Return(readOnlyCollection);

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
		public void ShouldConsiderShiftStartAndEndWithinADay()
		{
			_previousDay.Stub(x => x.Period)
					   .Return(new DateOnlyPeriod(_previousDate, _previousDate).ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone()));
			_firstDay.Stub(x => x.Period)
					   .Return(new DateOnlyPeriod(_dateOnly, _dateOnly).ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone()));
			_scheduleDays = new[]
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

			var personAssignmentsList = new List<IPersonAssignment> { personAssignment };
			var readOnlyCollection = new ReadOnlyCollection<IPersonAssignment>(personAssignmentsList);
			_previousDay.Stub(x => x.PersonAssignmentCollection())
							.Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment>()));
			_firstDay.Stub(x => x.PersonAssignmentCollection()).Return(readOnlyCollection);

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
		public void ShouldHandleShiftsOnSequenceDays()
		{
			_previousDay.Stub(x => x.Period)
					   .Return(new DateOnlyPeriod(_previousDate, _previousDate).ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone()));
			_firstDay.Stub(x => x.Period)
					   .Return(new DateOnlyPeriod(_dateOnly, _dateOnly).ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone()));
			var secondDay = MockRepository.GenerateMock<IScheduleDay>();
			secondDay.Stub(x => x.Period)
					   .Return(new DateOnlyPeriod(_dateOnly.AddDays(1), _dateOnly.AddDays(1)).ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone()));
			_scheduleDays = new[]
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

			_previousDay.Stub(x => x.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment>()));
			_firstDay.Stub(x => x.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> {personAssignment}));
			secondDay.Stub(x => x.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> {personAssignmentOnSecondDay}));

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

		[Test]
		public void ShouldHandleShiftsAndEmptyDayMixed1()
		{
			_previousDay.Stub(x => x.Period)
					   .Return(new DateOnlyPeriod(_previousDate, _previousDate).ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone()));
			_firstDay.Stub(x => x.Period)
					   .Return(new DateOnlyPeriod(_dateOnly, _dateOnly).ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone()));
			var secondDay = MockRepository.GenerateMock<IScheduleDay>();
			secondDay.Stub(x => x.Period)
					   .Return(new DateOnlyPeriod(_dateOnly.AddDays(1), _dateOnly.AddDays(1)).ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone()));
			_scheduleDays = new[]
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

			_previousDay.Stub(x => x.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment>()));
			_firstDay.Stub(x => x.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { personAssignment }));
			secondDay.Stub(x => x.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment>()));

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
			absenceLayer.Period.EndDateTime.Should().Be(command.EndDate.AddDays(1).AddMinutes(-1));

			var @event = _personAbsenceRepository.Single().PopAllEvents().Single() as FullDayAbsenceAddedEvent;
			@event.StartDateTime.Should().Be(command.StartDate.AddHours(10));
			@event.EndDateTime.Should().Be(command.EndDate.AddDays(1).AddMinutes(-1));
		}

		[Test]
		public void ShouldHandleShiftsAndEmptyDayMixed2()
		{
			_previousDay.Stub(x => x.Period)
					   .Return(new DateOnlyPeriod(_previousDate, _previousDate).ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone()));
			_firstDay.Stub(x => x.Period)
					   .Return(new DateOnlyPeriod(_dateOnly, _dateOnly).ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone()));
			var secondDay = MockRepository.GenerateMock<IScheduleDay>();
			secondDay.Stub(x => x.Period)
					   .Return(new DateOnlyPeriod(_dateOnly.AddDays(1), _dateOnly.AddDays(1)).ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone()));
			_scheduleDays = new[]
				{
					_previousDay,
					_firstDay,
					secondDay
				};
			_scheduleRange.Stub(
				x => x.ScheduledDayCollection(new DateOnlyPeriod(new DateOnly(_dateTime).AddDays(-1), new DateOnly(_dateTime).AddDays(1)))).Return(_scheduleDays);

			var assignmentStart = new DateTime(2013, 3, 26, 10, 0, 0, 0, DateTimeKind.Utc);
			var assignmentEnd = new DateTime(2013, 3, 26, 15, 0, 0, 0, DateTimeKind.Utc);
			var assignmentPeriod = new DateTimePeriod(assignmentStart, assignmentEnd);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(_person, assignmentPeriod);

			_previousDay.Stub(x => x.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment>()));
			_firstDay.Stub(x => x.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment>()));
			secondDay.Stub(x => x.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> {personAssignment}));

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
			absenceLayer.Period.StartDateTime.Should().Be(command.StartDate);
			absenceLayer.Period.EndDateTime.Should().Be(command.EndDate.AddHours(15));

			var @event = _personAbsenceRepository.Single().PopAllEvents().Single() as FullDayAbsenceAddedEvent;
			@event.StartDateTime.Should().Be(command.StartDate);
			@event.EndDateTime.Should().Be(command.EndDate.AddHours(15));
		}

	}
}