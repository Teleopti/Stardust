using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
    public interface IRuleSetAccordingToAccessabilityFilter
    {
        IEnumerable<IWorkShiftRuleSet> FilterForRoleModel(ITeamBlockInfo teamBlockInfo, bool useShiftsForRestrictions);
        IEnumerable<IWorkShiftRuleSet> FilterForTeamMember(IPerson person, DateOnly dateOnly, bool useShiftsForRestrictions);
    }

    public class RuleSetAccordingToAccessabilityFilter : IRuleSetAccordingToAccessabilityFilter
    {
        private readonly ITeamBlockRuleSetBagExtractor _teamBlockRuleSetBagExtractor;
        private readonly ITeamBlockIncludedWorkShiftRuleFilter _teamBlockIncludedWorkShiftRuleFilter;
	    private readonly IRuleSetSkillActivityChecker _ruleSetSkillActivityChecker;
	    private readonly IGroupPersonSkillAggregator _skillAggregator;

	    public RuleSetAccordingToAccessabilityFilter(ITeamBlockRuleSetBagExtractor teamBlockRuleSetBagExtractor,
		    ITeamBlockIncludedWorkShiftRuleFilter teamBlockIncludedWorkShiftRuleFilter,
		    IRuleSetSkillActivityChecker ruleSetSkillActivityChecker, IGroupPersonSkillAggregator skillAggregator)
	    {
		    _teamBlockRuleSetBagExtractor = teamBlockRuleSetBagExtractor;
		    _teamBlockIncludedWorkShiftRuleFilter = teamBlockIncludedWorkShiftRuleFilter;
		    _ruleSetSkillActivityChecker = ruleSetSkillActivityChecker;
		    _skillAggregator = skillAggregator;
	    }

	    public IEnumerable<IWorkShiftRuleSet> FilterForRoleModel(ITeamBlockInfo teamBlockInfo, bool useShiftsForRestrictions)
        {
            IList<IRuleSetBag> extractedRuleSetBags = _teamBlockRuleSetBagExtractor.GetRuleSetBag(teamBlockInfo).ToList();
	        var filteredList = _teamBlockIncludedWorkShiftRuleFilter.Filter(teamBlockInfo.BlockInfo.BlockPeriod,
	                                                                        extractedRuleSetBags);
			filteredList = filterForSkillActivity(filteredList, teamBlockInfo);
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

	    public IEnumerable<IWorkShiftRuleSet> FilterForTeamMember(IPerson person, DateOnly explicitDateToCheck, bool useShiftsForRestrictions)
        {
	        var extractedRuleSetBags = _teamBlockRuleSetBagExtractor.GetRuleSetBagForTeamMember(person, explicitDateToCheck);
			var filteredList = _teamBlockIncludedWorkShiftRuleFilter.Filter(new DateOnlyPeriod(explicitDateToCheck, explicitDateToCheck),
	                                                                        extractedRuleSetBags.ToList());
			if (!useShiftsForRestrictions)
				filteredList = filterForShiftsForRestrictions(filteredList);

			return filteredList;
        }

	    private IEnumerable<IWorkShiftRuleSet> filterForSkillActivity(IEnumerable<IWorkShiftRuleSet> filteredList, ITeamBlockInfo teamBlockInfo)
	    {
		    var retList = new List<IWorkShiftRuleSet>();
		    var aggregatedSkills = _skillAggregator.AggregatedSkills(teamBlockInfo.TeamInfo.GroupMembers,
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