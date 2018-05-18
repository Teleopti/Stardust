using System;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class ScheduleChangedEvent : ScheduleChangedEventBase
	{
		public DateTime? Date { get; set; }
	}
}