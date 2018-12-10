using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver
{
    public interface IVerifyWeeklyRestAroundDayOffSpecification
    {
		bool IsSatisfy(IList<DateOnly> dayOffList, IScheduleRange currentSchedules);
    }

    public class VerifyWeeklyRestAroundDayOffSpecification : IVerifyWeeklyRestAroundDayOffSpecification
    {
        public bool IsSatisfy(IList<DateOnly> dayOffList, IScheduleRange currentSchedules)
        {
            foreach (var dayOffDate in dayOffList)
            {
                var previousScheduleDay = currentSchedules.ScheduledDay(dayOffDate.AddDays(-1)).SignificantPart();
                var nextScheduleDay = currentSchedules.ScheduledDay(dayOffDate.AddDays(1)).SignificantPart();
                if (isMissingShiftOnNeighbouringDays(previousScheduleDay, nextScheduleDay, currentSchedules, dayOffDate) ||
                    isNeighbouringDaysOff(previousScheduleDay, nextScheduleDay, currentSchedules, dayOffDate))
                    return false;
            }
            return true;
        }

        private bool isMissingShiftOnNeighbouringDays(SchedulePartView previousScheduleDay, SchedulePartView nextScheduleDay, IScheduleRange currentSchedules, DateOnly dayOffDate)
        {
	        var isMissingBefore = false;
	        var isMissingAfter = false;

	        if (previousScheduleDay == SchedulePartView.None)
	        {
		        isMissingBefore = currentSchedules.ScheduledDay(dayOffDate.AddDays(-2)).SignificantPart() == SchedulePartView.None;
	        }

	        if (nextScheduleDay == SchedulePartView.None)
	        {
		        isMissingAfter = currentSchedules.ScheduledDay(dayOffDate.AddDays(2)).SignificantPart() == SchedulePartView.None;
			}

	        return isMissingBefore || isMissingAfter;
        }

	    private bool isNeighbouringDaysOff(SchedulePartView previousScheduleDay, SchedulePartView nextScheduleDay, IScheduleRange currentSchedules, DateOnly dayOffDate)
	    {
		    var isDayOffBefore = false;
		    var isDayOffAfter = false;

		    if (previousScheduleDay == SchedulePartView.DayOff)
		    {
			    isDayOffBefore = currentSchedules.ScheduledDay(dayOffDate.AddDays(-2)).SignificantPart() == SchedulePartView.DayOff;
		    }

		    if (nextScheduleDay == SchedulePartView.DayOff)
		    {
			    isDayOffAfter = currentSchedules.ScheduledDay(dayOffDate.AddDays(2)).SignificantPart() == SchedulePartView.DayOff;
		    }

		    return isDayOffBefore || isDayOffAfter;
	    }
    }
}