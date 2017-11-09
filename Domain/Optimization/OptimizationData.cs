using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class OptimizationData
	{
		public DateOnlyPeriod DateOnlyPeriod { get; set; }
		public IEnumerable<IPerson> Persons { get; set; }
		public IPlanningGroup PlanningGroup { get; set; }
		public bool UsePreferences { get; set; }
	}
}