using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.NonBlendSkill
{
	public class NonBlendSkillCalculator : INonBlendSkillCalculator
	{
		private readonly INonBlendSkillImpactOnPeriodForProjection _nonBlendSkillImpactOnPeriodForProjection;

		public NonBlendSkillCalculator(INonBlendSkillImpactOnPeriodForProjection nonBlendSkillImpactOnPeriodForProjection)
		{
			_nonBlendSkillImpactOnPeriodForProjection = nonBlendSkillImpactOnPeriodForProjection;
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
        public void Calculate(DateOnly day, IResourceCalculationDataContainer relevantProjections, ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods, bool addToEarlierResult)
		{

			foreach (KeyValuePair<ISkill, ISkillStaffPeriodDictionary> pair in relevantSkillStaffPeriods)
			{
                var skill = pair.Key;
                if (skill.SkillType.ForecastSource != ForecastSource.NonBlendSkill)
					continue;

				ISkillStaffPeriodDictionary skillStaffPeriodDictionary = pair.Value;
			    
				foreach (ISkillStaffPeriod skillStaffPeriod in skillStaffPeriodDictionary.Values)
				{
                    var result = relevantProjections.SkillResources(skill, skillStaffPeriod.Period).Item1;
                    if (addToEarlierResult)
                        result += skillStaffPeriod.Payload.CalculatedLoggedOn;
					skillStaffPeriod.Payload.CalculatedLoggedOn = result;
					skillStaffPeriod.SetCalculatedResource65(result);
				}
			}
		}
	}
}