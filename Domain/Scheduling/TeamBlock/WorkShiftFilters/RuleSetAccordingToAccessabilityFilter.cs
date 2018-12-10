using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public class RuleSetAccordingToAccessabilityFilter
    {
        private readonly RuleSetBagExtractorProvider _ruleSetBagExtractorProvider;
        private readonly TeamBlockIncludedWorkShiftRuleFilter _teamBlockIncludedWorkShiftRuleFilter;
	    private readonly IRuleSetSkillActivityChecker _ruleSetSkillActivityChecker;

	    public RuleSetAccordingToAccessabilityFilter(RuleSetBagExtractorProvider ruleSetBagExtractorProvider,
		    TeamBlockIncludedWorkShiftRuleFilter teamBlockIncludedWorkShiftRuleFilter,
		    IRuleSetSkillActivityChecker ruleSetSkillActivityChecker)
	    {
			_ruleSetBagExtractorProvider = ruleSetBagExtractorProvider;
		    _teamBlockIncludedWorkShiftRuleFilter = teamBlockIncludedWorkShiftRuleFilter;
		    _ruleSetSkillActivityChecker = ruleSetSkillActivityChecker;
	    }

	    public IEnumerable<IWorkShiftRuleSet> FilterForRoleModel(IGroupPersonSkillAggregator groupPersonSkillAggregator, ITeamBlockInfo teamBlockInfo, SchedulingOptions schedulingOptions, bool useShiftsForRestrictions)
        {
            IList<IRuleSetBag> extractedRuleSetBags = _ruleSetBagExtractorProvider.Fetch(schedulingOptions).GetRuleSetBag(teamBlockInfo).ToList();
	        var filteredList = _teamBlockIncludedWorkShiftRuleFilter.Filter(teamBlockInfo.BlockInfo.BlockPeriod,
	                                                                        extractedRuleSetBags);
			filteredList = filterForSkillActivity(filteredList, teamBlockInfo, groupPersonSkillAggregator);
		    filteredList = filterForShiftsForRestrictions(filteredList, useShiftsForRestrictions);

            return filteredList;
        }

	    private static IEnumerable<IWorkShiftRuleSet> filterForShiftsForRestrictions(IEnumerable<IWorkShiftRuleSet> filteredList, bool useShiftsForRestrictions)
	    {
			return filteredList.Where(x => x.OnlyForRestrictions == useShiftsForRestrictions);
		}

	    public IEnumerable<IWorkShiftRuleSet> FilterForTeamMember(IPerson person, DateOnly explicitDateToCheck, SchedulingOptions schedulingOptions, bool useShiftsForRestrictions)
        {
	        var extractedRuleSetBags = new[] { _ruleSetBagExtractorProvider.Fetch(schedulingOptions).GetRuleSetBagForTeamMember(person, explicitDateToCheck)};
			var filteredList = _teamBlockIncludedWorkShiftRuleFilter.Filter(explicitDateToCheck.ToDateOnlyPeriod(), extractedRuleSetBags);
			filteredList = filterForShiftsForRestrictions(filteredList, useShiftsForRestrictions);

			return filteredList;
        }

	    private IEnumerable<IWorkShiftRuleSet> filterForSkillActivity(IEnumerable<IWorkShiftRuleSet> filteredList, ITeamBlockInfo teamBlockInfo, IGroupPersonSkillAggregator groupPersonSkillAggregator)
	    {
		    var aggregatedSkills = groupPersonSkillAggregator.AggregatedSkills(teamBlockInfo.TeamInfo.GroupMembers,
				teamBlockInfo.BlockInfo.BlockPeriod).ToList();
		    return filteredList.Where(w => _ruleSetSkillActivityChecker.CheckSkillActivities(w, aggregatedSkills));
	    }
    }
}