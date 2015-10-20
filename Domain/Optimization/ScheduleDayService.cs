using System;
using System.Collections.Generic;
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
        private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
        private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
    	private readonly ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
	    private readonly Func<ISchedulerStateHolder> _schedulingResultStateHolder;

	    public ScheduleDayService(IScheduleService scheduleService,
								  IDeleteSchedulePartService deleteSchedulePartService,
								  IResourceOptimizationHelper resourceOptimizationHelper,
								  IEffectiveRestrictionCreator effectiveRestrictionCreator,
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
			Func<ISchedulerStateHolder> schedulingResultStateHolder
			)
		{
			_scheduleService = scheduleService;
			_deleteSchedulePartService = deleteSchedulePartService;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
			_schedulePartModifyAndRollbackService = schedulePartModifyAndRollbackService;
		    _schedulingResultStateHolder = schedulingResultStateHolder;
		}

		public bool ScheduleDay(IScheduleDay schedulePart, ISchedulingOptions schedulingOptions)
        {
			var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(schedulePart, schedulingOptions);

        	var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, schedulingOptions.ConsiderShortBreaks);
			return _scheduleService.SchedulePersonOnDay(schedulePart, schedulingOptions, effectiveRestriction, resourceCalculateDelayer, _schedulePartModifyAndRollbackService);
        }

		public IList<IScheduleDay> DeleteMainShift(IList<IScheduleDay> schedulePartList, ISchedulingOptions schedulingOptions)
        {
            //TODO use a new Delete method with a rollbackservice
            var options = new DeleteOption {MainShift = true};

            var retList = _deleteSchedulePartService.Delete(schedulePartList, options, _schedulePartModifyAndRollbackService, new NoBackgroundWorker());
            
            ICollection<DateOnly> daysToRecalculate = new HashSet<DateOnly>();
            foreach (var part in schedulePartList)
            {
                var date = new DateOnly(part.Period.StartDateTimeLocal(_schedulingResultStateHolder().TimeZoneInfo));
                daysToRecalculate.Add(date);
                daysToRecalculate.Add(date.AddDays(1));
            }

            foreach (var date in daysToRecalculate)
            {
				_resourceOptimizationHelper.ResourceCalculateDate(date, schedulingOptions.ConsiderShortBreaks);
            }

            return retList;
        }
    }
}
