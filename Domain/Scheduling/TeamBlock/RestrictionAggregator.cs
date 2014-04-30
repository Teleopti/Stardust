using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public interface IRestrictionAggregator
    {
        IEffectiveRestriction Aggregate(ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions);

		IEffectiveRestriction AggregatePerDayPerPerson(DateOnly dateOnly, IPerson person, ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions, IShiftProjectionCache suggestedShiftProjectionCache);
    }

    public class RestrictionAggregator : IRestrictionAggregator
    {
        private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
        private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
	    private readonly IScheduleRestrictionExtractor _scheduleRestrictionExtractor;
	    private readonly ISuggestedShiftRestrictionExtractor _suggestedShiftRestrictionExtractor;
	    private readonly IScheduleDayEquator _scheduleDayEquator;
	    private readonly IAssignmentPeriodRule _nightlyRestRule;

	    public RestrictionAggregator(
            IEffectiveRestrictionCreator effectiveRestrictionCreator,
            ISchedulingResultStateHolder schedulingResultStateHolder,
			IScheduleRestrictionExtractor scheduleRestrictionExtractor,
			ISuggestedShiftRestrictionExtractor suggestedShiftRestrictionExtractor,
			IScheduleDayEquator scheduleDayEquator,
			IAssignmentPeriodRule nightlyRestRule)
        {
            _effectiveRestrictionCreator = effectiveRestrictionCreator;
            _schedulingResultStateHolder = schedulingResultStateHolder;
		    _scheduleRestrictionExtractor = scheduleRestrictionExtractor;
		    _suggestedShiftRestrictionExtractor = suggestedShiftRestrictionExtractor;
		    _scheduleDayEquator = scheduleDayEquator;
		    _nightlyRestRule = nightlyRestRule;
        }

	    public IEffectiveRestriction Aggregate(ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions)
	    {
		    if (teamBlockInfo == null)
			    return null;
		    var dateOnlyList = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection();
		    if (dateOnlyList == null) return null;

		    var groupMembers = teamBlockInfo.TeamInfo.GroupMembers;
		    var matrixList = teamBlockInfo.TeamInfo.MatrixesForGroup().ToList();
		    var scheduleDictionary = _schedulingResultStateHolder.Schedules;
		    var timeZone = TeleoptiPrincipal.Current.Regional.TimeZone;

		    IEffectiveRestriction effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
		                                                                          new EndTimeLimitation(),
		                                                                          new WorkTimeLimitation(), null, null, null,
		                                                                          new List<IActivityRestriction>());

		    effectiveRestriction = combineRestriction(new TeamBlockEffectiveRestrcition(_effectiveRestrictionCreator, groupMembers, schedulingOptions,
				                                      scheduleDictionary), dateOnlyList, matrixList, effectiveRestriction);

		    effectiveRestriction = combineRestriction(new SameStartTimeRestriction(timeZone), dateOnlyList,
		                                              matrixList, effectiveRestriction);

		    effectiveRestriction = combineRestriction(new SameEndTimeRestriction(timeZone), dateOnlyList, matrixList,
		                                              effectiveRestriction);

		    effectiveRestriction = combineRestriction(new SameShiftRestriction(_scheduleDayEquator), dateOnlyList, matrixList,
		                                              effectiveRestriction);

		    effectiveRestriction = combineRestriction(new SameShiftCategoryRestriction(), dateOnlyList, matrixList,
		                                              effectiveRestriction);

		    effectiveRestriction = combineRestriction(new NightlyRestRestrcition(_nightlyRestRule), dateOnlyList, matrixList,
		                                              effectiveRestriction);

		    return effectiveRestriction;
	    }

	    public IEffectiveRestriction AggregatePerDayPerPerson(DateOnly dateOnly, IPerson person,
	                                                          ITeamBlockInfo teamBlockInfo,
	                                                          ISchedulingOptions schedulingOptions,
	                                                          IShiftProjectionCache suggestedShiftProjectionCache)
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
            if (schedulingOptions.UseTeam)
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

                if (schedulingOptions.UseTeam)
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

	    private static IEffectiveRestriction combineRestriction(IScheduleRestrictionStrategy strategy,
	                                                            IList<DateOnly> dateOnlyList,
	                                                            IList<IScheduleMatrixPro> matrixList,
	                                                            IEffectiveRestriction effectiveRestriction)
	    {
		    var restriction = strategy.ExtractRestriction(dateOnlyList, matrixList);
		    return effectiveRestriction.Combine(restriction);
	    }
    }
}