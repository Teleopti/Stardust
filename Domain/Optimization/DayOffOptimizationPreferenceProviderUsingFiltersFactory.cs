namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffOptimizationPreferenceProviderUsingFiltersFactory
	{
		public IDayOffOptimizationPreferenceProvider Create(PlanningGroup planningGroup)
		{
			return new DayOffOptimizationPreferenceProviderUsingFilters(planningGroup.Settings);
		}
	}
}
