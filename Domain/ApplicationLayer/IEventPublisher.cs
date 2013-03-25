using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public interface IEventPublisher
	{
		void Publish(IEvent @event);
	}
}