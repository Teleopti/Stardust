using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Hql.Ast.ANTLR.Tree;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
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
		public void InvokeInAdherenceState_WhenlastStateForThatPersonWasInAdherence_ShouldNotPublishNewEvent()
		{
			var publisher = new FakeEventsPublisher();
			var target = new AgentStateChangedCommandHandler(publisher);
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();

			var state1 = new ActualAgentState
			{
				PersonId = person1,
				StaffingEffect = 0
			};
			var state2 = new ActualAgentState
			{
				PersonId = person2,
				StaffingEffect = 1
			};
			var state3 = new ActualAgentState
			{
				PersonId = person1,
				StaffingEffect = 0
			};

			target.Invoke(state1);
			target.Invoke(state2);
			target.Invoke(state3);

			publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Single().PersonId.Should().Be.EqualTo(person1);
		}

		[Test]
		public void InvokeOutOfAdherenceState_WhenlastStateForThatPersonWasOutOfAdherence_ShouldNotPublishNewEvent()
		{
			var publisher = new FakeEventsPublisher();
			var target = new AgentStateChangedCommandHandler(publisher);
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();

			var state1 = new ActualAgentState
			{
				PersonId = person1,
				StaffingEffect = 1
			};
			var state2 = new ActualAgentState
			{
				PersonId = person2,
				StaffingEffect = 0
			};
			var state3 = new ActualAgentState
			{
				PersonId = person1,
				StaffingEffect = -1
			};

			target.Invoke(state1);
			target.Invoke(state2);
			target.Invoke(state3);

			publisher.PublishedEvents.OfType<PersonOutOfAdherenceEvent>().Single().PersonId.Should().Be.EqualTo(person1);
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
		private readonly IDictionary<Guid, double> _sentEvents = new Dictionary<Guid, double>();

		public AgentStateChangedCommandHandler(IEventPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}

		private bool stateIsChanged(Guid personId, double newState)
		{
			if (!_sentEvents.ContainsKey(personId)) return true;
			var oldState = _sentEvents[personId];
			if (oldState == 0)
			{
				return newState != oldState;
			}
			return newState == 0;
		}

		public void Invoke(IActualAgentState agentState)
		{
			var personId = agentState.PersonId;
			var staffingEffect = agentState.StaffingEffect;

			if (!stateIsChanged(personId, staffingEffect)) return;

			if (staffingEffect == 0)
			{
				_eventPublisher.Publish(new PersonInAdherenceEvent() { PersonId = personId });
			}
			else
			{
				_eventPublisher.Publish(new PersonOutOfAdherenceEvent(){PersonId = personId});
			}
			_sentEvents[agentState.PersonId] = agentState.StaffingEffect;
		}


	}
}