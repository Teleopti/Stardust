using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class ShiftProjectionCacheManager : IShiftProjectionCacheManager, IDisposable
    {
        private readonly IDictionary<IWorkShiftRuleSet, List<IShiftProjectionCache>> _ruleSetListDictionary = new Dictionary<IWorkShiftRuleSet, List<IShiftProjectionCache>>();
        private readonly IShiftFromMasterActivityService _shiftFromMasterActivityService;
        private readonly IRuleSetDeletedActivityChecker _ruleSetDeletedActivityChecker;
    	private readonly IRuleSetDeletedShiftCategoryChecker _rulesSetDeletedShiftCategoryChecker;
        private readonly IRuleSetProjectionEntityService _ruleSetProjectionEntityService;
	    private readonly IWorkShiftFromEditableShift _workShiftFromEditableShift;
		private readonly IPersonalShiftMeetingTimeChecker personalShiftMeetingTimeChecker = new PersonalShiftMeetingTimeChecker();

	    public ShiftProjectionCacheManager(IShiftFromMasterActivityService shiftFromMasterActivityService, 
            IRuleSetDeletedActivityChecker ruleSetDeletedActivityChecker, 
			IRuleSetDeletedShiftCategoryChecker rulesSetDeletedShiftCategoryChecker,
            IRuleSetProjectionEntityService ruleSetProjectionEntityService,
			IWorkShiftFromEditableShift workShiftFromEditableShift)
        {
            _shiftFromMasterActivityService = shiftFromMasterActivityService;
            _ruleSetDeletedActivityChecker = ruleSetDeletedActivityChecker;
			_rulesSetDeletedShiftCategoryChecker = rulesSetDeletedShiftCategoryChecker;
		    _ruleSetProjectionEntityService = ruleSetProjectionEntityService;
	        _workShiftFromEditableShift = workShiftFromEditableShift;
        }

	    public IShiftProjectionCache ShiftProjectionCacheFromShift(IEditableShift shift, DateOnly currentDate, TimeZoneInfo agentTimeZone)
	    {
		    var workShift = _workShiftFromEditableShift.Convert(shift, currentDate, agentTimeZone);
		    var ret = new ShiftProjectionCache(workShift, personalShiftMeetingTimeChecker);
			ret.SetDate(currentDate, agentTimeZone);

		    return ret;
	    }

	    public IList<IShiftProjectionCache> ShiftProjectionCachesFromRuleSets(DateOnly scheduleDateOnly, TimeZoneInfo timeZone, IEnumerable<IWorkShiftRuleSet> ruleSets,
		    bool forRestrictionsOnly, bool checkExcluded)
	    {
			var shiftProjectionCaches = new List<IShiftProjectionCache>();
		    ruleSets.ForEach(ruleSet =>
		    {
			    if (checkExcluded && !ruleSet.IsValidDate(scheduleDateOnly))
				    return;

			    if (_ruleSetDeletedActivityChecker.ContainsDeletedActivity(ruleSet))
				    return;

			    if (_rulesSetDeletedShiftCategoryChecker.ContainsDeletedShiftCategory(ruleSet))
				    return;

			    var ruleSetList = getShiftsForRuleSet(ruleSet);
			    if (ruleSetList == null)
				    return;

			    foreach (var projectionCache in ruleSetList)
			    {
				    shiftProjectionCaches.Add(projectionCache);
				    projectionCache.SetDate(scheduleDateOnly, timeZone);
			    }
		    });

			return shiftProjectionCaches;
		}

	    public IList<IShiftProjectionCache> ShiftProjectionCachesFromRuleSets(DateOnly scheduleDateOnly,
		    TimeZoneInfo timeZone, IRuleSetBag bag, bool forRestrictionsOnly, bool checkExcluded)
	    {
		    if (bag == null)
			    return new List<IShiftProjectionCache>();
		    var ruleSets =
			    bag.RuleSetCollection.Where(workShiftRuleSet => workShiftRuleSet.OnlyForRestrictions == forRestrictionsOnly);
		    return ShiftProjectionCachesFromRuleSets(scheduleDateOnly, timeZone, ruleSets, forRestrictionsOnly, checkExcluded);
	    }

	    private IEnumerable<IShiftProjectionCache> getShiftsForRuleSet(IWorkShiftRuleSet ruleSet)
        {
			List<IShiftProjectionCache> shiftProjectionCacheList;

			if (!_ruleSetListDictionary.TryGetValue(ruleSet, out shiftProjectionCacheList))
            {
				var callback = new WorkShiftAddStopperCallback();
				callback.StartNewRuleSet(ruleSet);
				var tmpList = _ruleSetProjectionEntityService.ProjectionCollection(ruleSet, callback).Select(s => s.WorkShift).ToArray();
                
				shiftProjectionCacheList = new List<IShiftProjectionCache>();
	            shiftProjectionCacheList.AddRange(
		            tmpList.SelectMany(shift => _shiftFromMasterActivityService.ExpandWorkShiftsWithMasterActivity(shift))
			            .Select(workShift => new ShiftProjectionCache(workShift, personalShiftMeetingTimeChecker)));
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
