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
	public class RemoveActivityCommandHandlerTest  :ISetup
	{
		public RemoveActivityCommandHandler Target;
		public FakeScheduleStorage ScheduleStorage;
		public FakePersonRepository PersonRepository;
		public FakeActivityRepository ActivityRepository;
		private IActivity _mainActivity;
		public FakeCurrentScenario CurrentScenario;
		public FakeSkillCombinationResourceRepository SkillCombinationResourceRepository;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;
		public FakePersonSkillProvider PersonSkillProvider;
		public MutableNow Now;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<RemoveActivityCommandHandler>().For<IHandleCommand<RemoveActivityCommand>>();
			system.UseTestDouble<FakeScheduleStorage>().For<IScheduleStorage>();
			system.UseTestDouble<FakeScheduleDifferenceSaver>().For<IScheduleDifferenceSaver>();
			system.UseTestDouble<ScheduleDayDifferenceSaver>().For<IScheduleDayDifferenceSaver>();
			system.UseTestDouble<FakeCurrentScenario>().For<ICurrentScenario>();
			system.UseTestDouble<FakeSkillCombinationResourceRepository>().For<ISkillCombinationResourceRepository>();
			system.UseTestDouble<FakePersonSkillProvider>().For<IPersonSkillProvider>();
			_mainActivity = ActivityFactory.CreateActivity("mainActivity");
		}

		[Test]
		public void ShouldRaiseRemoveActivityEvent()
		{
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);
			var activity = ActivityFactory.CreateActivity("Phone").WithId();
			ActivityRepository.Add(activity);
			ActivityRepository.Add(_mainActivity);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,_mainActivity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 16));
			
			personAssignment.AddActivity(activity, new DateTimePeriod(2013, 11, 14, 12, 2013, 11, 14, 14));
			personAssignment.ShiftLayers.ForEach(sl => EntityExtensions.WithId<ShiftLayer>(sl));
			var shiftLayer = personAssignment.ShiftLayers.First(sl => sl.Payload == activity);
			var command = new RemoveActivityCommand
			{
				PersonId = person.Id.GetValueOrDefault(),
				ShiftLayerId = shiftLayer.Id.GetValueOrDefault(),
				Date = new DateOnly(2013, 11, 14),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = Guid.NewGuid()
				}
			};

			CurrentScenario.FakeScenario(personAssignment.Scenario);
			ScheduleStorage.Add(personAssignment);

			Target.Handle(command);

			var dic = ScheduleStorage.FindSchedulesForPersons(new ScheduleDateTimePeriod(new DateTimePeriod(command.Date.Date.Utc(), command.Date.Date.Utc())), personAssignment.Scenario, new PersonProvider(new[] { person }), new ScheduleDictionaryLoadOptions(false, false), new[] { person });
			var scheduleRange = dic[person];
			var scheduleDay = scheduleRange.ScheduledDay(command.Date);
			personAssignment = scheduleDay.PersonAssignment();

			var @event = personAssignment.PopAllEvents().OfType<PersonAssignmentLayerRemovedEvent>().Single();
			@event.PersonId.Should().Be(person.Id.GetValueOrDefault());
			@event.Date.Should().Be(new DateTime(2013, 11, 14));
			@event.StartDateTime.Should().Be(shiftLayer.Period.StartDateTime);
			@event.EndDateTime.Should().Be(shiftLayer.Period.EndDateTime);
		}

		[Test]
		public void ShouldReportErrorWhenRemoveBaseShiftLayer()
		{
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);
			ActivityRepository.Add(_mainActivity);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, _mainActivity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 16));

			personAssignment.ShiftLayers.ForEach(sl => sl.WithId());
			var shiftLayer = personAssignment.ShiftLayers.First(sl => sl.Payload == _mainActivity);
			var command = new RemoveActivityCommand
			{
				PersonId = person.Id.GetValueOrDefault(),
				ShiftLayerId = shiftLayer.Id.GetValueOrDefault(),
				Date = new DateOnly(2013, 11, 14),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = Guid.NewGuid()
				}
			};

			CurrentScenario.FakeScenario(personAssignment.Scenario);
			ScheduleStorage.Add(personAssignment);

			Target.Handle(command);

			var dic = ScheduleStorage.FindSchedulesForPersons(new ScheduleDateTimePeriod(new DateTimePeriod(command.Date.Date.Utc(), command.Date.Date.Utc())), personAssignment.Scenario, new PersonProvider(new[] { person }), new ScheduleDictionaryLoadOptions(false, false), new[] { person });
			var scheduleRange = dic[person];
			var scheduleDay = scheduleRange.ScheduledDay(command.Date);
			personAssignment = scheduleDay.PersonAssignment();

			personAssignment.ShiftLayers.Count().Should().Be.EqualTo(1);
			command.ErrorMessages.Single().Should().Be.EqualTo(Resources.CannotDeleteBaseActivity);
		}

		[Test]
		public void ShouldReportErrorWhenShiftLayerNotFound()
		{
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);
			ActivityRepository.Add(_mainActivity);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, _mainActivity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 16));

			personAssignment.ShiftLayers.ForEach(sl => sl.WithId());
			var invalidLayerId = Guid.NewGuid();
			var command = new RemoveActivityCommand
			{
				PersonId = person.Id.GetValueOrDefault(),
				ShiftLayerId = invalidLayerId,
				Date = new DateOnly(2013, 11, 14),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = Guid.NewGuid()
				}
			};

			CurrentScenario.FakeScenario(personAssignment.Scenario);
			ScheduleStorage.Add(personAssignment);

			Target.Handle(command);

			var dic = ScheduleStorage.FindSchedulesForPersons(new ScheduleDateTimePeriod(new DateTimePeriod(command.Date.Date.Utc(), command.Date.Date.Utc())), personAssignment.Scenario, new PersonProvider(new[] { person }), new ScheduleDictionaryLoadOptions(false, false), new[] { person });
			var scheduleRange = dic[person];
			var scheduleDay = scheduleRange.ScheduledDay(command.Date);
			personAssignment = scheduleDay.PersonAssignment();

			personAssignment.ShiftLayers.Count().Should().Be.EqualTo(1);
			command.ErrorMessages.Single().Should().Be.EqualTo(Resources.NoShiftsFound);
		}

		[Test]
		public void ShouldPersistDeltasWhenActivityRemoved()
		{
			Now.Is(new DateTime(2013, 11, 14, 8, 0, 0, DateTimeKind.Utc));
			IntervalLengthFetcher.Has(15);
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);
			var activity = ActivityFactory.CreateActivity("Phone").WithId();
			activity.RequiresSkill = true;
			ActivityRepository.Add(activity);
			ActivityRepository.Add(_mainActivity);

			var skill = SkillFactory.CreateSkillWithId("skill", 15);
			skill.Activity = activity;

			PersonSkillProvider.SkillCombination = new SkillCombination(new[] { skill }, new DateOnlyPeriod(), null, new[] { skill });

			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, _mainActivity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 16));

			personAssignment.AddActivity(activity, new DateTimePeriod(2013, 11, 14, 12, 2013, 11, 14, 14));
			personAssignment.ShiftLayers.ForEach(sl => sl.WithId());
			var shiftLayer = personAssignment.ShiftLayers.First(sl => sl.Payload == activity);
			var command = new RemoveActivityCommand
			{
				PersonId = person.Id.GetValueOrDefault(),
				ShiftLayerId = shiftLayer.Id.GetValueOrDefault(),
				Date = new DateOnly(2013, 11, 14),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = Guid.NewGuid()
				}
			};

			CurrentScenario.FakeScenario(personAssignment.Scenario);
			ScheduleStorage.Add(personAssignment);
			
			Target.Handle(command);

			var combs = SkillCombinationResourceRepository.LoadSkillCombinationResources(new DateTimePeriod(2013, 11, 14, 12, 2013, 11, 14, 14), false).ToList();
			combs.Count.Should().Be.EqualTo(8);

		}

		[Test]
		public void ShouldPersistDeltasWhenActivityUnderRequireSkill()
		{
			Now.Is(new DateTime(2013, 11, 14, 8, 0, 0, DateTimeKind.Utc));
			IntervalLengthFetcher.Has(15);
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);
			var activity = ActivityFactory.CreateActivity("Phone").WithId();
			activity.RequiresSkill = true;
			_mainActivity.RequiresSkill = true;

			ActivityRepository.Add(activity);
			ActivityRepository.Add(_mainActivity);

			var skill = SkillFactory.CreateSkillWithId("skill", 15);
			skill.Activity = activity;
			var skill2 = SkillFactory.CreateSkillWithId("skill2", 15);
			skill2.Activity = _mainActivity;

			PersonSkillProvider.SkillCombination = new SkillCombination(new[] { skill,skill2 }, new DateOnlyPeriod(), null, new[] { skill,skill2 });

			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, _mainActivity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 16));

			personAssignment.AddActivity(activity, new DateTimePeriod(2013, 11, 14, 12, 2013, 11, 14, 14));
			personAssignment.ShiftLayers.ForEach(sl => sl.WithId());
			var shiftLayer = personAssignment.ShiftLayers.First(sl => sl.Payload == activity);
			var command = new RemoveActivityCommand
			{
				PersonId = person.Id.GetValueOrDefault(),
				ShiftLayerId = shiftLayer.Id.GetValueOrDefault(),
				Date = new DateOnly(2013, 11, 14),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = Guid.NewGuid()
				}
			};

			CurrentScenario.FakeScenario(personAssignment.Scenario);
			ScheduleStorage.Add(personAssignment);

			Target.Handle(command);

			var combs = SkillCombinationResourceRepository.LoadSkillCombinationResources(new DateTimePeriod(2013, 11, 14, 12, 2013, 11, 14, 14), false).ToList();
			combs.Count.Should().Be.EqualTo(16);
			combs.Count(x => x.Resource == -1).Should().Be.EqualTo(8);
			combs.Count(x => x.Resource == 1).Should().Be.EqualTo(8);

		}
	}
}