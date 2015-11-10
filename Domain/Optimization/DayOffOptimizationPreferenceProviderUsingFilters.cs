using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffOptimizationPreferenceProviderUsingFilters : IDayOffOptimizationPreferenceProvider
	{
		private readonly IEnumerable<DayOffRules> _dayOffRules;

		public DayOffOptimizationPreferenceProviderUsingFilters(IEnumerable<DayOffRules> dayOffRules)
		{
			_dayOffRules = dayOffRules.OrderBy(x => x.Default);
		}

		public IDaysOffPreferences ForAgent(IPerson person, DateOnly dateOnly)
		{
			foreach (var dayOffRule in _dayOffRules.Where(dayOffRule => dayOffRule.IsValidForAgent(person, dateOnly)))
			{
				return mapToDayOffPrefences(dayOffRule);
			}

			return mapToDayOffPrefences(DayOffRules.CreateDefault());
		}

		private static DaysOffPreferences mapToDayOffPrefences(DayOffRules dayOffRules)
		{
			return new DaysOffPreferences
			{
				ConsecutiveDaysOffValue = dayOffRules.ConsecutiveDayOffs,
				UseConsecutiveDaysOff = true,
				ConsecutiveWorkdaysValue = dayOffRules.ConsecutiveWorkdays,
				UseConsecutiveWorkdays = true,
				ConsiderWeekAfter = true,
				ConsiderWeekBefore = true,
				DaysOffPerWeekValue = dayOffRules.DayOffsPerWeek,
				UseDaysOffPerWeek = true
			};
		}
	}
}
