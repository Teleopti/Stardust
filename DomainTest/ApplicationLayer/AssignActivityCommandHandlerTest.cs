using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	public class AssignActivityCommandHandlerTest
	{
		[Test]
		public void ShouldRaiseActivityAddedEvent()
		{
			var personRepository = new FakeWriteSideRepository<IPerson> { PersonFactory.CreatePersonWithId() };
			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var addedActivity = ActivityFactory.CreateActivity("Added activity");
			var activityRepository = new FakeWriteSideRepository<IActivity> { mainActivity, addedActivity };
			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository
				{
					PersonAssignmentFactory.CreateAssignmentWithMainShift(
						mainActivity, personRepository.Single(),
						new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 16))
				};
			var target = new AssignActivityCommandHandler(personAssignmentRepository,
								   new ThisCurrentScenario(personAssignmentRepository.Single().Scenario),
								   activityRepository, personRepository, new UtcTimeZone());

			var command = new AssignActivityCommand
			{
				PersonId = personRepository.Single().Id.Value,
				Date = new DateOnly(2013, 11, 14),
				ActivityId = addedActivity.Id.Value,
				StartTime = new DateTime(2013, 11, 14, 14, 00, 00, DateTimeKind.Utc),
				EndTime = new DateTime(2013, 11, 14, 15, 00, 00, DateTimeKind.Utc),
			};
			target.Handle(command);

			var @event = personAssignmentRepository.Single().PopAllEvents().OfType<ActivityAssignedEvent>().Single(e => e.ActivityId == addedActivity.Id.Value);
			@event.PersonId.Should().Be(personRepository.Single().Id.Value);
			@event.Date.Should().Be(new DateOnly(2013, 11, 14));
			@event.StartDateTime.Should().Be(command.StartTime);
			@event.EndDateTime.Should().Be(command.EndTime);
			@event.ScenarioId.Should().Be(personAssignmentRepository.Single().Scenario.Id.Value);
		}

		[Test]
		public void ShouldSetupEntityState()
		{
			var personRepository = new FakeWriteSideRepository<IPerson> { PersonFactory.CreatePersonWithId() };
			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var addedActivity = ActivityFactory.CreateActivity("Added activity");
			var activityRepository = new FakeWriteSideRepository<IActivity> { mainActivity, addedActivity };
			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository
				{
					PersonAssignmentFactory.CreateAssignmentWithMainShift(
						mainActivity, personRepository.Single(),
						new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 16))
				};
			var target = new AssignActivityCommandHandler(personAssignmentRepository,
								   new ThisCurrentScenario(personAssignmentRepository.Single().Scenario), 
								   activityRepository, personRepository, new UtcTimeZone());

			var command = new AssignActivityCommand
			{
				PersonId = personRepository.Single().Id.Value,
				Date = new DateOnly(2013, 11, 14),
				ActivityId = addedActivity.Id.Value,
				StartTime = new DateTime(2013, 11, 14, 14, 00, 00, DateTimeKind.Utc),
				EndTime = new DateTime(2013, 11, 14, 15, 00, 00, DateTimeKind.Utc),
			};
			target.Handle(command);

			var addedLayer = personAssignmentRepository.Single().ShiftLayers.Last();
			addedLayer.Payload.Should().Be(addedActivity);
			addedLayer.Period.StartDateTime.Should().Be(command.StartTime);
			addedLayer.Period.EndDateTime.Should().Be(command.EndTime);
		}

		[Test]
		public void ShouldConvertTimesFromUsersTimeZone()
		{
			var personRepository = new FakeWriteSideRepository<IPerson> { PersonFactory.CreatePersonWithId() };
			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var addedActivity = ActivityFactory.CreateActivity("Added activity");
			var activityRepository = new FakeWriteSideRepository<IActivity> { mainActivity, addedActivity };
			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository
				{
					PersonAssignmentFactory.CreateAssignmentWithMainShift(
						mainActivity, personRepository.Single(),
						new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 16))
				};
			var target = new AssignActivityCommandHandler(personAssignmentRepository,
								   new ThisCurrentScenario(personAssignmentRepository.Single().Scenario),
								   activityRepository, personRepository,
								   new SpecificTimeZone(TimeZoneInfoFactory.HawaiiTimeZoneInfo())
								   );

			var command = new AssignActivityCommand
			{
				PersonId = personRepository.Single().Id.Value,
				Date = new DateOnly(2013, 11, 14),
				ActivityId = addedActivity.Id.Value,
				StartTime = new DateTime(2013, 11, 14, 14, 00, 00),
				EndTime = new DateTime(2013, 11, 14, 15, 00, 00),
			};
			target.Handle(command);

			var addedLayer = personAssignmentRepository.Single().ShiftLayers.Last();
			addedLayer.Period.StartDateTime.Should().Be(TimeZoneHelper.ConvertToUtc(command.StartTime, TimeZoneInfoFactory.HawaiiTimeZoneInfo()));
			addedLayer.Period.EndDateTime.Should().Be(TimeZoneHelper.ConvertToUtc(command.EndTime, TimeZoneInfoFactory.HawaiiTimeZoneInfo()));
		}

	}

	public class AssignActivityCommandHandler : IHandleCommand<AssignActivityCommand>
	{
		private readonly IProxyForId<IActivity> _activityForId;
		private readonly IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey> _personAssignmentRepository;
		private readonly ICurrentScenario _currentScenario;
		private readonly IProxyForId<IPerson> _personForId;
		private readonly IUserTimeZone _timeZone;

		public AssignActivityCommandHandler(
			IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey> personAssignmentRepository,
			ICurrentScenario currentScenario, IProxyForId<IActivity> activityForId, IProxyForId<IPerson> personForId,
			IUserTimeZone timeZone)
		{
			_activityForId = activityForId;
			_personAssignmentRepository = personAssignmentRepository;
			_currentScenario = currentScenario;
			_personForId = personForId;
			_timeZone = timeZone;
		}

		public void Handle(AssignActivityCommand command)
		{
			var activity = _activityForId.Load(command.ActivityId);
			var personAssignment = _personAssignmentRepository.LoadAggregate(new PersonAssignmentKey
				{
					Date = command.Date,
					Scenario = _currentScenario.Current(),
					Person = _personForId.Load(command.PersonId)
				});
			var period = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(command.StartTime, _timeZone.TimeZone()), TimeZoneHelper.ConvertToUtc(command.EndTime, _timeZone.TimeZone()));

			personAssignment.AssignActivity(activity, period);
		}
	}
}