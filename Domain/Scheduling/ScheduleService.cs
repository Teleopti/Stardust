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
        private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
        private readonly Hashtable _finderResults = new Hashtable();

        public ScheduleService(
            IWorkShiftFinderService finderService, 
            IScheduleMatrixListCreator scheduleMatrixListCreator,
            IShiftCategoryLimitationChecker shiftCategoryLimitationChecker, 
            IEffectiveRestrictionCreator effectiveRestrictionCreator)
        {
            _finderService = finderService;
            _scheduleMatrixListCreator = scheduleMatrixListCreator;
            _shiftCategoryLimitationChecker = shiftCategoryLimitationChecker;
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

		//only one usage of this, try to remove
        public bool SchedulePersonOnDay(
			IScheduleDay schedulePart, 
			ISchedulingOptions schedulingOptions, 
			IResourceCalculateDelayer resourceCalculateDelayer,
			IPossibleStartEndCategory possibleStartEndCategory,
			ISchedulePartModifyAndRollbackService rollbackService)
        {
            var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(schedulePart, schedulingOptions);
            return SchedulePersonOnDay(schedulePart, schedulingOptions, effectiveRestriction, resourceCalculateDelayer, possibleStartEndCategory, rollbackService);
        }

		public bool SchedulePersonOnDay(
			IScheduleDay schedulePart,
			ISchedulingOptions schedulingOptions,
			IEffectiveRestriction effectiveRestriction,
			IResourceCalculateDelayer resourceCalculateDelayer,
			IPossibleStartEndCategory possibleStartEndCategory,
			ISchedulePartModifyAndRollbackService rollbackService)
		{
			return schedulePersonOnDay(schedulePart, schedulingOptions, effectiveRestriction, resourceCalculateDelayer,
			                           possibleStartEndCategory, null, rollbackService);
		}

    	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        private bool schedulePersonOnDay(
            IScheduleDay schedulePart,
            ISchedulingOptions schedulingOptions,
            IEffectiveRestriction effectiveRestriction,
			IResourceCalculateDelayer resourceCalculateDelayer,
			IPossibleStartEndCategory possibleStartEndCategory,
			IPerson person,
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
                if(person == null )
                    person = schedulePart.Person;
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
                schedulePart.AddMainShift(cache.ShiftProjection.TheMainShift);
                rollbackService.Modify(schedulePart);

            	resourceCalculateDelayer.CalculateIfNeeded(scheduleDateOnly, cache.ShiftProjection.WorkShiftProjectionPeriod);

                return true;
            }
        }
    }
}
