using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.Domain.ResourcePlanner
{
	public interface IDayOffOptimizationPreferenceProviderForPlanningPeriod
	{
		IDayOffOptimizationPreferenceProvider Fetch(PlanningGroup planningGroup);
	}
}