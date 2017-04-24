using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class NightRestWhiteSpotSolverService : INightRestWhiteSpotSolverService
    {
        private readonly INightRestWhiteSpotSolver _solver;
    	private readonly IDeleteAndResourceCalculateService _deleteAndResourceCalculateService;
        private readonly IScheduleService _scheduleService;
        private readonly Func<IWorkShiftFinderResultHolder> _workShiftFinderResultHolder;
    	private readonly IResourceCalculateDelayer _resourceCalculateDelayer;

    	public NightRestWhiteSpotSolverService(INightRestWhiteSpotSolver solver, IDeleteAndResourceCalculateService deleteAndResourceCalculateService, 
			IScheduleService scheduleService, Func<IWorkShiftFinderResultHolder> workShiftFinderResultHolder,
			IResourceCalculateDelayer resourceCalculateDelayer)
        {
            _solver = solver;
    		_deleteAndResourceCalculateService = deleteAndResourceCalculateService;
            _scheduleService = scheduleService;
            _workShiftFinderResultHolder = workShiftFinderResultHolder;
        	_resourceCalculateDelayer = resourceCalculateDelayer;
        }
		
        public bool Resolve(IScheduleMatrixPro matrix, ISchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService)
        {
	        var iterations = 0;
	        while (resolveExecute(matrix, schedulingOptions, schedulePartModifyAndRollbackService) && iterations < 10)
	        {
		        iterations++;
	        }
	        return matrix.IsFullyScheduled();
        }

	    private bool resolveExecute(IScheduleMatrixPro matrix, ISchedulingOptions schedulingOptions,
		    ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService)
	    {
		    NightRestWhiteSpotSolverResult solverResult = _solver.Resolve(matrix);
		    // om inte solvern returnerar något så returnera false
		    if (!solverResult.DaysToDelete.Any())
			    return false;

		    var daysToDelete =
			    solverResult.DaysToDelete.Select(dateOnly => matrix.GetScheduleDayByKey(dateOnly).DaySchedulePart()).ToArray();
		    _deleteAndResourceCalculateService.DeleteWithResourceCalculation(daysToDelete,
			    schedulePartModifyAndRollbackService,
			    schedulingOptions.ConsiderShortBreaks, false);

		    bool success = false;
		    IPerson person = matrix.Person;

		    var daysInConsideration = solverResult.DaysToReschedule().ToList();
		    foreach (var dateOnly in solverResult.DaysToReschedule())
		    {
			    if (!daysInConsideration.Contains(dateOnly)) continue;
			    daysInConsideration.Remove(dateOnly);
			    _workShiftFinderResultHolder().Clear(person, dateOnly);
			    if (_scheduleService.SchedulePersonOnDay(matrix.GetScheduleDayByKey(dateOnly).DaySchedulePart(), schedulingOptions,
				    _resourceCalculateDelayer, schedulePartModifyAndRollbackService))
			    {
				    success = true;
			    }
			    else if (!solverResult.DaysToDelete.Contains(dateOnly))
			    {
				    success = false;
				    if (solverResult.DaysToReschedule().Contains(dateOnly.AddDays(-1)))
				    {
					    _scheduleService.SchedulePersonOnDay(matrix.GetScheduleDayByKey(dateOnly.AddDays(-1)).DaySchedulePart(),
						    schedulingOptions, _resourceCalculateDelayer,
						    schedulePartModifyAndRollbackService);
					    daysInConsideration.Remove(dateOnly.AddDays(-1));
				    }
				    if (daysInConsideration.Count == 0)
					    break;
			    }
		    }

		    return success;
	    }
    }
}