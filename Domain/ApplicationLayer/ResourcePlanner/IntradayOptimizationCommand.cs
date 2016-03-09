using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public class IntradayOptimizationCommand
	{
		public DateOnlyPeriod Period { get; set; }
		public IEnumerable<IPerson> Agents { get; set; }
		public bool RunResolveWeeklyRestRule { get; set; }
	}
}