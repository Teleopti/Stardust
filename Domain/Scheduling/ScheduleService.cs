using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.Domain.Scheduling
{
    public class ScheduleService : IScheduleService
    {
	    private readonly Func<ISchedulerStateHolder> _stateHolder;
	    private readonly IWorkShiftFinderService _finderService;
		private readonly IMatrixListFactory _scheduleMatrixListCreator;
        private readonly IShiftCategoryLimitationChecker _shiftCategoryLimitationChecker;
        private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
        private readonly Hashtable _finderResults = new Hashtable();

        public ScheduleService(
					Func<ISchedulerStateHolder> stateHolder,
			IWorkShiftFinderService finderService, 
            IMatrixListFactory scheduleMatrixListCreator,
            IShiftCategoryLimitationChecker shiftCategoryLimitationChecker, 
            IEffectiveRestrictionCreator effectiveRestrictionCreator)
        {
	        _stateHolder = stateHolder;
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
			SchedulingOptions schedulingOptions, 
			IResourceCalculateDelayer resourceCalculateDelayer,
			ISchedulePartModifyAndRollbackService rollbackService)
        {
            var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(schedulePart, schedulingOptions);
            return SchedulePersonOnDay(schedulePart, schedulingOptions, effectiveRestriction, resourceCalculateDelayer, rollbackService);
        }

		public bool SchedulePersonOnDay(
			IScheduleDay schedulePart,
			SchedulingOptions schedulingOptions,
			IEffectiveRestriction effectiveRestriction,
			IResourceCalculateDelayer resourceCalculateDelayer,
			ISchedulePartModifyAndRollbackService rollbackService)
		{
			return schedulePersonOnDay(schedulePart, schedulingOptions, effectiveRestriction, resourceCalculateDelayer,
			                           null, rollbackService);
		}
		
        private bool schedulePersonOnDay(
            IScheduleDay schedulePart,
            SchedulingOptions schedulingOptions,
            IEffectiveRestriction effectiveRestriction,
			IResourceCalculateDelayer resourceCalculateDelayer,
			IPerson person,
			ISchedulePartModifyAndRollbackService rollbackService)
        {
            using (PerformanceOutput.ForOperation("SchedulePersonOnDay"))
            {
                WorkShiftFinderServiceResult cache;
                if (schedulePart.IsScheduled())
                {
                    return true;
                }

                var scheduleDateOnly = schedulePart.DateOnlyAsPeriod.DateOnly;
                person = person ?? schedulePart.Person;

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

                    IList<IScheduleMatrixPro> matrixList = _scheduleMatrixListCreator.CreateMatrixListForSelection(_stateHolder().Schedules, new List <IScheduleDay> { schedulePart });
                    if (matrixList.Count == 0)
                        return false;
                    IScheduleMatrixPro matrix = matrixList[0];

                    cache = _finderService.FindBestShift(schedulePart, schedulingOptions, matrix, effectiveRestriction);
                }

                if (cache.ResultHolder == null)
                {
                    if (!_finderResults.Contains(cache.FinderResult.PersonDateKey))
                    {
                        _finderResults.Add(cache.FinderResult.PersonDateKey, cache.FinderResult);
                    }
                    return false;
                }

	            if (_finderResults.Contains(cache.FinderResult.PersonDateKey))
	            {
		            var res = (WorkShiftFinderResult) _finderResults[cache.FinderResult.PersonDateKey];
		            res.Successful = true;
	            }

	            schedulePart.AddMainShift(cache.ResultHolder.ShiftProjection.TheMainShift);
                rollbackService.Modify(schedulePart);

            	resourceCalculateDelayer.CalculateIfNeeded(scheduleDateOnly, cache.ResultHolder.ShiftProjection.WorkShiftProjectionPeriod, false);

                return true;
            }
        }
    }
}
