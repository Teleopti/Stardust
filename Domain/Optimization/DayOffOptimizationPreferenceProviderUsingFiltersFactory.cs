using Teleopti.Ccc.Domain.InterfaceLegacy;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class BlockPreferenceProviderUsingFiltersFactory
	{
		private readonly IPlanningGroupSettingsRepository _planningGroupSettingsRepository;

		public BlockPreferenceProviderUsingFiltersFactory(IPlanningGroupSettingsRepository planningGroupSettingsRepository)
		{
			_planningGroupSettingsRepository = planningGroupSettingsRepository;
		}

		public IBlockPreferenceProvider Create()
		{
			return new BlockPreferenceProviderUsingFilters(_planningGroupSettingsRepository.LoadAllWithoutPlanningGroup());
		}

		public IBlockPreferenceProvider Create(IPlanningGroup planningGroup)
		{
			return new BlockPreferenceProviderUsingFilters(_planningGroupSettingsRepository.LoadAllByPlanningGroup(planningGroup));
		}
	}

	public class DayOffOptimizationPreferenceProviderUsingFiltersFactory
	{
		private readonly IPlanningGroupSettingsRepository _planningGroupSettingsRepository;

		public DayOffOptimizationPreferenceProviderUsingFiltersFactory(IPlanningGroupSettingsRepository planningGroupSettingsRepository)
		{
			_planningGroupSettingsRepository = planningGroupSettingsRepository;
		}

		public IDayOffOptimizationPreferenceProvider Create()
		{
			return new DayOffOptimizationPreferenceProviderUsingFilters(_planningGroupSettingsRepository.LoadAllWithoutPlanningGroup());
		}

		public IDayOffOptimizationPreferenceProvider Create(IPlanningGroup planningGroup)
		{
			return new DayOffOptimizationPreferenceProviderUsingFilters(_planningGroupSettingsRepository.LoadAllByPlanningGroup(planningGroup));
		}
	}
}
