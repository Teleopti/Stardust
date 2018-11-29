using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
    public interface IRuleSetBagExtractor
    {
        IEnumerable<IRuleSetBag> GetRuleSetBag(ITeamBlockInfo teamBlockInfo);
        IRuleSetBag GetRuleSetBagForTeamMember(IPerson person, DateOnly dateOnly);
    }

    public class RuleSetBagExtractor : IRuleSetBagExtractor
    {
        public IEnumerable<IRuleSetBag> GetRuleSetBag(ITeamBlockInfo teamBlockInfo)
        {
            var teamInfo = teamBlockInfo.TeamInfo;
	        var possibleWorkShiftRuleSetBagList =
		        teamInfo.GroupMembers.SelectMany(p => p.PersonPeriods(teamBlockInfo.BlockInfo.BlockPeriod))
			        .Select(p => p.RuleSetBag)
			        .Where(p => p != null)
			        .Distinct();
			
            return possibleWorkShiftRuleSetBagList;
        }

	    public IRuleSetBag GetRuleSetBagForTeamMember(IPerson person, DateOnly dateOnly)
	    {
		    return person.Period(dateOnly)?.RuleSetBag;
	    }
    }
}