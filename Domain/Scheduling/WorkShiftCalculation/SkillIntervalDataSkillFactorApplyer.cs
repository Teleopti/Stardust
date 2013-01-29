

using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation
{
	public interface ISkillIntervalDataSkillFactorApplyer
	{
		ISkillIntervalData ApplyFactors(ISkillIntervalData skillIntervalData, ISkill skill);
	}

	public class SkillIntervalDataSkillFactorApplyer : ISkillIntervalDataSkillFactorApplyer
	{
        public SkillIntervalDataSkillFactorApplyer(){}

	    public ISkillIntervalData ApplyFactors(ISkillIntervalData skillIntervalData, ISkill skill)
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
	}
}