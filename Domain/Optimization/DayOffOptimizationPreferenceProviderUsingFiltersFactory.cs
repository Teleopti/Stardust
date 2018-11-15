namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffOptimizationPreferenceProviderUsingFiltersFactory
	{
		private readonly IPlanningGroupSettingsRepository _planningGroupSettingsRepository;

		public DayOffOptimizationPreferenceProviderUsingFiltersFactory(IPlanningGroupSettingsRepository planningGroupSettingsRepository)
		{
			_planningGroupSettingsRepository = planningGroupSettingsRepository;
		}

		public IDayOffOptimizationPreferenceProvider Create(PlanningGroup planningGroup)
		{
			return new DayOffOptimizationPreferenceProviderUsingFilters(_planningGroupSettingsRepository.LoadAllByPlanningGroup(planningGroup));
		}
	}
}
