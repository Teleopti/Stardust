using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffOptimizationPreferenceProviderCreator
	{
		private readonly IDayOffRulesRepository _dayOffRulesRepository;

		public DayOffOptimizationPreferenceProviderCreator(IDayOffRulesRepository dayOffRulesRepository)
		{
			_dayOffRulesRepository = dayOffRulesRepository;
		}

		public IDayOffOptimizationPreferenceProvider Create(IDaysOffPreferences daysOffPreferences)
		{

			if(daysOffPreferences != null)
				return new DayOffOptimizationPreferenceProvider(daysOffPreferences);

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

			return new DayOffOptimizationPreferenceProviderWeb(preferences);
		} 
	}
}
