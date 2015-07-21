using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public interface IEventPopulatingPublisher
	{
		void Publish(params IEvent[] events);
	}
}