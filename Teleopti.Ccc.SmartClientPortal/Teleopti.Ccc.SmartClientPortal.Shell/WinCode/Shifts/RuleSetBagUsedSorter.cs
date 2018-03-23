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

		public IList<IRuleSetBag> SortRuleSetBagsUsedFirst(IEnumerable<IRuleSetBag> ruleSetBags, IWorkShiftRuleSet ruleSet)
		{
			var sortedRuleSetBags = new List<IRuleSetBag>();
			var notUsedRuleSetBags = new List<IRuleSetBag>();
			foreach (var ruleSetBag in ruleSetBags.OrderBy(x => x.Description.ToString()))
			{
				if (ruleSet.RuleSetBagCollection.Contains(ruleSetBag))
					sortedRuleSetBags.Add(ruleSetBag);
				else
					notUsedRuleSetBags.Add(ruleSetBag);	
			}

			sortedRuleSetBags.AddRange(notUsedRuleSetBags);
			return sortedRuleSetBags;
		}
	}
}
