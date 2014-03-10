using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
    public interface ITeamBlockWorkShiftRuleFilter
    {
        IEnumerable<IWorkShiftRuleSet> Filter(DateOnlyPeriod blockPeriod, IList<IRuleSetBag> ruleSetBags);
    }

    public class TeamBlockWorkShiftRuleFilter : ITeamBlockWorkShiftRuleFilter
    {
        public IEnumerable<IWorkShiftRuleSet> Filter(DateOnlyPeriod blockPeriod, IList<IRuleSetBag> ruleSetBags)
        {
            var filteredList = new List<IWorkShiftRuleSet>();
            foreach (var ruleSet in ruleSetBags)
            {
                foreach (var workShiftRuleSet in ruleSet.RuleSetCollection)
                {
                    var isIncluded = true;
                    if (workShiftRuleSet.DefaultAccessibility == DefaultAccessibility.Excluded ) isIncluded = false;
                    var accessibilityDaysOfWeek = workShiftRuleSet.AccessibilityDaysOfWeek.ToList();
                    var accessibilityDates = workShiftRuleSet.AccessibilityDates.ToList();
                    var isBlockCleared = true;
                    foreach (var dateOnly in blockPeriod.DayCollection())
                    {
                        if (isIncluded)
                        {
                            if ((accessibilityDaysOfWeek.Count >0 && accessibilityDaysOfWeek.Contains(dateOnly.DayOfWeek)) ||
                                (accessibilityDates.Count >0 && accessibilityDates.Contains(dateOnly)))
                            {
                                isBlockCleared = false;
                                break;
                            }
                        }
                        else
                        {
                            if ((accessibilityDaysOfWeek.Count >0 && !accessibilityDaysOfWeek.Contains(dateOnly.DayOfWeek)) ||
                            (accessibilityDates.Count > 0 && !accessibilityDates.Contains(dateOnly)))
                            {
                                isBlockCleared = false;
                                break;
                            }
                        }
                        

                    }
                    if (isBlockCleared)
                    {
                        if (filteredList.Contains(workShiftRuleSet)) continue;
                        filteredList.Add(workShiftRuleSet);
                    }

                }
            }
            return filteredList;
        }
    }
}