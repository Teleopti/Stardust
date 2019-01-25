using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[DomainTest]
	public class AddPersonalActivityCommandHandlerTest : IIsolateSystem
	{
		public AddPersonalActivityCommandHandler Target;
		public FakePersonRepository PersonRepository;
		public FakeWriteSideRepository<IActivity> ActivityRepository;
		public FakeActivityRepository ActivityRepository2;
		public FakeSkillRepository SkillRepository;
		public FakeScenarioRepository CurrentScenario;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;
		public FakeSkillCombinationResourceRepository SkillCombinationResourceRepository;
		public MutableNow Now;

		private DateOnly _date;
		private DateTime _startTime;
		private DateTime _endTime;
		private IActivity _mainActivity;
		private DateTimePeriod _mainActivityDateTimePeriod;
		private ISkill _skill;
		private ISkill _skill2;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakePersonAssignmentWriteSideRepository>().For<IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey>>();
			isolate.UseTestDouble<FakeWriteSideRepository<IActivity>>().For<IProxyForId<IActivity>>();
			isolate.UseTestDouble<ScheduleDayDifferenceSaver>().For<IScheduleDayDifferenceSaver>();
			isolate.UseTestDouble<AddPersonalActivityCommandHandler>().For<IHandleCommand<AddPersonalActivityCommand>>();
			isolate.UseTestDouble<FakeWriteSideRepository<IMultiplicatorDefinitionSet>>().For<IProxyForId<IMultiplicatorDefinitionSet>>();

			_date = new DateOnly(2016, 05, 17);
			_startTime = new DateTime(2016, 05, 17, 10, 0, 0, DateTimeKind.Utc);
			_endTime = new DateTime(2016, 05, 17, 12, 0, 0, DateTimeKind.Utc);
			_mainActivity = ActivityFactory.CreateActivity("mainActivity");
			_mainActivityDateTimePeriod = new DateTimePeriod(new DateTime(2016, 05, 17, 09, 0, 0, DateTimeKind.Utc), new DateTime(2016, 05, 17, 17, 0, 0, DateTimeKind.Utc));
			_skill = SkillFactory.CreateSkillWithId("skill", 15);
			_skill2 = SkillFactory.CreateSkillWithId("skill2", 15);
			_mainActivity.RequiresSkill = true;
			_skill.Activity = _mainActivity;
		}

		[Test]
		public void ShouldRaisePersonalActivityAddedEvent()
		{
			var scenario = CurrentScenario.Has("Default");
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);
			var activity = ActivityFactory.CreateActivity("Phone").WithId();
			ActivityRepository.Add(activity);
			ActivityRepository.Add(_mainActivity);
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, _date);
			personAssignment.AddActivity(_mainActivity, _mainActivityDateTimePeriod);
			PersonAssignmentRepository.Add(personAssignment);

			var command = new AddPersonalActivityCommand
			{
				Person = person,
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
			var ass = PersonAssignmentRepository.LoadAll().Single();
			var theEvent = ass.PopAllEvents(null).OfType<PersonalActivityAddedEvent>()
				.Single(e => e.ActivityId == command.PersonalActivityId);

			theEvent.Date.Should().Be.EqualTo(new DateTime(2016, 05, 17));
			theEvent.PersonId.Should().Be.EqualTo(command.Person.Id.GetValueOrDefault());
			theEvent.StartDateTime.Should().Be.EqualTo(command.StartTime);
			theEvent.EndDateTime.Should().Be.EqualTo(command.EndTime);
			theEvent.ScenarioId.Should().Be.EqualTo(scenario.Id.GetValueOrDefault());
			theEvent.LogOnBusinessUnitId.Should().Be(scenario.BusinessUnit.Id.GetValueOrDefault());
			theEvent.CommandId.Should().Be(command.TrackedCommandInfo.TrackId);

		}

		[Test]
		public void ShouldPersistDeltasOfChange()
		{
			var scenario = CurrentScenario.Has("Default");
			SkillRepository.Add(_skill);
			SkillRepository.Add(_skill2);

			Now.Is(new DateTime(2016, 5, 17, 0, 0, 0, DateTimeKind.Utc));
			IntervalLengthFetcher.Has(15);
			var person = PersonRepository.Has(_skill, _skill2);
			var activity = ActivityFactory.CreateActivity("Phone");
			activity.WithId();
			_skill2.Activity = activity;

			activity.RequiresSkill = true;
			ActivityRepository.Add(activity);
			ActivityRepository2.Add(activity);
			ActivityRepository.Add(_mainActivity);
			ActivityRepository2.Add(_mainActivity);
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, _date);
			personAssignment.AddActivity(_mainActivity, _mainActivityDateTimePeriod);
			PersonAssignmentRepository.Add(personAssignment);

			var command = new AddPersonalActivityCommand
			{
				Person = person,
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
			var scenario = CurrentScenario.Has("Default");
			var activity = ActivityFactory.CreateActivity("Phone").WithId();
			ActivityRepository.Add(activity);
			ActivityRepository.Add(_mainActivity);

			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);

			var yesterdayActivityPeriod = new DateTimePeriod(new DateTime(2016, 05, 16, 19, 0, 0, DateTimeKind.Utc), new DateTime(2016, 05, 17, 09, 0, 0, DateTimeKind.Utc));
			var personAssignmentYesterday = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, _date.AddDays(-1));
			personAssignmentYesterday.AddActivity(_mainActivity, yesterdayActivityPeriod);

			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, _date);
			personAssignment.AddActivity(_mainActivity, _mainActivityDateTimePeriod);
			PersonAssignmentRepository.Add(personAssignment);
			PersonAssignmentRepository.Add(personAssignmentYesterday);

			var command = new AddPersonalActivityCommand
			{
				Person = person,
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


		[Test]
		public void ShouldNotReportErrorWhenAddingActivityFromZeroOclockAndPreviousDayIsEmpty()
		{
			var scenario = CurrentScenario.Has("Default");
			var activity = ActivityFactory.CreateActivity("Phone").WithId();
			ActivityRepository.Add(activity);
			ActivityRepository.Add(_mainActivity);

			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);

			var personAssignmentYesterday = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, _date.AddDays(-1));

			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, _date);
			personAssignment.AddActivity(_mainActivity, _mainActivityDateTimePeriod);
			PersonAssignmentRepository.Add(personAssignment);
			PersonAssignmentRepository.Add(personAssignmentYesterday);

			var command = new AddPersonalActivityCommand
			{
				Person = person,
				Date = _date,
				PersonalActivityId = activity.Id.GetValueOrDefault(),
				StartTime = new DateTime(2016, 05, 17, 0, 0, 0, DateTimeKind.Utc),
				EndTime = new DateTime(2016, 05, 17, 09, 0, 0, DateTimeKind.Utc),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = Guid.NewGuid()
				}
			};

			Target.Handle(command);

			command.ErrorMessages.Should().Be.Empty();
		}

		[Test]
		public void ShouldCanAddPersonalActivityIfPersonAssignmentIsNotExists()
		{
			var scenario = CurrentScenario.Has("Default");
			SkillRepository.Add(_skill);
			Now.Is(new DateTime(2016, 5, 17, 0, 0, 0, DateTimeKind.Utc));
			IntervalLengthFetcher.Has(15);

			var person = PersonRepository.Has(_skill, _skill2);
			var activity = ActivityFactory.CreateActivity("Phone");
			activity.WithId();
			_skill2.Activity = activity;

			activity.RequiresSkill = true;
			ActivityRepository.Add(activity);

			var command = new AddPersonalActivityCommand
			{
				Person = person,
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

			command.ErrorMessages.Should().Be.Empty();
			var ass = PersonAssignmentRepository.LoadAll().Single();
			ass.Period.StartDateTime.Should().Be.EqualTo(_startTime);
			ass.Period.EndDateTime.Should().Be.EqualTo(_endTime);
			ass.ShiftLayers.Single().Payload.RequiresSkill.Should().Be.EqualTo(true);
			ass.ShiftLayers.Single().Payload.Name.Should().Be("Phone");
		}

	}
}
