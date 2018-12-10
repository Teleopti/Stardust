using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public class SchedulingCommand : ICommandIdentifier
	{
		public SchedulingCommand()
		{
			CommandId = Guid.NewGuid();
		}
		public Guid CommandId { get; set; }
		public IEnumerable<IPerson> AgentsToSchedule { get; set; }
		public DateOnlyPeriod Period { get; set; }
		public bool FromWeb { get; set; }
		public Guid PlanningPeriodId { get; set; }
		public bool RunDayOffOptimization { get; set; }
		public bool ScheduleWithoutPreferencesForFailedAgents { get; set; }
	}
}