using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	[DomainTest]
	public class AddOvertimeActivityCommandHandlerTest : ISetup
	{
		public AddOvertimeActivityCommandHandler Target;
		public FakePersonRepository PersonRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeCurrentScenario CurrentScenario;
		public FakeScheduleStorage ScheduleStorage;
		public FakeWriteSideRepository<IMultiplicatorDefinitionSet> MultiplicatorDefinitionSetRepository;
		public FakeLoggedOnUser LoggedOnUser;


		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakePersonAssignmentWriteSideRepository>().For<IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey>>();
			system.UseTestDouble<FakeCurrentScenario>().For<ICurrentScenario>();
			system.UseTestDouble<FakeScheduleStorage>().For<IScheduleStorage>();
			system.UseTestDouble<FakeScheduleDifferenceSaver>().For<IScheduleDifferenceSaver>();
			system.UseTestDouble<AddOvertimeActivityCommandHandler>().For<IHandleCommand<AddOvertimeActivityCommand>>();
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble<FakeWriteSideRepository<IMultiplicatorDefinitionSet>>().For<IProxyForId<IMultiplicatorDefinitionSet>>();
		}

		[Test]
		public void ShouldAddOvertimeActivityToEmptyDay()
		{

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
				PersonId = person.Id.GetValueOrDefault(),
				Date = new DateOnly(2013, 11, 14),
				ActivityId = activity.Id.GetValueOrDefault(),
				Period = new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 12),
				MultiplicatorDefinitionSetId = mds.Id.GetValueOrDefault()
			};
			Target.Handle(command);

			var addedPersonAssignment = (PersonAssignment)ScheduleStorage.LoadAll().Single();

			addedPersonAssignment.Date.Should().Be.EqualTo(command.Date);
			addedPersonAssignment.Period.Should().Be.EqualTo(command.Period);
			var overtimeLayer = addedPersonAssignment.ShiftLayers.Single() as OvertimeShiftLayer;
			overtimeLayer.Should().Not.Be.Null();
			overtimeLayer.DefinitionSet.Should().Be.EqualTo(mds);
		}


		[Test]
		public void ShouldRaiseAddOvertimeActivityEvent()
		{
			var scenario = CurrentScenario.Current();
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);
			var activity = ActivityFactory.CreateActivity("Phone").WithId();
			var  mainActivity = ActivityFactory.CreateActivity("Phone").WithId();
			ActivityRepository.Add(activity);
			ActivityRepository.Add(mainActivity);

			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(person, CurrentScenario.Current(), new DateOnly(2013, 11, 14));
			personAssignment.AddActivity(mainActivity, new DateTimePeriod(2013, 11, 14, 6, 2013, 11, 14, 9));
			ScheduleStorage.Add(personAssignment);

			var mds = MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("double pay", MultiplicatorType.Overtime);
			mds.WithId();
			MultiplicatorDefinitionSetRepository.Add(mds);

			var command = new AddOvertimeActivityCommand
			{
				PersonId = person.Id.GetValueOrDefault(),
				Date = new DateOnly(2013, 11, 14),
				ActivityId = activity.Id.GetValueOrDefault(),
				Period = new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 12),
				MultiplicatorDefinitionSetId = mds.Id.GetValueOrDefault(),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = new Guid(),
					TrackId = new Guid()
				}
			};
			Target.Handle(command);

			var ass = (PersonAssignment) ScheduleStorage.LoadAll().Single();

			var addOvertimeEvent = ass.PopAllEvents().OfType<ActivityAddedEvent>()
				.Single(e => e.ActivityId == command.ActivityId);
		
			addOvertimeEvent.Date.Should().Be.EqualTo(new DateTime(2013, 11, 14));
			addOvertimeEvent.PersonId.Should().Be.EqualTo(command.PersonId);
			addOvertimeEvent.StartDateTime.Should().Be.EqualTo(command.Period.StartDateTime);
			addOvertimeEvent.EndDateTime.Should().Be.EqualTo(command.Period.EndDateTime);
			addOvertimeEvent.ScenarioId.Should().Be.EqualTo(scenario.Id.GetValueOrDefault());
			addOvertimeEvent.InitiatorId.Should().Be.EqualTo(command.TrackedCommandInfo.OperatedPersonId);
			addOvertimeEvent.CommandId.Should().Be.EqualTo(command.TrackedCommandInfo.TrackId);
			addOvertimeEvent.LogOnBusinessUnitId.Should().Be(scenario.BusinessUnit.Id.GetValueOrDefault());
		}
	}

	[TestFixture]
	[DomainTest]
	public class AddOvertimeActivityCommandHandlerNoDeltasTest : ISetup
	{
		public AddOvertimeActivityCommandHandlerNoDeltas Target;
		public FakeWriteSideRepository<IPerson> PersonRepository;
		public FakeWriteSideRepository<IActivity> ActivityRepository;
		public FakePersonAssignmentWriteSideRepository PersonAssignmentRepo;
		public FakeCurrentScenario CurrentScenario;
		public FakeScheduleStorage ScheduleStorage;
		public FakeWriteSideRepository<IMultiplicatorDefinitionSet> MultiplicatorDefinitionSetRepository;
		public FakeLoggedOnUser LoggedOnUser;


		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakePersonAssignmentWriteSideRepository>().For<IWriteSideRepositoryTypedId<IPersonAssignment,PersonAssignmentKey>>();
			system.UseTestDouble<FakeWriteSideRepository<IActivity>>().For<IProxyForId<IActivity>>();
			system.UseTestDouble<FakeWriteSideRepository<IPerson>>().For<IProxyForId<IPerson>>();
			system.UseTestDouble<FakeCurrentScenario>().For<ICurrentScenario>();
			system.UseTestDouble<FakeScheduleStorage>().For<IScheduleStorage>();
			system.UseTestDouble<AddOvertimeActivityCommandHandlerNoDeltas>().For<IHandleCommand<AddOvertimeActivityCommand>>();
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble<FakeWriteSideRepository<IMultiplicatorDefinitionSet>>().For<IProxyForId<IMultiplicatorDefinitionSet>>();
		}

		[Test]
		public void ShouldAddOvertimeActivityToEmptyDay()
		{
		
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
				PersonId = person.Id.Value,
				Date = new DateOnly(2013,11,14),
				ActivityId = activity.Id.Value,
				Period = new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 12),
				MultiplicatorDefinitionSetId = mds.Id.Value
			};
			Target.Handle(command);

			var addedPersonAssignment = PersonAssignmentRepo.Single();

			addedPersonAssignment.Date.Should().Be.EqualTo(command.Date);
			addedPersonAssignment.Period.Should().Be.EqualTo(command.Period);
			var overtimeLayer = addedPersonAssignment.ShiftLayers.Single() as OvertimeShiftLayer;
			overtimeLayer.Should().Not.Be.Null();
			overtimeLayer.DefinitionSet.Should().Be.EqualTo(mds);
		}


		[Test]
		public void ShouldRaiseAddOvertimeActivityEvent()
		{
			var scenario = CurrentScenario.Current();
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);
			var activity = ActivityFactory.CreateActivity("Phone");
			activity.WithId();
			ActivityRepository.Add(activity);

			var mds = MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("double pay",MultiplicatorType.Overtime);
			mds.WithId();
			MultiplicatorDefinitionSetRepository.Add(mds);

			var command = new AddOvertimeActivityCommand
			{
				PersonId = person.Id.Value,
				Date = new DateOnly(2013,11,14),
				ActivityId = activity.Id.Value,
				Period = new DateTimePeriod(2013,11,14,8,2013,11,14,12),
				MultiplicatorDefinitionSetId = mds.Id.Value,
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = new Guid(),
					TrackId = new Guid()
				}
			};
			Target.Handle(command);

			var addOvertimeEvent = PersonAssignmentRepo.Single().PopAllEvents().OfType<ActivityAddedEvent>()
				.Single(e => e.ActivityId == command.ActivityId);

			addOvertimeEvent.Date.Should().Be.EqualTo(new DateTime(2013, 11, 14));
			addOvertimeEvent.PersonId.Should().Be.EqualTo(command.PersonId);
			addOvertimeEvent.StartDateTime.Should().Be.EqualTo(command.Period.StartDateTime);
			addOvertimeEvent.EndDateTime.Should().Be.EqualTo(command.Period.EndDateTime);
			addOvertimeEvent.ScenarioId.Should().Be.EqualTo(scenario.Id.Value);
			addOvertimeEvent.InitiatorId.Should().Be.EqualTo(command.TrackedCommandInfo.OperatedPersonId);
			addOvertimeEvent.CommandId.Should().Be.EqualTo(command.TrackedCommandInfo.TrackId);
			addOvertimeEvent.LogOnBusinessUnitId.Should().Be(scenario.BusinessUnit.Id.GetValueOrDefault());
		}
	}
}
