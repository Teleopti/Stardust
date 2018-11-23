using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.Domain.ResourcePlanner
{
	public class BlockPreferenceProviderForPlanningPeriod : IBlockPreferenceProviderForPlanningPeriod
	{	
		public IBlockPreferenceProvider Fetch(AllSettingsForPlanningGroup allSettingsForPlanningGroup)
		{
			return new BlockPreferenceProviderUsingFilters(allSettingsForPlanningGroup);
		}
	}
}