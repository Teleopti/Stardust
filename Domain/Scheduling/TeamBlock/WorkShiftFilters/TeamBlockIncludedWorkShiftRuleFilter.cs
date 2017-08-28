using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
    public class TeamBlockIncludedWorkShiftRuleFilter
    {
        public IEnumerable<IWorkShiftRuleSet> Filter(DateOnlyPeriod periodToAggregate, IList<IRuleSetBag> ruleSetBags, bool optionsIsSameShift)
        {
            var filteredList = new List<IWorkShiftRuleSet>();
            foreach (var bag in ruleSetBags)
            {
				if(bag == null) continue;
	            foreach (var workShiftRuleSet in bag.RuleSetCollection)
	            {
					var ruleSetValidForAllDates = true;
		            foreach (var dateOnly in periodToAggregate.DayCollection())
		            {
			            if (optionsIsSameShift && !workShiftRuleSet.IsValidDate(dateOnly))
			            {
				            ruleSetValidForAllDates = false;
							break;
			            }
		            }
					if(ruleSetValidForAllDates)
						filteredList.Add(workShiftRuleSet);
	            }
            }

            return filteredList;
        }
    }
}