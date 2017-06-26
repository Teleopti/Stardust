using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public class SchedulingWasOrdered : ICommandIdentifier, IEvent
	{
		public DateOnly StartDate { get; set; }
		public DateOnly EndDate { get; set; }
		public IEnumerable<Guid> AgentsToSchedule { get; set; }
		public bool RunWeeklyRestSolver { get; set; }
		public Guid CommandId {get;set;}
	}
}