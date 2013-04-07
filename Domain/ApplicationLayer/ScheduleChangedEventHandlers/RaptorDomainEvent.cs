using System;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	[Serializable]
	public abstract class RaptorDomainEvent : Event, IRaptorDomainMessageInfo
	{
		public DateTime Timestamp { get; set; }// remove?
		public string Datasource { get; set; }
		public Guid BusinessUnitId { get; set; }
	}
}