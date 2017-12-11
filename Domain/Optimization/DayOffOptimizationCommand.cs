using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffOptimizationCommand
	{
		public DateOnlyPeriod Period { get; set; }
		public IEnumerable<IPerson> AgentsToOptimize { get; set; }
		public bool RunWeeklyRestSolver { get; set; }
	}
}