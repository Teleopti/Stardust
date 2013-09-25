using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	[IsNotDeadCode("Event handler resolved dynamically by implementations of IEventPublisher")]
	public interface IHandleEvent<TEvent> where TEvent : IEvent
	{
		void Handle(TEvent @event);
	}
}