using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver
{
	public interface IVerifyWeeklyRestNotLockedAroundDayOffSpecification
    {
		bool IsSatisfy(IList<DateOnly> dayOffList, IScheduleRange currentSchedules, IScheduleMatrixPro scheduleMatrix);
    }

	public class VerifyWeeklyRestNotLockedAroundDayOffSpecification : IVerifyWeeklyRestNotLockedAroundDayOffSpecification
    {
        public bool IsSatisfy(IList<DateOnly> dayOffList, IScheduleRange currentSchedules, IScheduleMatrixPro scheduleMatrix)
        {
            foreach (var dayOffDate in dayOffList)
            {
                var previousScheduleDay = currentSchedules.ScheduledDay(dayOffDate.AddDays(-1));
                var nextScheduleDay = currentSchedules.ScheduledDay(dayOffDate.AddDays(1));
                if (isNeighbouringDaysLocked(previousScheduleDay, nextScheduleDay, scheduleMatrix))
                    return false;
            }
            return true;
        }

		private bool isNeighbouringDaysLocked(IScheduleDay previousScheduleDay, IScheduleDay nextScheduleDay, IScheduleMatrixPro scheduleMatrixPro)
		{
			IList<DateTimePeriod> unlockeDays = scheduleMatrixPro.UnlockedDays.Select(day => day.DaySchedulePart().Period).ToList();
			if (!unlockeDays.Contains(previousScheduleDay.Period) || !unlockeDays.Contains(nextScheduleDay.Period))
				return true;
			return false;
		}
    }
}