using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
    public interface IRuleSetAccordingToAccessabilityFilter
    {
        IEnumerable<IWorkShiftRuleSet> FilterForRoleModel(IGroupPersonSkillAggregator groupPersonSkillAggregator, ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions, bool useShiftsForRestrictions);
        IEnumerable<IWorkShiftRuleSet> FilterForTeamMember(IPerson person, DateOnly dateOnly, ISchedulingOptions schedulingOptions, bool useShiftsForRestrictions);
    }

    public class RuleSetAccordingToAccessabilityFilter : IRuleSetAccordingToAccessabilityFilter
    {
        private readonly RuleSetBagExtractorProvider _ruleSetBagExtractorProvider;
        private readonly ITeamBlockIncludedWorkShiftRuleFilter _teamBlockIncludedWorkShiftRuleFilter;
	    private readonly IRuleSetSkillActivityChecker _ruleSetSkillActivityChecker;

	    public RuleSetAccordingToAccessabilityFilter(RuleSetBagExtractorProvider ruleSetBagExtractorProvider,
		    ITeamBlockIncludedWorkShiftRuleFilter teamBlockIncludedWorkShiftRuleFilter,
		    IRuleSetSkillActivityChecker ruleSetSkillActivityChecker)
	    {
			_ruleSetBagExtractorProvider = ruleSetBagExtractorProvider;
		    _teamBlockIncludedWorkShiftRuleFilter = teamBlockIncludedWorkShiftRuleFilter;
		    _ruleSetSkillActivityChecker = ruleSetSkillActivityChecker;
	    }

	    public IEnumerable<IWorkShiftRuleSet> FilterForRoleModel(IGroupPersonSkillAggregator groupPersonSkillAggregator, ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions, bool useShiftsForRestrictions)
        {
            IList<IRuleSetBag> extractedRuleSetBags = _ruleSetBagExtractorProvider.Fetch(schedulingOptions).GetRuleSetBag(teamBlockInfo).ToList();
	        var filteredList = _teamBlockIncludedWorkShiftRuleFilter.Filter(teamBlockInfo.BlockInfo.BlockPeriod,
	                                                                        extractedRuleSetBags);
			filteredList = filterForSkillActivity(filteredList, teamBlockInfo, groupPersonSkillAggregator);
		    if (!useShiftsForRestrictions)
			    filteredList = filterForShiftsForRestrictions(filteredList);

            return filteredList;
        }

	    private IEnumerable<IWorkShiftRuleSet> filterForShiftsForRestrictions(IEnumerable<IWorkShiftRuleSet> filteredList)
	    {
		    var result = new List<IWorkShiftRuleSet>();
		    foreach (var workShiftRuleSet in filteredList)
		    {
				if(!workShiftRuleSet.OnlyForRestrictions)
					result.Add(workShiftRuleSet);
		    }

		    return result;
	    }

	    public IEnumerable<IWorkShiftRuleSet> FilterForTeamMember(IPerson person, DateOnly explicitDateToCheck, ISchedulingOptions schedulingOptions, bool useShiftsForRestrictions)
        {
	        var extractedRuleSetBags = new[] { _ruleSetBagExtractorProvider.Fetch(schedulingOptions).GetRuleSetBagForTeamMember(person, explicitDateToCheck)};
			var filteredList = _teamBlockIncludedWorkShiftRuleFilter.Filter(new DateOnlyPeriod(explicitDateToCheck, explicitDateToCheck), extractedRuleSetBags);
			if (!useShiftsForRestrictions)
				filteredList = filterForShiftsForRestrictions(filteredList);

			return filteredList;
        }

	    private IEnumerable<IWorkShiftRuleSet> filterForSkillActivity(IEnumerable<IWorkShiftRuleSet> filteredList, ITeamBlockInfo teamBlockInfo, IGroupPersonSkillAggregator groupPersonSkillAggregator)
	    {
		    var retList = new List<IWorkShiftRuleSet>();
		    var aggregatedSkills = groupPersonSkillAggregator.AggregatedSkills(teamBlockInfo.TeamInfo.GroupMembers,
				teamBlockInfo.BlockInfo.BlockPeriod).ToList();
		    foreach (var workShiftRuleSet in filteredList)
		    {
			    if (_ruleSetSkillActivityChecker.CheckSkillActivities(workShiftRuleSet, aggregatedSkills))
					retList.Add(workShiftRuleSet);
		    }

		    return retList;
	    }
    }
}