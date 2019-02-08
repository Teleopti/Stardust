using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
    public abstract class PreferenceEventBase : EventWithInfrastructureContext
    {
        public IEnumerable<DateTime> RestrictionDates { get; set; }
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
