using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.Domain.Scheduling
{
    public class ScheduleService : IScheduleService
    {
	    private readonly Func<ISchedulerStateHolder> _stateHolder;
	    private readonly WorkShiftFinderService _finderService;
		private readonly MatrixListFactory _scheduleMatrixListCreator;
        private readonly IShiftCategoryLimitationChecker _shiftCategoryLimitationChecker;
        private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;

        public ScheduleService(
					Func<ISchedulerStateHolder> stateHolder,
					WorkShiftFinderService finderService,
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
			return schedulePersonOnDay(schedulePart, schedulingOptions, effectiveRestriction, resourceCalculateDelayer, rollbackService);
		}
		
        private bool schedulePersonOnDay(
            IScheduleDay schedulePart,
            SchedulingOptions schedulingOptions,
            IEffectiveRestriction effectiveRestriction,
			IResourceCalculateDelayer resourceCalculateDelayer,
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

                if (effectiveRestriction == null)
                {
                    return false;
                }

                if (effectiveRestriction.NotAvailable)
                    return false;

                using (PerformanceOutput.ForOperation("Finding the best shift in total"))
                {
                    _shiftCategoryLimitationChecker.SetBlockedShiftCategories(schedulingOptions, schedulePart.Person, scheduleDateOnly);

                    var matrixList = _scheduleMatrixListCreator.CreateMatrixListForSelection(_stateHolder().Schedules, new List <IScheduleDay> { schedulePart }).ToList();
                    if (matrixList.Count == 0)
                        return false;
                    IScheduleMatrixPro matrix = matrixList[0];

                    cache = _finderService.FindBestShift(schedulePart, schedulingOptions, matrix, effectiveRestriction);
                }

                if (cache == null)
                {
                    return false;
                }

	            schedulePart.AddMainShift(cache.ShiftProjection.TheMainShift);
                rollbackService.Modify(schedulePart);

            	resourceCalculateDelayer.CalculateIfNeeded(scheduleDateOnly, cache.ShiftProjection.WorkShiftProjectionPeriod(), false);
                return true;
            }
        }
    }
}
