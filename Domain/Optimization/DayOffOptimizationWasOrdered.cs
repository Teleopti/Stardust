using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffOptimizationWasOrdered : IEvent, ICommandIdentifier, IIslandInfo
	{
		public DateOnly StartDate { get; set; }
		public DateOnly EndDate { get; set; }
		public IEnumerable<Guid> Agents { get; set; }
		public Guid CommandId { get; set; }
		public IEnumerable<Guid> Skills { get; set; }
		public IEnumerable<Guid> AgentsInIsland { get; set; }
		public IEnumerable<LockInfo> UserLocks { get; set; }
	}
}