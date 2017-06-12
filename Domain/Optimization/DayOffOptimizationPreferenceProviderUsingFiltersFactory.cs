using Teleopti.Ccc.Domain.InterfaceLegacy;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffOptimizationPreferenceProviderUsingFiltersFactory
	{
		private readonly IDayOffRulesRepository _dayOffRulesRepository;

		public DayOffOptimizationPreferenceProviderUsingFiltersFactory(IDayOffRulesRepository dayOffRulesRepository)
		{
			_dayOffRulesRepository = dayOffRulesRepository;
		}

		public IDayOffOptimizationPreferenceProvider Create()
		{
			return new DayOffOptimizationPreferenceProviderUsingFilters(_dayOffRulesRepository.LoadAllWithoutPlanningGroup());
		}

		public IDayOffOptimizationPreferenceProvider Create(IPlanningGroup planningGroup)
		{
			return new DayOffOptimizationPreferenceProviderUsingFilters(_dayOffRulesRepository.LoadAllByPlanningGroup(planningGroup));
		}
	}
}
