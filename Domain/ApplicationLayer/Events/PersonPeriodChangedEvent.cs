using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class PersonPeriodChangedEvent : IEvent, ITimestamped
	{
		public DateTime Timestamp { get; set; }
		public Guid PersonId { get; set; }
		public Guid? CurrentBusinessUnitId { get; set; }
		public Guid? CurrentSiteId { get; set; }
		public Guid? CurrentTeamId { get; set; }

		[RemoveMeWithToggle(Toggles.RTA_RemoveSiteTeamOutOfAdherenceReadModels_40069)]
		public IEnumerable<Association> PreviousAssociation { get; set; }
	}
}