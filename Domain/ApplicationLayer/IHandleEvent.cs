using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public interface IHandleEvent<TEvent> where TEvent : IEvent
	{
		void Handle(TEvent @event);
	}

	public interface IHandleEventOnQueue<TEvent> where TEvent : IEvent
	{
		string QueueTo(TEvent @event);
	}
}