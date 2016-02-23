using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
    public class ScheduleService : IScheduleService
    {
        private readonly IWorkShiftFinderService _finderService;
		private readonly IMatrixListFactory _scheduleMatrixListCreator;
        private readonly IShiftCategoryLimitationChecker _shiftCategoryLimitationChecker;
        private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
        private readonly Hashtable _finderResults = new Hashtable();

        public ScheduleService(
			IWorkShiftFinderService finderService, 
            IMatrixListFactory scheduleMatrixListCreator,
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
			ISchedulePartModifyAndRollbackService rollbackService)
        {
            var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(schedulePart, schedulingOptions);
            return SchedulePersonOnDay(schedulePart, schedulingOptions, effectiveRestriction, resourceCalculateDelayer, rollbackService);
        }

		public bool SchedulePersonOnDay(
			IScheduleDay schedulePart,
			ISchedulingOptions schedulingOptions,
			IEffectiveRestriction effectiveRestriction,
			IResourceCalculateDelayer resourceCalculateDelayer,
			ISchedulePartModifyAndRollbackService rollbackService)
		{
			return schedulePersonOnDay(schedulePart, schedulingOptions, effectiveRestriction, resourceCalculateDelayer,
			                           null, rollbackService);
		}

    	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        private bool schedulePersonOnDay(
            IScheduleDay schedulePart,
            ISchedulingOptions schedulingOptions,
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
                        CreateMatrixListForSelection(
                            new List<IScheduleDay> { schedulePart });
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
