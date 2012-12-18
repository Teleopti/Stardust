using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation
{
    public interface IRestrictionAggregator
    {
        IEffectiveRestriction Aggregate(IEnumerable<DateOnly> dateOnlyList, IGroupPerson groupPerson, ISchedulingOptions schedulingOptions);
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

        public IEffectiveRestriction Aggregate(IEnumerable<DateOnly> dateOnlyList, IGroupPerson groupPerson, ISchedulingOptions schedulingOptions )
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
            return effectiveRestriction;
        }
    }
}