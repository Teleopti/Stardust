using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class ShiftProjectionCacheManager : IShiftProjectionCacheManager, IDisposable
    {
        private readonly IDictionary<IWorkShiftRuleSet, List<ShiftProjectionCache>> _ruleSetListDictionary = new Dictionary<IWorkShiftRuleSet, List<ShiftProjectionCache>>();
        private readonly IRuleSetDeletedActivityChecker _ruleSetDeletedActivityChecker;
    	private readonly IRuleSetDeletedShiftCategoryChecker _rulesSetDeletedShiftCategoryChecker;
	    private readonly IWorkShiftFromEditableShift _workShiftFromEditableShift;
		private readonly ShiftProjectionCacheFetcher _shiftProjectionCacheFetcher;
		private readonly IPersonalShiftMeetingTimeChecker personalShiftMeetingTimeChecker = new PersonalShiftMeetingTimeChecker();

	    public ShiftProjectionCacheManager(IRuleSetDeletedActivityChecker ruleSetDeletedActivityChecker, 
			IRuleSetDeletedShiftCategoryChecker rulesSetDeletedShiftCategoryChecker,
			IWorkShiftFromEditableShift workShiftFromEditableShift,
			ShiftProjectionCacheFetcher shiftProjectionCacheFetcher)
        {
            _ruleSetDeletedActivityChecker = ruleSetDeletedActivityChecker;
			_rulesSetDeletedShiftCategoryChecker = rulesSetDeletedShiftCategoryChecker;
	        _workShiftFromEditableShift = workShiftFromEditableShift;
			_shiftProjectionCacheFetcher = shiftProjectionCacheFetcher;
		}

	    public ShiftProjectionCache ShiftProjectionCacheFromShift(IEditableShift shift, IDateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod)
	    {
		    var workShift = _workShiftFromEditableShift.Convert(shift, dateOnlyAsDateTimePeriod.DateOnly,dateOnlyAsDateTimePeriod.TimeZone());
		    var ret = new ShiftProjectionCache(workShift, personalShiftMeetingTimeChecker);
			ret.SetDate(dateOnlyAsDateTimePeriod);

		    return ret;
	    }

	    public IList<ShiftProjectionCache> ShiftProjectionCachesFromRuleSets(IDateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod, IEnumerable<IWorkShiftRuleSet> ruleSets, bool checkExcluded)
	    {
		    var shiftProjectionCaches = ruleSets.Where(ruleSet =>
		    {
			    if (checkExcluded && !ruleSet.IsValidDate(dateOnlyAsDateTimePeriod.DateOnly))
				    return false;

			    if (_ruleSetDeletedActivityChecker.ContainsDeletedActivity(ruleSet))
				    return false;

			    if (_rulesSetDeletedShiftCategoryChecker.ContainsDeletedShiftCategory(ruleSet))
				    return false;

			    return true;
		    }).SelectMany(getShiftsForRuleSet).ToArray();

		    foreach (var shiftProjectionCache in shiftProjectionCaches)
		    {
			    shiftProjectionCache.SetDate(dateOnlyAsDateTimePeriod);
		    }
			
			return shiftProjectionCaches;
		}

	    public IList<ShiftProjectionCache> ShiftProjectionCachesFromRuleSets(IDateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod, IRuleSetBag bag, bool forRestrictionsOnly, bool checkExcluded)
	    {
		    if (bag == null)
			    return new List<ShiftProjectionCache>();
		    var ruleSets =
			    bag.RuleSetCollection.Where(workShiftRuleSet => workShiftRuleSet.OnlyForRestrictions == forRestrictionsOnly);
		    return ShiftProjectionCachesFromRuleSets(dateOnlyAsDateTimePeriod, ruleSets, checkExcluded);
	    }

		private IEnumerable<ShiftProjectionCache> getShiftsForRuleSet(IWorkShiftRuleSet ruleSet)
        {
			List<ShiftProjectionCache> shiftProjectionCacheList;

			if (!_ruleSetListDictionary.TryGetValue(ruleSet, out shiftProjectionCacheList))
            {
				shiftProjectionCacheList = _shiftProjectionCacheFetcher.Execute(ruleSet).ToList();

				_ruleSetListDictionary.Add(ruleSet, shiftProjectionCacheList);
            }
			return shiftProjectionCacheList;
        }

		public void Dispose()
	    {
			_ruleSetListDictionary.Clear();
	    }
    }
}
