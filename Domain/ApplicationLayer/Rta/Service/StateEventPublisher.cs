using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class StateEventPublisher
	{
		private readonly IEventPopulatingPublisher _eventPublisher;
		
		public StateEventPublisher(IEventPopulatingPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}

		public void Publish(Context info)
		{
			if (!info.State.StateChanged()) return;

			_eventPublisher.Publish(new PersonStateChangedEvent
			{
				BelongsToDate = info.Schedule.BelongsToDate,
				PersonId = info.PersonId,
				Timestamp = info.CurrentTime,
				AdherenceWithPreviousActivity = info.Adherence.AdherenceForNewStateAndPreviousActivity(),
				Adherence = info.Adherence.AdherenceForNewStateAndCurrentActivity()
			});
		}
	}
}