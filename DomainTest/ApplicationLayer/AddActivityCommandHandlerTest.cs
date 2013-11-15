using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	public class AddActivityCommandHandlerTest
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
			var target = new AddActivityCommandHandler(personAssignmentRepository,
								   new ThisCurrentScenario(personAssignmentRepository.Single().Scenario),
								   activityRepository, personRepository);

			var command = new AddActivityCommand
			{
				PersonId = personRepository.Single().Id.Value,
				Date = new DateOnly(2013, 11, 14),
				ActivityId = addedActivity.Id.Value,
				StartTime = new DateTime(2013, 11, 14, 14, 00, 00, DateTimeKind.Utc),
				EndTime = new DateTime(2013, 11, 14, 15, 00, 00, DateTimeKind.Utc),
			};
			target.Handle(command);

			var @event = personAssignmentRepository.Single().PopAllEvents().OfType<ActivityAddedEvent>().Single(e => e.ActivityId == addedActivity.Id.Value);
			@event.PersonId.Should().Be(personRepository.Single().Id.Value);
			@event.Date.Should().Be(new DateOnly(2013, 11, 14));
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
			var target = new AddActivityCommandHandler(personAssignmentRepository,
								   new ThisCurrentScenario(personAssignmentRepository.Single().Scenario), 
								   activityRepository, personRepository);

			var command = new AddActivityCommand
			{
				PersonId = personRepository.Single().Id.Value,
				Date = new DateOnly(2013, 11, 14),
				ActivityId = addedActivity.Id.Value,
				StartTime = new DateTime(2013, 11, 14, 14, 00, 00, DateTimeKind.Utc),
				EndTime = new DateTime(2013, 11, 14, 15, 00, 00, DateTimeKind.Utc),
			};
			target.Handle(command);

			var personAssignment = personAssignmentRepository.Single();
			var addedLayer = from l in personAssignment.ShiftLayers
			                 where l.Payload.Equals(addedActivity) &&
			                       l.Period.StartDateTime.Equals(command.StartTime) &&
			                       l.Period.EndDateTime.Equals(command.EndTime)
			                 select l;
			addedLayer.ToArray().Should().Have.Count.EqualTo(1);
		}
	}

	public class AddActivityCommandHandler : IHandleCommand<AddActivityCommand>
	{
		private readonly IProxyForId<IActivity> _activityForId;
		private readonly IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey> _personAssignmentRepository;
		private readonly ICurrentScenario _currentScenario;
		private readonly IProxyForId<IPerson> _personForId;

		public AddActivityCommandHandler(IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey> personAssignmentRepository, ICurrentScenario currentScenario, IProxyForId<IActivity> activityForId, IProxyForId<IPerson> personForId)
		{
			_activityForId = activityForId;
			_personAssignmentRepository = personAssignmentRepository;
			_currentScenario = currentScenario;
			_personForId = personForId;
		}

		public void Handle(AddActivityCommand command)
		{
			var activity = _activityForId.Load(command.ActivityId);
			var personAssignment = _personAssignmentRepository.LoadAggregate(new PersonAssignmentKey
				{
					Date = command.Date,
					Scenario = _currentScenario.Current(),
					Person = _personForId.Load(command.PersonId)
				});
			var period = new DateTimePeriod(TimeZoneInfo.ConvertTimeToUtc(command.StartTime), TimeZoneInfo.ConvertTimeToUtc(command.EndTime));

			personAssignment.AddMainLayer(activity, period);
		}
	}
}