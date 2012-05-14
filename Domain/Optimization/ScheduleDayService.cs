using System.Collections.Generic;
using System.ComponentModel;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
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

    	private ScheduleDayService() { }

		public ScheduleDayService(IScheduleService scheduleService,
								  IDeleteSchedulePartService deleteSchedulePartService,
								  IResourceOptimizationHelper resourceOptimizationHelper,
								  IEffectiveRestrictionCreator effectiveRestrictionCreator,
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService
			)
			: this()
		{
			_scheduleService = scheduleService;
			_deleteSchedulePartService = deleteSchedulePartService;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
			_schedulePartModifyAndRollbackService = schedulePartModifyAndRollbackService;
		}

    	public bool RescheduleDay(IScheduleDay schedulePart, ISchedulingOptions schedulingOptions)
        {
            var originalDay = (IScheduleDay) schedulePart.Clone();
            var partList = new List<IScheduleDay> { schedulePart };

			IList<IScheduleDay> retList = DeleteMainShift(partList, schedulingOptions);

			bool result = ScheduleDay(retList[0], schedulingOptions);
            if(!result)
            {
				_schedulePartModifyAndRollbackService.Modify(originalDay);
				_resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(originalDay.Period.LocalStartDateTime), true, schedulingOptions.ConsiderShortBreaks);
            }

            return result;
        }

		public bool ScheduleDay(IScheduleDay schedulePart, ISchedulingOptions schedulingOptions)
        {
			var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(schedulePart, schedulingOptions);

        	var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, true,
																		schedulingOptions.ConsiderShortBreaks);
			return _scheduleService.SchedulePersonOnDay(schedulePart, schedulingOptions, true, effectiveRestriction, resourceCalculateDelayer);
        }

		public IList<IScheduleDay> DeleteMainShift(IList<IScheduleDay> schedulePartList, ISchedulingOptions schedulingOptions)
        {
            //Delete old current shift
            //TODO use a new Delete method with a rollbackservice
            var options = new DeleteOption {MainShift = true};

            IList<IScheduleDay> retList;
            using (var bgWorker = new BackgroundWorker())
            {
                retList = _deleteSchedulePartService.Delete(schedulePartList, options, _schedulePartModifyAndRollbackService, bgWorker);
            }

            //recalc resources
            ICollection<DateOnly> daysToRecalculate = new HashSet<DateOnly>();
            foreach (var part in schedulePartList)
            {
                var date = new DateOnly(part.Period.LocalStartDateTime);
                daysToRecalculate.Add(date);
                daysToRecalculate.Add(date.AddDays(1));
            }

            foreach (var date in daysToRecalculate)
            {
				_resourceOptimizationHelper.ResourceCalculateDate(date, true, schedulingOptions.ConsiderShortBreaks);
            }

            return retList;
        }
    }
}
