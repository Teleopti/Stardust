using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class PreferenceChangedEvent : EventWithInfrastructureContext
	{
		public Guid PreferenceId { get; set; }
	}
}
