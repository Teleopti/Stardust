using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IRtaEventDecorator
	{
		void Decorate(Context info, IEvent @event);
	}
}