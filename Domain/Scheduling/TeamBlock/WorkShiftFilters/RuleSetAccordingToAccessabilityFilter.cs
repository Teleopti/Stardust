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

        public RuleSetAccordingToAccessabilityFilter(ITeamBlockRuleSetBagExtractor teamBlockRuleSetBagExtractor, ITeamBlockIncludedWorkShiftRuleFilter teamBlockIncludedWorkShiftRuleFilter)
        {
            _teamBlockRuleSetBagExtractor = teamBlockRuleSetBagExtractor;
            _teamBlockIncludedWorkShiftRuleFilter = teamBlockIncludedWorkShiftRuleFilter;
        }

        public IEnumerable<IWorkShiftRuleSet> Filter(ITeamBlockInfo teamBlockInfo)
        {
            IList<IRuleSetBag> extractedRuleSetBags = _teamBlockRuleSetBagExtractor.GetRuleSetBag(teamBlockInfo).ToList();
	        var filteredList = _teamBlockIncludedWorkShiftRuleFilter.Filter(teamBlockInfo.BlockInfo.BlockPeriod,
	                                                                        extractedRuleSetBags);
            return filteredList;
        }

        public IEnumerable<IWorkShiftRuleSet> Filter(ITeamBlockInfo teamBlockInfo, DateOnly explicitDateToCheck)
        {
	        var extractedRuleSetBags = _teamBlockRuleSetBagExtractor.GetRuleSetBag(teamBlockInfo, explicitDateToCheck);
			var filteredList = _teamBlockIncludedWorkShiftRuleFilter.Filter(new DateOnlyPeriod(explicitDateToCheck, explicitDateToCheck),
	                                                                        extractedRuleSetBags.ToList());
            return filteredList;
        }
    }
}