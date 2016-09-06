using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class ScheduleProjectionReadModelChangedEvent : Event
	{
		public Guid PersonId { get; set; }
	}
}