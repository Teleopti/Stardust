using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestWithStaticDependenciesDONOTUSE]
	public class MoveActivityCommandHandlerTest
	{
		[Test]
		public void ShouldDoThrowIfPersonAssignmentNotExists()
		{
			var agent = new Person().WithId();
			var activity = new Activity("act").WithId();
			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository(null);
			var scenario = new ThisCurrentScenario(new Scenario("scenario"));
			var personRepository = new FakeWriteSideRepository<IPerson>(null) { agent };
			var target = new MoveActivityCommandHandler(personAssignmentRepository, personRepository, new FakeWriteSideRepository<IActivity>(null) { activity}, scenario, new UtcTimeZone());

			var cmd = new MoveActivityCommand
			{
				AgentId = agent.Id.Value,
				ScheduleDate = DateOnly.Today,
				ActivityId = activity.Id.Value,
				NewStartTime = new DateTime(2000, 1, 1, 4, 0, 0),
				OldStartTime = new DateTime(2000,1,1),
				OldProjectionLayerLength = 120
			};

			var ex = Assert.Throws<InvalidOperationException>(()=>target.Handle(cmd)).ToString();
			ex.Should().Contain(cmd.AgentId.ToString());
			ex.Should().Contain(cmd.ScheduleDate.ToString());
			ex.Should().Contain(scenario.Current().Description.ToString());
		}

		[Test]
		public void ShouldChangeState()
		{
			var agent = new Person().WithId();
			var activity = new Activity("_").WithId();
			var orgStart = createDateTimeLocal(6);
			var orgEnd = createDateTimeLocal(11);
			var userTimeZone = new UtcTimeZone();
			var assignment = createPersonAssignmentWithOneLayer(activity, agent, orgStart, orgEnd, userTimeZone);

			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository(null) { assignment };
			var activityRepository = new FakeWriteSideRepository<IActivity>(null) { activity };
			var scenario = new ThisCurrentScenario(personAssignmentRepository.Single().Scenario);
			var personRepository = new FakeWriteSideRepository<IPerson>(null) { agent };
			var target = new MoveActivityCommandHandler(personAssignmentRepository, personRepository, activityRepository, scenario, userTimeZone);

			var cmd = new MoveActivityCommand
			{
				AgentId = agent.Id.Value,
				ScheduleDate = assignment.Date,
				ActivityId = activity.Id.Value,
				NewStartTime = createDateTimeLocal(2),
				OldStartTime = orgStart,
				OldProjectionLayerLength = Convert.ToInt32((orgEnd - orgStart).TotalMinutes)
			};

			target.Handle(cmd);

			var expectedStart = cmd.NewStartTime;
			var modifiedLayer = personAssignmentRepository.Single().ShiftLayers.Single();
			modifiedLayer.Payload.Should().Be(activity);
			modifiedLayer.Period.StartDateTime.Should().Be(expectedStart);
			modifiedLayer.Period.EndDateTime.Should().Be(expectedStart + (orgEnd - orgStart));
		}

		[Test]
		public void ShouldHandleTimeZoneForStartTime()
		{
			var agent = new Person().WithId();
			var activity = new Activity("_").WithId();
			var orgStart = createDateTimeLocal(6);
			var orgEnd = createDateTimeLocal(11);
			var userTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
			var userTimeZone = new SpecificTimeZone(userTimeZoneInfo);
			var assignment = createPersonAssignmentWithOneLayer(activity, agent, orgStart, orgEnd, userTimeZone);

			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository(null) { assignment };
			var activityRepository = new FakeWriteSideRepository<IActivity>(null) { activity };
			var scenario = new ThisCurrentScenario(personAssignmentRepository.Single().Scenario);
			var personRepository = new FakeWriteSideRepository<IPerson>(null) { agent };
			var target = new MoveActivityCommandHandler(personAssignmentRepository, personRepository, activityRepository, scenario, userTimeZone);

			var cmd = new MoveActivityCommand
			{
				AgentId = agent.Id.Value,
				ScheduleDate = assignment.Date,
				ActivityId = activity.Id.Value,
				NewStartTime = createDateTimeLocal(2),
				OldStartTime = orgStart,
				OldProjectionLayerLength = Convert.ToInt32((orgEnd - orgStart).TotalMinutes)
			};

			target.Handle(cmd);

			var expectedStartInLocal = new DateTime(cmd.NewStartTime.Ticks, DateTimeKind.Local);
			var expectedStartInUtc = TimeZoneHelper.ConvertToUtc(expectedStartInLocal, userTimeZoneInfo);
			var modifiedLayer = personAssignmentRepository.Single().ShiftLayers.Single();
			modifiedLayer.Period.StartDateTime.Should().Be(expectedStartInUtc);
		}

		[Test]
		public void ShouldRaiseActivityMovedEvent()
		{
			var agent = new Person().WithId();
			var activity = new Activity("_").WithId();
			var orgStart = createDateTimeLocal(6);
			var orgEnd = createDateTimeLocal(11);
			var userTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
			var userTimeZone = new SpecificTimeZone(userTimeZoneInfo);
			var assignment = createPersonAssignmentWithOneLayer(activity, agent, orgStart, orgEnd, userTimeZone);

			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository(null) { assignment };
			var activityRepository = new FakeWriteSideRepository<IActivity>(null) { activity };
			var scenario = new ThisCurrentScenario(personAssignmentRepository.Single().Scenario);
			var personRepository = new FakeWriteSideRepository<IPerson>(null) { agent };
			var target = new MoveActivityCommandHandler(personAssignmentRepository, personRepository, activityRepository, scenario, userTimeZone);
			var operatedPersonId = Guid.NewGuid();
			var trackId = Guid.NewGuid();
			var command = new MoveActivityCommand
			{
				AgentId = agent.Id.Value,
				ScheduleDate = assignment.Date,
				ActivityId = activity.Id.Value,
				NewStartTime = createDateTimeLocal(2),
				OldStartTime = orgStart,
				OldProjectionLayerLength = Convert.ToInt32((orgEnd - orgStart).TotalMinutes),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = operatedPersonId,
					TrackId = trackId
				}
			};

			target.Handle(command);
			var expectedStartInLocal = command.NewStartTime;
			var expectedStartInUtc = TimeZoneHelper.ConvertToUtc(expectedStartInLocal, userTimeZoneInfo);
			var @event = personAssignmentRepository.Single().PopAllEvents().OfType<ActivityMovedEvent>().Single();
			@event.PersonId.Should().Be(personRepository.Single().Id.Value);
			@event.StartDateTime.Should().Be(expectedStartInUtc);
			@event.ScenarioId.Should().Be(personAssignmentRepository.Single().Scenario.Id.Value);
			@event.InitiatorId.Should().Be(operatedPersonId);
			@event.CommandId.Should().Be(trackId);
			@event.LogOnBusinessUnitId.Should().Be(scenario.Current().BusinessUnit.Id.GetValueOrDefault());
		}

		private static IPersonAssignment createPersonAssignmentWithOneLayer(IActivity activity, IPerson agent, DateTime orgStart, DateTime orgEnd, IUserTimeZone userTimeZone)
		{
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(agent,
				activity, new DateTimePeriod(TimeZoneHelper.ConvertToUtc(orgStart, userTimeZone.TimeZone()),
					TimeZoneHelper.ConvertToUtc(orgEnd, userTimeZone.TimeZone())));
			return assignment;
		}

		private static DateTime createDateTimeLocal(int hourOnDay)
		{
			return new DateTime(2013, 11, 14, hourOnDay, 0, 0, 0, DateTimeKind.Local);
		}
	}
}