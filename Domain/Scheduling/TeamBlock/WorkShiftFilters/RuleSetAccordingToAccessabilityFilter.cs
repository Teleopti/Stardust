using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
    public interface IRuleSetAccordingToAccessabilityFilter
    {
        IEnumerable<IWorkShiftRuleSet> Filter(ITeamBlockInfo teamBlockInfo);
        IEnumerable<IWorkShiftRuleSet> Filter(ITeamBlockInfo teamBlockInfo, DateOnly dateOnly);
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

	    public IEnumerable<IWorkShiftRuleSet> Filter(ITeamBlockInfo teamBlockInfo)
        {
            IList<IRuleSetBag> extractedRuleSetBags = _teamBlockRuleSetBagExtractor.GetRuleSetBag(teamBlockInfo).ToList();
	        var filteredList = _teamBlockIncludedWorkShiftRuleFilter.Filter(teamBlockInfo.BlockInfo.BlockPeriod,
	                                                                        extractedRuleSetBags);
			filteredList = filterForSkillActivity(filteredList, teamBlockInfo);
            return filteredList;
        }

        public IEnumerable<IWorkShiftRuleSet> Filter(ITeamBlockInfo teamBlockInfo, DateOnly explicitDateToCheck)
        {
	        var extractedRuleSetBags = _teamBlockRuleSetBagExtractor.GetRuleSetBag(teamBlockInfo, explicitDateToCheck);
			var filteredList = _teamBlockIncludedWorkShiftRuleFilter.Filter(new DateOnlyPeriod(explicitDateToCheck, explicitDateToCheck),
	                                                                        extractedRuleSetBags.ToList());
	        filteredList = filterForSkillActivity(filteredList, teamBlockInfo);
            return filteredList;
        }

	    private IEnumerable<IWorkShiftRuleSet> filterForSkillActivity(IEnumerable<IWorkShiftRuleSet> filteredList, ITeamBlockInfo teamBlockInfo)
	    {
		    var retList = new List<IWorkShiftRuleSet>();
		    var aggregatedSkills = _skillAggregator.AggregatedSkills(teamBlockInfo.TeamInfo.GroupPerson,
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