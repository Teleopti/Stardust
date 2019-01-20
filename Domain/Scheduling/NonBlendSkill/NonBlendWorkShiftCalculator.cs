using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.NonBlendSkill
{
	public interface INonBlendWorkShiftCalculator
	{
		IWorkShiftCalculationResultHolder CalculateShiftValue(IPerson person, IVisualLayerCollection layers, IDictionary<ISkill, ISkillStaffPeriodDictionary> skillStaffPeriods,
			WorkShiftLengthHintOption lengthFactor, bool useMinimumPersons, bool useMaximumPersons);
	}

	public class NonBlendWorkShiftCalculator : INonBlendWorkShiftCalculator
	{
	    private readonly INonBlendSkillImpactOnPeriodForProjection _nonBlendSkillImpactOnPeriodForProjection;
		private readonly IWorkShiftCalculator _workShiftCalculator;
		private readonly ISkillPriorityProvider _skillPriorityProvider;

		public NonBlendWorkShiftCalculator(INonBlendSkillImpactOnPeriodForProjection nonBlendSkillImpactOnPeriodForProjection, IWorkShiftCalculator workShiftCalculator, ISkillPriorityProvider skillPriorityProvider)
        {
        	_nonBlendSkillImpactOnPeriodForProjection = nonBlendSkillImpactOnPeriodForProjection;
			_workShiftCalculator = workShiftCalculator;
			_skillPriorityProvider = skillPriorityProvider;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
        public IWorkShiftCalculationResultHolder CalculateShiftValue(IPerson person, IVisualLayerCollection layers, IDictionary<ISkill, ISkillStaffPeriodDictionary> skillStaffPeriods,
			WorkShiftLengthHintOption lengthFactor, bool useMinimumPersons, bool useMaximumPersons)
		{
			DateTimePeriod? vcPeriod = layers.Period();
			if (!vcPeriod.HasValue)
				return new WorkShiftCalculationResult();

			double result = 0;
	        
			foreach (KeyValuePair<ISkill, ISkillStaffPeriodDictionary> skillStaffPeriodDictionaryKeyValue in skillStaffPeriods)
			{
				ISkill skill = skillStaffPeriodDictionaryKeyValue.Key;
                // we don't check this for speed reasons
                //if (skill.SkillType.ForecastSource != ForecastSource.NonBlendSkill)
                //    continue;
			    
                foreach (KeyValuePair<DateTimePeriod, ISkillStaffPeriod> keyValuePair in skillStaffPeriodDictionaryKeyValue.Value)
				{
					if (!keyValuePair.Key.Intersect(vcPeriod.Value))
						continue;

					ISkillStaffPeriod skillStaffPeriod = keyValuePair.Value;
                    // we don't check either this for speed reasons
                    //DateOnly dateOnly = _nonBlendSkillImpactOnPeriodForProjection.SkillStaffPeriodDate(skillStaffPeriod, person);
                    //if (!_nonBlendSkillImpactOnPeriodForProjection.CheckPersonSkill(skill, person, dateOnly))
                    //    continue;

                    double thisImpact = _nonBlendSkillImpactOnPeriodForProjection.CalculatePeriod(skillStaffPeriod, layers, skill.Activity);

					double absoluteDifferenceScheduledHeadsAndMinMaxHeads = skillStaffPeriod.AbsoluteDifferenceScheduledHeadsAndMinMaxHeads(true);
					var origDemand = (int)Math.Round(skillStaffPeriod.FStaffTime().TotalMinutes);
					var assignedResource =
						(int)
						Math.Round(skillStaffPeriod.Payload.CalculatedResource *
								   skillStaffPeriod.Period.ElapsedTime().TotalMinutes);
					int maximumPersons = skillStaffPeriod.Payload.SkillPersonData.MaximumPersons;
					int minimumPersons = skillStaffPeriod.Payload.SkillPersonData.MinimumPersons;
					ISkillStaffPeriodDataHolder dataHolder = new SkillStaffPeriodDataInfo(origDemand,
																									assignedResource,
																									skillStaffPeriod.
																										Period,
																									minimumPersons,
																									maximumPersons,
																									absoluteDifferenceScheduledHeadsAndMinMaxHeads,
																									skillStaffPeriod.PeriodDistribution, _skillPriorityProvider.GetOverstaffingFactor(skill), _skillPriorityProvider.GetPriorityValue(skill));

					var intervalLength = (int)skillStaffPeriod.Period.ElapsedTime().TotalMinutes;
					var minutesImpact = (int)(intervalLength * thisImpact);
                    double periodValue = dataHolder.PeriodValue(minutesImpact, useMinimumPersons, useMaximumPersons);

					result += _workShiftCalculator.CalculateShiftValueForPeriod(periodValue, minutesImpact,
					                                                           lengthFactor, intervalLength);
					
				}
			}

			return new WorkShiftCalculationResult{ Value = result};
        }
    }
}
