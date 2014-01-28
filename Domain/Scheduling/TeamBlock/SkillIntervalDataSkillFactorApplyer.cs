

using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ISkillIntervalDataSkillFactorApplier
	{
		ISkillIntervalData ApplyFactors(ISkillIntervalData skillIntervalData, ISkill skill);
	}

	public class SkillIntervalDataSkillFactorApplier : ISkillIntervalDataSkillFactorApplier
	{
		//PBI 1156 contains an excel-sheet on how to calculate
		public ISkillIntervalData ApplyFactors(ISkillIntervalData skillIntervalData, ISkill skill)
		{
			if (skillIntervalData != null && skill != null)
			{
				var intervalLength = skillIntervalData.Resolution().TotalMinutes;
				var originalDemandsInMinutes = skillIntervalData.ForecastedDemand*intervalLength;
				var assignedResourcesInMinutes = (skillIntervalData.ForecastedDemand - skillIntervalData.CurrentDemand) *
				                                 intervalLength;
				
				double tweakedDemand = getTweakedCurrentDemand(originalDemandsInMinutes, assignedResourcesInMinutes,
				                                                skill.PriorityValue, skill.OverstaffingFactor);
				//Change sign because of demand is positive in teamblock
				double currentDemand = tweakedDemand/intervalLength*-1;

				return new SkillIntervalData(skillIntervalData.Period, skillIntervalData.ForecastedDemand, currentDemand,
				                             skillIntervalData.CurrentHeads, skillIntervalData.MinimumHeads,
				                             skillIntervalData.MaximumHeads);

			}
			return null;
		}

		private double getTweakedCurrentDemand(double originalDemandInMinutes, double assignedResourceInMinutes, double priorityValue, Percent overstaffingFactor)
		{
			double currentDemandInMinutes = assignedResourceInMinutes - originalDemandInMinutes;
			double overUnderStaffFaktor = overstaffingFactor.Value;
			if (currentDemandInMinutes < 0)
				overUnderStaffFaktor = 1 - overstaffingFactor.Value;
			return priorityValue * overUnderStaffFaktor * currentDemandInMinutes;
		}
	}
}