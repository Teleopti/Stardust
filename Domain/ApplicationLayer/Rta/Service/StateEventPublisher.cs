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
			
			var adherenceChanged = info.Adherence.AdherenceForPreviousStateAndCurrentActivity() != info.Adherence.CurrentAdherence();

			_eventPublisher.Publish(info, new PersonStateChangedEvent
			{
				PersonId = info.PersonId,
				Timestamp = info.CurrentTime,
				BusinessUnitId = info.BusinessUnitId,
				InOrNeutralAdherenceWithPreviousActivity =
					info.Adherence.AdherenceForNewStateAndPreviousActivity() == AdherenceState.In ||
					info.Adherence.AdherenceForNewStateAndPreviousActivity() == AdherenceState.Neutral,
				Adherence = info.Adherence.CurrentAdherenceForEvent()
			});

			if (adherenceChanged)
				_adherenceEventPublisher.Publish(info, info.CurrentTime, info.AdherenceState);
		}
	}
}