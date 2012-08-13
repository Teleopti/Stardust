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


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public bool ScheduleDay(IScheduleDay schedulePart, ISchedulingOptions schedulingOptions)
        {
			var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(schedulePart, schedulingOptions);

        	var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, true,
																		schedulingOptions.ConsiderShortBreaks);
			return _scheduleService.SchedulePersonOnDay(schedulePart, schedulingOptions, true, effectiveRestriction, resourceCalculateDelayer, null);
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
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
