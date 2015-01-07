using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class StateEventPublisher : IStateEventPublisher
	{
		private readonly IEventPopulatingPublisher _eventPublisher;
		private readonly IAdherenceEventPublisher _adherenceEventPublisher;

		public StateEventPublisher(IEventPopulatingPublisher eventPublisher, IAdherenceEventPublisher adherenceEventPublisher)
		{
			_eventPublisher = eventPublisher;
			_adherenceEventPublisher = adherenceEventPublisher;
		}

		public void Publish(StateInfo info)
		{
			if (info.NewState.StateId == info.PreviousState.StateId) return;

			_eventPublisher.Publish(new PersonStateChangedEvent
			{
				PersonId = info.NewState.PersonId,
				Timestamp = info.NewState.ReceivedTime,
				BusinessUnitId = info.NewState.BusinessUnitId,
				InAdherence = info.Adherence == Adherence.In,
				InAdherenceWithPreviousActivity = info.AdherenceForNewStateAndPreviousActivity == Adherence.In
			});

			_adherenceEventPublisher.Publish(info, info.NewState.ReceivedTime, info.AdherenceForPreviousStateAndCurrentActivity, info.Adherence);
		}
	}
}