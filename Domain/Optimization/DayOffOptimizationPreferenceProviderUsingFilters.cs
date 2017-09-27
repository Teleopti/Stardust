﻿using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffOptimizationPreferenceProviderUsingFilters : IDayOffOptimizationPreferenceProvider
	{
		private readonly IEnumerable<PlanningGroupSettings> _dayOffRules;

		public DayOffOptimizationPreferenceProviderUsingFilters(IEnumerable<PlanningGroupSettings> dayOffRules)
		{
			_dayOffRules = dayOffRules.OrderBy(x => x.Default);
		}

		public IDaysOffPreferences ForAgent(IPerson person, DateOnly dateOnly)
		{
			return mapToDayOffPrefences(_dayOffRules.Where(dayOffRule => dayOffRule.IsValidForAgent(person, dateOnly)).OrderByDescending(x=>x.Priority).FirstOrDefault() ?? PlanningGroupSettings.CreateDefault());
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
				UseDaysOffPerWeek = true
			};
		}
	}
}
