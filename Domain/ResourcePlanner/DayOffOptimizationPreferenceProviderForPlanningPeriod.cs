using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.Domain.ResourcePlanner
{
	public class DayOffOptimizationPreferenceProviderForPlanningPeriod : IDayOffOptimizationPreferenceProviderForPlanningPeriod
	{
		public IDayOffOptimizationPreferenceProvider Fetch(AllSettingsForPlanningGroup allSettingsForPlanningGroup)
		{
			return new DayOffOptimizationPreferenceProviderUsingFilters(allSettingsForPlanningGroup);
		}
	}
}