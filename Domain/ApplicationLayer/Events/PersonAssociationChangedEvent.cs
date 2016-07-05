using System;
using System.Collections.Generic;
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

		public int? Version { get; set; }

		public IEnumerable<Association> PreviousAssociation { get; set; }
	}

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