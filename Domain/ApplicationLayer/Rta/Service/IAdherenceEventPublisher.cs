using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IAdherenceEventPublisher
	{
		void Publish(StateInfo info, DateTime time, EventAdherence adherence);
	}
}