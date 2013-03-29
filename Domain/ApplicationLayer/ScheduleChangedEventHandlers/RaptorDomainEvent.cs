using System;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	[Serializable]
	public abstract class RaptorDomainEvent : Event, IRaptorDomainMessageInfo
	{
		public abstract Guid Identity { get; }
		public string Datasource { get; set; }
		public DateTime Timestamp { get; set; }
		public Guid BusinessUnitId { get; set; }
	}
}