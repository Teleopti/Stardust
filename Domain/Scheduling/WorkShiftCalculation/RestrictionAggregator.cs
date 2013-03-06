using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation
{
    public interface IRestrictionAggregator
    {
		IEffectiveRestriction Aggregate(IList<DateOnly> dateOnlyList, IGroupPerson groupPerson, IList<IScheduleMatrixPro> matrixList, ISchedulingOptions schedulingOptions);
    }

    public class RestrictionAggregator : IRestrictionAggregator
    {
        private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
        private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
	    private readonly IOpenHoursToEffectiveRestrictionConverter _openHoursToRestrictionConverter;
	    private readonly IScheduleRestrictionExtractor _scheduleRestrictionExtractor;

	    public RestrictionAggregator(
            IEffectiveRestrictionCreator effectiveRestrictionCreator,
            ISchedulingResultStateHolder schedulingResultStateHolder,
			IOpenHoursToEffectiveRestrictionConverter openHoursToRestrictionConverter,
			IScheduleRestrictionExtractor scheduleRestrictionExtractor)
        {
            _effectiveRestrictionCreator = effectiveRestrictionCreator;
            _schedulingResultStateHolder = schedulingResultStateHolder;
		    _openHoursToRestrictionConverter = openHoursToRestrictionConverter;
		    _scheduleRestrictionExtractor = scheduleRestrictionExtractor;
        }

        public IEffectiveRestriction Aggregate(IList<DateOnly> dateOnlyList, IGroupPerson groupPerson,IList<IScheduleMatrixPro> matrixList, ISchedulingOptions schedulingOptions)
        {
            var scheduleDictionary = _schedulingResultStateHolder.Schedules;
            if (groupPerson == null)
                return null;
            IEffectiveRestriction effectiveRestriction = null;
	        if (dateOnlyList != null)
	        {
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
                
		        var openHoursRestriction = _openHoursToRestrictionConverter.Convert(groupPerson, dateOnlyList);
                
		        if (effectiveRestriction != null)
			        effectiveRestriction = effectiveRestriction.Combine(openHoursRestriction);

		        var restrictionFromSchedules = _scheduleRestrictionExtractor.Extract(dateOnlyList, matrixList,
		                                                                             schedulingOptions);
		        if (restrictionFromSchedules == null)
			        return null;
		        if (effectiveRestriction != null)
					effectiveRestriction = effectiveRestriction.Combine(restrictionFromSchedules);

		        return effectiveRestriction;
	        }
	        return null;
        }

    }
}