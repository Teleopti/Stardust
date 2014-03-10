using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
    public interface ITeamBlockRuleSetBagExtractor
    {
        IEnumerable<IRuleSetBag> GetRuleSetBag(ITeamBlockInfo teamBlockInfo);
        IEnumerable<IRuleSetBag> GetRuleSetBag(ITeamBlockInfo teamBlockInfo, DateOnly dateOnly);
    }

    public class TeamBlockRuleSetBagExtractor : ITeamBlockRuleSetBagExtractor
    {
        public IEnumerable<IRuleSetBag> GetRuleSetBag(ITeamBlockInfo teamBlockInfo)
        {
            var possibleWorkShiftRuleSetBagList = new List<IRuleSetBag>();
            foreach (var person in teamBlockInfo.TeamInfo.GroupMembers )
            {
                foreach (var dateOnly in teamBlockInfo.BlockInfo.BlockPeriod.DayCollection())
                {
                    var tempWorkShiftRuleSetBag = person.Period(dateOnly).RuleSetBag;
                    if (tempWorkShiftRuleSetBag == null) continue;
                    if (possibleWorkShiftRuleSetBagList.Contains(tempWorkShiftRuleSetBag)) continue;
                    possibleWorkShiftRuleSetBagList.Add(tempWorkShiftRuleSetBag);
                }
            }
            return possibleWorkShiftRuleSetBagList;
        }

        public IEnumerable<IRuleSetBag> GetRuleSetBag(ITeamBlockInfo teamBlockInfo, DateOnly dateOnly)
        {
            var possibleWorkShiftRuleSetBagList = new List<IRuleSetBag>();
            foreach (var person in teamBlockInfo.TeamInfo.GroupMembers)
            {
                var tempWorkShiftRuleSetBag = person.Period(dateOnly).RuleSetBag;
                if (tempWorkShiftRuleSetBag == null) continue;
                if (possibleWorkShiftRuleSetBagList.Contains(tempWorkShiftRuleSetBag)) continue;
                possibleWorkShiftRuleSetBagList.Add(tempWorkShiftRuleSetBag);
            }
            return possibleWorkShiftRuleSetBagList;
        }
    }
}