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
			return new DayOffOptimizationPreferenceProviderUsingFilters(_dayOffRulesRepository.LoadAll());
		} 
	}
}
