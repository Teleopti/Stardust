﻿using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.PulseLoop
{
    public class PersonActivityChangePulseEvent: EventWithLogOnAndInitiator
    {
	    public Guid PersonId { get; set; }
		public bool PersonHaveExternalLogOn { get; set; }
    }
}
