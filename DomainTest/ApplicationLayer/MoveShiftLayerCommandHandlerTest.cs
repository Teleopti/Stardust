using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	public class MoveShiftLayerCommandHandlerTest
	{
		[Test]
		public void ShouldReturnPersonAssignmentIsNotValidDotErrorIfPersonAssignmentNotExists()
		{
			var agent = new Person().WithId();
			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository();
			var scenario = new ThisCurrentScenario(new Scenario("scenario"));
			var personRepository = new FakeWriteSideRepository<IPerson> { agent };
			var target = new MoveShiftLayerCommandHandler( personAssignmentRepository, personRepository, scenario);

			var cmd = new MoveShiftLayerCommand
			{
				AgentId = agent.Id.Value,
				ScheduleDate = new DateOnly(DateTime.UtcNow),
				ShiftLayerId = Guid.NewGuid(),
				NewStartTimeInUtc = new DateTime(2000, 1, 1, 4, 0, 0),
				TrackedCommandInfo = new TrackedCommandInfo { TrackId = Guid.NewGuid() }
			};

			target.Handle(cmd);
			cmd.ErrorMessages.Should().Contain(Resources.PersonAssignmentIsNotValid);
		}
		[Test]
		public void ShouldReturnShiftLayerNotExistErrorIfShiftLayerNotExists()
		{
			var agent = new Person().WithId();
			var activity = new Activity("act").WithId();
			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository
			{
                PersonAssignmentFactory.CreateAssignmentWithMainShift(
                    activity, agent,
                    new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 16))
            };
			var scenario = new ThisCurrentScenario(personAssignmentRepository.Single().Scenario);
            
			var personRepository = new FakeWriteSideRepository<IPerson> { agent };
			var target = new MoveShiftLayerCommandHandler( personAssignmentRepository, personRepository, scenario);

			var cmd = new MoveShiftLayerCommand
			{
				AgentId = agent.Id.Value,
				ScheduleDate = new DateOnly(2013, 11, 14),
				ShiftLayerId = Guid.NewGuid(),
				NewStartTimeInUtc = new DateTime(2013, 11, 14, 9, 0, 0),
				TrackedCommandInfo = new TrackedCommandInfo { TrackId = Guid.NewGuid() }
			};

			target.Handle(cmd);
			cmd.ErrorMessages.Should().Contain(Resources.NoShiftsFound);
		}

		[Test]
		public void ShouldChangeState()
		{
			var agent = new Person().WithId();
			var activity = new Activity("act").WithId();
			var orgStart = createDateTimeUtc(6);
			var orgEnd = createDateTimeUtc(11);
			var userTimeZone = new UtcTimeZone();
			var personAss = createPersonAssignmentWithOneLayer(activity, agent, orgStart, orgEnd, userTimeZone);

			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository
			{
				personAss
			};
			var scenario = new ThisCurrentScenario(personAssignmentRepository.Single().Scenario);
			var personRepository = new FakeWriteSideRepository<IPerson> { agent };
			var personAssignment = personAssignmentRepository.Single();
			var shiftLayer = personAssignment.ShiftLayers.Single();
			shiftLayer.WithId();
			var target = new MoveShiftLayerCommandHandler(personAssignmentRepository, personRepository, scenario);

			var cmd = new MoveShiftLayerCommand
			{
				AgentId = agent.Id.Value,
				ScheduleDate = new DateOnly(2013, 11, 14),
				ShiftLayerId = shiftLayer.Id.GetValueOrDefault(),
				NewStartTimeInUtc = createDateTimeUtc(2),
				TrackedCommandInfo = new TrackedCommandInfo { TrackId = Guid.NewGuid() }
			};

			target.Handle(cmd);

			var expectedStart = cmd.NewStartTimeInUtc;
			var modifiedLayer = personAssignmentRepository.Single().ShiftLayers.Single();
			modifiedLayer.Payload.Should().Be(activity);
			modifiedLayer.Period.StartDateTime.Should().Be(expectedStart);
			modifiedLayer.Period.EndDateTime.Should().Be(expectedStart + (orgEnd - orgStart));
		}

		[Test]
		public void ShouldRaiseActivityMovedEvent()
		{
			var agent = new Person().WithId();
			var activity = new Activity("act").WithId();
			var orgStart = createDateTimeUtc(6);
			var orgEnd = createDateTimeUtc(11);
			var userTimeZone = new UtcTimeZone();
			var personAss = createPersonAssignmentWithOneLayer(activity, agent, orgStart, orgEnd, userTimeZone);

			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository
			{
				personAss
			};
			var scenario = new ThisCurrentScenario(personAssignmentRepository.Single().Scenario);
			var personRepository = new FakeWriteSideRepository<IPerson> { agent };
			var personAssignment = personAssignmentRepository.Single();
			var shiftLayer = personAssignment.ShiftLayers.Single();
			shiftLayer.WithId();
			var target = new MoveShiftLayerCommandHandler(personAssignmentRepository, personRepository, scenario);
			var operatedPersonId = Guid.NewGuid();
			var trackId = Guid.NewGuid();
			var command = new MoveShiftLayerCommand
			{
				AgentId = agent.Id.Value,
				ScheduleDate = new DateOnly(2013, 11, 14),
				ShiftLayerId = shiftLayer.Id.GetValueOrDefault(),
				NewStartTimeInUtc = createDateTimeUtc(2),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = operatedPersonId,
					TrackId = trackId
				}
			};

			target.Handle(command);
			var expectedStartInUtc = command.NewStartTimeInUtc;
			var @event = personAssignmentRepository.Single().PopAllEvents().OfType<ActivityMovedEvent>().Single();
			@event.PersonId.Should().Be(personRepository.Single().Id.Value);
			@event.StartDateTime.Should().Be(expectedStartInUtc);
			@event.ScenarioId.Should().Be(personAssignmentRepository.Single().Scenario.Id.Value);
			@event.InitiatorId.Should().Be(operatedPersonId);
			@event.CommandId.Should().Be(trackId);
			@event.LogOnBusinessUnitId.Should().Be(scenario.Current().BusinessUnit.Id.GetValueOrDefault());
		}

		private static DateTime createDateTimeUtc(int hourOnDay)
		{
			return new DateTime(2013, 11, 14, hourOnDay, 0, 0, 0, DateTimeKind.Utc);
		}

		private static IPersonAssignment createPersonAssignmentWithOneLayer(IActivity activity, IPerson agent, DateTime orgStart, DateTime orgEnd, IUserTimeZone userTimeZone)
		{
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, agent,
				new DateTimePeriod(TimeZoneHelper.ConvertToUtc(orgStart, userTimeZone.TimeZone()),
					TimeZoneHelper.ConvertToUtc(orgEnd, userTimeZone.TimeZone())));
			return assignment;
		}
	}
}