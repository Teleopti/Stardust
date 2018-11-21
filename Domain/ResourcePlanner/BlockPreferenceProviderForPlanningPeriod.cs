using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.Domain.ResourcePlanner
{
	public class BlockPreferenceProviderForPlanningPeriod : IBlockPreferenceProviderForPlanningPeriod
	{	
		public IBlockPreferenceProvider Fetch(PlanningGroup planningGroup)
		{
			return new BlockPreferenceProviderUsingFilters(planningGroup.Settings);
		}
	}
}