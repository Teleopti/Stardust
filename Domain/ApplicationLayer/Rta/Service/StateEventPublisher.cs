using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class StateEventPublisher : IStateEventPublisher
	{
		private readonly IRtaDecoratingEventPublisher _eventPublisher;
		private readonly IAdherenceEventPublisher _adherenceEventPublisher;
		
		public StateEventPublisher(IRtaDecoratingEventPublisher eventPublisher, IAdherenceEventPublisher adherenceEventPublisher)
		{
			_eventPublisher = eventPublisher;
			_adherenceEventPublisher = adherenceEventPublisher;
		}

		public void Publish(StateInfo info)
		{
			if (info.CurrentStateId == info.PreviousStateId) return;
			
			var adherenceChanged = info.Adherence.AdherenceForPreviousStateAndCurrentActivity() != info.Adherence.AdherenceState();

			_eventPublisher.Publish(info, new PersonStateChangedEvent
			{
				PersonId = info.PersonId,
				Timestamp = info.CurrentTime,
				BusinessUnitId = info.BusinessUnitId,
				AdherenceWithPreviousActivity = info.Adherence.EventAdherenceForNewStateAndPreviousActivity(),
				Adherence = info.Adherence.EventAdherence()
			});

			if (adherenceChanged)
				_adherenceEventPublisher.Publish(info, info.CurrentTime, info.AdherenceState);
		}
	}
}