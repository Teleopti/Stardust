using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class AdherenceStateDecorator : IRtaEventDecorator
	{
		public void Decorate(StateInfo info, IEvent @event)
		{
			if (@event is PersonActivityStartEvent)
				((PersonActivityStartEvent)@event).Adherence = info.Adherence.AdherenceForPreviousStateAndCurrentActivity();
		}
	}
}