using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class PersonTerminalDateChangedEvent : EventWithInfrastructureContext
	{
		public Guid PersonId { get; set; }
		public Guid? BusinessUnitId { get; set; }
		public Guid? SiteId { get; set; }
		public Guid? TeamId { get; set; }
		public string TimeZoneInfoId { get; set; }
		public DateTime? PreviousTerminationDate { get; set; }
		public DateTime? TerminationDate { get; set; }

		[RemoveMeWithToggle(Toggles.RTA_RemoveSiteTeamOutOfAdherenceReadModels_40069)]
		public IEnumerable<Association> PreviousAssociations { get; set; }

		public IEnumerable<ExternalLogon> ExternalLogons { get; set; }

	}
}