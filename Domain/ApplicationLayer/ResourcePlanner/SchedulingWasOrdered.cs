using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public class SchedulingWasOrdered : ICommandIdentifier, IEvent, IIslandInfo
	{
		public DateOnly StartDate { get; set; }
		public DateOnly EndDate { get; set; }
		public IEnumerable<Guid> Agents { get; set; }
		public IEnumerable<Guid> AgentsInIsland { get; set; }
		public Guid CommandId {get;set;}
		public IEnumerable<LockInfo> UserLocks { get; set; }
		public IEnumerable<Guid> Skills { get; set; }
		public bool FromWeb { get; set; }
		public Guid PlanningPeriodId { get; set; }
		public bool RunDayOffOptimization { get; set; }
		public bool ScheduleWithoutPreferencesForFailedAgents { get; set; }
	}
}