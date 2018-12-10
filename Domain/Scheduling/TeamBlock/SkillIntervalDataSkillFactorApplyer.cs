

using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ISkillIntervalDataSkillFactorApplier
	{
		ISkillIntervalData ApplyFactors(ISkillIntervalData skillIntervalData, ISkill skill);
	}

	public class SkillIntervalDataSkillFactorApplier : ISkillIntervalDataSkillFactorApplier
	{
		private readonly ISkillPriorityProvider _skillPriorityProvider;

		public SkillIntervalDataSkillFactorApplier(ISkillPriorityProvider skillPriorityProvider)
		{
			_skillPriorityProvider = skillPriorityProvider;
		}

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
																_skillPriorityProvider.GetPriorityValue(skill), _skillPriorityProvider.GetOverstaffingFactor(skill));
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
			if (Math.Abs(overstaffingFactor.Value - 0.5) < 0.05)
				return priorityValue * currentDemandInMinutes;

			double overUnderStaffFaktor = 1;
			if (currentDemandInMinutes < 0)
				overUnderStaffFaktor = 1 - (overstaffingFactor.Value);
			return priorityValue * overUnderStaffFaktor * currentDemandInMinutes;
		}
	}
}