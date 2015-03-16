using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IAdherenceEventPublisher
	{
		void Publish(StateInfo info, DateTime time, AdherenceState adherence);
	}
}