using System;
using Teleopti.Ccc.Domain.ApplicationLayer;

namespace Teleopti.Ccc.Domain.ApplicationRtaQueue
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
