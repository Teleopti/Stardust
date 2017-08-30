using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public class SchedulingCommand : ICommandIdentifier
	{
		public SchedulingCommand()
		{
			CommandId = Guid.NewGuid();
		}
		public Guid CommandId { get; set; }
		public IEnumerable<Guid> AgentsToSchedule { get; set; }
		public DateOnlyPeriod Period { get; set; }
		public bool RunWeeklyRestSolver { get; set; }
		public bool FromWeb { get; set; }
	}
}