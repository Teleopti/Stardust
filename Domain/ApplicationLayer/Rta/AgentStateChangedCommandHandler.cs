using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
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
				@event = new PersonInAdherenceEvent
				{
					PersonId = agentState.PersonId,
					Timestamp = agentState.Timestamp
				};
			else
				@event = new PersonOutOfAdherenceEvent
				{
					PersonId = agentState.PersonId,
					Timestamp = agentState.Timestamp
				};

			Type current;
			_sentEvents.TryGetValue(agentState.PersonId, out current);
			if (current == null || current != @event.GetType())
				_eventPublisher.Publish(@event);
			_sentEvents[agentState.PersonId] = @event.GetType();
		}

	}
}