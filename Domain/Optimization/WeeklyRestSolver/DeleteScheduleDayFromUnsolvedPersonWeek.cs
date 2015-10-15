using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver
{
    public interface IDeleteScheduleDayFromUnsolvedPersonWeek
    {
        void DeleteAppropiateScheduleDay(IScheduleRange personScheduleRange, DateOnly dayOff, ISchedulePartModifyAndRollbackService rollbackService, DateOnlyPeriod selectedPeriod, IScheduleMatrixPro scheduleMatrix, IOptimizationPreferences optimizationPreferences);
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

	    public void DeleteAppropiateScheduleDay(IScheduleRange personScheduleRange, DateOnly dayOff, ISchedulePartModifyAndRollbackService rollbackService, DateOnlyPeriod selectedPeriod, IScheduleMatrixPro scheduleMatrix, IOptimizationPreferences optimizationPreferences)
        {
            //lets pick the previous day as an experinment 

			var dayToDelete = dayOff.AddDays(-1);
			if (!selectedPeriod.Contains(dayToDelete)) return;
			var scheduleDayToDelete = personScheduleRange.ScheduledDay(dayToDelete);
			if(isDaysLocked(scheduleDayToDelete, scheduleMatrix))
				return;

			if(isAnyOptimizeShiftPreferencesUsed(optimizationPreferences, scheduleDayToDelete))
				return;

            var deleteOption = new DeleteOption { Default = true };

			_deleteSchedulePartService.Delete(new List<IScheduleDay> { scheduleDayToDelete }, deleteOption, rollbackService, new NoBackgroundWorker());
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
			var timeZoneInfo = scheduleDay.TimeZone;

			var keepSelectedActivities = isKeepSelectedActivitiesAffecting(optimizationPreferences, projection);
			var alterBetween = isAlterBetweenAffecting(optimizationPreferences, projection, timeZoneInfo);
			var keepActivityLength = isKeepActivityLengthAffecting(optimizationPreferences, projection);

			return	(optimizationPreferences.Shifts.KeepStartTimes ||
					optimizationPreferences.Shifts.KeepEndTimes ||
					optimizationPreferences.Shifts.KeepShiftCategories ||
					keepSelectedActivities ||
					keepActivityLength ||
					alterBetween);
		}

	    private bool isAlterBetweenAffecting(IOptimizationPreferences optimizationPreferences, IVisualLayerCollection projection, TimeZoneInfo timeZone)
	    {
		    if (!optimizationPreferences.Shifts.AlterBetween)
			    return false;

		    var dateTimePeriod = projection.Period();
		    if (dateTimePeriod == null)
			    return false;

			var shiftStartDate = dateTimePeriod.Value.StartDateTime.Date;
		    var shiftStart = dateTimePeriod.Value.StartDateTime;
			var shiftEnd = dateTimePeriod.Value.EndDateTime;
			var dateOffset = (int)shiftEnd.Date.Subtract(shiftStartDate).TotalDays;
			var shiftStartUserLocalDateTime = TimeZoneHelper.ConvertFromUtc(shiftStart, timeZone);
		    var shiftEndUserLocalDateTime = TimeZoneHelper.ConvertFromUtc(shiftEnd, timeZone);
		    var shiftTimePeriod = new TimePeriod(shiftStartUserLocalDateTime.TimeOfDay, shiftEndUserLocalDateTime.TimeOfDay.Add((TimeSpan.FromDays(dateOffset))));

		    if (optimizationPreferences.Shifts.SelectedTimePeriod.Contains(shiftTimePeriod))
			    return false;

		    return true;
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
