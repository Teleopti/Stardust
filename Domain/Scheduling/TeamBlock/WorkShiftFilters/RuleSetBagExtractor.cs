﻿using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

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
            var possibleWorkShiftRuleSetBagList = new List<IRuleSetBag>();
	        var teamInfo = teamBlockInfo.TeamInfo;

			foreach (var person in teamInfo.GroupMembers)
            {
                foreach (var dateOnly in teamBlockInfo.BlockInfo.BlockPeriod.DayCollection())
                {
					//if(!teamInfo.UnLockedMembers(dateOnly).Contains(person))
					//	continue;

					if(person.Period(dateOnly) == null) continue;
	                
                    var tempWorkShiftRuleSetBag = person.Period(dateOnly).RuleSetBag;
                    if (tempWorkShiftRuleSetBag == null) 
						continue;

                    if (possibleWorkShiftRuleSetBagList.Contains(tempWorkShiftRuleSetBag)) continue;
                    possibleWorkShiftRuleSetBagList.Add(tempWorkShiftRuleSetBag);
                }
            }

            return possibleWorkShiftRuleSetBagList;
        }

	    public IRuleSetBag GetRuleSetBagForTeamMember(IPerson person, DateOnly dateOnly)
	    {
		    return person.Period(dateOnly)?.RuleSetBag;
	    }
    }
}