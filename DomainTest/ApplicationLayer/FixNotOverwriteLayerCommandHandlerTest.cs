using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.Repositories;
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
	public class FixNotOverwriteLayerCommandHandlerTest : ISetup
	{
		public FixNotOverwriteLayerCommandHandler Target;
		public FakeWriteSideRepository<IPerson> PersonRepository;
		public FakeWriteSideRepository<IActivity> ActivityRepository;
		public FakeCurrentScenario CurrentScenario;
		public FakeScheduleStorage ScheduleStorage;
		public FakeUserTimeZone UserTimeZone;
		public FakeShiftCategoryRepository ShiftCategoryRepository;
		public FakeLoggedOnUser LoggedOnUser;
		public FakePersonSkillProvider PersonSkillProvider;
		public MutableNow Now;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;
		public FakeSkillCombinationResourceRepository SkillCombinationResourceRepository;
		public FakeActivityRepository ActivityRepository2;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeWriteSideRepository<IActivity>>().For<IProxyForId<IActivity>>();
			system.UseTestDouble<FakeWriteSideRepository<IPerson>>().For<IProxyForId<IPerson>>();
			system.UseTestDouble<FakeCurrentScenario>().For<ICurrentScenario>();
			system.UseTestDouble<FakeScheduleStorage>().For<IScheduleStorage>();
			system.UseTestDouble<FixNotOverwriteLayerCommandHandler>()
				.For<IHandleCommand<FixNotOverwriteLayerCommand>>();
			system.UseTestDouble<FakeScheduleDifferenceSaver>().For<IScheduleDifferenceSaver>();
			system.UseTestDouble<ScheduleDayDifferenceSaver>().For<IScheduleDayDifferenceSaver>();
			system.UseTestDouble<FakeUserTimeZone>().For<IUserTimeZone>();
			system.UseTestDouble<FakeShiftCategoryRepository>().For<IShiftCategoryRepository>();
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble<FakeSkillCombinationResourceRepository>().For<ISkillCombinationResourceRepository>();
			system.UseTestDouble<FakePersonSkillProvider>().For<IPersonSkillProvider>();
		}

		[Test]
		public void ShouldFixNotOverwriteLayerInShift()
		{
			var scenario = CurrentScenario.Current();
			PersonRepository.Add(PersonFactory.CreatePersonWithId());

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day");
			ShiftCategoryRepository.Add(shiftCategory);
			var mainActivity = ActivityFactory.CreateActivity("Phone").WithId();
			var meetingActivity = ActivityFactory.CreateActivity("Meeting").WithId();
			var lunchActivity = ActivityFactory.CreateActivity("Lunch").WithId();

			mainActivity.AllowOverwrite = true;
			lunchActivity.AllowOverwrite = false;
			meetingActivity.AllowOverwrite = true;

			ActivityRepository.Add(meetingActivity);
			ActivityRepository.Add(mainActivity);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				PersonRepository.Single(), scenario, mainActivity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 15), shiftCategory);
			pa.AddActivity(lunchActivity, new DateTimePeriod(2013, 11, 14, 11, 2013, 11, 14, 14));
			pa.AddActivity(meetingActivity, new DateTimePeriod(2013, 11, 14, 11, 2013, 11, 14, 12));
			pa.ShiftLayers.ForEach(l => l.WithId());
			ScheduleStorage.Add(pa);

			var command = new FixNotOverwriteLayerCommand
			{
				PersonId = PersonRepository.Single().Id.GetValueOrDefault(),
				Date = new DateOnly(2013, 11, 14)
			};
			Target.Handle(command);

			var loadedPersonAss = ScheduleStorage.LoadAll().Single() as PersonAssignment;

			var movedLunchLayer = loadedPersonAss.ShiftLayers.First(l => l.Payload == lunchActivity);
			movedLunchLayer.Period.Should().Be(new DateTimePeriod(2013, 11, 14, 12, 2013, 11, 14, 15));
		}

		[Test]
		public void ShouldRaiseFixNotOverwriteLayerEvent()
		{
			var scenario = CurrentScenario.Current();
			PersonRepository.Add(PersonFactory.CreatePersonWithId());

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day");
			ShiftCategoryRepository.Add(shiftCategory);
			var mainActivity = ActivityFactory.CreateActivity("Phone").WithId();
			var meetingActivity = ActivityFactory.CreateActivity("Meeting").WithId();
			var lunchActivity = ActivityFactory.CreateActivity("Lunch").WithId();

			mainActivity.AllowOverwrite = true;
			lunchActivity.AllowOverwrite = false;
			meetingActivity.AllowOverwrite = true;

			ActivityRepository.Add(meetingActivity);
			ActivityRepository.Add(mainActivity);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				PersonRepository.Single(), scenario, mainActivity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 15), shiftCategory);
			pa.AddActivity(lunchActivity, new DateTimePeriod(2013, 11, 14, 11, 2013, 11, 14, 14));
			pa.AddActivity(meetingActivity, new DateTimePeriod(2013, 11, 14, 11, 2013, 11, 14, 12));
			pa.ShiftLayers.ForEach(l => l.WithId());
			ScheduleStorage.Add(pa);

			var operatePersonId = Guid.NewGuid();
			var trackId = Guid.NewGuid();
			var command = new FixNotOverwriteLayerCommand
			{
				PersonId = PersonRepository.Single().Id.GetValueOrDefault(),
				Date = new DateOnly(2013, 11, 14),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = operatePersonId,
					TrackId = trackId
				}
			};
			Target.Handle(command);

			var loadedPersonAss = ScheduleStorage.LoadAll().Single() as PersonAssignment;

			var movedLunchLayer = loadedPersonAss.ShiftLayers.First(l => l.Payload == lunchActivity);
			movedLunchLayer.Period.Should().Be(new DateTimePeriod(2013, 11, 14, 12, 2013, 11, 14, 15));
			var @event = loadedPersonAss.PopAllEvents().OfType<ActivityMovedEvent>().Single();
			@event.PersonId.Should().Be(PersonRepository.Single().Id.GetValueOrDefault());
			@event.ScenarioId.Should().Be(loadedPersonAss.Scenario.Id.GetValueOrDefault());
			@event.InitiatorId.Should().Be(operatePersonId);
			@event.CommandId.Should().Be(trackId);
			@event.LogOnBusinessUnitId.Should().Be(scenario.BusinessUnit.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldFixNotOverwriteLayerThatIsCoveredByMultipleLayersInShift()
		{
			var scenario = CurrentScenario.Current();
			PersonRepository.Add(PersonFactory.CreatePersonWithId());

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day");
			ShiftCategoryRepository.Add(shiftCategory);
			var mainActivity = ActivityFactory.CreateActivity("Phone").WithId();
			var meetingActivity = ActivityFactory.CreateActivity("Meeting").WithId();
			var lunchActivity = ActivityFactory.CreateActivity("Lunch").WithId();

			mainActivity.AllowOverwrite = true;
			lunchActivity.AllowOverwrite = false;
			meetingActivity.AllowOverwrite = true;

			ActivityRepository.Add(meetingActivity);
			ActivityRepository.Add(mainActivity);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				PersonRepository.Single(), scenario, mainActivity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 15), shiftCategory);
			pa.AddActivity(lunchActivity, new DateTimePeriod(2013, 11, 14, 11, 2013, 11, 14, 14));
			pa.AddActivity(meetingActivity, new DateTimePeriod(2013, 11, 14, 11, 2013, 11, 14, 12));
			pa.AddActivity(meetingActivity, new DateTimePeriod(2013, 11, 14, 13, 2013, 11, 14, 15));
			pa.ShiftLayers.ForEach(l => l.WithId());
			ScheduleStorage.Add(pa);


			var command = new FixNotOverwriteLayerCommand
			{
				PersonId = PersonRepository.Single().Id.GetValueOrDefault(),
				Date = new DateOnly(2013, 11, 14)
			};
			Target.Handle(command);

			var loadedPersonAss = ScheduleStorage.LoadAll().Single() as PersonAssignment;

			var movedLunchLayer = loadedPersonAss.ShiftLayers.First(l => l.Payload == lunchActivity);
			movedLunchLayer.Period.Should().Be(new DateTimePeriod(2013, 11, 14, 08, 2013, 11, 14, 11));
		}

		[Test]
		public void ShouldFixNotOverwriteLayerThatIsCoveredByMultipleLayersAndOneEndIsOutsideTheMainShift()
		{
			var scenario = CurrentScenario.Current();
			PersonRepository.Add(PersonFactory.CreatePersonWithId());

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day");
			ShiftCategoryRepository.Add(shiftCategory);
			var mainActivity = ActivityFactory.CreateActivity("Phone").WithId();
			var meetingActivity = ActivityFactory.CreateActivity("Meeting").WithId();
			var lunchActivity = ActivityFactory.CreateActivity("Lunch").WithId();

			mainActivity.AllowOverwrite = true;
			lunchActivity.AllowOverwrite = false;
			meetingActivity.AllowOverwrite = true;

			ActivityRepository.Add(meetingActivity);
			ActivityRepository.Add(mainActivity);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				PersonRepository.Single(), scenario, mainActivity, new DateTimePeriod(2013, 11, 14, 9, 2013, 11, 14, 18), shiftCategory);
			pa.AddActivity(lunchActivity, new DateTimePeriod(2013, 11, 14, 11, 2013, 11, 14, 14));
			pa.AddActivity(meetingActivity, new DateTimePeriod(2013, 11, 14, 11, 2013, 11, 14, 12));
			pa.AddActivity(meetingActivity, new DateTimePeriod(2013, 11, 14, 13, 2013, 11, 14, 15));
			pa.ShiftLayers.ForEach(l => l.WithId());
			ScheduleStorage.Add(pa);


			var command = new FixNotOverwriteLayerCommand
			{
				PersonId = PersonRepository.Single().Id.GetValueOrDefault(),
				Date = new DateOnly(2013, 11, 14)
			};
			Target.Handle(command);

			var loadedPersonAss = ScheduleStorage.LoadAll().Single() as PersonAssignment;

			var movedLunchLayer = loadedPersonAss.ShiftLayers.First(l => l.Payload == lunchActivity);
			movedLunchLayer.Period.Should().Be(new DateTimePeriod(2013, 11, 14, 15, 2013, 11, 14, 18));
		}

		[Test]
		public void ShouldFixMultipleNotOverwriteLayersInShift()
		{
			var scenario = CurrentScenario.Current();
			PersonRepository.Add(PersonFactory.CreatePersonWithId());

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day");
			ShiftCategoryRepository.Add(shiftCategory);
			var mainActivity = ActivityFactory.CreateActivity("Phone").WithId();
			var meetingActivity = ActivityFactory.CreateActivity("Meeting").WithId();
			var lunchActivity = ActivityFactory.CreateActivity("Lunch").WithId();
			var shortBreakActivity = ActivityFactory.CreateActivity("Short Break").WithId();

			mainActivity.AllowOverwrite = true;
			lunchActivity.AllowOverwrite = false;
			meetingActivity.AllowOverwrite = true;
			shortBreakActivity.AllowOverwrite = false;

			ActivityRepository.Add(meetingActivity);
			ActivityRepository.Add(mainActivity);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				PersonRepository.Single(), scenario, mainActivity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 18), shiftCategory);
			pa.AddActivity(lunchActivity, new DateTimePeriod(2013, 11, 14, 10, 2013, 11, 14, 11));
			pa.AddActivity(shortBreakActivity, new DateTimePeriod(2013, 11, 14, 13, 2013, 11, 14, 14));
			pa.AddActivity(meetingActivity, new DateTimePeriod(2013, 11, 14, 10, 2013, 11, 14, 14));
			pa.ShiftLayers.ForEach(l => l.WithId());
			ScheduleStorage.Add(pa);


			var command = new FixNotOverwriteLayerCommand
			{
				PersonId = PersonRepository.Single().Id.GetValueOrDefault(),
				Date = new DateOnly(2013, 11, 14)
			};
			Target.Handle(command);

			var loadedPersonAss = ScheduleStorage.LoadAll().Single() as PersonAssignment;

			var movedLunchLayer = loadedPersonAss.ShiftLayers.First(l => l.Payload == lunchActivity);
			var movedShortBreakLayer = loadedPersonAss.ShiftLayers.First(l => l.Payload == shortBreakActivity);
			movedLunchLayer.Period.Should().Be(new DateTimePeriod(2013, 11, 14, 9, 2013, 11, 14, 10));
			movedShortBreakLayer.Period.Should().Be(new DateTimePeriod(2013, 11, 14, 14, 2013, 11, 14, 15));
		}

		[Test]
		public void ShouldFixNotOverwritableLayerWhichIsAlreadyOverwrittenInShift()
		{
			var scenario = CurrentScenario.Current();
			PersonRepository.Add(PersonFactory.CreatePersonWithId());

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day");
			ShiftCategoryRepository.Add(shiftCategory);
			var mainActivity = ActivityFactory.CreateActivity("Phone").WithId();
			var meetingActivity = ActivityFactory.CreateActivity("Meeting").WithId();
			var lunchActivity = ActivityFactory.CreateActivity("Lunch").WithId();

			mainActivity.AllowOverwrite = true;
			lunchActivity.AllowOverwrite = false;
			meetingActivity.AllowOverwrite = true;

			ActivityRepository.Add(meetingActivity);
			ActivityRepository.Add(mainActivity);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				PersonRepository.Single(), scenario, mainActivity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 18), shiftCategory);
			pa.AddActivity(lunchActivity, new DateTimePeriod(2013, 11, 14, 10, 2013, 11, 14, 12));
			pa.AddActivity(mainActivity, new DateTimePeriod(2013, 11, 14, 11, 2013, 11, 14, 12));
			pa.AddActivity(meetingActivity, new DateTimePeriod(2013, 11, 14, 10, 2013, 11, 14, 14));
			pa.ShiftLayers.ForEach(l => l.WithId());
			ScheduleStorage.Add(pa);

			var command = new FixNotOverwriteLayerCommand
			{
				PersonId = PersonRepository.Single().Id.GetValueOrDefault(),
				Date = new DateOnly(2013, 11, 14)
			};
			Target.Handle(command);

			var loadedPersonAss = ScheduleStorage.LoadAll().Single() as PersonAssignment;

			var movedLunchLayer = loadedPersonAss.ShiftLayers.First(l => l.Payload == lunchActivity);
			movedLunchLayer.Period.Should().Be(new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 10));
		}

		[Test]
		public void ShouldFixMultipleNotOverwritableLayersWhenTheyOverlapEachOther()
		{
			var scenario = CurrentScenario.Current();
			PersonRepository.Add(PersonFactory.CreatePersonWithId());

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day");
			ShiftCategoryRepository.Add(shiftCategory);
			var mainActivity = ActivityFactory.CreateActivity("Phone").WithId();
			var meetingActivity = ActivityFactory.CreateActivity("Meeting").WithId();
			var lunchActivity = ActivityFactory.CreateActivity("Lunch").WithId();
			var shortBreakActivity = ActivityFactory.CreateActivity("Short Break").WithId();

			mainActivity.AllowOverwrite = true;
			lunchActivity.AllowOverwrite = false;
			meetingActivity.AllowOverwrite = true;
			shortBreakActivity.AllowOverwrite = false;

			ActivityRepository.Add(meetingActivity);
			ActivityRepository.Add(mainActivity);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				PersonRepository.Single(), scenario, mainActivity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 18), shiftCategory);
			pa.AddActivity(lunchActivity, new DateTimePeriod(2013, 11, 14, 10, 2013, 11, 14, 12));
			pa.AddActivity(shortBreakActivity, new DateTimePeriod(2013, 11, 14, 11, 2013, 11, 14, 13));
			pa.AddActivity(meetingActivity, new DateTimePeriod(2013, 11, 14, 10, 2013, 11, 14, 14));
			pa.ShiftLayers.ForEach(l => l.WithId());
			ScheduleStorage.Add(pa);

			var command = new FixNotOverwriteLayerCommand
			{
				PersonId = PersonRepository.Single().Id.GetValueOrDefault(),
				Date = new DateOnly(2013, 11, 14)
			};
			Target.Handle(command);

			var loadedPersonAss = ScheduleStorage.LoadAll().Single() as PersonAssignment;

			var movedLunchLayer = loadedPersonAss.ShiftLayers.First(l => l.Payload == lunchActivity);
			var movedShortBreakLayer = loadedPersonAss.ShiftLayers.First(l => l.Payload == shortBreakActivity);
			movedLunchLayer.Period.Should().Be(new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 10));
			movedShortBreakLayer.Period.Should().Be(new DateTimePeriod(2013, 11, 14, 14, 2013, 11, 14, 16));
		}

		[Test]
		public void ShouldReturnErrorMessageWhenScheduleDayCanNotBeFixedCompletely()
		{
			var scenario = CurrentScenario.Current();
			PersonRepository.Add(PersonFactory.CreatePersonWithId());

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day");
			ShiftCategoryRepository.Add(shiftCategory);
			var mainActivity = ActivityFactory.CreateActivity("Phone").WithId();
			var meetingActivity = ActivityFactory.CreateActivity("Meeting").WithId();
			var lunchActivity = ActivityFactory.CreateActivity("Lunch").WithId();
			var shortBreakActivity = ActivityFactory.CreateActivity("Short Break").WithId();

			mainActivity.AllowOverwrite = true;
			lunchActivity.AllowOverwrite = false;
			meetingActivity.AllowOverwrite = true;
			shortBreakActivity.AllowOverwrite = false;

			ActivityRepository.Add(meetingActivity);
			ActivityRepository.Add(mainActivity);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				PersonRepository.Single(), scenario, mainActivity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 18),
				shiftCategory);
			pa.AddActivity(lunchActivity, new DateTimePeriod(2013, 11, 14, 10, 2013, 11, 14, 12));
			pa.AddActivity(meetingActivity, new DateTimePeriod(2013, 11, 14, 9, 2013, 11, 14, 17));
			pa.ShiftLayers.ForEach(l => l.WithId());
			ScheduleStorage.Add(pa);

			var command = new FixNotOverwriteLayerCommand
			{
				PersonId = PersonRepository.Single().Id.GetValueOrDefault(),
				Date = new DateOnly(2013, 11, 14)
			};
			Target.Handle(command);

			var error = command.ErrorMessages.Single();
			error.Should().Be.EqualTo(Resources.OverlappedNonoverwritableActivitiesExist);
		}

		[Test]
		public void ShouldPersistDeltas()
		{
			IntervalLengthFetcher.Has(15);
			Now.Is(new DateTime(2013, 11, 14, 0, 0, 0, DateTimeKind.Utc));
			var skill = SkillFactory.CreateSkillWithId("skill", 15);
			PersonSkillProvider.SkillCombination = new SkillCombination(new[] { skill }, new DateOnlyPeriod(), null, new[] { skill });

			var scenario = CurrentScenario.Current();
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day");
			ShiftCategoryRepository.Add(shiftCategory);
			var mainActivity = ActivityFactory.CreateActivity("Phone").WithId();
			var meetingActivity = ActivityFactory.CreateActivity("Meeting").WithId();
			var lunchActivity = ActivityFactory.CreateActivity("Lunch").WithId();
			var shortBreakActivity = ActivityFactory.CreateActivity("Short Break").WithId();

			mainActivity.AllowOverwrite = true;
			mainActivity.RequiresSkill = true;
			skill.Activity = mainActivity;

			lunchActivity.AllowOverwrite = false;
			meetingActivity.AllowOverwrite = true;
			shortBreakActivity.AllowOverwrite = false; 

			ActivityRepository.Add(meetingActivity);
			ActivityRepository.Add(mainActivity);
			ActivityRepository.Add(lunchActivity);
			ActivityRepository.Add(shortBreakActivity);
			ActivityRepository2.Add(meetingActivity);
			ActivityRepository2.Add(mainActivity);
			ActivityRepository2.Add(lunchActivity);
			ActivityRepository2.Add(shortBreakActivity);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				PersonRepository.Single(), scenario, mainActivity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 18), shiftCategory);
			pa.AddActivity(lunchActivity, new DateTimePeriod(2013, 11, 14, 10, 2013, 11, 14, 12));
			pa.AddActivity(shortBreakActivity, new DateTimePeriod(2013, 11, 14, 11, 2013, 11, 14, 13));
			pa.AddActivity(meetingActivity, new DateTimePeriod(2013, 11, 14, 10, 2013, 11, 14, 14));
			pa.ShiftLayers.ForEach(l => l.WithId());
			ScheduleStorage.Add(pa);

			var command = new FixNotOverwriteLayerCommand
			{
				PersonId = PersonRepository.Single().Id.GetValueOrDefault(),
				Date = new DateOnly(2013, 11, 14)
			};
			Target.Handle(command);

			var combs = SkillCombinationResourceRepository.LoadSkillCombinationResources(new DateTimePeriod(2013, 11, 14, 2013, 11, 15), false).ToList();
			combs.Count.Should().Be.EqualTo(16);
			combs.Count(x => x.Resource == -1).Should().Be.EqualTo(16);
			combs.Count(x => x.StartDateTime >= new DateTime(2013, 11, 14, 8, 0, 0) && x.EndDateTime <= new DateTime(2013, 11, 14, 10, 0, 0)).Should().Be.EqualTo(8);
			combs.Count(x => x.StartDateTime >= new DateTime(2013, 11, 14, 14, 0, 0) && x.EndDateTime <= new DateTime(2013, 11, 14, 16, 0, 0)).Should().Be.EqualTo(8);
		}
	}

	[TestFixture]
	[DomainTest]
	public class FixNotOverwriteLayerCommandHandlerNoDeltasTest:ISetup
	{
		public FixNotOverwriteLayerCommandHandlerNoDeltas Target;
		public FakeWriteSideRepository<IPerson> PersonRepository;
		public FakeWriteSideRepository<IActivity> ActivityRepository;
		public FakePersonAssignmentWriteSideRepository PersonAssignmentRepo;
		public FakeCurrentScenario CurrentScenario;
		public FakeScheduleStorage ScheduleStorage;
		public FakeUserTimeZone UserTimeZone;
		public FakeShiftCategoryRepository ShiftCategoryRepository;
		public FakeLoggedOnUser LoggedOnUser;

		public void Setup(ISystem system,IIocConfiguration configuration)
		{
			system.UseTestDouble<FakePersonAssignmentWriteSideRepository>().For<IWriteSideRepositoryTypedId<IPersonAssignment,PersonAssignmentKey>>();
			system.UseTestDouble<FakeWriteSideRepository<IActivity>>().For<IProxyForId<IActivity>>();
			system.UseTestDouble<FakeWriteSideRepository<IPerson>>().For<IProxyForId<IPerson>>();
			system.UseTestDouble<FakeCurrentScenario>().For<ICurrentScenario>();
			system.UseTestDouble<FakeScheduleStorage>().For<IScheduleStorage>();
			system.UseTestDouble<FixNotOverwriteLayerCommandHandlerNoDeltas>().For<IHandleCommand<FixNotOverwriteLayerCommand>>();
			system.UseTestDouble<FakeUserTimeZone>().For<IUserTimeZone>();
			system.UseTestDouble<FakeShiftCategoryRepository>().For<IShiftCategoryRepository>();
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
		}

		[Test]
		public void ShouldFixNotOverwriteLayerInShift()
		{
			var scenario = CurrentScenario.Current();
			PersonRepository.Add(PersonFactory.CreatePersonWithId());

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day");
			ShiftCategoryRepository.Add(shiftCategory);
			var mainActivity = ActivityFactory.CreateActivity("Phone").WithId();
			var meetingActivity = ActivityFactory.CreateActivity("Meeting").WithId();
			var lunchActivity = ActivityFactory.CreateActivity("Lunch").WithId();

			mainActivity.AllowOverwrite = true;
			lunchActivity.AllowOverwrite = false;
			meetingActivity.AllowOverwrite = true;

			ActivityRepository.Add(meetingActivity);
			ActivityRepository.Add(mainActivity);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				PersonRepository.Single(),scenario, mainActivity, new DateTimePeriod(2013,11,14,8,2013,11,14,15), shiftCategory);
			pa.AddActivity(lunchActivity,new DateTimePeriod(2013,11,14,11,2013,11,14,14));
			pa.AddActivity(meetingActivity,new DateTimePeriod(2013,11,14,11,2013,11,14,12));
			pa.ShiftLayers.ForEach(l => l.WithId());
			PersonAssignmentRepo.Add(pa);
			ScheduleStorage.Add(pa);


			var command = new FixNotOverwriteLayerCommand
			{
				PersonId = PersonRepository.Single().Id.GetValueOrDefault(),
				Date = new DateOnly(2013,11,14)
			};
			Target.Handle(command);

			var movedLunchLayer = PersonAssignmentRepo.Single().ShiftLayers.First(l => l.Payload == lunchActivity);
			movedLunchLayer.Period.Should().Be(new DateTimePeriod(2013,11,14,12,2013,11,14,15));
		}

		[Test]
		public void ShouldRaiseFixNotOverwriteLayerEvent()
		{
			var scenario = CurrentScenario.Current();
			PersonRepository.Add(PersonFactory.CreatePersonWithId());

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day");
			ShiftCategoryRepository.Add(shiftCategory);
			var mainActivity = ActivityFactory.CreateActivity("Phone").WithId();
			var meetingActivity = ActivityFactory.CreateActivity("Meeting").WithId();
			var lunchActivity = ActivityFactory.CreateActivity("Lunch").WithId();

			mainActivity.AllowOverwrite = true;
			lunchActivity.AllowOverwrite = false;
			meetingActivity.AllowOverwrite = true;

			ActivityRepository.Add(meetingActivity);
			ActivityRepository.Add(mainActivity);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				PersonRepository.Single(),scenario, mainActivity, new DateTimePeriod(2013,11,14,8,2013,11,14,15), shiftCategory);
			pa.AddActivity(lunchActivity,new DateTimePeriod(2013,11,14,11,2013,11,14,14));
			pa.AddActivity(meetingActivity,new DateTimePeriod(2013,11,14,11,2013,11,14,12));
			pa.ShiftLayers.ForEach(l => l.WithId());
			PersonAssignmentRepo.Add(pa);
			ScheduleStorage.Add(pa);

			var operatePersonId = Guid.NewGuid();
			var trackId = Guid.NewGuid();
			var command = new FixNotOverwriteLayerCommand
			{
				PersonId = PersonRepository.Single().Id.GetValueOrDefault(),
				Date = new DateOnly(2013,11,14),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = operatePersonId,
					TrackId = trackId
				}
			};
			Target.Handle(command);

			var movedLunchLayer = PersonAssignmentRepo.Single().ShiftLayers.First(l => l.Payload == lunchActivity);
			movedLunchLayer.Period.Should().Be(new DateTimePeriod(2013,11,14,12,2013,11,14,15));
			var @event = PersonAssignmentRepo.Single().PopAllEvents().OfType<ActivityMovedEvent>().Single();
			@event.PersonId.Should().Be(PersonRepository.Single().Id.GetValueOrDefault());
			@event.ScenarioId.Should().Be(PersonAssignmentRepo.Single().Scenario.Id.GetValueOrDefault());
			@event.InitiatorId.Should().Be(operatePersonId);
			@event.CommandId.Should().Be(trackId);
			@event.LogOnBusinessUnitId.Should().Be(scenario.BusinessUnit.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldFixNotOverwriteLayerThatIsCoveredByMultipleLayersInShift()
		{
			var scenario = CurrentScenario.Current();
			PersonRepository.Add(PersonFactory.CreatePersonWithId());

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day");
			ShiftCategoryRepository.Add(shiftCategory);
			var mainActivity = ActivityFactory.CreateActivity("Phone").WithId();
			var meetingActivity = ActivityFactory.CreateActivity("Meeting").WithId();
			var lunchActivity = ActivityFactory.CreateActivity("Lunch").WithId();

			mainActivity.AllowOverwrite = true;
			lunchActivity.AllowOverwrite = false;
			meetingActivity.AllowOverwrite = true;

			ActivityRepository.Add(meetingActivity);
			ActivityRepository.Add(mainActivity);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				PersonRepository.Single(),scenario, mainActivity, new DateTimePeriod(2013,11,14,8,2013,11,14,15), shiftCategory);
			pa.AddActivity(lunchActivity,new DateTimePeriod(2013,11,14,11,2013,11,14,14));
			pa.AddActivity(meetingActivity,new DateTimePeriod(2013,11,14,11,2013,11,14,12));
			pa.AddActivity(meetingActivity,new DateTimePeriod(2013,11,14,13,2013,11,14,15));
			pa.ShiftLayers.ForEach(l => l.WithId());
			PersonAssignmentRepo.Add(pa);
			ScheduleStorage.Add(pa);


			var command = new FixNotOverwriteLayerCommand
			{
				PersonId = PersonRepository.Single().Id.GetValueOrDefault(),
				Date = new DateOnly(2013,11,14)
			};
			Target.Handle(command);

			var movedLunchLayer = PersonAssignmentRepo.Single().ShiftLayers.First(l => l.Payload == lunchActivity);
			movedLunchLayer.Period.Should().Be(new DateTimePeriod(2013,11,14,08,2013,11,14,11));
		}

		[Test]
		public void ShouldFixNotOverwriteLayerThatIsCoveredByMultipleLayersAndOneEndIsOutsideTheMainShift()
		{
			var scenario = CurrentScenario.Current();
			PersonRepository.Add(PersonFactory.CreatePersonWithId());

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day");
			ShiftCategoryRepository.Add(shiftCategory);
			var mainActivity = ActivityFactory.CreateActivity("Phone").WithId();
			var meetingActivity = ActivityFactory.CreateActivity("Meeting").WithId();
			var lunchActivity = ActivityFactory.CreateActivity("Lunch").WithId();

			mainActivity.AllowOverwrite = true;
			lunchActivity.AllowOverwrite = false;
			meetingActivity.AllowOverwrite = true;

			ActivityRepository.Add(meetingActivity);
			ActivityRepository.Add(mainActivity);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				PersonRepository.Single(),scenario, mainActivity, new DateTimePeriod(2013,11,14,9,2013,11,14,18), shiftCategory);
			pa.AddActivity(lunchActivity,new DateTimePeriod(2013,11,14,11,2013,11,14,14));
			pa.AddActivity(meetingActivity,new DateTimePeriod(2013,11,14,11,2013,11,14,12));
			pa.AddActivity(meetingActivity,new DateTimePeriod(2013,11,14,13,2013,11,14,15));
			pa.ShiftLayers.ForEach(l => l.WithId());
			PersonAssignmentRepo.Add(pa);
			ScheduleStorage.Add(pa);


			var command = new FixNotOverwriteLayerCommand
			{
				PersonId = PersonRepository.Single().Id.GetValueOrDefault(),
				Date = new DateOnly(2013,11,14)
			};
			Target.Handle(command);

			var movedLunchLayer = PersonAssignmentRepo.Single().ShiftLayers.First(l => l.Payload == lunchActivity);
			movedLunchLayer.Period.Should().Be(new DateTimePeriod(2013,11,14,15,2013,11,14,18));
		}

		[Test]
		public void ShouldFixMultipleNotOverwriteLayersInShift()
		{
			var scenario = CurrentScenario.Current();
			PersonRepository.Add(PersonFactory.CreatePersonWithId());

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day");
			ShiftCategoryRepository.Add(shiftCategory);
			var mainActivity = ActivityFactory.CreateActivity("Phone").WithId();
			var meetingActivity = ActivityFactory.CreateActivity("Meeting").WithId();
			var lunchActivity = ActivityFactory.CreateActivity("Lunch").WithId();
			var shortBreakActivity = ActivityFactory.CreateActivity("Short Break").WithId();

			mainActivity.AllowOverwrite = true;
			lunchActivity.AllowOverwrite = false;
			meetingActivity.AllowOverwrite = true;
			shortBreakActivity.AllowOverwrite = false;

			ActivityRepository.Add(meetingActivity);
			ActivityRepository.Add(mainActivity);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				PersonRepository.Single(),scenario, mainActivity, new DateTimePeriod(2013,11,14,8,2013,11,14,18), shiftCategory);
			pa.AddActivity(lunchActivity,new DateTimePeriod(2013,11,14,10,2013,11,14,11));
			pa.AddActivity(shortBreakActivity,new DateTimePeriod(2013,11,14,13,2013,11,14,14));
			pa.AddActivity(meetingActivity,new DateTimePeriod(2013,11,14,10,2013,11,14,14));
			pa.ShiftLayers.ForEach(l => l.WithId());
			PersonAssignmentRepo.Add(pa);
			ScheduleStorage.Add(pa);


			var command = new FixNotOverwriteLayerCommand
			{
				PersonId = PersonRepository.Single().Id.GetValueOrDefault(),
				Date = new DateOnly(2013,11,14)
			};
			Target.Handle(command);

			var movedLunchLayer = PersonAssignmentRepo.Single().ShiftLayers.First(l => l.Payload == lunchActivity);
			var movedShortBreakLayer = PersonAssignmentRepo.Single().ShiftLayers.First(l => l.Payload == shortBreakActivity);
			movedLunchLayer.Period.Should().Be(new DateTimePeriod(2013,11,14,9,2013,11,14,10));
			movedShortBreakLayer.Period.Should().Be(new DateTimePeriod(2013,11,14,14,2013,11,14,15));
		}

		[Test]
		public void ShouldFixNotOverwritableLayerWhichIsAlreadyOverwrittenInShift()
		{
			var scenario = CurrentScenario.Current();
			PersonRepository.Add(PersonFactory.CreatePersonWithId());

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day");
			ShiftCategoryRepository.Add(shiftCategory);
			var mainActivity = ActivityFactory.CreateActivity("Phone").WithId();
			var meetingActivity = ActivityFactory.CreateActivity("Meeting").WithId();
			var lunchActivity = ActivityFactory.CreateActivity("Lunch").WithId();

			mainActivity.AllowOverwrite = true;
			lunchActivity.AllowOverwrite = false;
			meetingActivity.AllowOverwrite = true;

			ActivityRepository.Add(meetingActivity);
			ActivityRepository.Add(mainActivity);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				PersonRepository.Single(),scenario, mainActivity, new DateTimePeriod(2013,11,14,8,2013,11,14,18), shiftCategory);
			pa.AddActivity(lunchActivity,new DateTimePeriod(2013,11,14,10,2013,11,14,12));
			pa.AddActivity(mainActivity,new DateTimePeriod(2013,11,14,11,2013,11,14,12));
			pa.AddActivity(meetingActivity,new DateTimePeriod(2013,11,14,10,2013,11,14,14));
			pa.ShiftLayers.ForEach(l => l.WithId());
			PersonAssignmentRepo.Add(pa);
			ScheduleStorage.Add(pa);

			var command = new FixNotOverwriteLayerCommand
			{
				PersonId = PersonRepository.Single().Id.GetValueOrDefault(),
				Date = new DateOnly(2013,11,14)
			};
			Target.Handle(command);

			var movedLunchLayer = PersonAssignmentRepo.Single().ShiftLayers.First(l => l.Payload == lunchActivity);
			movedLunchLayer.Period.Should().Be(new DateTimePeriod(2013,11,14,8,2013,11,14,10));
		}

		[Test]
		public void ShouldFixMultipleNotOverwritableLayersWhenTheyOverlapEachOther()
		{
			var scenario = CurrentScenario.Current();
			PersonRepository.Add(PersonFactory.CreatePersonWithId());

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day");
			ShiftCategoryRepository.Add(shiftCategory);
			var mainActivity = ActivityFactory.CreateActivity("Phone").WithId();
			var meetingActivity = ActivityFactory.CreateActivity("Meeting").WithId();
			var lunchActivity = ActivityFactory.CreateActivity("Lunch").WithId();
			var shortBreakActivity = ActivityFactory.CreateActivity("Short Break").WithId();

			mainActivity.AllowOverwrite = true;
			lunchActivity.AllowOverwrite = false;
			meetingActivity.AllowOverwrite = true;
			shortBreakActivity.AllowOverwrite = false;

			ActivityRepository.Add(meetingActivity);
			ActivityRepository.Add(mainActivity);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				PersonRepository.Single(),scenario, mainActivity, new DateTimePeriod(2013,11,14,8,2013,11,14,18), shiftCategory);
			pa.AddActivity(lunchActivity,new DateTimePeriod(2013,11,14,10,2013,11,14,12));
			pa.AddActivity(shortBreakActivity,new DateTimePeriod(2013,11,14,11,2013,11,14,13));
			pa.AddActivity(meetingActivity,new DateTimePeriod(2013,11,14,10,2013,11,14,14));
			pa.ShiftLayers.ForEach(l => l.WithId());
			PersonAssignmentRepo.Add(pa);
			ScheduleStorage.Add(pa);

			var command = new FixNotOverwriteLayerCommand
			{
				PersonId = PersonRepository.Single().Id.GetValueOrDefault(),
				Date = new DateOnly(2013,11,14)
			};
			Target.Handle(command);

			var movedLunchLayer = PersonAssignmentRepo.Single().ShiftLayers.First(l => l.Payload == lunchActivity);
			var movedShortBreakLayer = PersonAssignmentRepo.Single().ShiftLayers.First(l => l.Payload == shortBreakActivity);
			movedLunchLayer.Period.Should().Be(new DateTimePeriod(2013,11,14,8,2013,11,14,10));
			movedShortBreakLayer.Period.Should().Be(new DateTimePeriod(2013,11,14,14,2013,11,14,16));
		}

		[Test]
		public void ShouldReturnErrorMessageWhenScheduleDayCanNotBeFixedCompletely()
		{
			var scenario = CurrentScenario.Current();
			PersonRepository.Add(PersonFactory.CreatePersonWithId());

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day");
			ShiftCategoryRepository.Add(shiftCategory);
			var mainActivity = ActivityFactory.CreateActivity("Phone").WithId();
			var meetingActivity = ActivityFactory.CreateActivity("Meeting").WithId();
			var lunchActivity = ActivityFactory.CreateActivity("Lunch").WithId();
			var shortBreakActivity = ActivityFactory.CreateActivity("Short Break").WithId();

			mainActivity.AllowOverwrite = true;
			lunchActivity.AllowOverwrite = false;
			meetingActivity.AllowOverwrite = true;
			shortBreakActivity.AllowOverwrite = false;

			ActivityRepository.Add(meetingActivity);
			ActivityRepository.Add(mainActivity);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				PersonRepository.Single(),scenario, mainActivity, new DateTimePeriod(2013,11,14,8,2013,11,14,18), shiftCategory);
			pa.AddActivity(lunchActivity,new DateTimePeriod(2013,11,14,10,2013,11,14,12));
			pa.AddActivity(meetingActivity,new DateTimePeriod(2013,11,14,9,2013,11,14,17));
			pa.ShiftLayers.ForEach(l => l.WithId());
			PersonAssignmentRepo.Add(pa);
			ScheduleStorage.Add(pa);

			var command = new FixNotOverwriteLayerCommand
			{
				PersonId = PersonRepository.Single().Id.GetValueOrDefault(),
				Date = new DateOnly(2013,11,14)
			};
			Target.Handle(command);

			var error = command.ErrorMessages.Single();
			error.Should().Be.EqualTo(Resources.OverlappedNonoverwritableActivitiesExist);
		}
	}
}
