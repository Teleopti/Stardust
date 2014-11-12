using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
    public class PersonActivityStarting: EventWithLogOnAndInitiator
    {
	    public Guid PersonId { get; set; }
		public bool PersonHaveExternalLogOn { get; set; }
    }
}
