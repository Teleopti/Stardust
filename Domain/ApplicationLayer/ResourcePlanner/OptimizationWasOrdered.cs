using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public class OptimizationWasOrdered : IEvent, ICommandIdentifier
	{
		public DateOnlyPeriod Period { get; set; }
		public IEnumerable<Guid> AgentsInIsland { get; set; }
		public bool RunResolveWeeklyRestRule { get; set; }
		public IEnumerable<Guid> AgentsToOptimize { get; set; }
		public Guid CommandId { get; set; }
	}
}