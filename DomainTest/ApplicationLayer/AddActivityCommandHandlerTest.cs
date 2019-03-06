using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[DomainTest]
	[NoDefaultData]
	public class AddActivityCommandHandlerTest : IIsolateSystem
	{
		public AddActivityCommandHandler Target;
		public FakePersonRepository PersonRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeSkillRepository SkillRepository;
		public FakeSkillCombinationResourceRepository SkillCombinationResourceRepository;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;
		public MutableNow Now;
		public FakeUserTimeZone UserTimeZone;
		public FakeShiftCategoryRepository ShiftCategoryRepository;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<AddActivityCommandHandler>().For<IHandleCommand<AddActivityCommand>>();
			isolate.UseTestDouble<ScheduleDayDifferenceSaver>().For<IScheduleDayDifferenceSaver>();
		}

		[Test]
		public void ShouldRaiseEventWhenActivityAdded()
		{
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);
			var activity = ActivityFactory.CreateActivity("Phone").WithId();
			var mainActivity = ActivityFactory.CreateActivity("mainActivity");
			ActivityRepository.Add(activity);
			ActivityRepository.Add(mainActivity);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, mainActivity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 16));
			personAssignment.ShiftLayers.ForEach(sl => sl.WithId());
			personAssignment.SetId(Guid.NewGuid());
			var operatedPersonId = Guid.NewGuid();
			var trackId = Guid.NewGuid();
			var command = new AddActivityCommand
			{
				Person = person,
				ActivityId = activity.Id.GetValueOrDefault(),
				StartTime = new DateTime(2013, 11, 14, 9, 0, 0),
				EndTime = new DateTime(2013, 11, 14, 10, 0, 0),
				Date = new DateOnly(2013, 11, 14),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = operatedPersonId,
					TrackId = trackId
				}
			};
			//to make the event list empty
			personAssignment.PopAllEvents(null);
			ScenarioRepository.Has(personAssignment.Scenario);
			PersonAssignmentRepository.Add(personAssignment);

			Target.Handle(command);
			
			personAssignment = PersonAssignmentRepository.LoadAll().Single();

			var @event = personAssignment.PopAllEvents(null).OfType<ActivityAddedEvent>().Single();
			@event.PersonId.Should().Be(person.Id.GetValueOrDefault());
			@event.Date.Should().Be(new DateTime(2013, 11, 14));
			@event.StartDateTime.Should().Be(command.StartTime);
			@event.EndDateTime.Should().Be(command.EndTime);
			@event.ScenarioId.Should().Be.EqualTo(personAssignment.Scenario.Id.GetValueOrDefault());
			@event.InitiatorId.Should().Be.EqualTo(operatedPersonId);
			@event.CommandId.Should().Be(trackId);
			@event.LogOnBusinessUnitId.Should().Be(personAssignment.Scenario.GetOrFillWithBusinessUnit_DONTUSE().Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldConvertTimesFromUsersTimeZone()
		{
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);
			var scenario = ScenarioRepository.Has("Default");
			UserTimeZone.IsHawaii();
			ShiftCategoryRepository.Add(ShiftCategoryFactory.CreateShiftCategory());
			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var addedActivity = ActivityFactory.CreateActivity("Added activity");
			ActivityRepository.Add(mainActivity);
			ActivityRepository.Add(addedActivity);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, mainActivity,
					new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 16), ShiftCategoryFactory.CreateShiftCategory("regularShift"))
				.WithId();
			
			PersonAssignmentRepository.Add(pa);
			
			var command = new AddActivityCommand
			{
				Person = person,
				Date = new DateOnly(2013, 11, 14),
				ActivityId = addedActivity.Id.Value,
				StartTime = new DateTime(2013, 11, 14, 14, 00, 00),
				EndTime = new DateTime(2013, 11, 14, 15, 00, 00)
			};
			Target.Handle(command);

			var personAssignment = PersonAssignmentRepository.LoadAll().Single();

			var addedLayer = personAssignment.ShiftLayers.Last();
			addedLayer.Period.StartDateTime.Should().Be(TimeZoneHelper.ConvertToUtc(command.StartTime, TimeZoneInfoFactory.HawaiiTimeZoneInfo()));
			addedLayer.Period.EndDateTime.Should().Be(TimeZoneHelper.ConvertToUtc(command.EndTime, TimeZoneInfoFactory.HawaiiTimeZoneInfo()));
		}

		[Test]
		public void ShouldCreateNewPersonAssignmentIfNotExists()
		{
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);
			ScenarioRepository.Has("Default");
			var addedActivity = ActivityFactory.CreateActivity("Added activity");
			ActivityRepository.Add(addedActivity);
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day");
			ShiftCategoryRepository.Add(shiftCategory);

			var command = new AddActivityCommand
			{
				Person = person,
				Date = new DateOnly(2013, 11, 14),
				ActivityId = addedActivity.Id.Value,
				StartTime = new DateTime(2013, 11, 14, 14, 00, 00),
				EndTime = new DateTime(2013, 11, 14, 15, 00, 00)
			};

			Target.Handle(command);

			var personAssignment = PersonAssignmentRepository.LoadAll().Single();
			
			personAssignment.MainActivities().Single().Payload.Name.Should().Be("Added activity");
			personAssignment.ShiftCategory.Should().Be.SameInstanceAs(shiftCategory);
		}

		[Test]
		public void ShouldCreateNewShiftCategoryForActivityAddedOnDayOff()
		{
			var date = new DateOnly(2013, 11, 14);
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);
			var scenario = ScenarioRepository.Has("Default");
			var addedActivity = ActivityFactory.CreateActivity("Added activity");
			ActivityRepository.Add(addedActivity);
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day");
			ShiftCategoryRepository.Add(shiftCategory);
			
			var dayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(person, scenario, date, new DayOffTemplate());
			PersonAssignmentRepository.Add(dayOff);
			
			var command = new AddActivityCommand
			{
				Person = person,
				Date = date,
				ActivityId = addedActivity.Id.Value,
				StartTime = new DateTime(2013, 11, 14, 14, 00, 00),
				EndTime = new DateTime(2013, 11, 14, 15, 00, 00)
			};
			Target.Handle(command);

			var personAssignment = PersonAssignmentRepository.LoadAll().Single();

			personAssignment.MainActivities().Single().Payload.Name.Should().Be("Added activity");
			personAssignment.ShiftCategory.Should().Be.SameInstanceAs(shiftCategory);
		}

		[Test]
		public void ShouldSetShiftCategoryWhenAddingActivityToDayOffWithPersonalActivity()
		{
			var date = new DateOnly(2016, 7, 26);
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);
			var scenario = ScenarioRepository.Has("Default");
			var activity = ActivityFactory.CreateActivity("an activity");
			ActivityRepository.Add(activity);
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day");
			ShiftCategoryRepository.Add(shiftCategory);
			var dayOffWithPersonalActivity = PersonAssignmentFactory.CreateAssignmentWithDayOff(person, scenario, date, new DayOffTemplate());

			var personalActivity = ActivityFactory.CreateActivity("a personal activity");
			dayOffWithPersonalActivity.AddPersonalActivity(personalActivity, new DateTimePeriod(2016, 7, 26, 8, 2016, 7, 26, 9), true, null);
			PersonAssignmentRepository.Add(dayOffWithPersonalActivity);
			
			var command = new AddActivityCommand
			{
				Person = person,
				Date = date,
				ActivityId = activity.Id.Value,
				StartTime = new DateTime(2016,7, 26, 14, 00, 00),
				EndTime = new DateTime(2016, 7, 26, 15, 00, 00)
			};

			Target.Handle(command);

			var personAssignment = PersonAssignmentRepository.LoadAll().Single();

			personAssignment.MainActivities().Single().Payload.Name.Should().Be(activity.Name);
			personAssignment.ShiftCategory.Should().Be.SameInstanceAs(shiftCategory);
		}

		[Test]
		public void ShouldReportErrorIfActivityConflictsWithOvernightShiftsFromPreviousDay()
		{
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);
			var scenario = ScenarioRepository.Has("Default");
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day");
			ShiftCategoryRepository.Add(shiftCategory);

			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var addedActivity = ActivityFactory.CreateActivity("Added activity");
			ActivityRepository.Add(addedActivity);
			ActivityRepository.Add(mainActivity);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, mainActivity, new DateTimePeriod(2013, 11, 13, 23, 2013, 11, 14, 8), shiftCategory);
			PersonAssignmentRepository.Add(pa);
			
			var command = new AddActivityCommand
			{
				Person = person,
				Date = new DateOnly(2013, 11, 14),
				ActivityId = addedActivity.Id.Value,
				StartTime = new DateTime(2013, 11, 14, 5, 0, 0),
				EndTime = new DateTime(2013, 11, 14, 8, 0, 0)
			};
			Target.Handle(command);

			command.ErrorMessages.First().Should().Be.EqualTo(Resources.ActivityConflictsWithOvernightShiftsFromPreviousDay);
		}

		[Test]
		public void ShouldNotReportErrorWhenAddingActivityFromZeroOclockAndPreviousDayIsEmpty()
		{
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);
			var scenario = ScenarioRepository.Has("Default");
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day");
			ShiftCategoryRepository.Add(shiftCategory);

			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var addedActivity = ActivityFactory.CreateActivity("Added activity");
			ActivityRepository.Add(addedActivity);
			ActivityRepository.Add(mainActivity);
			var pa = PersonAssignmentFactory.CreateEmptyAssignment(person, scenario, new DateTimePeriod(2013, 11, 13, 0, 2013, 11, 14, 0));
			PersonAssignmentRepository.Add(pa);

			var command = new AddActivityCommand
			{
				Person = person,
				Date = new DateOnly(2013, 11, 14),
				ActivityId = addedActivity.Id.Value,
				StartTime = new DateTime(2013, 11, 14, 0, 0, 0),
				EndTime = new DateTime(2013, 11, 14, 8, 0, 0)
			};
			Target.Handle(command);

			command.ErrorMessages.Should().Be.Empty();
		}

		[Test]
		public void ShouldMoveConflictNonoverwritableLayerWhenItsFixable()
		{
			var scenario = ScenarioRepository.Has("Default");
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day");
			ShiftCategoryRepository.Add(shiftCategory);
			var mainActivity = ActivityFactory.CreateActivity("Phone").WithId();
			var addedActivity = ActivityFactory.CreateActivity("Added activity").WithId();
			var lunchActivity = ActivityFactory.CreateActivity("Lunch").WithId();
			mainActivity.AllowOverwrite = true;
			lunchActivity.AllowOverwrite = false;
			ActivityRepository.Add(addedActivity);
			ActivityRepository.Add(mainActivity);
			ActivityRepository.Add(lunchActivity);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, mainActivity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 16), shiftCategory);
			pa.AddActivity(lunchActivity, new DateTimePeriod(2013, 11, 14, 12, 2013, 11, 14, 13));
			pa.ShiftLayers.ForEach(l => l.WithId());
			PersonAssignmentRepository.Add(pa);
			
			var command = new AddActivityCommand
			{
				Person = person,
				Date = new DateOnly(2013, 11, 14),
				ActivityId = addedActivity.Id.Value,
				StartTime = new DateTime(2013, 11, 14, 12, 0, 0),
				EndTime = new DateTime(2013, 11, 14, 14, 0, 0),
				MoveConflictLayerAllowed = true
			};
			Target.Handle(command);

			var personAssignment = PersonAssignmentRepository.LoadAll().Single();

			var addedLayer = personAssignment.ShiftLayers.Last();
			addedLayer.Payload.Should().Be(addedActivity);
			addedLayer.Period.StartDateTime.Should().Be(command.StartTime);
			addedLayer.Period.EndDateTime.Should().Be(command.EndTime);

			var movedLunchLayer = personAssignment.ShiftLayers.First(l => l.Payload == lunchActivity);
			movedLunchLayer.Period.Should().Be(new DateTimePeriod(2013, 11, 14, 11, 2013, 11, 14, 12));
		}

		[Test]
		public void ShouldRaiseOneActivityAddedEventWithFixableConflictNonoverwritableLayer()
		{
			var scenario = ScenarioRepository.Has("Default");
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day");
			ShiftCategoryRepository.Add(shiftCategory);
			var mainActivity = ActivityFactory.CreateActivity("Phone").WithId();
			var addedActivity = ActivityFactory.CreateActivity("Added activity").WithId();
			var lunchActivity = ActivityFactory.CreateActivity("Lunch").WithId();
			mainActivity.AllowOverwrite = true;
			lunchActivity.AllowOverwrite = false;
			ActivityRepository.Add(addedActivity);
			ActivityRepository.Add(mainActivity);
			ActivityRepository.Add(lunchActivity);
			
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, mainActivity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 16), shiftCategory);
			pa.AddActivity(lunchActivity, new DateTimePeriod(2013, 11, 14, 12, 2013, 11, 14, 13));
			pa.ShiftLayers.ForEach(l => l.WithId());
			pa.PopAllEvents(null);
			PersonAssignmentRepository.Add(pa);
			var command = new AddActivityCommand
			{
				Person = person,
				Date = new DateOnly(2013, 11, 14),
				ActivityId = addedActivity.Id.Value,
				StartTime = new DateTime(2013, 11, 14, 12, 0, 0),
				EndTime = new DateTime(2013, 11, 14, 14, 0, 0),
				MoveConflictLayerAllowed = true
			};
			Target.Handle(command);

			var personAssignment = PersonAssignmentRepository.LoadAll().Single();

			var allEves = personAssignment.PopAllEvents(null);
			allEves.Count().Should().Be(1);
			var @events = allEves.OfType<ActivityAddedEvent>().Where(e => e.ActivityId == addedActivity.Id.Value);
			@events.Count().Should().Be(1);
		}

		[Test]
		public void ShouldReturnWarningForConflictNonoverwritableLayerWhenItsNotFixable()
		{
			var scenario = ScenarioRepository.Has("Default");
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day");
			ShiftCategoryRepository.Add(shiftCategory);
			var mainActivity = ActivityFactory.CreateActivity("Phone").WithId();
			var addedActivity = ActivityFactory.CreateActivity("Added activity").WithId();
			var lunchActivity = ActivityFactory.CreateActivity("Lunch").WithId();
			mainActivity.AllowOverwrite = true;
			lunchActivity.AllowOverwrite = false;
			ActivityRepository.Add(addedActivity);
			ActivityRepository.Add(mainActivity);
			ActivityRepository.Add(lunchActivity);
			
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, mainActivity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 15), shiftCategory);
			pa.AddActivity(lunchActivity, new DateTimePeriod(2013, 11, 14, 11, 2013, 11, 14, 14));
			pa.ShiftLayers.ForEach(l => l.WithId());
			PersonAssignmentRepository.Add(pa);
			
			var command = new AddActivityCommand
			{
				Person = person,
				Date = new DateOnly(2013, 11, 14),
				ActivityId = addedActivity.Id.Value,
				StartTime = new DateTime(2013, 11, 14, 9, 0, 0),
				EndTime = new DateTime(2013, 11, 14, 14, 0, 0),
				MoveConflictLayerAllowed = true
			};
			Target.Handle(command);

			var personAssignment = PersonAssignmentRepository.LoadAll().Single();

			var addedLayer = personAssignment.ShiftLayers.Last();
			addedLayer.Payload.Should().Be(addedActivity);
			addedLayer.Period.StartDateTime.Should().Be(command.StartTime);
			addedLayer.Period.EndDateTime.Should().Be(command.EndTime);

			var movedLunchLayer = personAssignment.ShiftLayers.First(l => l.Payload == lunchActivity);
			movedLunchLayer.Period.Should().Be(new DateTimePeriod(2013, 11, 14, 11, 2013, 11, 14, 14));
			command.WarningMessages.Count.Should().Be.EqualTo(1);
			command.WarningMessages.First().Should().Be(Resources.NewActivityOverlapsNonoverwritableActivities);
		}

		[Test]
		public void ShouldAddResourcesWhenActivityIsAdded()
		{
			Now.Is(new DateTime(2013, 11, 14, 0, 0, 0, DateTimeKind.Utc));
			IntervalLengthFetcher.Has(15);
			var activity = ActivityFactory.CreateActivity("Phone").WithId();
			var mainActivity = ActivityFactory.CreateActivity("mainActivity");
			ActivityRepository.Add(activity);
			ActivityRepository.Add(mainActivity);
			var skill1 = SkillFactory.CreateSkillWithId("skill1", 15);
			var skill2 = SkillFactory.CreateSkillWithId("skill2", 15);
			SkillRepository.Add(skill1);
			SkillRepository.Add(skill2);
			skill1.Activity = activity;
			skill2.Activity = mainActivity;
			var person = PersonRepository.Has(skill1, skill2);
			activity.RequiresSkill = true;
			mainActivity.RequiresSkill = true;
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, mainActivity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 16));
			personAssignment.ShiftLayers.ForEach(sl => sl.WithId());
			personAssignment.SetId(Guid.NewGuid());
			var operatedPersonId = Guid.NewGuid();
			var trackId = Guid.NewGuid();
			var command = new AddActivityCommand
			{
				Person = person,
				ActivityId = activity.Id.GetValueOrDefault(),
				StartTime = new DateTime(2013, 11, 14, 9, 0, 0),
				EndTime = new DateTime(2013, 11, 14, 10, 0, 0),
				Date = new DateOnly(2013, 11, 14),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = operatedPersonId,
					TrackId = trackId
				}
			};
			ScenarioRepository.Has(personAssignment.Scenario);
			PersonAssignmentRepository.Add(personAssignment);

			Target.Handle(command);

			var resources = SkillCombinationResourceRepository.LoadSkillCombinationResources(new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 16),false);
			resources.Count().Should().Be.EqualTo(8);
			resources.Count(x => x.Resource == -1).Should().Be.EqualTo(4);
			resources.Count(x => x.Resource == 1).Should().Be.EqualTo(4);

			
		}

		[Test]
		public void ShouldAddResourcesOnlyForActivityThatRequiresSkill()
		{
			Now.Is(new DateTime(2013, 11, 14, 0, 0, 0, DateTimeKind.Utc));
			IntervalLengthFetcher.Has(15);
			var activity = ActivityFactory.CreateActivity("Phone").WithId();
			var mainActivity = ActivityFactory.CreateActivity("mainActivity");
			ActivityRepository.Add(activity);
			ActivityRepository.Add(mainActivity);
			var skill1 = SkillFactory.CreateSkillWithId("skill1", 15);
			var skill2 = SkillFactory.CreateSkillWithId("skill2", 15);
			SkillRepository.Add(skill1);
			SkillRepository.Add(skill2);
			skill1.Activity = activity;
			skill2.Activity = mainActivity;
			var person = PersonRepository.Has(skill1, skill2);
			activity.RequiresSkill = true;
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, mainActivity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 16));
			personAssignment.ShiftLayers.ForEach(sl => sl.WithId());
			personAssignment.SetId(Guid.NewGuid());
			var operatedPersonId = Guid.NewGuid();
			var trackId = Guid.NewGuid();
			var command = new AddActivityCommand
			{
				Person = person,
				ActivityId = activity.Id.GetValueOrDefault(),
				StartTime = new DateTime(2013, 11, 14, 9, 0, 0),
				EndTime = new DateTime(2013, 11, 14, 10, 0, 0),
				Date = new DateOnly(2013, 11, 14),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = operatedPersonId,
					TrackId = trackId
				}
			};
			ScenarioRepository.Has(personAssignment.Scenario);
			PersonAssignmentRepository.Add(personAssignment);

			Target.Handle(command);

			var resources = SkillCombinationResourceRepository.LoadSkillCombinationResources(new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 16), false);
			resources.Count().Should().Be.EqualTo(4);
			resources.Count(x => x.Resource == 1).Should().Be.EqualTo(4);
			resources.Any(x => x.StartDateTime == new DateTime(2013, 11, 14, 9, 0, 0)).Should().Be.True();
			resources.Any(x => x.StartDateTime == new DateTime(2013, 11, 14, 9, 15, 0)).Should().Be.True();
			resources.Any(x => x.StartDateTime == new DateTime(2013, 11, 14, 9, 30, 0)).Should().Be.True();
			resources.Any(x => x.StartDateTime == new DateTime(2013, 11, 14, 9, 45, 0)).Should().Be.True();
		}

	}
}