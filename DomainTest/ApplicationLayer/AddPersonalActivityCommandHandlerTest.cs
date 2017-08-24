using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests;
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
	[Toggle(Toggles.Staffing_ReadModel_BetterAccuracy_Step4_43389)]
	public class AddPersonalActivityCommandHandlerTest : ISetup
	{
		public AddPersonalActivityCommandHandler Target;
		public FakeWriteSideRepository<IPerson> PersonRepository;
		public FakeWriteSideRepository<IActivity> ActivityRepository;
		public FakeActivityRepository ActivityRepository2;
		public FakeCurrentScenario CurrentScenario;
		public FakeScheduleStorage ScheduleStorage;
		public FakeWriteSideRepository<IMultiplicatorDefinitionSet> MultiplicatorDefinitionSetRepository;
		public FakeLoggedOnUser LoggedOnUser;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;
		public FakePersonSkillProvider PersonSkillProvider;
		public FakeSkillCombinationResourceRepository SkillCombinationResourceRepository;
		public MutableNow Now;
		private DateOnly _date;
		private DateTime _startTime;
		private DateTime _endTime;
		private IActivity _mainActivity;
		private DateTimePeriod _mainActivityDateTimePeriod;
		private DateTimePeriod _personalActivityDateTimePeriod;
		private ISkill _skill;
		private ISkill _skill2;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakePersonAssignmentWriteSideRepository>().For<IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey>>();
			system.UseTestDouble<FakeWriteSideRepository<IActivity>>().For<IProxyForId<IActivity>>();
			system.UseTestDouble<FakeWriteSideRepository<IPerson>>().For<IProxyForId<IPerson>>();
			system.UseTestDouble<FakeCurrentScenario>().For<ICurrentScenario>();
			system.UseTestDouble<FakeScheduleStorage>().For<IScheduleStorage>();
			system.UseTestDouble<FakeScheduleDifferenceSaver>().For<IScheduleDifferenceSaver>();
			system.UseTestDouble<ScheduleDayDifferenceSaver>().For<IScheduleDayDifferenceSaver>();
			system.UseTestDouble<AddPersonalActivityCommandHandler>().For<IHandleCommand<AddPersonalActivityCommand>>();
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble<FakeWriteSideRepository<IMultiplicatorDefinitionSet>>().For<IProxyForId<IMultiplicatorDefinitionSet>>();
			system.UseTestDouble<FakePersonSkillProvider>().For<IPersonSkillProvider>();
			//system.UseTestDouble<MutableNow>();
			
			_date = new DateOnly(2016, 05, 17);
			_startTime = new DateTime(2016, 05, 17, 10, 0, 0, DateTimeKind.Utc);
			_endTime = new DateTime(2016, 05, 17, 12, 0, 0, DateTimeKind.Utc);
			_mainActivity = ActivityFactory.CreateActivity("mainActivity");
			_mainActivityDateTimePeriod = new DateTimePeriod(new DateTime(2016, 05, 17, 09, 0, 0, DateTimeKind.Utc), new DateTime(2016, 05, 17, 17, 0, 0, DateTimeKind.Utc));
			_personalActivityDateTimePeriod = new DateTimePeriod(_startTime, _endTime);
			_skill = SkillFactory.CreateSkillWithId("skill", 15);
			_skill2 = SkillFactory.CreateSkillWithId("skill2", 15);
			_mainActivity.RequiresSkill = true;
			_skill.Activity = _mainActivity;
		}

		[Test]
		public void ShouldRaisePersonalActivityAddedEvent()
		{
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);
			var activity = ActivityFactory.CreateActivity("Phone").WithId();
			ActivityRepository.Add(activity);
			ActivityRepository.Add(_mainActivity);
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(person, CurrentScenario.Current(),_date);
			personAssignment.AddActivity(_mainActivity, _mainActivityDateTimePeriod);
			ScheduleStorage.Add(personAssignment);
				
			var command = new AddPersonalActivityCommand
			{
				PersonId = person.Id.GetValueOrDefault(),
				Date = _date,
				PersonalActivityId = activity.Id.GetValueOrDefault(),
				StartTime = _startTime,
				EndTime = _endTime,
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = Guid.NewGuid()
				}
			};

			Target.Handle(command);
			var ass = (PersonAssignment) ScheduleStorage.LoadAll().Single();
			var theEvent = ass.PopAllEvents().OfType<PersonalActivityAddedEvent>()
				.Single(e => e.ActivityId == command.PersonalActivityId);

			theEvent.Date.Should().Be.EqualTo(new DateTime(2016, 05, 17));
			theEvent.PersonId.Should().Be.EqualTo(command.PersonId);
			theEvent.StartDateTime.Should().Be.EqualTo(command.StartTime);
			theEvent.EndDateTime.Should().Be.EqualTo(command.EndTime);
			theEvent.ScenarioId.Should().Be.EqualTo(CurrentScenario.Current().Id.GetValueOrDefault());
			theEvent.LogOnBusinessUnitId.Should().Be(CurrentScenario.Current().BusinessUnit.Id.GetValueOrDefault());

		}

		[Test]
		public void ShouldPersistDeltasOfChange()
		{
			Now.Is(new DateTime(2016, 5, 17, 0, 0, 0, DateTimeKind.Utc));
			IntervalLengthFetcher.Has(15);
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);
			var activity = ActivityFactory.CreateActivity("Phone");
			activity.WithId();
			_skill2.Activity = activity;
			PersonSkillProvider.SkillCombination = new SkillCombination(new[] { _skill, _skill2 }, new DateOnlyPeriod(), null, new[] { _skill, _skill2 });
			activity.RequiresSkill = true;
			ActivityRepository.Add(activity);
			ActivityRepository2.Add(activity);
			ActivityRepository.Add(_mainActivity);
			ActivityRepository2.Add(_mainActivity);
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(person, CurrentScenario.Current(), _date);
			personAssignment.AddActivity(_mainActivity, _mainActivityDateTimePeriod);
			ScheduleStorage.Add(personAssignment);

			var command = new AddPersonalActivityCommand
			{
				PersonId = person.Id.GetValueOrDefault(),
				Date = _date,
				PersonalActivityId = activity.Id.GetValueOrDefault(),
				StartTime = _startTime,
				EndTime = _endTime,
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = Guid.NewGuid()
				}
			};

			Target.Handle(command);
			var combs = SkillCombinationResourceRepository.LoadSkillCombinationResources(_mainActivityDateTimePeriod, false).ToList();
			combs.Count.Should().Be.EqualTo(16);
			combs.Count(x => x.StartDateTime == _startTime).Should().Be.EqualTo(2);
			combs.Count(x => x.StartDateTime < _startTime).Should().Be.EqualTo(0);
			combs.Count(x => x.EndDateTime > _endTime).Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReportErrorIfActivityConflictsWithOvernightShiftsFromPreviousDay()
		{
			var activity = ActivityFactory.CreateActivity("Phone").WithId();
			ActivityRepository.Add(activity);
			ActivityRepository.Add(_mainActivity);

			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);

			var yesterdayActivityPeriod = new DateTimePeriod(new DateTime(2016, 05, 16, 19, 0, 0, DateTimeKind.Utc), new DateTime(2016, 05, 17, 09, 0, 0, DateTimeKind.Utc));
			var personAssignmentYesterday = PersonAssignmentFactory.CreatePersonAssignment(person, CurrentScenario.Current(), _date.AddDays(-1));
			personAssignmentYesterday.AddActivity(_mainActivity, yesterdayActivityPeriod);
			
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(person, CurrentScenario.Current(), _date);
			personAssignment.AddActivity(_mainActivity, _mainActivityDateTimePeriod);
			ScheduleStorage.Add(personAssignment);
			ScheduleStorage.Add(personAssignmentYesterday);

			var command = new AddPersonalActivityCommand
			{
				PersonId = person.Id.GetValueOrDefault(),
				Date = _date,
				PersonalActivityId = activity.Id.GetValueOrDefault(),
				StartTime = new DateTime(2016, 05, 17, 07, 0, 0, DateTimeKind.Utc),
				EndTime = new DateTime(2016, 05, 17, 09, 0, 0, DateTimeKind.Utc),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = Guid.NewGuid()
				}
			};

			Target.Handle(command);
			command.ErrorMessages.Single().Should().Be(Resources.ActivityConflictsWithOvernightShiftsFromPreviousDay);
		}
	}

	[TestFixture]
	[TestWithStaticDependenciesAvoidUse]
	public class AddPersonalActivityCommandHandlerNoDeltasTest
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
				PersonAssignmentFactory.CreateAssignmentWithMainShift(_personRepository.Single(),_mainActivity, _mainActivityDateTimePeriod)
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

			var target = new AddPersonalActivityCommandHandlerNoDeltas(personAssignmentRepository, currentScenario, _activityRepository, _personRepository, new UtcTimeZone());

			target.Handle(command);

			var @event =personAssignmentRepository.Single().PopAllEvents().OfType<PersonalActivityAddedEvent>().Single(e => e.ActivityId == _personalActivity.Id.Value);

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
				PersonAssignmentFactory.CreateAssignmentWithMainShift(_personRepository.Single(),_mainActivity, _mainActivityDateTimePeriod)
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

			var target = new AddPersonalActivityCommandHandlerNoDeltas(personAssignmentRepository, currentScenario, _activityRepository, _personRepository, new UtcTimeZone());

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

			var target = new AddPersonalActivityCommandHandlerNoDeltas(personAssignmentRepository, currentScenario, _activityRepository, _personRepository, new UtcTimeZone());

			target.Handle(command);

			command.ErrorMessages.Single().Should().Be(Resources.ActivityConflictsWithOvernightShiftsFromPreviousDay);
		}
	}

}
