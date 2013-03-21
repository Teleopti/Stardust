using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public interface IRestrictionAggregator
    {
        IEffectiveRestriction Aggregate(ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions);

        IEffectiveRestriction AggregatePerDay(ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions, IShiftProjectionCache suggestedShiftProjectionCache);
    }

    public class RestrictionAggregator : IRestrictionAggregator
    {
        private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
        private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
	    private readonly IOpenHoursToEffectiveRestrictionConverter _openHoursToRestrictionConverter;
	    private readonly IScheduleRestrictionExtractor _scheduleRestrictionExtractor;
	    private readonly ISuggestedShiftRestrictionExtractor _suggestedShiftRestrictionExtractor;

	    public RestrictionAggregator(
            IEffectiveRestrictionCreator effectiveRestrictionCreator,
            ISchedulingResultStateHolder schedulingResultStateHolder,
			IOpenHoursToEffectiveRestrictionConverter openHoursToRestrictionConverter,
			IScheduleRestrictionExtractor scheduleRestrictionExtractor,
			ISuggestedShiftRestrictionExtractor suggestedShiftRestrictionExtractor)
        {
            _effectiveRestrictionCreator = effectiveRestrictionCreator;
            _schedulingResultStateHolder = schedulingResultStateHolder;
		    _openHoursToRestrictionConverter = openHoursToRestrictionConverter;
		    _scheduleRestrictionExtractor = scheduleRestrictionExtractor;
		    _suggestedShiftRestrictionExtractor = suggestedShiftRestrictionExtractor;
        }

        public IEffectiveRestriction Aggregate(ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions)
        {
            return AggregatePerDay(teamBlockInfo, schedulingOptions, null);
        }

        public IEffectiveRestriction AggregatePerDay(ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions, IShiftProjectionCache suggestedShiftProjectionCache)
        {
            if (teamBlockInfo == null)
                return null;
            var groupPerson = teamBlockInfo.TeamInfo .GroupPerson;
            var dateOnlyList =teamBlockInfo.BlockInfo .BlockPeriod.DayCollection();
            var matrixList = teamBlockInfo.TeamInfo .MatrixesForGroup().ToList();
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
                if (suggestedShiftProjectionCache != null)
                {
                    var suggestedShiftRestriction = _suggestedShiftRestrictionExtractor.Extract(suggestedShiftProjectionCache,
                                                                                            schedulingOptions);
                    if (suggestedShiftRestriction == null)
                        return null;
                    if (effectiveRestriction != null)
                        effectiveRestriction = effectiveRestriction.Combine(suggestedShiftRestriction);
                }
                
                return effectiveRestriction;
            }
            return null;
        }

    }
}