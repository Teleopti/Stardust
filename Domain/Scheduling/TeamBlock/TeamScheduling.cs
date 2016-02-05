using System;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public interface ITeamScheduling
    {
		event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

	    void ExecutePerDayPerPerson(IPerson person, DateOnly dateOnly, ITeamBlockInfo teamBlockInfo,
		    IShiftProjectionCache shiftProjectionCache,
		    ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
		    IResourceCalculateDelayer resourceCalculateDelayer, bool doIntraIntervalCalculation);
    }

    public  class TeamScheduling : ITeamScheduling
    {
		public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

	    public void ExecutePerDayPerPerson(IPerson person, DateOnly dateOnly, ITeamBlockInfo teamBlockInfo,
		    IShiftProjectionCache shiftProjectionCache,
		    ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
		    IResourceCalculateDelayer resourceCalculateDelayer, bool doIntraIntervalCalculation)
	    {
		    var tempMatrixList =
			    teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(dateOnly)
				    .Where(scheduleMatrixPro => scheduleMatrixPro.Person == person)
				    .ToList();
		    if (!tempMatrixList.Any())
			    return;

		    var matrix = tempMatrixList.First();
		    var scheduleDayPro = matrix.GetScheduleDayByKey(dateOnly);
		    var scheduleDay = scheduleDayPro.DaySchedulePart();
		    if (!matrix.UnlockedDays.Contains(scheduleDayPro))
			    return;

		    if (scheduleDay.IsScheduled())
			    return;

		    var agentTimeZone = person.PermissionInformation.DefaultTimeZone();
		    assignShiftProjection(shiftProjectionCache, agentTimeZone, scheduleDay, dateOnly,
			    schedulePartModifyAndRollbackService);
		    onDayScheduled(new SchedulingServiceSuccessfulEventArgs(scheduleDay));
		    resourceCalculateDelayer.CalculateIfNeeded(scheduleDay.DateOnlyAsPeriod.DateOnly,
			    shiftProjectionCache.WorkShiftProjectionPeriod, doIntraIntervalCalculation);
	    }

	    private void onDayScheduled(SchedulingServiceBaseEventArgs args)
		{
			var handler = DayScheduled;
			if (handler != null)
			{
				handler(this, args);
			}
		}

		private void assignShiftProjection(IShiftProjectionCache shiftProjectionCache, TimeZoneInfo agentTimeZone, IScheduleDay destinationScheduleDay, DateOnly day, ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService)
        {
			shiftProjectionCache.SetDate(day, agentTimeZone);

			if (destinationScheduleDay.PersonAssignment() != null && destinationScheduleDay.PersonAssignment().PersonalActivities().Any())
			{
				var mainShiftPeriod = shiftProjectionCache.TheMainShift.ProjectionService().CreateProjection().Period().GetValueOrDefault();
				if (destinationScheduleDay.PersonAssignment().PersonalActivities().Any(personalShiftLayer => !mainShiftPeriod.Contains(personalShiftLayer.Period))) return;
			}

			if (destinationScheduleDay.PersonMeetingCollection().Any())
			{
				var mainShiftPeriod = shiftProjectionCache.TheMainShift.ProjectionService().CreateProjection().Period().GetValueOrDefault();
				if (destinationScheduleDay.PersonMeetingCollection().Any(personMeeting => !mainShiftPeriod.Contains(personMeeting.Period))) return;		
			}
					
			destinationScheduleDay.AddMainShift(shiftProjectionCache.TheMainShift);

            schedulePartModifyAndRollbackService.Modify(destinationScheduleDay);
        }
    }
}