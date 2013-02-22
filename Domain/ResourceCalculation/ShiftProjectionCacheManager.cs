using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
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

        public IList<IShiftProjectionCache> ShiftProjectionCachesFromRuleSetBag(DateOnly scheduleDateOnly, TimeZoneInfo timeZone, IRuleSetBag bag, bool forRestrictionsOnly)//, IPerson person)
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

        public IList<IShiftProjectionCache> ShiftProjectionCachesFromAdjustedRuleSetBag(DateOnly scheduleDateOnly, TimeZoneInfo timeZone, IRuleSetBag bag, bool forRestrictionsOnly, IEffectiveRestriction restriction)//, IPerson person)
        {
            var shiftProjectionCaches = new List<IShiftProjectionCache>();
            if (bag == null)
                return shiftProjectionCaches;
            
            var ruleSets = bag.RuleSetCollection.Where(workShiftRuleSet => workShiftRuleSet.OnlyForRestrictions == forRestrictionsOnly).ToList();
            
            foreach (IWorkShiftRuleSet ruleSet in ruleSets)
            {
                if (ruleSet.IsValidDate(scheduleDateOnly))
                {
                    var clonedRuleSet = (IWorkShiftRuleSet)ruleSet.Clone();

                    if (restriction != null)
                    {
                        var start = resolveTime(clonedRuleSet.TemplateGenerator.StartPeriod.Period.StartTime, restriction.StartTimeLimitation.StartTime, false);
                        var end = resolveTime(clonedRuleSet.TemplateGenerator.StartPeriod.Period.EndTime, restriction.StartTimeLimitation.EndTime, true);
                        if (start > end)
                            continue;
                        var startTimePeriod = new TimePeriod(start, end);

                        start = resolveTime(clonedRuleSet.TemplateGenerator.EndPeriod.Period.StartTime, restriction.EndTimeLimitation.StartTime, false);
                        end = resolveTime(clonedRuleSet.TemplateGenerator.EndPeriod.Period.EndTime, restriction.EndTimeLimitation.EndTime, true);
                        if (start > end)
                            continue;
                        var endTimePeriod = new TimePeriod(start, end);

                        if (endTimePeriod.EndTime < startTimePeriod.StartTime)
                            continue;

                        if (startTimePeriod.EndTime > endTimePeriod.EndTime)
                            startTimePeriod = new TimePeriod(startTimePeriod.StartTime, endTimePeriod.EndTime);

                        if (endTimePeriod.StartTime < startTimePeriod.StartTime)
                            endTimePeriod = new TimePeriod(startTimePeriod.StartTime, endTimePeriod.EndTime);

                        clonedRuleSet.TemplateGenerator.StartPeriod = new TimePeriodWithSegment(startTimePeriod, clonedRuleSet.TemplateGenerator.StartPeriod.Segment);
                        clonedRuleSet.TemplateGenerator.EndPeriod = new TimePeriodWithSegment(endTimePeriod, clonedRuleSet.TemplateGenerator.EndPeriod.Segment);
                    }

                    if (!_ruleSetDeletedActivityChecker.ContainsDeletedActivity(clonedRuleSet) && !_rulesSetDeletedShiftCategoryChecker.ContainsDeletedActivity(clonedRuleSet))
                    {
                        IEnumerable<IShiftProjectionCache> ruleSetList = GetShiftsForRuleset(clonedRuleSet);
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

        private static TimeSpan resolveTime(TimeSpan thisTime, TimeSpan? otherTime, bool min)
        {
            if (!otherTime.HasValue)
                return thisTime;

            if (min)
                return TimeSpan.FromTicks(Math.Min(otherTime.Value.Ticks, thisTime.Ticks));

            return TimeSpan.FromTicks(Math.Max(otherTime.Value.Ticks, thisTime.Ticks));
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
                        retList.Add(new ShiftProjectionCache(shift, new PersonalShiftMeetingTimeChecker()));
                    else
                    {
                        foreach (IWorkShift workShift in shiftsFromMasterActivity)
                        {
                            retList.Add(new ShiftProjectionCache(workShift, new PersonalShiftMeetingTimeChecker()));
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
