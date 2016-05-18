using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	public class AddPersonalActivityCommandHandlerTest
	{
		private IActivity _personalActivity;
		private DateTimePeriod _personalActivityDateTimePeriod;
		private DateTimePeriod _mainActivityDateTimePeriod;
		private IScenario _scenario;
		private DateTime _startTime;
		private DateTime _endTime;
		private FakeWriteSideRepository<IPerson> _personRepository;
		private IActivity _mainActivity;
		private FakeWriteSideRepository<IActivity> _activityRepository;
		private DateOnly _date;

		[SetUp]
		public void SetUp()
		{
			_scenario = new Scenario("scenario");
			_personRepository = new FakeWriteSideRepository<IPerson> { PersonFactory.CreatePersonWithId() };
			_personalActivity = ActivityFactory.CreateActivity("personalActivity");
			_mainActivity = ActivityFactory.CreateActivity("mainActivity");
			_activityRepository = new FakeWriteSideRepository<IActivity> {_mainActivity, _personalActivity};
			_date = new DateOnly(2016, 05,17);
			_startTime = new DateTime(2016, 05, 17, 10, 0, 0, DateTimeKind.Utc);
			_endTime = new DateTime(2016, 05, 17, 12, 0, 0, DateTimeKind.Utc);
			_personalActivityDateTimePeriod = new DateTimePeriod(_startTime, _endTime);
			_mainActivityDateTimePeriod = new DateTimePeriod(new DateTime(2016, 05, 17, 09, 0, 0, DateTimeKind.Utc), new DateTime(2016, 05, 17, 17, 0, 0, DateTimeKind.Utc));
		}

		[Test]
		public void ShouldRaisePersonalActivityAddedEvent()
		{
			var personAssignment = new PersonAssignment(_personRepository.Single(), _scenario, _date);
			personAssignment.AddActivity(_mainActivity, _mainActivityDateTimePeriod);
			personAssignment.AddPersonalActivity(_personalActivity, _personalActivityDateTimePeriod);
			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository
			{
				PersonAssignmentFactory.CreateAssignmentWithMainShift(
						_mainActivity, _personRepository.Single(),_mainActivityDateTimePeriod)
			};

			var currentScenario = new ThisCurrentScenario(personAssignmentRepository.Single().Scenario);
			
			var command = new AddPersonalActivityCommand
			{
				PersonId = _personRepository.Single().Id.Value,
				Date = _date,
				PersonalActivityId = _personalActivity.Id.Value,
				StartTime = _startTime,
				EndTime = _endTime,
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = Guid.NewGuid()
				}
			};

			var target = new AddPersonalActivityCommandHandler(personAssignmentRepository, currentScenario, _activityRepository, _personRepository, new UtcTimeZone());

			target.Handle(command);

			var @event =personAssignmentRepository.Single().PopAllEvents(new Now()).OfType<PersonalActivityAddedEvent>().Single(e => e.ActivityId == _personalActivity.Id.Value);

			@event.PersonId.Should().Be(_personRepository.Single().Id.Value);
			@event.Date.Should().Be(new DateTime(2016, 05, 17));
			@event.StartDateTime.Should().Be(command.StartTime);
			@event.EndDateTime.Should().Be(command.EndTime);
			@event.ScenarioId.Should().Be(personAssignmentRepository.Single().Scenario.Id.Value);
			@event.InitiatorId.Should().Be(command.TrackedCommandInfo.OperatedPersonId);
			@event.CommandId.Should().Be(command.TrackedCommandInfo.TrackId);
			@event.LogOnBusinessUnitId.Should().Be(currentScenario.Current().BusinessUnit.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldSetupEntityState()
		{
			var personAssignment = new PersonAssignment(_personRepository.Single(), _scenario, _date);
			personAssignment.AddActivity(_mainActivity, _mainActivityDateTimePeriod);
			personAssignment.AddPersonalActivity(_personalActivity, _personalActivityDateTimePeriod);
			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository
			{
				PersonAssignmentFactory.CreateAssignmentWithMainShift(
						_mainActivity, _personRepository.Single(),_mainActivityDateTimePeriod)
			};

			var currentScenario = new ThisCurrentScenario(personAssignmentRepository.Single().Scenario);

			var command = new AddPersonalActivityCommand
			{
				PersonId = _personRepository.Single().Id.Value,
				Date = _date,
				PersonalActivityId = _personalActivity.Id.Value,
				StartTime = _startTime,
				EndTime = _endTime,
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = Guid.NewGuid()
				}
			};

			var target = new AddPersonalActivityCommandHandler(personAssignmentRepository, currentScenario, _activityRepository, _personRepository, new UtcTimeZone());

			target.Handle(command);

			var result = personAssignment.ShiftLayers.OfType<PersonalShiftLayer>().Single();

			result.Payload.Id.Should().Be(command.PersonalActivityId);
			result.Period.StartDateTime.Should().Be(command.StartTime);
			result.Period.EndDateTime.Should().Be(command.EndTime);
		}

		[Test]
		public void ShouldReportErrorIfActivityConflictsWithOvernightShiftsFromPreviousDay()
		{
			var yesterdayActivity = ActivityFactory.CreateActivity("yesterdayActivity");
			_activityRepository.Add(yesterdayActivity);

			var yesterdayActivityPeriod = new DateTimePeriod(new DateTime(2016, 05, 16, 19, 0, 0, DateTimeKind.Utc), new DateTime(2016, 05, 17, 09, 0, 0, DateTimeKind.Utc));
			var personAssignmentYesterday = new PersonAssignment(_personRepository.Single(), _scenario, _date.AddDays(-1));
			personAssignmentYesterday.AddActivity(yesterdayActivity, yesterdayActivityPeriod);

			var personAssignment = new PersonAssignment(_personRepository.Single(), _scenario, _date);
			personAssignment.AddPersonalActivity(_personalActivity, _personalActivityDateTimePeriod);
			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository
			{
				personAssignmentYesterday
			};

			personAssignmentRepository.Add(personAssignment);

			var currentScenario = new ThisCurrentScenario(personAssignmentRepository.First().Scenario);

			var command = new AddPersonalActivityCommand
			{
				PersonId = _personRepository.Single().Id.Value,
				Date = _date,
				PersonalActivityId = _personalActivity.Id.Value,
				StartTime = new DateTime(2016, 05, 17, 07, 0 , 0, DateTimeKind.Utc),
				EndTime = new DateTime(2016, 05, 17, 09, 0, 0, DateTimeKind.Utc),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = Guid.NewGuid()
				}
			};

			var target = new AddPersonalActivityCommandHandler(personAssignmentRepository, currentScenario, _activityRepository, _personRepository, new UtcTimeZone());

			target.Handle(command);

			command.ErrorMessages.Single().Should().Be(Resources.ActivityConflictsWithOvernightShiftsFromPreviousDay);
		}
	}
}
