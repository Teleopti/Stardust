using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public interface IShiftProjectionCachesFromAdjustedRuleSetBagShiftFilter
	{
		IList<IShiftProjectionCache> Filter(DateOnly scheduleDateOnly, IPerson person, bool forRestrictionsOnly);
	}

	public class ShiftProjectionCachesFromAdjustedRuleSetBagShiftFilter : IShiftProjectionCachesFromAdjustedRuleSetBagShiftFilter
	{
		private readonly IRuleSetDeletedActivityChecker _ruleSetDeletedActivityChecker;
		private readonly IRuleSetDeletedShiftCategoryChecker _rulesSetDeletedShiftCategoryChecker;
		private readonly IRuleSetToShiftsGenerator _ruleSetToShiftsGenerator;
		private readonly IDictionary<IWorkShiftRuleSet, IList<IShiftProjectionCache>> _ruleSetListDictionary = new Dictionary<IWorkShiftRuleSet, IList<IShiftProjectionCache>>();

		public ShiftProjectionCachesFromAdjustedRuleSetBagShiftFilter(IRuleSetDeletedActivityChecker ruleSetDeletedActivityChecker,
			IRuleSetDeletedShiftCategoryChecker rulesSetDeletedShiftCategoryChecker,
			IRuleSetToShiftsGenerator ruleSetToShiftsGenerator)
		{
			_ruleSetDeletedActivityChecker = ruleSetDeletedActivityChecker;
			_rulesSetDeletedShiftCategoryChecker = rulesSetDeletedShiftCategoryChecker;
			_ruleSetToShiftsGenerator = ruleSetToShiftsGenerator;
		}

		public IList<IShiftProjectionCache> Filter(DateOnly scheduleDateOnly, IPerson person,  bool forRestrictionsOnly)
		{
			if (person == null)
				return null;
			var shiftProjectionCaches = new List<IShiftProjectionCache>();
			var timeZone = person.PermissionInformation.DefaultTimeZone();
			var personPeriod = person.Period(scheduleDateOnly);
			var bag = personPeriod.RuleSetBag;

			var ruleSets = bag.RuleSetCollection.Where(workShiftRuleSet => workShiftRuleSet.OnlyForRestrictions == forRestrictionsOnly).ToList();

			foreach (IWorkShiftRuleSet ruleSet in ruleSets)
			{
				if (!ruleSet.IsValidDate(scheduleDateOnly))
					continue;

				if (_ruleSetDeletedActivityChecker.ContainsDeletedActivity(ruleSet))
					continue;

				if (_rulesSetDeletedShiftCategoryChecker.ContainsDeletedActivity(ruleSet))
					continue;

				IEnumerable<IShiftProjectionCache> ruleSetList = getShiftsForRuleset(ruleSet);
				if (ruleSetList != null)
				{
					foreach (var projectionCache in ruleSetList)
					{
						shiftProjectionCaches.Add(projectionCache);
						projectionCache.SetDate(scheduleDateOnly, timeZone);
					}
				}
			}

			return shiftProjectionCaches;
		}

		private IEnumerable<IShiftProjectionCache> getShiftsForRuleset(IWorkShiftRuleSet ruleSet)
		{
			IList<IShiftProjectionCache> retList;

			if (!_ruleSetListDictionary.TryGetValue(ruleSet, out retList))
			{
				retList = _ruleSetToShiftsGenerator.Generate(ruleSet).ToList();
				_ruleSetListDictionary.Add(ruleSet, retList);
			}
			return retList;
		}
	}
}
