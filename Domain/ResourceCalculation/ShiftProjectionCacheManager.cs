using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
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

        public IList<IShiftProjectionCache> ShiftProjectionCachesFromRuleSetBag(DateOnly scheduleDateOnly, ICccTimeZoneInfo timeZone, IRuleSetBag bag, bool forRestrictionsOnly)//, IPerson person)
        {
            var shiftProjectionCaches = new List<IShiftProjectionCache>();
            if (bag == null)
                return shiftProjectionCaches;
            var ruleSets = bag.RuleSetCollection.Where(workShiftRuleSet => workShiftRuleSet.OnlyForRestrictions == forRestrictionsOnly).ToList();

            foreach (IWorkShiftRuleSet ruleSet in ruleSets)
            {
                if (ruleSet.IsValidDate(scheduleDateOnly))
                {
					if (!_ruleSetDeletedActivityChecker.ContainsDeletedActivity(ruleSet) && !_rulesSetDeletedShiftCategoryChecker.ContainsDeletedActivity(ruleSet))
                    {
                        IEnumerable<IShiftProjectionCache> ruleSetList = GetShiftsForRuleset(ruleSet);
                        if (ruleSetList != null)
                        {
                            foreach (var projectionCache in ruleSetList)
                            {
                                shiftProjectionCaches.Add(projectionCache);             
                                projectionCache.SetDate(scheduleDateOnly, timeZone);
                            }
                        }
                    }
                }
            }

            return shiftProjectionCaches;
        }

        private IEnumerable<IShiftProjectionCache> GetShiftsForRuleset(IWorkShiftRuleSet ruleSet)
        {
            IList<IShiftProjectionCache> retList;

            if (!_ruleSetListDictionary.TryGetValue(ruleSet, out retList))
            {

                IEnumerable<IWorkShiftVisualLayerInfo> infoList = _ruleSetProjectionEntityService.ProjectionCollection(ruleSet);
                IList<IWorkShift> tmpList = new List<IWorkShift>();
                foreach (var workShiftVisualLayerInfo in infoList)
                {
                    tmpList.Add(workShiftVisualLayerInfo.WorkShift);
                }

                retList = new List<IShiftProjectionCache>();
                foreach (IWorkShift shift in tmpList)
                {
                    IEnumerable<IWorkShift> shiftsFromMasterActivity = GetShiftFromMasterActivity(shift);

                    if (shiftsFromMasterActivity == null)
                        retList.Add(new ShiftProjectionCache(shift));
                    else
                    {
                        foreach (IWorkShift workShift in shiftsFromMasterActivity)
                        {
                            retList.Add(new ShiftProjectionCache(workShift));
                        }
                    }
                }
                _ruleSetListDictionary.Add(ruleSet, retList);

            }
            return retList;
        }

        private IEnumerable<IWorkShift> GetShiftFromMasterActivity(IWorkShift workShift)
        {
            return _shiftFromMasterActivityService.Generate(workShift);
        }
    }
}
