using System;
using System.Collections.Generic;
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
														   IList<IFilteredVisualLayerCollection> filteredProjections,
                                                           DateTimePeriod periodToCalculate);
    }

    public class ActivityDivider : IActivityDivider
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3")]
        public IDividedActivityData DivideActivity(ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods,
            IAffectedPersonSkillService affectedPersonSkillService,
            IActivity activity,
			IList<IFilteredVisualLayerCollection> filteredProjections,
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

            foreach (var layerColl in filteredProjections)
            {
                var currentPerson = layerColl.Person;
            	var projectionStart = layerColl.OriginalProjectionPeriod.GetValueOrDefault(periodToCalculate).StartDateTime;
            	var projectionStartDate = new DateOnly(TimeZoneHelper.ConvertFromUtc(projectionStart,
            	                                                        currentPerson.PermissionInformation.DefaultTimeZone
            	                                                        	()));
                double traff = relativPersonResource(layerColl,activity,periodToCalculate);
                if (traff > 0)
                {
                    var personSkillEfficiencyRow = new Dictionary<ISkill, double>();
                    var relativePersonSkillResourceRow = new Dictionary<ISkill, double>();
                    var personSkillResourceRow = new Dictionary<ISkill, double>();

                    ICollection<IPersonSkill> affectedPersonSkills =
                        affectedPersonSkillService.Execute(currentPerson, activity, projectionStartDate);

                    foreach (IPersonSkill personSkill in affectedPersonSkills)
                    {
                        ISkill skill = personSkill.Skill;

                        double skillEfficiencyValue = personSkill.SkillPercentage.Value;

                        double personSkillResourceValue = traff;
                        double bitwiseSkillEfficiencyValue = skillEfficiencyValue == 0 ? 0d : 1d;
                        double relativePersonSkillResourceValue = traff*bitwiseSkillEfficiencyValue;

                        relativePersonSkillResourceRow.Add(skill, relativePersonSkillResourceValue);
                        personSkillResourceRow.Add(skill, personSkillResourceValue);
                        personSkillEfficiencyRow.Add(skill, skillEfficiencyValue);

                        // add to sum also
                        double currentRelativePersonSkillResourceValue;
                        if (dividedActivity.RelativePersonSkillResourcesSum.TryGetValue(skill,out currentRelativePersonSkillResourceValue))
                        {
                            dividedActivity.RelativePersonSkillResourcesSum[skill] = currentRelativePersonSkillResourceValue + relativePersonSkillResourceValue;
                        }
                        else
                        {
                            dividedActivity.RelativePersonSkillResourcesSum.Add(skill, relativePersonSkillResourceValue);
                        }

                    }

					if (!dividedActivity.PersonSkillEfficiencies.ContainsKey(currentPerson))
					{
						dividedActivity.PersonSkillEfficiencies.Add(currentPerson, personSkillEfficiencyRow);
						dividedActivity.WeightedRelativePersonSkillResources.Add(currentPerson, personSkillResourceRow);
						dividedActivity.RelativePersonSkillResources.Add(currentPerson, relativePersonSkillResourceRow);
						dividedActivity.RelativePersonResources.Add(currentPerson, traff);

						double targetResourceValue = elapsedToCalculate.TotalMinutes * traff;
						dividedActivity.PersonResources.Add(currentPerson, targetResourceValue);
					}
                }
            }

            return dividedActivity;
        }

        #region Private methods

        private static double? skillDayDemand(ISkill skill, ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods, DateTimePeriod periodToCalculate)
        {
            double retVal = 0;
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
                    foreach (var openHourPeriod in skillStaffPeriods.SkillOpenHoursCollection)
                    {
                        if (openHourPeriod.Intersect(periodToCalculate))
                            anythingOpen = true;
                    }

                    double skillStaffPeriodSeconds = skillStaffPeriod.Key.ElapsedTime().TotalSeconds;
                    double intersectPercent =
                        intersection.Value.ElapsedTime().TotalSeconds/
                        skillStaffPeriodSeconds;
                    totalTime += skillStaffPeriod.Value.ForecastedDistributedDemand*
                                 skillStaffPeriodSeconds*intersectPercent;
                }


                retVal = totalTime / periodToCalculate.ElapsedTime().TotalSeconds;
            }
            if (!anythingOpen)
                return null;

            return retVal;
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

        private static double relativPersonResource(IVisualLayerCollection layers, IActivity activity, DateTimePeriod periodToCalculate)
        {
            double totalSeconds = 0;

            foreach (IVisualLayer layer in layers.FilterLayers(activity))
            {
                totalSeconds += layer.Period.Intersection(periodToCalculate).Value.ElapsedTime().TotalSeconds;
            }

            return totalSeconds / periodToCalculate.ElapsedTime().TotalSeconds;
        }

 
        #endregion

    }
}
