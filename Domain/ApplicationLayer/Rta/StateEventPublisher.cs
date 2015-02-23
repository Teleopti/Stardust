using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
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

			var adherenceChanged = info.AdherenceForPreviousStateAndCurrentActivity != info.Adherence;

			_eventPublisher.Publish(info, new PersonStateChangedEvent
			{
				PersonId = info.PersonId,
				Timestamp = info.CurrentTime,
				BusinessUnitId = info.BusinessUnitId,
				InAdherence = info.Adherence == Adherence.In,
				InAdherenceWithPreviousActivity = info.AdherenceForNewStateAndPreviousActivity == Adherence.In,
			});

			if (adherenceChanged)
				_adherenceEventPublisher.Publish(info, info.CurrentTime, info.Adherence);
		}
	}
}