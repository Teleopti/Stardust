namespace Teleopti.Ccc.Domain.Optimization
{
	public class BlockPreferenceProviderUsingFiltersFactory
	{
		public IBlockPreferenceProvider Create(PlanningGroup planningGroup)
		{
			return new BlockPreferenceProviderUsingFilters(planningGroup.Settings);
		}
	}
}