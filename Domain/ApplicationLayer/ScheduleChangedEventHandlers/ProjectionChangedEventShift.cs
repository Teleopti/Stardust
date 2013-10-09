using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	public class ProjectionChangedEventShift
	{
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		public IEnumerable<ProjectionChangedEventLayer> Layers { get; set; }
	}
}