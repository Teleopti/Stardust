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

		public IBlockPreferenceProvider Create(IPlanningGroup planningGroup)
		{
			return new BlockPreferenceProviderUsingFilters(_planningGroupSettingsRepository.LoadAllByPlanningGroup(planningGroup));
		}
	}
}