using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class UpdateStaffingLevelReadModelEvent : EventWithInfrastructureContext
	{
		public DateTime EndDateTime { get; set; }
		public DateTime StartDateTime { get; set; }
	}
}