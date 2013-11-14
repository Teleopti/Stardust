using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
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
			var person = PersonFactory.CreatePersonWithId();
			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var addingActivity = ActivityFactory.CreateActivity("Space out");
			var activityRepository = new FakeWriteSideRepository<IActivity> {mainActivity, addingActivity};
			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository
				{
					PersonAssignmentFactory.CreateAssignmentWithMainShift(
						mainActivity, person,
						new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 16),
						ShiftCategoryFactory.CreateShiftCategory("  "), ScenarioFactory.CreateScenarioAggregate(" ", true))
				};
			var target = new AddActivityCommandHandler(activityRepository, personAssignmentRepository);

			var command = new AddActivityCommand
				{
					PersonId = person.Id.Value,
					Date = new DateOnly(2013, 11, 14),
					ActivityId = addingActivity.Id.Value,
					StartTime = new DateTime(2013, 11, 14, 14, 00, 00),
					EndTime = new DateTime(2013, 11, 14, 15, 00, 00),
				};

			target.Handle(command);

			var @event = personAssignmentRepository.Single().PopAllEvents().OfType<ActivityAddedEvent>().Single(e => e.ActivityId == addingActivity.Id.Value);
			@event.PersonId.Should().Be(person.Id.Value);
			@event.Date.Should().Be(new DateOnly(2013, 11, 14));
		}

		[Test]
		public void ShouldSetupEntityState()
		{
			var person = PersonFactory.CreatePersonWithId();
			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var addingActivity = ActivityFactory.CreateActivity("Space out");
			var activityRepository = new FakeWriteSideRepository<IActivity> { mainActivity, addingActivity };
			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository
				{
					PersonAssignmentFactory.CreateAssignmentWithMainShift(
						mainActivity, person,
						new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 16),
						ShiftCategoryFactory.CreateShiftCategory("  "), ScenarioFactory.CreateScenarioAggregate(" ", true))
				};
			var target = new AddActivityCommandHandler(activityRepository, personAssignmentRepository);

			var command = new AddActivityCommand
			{
				PersonId = person.Id.Value,
				Date = new DateOnly(2013, 11, 14),
				ActivityId = addingActivity.Id.Value,
				StartTime = new DateTime(2013, 11, 14, 14, 00, 00, DateTimeKind.Utc),
				EndTime = new DateTime(2013, 11, 14, 15, 00, 00, DateTimeKind.Utc),
			};

			target.Handle(command);

			var personAssignment = personAssignmentRepository.Single();
			var addedLayer = from l in personAssignment.ShiftLayers
			                 where l.Payload.Equals(addingActivity) &&
			                       l.Period.StartDateTime.Equals(command.StartTime) &&
			                       l.Period.EndDateTime.Equals(command.EndTime)
			                 select l;
			addedLayer.ToArray().Should().Have.Count.EqualTo(1);
		}
	}

	public class AddActivityCommandHandler : IHandleCommand<AddActivityCommand>
	{
		private readonly IWriteSideRepository<IActivity> _activityRepository;
		private readonly IPersonAssignmentWriteSideRepository _personAssignmentRepository;

		public AddActivityCommandHandler(IWriteSideRepository<IActivity> activityRepository,
						 IPersonAssignmentWriteSideRepository personAssignmentRepository)
		{
			_activityRepository = activityRepository;
			_personAssignmentRepository = personAssignmentRepository;
		}

		public void Handle(AddActivityCommand command)
		{
			var activity = _activityRepository.Load(command.ActivityId);
			var personAssignment = _personAssignmentRepository.Load(command.PersonId, command.Date);
			var period = new DateTimePeriod(TimeZoneInfo.ConvertTimeToUtc(command.StartTime), TimeZoneInfo.ConvertTimeToUtc(command.EndTime));

			personAssignment.AddMainLayer(activity, period);
		}
	}
}