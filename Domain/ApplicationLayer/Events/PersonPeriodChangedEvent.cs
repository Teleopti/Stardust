using System;
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
	}
}