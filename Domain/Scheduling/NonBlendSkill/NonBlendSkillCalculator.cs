using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.NonBlendSkill
{
	public class NonBlendSkillCalculator : INonBlendSkillCalculator
	{
		public void Calculate(DateOnly day, IResourceCalculationDataContainer relevantProjections, ISkillResourceCalculationPeriodDictionary relevantSkillStaffPeriods, bool addToEarlierResult)
		{
			foreach (KeyValuePair<ISkill, IResourceCalculationPeriodDictionary> pair in relevantSkillStaffPeriods.Items())
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