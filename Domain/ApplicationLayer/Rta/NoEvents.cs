using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class NoEvents : IShiftEventPublisher, IAdherenceEventPublisher, IStateEventPublisher, IActivityEventPublisher
	{
		public void Publish(StateInfo info)
		{
		}

		public void Publish(StateInfo info, DateTime time, Adherence fromAdherence, Adherence toAdherence)
		{
		}
	}
}