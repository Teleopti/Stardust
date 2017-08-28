using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;
using EntityExtensions = Teleopti.Ccc.TestCommon.EntityExtensions;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	[DomainTest]
	[EnabledBy(Toggles.Staffing_ReadModel_BetterAccuracy_Step4_43389)]
	public class AddActivityCommandHandlerTest : ISetup
	{
		public AddActivityCommandHandler Target;
		public FakeScheduleStorage ScheduleStorage;
		public FakePersonRepository PersonRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeWriteSideRepository<IPerson> PersonRepository2;
		public FakeCurrentScenario CurrentScenario;
		public FakeSkillCombinationResourceRepository SkillCombinationResourceRepository;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;
		public FakePersonSkillProvider PersonSkillProvider;
		public MutableNow Now;
		public FakeWriteSideRepository<IActivity> ActivityRepository2;
		public FakeUserTimeZone UserTimeZone;
		public FakeShiftCategoryRepository ShiftCategoryRepository;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<AddActivityCommandHandler>().For<IHandleCommand<AddActivityCommand>>();
			system.UseTestDouble<FakeScheduleStorage>().For<IScheduleStorage>();
			system.UseTestDouble<FakeScheduleDifferenceSaver>().For<IScheduleDifferenceSaver>();
			system.UseTestDouble<ScheduleDayDifferenceSaver>().For<IScheduleDayDifferenceSaver>();
			system.UseTestDouble<FakeCurrentScenario>().For<ICurrentScenario>();
			system.UseTestDouble<FakeUserTimeZone>().For<IUserTimeZone>();
			system.UseTestDouble<FakeSkillCombinationResourceRepository>().For<ISkillCombinationResourceRepository>();
			system.UseTestDouble<FakePersonSkillProvider>().For<IPersonSkillProvider>();
			system.UseTestDouble<FakeShiftCategoryRepository>().For<IShiftCategoryRepository>();
			system.UseTestDouble<FakeWriteSideRepository<IPerson>>().For<IProxyForId<IPerson>>();

			system.UseTestDouble<FakeWriteSideRepository<IActivity>>().For<IProxyForId<IActivity>>();
		}

		[Test]
		public void ShouldRaiseEventWhenActivityAdded()
		{
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);
			PersonRepository2.Add(person);
			var activity = ActivityFactory.CreateActivity("Phone").WithId();
			var mainActivity = ActivityFactory.CreateActivity("mainActivity");
			ActivityRepository.Add(activity);
			ActivityRepository.Add(mainActivity);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, mainActivity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 16));
			ActivityRepository2.Add(activity);
			ActivityRepository2.Add(mainActivity);
			personAssignment.ShiftLayers.ForEach(sl => EntityExtensions.WithId<ShiftLayer>(sl));
			personAssignment.SetId(Guid.NewGuid());
			var operatedPersonId = Guid.NewGuid();
			var trackId = Guid.NewGuid();
			var command = new AddActivityCommand
			{
				PersonId = person.Id.GetValueOrDefault(),
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
			personAssignment.PopAllEvents();
			CurrentScenario.FakeScenario(personAssignment.Scenario);
			ScheduleStorage.Add(personAssignment);

			Target.Handle(command);

			var dic = ScheduleStorage.FindSchedulesForPersons(new ScheduleDateTimePeriod(new DateTimePeriod(command.Date.Date.Utc(), command.Date.Date.Utc())), personAssignment.Scenario, new PersonProvider(new[] { person }), new ScheduleDictionaryLoadOptions(false, false), new[] { person });
			var scheduleRange = dic[person];
			var scheduleDay = scheduleRange.ScheduledDay(command.Date);
			personAssignment = scheduleDay.PersonAssignment();

			var @event = personAssignment.PopAllEvents().OfType<ActivityAddedEvent>().Single();
			@event.PersonId.Should().Be(person.Id.GetValueOrDefault());
			@event.Date.Should().Be(new DateTime(2013, 11, 14));
			@event.StartDateTime.Should().Be(command.StartTime);
			@event.EndDateTime.Should().Be(command.EndTime);
			@event.ScenarioId.Should().Be.EqualTo(personAssignment.Scenario.Id.GetValueOrDefault());
			@event.InitiatorId.Should().Be.EqualTo(operatedPersonId);
			@event.CommandId.Should().Be(trackId);
			@event.LogOnBusinessUnitId.Should().Be(personAssignment.Scenario.BusinessUnit.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldConvertTimesFromUsersTimeZone()
		{
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);
			PersonRepository2.Add(person);
			var scenario = CurrentScenario.Current();
			CurrentScenario.FakeScenario(scenario);
			UserTimeZone.IsHawaii();
			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var addedActivity = ActivityFactory.CreateActivity("Added activity");
			ActivityRepository.Add(mainActivity);
			ActivityRepository.Add(addedActivity);
			ActivityRepository2.Add(mainActivity);
			ActivityRepository2.Add(addedActivity);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, mainActivity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 16), ShiftCategoryFactory.CreateShiftCategory("regularShift"));


			ScheduleStorage.Add(pa);


			var command = new AddActivityCommand
			{
				PersonId = person.Id.GetValueOrDefault(),
				Date = new DateOnly(2013, 11, 14),
				ActivityId = addedActivity.Id.Value,
				StartTime = new DateTime(2013, 11, 14, 14, 00, 00),
				EndTime = new DateTime(2013, 11, 14, 15, 00, 00)
			};
			Target.Handle(command);

			var dic = ScheduleStorage.FindSchedulesForPersons(new ScheduleDateTimePeriod(new DateTimePeriod(command.Date.Date.Utc(), command.Date.Date.Utc())), pa.Scenario, new PersonProvider(new[] { person }), new ScheduleDictionaryLoadOptions(false, false), new[] { person });
			var scheduleRange = dic[person];
			var scheduleDay = scheduleRange.ScheduledDay(command.Date);
			var personAssignment = scheduleDay.PersonAssignment();

			var addedLayer = personAssignment.ShiftLayers.Last();
			addedLayer.Period.StartDateTime.Should().Be(TimeZoneHelper.ConvertToUtc(command.StartTime, TimeZoneInfoFactory.HawaiiTimeZoneInfo()));
			addedLayer.Period.EndDateTime.Should().Be(TimeZoneHelper.ConvertToUtc(command.EndTime, TimeZoneInfoFactory.HawaiiTimeZoneInfo()));
		}

		[Test]
		public void ShouldCreateNewPersonAssignmentIfNotExists()
		{
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);
			PersonRepository2.Add(person);
			var scenario = CurrentScenario.Current();
			CurrentScenario.FakeScenario(scenario);
			var addedActivity = ActivityFactory.CreateActivity("Added activity");
			ActivityRepository.Add(addedActivity);
			ActivityRepository2.Add(addedActivity);
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day");
			ShiftCategoryRepository.Add(shiftCategory);

			var command = new AddActivityCommand
			{
				PersonId = person.Id.Value,
				Date = new DateOnly(2013, 11, 14),
				ActivityId = addedActivity.Id.Value,
				StartTime = new DateTime(2013, 11, 14, 14, 00, 00),
				EndTime = new DateTime(2013, 11, 14, 15, 00, 00)
			};

			Target.Handle(command);


			var dic = ScheduleStorage.FindSchedulesForPersons(new ScheduleDateTimePeriod(new DateTimePeriod(command.Date.Date.Utc(), command.Date.Date.Utc())), scenario, new PersonProvider(new[] { person }), new ScheduleDictionaryLoadOptions(false, false), new[] { person });
			var scheduleRange = dic[person];
			var scheduleDay = scheduleRange.ScheduledDay(command.Date);
			var personAssignment = scheduleDay.PersonAssignment();

			//PersonAssignmentRepo.Count().Should().Be.EqualTo(1);
			personAssignment.MainActivities().Single().Payload.Name.Should().Be("Added activity");
			personAssignment.ShiftCategory.Should().Be.SameInstanceAs(shiftCategory);
		}

		[Test]
		public void ShouldCreateNewShiftCategoryForActivityAddedOnDayOff()
		{
			var date = new DateOnly(2013, 11, 14);
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);
			PersonRepository2.Add(person);
			var scenario = CurrentScenario.Current();
			CurrentScenario.FakeScenario(scenario);
			var addedActivity = ActivityFactory.CreateActivity("Added activity");
			ActivityRepository.Add(addedActivity);
			ActivityRepository2.Add(addedActivity);
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day");
			ShiftCategoryRepository.Add(shiftCategory);



			var dayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(person, scenario, date, new DayOffTemplate());
			//PersonAssignmentRepo.Add(dayOff);
			ScheduleStorage.Add(dayOff);

			//PersonAssignmentRepo.Count().Should().Be.EqualTo(1);
			//PersonAssignmentRepo.SingleOrDefault().ShiftLayers.Count().Should().Be.EqualTo(0);

			var command = new AddActivityCommand
			{
				PersonId = person.Id.GetValueOrDefault(),
				Date = date,
				ActivityId = addedActivity.Id.Value,
				StartTime = new DateTime(2013, 11, 14, 14, 00, 00),
				EndTime = new DateTime(2013, 11, 14, 15, 00, 00)
			};
			Target.Handle(command);

			var dic = ScheduleStorage.FindSchedulesForPersons(new ScheduleDateTimePeriod(new DateTimePeriod(command.Date.Date.Utc(), command.Date.Date.Utc())), scenario, new PersonProvider(new[] { person }), new ScheduleDictionaryLoadOptions(false, false), new[] { person });
			var scheduleRange = dic[person];
			var scheduleDay = scheduleRange.ScheduledDay(command.Date);
			var personAssignment = scheduleDay.PersonAssignment();

			personAssignment.MainActivities().Single().Payload.Name.Should().Be("Added activity");
			personAssignment.ShiftCategory.Should().Be.SameInstanceAs(shiftCategory);
		}

		[Test]
		public void ShouldSetShiftCategoryWhenAddingActivityToDayOffWithPersonalActivity()
		{
			var date = new DateOnly(2016, 7, 26);
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);
			PersonRepository2.Add(person);
			var scenario = CurrentScenario.Current();
			CurrentScenario.FakeScenario(scenario);
			var activity = ActivityFactory.CreateActivity("an activity");
			ActivityRepository.Add(activity);
			ActivityRepository2.Add(activity);
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day");
			ShiftCategoryRepository.Add(shiftCategory);
			var dayOffWithPersonalActivity = PersonAssignmentFactory.CreateAssignmentWithDayOff(person, scenario, date, new DayOffTemplate());

			var personalActivity = ActivityFactory.CreateActivity("a personal activity");
			dayOffWithPersonalActivity.AddPersonalActivity(personalActivity, new DateTimePeriod(), true, null);
			ScheduleStorage.Add(dayOffWithPersonalActivity);


			
			var command = new AddActivityCommand
			{
				PersonId = person.Id.GetValueOrDefault(),
				Date = date,
				ActivityId = activity.Id.Value,
				StartTime = new DateTime(2016,7, 26, 14, 00, 00),
				EndTime = new DateTime(2016, 7, 26, 15, 00, 00)
			};

			Target.Handle(command);

			var dic = ScheduleStorage.FindSchedulesForPersons(new ScheduleDateTimePeriod(new DateTimePeriod(command.Date.Date.Utc(), command.Date.Date.Utc())), scenario, new PersonProvider(new[] { person }), new ScheduleDictionaryLoadOptions(false, false), new[] { person });
			var scheduleRange = dic[person];
			var scheduleDay = scheduleRange.ScheduledDay(command.Date);
			var personAssignment = scheduleDay.PersonAssignment();

			personAssignment.MainActivities().Single().Payload.Name.Should().Be(activity.Name);
			personAssignment.ShiftCategory.Should().Be.SameInstanceAs(shiftCategory);
		}

		[Test]
		public void ShouldReportErrorIfActivityConflictsWithOvernightShiftsFromPreviousDay()
		{
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);
			PersonRepository2.Add(person);
			var scenario = CurrentScenario.Current();
			CurrentScenario.FakeScenario(scenario);
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day");
			ShiftCategoryRepository.Add(shiftCategory);

			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var addedActivity = ActivityFactory.CreateActivity("Added activity");
			ActivityRepository.Add(addedActivity);
			ActivityRepository.Add(mainActivity);
			ActivityRepository2.Add(addedActivity);
			ActivityRepository2.Add(mainActivity);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, mainActivity, new DateTimePeriod(2013, 11, 13, 23, 2013, 11, 14, 8), shiftCategory);
			ScheduleStorage.Add(pa);


			var command = new AddActivityCommand
			{
				PersonId = person.Id.GetValueOrDefault(),
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
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);
			PersonRepository2.Add(person);

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

			ActivityRepository2.Add(addedActivity);
			ActivityRepository2.Add(mainActivity);
			ActivityRepository2.Add(lunchActivity);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, mainActivity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 16), shiftCategory);
			pa.AddActivity(lunchActivity, new DateTimePeriod(2013, 11, 14, 12, 2013, 11, 14, 13));
			pa.ShiftLayers.ForEach(l => l.WithId());
			ScheduleStorage.Add(pa);


			var command = new AddActivityCommand
			{
				PersonId = person.Id.Value,
				Date = new DateOnly(2013, 11, 14),
				ActivityId = addedActivity.Id.Value,
				StartTime = new DateTime(2013, 11, 14, 12, 0, 0),
				EndTime = new DateTime(2013, 11, 14, 14, 0, 0),
				MoveConflictLayerAllowed = true
			};
			Target.Handle(command);

			var dic = ScheduleStorage.FindSchedulesForPersons(new ScheduleDateTimePeriod(new DateTimePeriod(command.Date.Date.Utc(), command.Date.Date.Utc())), scenario, new PersonProvider(new[] { person }), new ScheduleDictionaryLoadOptions(false, false), new[] { person });
			var scheduleRange = dic[person];
			var scheduleDay = scheduleRange.ScheduledDay(command.Date);
			var personAssignment = scheduleDay.PersonAssignment();

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
			var scenario = CurrentScenario.Current();
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);
			PersonRepository2.Add(person);

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

			ActivityRepository2.Add(addedActivity);
			ActivityRepository2.Add(mainActivity);
			ActivityRepository2.Add(lunchActivity);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, mainActivity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 16), shiftCategory);
			pa.AddActivity(lunchActivity, new DateTimePeriod(2013, 11, 14, 12, 2013, 11, 14, 13));
			pa.ShiftLayers.ForEach(l => l.WithId());
			pa.PopAllEvents();
			ScheduleStorage.Add(pa);
			var command = new AddActivityCommand
			{
				PersonId = person.Id.GetValueOrDefault(),
				Date = new DateOnly(2013, 11, 14),
				ActivityId = addedActivity.Id.Value,
				StartTime = new DateTime(2013, 11, 14, 12, 0, 0),
				EndTime = new DateTime(2013, 11, 14, 14, 0, 0),
				MoveConflictLayerAllowed = true
			};
			Target.Handle(command);

			var dic = ScheduleStorage.FindSchedulesForPersons(new ScheduleDateTimePeriod(new DateTimePeriod(command.Date.Date.Utc(), command.Date.Date.Utc())), scenario, new PersonProvider(new[] { person }), new ScheduleDictionaryLoadOptions(false, false), new[] { person });
			var scheduleRange = dic[person];
			var scheduleDay = scheduleRange.ScheduledDay(command.Date);
			var personAssignment = scheduleDay.PersonAssignment();

			var allEves = personAssignment.PopAllEvents();
			allEves.Count().Should().Be(1);
			var @events = allEves.OfType<ActivityAddedEvent>().Where(e => e.ActivityId == addedActivity.Id.Value);
			@events.Count().Should().Be(1);
		}
		[Test]
		public void ShouldReturnWarningForConflictNonoverwritableLayerWhenItsNotFixable()
		{
			var scenario = CurrentScenario.Current();
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);
			PersonRepository2.Add(person);

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

			ActivityRepository2.Add(addedActivity);
			ActivityRepository2.Add(mainActivity);
			ActivityRepository2.Add(lunchActivity);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, mainActivity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 15), shiftCategory);
			pa.AddActivity(lunchActivity, new DateTimePeriod(2013, 11, 14, 11, 2013, 11, 14, 14));
			pa.ShiftLayers.ForEach(l => l.WithId());
			ScheduleStorage.Add(pa);


			var command = new AddActivityCommand
			{
				PersonId =  person.Id.GetValueOrDefault(),
				Date = new DateOnly(2013, 11, 14),
				ActivityId = addedActivity.Id.Value,
				StartTime = new DateTime(2013, 11, 14, 9, 0, 0),
				EndTime = new DateTime(2013, 11, 14, 14, 0, 0),
				MoveConflictLayerAllowed = true
			};
			Target.Handle(command);

			var dic = ScheduleStorage.FindSchedulesForPersons(new ScheduleDateTimePeriod(new DateTimePeriod(command.Date.Date.Utc(), command.Date.Date.Utc())), scenario, new PersonProvider(new[] { person }), new ScheduleDictionaryLoadOptions(false, false), new[] { person });
			var scheduleRange = dic[person];
			var scheduleDay = scheduleRange.ScheduledDay(command.Date);
			var personAssignment = scheduleDay.PersonAssignment();

			var addedLayer = personAssignment.ShiftLayers.Last();
			addedLayer.Payload.Should().Be(addedActivity);
			addedLayer.Period.StartDateTime.Should().Be(command.StartTime);
			addedLayer.Period.EndDateTime.Should().Be(command.EndTime);

			var movedLunchLayer = personAssignment.ShiftLayers.First(l => l.Payload == lunchActivity);
			movedLunchLayer.Period.Should().Be(new DateTimePeriod(2013, 11, 14, 11, 2013, 11, 14, 14));
			command.WarningMessages.Count.Should().Be.EqualTo(1);
			command.WarningMessages.First().Should().Be(Resources.NewActivityOverlapsNonoverwritableActivities);
		}
	}
}