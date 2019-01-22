using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver
{
    public class DeleteScheduleDayFromUnsolvedPersonWeek
    {
        private readonly IDeleteSchedulePartService _deleteSchedulePartService;
	    private readonly IScheduleDayIsLockedSpecification _scheduleDayIsLockedSpecification;
	    private readonly CorrectAlteredBetween _correctAlteredBetween;
	    private readonly OptimizerActivitiesPreferencesFactory _optimizerActivitiesPreferencesFactory;

	    public DeleteScheduleDayFromUnsolvedPersonWeek(IDeleteSchedulePartService deleteSchedulePartService, 
			IScheduleDayIsLockedSpecification scheduleDayIsLockedSpecification,
			CorrectAlteredBetween correctAlteredBetween,
			OptimizerActivitiesPreferencesFactory optimizerActivitiesPreferencesFactory)
        {
	        _deleteSchedulePartService = deleteSchedulePartService;
	        _scheduleDayIsLockedSpecification = scheduleDayIsLockedSpecification;
	        _correctAlteredBetween = correctAlteredBetween;
	        _optimizerActivitiesPreferencesFactory = optimizerActivitiesPreferencesFactory;
        }

	    public void DeleteAppropiateScheduleDay(IScheduleRange personScheduleRange, DateOnly dayOff,
		    ISchedulePartModifyAndRollbackService rollbackService, DateOnlyPeriod selectedPeriod,
		    IScheduleMatrixPro scheduleMatrix, IOptimizationPreferences optimizationPreferences)
	    {
		    var dayToDelete = dayOff.AddDays(-1);
		    var firstTrySuccess = tryDeleteDay(dayToDelete, personScheduleRange, selectedPeriod, rollbackService, scheduleMatrix,
			    optimizationPreferences);
			if (!firstTrySuccess)
				tryDeleteDay(dayToDelete.AddDays(2), personScheduleRange, selectedPeriod, rollbackService, scheduleMatrix,
				optimizationPreferences);

		}

	    private bool tryDeleteDay(DateOnly dayToDelete, IScheduleRange personScheduleRange, DateOnlyPeriod selectedPeriod, ISchedulePartModifyAndRollbackService rollbackService, IScheduleMatrixPro scheduleMatrix, IOptimizationPreferences optimizationPreferences)
	    {
			if (!selectedPeriod.Contains(dayToDelete))
				return false;

			var scheduleDayToDelete = personScheduleRange.ScheduledDay(dayToDelete);
			if (isDaysLocked(scheduleDayToDelete, scheduleMatrix))
				return false;

			if (isAnyOptimizeShiftPreferencesUsed(optimizationPreferences, scheduleDayToDelete))
				return false;

			var deleteOption = new DeleteOption { Default = true };
			_deleteSchedulePartService.Delete(new List<IScheduleDay> { scheduleDayToDelete }, deleteOption, rollbackService, new NoSchedulingProgress());
		    return true;
	    }

		private bool isDaysLocked(IScheduleDay scheduleDay, IScheduleMatrixPro scheduleMatrix)
		{
			return _scheduleDayIsLockedSpecification.IsSatisfy(scheduleDay, scheduleMatrix);
		}

		private bool isAnyOptimizeShiftPreferencesUsed(IOptimizationPreferences optimizationPreferences, IScheduleDay scheduleDay)
		{
			if (optimizationPreferences == null)
				return false;

			var projection = scheduleDay.ProjectionService().CreateProjection();

			return optimizationPreferences.Shifts.KeepStartTimes ||
						 optimizationPreferences.Shifts.KeepEndTimes ||
						 optimizationPreferences.Shifts.KeepShiftCategories ||
						 isKeepSelectedActivitiesAffecting(optimizationPreferences, projection) ||
						 isKeepActivityLengthAffecting(optimizationPreferences, projection) ||
						 !_correctAlteredBetween.Execute(scheduleDay.DateOnlyAsPeriod.DateOnly,
										 projection,
										 new VisualLayerCollection(Enumerable.Empty<IVisualLayer>(), new NoProjectionMerger()),
										 _optimizerActivitiesPreferencesFactory.Create(optimizationPreferences));
		}

	    private bool isKeepSelectedActivitiesAffecting(IOptimizationPreferences optimizationPreferences, IVisualLayerCollection projection)
	    {
		    if (!optimizationPreferences.Shifts.SelectedActivities.Any())
			    return false;
	
			foreach (var selectedActivity in optimizationPreferences.Shifts.SelectedActivities)
			{
				if (projection.Any(visualLayer => selectedActivity.Equals(visualLayer.Payload)))
					return true;
	
			}

		    return false;
	    }

	    private bool isKeepActivityLengthAffecting(IOptimizationPreferences optimizationPreferences, IEnumerable<IVisualLayer> projection)
	    {
		    if (!optimizationPreferences.Shifts.KeepActivityLength)
			    return false;

		    foreach (var visualLayer in projection)
		    {
			    if (visualLayer.Payload.Equals(optimizationPreferences.Shifts.ActivityToKeepLengthOn))
				    return true;

		    }

		    return false;
	    }
    }
}
