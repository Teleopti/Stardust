using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class RtaEventPublisher : IRtaEventPublisher
	{
		private readonly IEventPublisher _eventPublisher;
		private readonly IDictionary<Guid, Type> _sentEvents = new Dictionary<Guid, Type>();

		public RtaEventPublisher(IEventPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}

		public void Publish(StateInfo info)
		{
			publishShiftStartEvent(info);
			publishAdherenceEvent(info);
		}

		private void publishAdherenceEvent(StateInfo info)
		{
			var agentState = info.NewState;

			IEvent @event;
			if (agentState.InAdherence)
				@event = new PersonInAdherenceEvent
				{
					PersonId = agentState.PersonId,
					Timestamp = agentState.ReceivedTime
				};
			else
				@event = new PersonOutOfAdherenceEvent
				{
					PersonId = agentState.PersonId,
					Timestamp = agentState.ReceivedTime
				};

			Type current;
			_sentEvents.TryGetValue(agentState.PersonId, out current);
			var adherenceHasChanged = current == null || current != @event.GetType();
			if (!adherenceHasChanged) return;

			_eventPublisher.Publish(@event);
			_sentEvents[agentState.PersonId] = @event.GetType();
		}

		private void publishShiftStartEvent(StateInfo info)
		{
			if (info.CurrentActivity == null) return;

			if (info.CurrentShiftStartTime != info.PreviousStateShiftStartTime)
			{
				_eventPublisher.Publish(new PersonShiftStartEvent
				{
					PersonId = info.NewState.PersonId,
					ShiftStartTime = info.CurrentShiftStartTime,
					ShiftEndTime = info.CurrentShiftEndTime
				});
			}
		}
	}
}