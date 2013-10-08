using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	public class ProjectionChangedEventDayOff
	{
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
	}
}