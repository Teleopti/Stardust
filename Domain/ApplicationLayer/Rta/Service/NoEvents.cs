using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class NoEvents : IShiftEventPublisher, IAdherenceEventPublisher, IStateEventPublisher, IActivityEventPublisher
	{
		public void Publish(StateInfo info)
		{
		}

		public void Publish(StateInfo info, DateTime time, AdherenceState adherence)
		{
		}
	}
}