using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	[IsNotDeadCode("Event handler resolved dynamically by implementations of IEventPublisher")]
	public interface IHandleEvent<TEvent> where TEvent : IEvent
	{
		void Handle(TEvent @event);
	}

	public interface IHandleEventOnQueue<TEvent> where TEvent : IEvent
	{
		string QueueTo(TEvent @event);
	}
}