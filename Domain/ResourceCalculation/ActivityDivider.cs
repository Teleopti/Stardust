using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class ActivityDivider
	{
        public DividedActivityData DivideActivity(ISkillResourceCalculationPeriodDictionary relevantSkillStaffPeriods,
			AffectedPersonSkillService affectedPersonSkillService,
            IActivity activity,
			IResourceCalculationDataContainer filteredProjections,
            DateTimePeriod periodToCalculate)
        {
            var dividedActivity = new DividedActivityData();
            var elapsedToCalculate = periodToCalculate.ElapsedTime();

            var skillsForActivity = affectedPersonSkillService.ActivityLookup[activity].ToHashSet();
            foreach (ISkill skill in skillsForActivity)
			{
                double? targetDemandValue = skillDayDemand(skill,relevantSkillStaffPeriods, periodToCalculate);
                if (targetDemandValue.HasValue)
                    dividedActivity.TargetDemands.Add(skill, targetDemandValue.Value);
            }

	        var periodResources = filteredProjections.AffectedResources(activity, periodToCalculate);
	        foreach (var periodResource in periodResources)
	        {
				if (periodResource.Value.Resource == 0) continue;
		        var skills = periodResource.Value.Skills;
				var headCount = periodResource.Value.Count;

				var personSkillEfficiencyRow = new Dictionary<ISkill, double>();
				var personSkillResourceRow = new Dictionary<ISkill, double>();

				foreach (var skill in skills)
				{
					var traff = periodResource.Value.Resource;

					double skillEfficiencyValue;
					if (!periodResource.Value.SkillEffiencies.TryGetValue(skill.Id.GetValueOrDefault(), out skillEfficiencyValue) || headCount == 0)
					{
						skillEfficiencyValue = 1;
					}
					else
					{
						skillEfficiencyValue = skillEfficiencyValue/headCount;
					}

					double personSkillResourceValue = traff;
					double bitwiseSkillEfficiencyValue = skillEfficiencyValue == 0 ? 0d : 1d;
					double relativePersonSkillResourceValue = traff * bitwiseSkillEfficiencyValue;
					personSkillResourceRow.Add(skill, personSkillResourceValue);
					personSkillEfficiencyRow.Add(skill, skillEfficiencyValue);

					// add to sum also
					double currentRelativePersonSkillResourceValue;
					if (dividedActivity.RelativePersonSkillResourcesSum.TryGetValue(skill, out currentRelativePersonSkillResourceValue))
					{
						dividedActivity.RelativePersonSkillResourcesSum[skill] = currentRelativePersonSkillResourceValue + relativePersonSkillResourceValue;
					}
					else
					{
						dividedActivity.RelativePersonSkillResourcesSum.Add(skill, relativePersonSkillResourceValue);
					}
				}

				if (!dividedActivity.KeyedSkillResourceEfficiencies.ContainsKey(periodResource.Key))
				{
					dividedActivity.KeyedSkillResourceEfficiencies.Add(periodResource.Key, personSkillEfficiencyRow);
					dividedActivity.WeightedRelativeKeyedSkillResourceResources.Add(periodResource.Key, personSkillResourceRow);
					dividedActivity.RelativePersonResources.Add(periodResource.Key, periodResource.Value.Resource);

					double targetResourceValue = elapsedToCalculate.TotalMinutes * periodResource.Value.Resource;
					dividedActivity.PersonResources.Add(periodResource.Key, targetResourceValue);
				}
	        }

            return dividedActivity;
        }

		private static double? skillDayDemand(ISkill skill, ISkillResourceCalculationPeriodDictionary relevantSkillStaffPeriods, DateTimePeriod periodToCalculate)
        {
			IResourceCalculationPeriodDictionary skillStaffPeriods;
            bool anythingOpen = false;
            if (!relevantSkillStaffPeriods.TryGetValue(skill, out skillStaffPeriods))
                return null;

            double totalTime = 0;

            foreach (var skillStaffPeriod in skillStaffPeriods.Items())
            {
                if (periodToCalculate.StartDateTime>skillStaffPeriod.Key.EndDateTime) continue;
                if (periodToCalculate.EndDateTime<skillStaffPeriod.Key.StartDateTime) break;

                DateTimePeriod? intersection = periodToCalculate.Intersection(skillStaffPeriod.Key);

                if (intersection.HasValue)
                {
                    if (!anythingOpen && relevantSkillStaffPeriods.IsOpen(skill, periodToCalculate))
                    {
	                    anythingOpen = true;
                    }

                    double skillStaffPeriodSeconds = skillStaffPeriod.Key.ElapsedTime().TotalSeconds;
                    double intersectPercent =
                        intersection.Value.ElapsedTime().TotalSeconds/
                        skillStaffPeriodSeconds;
                    totalTime += skillStaffPeriod.Value.ForecastedDistributedDemand*
                                 skillStaffPeriodSeconds*intersectPercent;
                }
            }
            if (!anythingOpen)
                return null;

			return totalTime / periodToCalculate.ElapsedTime().TotalSeconds;
        }
    }
}
