using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffOptimizationPreferenceProviderUsingFilters : IDayOffOptimizationPreferenceProvider
	{
		private readonly AllSettingsForPlanningGroup _allSettingsForPlanningGroup;

		public DayOffOptimizationPreferenceProviderUsingFilters(AllSettingsForPlanningGroup allSettingsForPlanningGroup)
		{
			_allSettingsForPlanningGroup = allSettingsForPlanningGroup;
		}

		public IDaysOffPreferences ForAgent(IPerson person, DateOnly dateOnly)
		{
			return mapToDayOffPrefences(_allSettingsForPlanningGroup.ForAgent(person, dateOnly));
		}

		private static DaysOffPreferences mapToDayOffPrefences(PlanningGroupSettings planningGroupSettings)
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
