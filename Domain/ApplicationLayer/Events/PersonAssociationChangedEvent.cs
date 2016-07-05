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
		public Guid? PreviousTeam { get; set; }
		public Guid? PreviousSite { get; set; }
		public IEnumerable<Guid> PreviousTeams { get; set; }
		public IEnumerable<Guid> PreviousSites { get; set; }
	}
}