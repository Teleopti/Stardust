using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class PersonAssociationChangedEvent : IEvent
	{
		public Guid PersonId { get; set; }
		public DateTime Timestamp { get; set; }
		public Guid? BusinessUnitId { get; set; }
		public Guid? SiteId { get; set; }
		public Guid? TeamId { get; set; }

		[RemoveMeWithToggle(Toggles.RTA_RemoveSiteTeamOutOfAdherenceReadModels_40069)]
		public int? Version { get; set; }
		[RemoveMeWithToggle(Toggles.RTA_RemoveSiteTeamOutOfAdherenceReadModels_40069)]
		public IEnumerable<Association> PreviousAssociation { get; set; }

		public IEnumerable<ExternalLogon> ExternalLogons { get; set; }
	}

	public class ExternalLogon
	{
		public string UserCode { get; set; }
		public int DataSourceId { get; set; }
	}

	[RemoveMeWithToggle(Toggles.RTA_RemoveSiteTeamOutOfAdherenceReadModels_40069)]
	public class Association
	{
		public Guid BusinessUnitId { get; set; }
		public Guid SiteId { get; set; }
		public Guid TeamId { get; set; }

		public override string ToString()
		{
			return "BU: " + BusinessUnitId + Environment.NewLine +" Site: " + SiteId + Environment.NewLine + " Team:" + TeamId + Environment.NewLine;
		}
	}
}