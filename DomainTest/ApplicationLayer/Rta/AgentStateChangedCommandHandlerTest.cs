using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Hql.Ast.ANTLR.Tree;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	[TestFixture]
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
		public void ShouldNotPublishEventIfStillInAdherence()
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
		public void ShouldPublishEventsForEachPerson()
		{
			var publisher = new FakeEventsPublisher();
			var target = new AgentStateChangedCommandHandler(publisher);
			var state1 = new ActualAgentState
			{
				PersonId = Guid.NewGuid(),
				StaffingEffect = 0
			};
			var state2 = new ActualAgentState
			{
				PersonId = Guid.NewGuid(),
				StaffingEffect = 1
			};

			target.Invoke(state1);
			target.Invoke(state2);
			target.Invoke(state1);

			publisher.PublishedEvents.Should().Have.Count.EqualTo(2);
			var event1 = publisher.PublishedEvents.ElementAt(0) as PersonInAdherenceEvent;
			event1.PersonId.Should().Be(state1.PersonId);
			var event2 = publisher.PublishedEvents.ElementAt(1) as PersonOutOfAdherenceEvent;
			event2.PersonId.Should().Be(state2.PersonId);
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
		private readonly IEventPublisher _eventPublisher;
		private readonly IDictionary<Guid, Type> _sentEvents = new Dictionary<Guid, Type>();

		public AgentStateChangedCommandHandler(IEventPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}

		public void Invoke(IActualAgentState agentState)
		{
			IEvent @event;
			if (agentState.StaffingEffect.Equals(0))
				@event = new PersonInAdherenceEvent { PersonId = agentState.PersonId };
			else
				@event = new PersonOutOfAdherenceEvent { PersonId = agentState.PersonId };

			Type current;
			_sentEvents.TryGetValue(agentState.PersonId, out current);
			if (current == null || current != @event.GetType())
				_eventPublisher.Publish(@event);
			_sentEvents[agentState.PersonId] = @event.GetType();
		}

	}

}