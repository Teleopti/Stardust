using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface IRtaEventDecorator
	{
		void Decorate(StateInfo info, IEvent @event);
	}
}