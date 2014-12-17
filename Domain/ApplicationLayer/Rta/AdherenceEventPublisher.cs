using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class AdherenceEventPublisher : IAdherenceEventPublisher
	{
		private readonly IEventPopulatingPublisher _eventPublisher;
		private readonly IDictionary<Guid, Type> _sentEvents = new Dictionary<Guid, Type>();

		public AdherenceEventPublisher(IEventPopulatingPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}

		public void Publish(StateInfo info, DateTime time, bool inAdherence)
		{
			var agentState = info.NewState;
			
			IEvent @event;
			if (inAdherence)
				@event = new PersonInAdherenceEvent
				{
					PersonId = agentState.PersonId,
					Timestamp = time,
					BusinessUnitId = info.NewState.BusinessUnitId,
					TeamId = info.TeamId
				};
			else
			{
				@event = new PersonOutOfAdherenceEvent
				{
					PersonId = agentState.PersonId,
					Timestamp = time,
					BusinessUnitId = info.NewState.BusinessUnitId,
					TeamId = info.TeamId
				};
			}

			Type current;
			_sentEvents.TryGetValue(agentState.PersonId, out current);
			var adherenceHasChanged = current == null || current != @event.GetType();
			if (!adherenceHasChanged) return;

			_eventPublisher.Publish(@event);
			_sentEvents[agentState.PersonId] = @event.GetType();
		}

	}
}