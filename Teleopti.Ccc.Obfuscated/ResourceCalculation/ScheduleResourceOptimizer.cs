﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Obfuscated.ResourceCalculation
{
    /// <summary>
    /// Takes the input domain classes, runs the resource optimalization and writes back the result
    /// data to the domain.
    /// </summary>
    public class ScheduleResourceOptimizer
    {
        private readonly ISkillSkillStaffPeriodExtendedDictionary _skillStaffPeriods;
        private readonly IAffectedPersonSkillService _personSkillService;
        private readonly IActivityDivider _activityDivider;
        private ICollection<IVisualLayerCollection> _projections;
        private readonly IList<IActivity> _distinctActivities;

        private const double _quotient = 1d; // the outer quotient: default = 1
        private const int _maximumIteration = 50; // the maximum number of iterations
        
        public ScheduleResourceOptimizer(IEnumerable<IVisualLayerCollection> relevantProjections, 
            ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods, 
            IAffectedPersonSkillService personSkillService, 
            bool clearSkillStaffPeriods, IActivityDivider activityDivider)
        {
            //should not be called from constructor!
            setProjections(relevantProjections);

            _skillStaffPeriods = relevantSkillStaffPeriods;
            _personSkillService = personSkillService;
            _activityDivider = activityDivider;
            _distinctActivities = getDistinctActivities();
            if(clearSkillStaffPeriods)
                this.clearSkillStaffPeriods();
        }

        public void Optimize(DateTimePeriod datePeriodToRecalculate, bool useOccupancyAdjustment)
        {
            var affectedSkills = _personSkillService.AffectedSkills;
			foreach (IActivity currentActivity in _distinctActivities)
			{
				optimizeActivity(affectedSkills, currentActivity, datePeriodToRecalculate, useOccupancyAdjustment);
			}
        }

        private void optimizeActivity(IEnumerable<ISkill> affectedSkills, IActivity currentActivity, DateTimePeriod datePeriodToRecalculate, bool useOccupancyAdjustment)
        {
            IList<ISkill> skills = new List<ISkill>();
            foreach (var affectedSkill in affectedSkills)
            {
                if (affectedSkill != null && affectedSkill.Activity.Equals(currentActivity))
                    skills.Add(affectedSkill);
            }

            //All skills with same activity must have the same resolution
            TimeSpan defaultResolution = TimeSpan.FromMinutes(skills[0].DefaultResolution);
            DateTime currentStart =
                datePeriodToRecalculate.StartDateTime.Date.Add(
                    TimeHelper.FitToDefaultResolution(datePeriodToRecalculate.StartDateTime.TimeOfDay,
                                                      (int) defaultResolution.TotalMinutes));

            while (currentStart < datePeriodToRecalculate.EndDateTime)
            {
                DateTime currentEnd =
                    currentStart.Add(defaultResolution);

                DateTimePeriod completeIntervalPeriod = new DateTimePeriod(currentStart, currentEnd);

                //Allways empty all periods for current skill
                foreach (ISkill skill in skills)
                {
                    ISkillStaffPeriodDictionary skillStaffPeriodDic;
                    if (_skillStaffPeriods.TryGetValue(skill, out skillStaffPeriodDic))
                    {
                        ISkillStaffPeriod skillStaffPeriod;
                        if (skillStaffPeriodDic.TryGetValue(completeIntervalPeriod, out skillStaffPeriod))
                        {
                            skillStaffPeriod.SetCalculatedResource65(0);
                            skillStaffPeriod.Payload.CalculatedLoggedOn = 0;
                        }
                    }
                }
                optimizeActivityPeriod(currentActivity, completeIntervalPeriod, useOccupancyAdjustment);
                currentStart = currentStart.Add(defaultResolution);
            }
        }

        private void setProjections(IEnumerable<IVisualLayerCollection> relevantProjections)
        {
            _projections = new List<IVisualLayerCollection>();
            foreach (var projection in relevantProjections)
            {
                if (projection.HasLayers && hasActivityLayer(projection))
                {
                    _projections.Add(projection);   
                }
            }
        }

        //move to visuallayercollection
        private static bool hasActivityLayer(IVisualLayerCollection layerCollection)
        {
            foreach (IVisualLayer visualLayer in layerCollection)
            {
                if (visualLayer.Payload is IActivity)
                    return true;
            }
            return false;
        }

        private void optimizeActivityPeriod(IActivity currentActivity, DateTimePeriod completeIntervalPeriod, bool useOccupancyAdjustment)
        {
            DateTimePeriod completeIntervalPeriodMinusOneTick = completeIntervalPeriod.ChangeEndTime(TimeSpan.FromTicks(-1));
            var filteredVisualLayers = createFilteredVisualLayerColl(completeIntervalPeriodMinusOneTick);
            if (filteredVisualLayers.Count == 0)
                return;

            IDividedActivityData dividedActivityData = _activityDivider.DivideActivity(_skillStaffPeriods, _personSkillService,
                                                                  currentActivity, filteredVisualLayers,
                                                                  completeIntervalPeriodMinusOneTick);

            IEnumerable<ISkill> relevantSkills = dividedActivityData.TargetDemands.Keys;
            IDictionary<ISkill, ISkillStaffPeriod> relevantSkillStaffPeriods =
                getRelevantSkillStaffPeriod(relevantSkills, completeIntervalPeriod);

            if (relevantSkillStaffPeriods.Count > 0)
            {
                if (useOccupancyAdjustment)
                {
                    //Do nothing here until occupancy works
                    //var occCalculator =
                    //    new OccupancyCalculator(relevantSkillStaffPeriods, dividedActivityData.RelativePersonSkillResources);
                    //occCalculator.AdjustOccupancy();
                }

                var furnessDataConverter = new FurnessDataConverter(dividedActivityData);
                IFurnessData furnessData = furnessDataConverter.ConvertDividedActivityToFurnessData();

                IFurnessEvaluator furnessEvaluator = new FurnessEvaluator(furnessData);
                furnessEvaluator.Evaluate(_quotient, _maximumIteration);
                IDividedActivityData optimizedActivityData = furnessDataConverter.ConvertFurnessDataBackToActivity();

                setFurnessResultsToSkillStaffPeriods(completeIntervalPeriod, relevantSkillStaffPeriods, optimizedActivityData);
            }
        }

        private static void setFurnessResultsToSkillStaffPeriods(DateTimePeriod completeIntervalPeriod, IDictionary<ISkill, ISkillStaffPeriod> relevantSkillStaffPeriods, IDividedActivityData optimizedActivityData)
        {
            foreach (var skillPair in relevantSkillStaffPeriods)
            {
                ISkillStaffPeriod staffPeriod = skillPair.Value;
                double resource;
                optimizedActivityData.WeightedRelativePersonSkillResourcesSum.TryGetValue(skillPair.Key, out resource);
                double calculatedresource = resource / completeIntervalPeriod.ElapsedTime().TotalMinutes;
                staffPeriod.SetCalculatedResource65(calculatedresource);
                double loggedOn;
                optimizedActivityData.RelativePersonSkillResourcesSum.TryGetValue(skillPair.Key, out loggedOn);
                staffPeriod.Payload.CalculatedLoggedOn = loggedOn;
            }
        }

        private IList<IFilteredVisualLayerCollection> createFilteredVisualLayerColl(DateTimePeriod periodToCalculate)
        {
			IList<IFilteredVisualLayerCollection> ret = new List<IFilteredVisualLayerCollection>();
            foreach (var projection in _projections)
            {
                var period = projection.Period();
                if (!period.HasValue) continue;
                if (!period.Value.Intersect(periodToCalculate)) continue;

                var coll = projection.FilterLayers(periodToCalculate);
                if(coll.HasLayers)
                {
                    ret.Add(coll);                    
                }
            }

            return ret;
        }

        private void clearSkillStaffPeriods()
        {
            foreach (ISkillStaffPeriodDictionary skillStaffDic in _skillStaffPeriods.Values)
            {
                if (((ISkill)skillStaffDic.Skill).SkillType.ForecastSource == ForecastSource.InboundTelephony)
                {
                    foreach (ISkillStaffPeriod skillStaffPeriod in skillStaffDic.Values)
                    {
                        skillStaffPeriod.Payload.CalculatedLoggedOn = 0;
                        skillStaffPeriod.SetCalculatedResource65(0);
                    }
                }
            }
        }

       private IDictionary<ISkill, ISkillStaffPeriod> getRelevantSkillStaffPeriod(IEnumerable<ISkill> relevantSkills, DateTimePeriod intervalPeriod)
        {
            IDictionary<ISkill, ISkillStaffPeriod> relevantSkillStaffPeriods = new Dictionary<ISkill, ISkillStaffPeriod>();

            foreach (ISkill skill in relevantSkills)
            {
                ISkillStaffPeriodDictionary skillStaffPeriodDictionary;
                if (!_skillStaffPeriods.TryGetValue(skill, out skillStaffPeriodDictionary)) continue;
                ISkillStaffPeriod staffPeriod;
                if (skillStaffPeriodDictionary.TryGetResolutionAdjustedValue(intervalPeriod, out staffPeriod))
                {
                    if (staffPeriod.Payload.MultiskillMinOccupancy.HasValue)
                    {
                        staffPeriod.Payload.MultiskillMinOccupancy = null;
                    }
                    
                    relevantSkillStaffPeriods.Add(skill, staffPeriod);
                }
            }
            return relevantSkillStaffPeriods;
        }

        private IList<IActivity> getDistinctActivities()
        {
            return new List<IActivity>(_personSkillService.AffectedSkills.Select(s => s.Activity).Distinct());
        }
    }
}
