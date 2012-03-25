using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
    public class ScheduleService : IScheduleService
    {
        private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
        private readonly IWorkShiftFinderService _finderService;
        private readonly IScheduleMatrixListCreator _scheduleMatrixListCreator;
        private readonly IShiftCategoryLimitationChecker _shiftCategoryLimitationChecker;
        private readonly ISchedulePartModifyAndRollbackService _rollbackService;
        private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
        private readonly Hashtable _finderResults = new Hashtable();

        public ScheduleService(
            IResourceOptimizationHelper resourceOptimizationHelper,
            IWorkShiftFinderService finderService, 
            IScheduleMatrixListCreator scheduleMatrixListCreator,
            IShiftCategoryLimitationChecker shiftCategoryLimitationChecker, 
            ISchedulePartModifyAndRollbackService rollbackService,
            IEffectiveRestrictionCreator effectiveRestrictionCreator)
        {
            _resourceOptimizationHelper = resourceOptimizationHelper;
            _finderService = finderService;
            _scheduleMatrixListCreator = scheduleMatrixListCreator;
            _shiftCategoryLimitationChecker = shiftCategoryLimitationChecker;
            _rollbackService = rollbackService;
            _effectiveRestrictionCreator = effectiveRestrictionCreator;
        }

        public ReadOnlyCollection<IWorkShiftFinderResult> FinderResults
        {
            get
            {
                IList<IWorkShiftFinderResult> tmp = new List<IWorkShiftFinderResult>(_finderResults.Count);
                foreach (DictionaryEntry finderResult in _finderResults)
                {
                    tmp.Add((IWorkShiftFinderResult)finderResult.Value);
                }
                return new ReadOnlyCollection<IWorkShiftFinderResult>(tmp);
            }
        }

        public void ClearFinderResults()
        {
            _finderResults.Clear();
        }

        public bool SchedulePersonOnDay(IScheduleDay schedulePart, ISchedulingOptions schedulingOptions, bool useOccupancyAdjustment, IShiftCategory useThisCategory)
        {
            var originalCategory = schedulingOptions.ShiftCategory;
            schedulingOptions.ShiftCategory = useThisCategory;
            var result = SchedulePersonOnDay(schedulePart, schedulingOptions, useOccupancyAdjustment);
            schedulingOptions.ShiftCategory = originalCategory;
            return result;
        }

        public bool SchedulePersonOnDay(IScheduleDay schedulePart, ISchedulingOptions schedulingOptions, bool useOccupancyAdjustment)
        {
            var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(schedulePart, schedulingOptions);
            return SchedulePersonOnDay(schedulePart, schedulingOptions, useOccupancyAdjustment, effectiveRestriction);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public bool SchedulePersonOnDay(
            IScheduleDay schedulePart,
            ISchedulingOptions schedulingOptions,
            bool useOccupancyAdjustment, 
            IEffectiveRestriction effectiveRestriction)
        {
            using (PerformanceOutput.ForOperation("SchedulePersonOnDay"))
            {
                IWorkShiftCalculationResultHolder cache;
                if (schedulePart.IsScheduled())
                {
                    return true;
                }

                var scheduleDateOnly = schedulePart.DateOnlyAsPeriod.DateOnly;
                var person = schedulePart.Person;
                if (effectiveRestriction == null)
                {
                    IWorkShiftFinderResult finderResult = new WorkShiftFinderResult(person, scheduleDateOnly);
                    finderResult.AddFilterResults(new WorkShiftFilterResult(UserTexts.Resources.ConflictingRestrictions, 0, 0));
                    if (!_finderResults.Contains(finderResult.PersonDateKey))
                        _finderResults.Add(finderResult.PersonDateKey, finderResult);
                    return false;
                }

                if (effectiveRestriction.NotAvailable)
                    return false;

                using (PerformanceOutput.ForOperation("Finding the best shift in total"))
                {
                    _shiftCategoryLimitationChecker.SetBlockedShiftCategories(schedulingOptions, person, scheduleDateOnly);

                    IList<IScheduleMatrixPro> matrixList = _scheduleMatrixListCreator.
                        CreateMatrixListFromScheduleParts(
                            new List<IScheduleDay> { schedulePart });
                    if (matrixList.Count == 0)
                        return false;
                    IScheduleMatrixPro matrix = matrixList[0];

                    cache = _finderService.FindBestShift(schedulePart, schedulingOptions, matrix);
                }

                if (cache == null)
                {
                    if (!_finderResults.Contains(_finderService.FinderResult.PersonDateKey))
                    {
                        _finderResults.Add(_finderService.FinderResult.PersonDateKey, _finderService.FinderResult);
                    }
                    return false;
                }

                schedulePart.AddMainShift((IMainShift)cache.ShiftProjection.TheMainShift.EntityClone());
                _rollbackService.Modify(schedulePart);

                _resourceOptimizationHelper.ResourceCalculateDate(scheduleDateOnly, useOccupancyAdjustment, schedulingOptions.ConsiderShortBreaks);
                DateTimePeriod? dateTimePeriod = cache.ShiftProjection.WorkShiftProjectionPeriod;
                if (dateTimePeriod.HasValue)
                {
                    DateTimePeriod period = dateTimePeriod.Value;
                    if (period.StartDateTime.Date != period.EndDateTime.Date)
                        _resourceOptimizationHelper.ResourceCalculateDate(scheduleDateOnly.AddDays(1), useOccupancyAdjustment, schedulingOptions.ConsiderShortBreaks);
                }

                return true;
            }
        }
    }
}
