using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
    public class ScheduleService : IScheduleService
    {
        private readonly IWorkShiftFinderService _finderService;
        private readonly IScheduleMatrixListCreator _scheduleMatrixListCreator;
        private readonly IShiftCategoryLimitationChecker _shiftCategoryLimitationChecker;
        private readonly ISchedulePartModifyAndRollbackService _rollbackService;
        private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
        private readonly Hashtable _finderResults = new Hashtable();

        public ScheduleService(
            IWorkShiftFinderService finderService, 
            IScheduleMatrixListCreator scheduleMatrixListCreator,
            IShiftCategoryLimitationChecker shiftCategoryLimitationChecker, 
            ISchedulePartModifyAndRollbackService rollbackService,
            IEffectiveRestrictionCreator effectiveRestrictionCreator)
        {
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

        public bool SchedulePersonOnDay(
			IScheduleDay schedulePart, 
			ISchedulingOptions schedulingOptions, 
			bool useOccupancyAdjustment, 
			IResourceCalculateDelayer resourceCalculateDelayer,
			IPossibleStartEndCategory possibleStartEndCategory)
        {
            var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(schedulePart, schedulingOptions);
            return SchedulePersonOnDay(schedulePart, schedulingOptions, useOccupancyAdjustment, effectiveRestriction, resourceCalculateDelayer, possibleStartEndCategory);
        }

		public bool SchedulePersonOnDay(
			IScheduleDay schedulePart,
			ISchedulingOptions schedulingOptions,
			IEffectiveRestriction effectiveRestriction,
			IResourceCalculateDelayer resourceCalculateDelayer,
			ISchedulePartModifyAndRollbackService rollbackService)
		{
			return schedulePersonOnDay(schedulePart, schedulingOptions, effectiveRestriction,
			                           resourceCalculateDelayer, null, rollbackService);
		}

		public bool SchedulePersonOnDay(
			IScheduleDay schedulePart,
			ISchedulingOptions schedulingOptions,
			bool useOccupancyAdjustment,
			IEffectiveRestriction effectiveRestriction,
			IResourceCalculateDelayer resourceCalculateDelayer,
			IPossibleStartEndCategory possibleStartEndCategory)
		{
			return schedulePersonOnDay(schedulePart, schedulingOptions, effectiveRestriction, resourceCalculateDelayer,
			                           possibleStartEndCategory, _rollbackService);
		}


    	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        private bool schedulePersonOnDay(
            IScheduleDay schedulePart,
            ISchedulingOptions schedulingOptions,
            IEffectiveRestriction effectiveRestriction,
			IResourceCalculateDelayer resourceCalculateDelayer,
			IPossibleStartEndCategory possibleStartEndCategory,
			ISchedulePartModifyAndRollbackService rollbackService)
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

                    cache = _finderService.FindBestShift(schedulePart, schedulingOptions, matrix, effectiveRestriction, possibleStartEndCategory);
                }

                if (cache == null)
                {
                    if (!_finderResults.Contains(_finderService.FinderResult.PersonDateKey))
                    {
                        _finderResults.Add(_finderService.FinderResult.PersonDateKey, _finderService.FinderResult);
                    }
                    return false;
                }

					 if (_finderResults.Contains(_finderService.FinderResult.PersonDateKey))
					 {
					 	var res = (WorkShiftFinderResult)_finderResults[_finderService.FinderResult.PersonDateKey];
					 	res.Successful = true;
					 }
                schedulePart.AddMainShift((IMainShift)cache.ShiftProjection.TheMainShift.EntityClone());
                rollbackService.Modify(schedulePart);

            	resourceCalculateDelayer.CalculateIfNeeded(scheduleDateOnly,
            	                                           cache.ShiftProjection.WorkShiftProjectionPeriod, new List<IScheduleDay>{schedulePart});

                return true;
            }
        }
    }
}
