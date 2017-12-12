using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffOptimizationWasOrdered : IEvent, ICommandIdentifier //,IIslandInfo
	{
		public DateOnly StartDate { get; set; }
		public DateOnly EndDate { get; set; }
		public IEnumerable<IPerson> Agents { get; set; }
		public bool RunWeeklyRestSolver { get; set; }
		public Guid PlanningPeriodId { get; set; }
		public Guid CommandId { get; set; }
	}
}