using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	[TestFixture, Ignore]
	public class AgentStateChangedCommandHandlerTest
	{
		[Test]
		public void ShouldPublishPersonInAdherenceEventWhenNoStaffingEffect()
		{
			var publisher = new FakeEventsPublisher();
			var target = new AgentStateChangedCommandHandler(publisher);
			var state = new ActualAgentState
			{
				PersonId = Guid.NewGuid(),
				StaffingEffect = 0
			};

			target.Invoke(state);

			var @event = publisher.PublishedEvents.Single() as PersonInAdherenceEvent;
			@event.PersonId.Should().Be(state.PersonId);
		}

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
		public void ShouldPublishOnePersonInAdherenceEventIfNoChange()
		{
			var publisher = new FakeEventsPublisher();
			var target = new AgentStateChangedCommandHandler(publisher);
			var state = new ActualAgentState
			{
				PersonId = Guid.NewGuid(),
				StaffingEffect = 0
			};

			target.Invoke(state);
			target.Invoke(state);

			publisher.PublishedEvents.Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldPublishOnePersonOutOfAdherenceEventIfNoChange()
		{
			var publisher = new FakeEventsPublisher();
			var target = new AgentStateChangedCommandHandler(publisher);
			var state1 = new ActualAgentState
			{
				PersonId = Guid.NewGuid(),
				StaffingEffect = -1
			};
			var state2 = new ActualAgentState
			{
				PersonId = Guid.NewGuid(),
				StaffingEffect = 1
			};

			target.Invoke(state1);
			target.Invoke(state2);

			publisher.PublishedEvents.Should().Have.Count.EqualTo(1);
		}


	}

	public class PersonOutOfAdherenceEvent : IEvent
	{
		public Guid PersonId { get; set; }
	}

	public class PersonInAdherenceEvent : IEvent
	{
		public Guid PersonId { get; set; }
	}

	public class AgentStateChangedCommandHandler //: IActualAgentStateHasBeenSent
	{
		public AgentStateChangedCommandHandler(IEventsPublisher eventsPublisher)
		{
		}

		public void Invoke(IActualAgentState agentState)
		{
		}
	}
}