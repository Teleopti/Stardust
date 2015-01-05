using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface IAdherenceEventPublisher
	{
		void Publish(StateInfo info, DateTime time, Adherence adherence, Adherence previousAdherence);
	}
}