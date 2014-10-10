using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	[TestFixture]
	public class AgentStateChangedOutOfAdherenceCommandHandlerTest
	{
		[Test]
		public void ShouldPublishPersonOutOfAdherenceEventOnPositiveStaffingEffect()
		{
			var publisher = new FakeEventsPublisher();
			var target = new AgentStateChangedCommandHandler(publisher);
			var state = new ActualAgentState
			{
				PersonId = Guid.NewGuid(),
				StaffingEffect = 1
			};

			target.Invoke(state);

			var @event = publisher.PublishedEvents.Single() as PersonOutOfAdherenceEvent;
			@event.PersonId.Should().Be(state.PersonId);
		}

		[Test]
		public void ShouldPublishPersonOutOfAdherenceEventOnNegativeStaffingEffect()
		{
			var publisher = new FakeEventsPublisher();
			var target = new AgentStateChangedCommandHandler(publisher);
			var state = new ActualAgentState
			{
				PersonId = Guid.NewGuid(),
				StaffingEffect = -1
			};

			target.Invoke(state);

			var @event = publisher.PublishedEvents.Single() as PersonOutOfAdherenceEvent;
			@event.PersonId.Should().Be(state.PersonId);
		}


		[Test]
		public void ShouldNotPublishEventIfStillOutOfAdherence()
		{
			var publisher = new FakeEventsPublisher();
			var target = new AgentStateChangedCommandHandler(publisher);
			var state1 = new ActualAgentState
			{
				PersonId = Guid.NewGuid(),
				StaffingEffect = 1
			};
			var state2 = new ActualAgentState
			{
				PersonId = state1.PersonId,
				StaffingEffect = -1
			};

			target.Invoke(state1);
			target.Invoke(state2);

			publisher.PublishedEvents.Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldPublishTheTimeWhenPersonOutOfAdherence()
		{
			var publisher = new FakeEventsPublisher();
			var target = new AgentStateChangedCommandHandler(publisher);

			var state = new ActualAgentState
			{
				PersonId = Guid.NewGuid(),
				StaffingEffect = 1,
				Timestamp = DateTime.Now
			};

			target.Invoke(state);

			var @event = publisher.PublishedEvents.Single() as PersonOutOfAdherenceEvent;
			@event.Timestamp.Should().Be(state.Timestamp);
		}
	}
}