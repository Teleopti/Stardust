﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.UserTexts;
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
			var scenario = new ThisCurrentScenario(personAssignmentRepository.Single().Scenario);
			var target = new AddActivityCommandHandler(personAssignmentRepository,
								   scenario,
								   activityRepository, personRepository, new UtcTimeZone(), null);

			var operatedPersonId = Guid.NewGuid();
			var trackId = Guid.NewGuid();
			var command = new AddActivityCommand
			{
				PersonId = personRepository.Single().Id.Value,
				Date = new DateOnly(2013, 11, 14),
				ActivityId = addedActivity.Id.Value,
				StartTime = new DateTime(2013, 11, 14, 14, 00, 00, DateTimeKind.Utc),
				EndTime = new DateTime(2013, 11, 14, 15, 00, 00, DateTimeKind.Utc),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = operatedPersonId,
					TrackId = trackId
				}
			};
			target.Handle(command);

			var @event = personAssignmentRepository.Single().PopAllEvents(new Now()).OfType<ActivityAddedEvent>().Single(e => e.ActivityId == addedActivity.Id.Value);
			@event.PersonId.Should().Be(personRepository.Single().Id.Value);
			@event.Date.Should().Be(new DateTime(2013, 11, 14));
			@event.StartDateTime.Should().Be(command.StartTime);
			@event.EndDateTime.Should().Be(command.EndTime);
			@event.ScenarioId.Should().Be(personAssignmentRepository.Single().Scenario.Id.Value);
			@event.InitiatorId.Should().Be(operatedPersonId);
			@event.CommandId.Should().Be(trackId);
			@event.LogOnBusinessUnitId.Should().Be(scenario.Current().BusinessUnit.Id.GetValueOrDefault());
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
								   activityRepository, personRepository, new UtcTimeZone(), null);

			var command = new AddActivityCommand
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
			var target = new AddActivityCommandHandler(personAssignmentRepository,
								   new ThisCurrentScenario(personAssignmentRepository.Single().Scenario),
								   activityRepository, personRepository,
								   new SpecificTimeZone(TimeZoneInfoFactory.HawaiiTimeZoneInfo()), null);

			var command = new AddActivityCommand
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

		[Test]
		public void ShouldCreateNewPersonAssignmentIfNotExists()
		{
			var personRepository = new FakeWriteSideRepository<IPerson> { PersonFactory.CreatePersonWithId() };
			var addedActivity = ActivityFactory.CreateActivity("Added activity");
			var activityRepository = new FakeWriteSideRepository<IActivity> { addedActivity };
			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository();
			var shiftCategoryRepository = MockRepository.GenerateMock<IShiftCategoryRepository>();
			var shiftCategory = new ShiftCategory("Day");
			shiftCategoryRepository.Stub(x => x.FindAll()).Return(new List<IShiftCategory> {shiftCategory});

			var target = new AddActivityCommandHandler(personAssignmentRepository,
								   new ThisCurrentScenario(ScenarioFactory.CreateScenarioWithId("scenario", true)),
								   activityRepository, personRepository,
								   new UtcTimeZone(), shiftCategoryRepository
								   );

			personAssignmentRepository.Count().Should().Be.EqualTo(0);

			var command = new AddActivityCommand
			{
				PersonId = personRepository.Single().Id.Value,
				Date = new DateOnly(2013, 11, 14),
				ActivityId = addedActivity.Id.Value
			};
			target.Handle(command);

			personAssignmentRepository.Count().Should().Be.EqualTo(1);
			personAssignmentRepository.Single().MainActivities().Single().Payload.Name.Should().Be("Added activity");
			personAssignmentRepository.Single().ShiftCategory.Should().Be.SameInstanceAs(shiftCategory);
		}

		[Test]
		public void ShouldReportErrorIfActivityConflictsWithOvernightShiftsFromPreviousDay()
		{
			var personRepository = new FakeWriteSideRepository<IPerson> { PersonFactory.CreatePersonWithId() };
			var addedActivity = ActivityFactory.CreateActivity("Added activity");
			var activityRepository = new FakeWriteSideRepository<IActivity> { addedActivity };			
			var shiftCategoryRepository = MockRepository.GenerateMock<IShiftCategoryRepository>();
			var shiftCategory = new ShiftCategory("Day");
			shiftCategoryRepository.Stub(x => x.FindAll()).Return(new List<IShiftCategory> { shiftCategory });

			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				mainActivity, personRepository.Single(),
				new DateTimePeriod(2013,11,13,23,2013,11,14,8));

			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository { pa };

			var target = new AddActivityCommandHandler(personAssignmentRepository,
								   new ThisCurrentScenario(personAssignmentRepository.Single().Scenario),
								   activityRepository,personRepository,
								   new UtcTimeZone(),shiftCategoryRepository
								   );
			
			var command = new AddActivityCommand
			{
				PersonId = personRepository.Single().Id.Value,
				Date = new DateOnly(2013,11,14),
				ActivityId = addedActivity.Id.Value,
				StartTime = new DateTime(2013, 11, 14, 5, 0, 0),
				EndTime = new DateTime(2013,11,14,8,0,0),
			};
			target.Handle(command);

			command.ErrorMessages.First().Should().Be.EqualTo(Resources.ActivityConflictsWithOvernightShiftsFromPreviousDay);
		}


	}
}