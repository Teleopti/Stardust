using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation
{
    public interface IRestrictionAggregator
    {
        IEffectiveRestriction Aggregate(IList<DateOnly> dateOnlyList, IGroupPerson groupPerson, ISchedulingOptions schedulingOptions);
    }

    public class RestrictionAggregator : IRestrictionAggregator
    {
        private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
        private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
        
        public RestrictionAggregator(
            IEffectiveRestrictionCreator effectiveRestrictionCreator,
            ISchedulingResultStateHolder schedulingResultStateHolder)
        {
            _effectiveRestrictionCreator = effectiveRestrictionCreator;
            _schedulingResultStateHolder = schedulingResultStateHolder;
        }

        public IEffectiveRestriction Aggregate(IList<DateOnly> dateOnlyList, IGroupPerson groupPerson, ISchedulingOptions schedulingOptions)
        {
            var scheduleDictionary = _schedulingResultStateHolder.Schedules;
            if (groupPerson == null)
                return null;
            IEffectiveRestriction effectiveRestriction = null;
            foreach (var dateOnly in dateOnlyList)
            {
                var restriction = _effectiveRestrictionCreator.GetEffectiveRestriction(groupPerson.GroupMembers,
                                                                                       dateOnly, schedulingOptions,
                                                                                       scheduleDictionary);
                if (restriction == null)
                    return null;
                if (effectiveRestriction != null)
                    effectiveRestriction = effectiveRestriction.Combine(restriction);
                else
                    effectiveRestriction = restriction;
                if (effectiveRestriction == null)
                    return null;
            }

            var openHoursRestriction = openHoursToEfffectiveRestriction(dateOnlyList);
            if (effectiveRestriction != null)
                effectiveRestriction = effectiveRestriction.Combine(openHoursRestriction);
            return effectiveRestriction;
        }

        private IEffectiveRestriction openHoursToEfffectiveRestriction(IList<DateOnly> dateOnlyList)
        {
            var skillDays = _schedulingResultStateHolder.SkillDaysOnDateOnly(dateOnlyList);
            var openHours = new List<TimePeriod>();
            var reducedOpenHours = new List<TimePeriod>();
            skillDays.ForEach(s => openHours.AddRange(s.OpenHours()));
            reducedOpenHours.Add(new TimePeriod(TimeSpan.MinValue, TimeSpan.MaxValue));
            foreach (var timePeriod in openHours)
            {
                for (var i = 0; i < reducedOpenHours.Count; i++ )
                {
                    var intersection = reducedOpenHours[i].Intersection(timePeriod);
                    if (intersection != null)
                    {
                        if (!reducedOpenHours.Contains(intersection.Value))
                        {
                            reducedOpenHours.RemoveAt(i);
                            reducedOpenHours.Add(intersection.Value);
                        }
                    }
                    else
                    {
                        reducedOpenHours.Add(timePeriod);
                    }
                }
            }
            var latestStartTime = TimeSpan.MinValue;
            var earliestEndTime = TimeSpan.MaxValue;
            foreach (var timePeriod in reducedOpenHours)
            {
                if (timePeriod.StartTime > latestStartTime)
                    latestStartTime = timePeriod.StartTime;
                if (timePeriod.EndTime < earliestEndTime)
                    earliestEndTime = timePeriod.EndTime;
            }
            var startTimeLimitation = new StartTimeLimitation(latestStartTime, null);
            var endTimeLimitation = new EndTimeLimitation(null, earliestEndTime);
            var workTimeLimitation = new WorkTimeLimitation();
            var restriction = new EffectiveRestriction(startTimeLimitation, endTimeLimitation,
                                                                   workTimeLimitation, null, null, null,
                                                                   new List<IActivityRestriction>());
            return restriction;
        }
    }
}