using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.Domain.ResourcePlanner
{
	public class DayOffOptimizationPreferenceProviderForPlanningPeriod : IDayOffOptimizationPreferenceProviderForPlanningPeriod
	{
		public IDayOffOptimizationPreferenceProvider Fetch(PlanningGroup planningGroup)
		{
			return new DayOffOptimizationPreferenceProviderUsingFilters(planningGroup.Settings);
		}
	}
}