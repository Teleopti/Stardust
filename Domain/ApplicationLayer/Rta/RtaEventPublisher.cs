using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class RtaEventPublisher : IRtaEventPublisher
	{
		private readonly IEventPopulatingPublisher _eventPopulatingPublisher;
		private readonly IDictionary<Guid, Type> _sentEvents = new Dictionary<Guid, Type>();

		public RtaEventPublisher(IEventPopulatingPublisher eventPopulatingPublisher)
		{
			_eventPopulatingPublisher = eventPopulatingPublisher;
		}

		public void Publish(StateInfo info)
		{
			publishShiftStartEvent(info);
			publishShiftEndEvent(info);
			publishAdherenceEvent(info);
		}

		private void publishShiftStartEvent(StateInfo info)
		{
			if (info.IsScheduled && !info.WasScheduled)
			{
				_eventPopulatingPublisher.Publish(new PersonShiftStartEvent
				{
					PersonId = info.NewState.PersonId,
					ShiftStartTime = info.CurrentShiftStartTime,
					ShiftEndTime = info.CurrentShiftEndTime,
					BusinessUnitId = info.NewState.BusinessUnitId
				});
			}
		}

		private void publishShiftEndEvent(StateInfo info)
		{
			if (!info.IsScheduled && info.WasScheduled)
			{
				_eventPopulatingPublisher.Publish(new PersonShiftEndEvent
				{
					PersonId = info.NewState.PersonId,
					ShiftStartTime = info.PreviousShiftStartTime,
					ShiftEndTime = info.PreviousShiftEndTime,
					BusinessUnitId = info.NewState.BusinessUnitId
				});
			}

		}

		private void publishAdherenceEvent(StateInfo info)
		{
			var agentState = info.NewState;

			IEvent @event;
			if (agentState.InAdherence)
				@event = new PersonInAdherenceEvent
				{
					PersonId = agentState.PersonId,
					Timestamp = agentState.ReceivedTime,
					BusinessUnitId = agentState.BusinessUnitId
				};
			else
				@event = new PersonOutOfAdherenceEvent
				{
					PersonId = agentState.PersonId,
					Timestamp = agentState.ReceivedTime,
					BusinessUnitId = agentState.BusinessUnitId
				};

			Type current;
			_sentEvents.TryGetValue(agentState.PersonId, out current);
			var adherenceHasChanged = current == null || current != @event.GetType();
			if (!adherenceHasChanged) return;

			_eventPopulatingPublisher.Publish(@event);
			_sentEvents[agentState.PersonId] = @event.GetType();
		}

	}
}