using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public interface IRestrictionAggregator
    {
        IEffectiveRestriction Aggregate(ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions);

		IEffectiveRestriction AggregatePerDayPerPerson(DateOnly dateOnly, IPerson person, ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions, IShiftProjectionCache suggestedShiftProjectionCache, bool isTeamScheduling);
    }

    public class RestrictionAggregator : IRestrictionAggregator
    {
        private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
        private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
	    private readonly IScheduleRestrictionExtractor _scheduleRestrictionExtractor;
	    private readonly ISuggestedShiftRestrictionExtractor _suggestedShiftRestrictionExtractor;

	    public RestrictionAggregator(
            IEffectiveRestrictionCreator effectiveRestrictionCreator,
            ISchedulingResultStateHolder schedulingResultStateHolder,
			IScheduleRestrictionExtractor scheduleRestrictionExtractor,
			ISuggestedShiftRestrictionExtractor suggestedShiftRestrictionExtractor)
        {
            _effectiveRestrictionCreator = effectiveRestrictionCreator;
            _schedulingResultStateHolder = schedulingResultStateHolder;
		    _scheduleRestrictionExtractor = scheduleRestrictionExtractor;
		    _suggestedShiftRestrictionExtractor = suggestedShiftRestrictionExtractor;
        }

	    public IEffectiveRestriction Aggregate(ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions)
	    {
		    if (teamBlockInfo == null)
			    return null;
		    var groupPerson = teamBlockInfo.TeamInfo.GroupPerson;
		    var dateOnlyList = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection();
		    var matrixList = teamBlockInfo.TeamInfo.MatrixesForGroup().ToList();
		    var scheduleDictionary = _schedulingResultStateHolder.Schedules;

		    if (dateOnlyList == null) return null;
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

		    var timeZone = TeleoptiPrincipal.Current.Regional.TimeZone;
		    var restrictionFromSchedules = _scheduleRestrictionExtractor.Extract(dateOnlyList, matrixList,
		                                                                         schedulingOptions, timeZone,teamBlockInfo );
		    if (restrictionFromSchedules == null)
			    return null;
		    if (effectiveRestriction != null)
			    effectiveRestriction = effectiveRestriction.Combine(restrictionFromSchedules);

		    return effectiveRestriction;
	    }

	    public IEffectiveRestriction AggregatePerDayPerPerson(DateOnly dateOnly, IPerson person,
	                                                          ITeamBlockInfo teamBlockInfo,
	                                                          ISchedulingOptions schedulingOptions,
	                                                          IShiftProjectionCache suggestedShiftProjectionCache,
	                                                          bool isTeamScheduling)
	    {
	        //TODO: removing this team scheduling in the next task operation cleanup
            var dateOnlyList = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection();
		    var scheduleDictionary = _schedulingResultStateHolder.Schedules;
		    var restriction = _effectiveRestrictionCreator.GetEffectiveRestriction(new List<IPerson> {person},
		                                                                           dateOnly, schedulingOptions,
		                                                                           scheduleDictionary);
		    if (restriction == null)
			    return null;

		    var timeZone = TeleoptiPrincipal.Current.Regional.TimeZone;
		    var matrixes = teamBlockInfo.TeamInfo.MatrixesForMemberAndPeriod(person, teamBlockInfo.BlockInfo.BlockPeriod);
            if (schedulingOptions.UseGroupScheduling)
		    {
			    var teamMatrixList = teamBlockInfo.TeamInfo.MatrixesForGroup().ToList();
			    var restrictionFromOneTeam = _scheduleRestrictionExtractor.ExtractForOneTeamOneDay(dateOnly, teamMatrixList,
			                                                                                       schedulingOptions, timeZone);
			    restriction = restriction.Combine(restrictionFromOneTeam);
			    if (restriction == null)
				    return null;
		    }

		    var restrictionFromOneBlock = _scheduleRestrictionExtractor.ExtractForOnePersonOneBlock(dateOnlyList,
		                                                                                            matrixes.ToList(),
		                                                                                            schedulingOptions,
		                                                                                            timeZone);
		    restriction = restriction.Combine(restrictionFromOneBlock);
		    if (restriction == null)
			    return null;

		    if (suggestedShiftProjectionCache != null)
		    {

                if (schedulingOptions.UseGroupScheduling)
			    {
				    var suggestedShiftRestrictionForOneTeam =
					    _suggestedShiftRestrictionExtractor.ExtractForOneTeam(suggestedShiftProjectionCache, schedulingOptions);

				    restriction = restriction.Combine(suggestedShiftRestrictionForOneTeam);
				    if (restriction == null)
					    return null;
			    }

			    var suggestedShiftRestrictionForOneBlock =
				    _suggestedShiftRestrictionExtractor.ExtractForOneBlock(suggestedShiftProjectionCache, schedulingOptions);
			    restriction = restriction.Combine(suggestedShiftRestrictionForOneBlock);

		    }
		    return restriction;
	    }
    }
}