using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffOptimizationPreferenceProviderUsingFilters : IDayOffOptimizationPreferenceProvider
	{
		private readonly AllPlanningGroupSettings _allPlanningGroupSettings;

		public DayOffOptimizationPreferenceProviderUsingFilters(AllPlanningGroupSettings allPlanningGroupSettings)
		{
			_allPlanningGroupSettings = allPlanningGroupSettings;
		}

		public IDaysOffPreferences ForAgent(IPerson person, DateOnly dateOnly)
		{
			return mapToDayOffPreferences(_allPlanningGroupSettings.ForAgent(person, dateOnly));
		}

		private static DaysOffPreferences mapToDayOffPreferences(PlanningGroupSettings planningGroupSettings)
		{
			return new DaysOffPreferences
			{
				ConsecutiveDaysOffValue = planningGroupSettings.ConsecutiveDayOffs,
				UseConsecutiveDaysOff = true,
				ConsecutiveWorkdaysValue = planningGroupSettings.ConsecutiveWorkdays,
				UseConsecutiveWorkdays = true,
				ConsiderWeekAfter = true,
				ConsiderWeekBefore = true,
				DaysOffPerWeekValue = planningGroupSettings.DayOffsPerWeek,
				UseDaysOffPerWeek = true,
				FullWeekendsOffValue = planningGroupSettings.FullWeekendsOff,
				UseFullWeekendsOff = true,
				WeekEndDaysOffValue = planningGroupSettings.WeekendDaysOff,
				UseWeekEndDaysOff = true
			};
		}
	}
}
