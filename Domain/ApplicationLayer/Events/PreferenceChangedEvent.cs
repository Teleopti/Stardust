using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
    public abstract class PreferenceEventBase : EventWithInfrastructureContext
    {
        public Guid PreferenceDayId { get; set; }
        public DateTime RestrictionDate { get; set; }
        public Guid PersonId { get; set; }
    }

    public class PreferenceChangedEvent : PreferenceEventBase
    {
		public Guid ScenarioId { get; set; }
	}
    public class PreferenceCreatedEvent : PreferenceEventBase
    {
        public Guid ScenarioId { get; set; }
    }
    public class PreferenceDeletedEvent : PreferenceEventBase
    {
    }
}
