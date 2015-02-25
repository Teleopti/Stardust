using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver
{
    public interface IDeleteScheduleDayFromUnsolvedPersonWeek
    {
        void DeleteAppropiateScheduleDay(IScheduleRange personScheduleRange, DateOnly dayOff, ISchedulePartModifyAndRollbackService rollbackService, DateOnlyPeriod selectedPeriod, IScheduleMatrixPro scheduleMatrix);
    }
    public class DeleteScheduleDayFromUnsolvedPersonWeek : IDeleteScheduleDayFromUnsolvedPersonWeek
    {
        private readonly IDeleteSchedulePartService _deleteSchedulePartService;
	    private readonly IScheduleDayIsLockedSpecification _scheduleDayIsLockedSpecification;

	    public DeleteScheduleDayFromUnsolvedPersonWeek(IDeleteSchedulePartService deleteSchedulePartService, 
			IScheduleDayIsLockedSpecification scheduleDayIsLockedSpecification)
        {
	        _deleteSchedulePartService = deleteSchedulePartService;
	        _scheduleDayIsLockedSpecification = scheduleDayIsLockedSpecification;
        }

	    public void DeleteAppropiateScheduleDay(IScheduleRange personScheduleRange, DateOnly dayOff, ISchedulePartModifyAndRollbackService rollbackService, DateOnlyPeriod selectedPeriod, IScheduleMatrixPro scheduleMatrix)
        {
            //lets pick the previous day as an experinment 

			var dayToDelete = dayOff.AddDays(-1);
			if (!selectedPeriod.Contains(dayToDelete)) return;
			var scheduleDayToDelete = personScheduleRange.ScheduledDay(dayToDelete);
			if(isDaysLocked(scheduleDayToDelete, scheduleMatrix))
				return;

            var deleteOption = new DeleteOption { Default = true };
            using (var bgWorker = new BackgroundWorker())
            {
                _deleteSchedulePartService.Delete(new List<IScheduleDay> { scheduleDayToDelete }, deleteOption, rollbackService, bgWorker);
            }
        }


		private bool isDaysLocked(IScheduleDay scheduleDay, IScheduleMatrixPro scheduleMatrix)
		{
			return _scheduleDayIsLockedSpecification.IsSatisfy(scheduleDay, scheduleMatrix);
		}
    }
}
