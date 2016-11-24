﻿using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios
{
	public static class ResourceCalculationDataCreator
	{
		public static IResourceCalculationData WithData(IScenario scenario, 
											DateOnly date, 
											IEnumerable<IPersistableScheduleData> persistableScheduleData, 
											IEnumerable<ISkillDay> skillDays,
											bool considerShortbreaks,
											bool doIntraIntervalCalculation)
		{
			return WithData(scenario, date.ToDateOnlyPeriod(), persistableScheduleData, skillDays, considerShortbreaks, doIntraIntervalCalculation);
		}

		public static IResourceCalculationData WithData(IScenario scenario,
									DateOnlyPeriod period,
									IEnumerable<IPersistableScheduleData> persistableScheduleData,
									IEnumerable<ISkillDay> skillDays,
									bool considerShortbreaks,
									bool doIntraIntervalCalculation)
		{
			var skillDaysDic = skillDays.GroupBy(x => x.Skill).ToDictionary(k=> k.Key, v => v.AsEnumerable());

			return new ResourceCalculationData(
				ScheduleDictionaryCreator.WithData(scenario, period, persistableScheduleData),
				skillDaysDic.Keys, skillDaysDic, considerShortbreaks, doIntraIntervalCalculation);
		}
	}
}