using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Events
{
	public class PersonOutOfAdherenceEvent : IEvent
	{
		public Guid PersonId { get; set; }
		public DateOnly? BelongsToDate { get; set; }
		public DateTime Timestamp { get; set; }
		public Guid BusinessUnitId { get; set; }
		public Guid TeamId { get; set; }
		public Guid SiteId { get; set; }
	}
}