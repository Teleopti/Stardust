using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.NonBlendSkill
{
	public class NonBlendSkillCalculator : INonBlendSkillCalculator
	{
		public void Calculate(DateOnly day, IResourceCalculationDataContainer relevantProjections,
			IEnumerable<KeyValuePair<ISkill, IResourceCalculationPeriodDictionary>> relevantResourceCalculationPeriods,
			bool addToEarlierResult)
		{
			foreach (var pair in relevantResourceCalculationPeriods)
			{
				var skill = pair.Key;
				if (skill.SkillType.ForecastSource != ForecastSource.NonBlendSkill)
					continue;

				var skillStaffPeriodDictionary = pair.Value;

				foreach (var skillStaffPeriod in skillStaffPeriodDictionary.OnlyValues())
				{
					var result = relevantProjections.SkillResources(skill, skillStaffPeriod.CalculationPeriod).Item1;
					if (addToEarlierResult)
						result += skillStaffPeriod.CalculatedLoggedOn;

					skillStaffPeriod.SetCalculatedLoggedOn(result);
					skillStaffPeriod.SetCalculatedResource65(result);
				}
			}
		}
	}
}