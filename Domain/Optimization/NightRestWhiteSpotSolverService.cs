using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{

    public class NightRestWhiteSpotSolverService : INightRestWhiteSpotSolverService
    {
        private readonly INightRestWhiteSpotSolver _solver;
        private readonly IDeleteSchedulePartService _deleteSchedulePartService;
        private readonly ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
        private readonly IScheduleService _scheduleService;
        private readonly IWorkShiftFinderResultHolder _workShiftFinderResultHolder;

        public NightRestWhiteSpotSolverService(INightRestWhiteSpotSolver solver, IDeleteSchedulePartService deleteSchedulePartService, 
            ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService, IScheduleService scheduleService, IWorkShiftFinderResultHolder workShiftFinderResultHolder)
        {
            _solver = solver;
            _deleteSchedulePartService = deleteSchedulePartService;
            _schedulePartModifyAndRollbackService = schedulePartModifyAndRollbackService;
            _scheduleService = scheduleService;
            _workShiftFinderResultHolder = workShiftFinderResultHolder;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public bool Resolve(IScheduleMatrixPro matrix, ISchedulingOptions schedulingOptions)
        {
            NightRestWhiteSpotSolverResult solverResult = _solver.Resolve(matrix);
            // om inte solvern returnerar något så returnera false
            if (solverResult.DaysToDelete.Count == 0)
                return false;

            IList<IScheduleDay> daysToDelete = new List<IScheduleDay>();

            foreach (var dateOnly in solverResult.DaysToDelete)
            {
                daysToDelete.Add(matrix.GetScheduleDayByKey(dateOnly).DaySchedulePart());
            }
            _deleteSchedulePartService.Delete(daysToDelete, _schedulePartModifyAndRollbackService);

            bool success = false;
            IPerson person = matrix.Person;
            foreach (var dateOnly in solverResult.DaysToReschedule())
            {
                _workShiftFinderResultHolder.Clear(person, dateOnly);
                
                if(_scheduleService.SchedulePersonOnDay(matrix.GetScheduleDayByKey(dateOnly).DaySchedulePart(), schedulingOptions, true))
                {
                    //_schedulePartModifyAndRollbackService.Rollback();
                    success = true;
                }
            }

            return success;
        }
    }
}