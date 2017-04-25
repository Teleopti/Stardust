using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class ScheduleDayService :IScheduleDayService
    {
    	private readonly IScheduleService _scheduleService;
        private readonly IDeleteSchedulePartService _deleteSchedulePartService;
        private readonly IResourceCalculation _resourceOptimizationHelper;
        private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
    	private readonly ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
	    private readonly Func<ISchedulerStateHolder> _schedulingResultStateHolder;
	    private readonly IUserTimeZone _userTimeZone;

	    public ScheduleDayService(IScheduleService scheduleService,
								  IDeleteSchedulePartService deleteSchedulePartService,
								  IResourceCalculation resourceOptimizationHelper,
								  IEffectiveRestrictionCreator effectiveRestrictionCreator,
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
			Func<ISchedulerStateHolder> schedulingResultStateHolder,
			IUserTimeZone userTimeZone
			)
		{
			_scheduleService = scheduleService;
			_deleteSchedulePartService = deleteSchedulePartService;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
			_schedulePartModifyAndRollbackService = schedulePartModifyAndRollbackService;
		    _schedulingResultStateHolder = schedulingResultStateHolder;
			_userTimeZone = userTimeZone;
		}

		public bool ScheduleDay(IScheduleDay schedulePart, SchedulingOptions schedulingOptions)
        {
			var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(schedulePart, schedulingOptions);

        	var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, schedulingOptions.ConsiderShortBreaks, _schedulingResultStateHolder().SchedulingResultState, _userTimeZone);
			return _scheduleService.SchedulePersonOnDay(schedulePart, schedulingOptions, effectiveRestriction, resourceCalculateDelayer, _schedulePartModifyAndRollbackService);
        }

		public IList<IScheduleDay> DeleteMainShift(IList<IScheduleDay> schedulePartList, SchedulingOptions schedulingOptions)
        {
            //TODO use a new Delete method with a rollbackservice
            var options = new DeleteOption {MainShift = true};

            var retList = _deleteSchedulePartService.Delete(schedulePartList, options, _schedulePartModifyAndRollbackService, new NoSchedulingProgress());
            
            ICollection<DateOnly> daysToRecalculate = new HashSet<DateOnly>();
            foreach (var part in schedulePartList)
            {
                var date = new DateOnly(part.Period.StartDateTimeLocal(_schedulingResultStateHolder().TimeZoneInfo));
                daysToRecalculate.Add(date);
                daysToRecalculate.Add(date.AddDays(1));
            }

			var resCalcData = _schedulingResultStateHolder().SchedulingResultState.ToResourceOptimizationData(schedulingOptions.ConsiderShortBreaks, false);
			foreach (var date in daysToRecalculate)
            {
				_resourceOptimizationHelper.ResourceCalculate(date, resCalcData);
            }

            return retList;
        }
    }
}
