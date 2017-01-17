using System;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public interface ITeamScheduling
    {
	    bool ExecutePerDayPerPerson(IPerson person, DateOnly dateOnly, ITeamBlockInfo teamBlockInfo,
		    IShiftProjectionCache shiftProjectionCache,
		    ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
		    IResourceCalculateDelayer resourceCalculateDelayer, bool doIntraIntervalCalculation, INewBusinessRuleCollection businessRules, ISchedulingOptions schedulingOptions, 
				Func<SchedulingServiceBaseEventArgs, bool> dayScheduled);
    }

    public class TeamScheduling : ITeamScheduling
    {
	    private readonly AssignScheduledLayers _assignScheduledLayers;

	    public TeamScheduling(AssignScheduledLayers assignScheduledLayers)
	    {
		    _assignScheduledLayers = assignScheduledLayers;
	    }

	    public bool ExecutePerDayPerPerson(IPerson person, DateOnly dateOnly, ITeamBlockInfo teamBlockInfo,
		    IShiftProjectionCache shiftProjectionCache,
		    ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
		    IResourceCalculateDelayer resourceCalculateDelayer, bool doIntraIntervalCalculation, INewBusinessRuleCollection businessRules, ISchedulingOptions schedulingOptions,
				Func<SchedulingServiceBaseEventArgs, bool> dayScheduled)
	    {
		    var tempMatrixList =
			    teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(dateOnly)
				    .Where(scheduleMatrixPro => scheduleMatrixPro.Person == person)
				    .ToList();
		    if (!tempMatrixList.Any())
			    return false;

		    var matrix = tempMatrixList.First();
		    var scheduleDayPro = matrix.GetScheduleDayByKey(dateOnly);
		    var scheduleDay = scheduleDayPro.DaySchedulePart();
		    if (!matrix.UnlockedDays.Contains(scheduleDayPro))
			    return false;

		    if (schedulingOptions.IsDayScheduled(scheduleDay))
			    return false;
			
		    assignShiftProjection(shiftProjectionCache, scheduleDay,
			    schedulePartModifyAndRollbackService, businessRules, schedulingOptions);

			resourceCalculateDelayer.CalculateIfNeeded(scheduleDay.DateOnlyAsPeriod.DateOnly,
			    shiftProjectionCache.WorkShiftProjectionPeriod, doIntraIntervalCalculation);
			return dayScheduled != null && dayScheduled(new SchedulingServiceSuccessfulEventArgs(scheduleDay));
		}

		private void assignShiftProjection(IShiftProjectionCache shiftProjectionCache, IScheduleDay destinationScheduleDay, ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService, INewBusinessRuleCollection businessRules, ISchedulingOptions schedulingOptions)
        {
			shiftProjectionCache.SetDate(destinationScheduleDay.DateOnlyAsPeriod);

	        var personAssignment = destinationScheduleDay.PersonAssignment();
	        if (personAssignment != null && personAssignment.PersonalActivities().Any())
			{
				var mainShiftPeriod = shiftProjectionCache.TheMainShift.ProjectionService().CreateProjection().Period().GetValueOrDefault();
				if (personAssignment.PersonalActivities().Any(personalShiftLayer => !mainShiftPeriod.Contains(personalShiftLayer.Period))) return;
			}

	        var personMeetingCollection = destinationScheduleDay.PersonMeetingCollection();
	        if (personMeetingCollection.Any())
			{
				var mainShiftPeriod = shiftProjectionCache.TheMainShift.ProjectionService().CreateProjection().Period().GetValueOrDefault();
				if (personMeetingCollection.Any(personMeeting => !mainShiftPeriod.Contains(personMeeting.Period))) return;		
			}
			_assignScheduledLayers.Execute(schedulingOptions, destinationScheduleDay, shiftProjectionCache.TheMainShift);
					
            schedulePartModifyAndRollbackService.Modify(destinationScheduleDay, businessRules);
        }
    }
}