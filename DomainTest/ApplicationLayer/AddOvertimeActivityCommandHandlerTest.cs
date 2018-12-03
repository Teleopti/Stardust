using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	[DomainTest]
	public class AddOvertimeActivityCommandHandlerTest : IIsolateSystem
	{
		public AddOvertimeActivityCommandHandler Target;
		public FakePersonRepository PersonRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeScenarioRepository CurrentScenario;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeWriteSideRepository<IMultiplicatorDefinitionSet> MultiplicatorDefinitionSetRepository;
		public FakeLoggedOnUser LoggedOnUser;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakePersonAssignmentWriteSideRepository>().For<IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey>>();
			isolate.UseTestDouble<FakePersonAssignmentRepository>().For<IPersonAssignmentRepository>();
			isolate.UseTestDouble<AddOvertimeActivityCommandHandler>().For<IHandleCommand<AddOvertimeActivityCommand>>();
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			isolate.UseTestDouble<FakeWriteSideRepository<IMultiplicatorDefinitionSet>>().For<IProxyForId<IMultiplicatorDefinitionSet>>();
		}

		[Test]
		public void ShouldAddOvertimeActivityToEmptyDay()
		{
			CurrentScenario.Has("Default");
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);
			var activity = ActivityFactory.CreateActivity("Phone");
			activity.WithId();
			ActivityRepository.Add(activity);

			var mds = MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("double pay", MultiplicatorType.Overtime);
			mds.WithId();
			MultiplicatorDefinitionSetRepository.Add(mds);

			var command = new AddOvertimeActivityCommand
			{
				Person = person,
				Date = new DateOnly(2013, 11, 14),
				ActivityId = activity.Id.GetValueOrDefault(),
				Period = new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 12),
				MultiplicatorDefinitionSetId = mds.Id.GetValueOrDefault()
			};
			Target.Handle(command);

			var addedPersonAssignment = PersonAssignmentRepository.LoadAll().Single();

			addedPersonAssignment.Date.Should().Be.EqualTo(command.Date);
			addedPersonAssignment.Period.Should().Be.EqualTo(command.Period);
			var overtimeLayer = addedPersonAssignment.ShiftLayers.Single() as OvertimeShiftLayer;
			overtimeLayer.Should().Not.Be.Null();
			overtimeLayer.DefinitionSet.Should().Be.EqualTo(mds);
		}


		[Test]
		public void ShouldRaiseAddOvertimeActivityEvent()
		{
			var scenario = CurrentScenario.Has("Default");
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);
			var activity = ActivityFactory.CreateActivity("Phone").WithId();
			var mainActivity = ActivityFactory.CreateActivity("Phone").WithId();
			ActivityRepository.Add(activity);
			ActivityRepository.Add(mainActivity);

			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(2013, 11, 14));
			personAssignment.AddActivity(mainActivity, new DateTimePeriod(2013, 11, 14, 6, 2013, 11, 14, 9));
			PersonAssignmentRepository.Add(personAssignment);

			var mds = MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("double pay", MultiplicatorType.Overtime);
			mds.WithId();
			MultiplicatorDefinitionSetRepository.Add(mds);

			var command = new AddOvertimeActivityCommand
			{
				Person = person,
				Date = new DateOnly(2013, 11, 14),
				ActivityId = activity.Id.GetValueOrDefault(),
				Period = new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 12),
				MultiplicatorDefinitionSetId = mds.Id.GetValueOrDefault(),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = Guid.NewGuid()
				}
			};
			Target.Handle(command);

			var ass = PersonAssignmentRepository.LoadAll().Single();

			var addOvertimeEvent = ass.PopAllEvents().OfType<ActivityAddedEvent>()
				.Single(e => e.ActivityId == command.ActivityId);

			addOvertimeEvent.Date.Should().Be.EqualTo(new DateTime(2013, 11, 14));
			addOvertimeEvent.PersonId.Should().Be.EqualTo(command.Person.Id.GetValueOrDefault());
			addOvertimeEvent.StartDateTime.Should().Be.EqualTo(command.Period.StartDateTime);
			addOvertimeEvent.EndDateTime.Should().Be.EqualTo(command.Period.EndDateTime);
			addOvertimeEvent.ScenarioId.Should().Be.EqualTo(scenario.Id.GetValueOrDefault());
			addOvertimeEvent.InitiatorId.Should().Be.EqualTo(command.TrackedCommandInfo.OperatedPersonId);
			addOvertimeEvent.CommandId.Should().Be.EqualTo(command.TrackedCommandInfo.TrackId);
			addOvertimeEvent.LogOnBusinessUnitId.Should().Be(scenario.BusinessUnit.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldNotAddOvertimeActivityWhenActivityPeriodStartDateAndScheduleDateDontMatch()
		{
			var scenario = CurrentScenario.Has("Default");
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);
			var activity = ActivityFactory.CreateActivity("Phone").WithId();
			var mainActivity = ActivityFactory.CreateActivity("Phone").WithId();
			ActivityRepository.Add(activity);
			ActivityRepository.Add(mainActivity);

			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(2013, 11, 14));
			personAssignment.AddActivity(mainActivity, new DateTimePeriod(2013, 11, 14, 6, 2013, 11, 14, 9));
			PersonAssignmentRepository.Add(personAssignment);

			var mds = MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("double pay", MultiplicatorType.Overtime);
			mds.WithId();
			MultiplicatorDefinitionSetRepository.Add(mds);

			var command = new AddOvertimeActivityCommand
			{
				Person = person,
				Date = new DateOnly(2013, 11, 14),
				ActivityId = activity.Id.GetValueOrDefault(),
				Period = new DateTimePeriod(2013, 11, 13, 8, 2013, 11, 13, 12),
				MultiplicatorDefinitionSetId = mds.Id.GetValueOrDefault(),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = Guid.NewGuid()
				}
			};
			Target.Handle(command);

			command.ErrorMessages.Single().Should().Be.EqualTo(Resources.InvalidInput);

		}

		[Test]
		public void ShouldNotThrowPermissionCheckExceptionWhenAddOvertimeActivityToAgentWithoutSchedulePeriod()
		{
			var scenario = CurrentScenario.Has("Default");
			var person = PersonFactory.CreatePersonWithId();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.MountainTimeZoneInfo());
			PersonRepository.Add(person);
			var activity = ActivityFactory.CreateActivity("Phone").WithId();
			var mainActivity = ActivityFactory.CreateActivity("Phone").WithId();
			ActivityRepository.Add(activity);
			ActivityRepository.Add(mainActivity);
			
			var mds = MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("double pay", MultiplicatorType.Overtime);
			mds.WithId();
			MultiplicatorDefinitionSetRepository.Add(mds);

			var command = new AddOvertimeActivityCommand
			{
				Person = person,
				Date = new DateOnly(2013, 11, 14),
				ActivityId = activity.Id.GetValueOrDefault(),
				Period = new DateTimePeriod(2013, 11, 14, 13, 2013, 11, 14, 18),
				MultiplicatorDefinitionSetId = mds.Id.GetValueOrDefault(),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = Guid.NewGuid()
				}
			};
			Target.Handle(command);

			var addedPersonAssignment = PersonAssignmentRepository.LoadAll().Single();

			addedPersonAssignment.Date.Should().Be.EqualTo(command.Date);
			addedPersonAssignment.Period.Should().Be.EqualTo(command.Period);
			var overtimeLayer = addedPersonAssignment.ShiftLayers.Single() as OvertimeShiftLayer;
			overtimeLayer.Should().Not.Be.Null();
			overtimeLayer.DefinitionSet.Should().Be.EqualTo(mds);

		}

		[Test]
		public void ShouldNotAddOvertimeActivityWhenActivityPeriodStartDateBeforeScheduleDateAndIntersectedWithAssPeriod()
		{
			var scenario = CurrentScenario.Has("Default");
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);
			var activity = ActivityFactory.CreateActivity("Phone").WithId();
			var mainActivity = ActivityFactory.CreateActivity("Phone").WithId();
			ActivityRepository.Add(activity);
			ActivityRepository.Add(mainActivity);

			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(2013, 11, 14));
			personAssignment.AddActivity(mainActivity, new DateTimePeriod(2013, 11, 14, 6, 2013, 11, 14, 9));
			PersonAssignmentRepository.Add(personAssignment);

			var mds = MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("double pay", MultiplicatorType.Overtime);
			mds.WithId();
			MultiplicatorDefinitionSetRepository.Add(mds);

			var command = new AddOvertimeActivityCommand
			{
				Person = person,
				Date = new DateOnly(2013, 11, 14),
				ActivityId = activity.Id.GetValueOrDefault(),
				Period = new DateTimePeriod(2013, 11, 13, 22, 2013, 11, 14, 6),
				MultiplicatorDefinitionSetId = mds.Id.GetValueOrDefault(),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = Guid.NewGuid()
				}
			};
			Target.Handle(command);

			command.ErrorMessages.Single().Should().Be.EqualTo(Resources.InvalidInput);
		}

		[Test]
		public void ShouldAddOvertimeActivityWhenActivityPeriodStartDateAfterScheduleDateAndIntersectedWithAssPeriod()
		{
			var scenario = CurrentScenario.Has("Default");
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);
			var activity = ActivityFactory.CreateActivity("Phone").WithId();
			var mainActivity = ActivityFactory.CreateActivity("Phone").WithId();
			ActivityRepository.Add(activity);
			ActivityRepository.Add(mainActivity);

			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(2013, 11, 14));
			personAssignment.AddActivity(mainActivity, new DateTimePeriod(2013, 11, 14, 6, 2013, 11, 15, 2));
			PersonAssignmentRepository.Add(personAssignment);

			var mds = MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("double pay", MultiplicatorType.Overtime);
			mds.WithId();
			MultiplicatorDefinitionSetRepository.Add(mds);

			var command = new AddOvertimeActivityCommand
			{
				Person = person,
				Date = new DateOnly(2013, 11, 14),
				ActivityId = activity.Id.GetValueOrDefault(),
				Period = new DateTimePeriod(2013, 11, 15, 2, 2013, 11, 15, 3),
				MultiplicatorDefinitionSetId = mds.Id.GetValueOrDefault(),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = Guid.NewGuid()
				}
			};
			Target.Handle(command);

			command.ErrorMessages.Count.Should().Be.EqualTo(0);
			var assignment = PersonAssignmentRepository.LoadAll().Single();

			assignment.Date.Should().Be.EqualTo(command.Date);
			assignment.Period.Should().Be.EqualTo(new DateTimePeriod(2013, 11, 14, 6, 2013, 11, 15, 3));
			var overtimeLayer = assignment.ShiftLayers.Single(l => l is OvertimeShiftLayer) as OvertimeShiftLayer;
			overtimeLayer.Should().Not.Be.Null();
			overtimeLayer.DefinitionSet.Should().Be.EqualTo(mds);
		}

		[Test]
		public void ShouldAddOvertimeActivityWhenActivityPeriodStartOnScheduleDateInAgentTimezone()
		{
			var scenario = CurrentScenario.Has("Default");
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);
			var activity = ActivityFactory.CreateActivity("Phone").WithId();
			var mainActivity = ActivityFactory.CreateActivity("Phone").WithId();
			ActivityRepository.Add(activity);
			ActivityRepository.Add(mainActivity);

			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(2013, 11, 14));
			personAssignment.AddActivity(mainActivity, new DateTimePeriod(2013, 11, 14, 6, 2013, 11, 14, 9));
			PersonAssignmentRepository.Add(personAssignment);

			var mds = MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("double pay", MultiplicatorType.Overtime);
			mds.WithId();
			MultiplicatorDefinitionSetRepository.Add(mds);

			var command = new AddOvertimeActivityCommand
			{
				Person = person,
				Date = new DateOnly(2013, 11, 14),
				ActivityId = activity.Id.GetValueOrDefault(),
				Period = new DateTimePeriod(2013, 11, 14, 10, 2013, 11, 14, 12),
				MultiplicatorDefinitionSetId = mds.Id.GetValueOrDefault(),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = Guid.NewGuid()
				}
			};
			Target.Handle(command);

			var assignment = PersonAssignmentRepository.LoadAll().Single();

			assignment.Date.Should().Be.EqualTo(command.Date);
			assignment.Period.Should().Be.EqualTo(new DateTimePeriod(2013, 11, 14, 6, 2013, 11, 14, 12));
			var overtimeLayer = assignment.ShiftLayers.Single(l => l is OvertimeShiftLayer) as OvertimeShiftLayer;
			overtimeLayer.Should().Not.Be.Null();
			overtimeLayer.DefinitionSet.Should().Be.EqualTo(mds);
			command.ErrorMessages.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldAllowToAddDisconnectedOvertimeActivityToADayWithShift()
		{
			var scenario = CurrentScenario.Has("Default");
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);

			var activity = ActivityFactory.CreateActivity("Phone").WithId();
			var mainActivity = ActivityFactory.CreateActivity("Phone").WithId();
			ActivityRepository.Add(activity);
			ActivityRepository.Add(mainActivity);

			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(2013, 11, 14));
			personAssignment.AddActivity(mainActivity, new DateTimePeriod(2013, 11, 14, 18, 2013, 11, 15, 1));
			PersonAssignmentRepository.Add(personAssignment);

			var mds = MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("double pay", MultiplicatorType.Overtime);
			mds.WithId();
			MultiplicatorDefinitionSetRepository.Add(mds);

			var command = new AddOvertimeActivityCommand
			{
				Person = person,
				AllowDisconnected = true,
				Date = new DateOnly(2013, 11, 14),
				ActivityId = activity.Id.GetValueOrDefault(),
				Period = new DateTimePeriod(2013, 11, 15, 2, 2013, 11, 15, 4),
				MultiplicatorDefinitionSetId = mds.Id.GetValueOrDefault()
			};
			Target.Handle(command);

			var addedPersonAssignment = PersonAssignmentRepository.LoadAll().LastOrDefault();

			addedPersonAssignment.Date.Should().Be.EqualTo(command.Date);
			addedPersonAssignment.Period.Should().Be.EqualTo(new DateTimePeriod(2013, 11, 14, 18, 2013, 11, 15, 4));
			var overtimeLayer = addedPersonAssignment.ShiftLayers.Last() as OvertimeShiftLayer;
			overtimeLayer.Should().Not.Be.Null();
			overtimeLayer.DefinitionSet.Should().Be.EqualTo(mds);
			overtimeLayer.Period.StartDateTime.Should().Be(command.Period.StartDateTime);
			overtimeLayer.Period.EndDateTime.Should().Be(command.Period.EndDateTime);
		}
	}
}
