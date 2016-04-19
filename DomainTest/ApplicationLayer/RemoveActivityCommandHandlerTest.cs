using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Util;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	public class RemoveActivityCommandHandlerTest
	{
		[Test]
		public void ShouldRaiseRemoveActivityEvent()
		{
			var personRepository = new FakeWriteSideRepository<IPerson> {PersonFactory.CreatePersonWithId()};
			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository
			{
				PersonAssignmentFactory.CreateAssignmentWithMainShift(
					mainActivity, personRepository.Single(),
					new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 16))
			};
			var scenario = new ThisCurrentScenario(personAssignmentRepository.Single().Scenario);


			var personAssignment = personAssignmentRepository.Single();
			var shiftLayer = personAssignment.ShiftLayers.Single();
			shiftLayer.WithId();


			var target = new RemoveActivityCommandHandler(personAssignmentRepository, scenario, personRepository);

			var command = new RemoveActivityCommand
			{
				PersonId = personRepository.Single().Id.Value,
				ShiftLayerId = shiftLayer.Id.Value,
				Date = new DateOnly(2013, 11, 14),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = Guid.NewGuid()
				}
			};

			target.Handle(command);

			var @event = personAssignment.PopAllEvents(new Now()).OfType<PersonAssignmentLayerRemovedEvent>().Single();
			@event.PersonId.Should().Be(personRepository.Single().Id.Value);
			@event.Date.Should().Be(new DateTime(2013, 11, 14));
			@event.StartDateTime.Should().Be(shiftLayer.Period.StartDateTime);
			@event.EndDateTime.Should().Be(shiftLayer.Period.EndDateTime);

		}

		[Test]
		public void ShouldSetupEntityState()
		{
			var personRepository = new FakeWriteSideRepository<IPerson> {PersonFactory.CreatePersonWithId()};
			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository
			{
				PersonAssignmentFactory.CreateAssignmentWithMainShift(
					mainActivity, personRepository.Single(),
					new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 16))
			};
			var scenario = new ThisCurrentScenario(personAssignmentRepository.Single().Scenario);


			var personAssignment = personAssignmentRepository.Single();
			var shiftLayer = personAssignment.ShiftLayers.Single();
			shiftLayer.WithId();


			var target = new RemoveActivityCommandHandler(personAssignmentRepository, scenario, personRepository);

			var command = new RemoveActivityCommand
			{
				PersonId = personRepository.Single().Id.Value,
				ShiftLayerId = shiftLayer.Id.Value,
				Date = new DateOnly(2013, 11, 14),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = Guid.NewGuid()
				}
			};

			target.Handle(command);

			personAssignment.ShiftLayers.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReportErrorWhenShiftLayerNotFound()
		{
			var personRepository = new FakeWriteSideRepository<IPerson> { PersonFactory.CreatePersonWithId() };
			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository
			{
				PersonAssignmentFactory.CreateAssignmentWithMainShift(
					mainActivity, personRepository.Single(),
					new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 16))
			};
			var scenario = new ThisCurrentScenario(personAssignmentRepository.Single().Scenario);


			var personAssignment = personAssignmentRepository.Single();
			var shiftLayer = personAssignment.ShiftLayers.Single();
			shiftLayer.WithId();

			var invalidLayerId = Guid.NewGuid();

			var target = new RemoveActivityCommandHandler(personAssignmentRepository,scenario,personRepository);

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
