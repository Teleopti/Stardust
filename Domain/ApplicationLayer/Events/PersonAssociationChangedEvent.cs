using System;
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
	}
}