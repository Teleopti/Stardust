using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator
{
	public class ValidateScheduleProjectionReadOnlyEvent : EventWithInfrastructureContext
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
	}
}
