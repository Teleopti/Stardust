using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class ShiftProjectionCacheManager : IShiftProjectionCacheManager
    {
        private readonly IDictionary<IWorkShiftRuleSet, IList<IShiftProjectionCache>> _ruleSetListDictionary = new Dictionary<IWorkShiftRuleSet, IList<IShiftProjectionCache>>();
        private readonly IShiftFromMasterActivityService _shiftFromMasterActivityService;
        private readonly IRuleSetDeletedActivityChecker _ruleSetDeletedActivityChecker;
    	private readonly IRuleSetDeletedShiftCategoryChecker _rulesSetDeletedShiftCategoryChecker;
        private readonly IRuleSetProjectionEntityService _ruleSetProjectionEntityService;

        public ShiftProjectionCacheManager(IShiftFromMasterActivityService shiftFromMasterActivityService, 
            IRuleSetDeletedActivityChecker ruleSetDeletedActivityChecker, IRuleSetDeletedShiftCategoryChecker rulesSetDeletedShiftCategoryChecker,
            IRuleSetProjectionEntityService ruleSetProjectionEntityService)
        {
            _shiftFromMasterActivityService = shiftFromMasterActivityService;
            _ruleSetDeletedActivityChecker = ruleSetDeletedActivityChecker;
			_rulesSetDeletedShiftCategoryChecker = rulesSetDeletedShiftCategoryChecker;
		    _ruleSetProjectionEntityService = ruleSetProjectionEntityService;
        }

	    public IList<IShiftProjectionCache> ShiftProjectionCachesFromRuleSetBag(DateOnly scheduleDateOnly,
		    TimeZoneInfo timeZone, IRuleSetBag bag, bool forRestrictionsOnly, bool checkExcluded)
	    {
		    var shiftProjectionCaches = new List<IShiftProjectionCache>();
		    if (bag == null)
			    return shiftProjectionCaches;
		    var ruleSets =
			    bag.RuleSetCollection.Where(workShiftRuleSet => workShiftRuleSet.OnlyForRestrictions == forRestrictionsOnly)
				    .ToList();

		    foreach (IWorkShiftRuleSet ruleSet in ruleSets)
		    {
			    if (checkExcluded && !ruleSet.IsValidDate(scheduleDateOnly))
				    continue;

			    if (_ruleSetDeletedActivityChecker.ContainsDeletedActivity(ruleSet))
				    continue;

			    if (_rulesSetDeletedShiftCategoryChecker.ContainsDeletedActivity(ruleSet))
				    continue;

			    IEnumerable<IShiftProjectionCache> ruleSetList = getShiftsForRuleSet(ruleSet);
			    if (ruleSetList == null)
				    continue;

			    foreach (var projectionCache in ruleSetList)
			    {
				    shiftProjectionCaches.Add(projectionCache);
				    projectionCache.SetDate(scheduleDateOnly, timeZone);
			    }
		    }

		    return shiftProjectionCaches;
	    }

	    private IEnumerable<IShiftProjectionCache> getShiftsForRuleSet(IWorkShiftRuleSet ruleSet)
        {
			IList<IShiftProjectionCache> shiftProjectionCacheList;

			if (!_ruleSetListDictionary.TryGetValue(ruleSet, out shiftProjectionCacheList))
            {
				var callback = new WorkShiftAddStopperCallback();
				callback.StartNewRuleSet(ruleSet);
				IEnumerable<IWorkShiftVisualLayerInfo> infoList = _ruleSetProjectionEntityService.ProjectionCollection(ruleSet, callback);
                IList<IWorkShift> tmpList = new List<IWorkShift>();
                foreach (var workShiftVisualLayerInfo in infoList)
                {
                    tmpList.Add(workShiftVisualLayerInfo.WorkShift);
                }

				shiftProjectionCacheList = new List<IShiftProjectionCache>();
                foreach (IWorkShift shift in tmpList)
                {
					IEnumerable<IWorkShift> shiftsFromMasterActivity = _shiftFromMasterActivityService.Generate(shift);

                    if (shiftsFromMasterActivity == null)
						shiftProjectionCacheList.Add(new ShiftProjectionCache(shift, new PersonalShiftMeetingTimeChecker()));
                    else
                    {
                        foreach (IWorkShift workShift in shiftsFromMasterActivity)
                        {
							shiftProjectionCacheList.Add(new ShiftProjectionCache(workShift, new PersonalShiftMeetingTimeChecker()));
                        }
                    }
                }
				_ruleSetListDictionary.Add(ruleSet, shiftProjectionCacheList);
            }
			return shiftProjectionCacheList;
        }
    }
}
