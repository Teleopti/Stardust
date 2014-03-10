using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
    public interface IRuleSetAccordingToAccessabilityFilter
    {
        IEnumerable<IWorkShiftRuleSet> Filter(ITeamBlockInfo teamBlockInfo);
    }

    public class RuleSetAccordingToAccessabilityFilter : IRuleSetAccordingToAccessabilityFilter
    {
        private readonly ITeamBlockRuleSetBagExtractor _teamBlockRuleSetBagExtractor;
        private readonly ITeamBlockWorkShiftRuleFilter _teamBlockWorkShiftRuleFilter;

        public RuleSetAccordingToAccessabilityFilter(ITeamBlockRuleSetBagExtractor teamBlockRuleSetBagExtractor, ITeamBlockWorkShiftRuleFilter teamBlockWorkShiftRuleFilter)
        {
            _teamBlockRuleSetBagExtractor = teamBlockRuleSetBagExtractor;
            _teamBlockWorkShiftRuleFilter = teamBlockWorkShiftRuleFilter;
        }

        public IEnumerable<IWorkShiftRuleSet> Filter(ITeamBlockInfo teamBlockInfo)
        {
            IList<IRuleSetBag> extractedRuleSetBags = _teamBlockRuleSetBagExtractor.GetRuleSetBag(teamBlockInfo).ToList();
            var filteredList = _teamBlockWorkShiftRuleFilter.Filter(teamBlockInfo.BlockInfo.BlockPeriod, extractedRuleSetBags);
            return filteredList;
        }
    }
}