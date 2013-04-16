using System;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;

namespace Teleopti.Interfaces.Messages.Rta
{
    /// <summary>
    /// 
    /// </summary>
    public class PersonWithExternalLogOn: RaptorDomainEvent
    {
	    /// <summary>
	    /// 
	    /// </summary>
	    public Guid PersonId { get; set; }
    }
}
