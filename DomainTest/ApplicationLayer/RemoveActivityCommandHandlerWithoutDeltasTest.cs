using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	[TestWithStaticDependenciesAvoidUse]
	[DisabledBy(Toggles.Staffing_ReadModel_BetterAccuracy_Step4_43389)]
	public class RemoveActivityCommandHandlerWithoutDeltasTest
	{
		[Test]
		public void ShouldRaiseRemoveActivityEvent()
		{
			var personRepository = new FakeWriteSideRepository<IPerson> {PersonFactory.CreatePersonWithId()};
			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var otherActivity = ActivityFactory.CreateActivity("Admin");

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(personRepository.Single(),
				mainActivity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 16));

			pa.AddActivity(otherActivity,new DateTimePeriod(2013,11,14,12,2013,11,14,14));

			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository {pa};			

			var scenario = new ThisCurrentScenario(personAssignmentRepository.Single().Scenario);

			var personAssignment = personAssignmentRepository.Single();
			personAssignment.ShiftLayers.ForEach(sl => sl.WithId());
			
			var target = new RemoveActivityCommandHandlerWithoutDeltas(personAssignmentRepository, scenario, personRepository);

			var otherActivityLayer = personAssignment.ShiftLayers.First(sl => sl.Payload == otherActivity);

			var command = new RemoveActivityCommand
			{
				PersonId = personRepository.Single().Id.Value,
				ShiftLayerId = otherActivityLayer.Id.Value,
				Date = new DateOnly(2013, 11, 14),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = Guid.NewGuid()
				}
			};

			target.Handle(command);

			var @event = personAssignment.PopAllEvents().OfType<PersonAssignmentLayerRemovedEvent>().Single();
			@event.PersonId.Should().Be(personRepository.Single().Id.Value);
			@event.Date.Should().Be(new DateTime(2013, 11, 14));
			@event.StartDateTime.Should().Be(otherActivityLayer.Period.StartDateTime);
			@event.EndDateTime.Should().Be(otherActivityLayer.Period.EndDateTime);

		}

		[Test]
		public void ShouldSetupEntityStateForMainShiftLayer()
		{
			var personRepository = new FakeWriteSideRepository<IPerson> { PersonFactory.CreatePersonWithId() };
			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var otherActivity = ActivityFactory.CreateActivity("Admin");

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				personRepository.Single(),
				mainActivity, new DateTimePeriod(2013,11,14,8,2013,11,14,16));

			pa.AddActivity(otherActivity,new DateTimePeriod(2013,11,14,12,2013,11,14,14));

			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository { pa };

			var scenario = new ThisCurrentScenario(personAssignmentRepository.Single().Scenario);

			var personAssignment = personAssignmentRepository.Single();
			personAssignment.ShiftLayers.ForEach(sl => sl.WithId());

			var target = new RemoveActivityCommandHandlerWithoutDeltas(personAssignmentRepository,scenario,personRepository);

			var otherActivityLayer = personAssignment.ShiftLayers.First(sl => sl.Payload == otherActivity);

			var command = new RemoveActivityCommand
			{
				PersonId = personRepository.Single().Id.Value,
				ShiftLayerId = otherActivityLayer.Id.Value,
				Date = new DateOnly(2013,11,14),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = Guid.NewGuid()
				}
			};

			target.Handle(command);

			personAssignment.ShiftLayers.Count().Should().Be.EqualTo(1);
		}
		[Test]
		public void ShouldSetupEntityStateForPersonalShiftLayer()
		{
			var personRepository = new FakeWriteSideRepository<IPerson> { PersonFactory.CreatePersonWithId() };
			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var otherActivity = ActivityFactory.CreateActivity("Admin");

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				personRepository.Single(),
				mainActivity, new DateTimePeriod(2013,11,14,8,2013,11,14,16));

			pa.AddPersonalActivity(otherActivity,new DateTimePeriod(2013,11,14,12,2013,11,14,14));

			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository { pa };

			var scenario = new ThisCurrentScenario(personAssignmentRepository.Single().Scenario);

			var personAssignment = personAssignmentRepository.Single();
			personAssignment.ShiftLayers.ForEach(sl => sl.WithId());

			var target = new RemoveActivityCommandHandlerWithoutDeltas(personAssignmentRepository,scenario,personRepository);

			var otherActivityLayer = personAssignment.ShiftLayers.First(sl => sl.Payload == otherActivity);

			var command = new RemoveActivityCommand
			{
				PersonId = personRepository.Single().Id.Value,
				ShiftLayerId = otherActivityLayer.Id.Value,
				Date = new DateOnly(2013,11,14),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = Guid.NewGuid()
				}
			};

			target.Handle(command);

			personAssignment.ShiftLayers.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldReportErrorWhenRemoveBaseShiftLayer()
		{
			var personRepository = new FakeWriteSideRepository<IPerson> { PersonFactory.CreatePersonWithId() };
			var mainActivity = ActivityFactory.CreateActivity("Phone");

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				personRepository.Single(),
				mainActivity, new DateTimePeriod(2013,11,14,8,2013,11,14,16));
		
			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository { pa };

			var scenario = new ThisCurrentScenario(personAssignmentRepository.Single().Scenario);

			var personAssignment = personAssignmentRepository.Single();
			personAssignment.ShiftLayers.ForEach(sl => sl.WithId());

			var target = new RemoveActivityCommandHandlerWithoutDeltas(personAssignmentRepository,scenario,personRepository);
			var shiftLayer = personAssignment.ShiftLayers.First();

			var command = new RemoveActivityCommand
			{
				PersonId = personRepository.Single().Id.Value,
				ShiftLayerId = shiftLayer.Id.Value,
				Date = new DateOnly(2013,11,14),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = Guid.NewGuid()
				}
			};

			target.Handle(command);

			personAssignment.ShiftLayers.Count().Should().Be.EqualTo(1);
			command.ErrorMessages.Single().Should().Be.EqualTo(Resources.CannotDeleteBaseActivity);
		}

		[Test]
		public void ShouldReportErrorWhenShiftLayerNotFound()
		{
			var personRepository = new FakeWriteSideRepository<IPerson> { PersonFactory.CreatePersonWithId() };
			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository
			{
				PersonAssignmentFactory.CreateAssignmentWithMainShift(personRepository.Single(),
					mainActivity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 16))
			};
			var scenario = new ThisCurrentScenario(personAssignmentRepository.Single().Scenario);


			var personAssignment = personAssignmentRepository.Single();
			var shiftLayer = personAssignment.ShiftLayers.Single();
			shiftLayer.WithId();

			var invalidLayerId = Guid.NewGuid();

			var target = new RemoveActivityCommandHandlerWithoutDeltas(personAssignmentRepository,scenario,personRepository);

			var command = new RemoveActivityCommand
			{
				PersonId = personRepository.Single().Id.Value,
				ShiftLayerId = invalidLayerId,
				Date = new DateOnly(2013,11,14),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = Guid.NewGuid()
				}
			};

			target.Handle(command);

			personAssignment.ShiftLayers.Count().Should().Be.EqualTo(1);

			command.ErrorMessages.Single().Should().Be.EqualTo(Resources.NoShiftsFound);

		}

	}
}
