using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation
{
    public interface IWorkShiftFilterService
    {
        IList<IShiftProjectionCache> Filter(DateOnly dateOnly, IPerson person, IScheduleMatrixPro matrix, IEffectiveRestriction effectiveRestriction, ISchedulingOptions schedulingOptions, IPossibleStartEndCategory possibleStartEndCategory);
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

        public IList<IShiftProjectionCache> Filter(DateOnly dateOnly, IPerson person, IScheduleMatrixPro matrix, IEffectiveRestriction effectiveRestriction, ISchedulingOptions schedulingOptions, IPossibleStartEndCategory possibleStartEndCategory)
        {
            FinderResult = new WorkShiftFinderResult(person, dateOnly);
            _workShiftMinMaxCalculator.ResetCache();
            var currentSchedulePeriod = person.VirtualSchedulePeriod(dateOnly);

            if (!currentSchedulePeriod.IsValid)
                return null;

            if (!_shiftProjectionCacheFilter.CheckRestrictions(schedulingOptions, effectiveRestriction, FinderResult))
                return null;

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
                    effectiveRestriction.ShiftCategory = schedulingOptions.ShiftCategory;
                }
                if (possibleStartEndCategory != null)
                    shiftList = _shiftProjectionCacheFilter.FilterOnShiftCategory(possibleStartEndCategory.ShiftCategory, shiftList, FinderResult);

                shiftList = _shiftProjectionCacheFilter.FilterOnGroupSchedulingCommonStartEnd(shiftList, possibleStartEndCategory, schedulingOptions, FinderResult);

                shiftList = _shiftProjectionCacheFilter.FilterOnGroupSchedulingCommonActivity(shiftList, schedulingOptions, possibleStartEndCategory, FinderResult);

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

                MinMax<TimeSpan>? allowedMinMax = _workShiftMinMaxCalculator.MinMaxAllowedShiftContractTime(dateOnly, matrix,
                                                                                                            schedulingOptions);

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

                if (schedulingOptions.WorkShiftLengthHintOption == WorkShiftLengthHintOption.AverageWorkTime)
                {
                    shiftList = _shiftLengthDecider.FilterList(shiftList, _workShiftMinMaxCalculator, matrix, schedulingOptions);
                    if (shiftList.Count == 0)
                        return null;
                }
            }
            return shiftList;
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