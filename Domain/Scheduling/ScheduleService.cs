using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
    public class ScheduleService : IScheduleService
    {
	    private readonly Func<ISchedulerStateHolder> _stateHolder;
	    private readonly IWorkShiftFinderService _finderService;
		private readonly MatrixListFactory _scheduleMatrixListCreator;
        private readonly IShiftCategoryLimitationChecker _shiftCategoryLimitationChecker;
        private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
        private readonly Dictionary<Tuple<Guid,DateOnly>,WorkShiftFinderResult> _finderResults = new Dictionary<Tuple<Guid, DateOnly>, WorkShiftFinderResult>();

        public ScheduleService(
					Func<ISchedulerStateHolder> stateHolder,
			IWorkShiftFinderService finderService,
					MatrixListFactory scheduleMatrixListCreator,
            IShiftCategoryLimitationChecker shiftCategoryLimitationChecker, 
            IEffectiveRestrictionCreator effectiveRestrictionCreator)
        {
	        _stateHolder = stateHolder;
	        _finderService = finderService;
            _scheduleMatrixListCreator = scheduleMatrixListCreator;
            _shiftCategoryLimitationChecker = shiftCategoryLimitationChecker;
            _effectiveRestrictionCreator = effectiveRestrictionCreator;
        }

        public ReadOnlyCollection<WorkShiftFinderResult> FinderResults => new ReadOnlyCollection<WorkShiftFinderResult>(_finderResults.Values.ToArray());

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
                    var finderResult = new WorkShiftFinderResult(person, scheduleDateOnly);
                    finderResult.AddFilterResults(new WorkShiftFilterResult(UserTexts.Resources.ConflictingRestrictions, 0, 0));
                    if (!_finderResults.ContainsKey(finderResult.PersonDateKey))
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
                    if (!_finderResults.ContainsKey(cache.FinderResult.PersonDateKey))
                    {
                        _finderResults.Add(cache.FinderResult.PersonDateKey, cache.FinderResult);
                    }
                    return false;
                }

	            WorkShiftFinderResult res;
	            if (_finderResults.TryGetValue(cache.FinderResult.PersonDateKey, out res))
	            {
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
