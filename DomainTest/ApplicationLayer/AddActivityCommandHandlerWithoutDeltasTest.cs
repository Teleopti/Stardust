using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	[DomainTest]
	public class AddActivityCommandHandlerWithoutDeltasTest:ISetup
	{
		public AddActivityCommandHandlerWithoutDeltas Target;
		public FakeWriteSideRepository<IPerson> PersonRepository;
		public FakeWriteSideRepository<IActivity> ActivityRepository;
		public FakePersonAssignmentWriteSideRepository PersonAssignmentRepo;
		public FakeCurrentScenario CurrentScenario;
		public FakeScheduleStorage ScheduleStorage;
		public FakeUserTimeZone UserTimeZone;
		public FakeShiftCategoryRepository ShiftCategoryRepository;
		public FakeLoggedOnUser LoggedOnUser;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakePersonAssignmentWriteSideRepository>().For<IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey>>();
			system.UseTestDouble<FakeWriteSideRepository<IActivity>>().For<IProxyForId<IActivity>>();
			system.UseTestDouble<FakeWriteSideRepository<IPerson>>().For<IProxyForId<IPerson>>();
			system.UseTestDouble<FakeCurrentScenario>().For<ICurrentScenario>();
			system.UseTestDouble<FakeScheduleStorage>().For<IScheduleStorage>();
			system.UseTestDouble<AddActivityCommandHandlerWithoutDeltas>().For<IHandleCommand<AddActivityCommand>>();
			system.UseTestDouble<FakeUserTimeZone>().For<IUserTimeZone>();
			system.UseTestDouble<FakeShiftCategoryRepository>().For<IShiftCategoryRepository>();
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
		}

		[Test]
		public void ShouldRaiseActivityAddedEvent()
		{
			var scenario = CurrentScenario.Current();
			PersonRepository.Add(PersonFactory.CreatePersonWithId());
			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var addedActivity = ActivityFactory.CreateActivity("Added activity");
			ActivityRepository.Add(mainActivity);
			ActivityRepository.Add(addedActivity);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(PersonRepository.Single(),scenario, mainActivity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 16), ShiftCategoryFactory.CreateShiftCategory("regularShift"));
			PersonAssignmentRepo.Add(pa);

			var operatedPersonId = Guid.NewGuid();
			var trackId = Guid.NewGuid();
			var command = new AddActivityCommand
			{
				PersonId = PersonRepository.Single().Id.Value,
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
			Target.Handle(command);

			var @event = PersonAssignmentRepo.Single().PopAllEvents().OfType<ActivityAddedEvent>().Single(e => e.ActivityId == addedActivity.Id.Value);
			@event.PersonId.Should().Be(PersonRepository.Single().Id.Value);
			@event.Date.Should().Be(new DateTime(2013, 11, 14));
			@event.StartDateTime.Should().Be(command.StartTime);
			@event.EndDateTime.Should().Be(command.EndTime);
			@event.ScenarioId.Should().Be(PersonAssignmentRepo.Single().Scenario.Id.Value);
			@event.InitiatorId.Should().Be(operatedPersonId);
			@event.CommandId.Should().Be(trackId);
			@event.LogOnBusinessUnitId.Should().Be(scenario.BusinessUnit.Id.GetValueOrDefault());
		}

		
		[Test]
		public void ShouldSetupEntityState()
		{
			var scenario = CurrentScenario.Current();
			PersonRepository.Add(PersonFactory.CreatePersonWithId());
			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var addedActivity = ActivityFactory.CreateActivity("Added activity");
			ActivityRepository.Add(mainActivity);
			ActivityRepository.Add(addedActivity);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(PersonRepository.Single(), scenario, mainActivity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 16), ShiftCategoryFactory.CreateShiftCategory("regularShift"));
			PersonAssignmentRepo.Add(pa);


			var command = new AddActivityCommand
			{
				PersonId = PersonRepository.Single().Id.Value,
				Date = new DateOnly(2013, 11, 14),
				ActivityId = addedActivity.Id.Value,
				StartTime = new DateTime(2013, 11, 14, 14, 00, 00, DateTimeKind.Utc),
				EndTime = new DateTime(2013, 11, 14, 15, 00, 00, DateTimeKind.Utc)
			};
			Target.Handle(command);

			var addedLayer = PersonAssignmentRepo.Single().ShiftLayers.Last();
			addedLayer.Payload.Should().Be(addedActivity);
			addedLayer.Period.StartDateTime.Should().Be(command.StartTime);
			addedLayer.Period.EndDateTime.Should().Be(command.EndTime);
		}
		
		[Test]
		public void ShouldConvertTimesFromUsersTimeZone()
		{
			var scenario = CurrentScenario.Current();
			UserTimeZone.IsHawaii();
			PersonRepository.Add(PersonFactory.CreatePersonWithId());
			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var addedActivity = ActivityFactory.CreateActivity("Added activity");
			ActivityRepository.Add(mainActivity);
			ActivityRepository.Add(addedActivity);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(PersonRepository.Single(), scenario, mainActivity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 16), ShiftCategoryFactory.CreateShiftCategory("regularShift"));
			PersonAssignmentRepo.Add(pa);


			var command = new AddActivityCommand
			{
				PersonId = PersonRepository.Single().Id.Value,
				Date = new DateOnly(2013, 11, 14),
				ActivityId = addedActivity.Id.Value,
				StartTime = new DateTime(2013, 11, 14, 14, 00, 00),
				EndTime = new DateTime(2013, 11, 14, 15, 00, 00)
			};
			Target.Handle(command);

			var addedLayer = PersonAssignmentRepo.Single().ShiftLayers.Last();
			addedLayer.Period.StartDateTime.Should().Be(TimeZoneHelper.ConvertToUtc(command.StartTime, TimeZoneInfoFactory.HawaiiTimeZoneInfo()));
			addedLayer.Period.EndDateTime.Should().Be(TimeZoneHelper.ConvertToUtc(command.EndTime, TimeZoneInfoFactory.HawaiiTimeZoneInfo()));
		}
		
		[Test]
		public void ShouldCreateNewPersonAssignmentIfNotExists()
		{
			PersonRepository.Add(PersonFactory.CreatePersonWithId());
			var addedActivity = ActivityFactory.CreateActivity("Added activity");
			ActivityRepository.Add(addedActivity);
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day");
			ShiftCategoryRepository.Add(shiftCategory);


			PersonAssignmentRepo.Count().Should().Be.EqualTo(0);

			var command = new AddActivityCommand
			{
				PersonId = PersonRepository.Single().Id.Value,
				Date = new DateOnly(2013, 11, 14),
				ActivityId = addedActivity.Id.Value
			};
			Target.Handle(command);

			PersonAssignmentRepo.Count().Should().Be.EqualTo(1);
			PersonAssignmentRepo.Single().MainActivities().Single().Payload.Name.Should().Be("Added activity");
			PersonAssignmentRepo.Single().ShiftCategory.Should().Be.SameInstanceAs(shiftCategory);
		}
		
		[Test]
		public void ShouldCreateNewShiftCategoryForActivityAddedOnDayOff()
		{
			var date = new DateOnly(2013, 11, 14);
			var scenario = CurrentScenario.Current();
			PersonRepository.Add(PersonFactory.CreatePersonWithId());
			var addedActivity = ActivityFactory.CreateActivity("Added activity");
			ActivityRepository.Add(addedActivity);
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day");
			ShiftCategoryRepository.Add(shiftCategory);


			
			var dayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(PersonRepository.SingleOrDefault(), scenario, date, new DayOffTemplate());
			PersonAssignmentRepo.Add(dayOff);

			PersonAssignmentRepo.Count().Should().Be.EqualTo(1);
			PersonAssignmentRepo.SingleOrDefault().ShiftLayers.Count().Should().Be.EqualTo(0);

			var command = new AddActivityCommand
			{
				PersonId = PersonRepository.Single().Id.Value,
				Date = date,
				ActivityId = addedActivity.Id.Value
			};
			Target.Handle(command);

			PersonAssignmentRepo.Count().Should().Be.EqualTo(1);
			PersonAssignmentRepo.SingleOrDefault().MainActivities().Single().Payload.Name.Should().Be("Added activity");
			PersonAssignmentRepo.SingleOrDefault().ShiftCategory.Should().Be.SameInstanceAs(shiftCategory);
		}
		
		[Test]
		public void ShouldSetShiftCategoryWhenAddingActivityToDayOffWithPersonalActivity()
		{
			var date = new DateOnly(2016, 7, 26);
			var scenario = CurrentScenario.Current();
			PersonRepository.Add(PersonFactory.CreatePersonWithId());

			var activity = ActivityFactory.CreateActivity("an activity");
			ActivityRepository.Add(activity);
			var dayOffWithPersonalActivity = PersonAssignmentFactory.CreateAssignmentWithDayOff(PersonRepository.SingleOrDefault(), scenario, date, new DayOffTemplate());

			var personalActivity = ActivityFactory.CreateActivity("a personal activity");
			dayOffWithPersonalActivity.AddPersonalActivity(personalActivity, new DateTimePeriod(), true, null);
			PersonAssignmentRepo.Add(dayOffWithPersonalActivity);

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day");
			ShiftCategoryRepository.Add(shiftCategory);

			PersonAssignmentRepo.Count().Should().Be.EqualTo(1);
			PersonAssignmentRepo.SingleOrDefault().ShiftLayers.Count().Should().Be.EqualTo(1);


			var command = new AddActivityCommand
			{
				PersonId = PersonRepository.Single().Id.Value,
				Date = date,
				ActivityId = activity.Id.Value
			};

			Target.Handle(command);

			PersonAssignmentRepo.SingleOrDefault().MainActivities().Single().Payload.Name.Should().Be(activity.Name);
			PersonAssignmentRepo.SingleOrDefault().ShiftCategory.Should().Be.SameInstanceAs(shiftCategory);
		}
		
		[Test]
		public void ShouldReportErrorIfActivityConflictsWithOvernightShiftsFromPreviousDay()
		{
			var scenario = CurrentScenario.Current();
			PersonRepository.Add(PersonFactory.CreatePersonWithId());
			
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day");
			ShiftCategoryRepository.Add(shiftCategory);
			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var addedActivity = ActivityFactory.CreateActivity("Added activity");
			ActivityRepository.Add(addedActivity);
			ActivityRepository.Add(mainActivity);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(PersonRepository.Single(), scenario, mainActivity, new DateTimePeriod(2013, 11, 13, 23, 2013, 11, 14, 8), shiftCategory);
			PersonAssignmentRepo.Add(pa);


			var command = new AddActivityCommand
			{
				PersonId = PersonRepository.Single().Id.Value,
				Date = new DateOnly(2013, 11, 14),
				ActivityId = addedActivity.Id.Value,
				StartTime = new DateTime(2013, 11, 14, 5, 0, 0),
				EndTime = new DateTime(2013, 11, 14, 8, 0, 0)
			};
			Target.Handle(command);

			command.ErrorMessages.First().Should().Be.EqualTo(Resources.ActivityConflictsWithOvernightShiftsFromPreviousDay);
		}
				
		[Test]
		public void ShouldMoveConflictNonoverwritableLayerWhenItsFixable()
		{
			var scenario = CurrentScenario.Current();
			PersonRepository.Add(PersonFactory.CreatePersonWithId());
			
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day");
			ShiftCategoryRepository.Add(shiftCategory);
			var mainActivity = ActivityFactory.CreateActivity("Phone").WithId();
			var addedActivity = ActivityFactory.CreateActivity("Added activity").WithId();
			var lunchActivity = ActivityFactory.CreateActivity("Lunch").WithId();
			mainActivity.AllowOverwrite = true;
			lunchActivity.AllowOverwrite = false;
			ActivityRepository.Add(addedActivity);
			ActivityRepository.Add(mainActivity);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(PersonRepository.Single(), scenario, mainActivity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 16), shiftCategory);
			pa.AddActivity(lunchActivity, new DateTimePeriod(2013, 11, 14, 12, 2013, 11, 14,13));
			pa.ShiftLayers.ForEach(l => l.WithId());
			PersonAssignmentRepo.Add(pa);
			ScheduleStorage.Add(pa);


			var command = new AddActivityCommand
			{
				PersonId = PersonRepository.Single().Id.Value,
				Date = new DateOnly(2013, 11, 14),
				ActivityId = addedActivity.Id.Value,
				StartTime = new DateTime(2013, 11, 14, 12, 0, 0),
				EndTime = new DateTime(2013, 11, 14, 14, 0, 0),
				MoveConflictLayerAllowed = true
			};
			Target.Handle(command);

			var addedLayer = PersonAssignmentRepo.Single().ShiftLayers.Last();
			addedLayer.Payload.Should().Be(addedActivity);
			addedLayer.Period.StartDateTime.Should().Be(command.StartTime);
			addedLayer.Period.EndDateTime.Should().Be(command.EndTime);

			var movedLunchLayer = PersonAssignmentRepo.Single().ShiftLayers.First(l => l.Payload == lunchActivity);
			movedLunchLayer.Period.Should().Be(new DateTimePeriod(2013, 11, 14, 11, 2013, 11, 14, 12));
		}
		[Test]
		public void ShouldRaiseOneActivityAddedEventWithFixableConflictNonoverwritableLayer()
		{
			var scenario = CurrentScenario.Current();
			PersonRepository.Add(PersonFactory.CreatePersonWithId());

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day");
			ShiftCategoryRepository.Add(shiftCategory);
			var mainActivity = ActivityFactory.CreateActivity("Phone").WithId();
			var addedActivity = ActivityFactory.CreateActivity("Added activity").WithId();
			var lunchActivity = ActivityFactory.CreateActivity("Lunch").WithId();
			mainActivity.AllowOverwrite = true;
			lunchActivity.AllowOverwrite = false;
			ActivityRepository.Add(addedActivity);
			ActivityRepository.Add(mainActivity);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(PersonRepository.Single(), scenario, mainActivity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 16), shiftCategory);
			pa.AddActivity(lunchActivity, new DateTimePeriod(2013, 11, 14, 12, 2013, 11, 14, 13));
			pa.ShiftLayers.ForEach(l => l.WithId());
			PersonAssignmentRepo.Add(pa);
			ScheduleStorage.Add(pa);
			PersonAssignmentRepo.Single().PopAllEvents();
			var command = new AddActivityCommand
			{
				PersonId = PersonRepository.Single().Id.Value,
				Date = new DateOnly(2013, 11, 14),
				ActivityId = addedActivity.Id.Value,
				StartTime = new DateTime(2013, 11, 14, 12, 0, 0),
				EndTime = new DateTime(2013, 11, 14, 14, 0, 0),
				MoveConflictLayerAllowed = true
			};
			Target.Handle(command);
			var allEves = PersonAssignmentRepo.Single().PopAllEvents();
			allEves.Count().Should().Be(1);
			var @events = allEves.OfType<ActivityAddedEvent>().Where(e => e.ActivityId == addedActivity.Id.Value);
			@events.Count().Should().Be(1);
		}
		[Test]
		public void ShouldReturnWarningForConflictNonoverwritableLayerWhenItsNotFixable()
		{
			var scenario = CurrentScenario.Current();
			PersonRepository.Add(PersonFactory.CreatePersonWithId());
			
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day");
			ShiftCategoryRepository.Add(shiftCategory);
			var mainActivity = ActivityFactory.CreateActivity("Phone").WithId();
			var addedActivity = ActivityFactory.CreateActivity("Added activity").WithId();
			var lunchActivity = ActivityFactory.CreateActivity("Lunch").WithId();
			mainActivity.AllowOverwrite = true;
			lunchActivity.AllowOverwrite = false;
			ActivityRepository.Add(addedActivity);
			ActivityRepository.Add(mainActivity);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(PersonRepository.Single(), scenario, mainActivity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 15), shiftCategory);
			pa.AddActivity(lunchActivity, new DateTimePeriod(2013, 11, 14, 11, 2013, 11, 14,14));
			pa.ShiftLayers.ForEach(l => l.WithId());
			PersonAssignmentRepo.Add(pa);
			ScheduleStorage.Add(pa);


			var command = new AddActivityCommand
			{
				PersonId = PersonRepository.Single().Id.Value,
				Date = new DateOnly(2013, 11, 14),
				ActivityId = addedActivity.Id.Value,
				StartTime = new DateTime(2013, 11, 14, 9, 0, 0),
				EndTime = new DateTime(2013, 11, 14, 14, 0, 0),
				MoveConflictLayerAllowed = true
			};
			Target.Handle(command);

			var addedLayer = PersonAssignmentRepo.Single().ShiftLayers.Last();
			addedLayer.Payload.Should().Be(addedActivity);
			addedLayer.Period.StartDateTime.Should().Be(command.StartTime);
			addedLayer.Period.EndDateTime.Should().Be(command.EndTime);

			var movedLunchLayer = PersonAssignmentRepo.Single().ShiftLayers.First(l => l.Payload == lunchActivity);
			movedLunchLayer.Period.Should().Be(new DateTimePeriod(2013, 11, 14, 11, 2013, 11, 14, 14));
			command.WarningMessages.Count.Should().Be.EqualTo(1);
			command.WarningMessages.First().Should().Be(Resources.NewActivityOverlapsNonoverwritableActivities);
		}

	}
}