using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
    public class TeamBlockIncludedWorkShiftRuleFilter
    {
        public IEnumerable<IWorkShiftRuleSet> Filter(DateOnlyPeriod periodToAggregate, IList<IRuleSetBag> ruleSetBags)
        {
            var filteredList = new List<IWorkShiftRuleSet>();
            foreach (var ruleSet in ruleSetBags)
            {
				if(ruleSet == null) continue;
	            foreach (var workShiftRuleSet in ruleSet.RuleSetCollection)
	            {
					var ruleSetValidForAllDates = true;
		            foreach (var dateOnly in periodToAggregate.DayCollection())
		            {
			            if (!workShiftRuleSet.IsValidDate(dateOnly))
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