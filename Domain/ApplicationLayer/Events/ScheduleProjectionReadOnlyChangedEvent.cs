using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class ScheduleProjectionReadOnlyChangedEvent : Event
	{
		public Guid PersonId { get; set; }
	}
}