using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class NightRestWhiteSpotSolverService : INightRestWhiteSpotSolverService
    {
        private readonly NightRestWhiteSpotSolver _solver;
    	private readonly DeleteAndResourceCalculateService _deleteAndResourceCalculateService;
        private readonly IScheduleService _scheduleService;
    	private readonly IResourceCalculateDelayer _resourceCalculateDelayer;

    	public NightRestWhiteSpotSolverService(NightRestWhiteSpotSolver solver, DeleteAndResourceCalculateService deleteAndResourceCalculateService, 
			IScheduleService scheduleService, IResourceCalculateDelayer resourceCalculateDelayer)
        {
            _solver = solver;
    		_deleteAndResourceCalculateService = deleteAndResourceCalculateService;
            _scheduleService = scheduleService;
        	_resourceCalculateDelayer = resourceCalculateDelayer;
        }
		
        public bool Resolve(IScheduleMatrixPro matrix, SchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService)
        {
	        var iterations = 0;
	        while (resolveExecute(matrix, schedulingOptions, schedulePartModifyAndRollbackService) && iterations < 10)
	        {
		        iterations++;
	        }
	        return matrix.IsFullyScheduled();
        }

	    private bool resolveExecute(IScheduleMatrixPro matrix, SchedulingOptions schedulingOptions,
		    ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService)
	    {
		    NightRestWhiteSpotSolverResult solverResult = _solver.Resolve(matrix, schedulingOptions);
		    // om inte solvern returnerar något så returnera false
		    if (!solverResult.DaysToDelete.Any())
			    return false;

		    var daysToDelete =
			    solverResult.DaysToDelete.Select(dateOnly => matrix.GetScheduleDayByKey(dateOnly).DaySchedulePart()).ToArray();
		    _deleteAndResourceCalculateService.DeleteWithResourceCalculation(daysToDelete,
			    schedulePartModifyAndRollbackService,
			    schedulingOptions.ConsiderShortBreaks, false);

		    bool success = false;
		    var daysInConsideration = solverResult.DaysToReschedule().ToHashSet();
		    foreach (var dateOnly in solverResult.DaysToReschedule())
		    {
			    if (!daysInConsideration.Contains(dateOnly)) continue;
			    daysInConsideration.Remove(dateOnly);
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