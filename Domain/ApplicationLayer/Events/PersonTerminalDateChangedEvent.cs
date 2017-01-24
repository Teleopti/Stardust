using System;
using System.Collections.Generic;

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
		public string SiteName { get; set; }
		public string TeamName { get; set; }

		public IEnumerable<ExternalLogon> ExternalLogons { get; set; }

	}
}