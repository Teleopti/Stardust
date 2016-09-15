using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class StateEventPublisher
	{
		private readonly IEventPopulatingPublisher _eventPublisher;
		private readonly AdherenceEventPublisher _adherenceEventPublisher;
		
		public StateEventPublisher(IEventPopulatingPublisher eventPublisher, AdherenceEventPublisher adherenceEventPublisher)
		{
			_eventPublisher = eventPublisher;
			_adherenceEventPublisher = adherenceEventPublisher;
		}

		public void Publish(Context info)
		{
			if (!info.State.StateGroupChanged()) return;

			_eventPublisher.Publish(new PersonStateChangedEvent
			{
				BelongsToDate = info.Schedule.BelongsToDate,
				PersonId = info.PersonId,
				Timestamp = info.CurrentTime,
				AdherenceWithPreviousActivity = info.Adherence.AdherenceForNewStateAndPreviousActivity(),
				Adherence = info.Adherence.AdherenceForNewStateAndCurrentActivity()
			});

			if (info.Adherence.AdherenceChangedFromStateChange())
				_adherenceEventPublisher.Publish(info, info.CurrentTime, info.Adherence.AdherenceForNewStateAndCurrentActivity());
		}
	}
}