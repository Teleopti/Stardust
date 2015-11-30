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
			if (!info.State.StateGroupChanged()) return;

			_eventPublisher.Publish(info, new PersonStateChangedEvent
			{
				PersonId = info.Person.PersonId,
				Timestamp = info.CurrentTime,
				AdherenceWithPreviousActivity = info.Adherence.AdherenceForNewStateAndPreviousActivity(),
				Adherence = info.Adherence.AdherenceForNewStateAndCurrentActivity()
			});

			if (info.Adherence.AdherenceChangedFromStateChange())
				_adherenceEventPublisher.Publish(info, info.CurrentTime, info.Adherence.AdherenceForNewStateAndCurrentActivity());
		}
	}
}