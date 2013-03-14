using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public interface IRestrictionAggregator
    {
        IEffectiveRestriction Aggregate(ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions);

        IEffectiveRestriction AggregatePerDay(ITeamInfo teamInfo, ISchedulingOptions schedulingOptions,IBlockInfo blockInfo );
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

        public IEffectiveRestriction Aggregate(ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions)
        {
			if (teamBlockInfo == null)
				return null;
            var groupPerson = teamBlockInfo.TeamInfo.GroupPerson;
            var dateOnlyList = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection();
	        var matrixList = teamBlockInfo.MatrixesForGroupAndBlock().ToList();
            var scheduleDictionary = _schedulingResultStateHolder.Schedules;
			
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

		        if (effectiveRestriction != null && openHoursRestriction != null)
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

        public IEffectiveRestriction AggregatePerDay(ITeamInfo teamInfo, ISchedulingOptions schedulingOptions, IBlockInfo blockInfo   )
        {
            if (teamInfo == null)
                return null;
            var groupPerson = teamInfo.GroupPerson;
            var dateOnlyList = blockInfo.BlockPeriod.DayCollection();
            var matrixList = teamInfo.MatrixesForGroup().ToList();
            var scheduleDictionary = _schedulingResultStateHolder.Schedules;

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

                if (effectiveRestriction != null && openHoursRestriction != null)
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