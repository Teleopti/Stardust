using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public interface ISingleSkillCalculator
	{
		void Calculate(IResourceCalculationDataContainer relevantProjections,
		               ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods,
					   IResourceCalculationDataContainer toRemove, IResourceCalculationDataContainer toAdd);
	}

	public class SingleSkillCalculator : ISingleSkillCalculator
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public void Calculate(IResourceCalculationDataContainer relevantProjections, 
			ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods,
			IResourceCalculationDataContainer toRemove,
			IResourceCalculationDataContainer toAdd)
		{
			foreach (KeyValuePair<ISkill, ISkillStaffPeriodDictionary> pair in relevantSkillStaffPeriods)
			{
				var skill = pair.Key;

				ISkillStaffPeriodDictionary skillStaffPeriodDictionary = pair.Value;

				foreach (ISkillStaffPeriod skillStaffPeriod in skillStaffPeriodDictionary.Values)
				{
					double result1;
					double result2;
					if(toRemove.HasItems() || toAdd.HasItems())
					{
						Tuple<double, double> resultToRemove = nonBlendSkillImpactOnPeriodForProjection(skillStaffPeriod, toRemove, skill);
						Tuple<double, double> resultToAdd = nonBlendSkillImpactOnPeriodForProjection(skillStaffPeriod, toAdd, skill);
						result1 = skillStaffPeriod.Payload.CalculatedResource - resultToRemove.Item1 + resultToAdd.Item1;
						result2 = skillStaffPeriod.Payload.CalculatedLoggedOn - resultToRemove.Item2 + resultToAdd.Item2;
					}
					else
					{
						Tuple<double, double> result = nonBlendSkillImpactOnPeriodForProjection(skillStaffPeriod, relevantProjections, skill);
						result1 = result.Item1;
						result2 = result.Item2;
					}

					if (!skillStaffPeriod.Payload.CalculatedLoggedOn.Equals(result2))
					{
						skillStaffPeriod.Payload.CalculatedLoggedOn = result2;
						skillStaffPeriod.SetCalculatedResource65(result1);
					}
					
				}
			}
		}

		private static Tuple<double, double> nonBlendSkillImpactOnPeriodForProjection(ISkillStaffPeriod skillStaffPeriod, IResourceCalculationDataContainer shiftList, ISkill skill)
		{
			return shiftList.SkillResources(skill, skillStaffPeriod.Period);
		}
	}
}