using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public class OptimizationWasOrdered : IEvent
	{
		public DateOnlyPeriod Period { get; set; }
		public IEnumerable<Guid> AgentsInIsland { get; set; }
		public bool RunResolveWeeklyRestRule { get; set; }
		public IEnumerable<Guid> AgentsToOptimize { get; set; }
	}
}