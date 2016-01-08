using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public interface IActivityDivider
    {
        /// <summary>
        /// Extracts the important data from the input parameters and divides them
        /// into a digestable data structure that can be fed to FurnessDataConverter.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Tamas
        /// Created date: 2008-02-07
        /// </remarks>
        IDividedActivityData DivideActivity(ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods,
                                                             IAffectedPersonSkillService affectedPersonSkillService,
                                                           IActivity activity,
														   IResourceCalculationDataContainer filteredProjections,
                                                           DateTimePeriod periodToCalculate);
    }

    public class ActivityDivider : IActivityDivider
    {
        public IDividedActivityData DivideActivity(ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods,
            IAffectedPersonSkillService affectedPersonSkillService,
            IActivity activity,
			IResourceCalculationDataContainer filteredProjections,
            DateTimePeriod periodToCalculate)
        {
            var dividedActivity = new DividedActivityData();
            var elapsedToCalculate = periodToCalculate.ElapsedTime();

            IEnumerable<ISkill> skillsForActivity = skillsInActivity(affectedPersonSkillService,activity);
            foreach (ISkill skill in skillsForActivity)
            {
                double? targetDemandValue = skillDayDemand(skill,relevantSkillStaffPeriods,periodToCalculate);
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
				var relativePersonSkillResourceRow = new Dictionary<ISkill, double>();
				var personSkillResourceRow = new Dictionary<ISkill, double>();

				foreach (var skill in skills)
				{
					var intervalPart = ((double) filteredProjections.MinSkillResolution/skill.DefaultResolution);
					var traff = periodResource.Value.Resource*intervalPart;

					double skillEfficiencyValue;
					if (!periodResource.Value.SkillEffiencies.TryGetValue(skill.Id.GetValueOrDefault(), out skillEfficiencyValue))
					{
						skillEfficiencyValue = intervalPart;
					}
					else
					{
						skillEfficiencyValue = (skillEfficiencyValue/headCount)*intervalPart;
					}

					double personSkillResourceValue = traff;
					double bitwiseSkillEfficiencyValue = skillEfficiencyValue == 0 ? 0d : 1d;
					double relativePersonSkillResourceValue = traff * bitwiseSkillEfficiencyValue;

					relativePersonSkillResourceRow.Add(skill, relativePersonSkillResourceValue);
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
					dividedActivity.RelativeKeyedSkillResourceResources.Add(periodResource.Key, relativePersonSkillResourceRow);
					dividedActivity.RelativePersonResources.Add(periodResource.Key, periodResource.Value.Resource);

					double targetResourceValue = elapsedToCalculate.TotalMinutes * periodResource.Value.Resource;
					dividedActivity.PersonResources.Add(periodResource.Key, targetResourceValue);
				}
	        }

            return dividedActivity;
        }

	    private static double? skillDayDemand(ISkill skill, ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods, DateTimePeriod periodToCalculate)
        {
            ISkillStaffPeriodDictionary skillStaffPeriods;
            bool anythingOpen = false;
            if (!relevantSkillStaffPeriods.TryGetValue(skill, out skillStaffPeriods))
                return null;

            double totalTime = 0;

            foreach (var skillStaffPeriod in skillStaffPeriods)
            {
                if (periodToCalculate.StartDateTime>skillStaffPeriod.Key.EndDateTime) continue;
                if (periodToCalculate.EndDateTime<skillStaffPeriod.Key.StartDateTime) break;

                DateTimePeriod? intersection = periodToCalculate.Intersection(skillStaffPeriod.Key);

                if (intersection.HasValue)
                {
                    if (!anythingOpen && skillStaffPeriods.SkillOpenHoursCollection.Any(openHourPeriod => openHourPeriod.Intersect(periodToCalculate)))
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

        private static IEnumerable<ISkill> skillsInActivity(IAffectedPersonSkillService affectedPersonSkillService, IActivity activity)
        {
            var distinctList = new HashSet<ISkill>();
            foreach (var affectedSkill in affectedPersonSkillService.AffectedSkills)
            {
                if (affectedSkill.Activity.Equals(activity))
                {
                    distinctList.Add(affectedSkill);
                }
            }
            return distinctList;
        }
    }
}
