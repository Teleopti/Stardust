using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public class TeamScheduling
    {
	    private readonly AssignScheduledLayers _assignScheduledLayers;
	    private readonly IDayOffsInPeriodCalculator _dayOffsInPeriodCalculator;

	    public TeamScheduling(AssignScheduledLayers assignScheduledLayers, 
			IDayOffsInPeriodCalculator dayOffsInPeriodCalculator)
	    {
		    _assignScheduledLayers = assignScheduledLayers;
		    _dayOffsInPeriodCalculator = dayOffsInPeriodCalculator;
	    }

	    public bool ExecutePerDayPerPerson(IEnumerable<IPersonAssignment> orginalPersonAssignments, IPerson person, DateOnly dateOnly, ITeamBlockInfo teamBlockInfo,
		    ShiftProjectionCache shiftProjectionCache,
		    ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
		    IResourceCalculateDelayer resourceCalculateDelayer, bool doIntraIntervalCalculation, INewBusinessRuleCollection businessRules, SchedulingOptions schedulingOptions,
			IScheduleDictionary schedules, Func<SchedulingServiceBaseEventArgs, bool> dayScheduled)
	    {
		    var tempMatrixList =
			    teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(dateOnly)
				    .Where(scheduleMatrixPro => scheduleMatrixPro.Person == person)
				    .ToList();
		    if (!tempMatrixList.Any())
			    return false;

		    var matrix = tempMatrixList.First();

			if (!_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(schedules, matrix.SchedulePeriod, out int _, out IList<IScheduleDay> _))
			{
				return false;
			}

			var scheduleDayPro = matrix.GetScheduleDayByKey(dateOnly);
		    var scheduleDay = scheduleDayPro.DaySchedulePart();
		    if (!matrix.UnlockedDays.Contains(scheduleDayPro))
			    return false;

		    if (schedulingOptions.IsDayScheduled(scheduleDay))
			    return false;
			

		    assignShiftProjection(orginalPersonAssignments, shiftProjectionCache, scheduleDay,
			    schedulePartModifyAndRollbackService, businessRules, schedulingOptions, resourceCalculateDelayer, doIntraIntervalCalculation);

			return dayScheduled != null && dayScheduled(new SchedulingServiceSuccessfulEventArgs(scheduleDay));
		}

		private void assignShiftProjection(IEnumerable<IPersonAssignment> orginalPersonAssignments, ShiftProjectionCache shiftProjectionCache, IScheduleDay destinationScheduleDay, 
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService, INewBusinessRuleCollection businessRules, 
			SchedulingOptions schedulingOptions, IResourceCalculateDelayer resourceCalculateDelayer, bool doIntraIntervalCalculation)
        {
			shiftProjectionCache.SetDate(destinationScheduleDay.DateOnlyAsPeriod);

	        var personAssignment = destinationScheduleDay.PersonAssignment();
	        if (personAssignment != null && personAssignment.PersonalActivities().Any())
			{
				var mainShiftPeriod = shiftProjectionCache.MainShiftProjection.Period().GetValueOrDefault();
				if (personAssignment.PersonalActivities().Any(personalShiftLayer => !mainShiftPeriod.Contains(personalShiftLayer.Period))) return;
			}

	        var personMeetingCollection = destinationScheduleDay.PersonMeetingCollection();
	        if (personMeetingCollection.Any())
			{
				var mainShiftPeriod = shiftProjectionCache.MainShiftProjection.Period().GetValueOrDefault();
				if (personMeetingCollection.Any(personMeeting => !mainShiftPeriod.Contains(personMeeting.Period))) return;
			}

			//Fix key if perf too slow to find ass
	        var assignmentBefore = orginalPersonAssignments.FirstOrDefault(x => x.Date == destinationScheduleDay.DateOnlyAsPeriod.DateOnly && x.Person.Equals(destinationScheduleDay.Person));
			_assignScheduledLayers.Execute(schedulingOptions, destinationScheduleDay, shiftProjectionCache.TheMainShift);
	        if (assignmentChanged(assignmentBefore, destinationScheduleDay))
	        {
				schedulePartModifyAndRollbackService.Modify(destinationScheduleDay, businessRules);

		        resourceCalculateDelayer.CalculateIfNeeded(destinationScheduleDay.DateOnlyAsPeriod.DateOnly,
			        shiftProjectionCache.WorkShiftProjectionPeriod, doIntraIntervalCalculation);
			}
		}

	    private static bool assignmentChanged(IPersonAssignment assignmentBefore, IScheduleDay destinationScheduleDay)
	    {
		    if (assignmentBefore == null)
			    return true;
		    var currentAssignment = destinationScheduleDay.PersonAssignment();
		    return !assignmentBefore.MainActivities().SequenceEqual(currentAssignment.MainActivities(), new sameMainShifts());
	    }

		private class sameMainShifts : IEqualityComparer<MainShiftLayer>
		{
			public bool Equals(MainShiftLayer x, MainShiftLayer y)
			{
				return x.Period == y.Period && x.Payload.Equals(y.Payload);
			}

			public int GetHashCode(MainShiftLayer obj)
			{
				return obj.Payload.GetHashCode() ^ obj.Period.GetHashCode();
			}
		}
	}
}