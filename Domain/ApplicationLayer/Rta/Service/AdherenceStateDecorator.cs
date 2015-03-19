using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class AdherenceStateDecorator : IRtaEventDecorator
	{
		private AdherenceMapper _mapper;

		public AdherenceStateDecorator()
		{
			_mapper = new AdherenceMapper();
		}

		public void Decorate(StateInfo info, IEvent @event)
		{
			if (@event is PersonStateChangedEvent)
				((PersonStateChangedEvent) @event).Adherence =
					_mapper.GetNeutralIfUnknownAdherence(info.Adherence);
			if (@event is PersonActivityStartEvent)
				((PersonActivityStartEvent) @event).Adherence =
					_mapper.GetNeutralIfUnknownAdherence(info.AdherenceForPreviousStateAndCurrentActivity);
		}	
	}
}