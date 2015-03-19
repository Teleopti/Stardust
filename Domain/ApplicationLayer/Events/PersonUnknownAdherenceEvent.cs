using System;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class PersonUnknownAdherenceEvent : IEvent , ILogOnInfo
	{
		public Guid PersonId { get; set; }
		public DateOnly? BelongsToDate { get; set; }
		public DateTime Timestamp { get; set; }
		public Guid TeamId { get; set; }
		public Guid SiteId { get; set; }
		public string Datasource { get; set; }
		public Guid BusinessUnitId { get; set; }
	}
}