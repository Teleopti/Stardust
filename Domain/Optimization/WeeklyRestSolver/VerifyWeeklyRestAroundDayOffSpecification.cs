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
                var previousScheduleDay = currentSchedules.ScheduledDay(dayOffDate.AddDays(-1));
                var nextScheduleDay = currentSchedules.ScheduledDay(dayOffDate.AddDays(1));
                if (isMissingShiftOnNeighbouringDays(previousScheduleDay, nextScheduleDay) ||
                    isNeighbouringDaysOff(previousScheduleDay, nextScheduleDay))
                    return false;
            }
            return true;
        }

        private bool isMissingShiftOnNeighbouringDays(IScheduleDay previousScheduleDay, IScheduleDay nextScheduleDay)
        {
            if (previousScheduleDay.SignificantPart() == SchedulePartView.None || nextScheduleDay.SignificantPart() == SchedulePartView.None)
                return true;
            return false;
        }

        private bool isNeighbouringDaysOff(IScheduleDay previousScheduleDay, IScheduleDay nextScheduleDay)
        {
            if (previousScheduleDay.SignificantPart() == SchedulePartView.DayOff || nextScheduleDay.SignificantPart() == SchedulePartView.DayOff)
                return true;
            return false;
        }
    }
}