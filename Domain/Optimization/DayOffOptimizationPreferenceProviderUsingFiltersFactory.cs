using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffOptimizationPreferenceProviderUsingFiltersFactory
	{
		private readonly IDayOffRulesRepository _dayOffRulesRepository;

		public DayOffOptimizationPreferenceProviderUsingFiltersFactory(IDayOffRulesRepository dayOffRulesRepository)
		{
			_dayOffRulesRepository = dayOffRulesRepository;
		}

		public IDayOffOptimizationPreferenceProvider Create(IDaysOffPreferences daysOffPreferences)
		{
			var defaultRules = _dayOffRulesRepository.Default();

			var preferences = new DaysOffPreferences
			{
				ConsecutiveDaysOffValue = defaultRules.ConsecutiveDayOffs,
				UseConsecutiveDaysOff = true,
				ConsecutiveWorkdaysValue = defaultRules.ConsecutiveWorkdays,
				UseConsecutiveWorkdays = true,
				ConsiderWeekAfter = true,
				ConsiderWeekBefore = true,
				DaysOffPerWeekValue = defaultRules.DayOffsPerWeek,
				UseDaysOffPerWeek = true
			};

			return new DayOffOptimizationPreferenceProviderUsingFilters(preferences);
		} 
	}
}
