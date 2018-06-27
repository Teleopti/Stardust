using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class ActivityTypeChangedEvent : EventWithInfrastructureContext, ICommandIdentifier
	{
		public IPerson Person { get; set; }
		public DateOnly Date { get; set; }
		public IActivity Activity { get; set; }
		public ShiftLayer ShiftLayer { get; set; }
		public bool IsNew { get; set; }
		public IList<string> ErrorMessages { get; set; }

		public Guid CommandId { get; set; }
		public object StartDateTime { get; set; }
		public object EndDateTime { get; set; }
	}
}
