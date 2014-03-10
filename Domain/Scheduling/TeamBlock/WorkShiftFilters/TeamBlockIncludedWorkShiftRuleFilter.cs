using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
    public interface ITeamBlockIncludedWorkShiftRuleFilter
    {
        IEnumerable<IWorkShiftRuleSet> Filter(DateOnlyPeriod periodToAggregate, IList<IRuleSetBag> ruleSetBags);
    }

    public class TeamBlockIncludedWorkShiftRuleFilter : ITeamBlockIncludedWorkShiftRuleFilter
    {
        public IEnumerable<IWorkShiftRuleSet> Filter(DateOnlyPeriod periodToAggregate, IList<IRuleSetBag> ruleSetBags)
        {
            var filteredList = new List<IWorkShiftRuleSet>();
            foreach (var ruleSet in ruleSetBags)
            {
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