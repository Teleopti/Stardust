using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class StateEventPublisher : IStateEventPublisher
	{
		private readonly IEventPopulatingPublisher _eventPublisher;

		public StateEventPublisher(IEventPopulatingPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}

		public void Publish(StateInfo info)
		{
			if (info.NewState.StateId != info.PreviousState.StateId)
				_eventPublisher.Publish(new PersonStateChangedEvent
				{
					PersonId = info.NewState.PersonId,
					Timestamp = info.NewState.ReceivedTime,
					BusinessUnitId = info.NewState.BusinessUnitId
				});
		}
	}
}