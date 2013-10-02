using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public interface IPublishEventsFromEventHandlers
	{
		void Publish(IEvent @event);
	}
}