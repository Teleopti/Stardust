using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
    public class PersonActivityStarting: RaptorDomainEvent
    {
	    public Guid PersonId { get; set; }
		public bool PersonHaveExternalLogOn { get; set; }
    }
}
