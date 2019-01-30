using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class UpdateSkillForecastReadModelEvent : EventWithInfrastructureContext
	{
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
	}
}