using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts
{
	public class RuleSetBagUsedSorter
	{
		public IList<IWorkShiftRuleSet> SortRuleSetsUsedFirst(IEnumerable<IWorkShiftRuleSet> ruleSets, IRuleSetBag ruleSetBag)
		{
			var sortedRuleSets = new List<IWorkShiftRuleSet>();
			var notUsedRuleSets = new List<IWorkShiftRuleSet>();
			foreach (var ruleSet in ruleSets.OrderBy(x => x.Description.ToString()))
			{
				if (ruleSetBag.RuleSetCollection.Contains(ruleSet))
					sortedRuleSets.Add(ruleSet);
				else
					notUsedRuleSets.Add(ruleSet);
			}

			sortedRuleSets.AddRange(notUsedRuleSets);
			return sortedRuleSets;
		}
	}
}
