using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public interface IEventPopulatingPublisher
	{
		void Publish(params IEvent[] events);
	}
}