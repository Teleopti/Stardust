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
		    IResourceCalculateDelayer resourceCalculateDelayer);
    }

    public  class TeamScheduling : ITeamScheduling
    {
		public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

	    public void ExecutePerDayPerPerson(IPerson person, DateOnly dateOnly, ITeamBlockInfo teamBlockInfo,
		    IShiftProjectionCache shiftProjectionCache,
		    ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
		    IResourceCalculateDelayer resourceCalculateDelayer)
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
		    OnDayScheduled(new SchedulingServiceSuccessfulEventArgs(scheduleDay));
		    resourceCalculateDelayer.CalculateIfNeeded(scheduleDay.DateOnlyAsPeriod.DateOnly,
			    shiftProjectionCache.WorkShiftProjectionPeriod);
	    }

	    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		protected virtual void OnDayScheduled(SchedulingServiceBaseEventArgs scheduleServiceBaseEventArgs)
		{
			EventHandler<SchedulingServiceBaseEventArgs> temp = DayScheduled;
			if (temp != null)
			{
				temp(this, scheduleServiceBaseEventArgs);
			}
		}

		private void assignShiftProjection(IShiftProjectionCache shiftProjectionCache, TimeZoneInfo agentTimeZone, IScheduleDay destinationScheduleDay, DateOnly day, ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService)
        {
			shiftProjectionCache.SetDate(day, agentTimeZone);
			destinationScheduleDay.AddMainShift(shiftProjectionCache.TheMainShift);

            schedulePartModifyAndRollbackService.Modify(destinationScheduleDay);
        }
    }
}