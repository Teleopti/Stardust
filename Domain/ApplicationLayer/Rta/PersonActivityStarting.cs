using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
    /// <summary>
    /// 
    /// </summary>
    public class PersonActivityStarting: RaptorDomainEvent
    {
	    /// <summary>
	    /// 
	    /// </summary>
	    public Guid PersonId { get; set; }
    }
}
