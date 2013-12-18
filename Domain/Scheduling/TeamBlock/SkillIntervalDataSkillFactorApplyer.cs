

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
        public SkillIntervalDataSkillFactorApplier(){}

	    public ISkillIntervalData ApplyFactors(ISkillIntervalData skillIntervalData, ISkill skill)
		{
	        if (skillIntervalData != null && skill != null)
	        {
	                double currentDemand = skillIntervalData.CurrentDemand * skill.PriorityValue;
	                if(currentDemand > 0 && skill.OverstaffingFactor.Value < 0.5)
	                {
	                    currentDemand = currentDemand + (((0.5 - skill.OverstaffingFactor.Value)*2)*currentDemand);
	                }

	                if(currentDemand < 0 && skill.OverstaffingFactor.Value > 0.5)
	                {
	                    currentDemand = currentDemand + (((skill.OverstaffingFactor.Value - 0.5)*2)*currentDemand);
	                }

	                double diff = currentDemand - skillIntervalData.CurrentDemand;
	                double forecastedDemand = skillIntervalData.ForecastedDemand + diff;

	                return new SkillIntervalData(skillIntervalData.Period, forecastedDemand, currentDemand,
	                                             skillIntervalData.CurrentHeads, skillIntervalData.MinimumHeads,
	                                             skillIntervalData.MaximumHeads);
	            
	        }
	        return null;
		}
	}
}