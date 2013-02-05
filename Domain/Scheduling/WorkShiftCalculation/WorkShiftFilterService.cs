using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation
{
    public interface IWorkShiftFilterService
    {
        IList<IShiftProjectionCache> Filter(DateOnly dateOnly, IPerson person, IList<IScheduleMatrixPro> matrixList, IEffectiveRestriction effectiveRestriction, ISchedulingOptions schedulingOptions);
    }

    public class WorkShiftFilterService : IWorkShiftFilterService
    {
        private readonly IShiftProjectionCacheManager _shiftProjectionCacheManager;
        private readonly IShiftProjectionCacheFilter _shiftProjectionCacheFilter;
        private readonly IWorkShiftMinMaxCalculator _workShiftMinMaxCalculator;
        private readonly ISchedulingResultStateHolder _resultStateHolder;
        private readonly IShiftLengthDecider _shiftLengthDecider;

        public WorkShiftFilterService(IShiftProjectionCacheManager shiftProjectionCacheManager,
                                      IShiftProjectionCacheFilter shiftProjectionCacheFilter,
                                      IWorkShiftMinMaxCalculator workShiftMinMaxCalculator,
                                      ISchedulingResultStateHolder resultStateHolder,
                                      IShiftLengthDecider shiftLengthDecider)
        {
            _shiftProjectionCacheManager = shiftProjectionCacheManager;
            _shiftProjectionCacheFilter = shiftProjectionCacheFilter;
            _workShiftMinMaxCalculator = workShiftMinMaxCalculator;
            _resultStateHolder = resultStateHolder;
            _shiftLengthDecider = shiftLengthDecider;
        }

        public IList<IShiftProjectionCache> Filter(DateOnly dateOnly, IPerson person, IList<IScheduleMatrixPro> matrixList, IEffectiveRestriction effectiveRestriction, ISchedulingOptions schedulingOptions)
        {
            FinderResult = new WorkShiftFinderResult(person, dateOnly);
            _workShiftMinMaxCalculator.ResetCache();
            if (person != null)
            {
                var currentSchedulePeriod = person.VirtualSchedulePeriod(dateOnly);

                if (!currentSchedulePeriod.IsValid)
                    return null;
            }

            if (!_shiftProjectionCacheFilter.CheckRestrictions(schedulingOptions, effectiveRestriction, FinderResult))
                return null;

            if (schedulingOptions == null)
                return null;

            if (person != null)
            {
                var timeZone = person.PermissionInformation.DefaultTimeZone();

                var personPeriod = person.Period(dateOnly);
                IRuleSetBag bag = personPeriod.RuleSetBag;

                var shiftList = _shiftProjectionCacheManager.ShiftProjectionCachesFromAdjustedRuleSetBag(dateOnly, timeZone,
                                                                                                         bag, false,
                                                                                                         effectiveRestriction);
                if (shiftList.Count > 0)
                {
                    if (schedulingOptions.ShiftCategory != null)
                    {
                        // override the one in Effective
                        if (effectiveRestriction != null)
                            effectiveRestriction.ShiftCategory = schedulingOptions.ShiftCategory;
                    }
                    shiftList = _shiftProjectionCacheFilter.FilterOnMainShiftOptimizeActivitiesSpecification(shiftList, schedulingOptions.MainShiftOptimizeActivitySpecification);

                    shiftList = _shiftProjectionCacheFilter.FilterOnRestrictionAndNotAllowedShiftCategories(dateOnly,
                                                                                                            person.
                                                                                                                PermissionInformation.
                                                                                                                DefaultTimeZone(),
                                                                                                            shiftList,
                                                                                                            effectiveRestriction,
                                                                                                            schedulingOptions.
                                                                                                                NotAllowedShiftCategories,
                                                                                                            FinderResult);


                    if (shiftList.Count == 0)
                        return null;

                    MinMax<TimeSpan>? allowedMinMax = null;
                    if (matrixList != null)
                        foreach (var matrix in matrixList)
                        {
                            var minMax = _workShiftMinMaxCalculator.MinMaxAllowedShiftContractTime(dateOnly, matrix, schedulingOptions);
                            if (!minMax.HasValue) continue;
                            if (!allowedMinMax.HasValue)
                                allowedMinMax = minMax;
                            else
                            {
                                if (minMax.Value.Minimum > allowedMinMax.Value.Minimum)
                                    allowedMinMax = new MinMax<TimeSpan>(minMax.Value.Minimum, allowedMinMax.Value.Maximum);
                                if (minMax.Value.Maximum < allowedMinMax.Value.Maximum)
                                    allowedMinMax = new MinMax<TimeSpan>(allowedMinMax.Value.Minimum, minMax.Value.Maximum);
                            }
                        }

                    if (!allowedMinMax.HasValue)
                    {
                        loggFilterResult(UserTexts.Resources.NoShiftsThatMatchesTheContractTimeCouldBeFound, shiftList.Count, 0);
                        return null;
                    }

                    var wholeRange = ScheduleDictionary[person];
                    shiftList = _shiftProjectionCacheFilter.Filter(allowedMinMax.Value, shiftList, dateOnly, wholeRange,
                                                                   FinderResult);
                    if (shiftList.Count == 0)
                        return null;

                    if (schedulingOptions.WorkShiftLengthHintOption == WorkShiftLengthHintOption.AverageWorkTime && matrixList!=null)
                    {
                        shiftList = _shiftLengthDecider.FilterList(shiftList, _workShiftMinMaxCalculator, matrixList[0], schedulingOptions);
                        if (shiftList.Count == 0)
                            return null;
                    }
                }
                return shiftList;
            }
            return null;
        }

        private void loggFilterResult(string message, int countWorkShiftsBefore, int countWorkShiftsAfter)
        {
            if (FinderResult != null)
                FinderResult.AddFilterResults(new WorkShiftFilterResult(message, countWorkShiftsBefore, countWorkShiftsAfter));
        }

        public IWorkShiftFinderResult FinderResult { get; private set; }

        private IScheduleDictionary ScheduleDictionary
        {
            get { return _resultStateHolder.Schedules; }
        }
    }
}