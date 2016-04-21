using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class PreferenceChangedEvent : EventWithInfrastructureContext
	{
		public Guid PreferenceDayId { get; set; }
		public DateTime RestrictionDate { get; set; }
		public Guid PersonId { get; set; }
	}
}
