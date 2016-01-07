using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

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
                if (isMissingShiftOnNeighbouringDays(previousScheduleDay, nextScheduleDay) ||
                    isNeighbouringDaysOff(previousScheduleDay, nextScheduleDay))
                    return false;
            }
            return true;
        }

        private bool isMissingShiftOnNeighbouringDays(SchedulePartView previousScheduleDay, SchedulePartView nextScheduleDay)
        {
	        return previousScheduleDay == SchedulePartView.None || nextScheduleDay == SchedulePartView.None;
        }

	    private bool isNeighbouringDaysOff(SchedulePartView previousScheduleDay, SchedulePartView nextScheduleDay)
	    {
		    return previousScheduleDay == SchedulePartView.DayOff || nextScheduleDay == SchedulePartView.DayOff;
	    }
    }
}