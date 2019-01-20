using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

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
        IDividedActivityData DivideActivity(ISkillResourceCalculationPeriodDictionary relevantSkillStaffPeriods,
                                                             ILookup<IActivity,ISkill> affectedPersonSkillService,
                                                           IActivity activity,
			IResourceCalculationDataContainer filteredProjections,
                                                           DateTimePeriod periodToCalculate);

		DateTimePeriod FetchPeriodForSkill(DateTimePeriod period, TimeZoneInfo timeZone);
	}

    public class ActivityDivider : IActivityDivider
	{
        public IDividedActivityData DivideActivity(ISkillResourceCalculationPeriodDictionary relevantSkillStaffPeriods,
			ILookup<IActivity, ISkill> affectedPersonSkillService,
            IActivity activity,
			IResourceCalculationDataContainer filteredProjections,
            DateTimePeriod periodToCalculate)
        {
            var dividedActivity = new DividedActivityData();
            var elapsedToCalculate = periodToCalculate.ElapsedTime();

            var skillsForActivity = affectedPersonSkillService[activity].ToHashSet();
            foreach (ISkill skill in skillsForActivity)
			{
				var periodToCalculateAdjusted = FetchPeriodForSkill(periodToCalculate, skill.TimeZone);
                double? targetDemandValue = skillDayDemand(skill,relevantSkillStaffPeriods, periodToCalculateAdjusted);
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

		public DateTimePeriod FetchPeriodForSkill(DateTimePeriod period, TimeZoneInfo timeZone)
		{
			var minutesOffset = timeZone.BaseUtcOffset.Minutes;
			if (minutesOffset == 0)
				return period;

			minutesOffset = 60 - minutesOffset;
			if (minutesOffset > 60)
				minutesOffset = minutesOffset % 60 * -1;

			return period.MovePeriod(TimeSpan.FromMinutes(minutesOffset));
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
