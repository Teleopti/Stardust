using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public class IntradayOptimizationWasOrdered : IEvent, ICommandIdentifier, IIslandInfo
	{
		public DateOnly StartDate { get; set; }
		public DateOnly EndDate { get; set; }
		public IEnumerable<Guid> AgentsInIsland { get; set; }
		public bool RunResolveWeeklyRestRule { get; set; }
		public IEnumerable<Guid> Agents { get; set; }
		public Guid CommandId { get; set; }
		public IEnumerable<LockInfo> UserLocks { get; set; }
		public IEnumerable<Guid> Skills { get; set; }
		public Guid PlanningPeriodId { get; set; } //null if from desktop. if that is "fixed", we can remove the two dates as well
	}
}