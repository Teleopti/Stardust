using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
    public class ScheduleProjectionReadOnlyChanged : EventWithInfrastructureContext
    {
	    public Guid PersonId { get; set; }

	    public DateTime ActivityStartDateTime { get; set; }

	    public DateTime ActivityEndDateTime { get; set; }
    }
}
