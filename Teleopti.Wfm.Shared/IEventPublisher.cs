using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public interface IEventPublisher
	{
		void Publish(params IEvent[] events);
	}
}