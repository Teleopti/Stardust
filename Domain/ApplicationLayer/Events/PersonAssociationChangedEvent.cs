using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class PersonAssociationChangedEvent : IEvent
	{
		public Guid PersonId { get; set; }
		public Guid? BusinessUnitId { get; set; }
		public Guid? SiteId { get; set; }
		public string SiteName { get; set; }
		public Guid? TeamId { get; set; }
		public string TeamName { get; set; }
		public DateTime Timestamp { get; set; }

		public IEnumerable<ExternalLogon> ExternalLogons { get; set; }
	}

	public class ExternalLogon
	{
		public string UserCode { get; set; }
		public int DataSourceId { get; set; }
	}
}