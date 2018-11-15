using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class BlockPreferenceProviderUsingFiltersFactory
	{
		private readonly ISchedulingOptionsProvider _schedulingOptionsProvider;

		public BlockPreferenceProviderUsingFiltersFactory(ISchedulingOptionsProvider schedulingOptionsProvider)
		{
			_schedulingOptionsProvider = schedulingOptionsProvider;
		}

		public IBlockPreferenceProvider Create(PlanningGroup planningGroup)
		{
			return new BlockPreferenceProviderUsingFilters(planningGroup.Settings, _schedulingOptionsProvider.Fetch(null));
		}
	}
}