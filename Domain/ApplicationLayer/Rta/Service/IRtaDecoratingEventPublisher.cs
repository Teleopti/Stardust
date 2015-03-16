using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IRtaDecoratingEventPublisher
	{
		void Publish(StateInfo info, IEvent @event);
	}
}