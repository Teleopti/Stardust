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
        void DeleteAppropiateScheduleDay(IScheduleRange personScheduleRange, DateOnly dayOff, ISchedulePartModifyAndRollbackService rollbackService, DateOnlyPeriod selectedPeriod);
    }
    public class DeleteScheduleDayFromUnsolvedPersonWeek : IDeleteScheduleDayFromUnsolvedPersonWeek
    {
        private readonly IDeleteSchedulePartService _deleteSchedulePartService;

        public DeleteScheduleDayFromUnsolvedPersonWeek(IDeleteSchedulePartService deleteSchedulePartService)
        {
            _deleteSchedulePartService = deleteSchedulePartService;
        }

        public void DeleteAppropiateScheduleDay(IScheduleRange personScheduleRange, DateOnly dayOff, ISchedulePartModifyAndRollbackService rollbackService, DateOnlyPeriod selectedPeriod)
        {
            //lets pick the previous day as an experinment 
	        if (!selectedPeriod.Contains(dayOff.AddDays(-1))) return;
			  var scheduleDayToDelete = personScheduleRange.ScheduledDay(dayOff.AddDays(-1));
            var deleteOption = new DeleteOption { Default = true };
            using (var bgWorker = new BackgroundWorker())
            {
                _deleteSchedulePartService.Delete(new List<IScheduleDay> { scheduleDayToDelete }, deleteOption, rollbackService, bgWorker);
            }
        }
    }
}
