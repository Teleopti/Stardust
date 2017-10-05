using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class BlockPreferenceProviderUsingFiltersFactory
	{
		private readonly IPlanningGroupSettingsRepository _planningGroupSettingsRepository;
		private readonly ISchedulingOptionsProvider _schedulingOptionsProvider;

		public BlockPreferenceProviderUsingFiltersFactory(IPlanningGroupSettingsRepository planningGroupSettingsRepository, ISchedulingOptionsProvider schedulingOptionsProvider)
		{
			_planningGroupSettingsRepository = planningGroupSettingsRepository;
			_schedulingOptionsProvider = schedulingOptionsProvider;
		}

		public IBlockPreferenceProvider Create()
		{
			return new BlockPreferenceProviderUsingFilters(_planningGroupSettingsRepository.LoadAllWithoutPlanningGroup(), _schedulingOptionsProvider.Fetch(null));
		}

		public IBlockPreferenceProvider Create(IPlanningGroup planningGroup)
		{
			return new BlockPreferenceProviderUsingFilters(_planningGroupSettingsRepository.LoadAllByPlanningGroup(planningGroup), _schedulingOptionsProvider.Fetch(null));
		}
	}
}